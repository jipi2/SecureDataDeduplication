using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace FileStorageApp.Server.Services
{
    public class FileService
    {
        public FileRepository _fileRepo { get; set; }
        public UserService _userService { get; set; }
        public AzureBlobService _azureBlobService { get; set; } 
        public FileService(FileRepository fileRepository, UserService userService, AzureBlobService azureBlobService)
        {
            _fileRepo = fileRepository;
            _userService = userService;
            _azureBlobService = azureBlobService;
        }

        public async Task<Dictionary<string, string>> GetDFParameters(string Userid)
        {
            User user = await _userService.GetUserById(Userid);

            DHParameters parameters = Utils.GenerateParameters();
            AsymmetricCipherKeyPair serverKeys = Utils.GenerateDFKeys(parameters);

            user.ServerDHPrivate = Utils.GetPemAsString(serverKeys.Private);
            user.ServerDHPublic = Utils.GetPemAsString(serverKeys.Public);

            await _userService.SaveServerDFKeysForUser(user, serverKeys, Utils.GetP(parameters), Utils.GetG(parameters));

            Dictionary<string, string> stringParams = new Dictionary<string, string>();
            stringParams.Add("G", Utils.GetG(parameters));
            stringParams.Add("P", Utils.GetP(parameters));
            stringParams.Add("ServerPubKey", Utils.GetPublicKey(serverKeys));

            return stringParams;
        }

        public async Task<bool> DFkeyExchange(string pubKey, string userId)
        {
            try
            {
                string stringPrivKey = await _userService.GetPrivateKeyOfServerForUser(userId);
              
                AsymmetricKeyParameter privKey = (AsymmetricKeyParameter)Utils.ReadPrivateKeyFromPemString(stringPrivKey);

                Dictionary<string, string> stringParams = await _userService.GetParametersForDF(userId);
                DHParameters parameters = Utils.GenerateParameters(stringParams["G"], stringParams["P"]);

                //calculam secretul in continuare
                Org.BouncyCastle.Math.BigInteger serverSecret = Utils.ComputeSharedSecret(pubKey, privKey, parameters);
                byte[] byteSecret = serverSecret.ToByteArray();
                byte[] symKey = Utils.ComputeHash(byteSecret);
                await _userService.SaveSymKeyForUser(userId, symKey);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<bool> ComputeFileMetadata(Dictionary<string, string> fileParams, string base64EncFile, string userId)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                string base64SymKey;
                if (user.SymKey != null)
                    base64SymKey = user.SymKey;
                else
                    return false;

                string base64Tag = Utils.DecryptAes(Convert.FromBase64String(fileParams["base64TagEnc"]), Convert.FromBase64String(base64SymKey));
                string base64Key = Utils.DecryptAes(Convert.FromBase64String(fileParams["base64KeyEnc"]), Convert.FromBase64String(base64SymKey));
                string base64Iv = Utils.DecryptAes(Convert.FromBase64String(fileParams["base64IvEnc"]), Convert.FromBase64String(base64SymKey));
                string fileName = Encoding.UTF8.GetString( Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fileParams["encFileName"]), Convert.FromBase64String(base64SymKey))));


              /*  string HexTag = Utils.ByteToHex(Convert.FromBase64String(base64Tag));*/
                if (await _fileRepo.GetFileMetaByTag(base64Tag) == false)
                {
                    //upload enc file in azure blob storage and get the blob url for it
                    string? blobUrl = await _azureBlobService.UploadFileToCloud(base64EncFile, base64Tag);

                    if (blobUrl == null)
                    {
                        throw new Exception("Url for Azure Blob Stroage is null");
                    }

                    FileMetadata fileMeta = new FileMetadata
                    {
                        FileName = fileName,
                        BlobLink = blobUrl,
                        isDeleted = false,
                        Key = base64Key,
                        Iv = base64Iv,
                        Tag = base64Tag,
                        UploadDate = DateTime.UtcNow
                    };

                    await _fileRepo.SaveFile(fileMeta);
                    await _userService.AddFile(userId, fileMeta);
                }
                else
                    Console.WriteLine("This file exists");

                return true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<FilesNameDate>?> GetFileNamesAndDatesOfUser(string id)
        {
            User? user = await _userService.GetUserById(id);
            if (user == null)
                return null;
            List<FilesNameDate>? result = user.Files?.Select(fileMeta => new FilesNameDate
            {
                FileName = fileMeta.FileName,
                UploadDate = fileMeta.UploadDate,

            }).ToList();

            if (result == null)
            {
                result = new List<FilesNameDate>();
                result.Add(new FilesNameDate
                {
                    FileName = "No Files.",
                    UploadDate = DateTime.UtcNow
                });
            }

            return result;
        }

        public async Task<ServerBlobFIle>? GetFileFromBlob(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            FileMetadata file = user.Files.Where(f => f.FileName == fileName).FirstOrDefault();

            string base64file = await _azureBlobService.GetContentFileFromBlob(file.Tag);

            ServerBlobFIle serverFile = new ServerBlobFIle
            {
                FileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), Convert.FromBase64String(user.SymKey)),
                FileKey = Utils.EncryptAes(Convert.FromBase64String(file.Key), Convert.FromBase64String(user.SymKey)),
                FileIv = Utils.EncryptAes(Convert.FromBase64String(file.Iv), Convert.FromBase64String(user.SymKey)),
                EncBase64File = base64file
            };

            return serverFile;
        }
    }
}
