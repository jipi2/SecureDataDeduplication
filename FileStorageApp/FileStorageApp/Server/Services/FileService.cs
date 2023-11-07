using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Numerics;


namespace FileStorageApp.Server.Services
{
    public class FileService
    {
        public FileRepository _fileRepo { get; set; }
        public UserService _userService { get; set; }
        public FileService(FileRepository fileRepository, UserService userService)
        {
            _fileRepo = fileRepository;
            _userService = userService;
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

                string HexTag = Utils.ByteToHex(Convert.FromBase64String(base64Tag));
                if (await _fileRepo.GetFileMetaByTag(HexTag) == false)
                {
                    FileMetadata fileMeta = new FileMetadata
                    {
                        BlobLink = "Link for Bob",
                        isDeleted = false,
                        Key = base64Key,
                        Iv = base64Iv,
                        Tag = HexTag,
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
    }
}
