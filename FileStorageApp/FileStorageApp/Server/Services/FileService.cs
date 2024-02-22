using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace FileStorageApp.Server.Services
{
    public class FileService
    {
        public FileRepository _fileRepo { get; set; }
        public RespRepository _respRepo { get; set; }
        public UserService _userService { get; set; }
        public UserRepository _userRepo { get; set; }
        public AzureBlobService _azureBlobService { get; set; } 
        IConfiguration _configuration { get; set; }
        public FileService(FileRepository fileRepository, UserService userService, AzureBlobService azureBlobService, IConfiguration configuration, RespRepository respRepo, UserRepository userRepo)
        {
            _fileRepo = fileRepository;
            _userService = userService;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _respRepo = respRepo;
            _userRepo = userRepo;
        }

        public async Task<DFparametersDto> GetDFParameters(string Userid)
        {
            try
            {
                User user = await _userService.GetUserById(Userid);

                DHParameters parameters = Utils.GenerateParameters();
                AsymmetricCipherKeyPair serverKeys = Utils.GenerateDFKeys(parameters);

                user.ServerDHPrivate = Utils.GetPemAsString(serverKeys.Private);
                user.ServerDHPublic = Utils.GetPemAsString(serverKeys.Public);

                await _userService.SaveServerDFKeysForUser(user, serverKeys, Utils.GetP(parameters), Utils.GetG(parameters));

                DFparametersDto dFparams = new DFparametersDto
                {
                    G = Utils.GetG(parameters),
                    P = Utils.GetP(parameters),
                    ServerPubKey = Utils.GetPublicKey(serverKeys)
                };

                return dFparams;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
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

        public async Task<string> GetFileFromBlobWithoutEncryption(string base64tag)
        {
            string base64file = await _azureBlobService.GetContentFileFromBlob(base64tag);
            return base64file;
        }

        private async Task GenerateMerkleTreeChallenges(int fileMetadataId, MerkleTree mt)
        {
            int J = Convert.ToInt32(_configuration["J"]);
            List<Resp> respList = new List<Resp>(J);
            int pos1=0, pos2=0, pos3=0;
            byte[] answ = new byte[mt.HashTree[0]._hash.Length];

            for (int i = 0; i < J; i++)
            {
                answ = Utils.GenerateResp(mt, ref pos1, ref pos2, ref pos3);
                respList.Add(new Resp
                {
                    Position_1 = pos1,
                    Position_2 = pos2,
                    Position_3 = pos3,
                    Answer = Convert.ToBase64String(answ),
                    wasUsed = false,
                    FileMetadataId = fileMetadataId
                });
            }
            await _respRepo.SaveMultipleResps(respList);
            
        }

        private async Task<MerkleTree> GetMerkleTree(string base64EncFile)
        {
            byte[] bytesEncFile = Convert.FromBase64String(base64EncFile);
            Stream stream = new MemoryStream(bytesEncFile);
            MerkleTree mt = Utils.GetMerkleTree(stream);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }

        private async Task SaveBlob(string base64EncFile, string base64Tag, string fileName, string base64Key, string base64Iv, string userId)
        {
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
/*            await _userService.AddFile(userId, fileMeta);*/
            MerkleTree mt = await GetMerkleTree(base64EncFile);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
        }

        private async Task<bool> CheckIfFileNameExists(User user, string fileName)
        {
            if (user.Files == null)
                return false;
            foreach (FileMetadata file in user.Files)
            {
                if (file.FileName.Equals(fileName))
                    return true;
            }
            return false;
        }   
        public async Task<FileMetaChallenge?> ComputeFileMetadata(FileParamsDto fileParams, string userId)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                string base64SymKey;
                if (user.SymKey != null)
                    base64SymKey = user.SymKey;
                else
                    return null;

                string base64Tag = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64TagEnc), Convert.FromBase64String(base64SymKey));
                string base64Key = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64KeyEnc), Convert.FromBase64String(base64SymKey));
                string base64Iv = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64IvEnc), Convert.FromBase64String(base64SymKey));
                string fileName = Encoding.UTF8.GetString( Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fileParams.encFileName), Convert.FromBase64String(base64SymKey))));

                FileMetaChallenge fmc = new FileMetaChallenge();
                fmc.id = "";

                if(await CheckIfFileNameExists(user, fileName) == true)
                {
                    fmc.id = "File name already exists!";
                    return fmc;
                }

                if (await _fileRepo.GetFileMetaByTag(base64Tag) == false)
                {
                    //upload enc file in azure blob storage and get the blob url for it
                    /*string? blobUrl = await _azureBlobService.UploadFileToCloud(base64EncFile, base64Tag);

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
                    MerkleTree mt = await GetMerkleTree(base64EncFile);
                    await GenerateMerkleTreeChallenges(fileMeta.Id, mt);*/

                    await SaveBlob(fileParams.base64EncFile, base64Tag, fileName, base64Key, base64Iv, userId);
                   
                }      
                
                FileMetadata? fileMeta = await _fileRepo.GetFileMetaWithResp(base64Tag);
                if (fileMeta == null)
                    return null;

                Resp? resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                if(resp == null)
                {
                    //reload the challenges
                    ServerBlobFIle blobFile = await GetFileFromBlob(userId, fileName);
                    MerkleTree mt = await GetMerkleTree(blobFile.EncBase64File);
                    await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
                    resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                }
                fmc.id = Utils.EncryptAes(BitConverter.GetBytes(resp.Id), Convert.FromBase64String(base64SymKey));
                fmc.n1 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_1), Convert.FromBase64String(base64SymKey));
                fmc.n2 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_2), Convert.FromBase64String(base64SymKey));
                fmc.n3 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_3), Convert.FromBase64String(base64SymKey));
                resp.wasUsed = true;
                await _respRepo.UpdateResp(resp);
                return fmc;            
               
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> CheckEncTag(string userId, string encTag)
        {
            User user = await _userService.GetUserById(userId);
            string base64SymKey;
            if (user.SymKey != null)
                base64SymKey = user.SymKey;
            else
                throw new Exception("Problems with Crypto params!");
            string base64Tag = Utils.DecryptAes(Convert.FromBase64String(encTag), Convert.FromBase64String(base64SymKey));
            if(await _fileRepo.GetFileMetaByTag(base64Tag) == false)
            {
                return false;
            }
            return true;

        }
        public async Task<bool> SaveFileToUser(string userId, FileResp userResp)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                string base64SymKey;
                if (user.SymKey != null)
                    base64SymKey = user.SymKey;
                else
                    return false;

                byte[] symKey = Convert.FromBase64String(base64SymKey);

                int respId = BitConverter.ToInt32(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(userResp.Id), symKey)));
                Resp? resp = await _respRepo.GetRespById(Convert.ToInt32(respId));

                string userFilename = Encoding.UTF8.GetString(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(userResp.FileName), symKey)));
                string userAnsw = Utils.DecryptAes(Convert.FromBase64String(userResp.Answer), symKey);

                if (resp == null) return false;
                if (resp.Answer.Equals(userAnsw))
                {
                    FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(resp.FileMetadataId);
                    if(fileMeta == null)
                        return false;
                    
                    if (fileMeta.FileName.Equals(userFilename) == false)
                    {
                        FileMetadata userFile = new FileMetadata
                        {
                            FileName = userFilename,
                            BlobLink = fileMeta.BlobLink,
                            isDeleted = false,
                            Key = fileMeta.Key,
                            Iv = fileMeta.Iv,
                            Tag = fileMeta.Tag,
                            UploadDate = DateTime.UtcNow
                        };
                        await _fileRepo.SaveFile(userFile);
                        await _userService.AddFile(userId, userFile);
                    }
                    else
                        await _userService.AddFile(userId, fileMeta);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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

        //proxy logic
        public async Task<bool> CheckTagAvailabilityInCloud(string base64tag)
        {
            return await _fileRepo.GetFileMetaByTag(base64tag);
        }

        public async Task<FileMetaChallenge?> GetChallengeForTag(string base64Tag)
        {
            try
            {
                string base64encFile = await GetFileFromBlobWithoutEncryption(base64Tag);
                FileMetadata fileMeta = await _fileRepo.GetFileMetaWithResp(base64Tag);

                Resp? resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                if (resp == null)
                {
                    //reload the challenges
                    MerkleTree mt = await GetMerkleTree(base64encFile);
                    await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
                    resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                }
                FileMetaChallenge fmc = new FileMetaChallenge();
                fmc.id = resp.Id.ToString();
                fmc.n1 = resp.Position_1.ToString();
                fmc.n2 = resp.Position_2.ToString();
                fmc.n3 = resp.Position_3.ToString();
                resp.wasUsed = true;
                await _respRepo.UpdateResp(resp);
                return fmc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> VerifyChallengeResponseFromProxy(FileResp proxyResp)
        {
            try
            {
                int respId = Int32.Parse(proxyResp.Id);
                Resp? resp = await _respRepo.GetRespById(Convert.ToInt32(respId));

                if (resp == null) return false;
                if (resp.Answer.Equals(resp.Answer))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<FileDecDataDto> GetDecryptedFileParams(FileEncDataDto fileEncData)
        {
            FileDecDataDto fileDecData = new FileDecDataDto();

            User? user = await _userRepo.GetUserByEmail(fileEncData.userEmail);
            if (user == null)
                throw new Exception("User does not exist!");

            fileDecData.tag = Utils.DecryptAes(Convert.FromBase64String(fileEncData.encBase64Tag), Convert.FromBase64String(user.SymKey));
            fileDecData.base64key = Utils.DecryptAes(Convert.FromBase64String(fileEncData.base64KeyEnc), Convert.FromBase64String(user.SymKey));
            fileDecData.base64iv = Utils.DecryptAes(Convert.FromBase64String(fileEncData.base64IvEnc), Convert.FromBase64String(user.SymKey));
            fileDecData.fileName = Encoding.UTF8.GetString(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(fileEncData.encFileName), Convert.FromBase64String(user.SymKey))));
            
   
            if (await CheckIfFileNameExists(user, fileDecData.fileName) == true)
                throw new Exception("User has allready a file with this name!");

            return fileDecData;
        }


        public async Task SaveDedupFile(FileDedupDto fileDedupDto)
        {
            User? user = await _userRepo.GetUserByEmail(fileDedupDto.userEmail);
            if(user == null)
                throw new Exception("User does not exist!");

            FileMetadata? fileMeta = await _fileRepo.GetFileMetaByTagIfExists(fileDedupDto.base64tag);
            if (fileMeta == null)
                throw new Exception("The file with this tag does not exists!");

            if (fileMeta.FileName.Equals(fileDedupDto.fileName) == false)
            {
                FileMetadata userFile = new FileMetadata
                {
                    FileName = fileDedupDto.fileName,
                    BlobLink = fileMeta.BlobLink,
                    isDeleted = false,
                    Key = fileMeta.Key,
                    Iv = fileMeta.Iv,
                    Tag = fileMeta.Tag,
                    UploadDate = DateTime.UtcNow
                };
                await _fileRepo.SaveFile(userFile);
                await _userService.AddFile(Convert.ToString(user.Id), userFile);
            }
            else
                await _userService.AddFile(Convert.ToString(user.Id), fileMeta);
        }

        public async Task SaveFileFromCache(FileFromCacheDto cacheFile)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud(cacheFile.base64EncFile, cacheFile.base64Tag);

            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            List<string> fileNamesUsed = new List<string>();
            foreach (UsersEmailsFilenames uef in cacheFile.emailsFilenames)
            { 
                User? user = await _userRepo.GetUserByEmail(uef.userEmail);
                if (user == null)
                    throw new Exception("User does not exist!");

                if(fileNamesUsed.Contains(uef.fileName))
                {
                    var fileMeta = await _fileRepo.GetFileMetaByTagAndFilename(cacheFile.base64Tag, uef.fileName);
                    if(fileMeta == null)
                        throw new Exception("File does not exist!");
                    await _userService.AddFile(Convert.ToString(user.Id), fileMeta);
                }
                else
                {
                    FileMetadata file = new FileMetadata
                    {
                        FileName = uef.fileName,
                        BlobLink = blobUrl,
                        isDeleted = false,
                        Key = cacheFile.key,
                        Iv = cacheFile.iv,
                        Tag = cacheFile.base64Tag,
                        UploadDate = DateTime.Parse(uef.uploadTime)
                    };
                    await _userService.AddFile(Convert.ToString(user.Id), file);
                    fileNamesUsed.Add(uef.fileName);
                    MerkleTree mt = await GetMerkleTree(cacheFile.base64EncFile);
                    await GenerateMerkleTreeChallenges(file.Id, mt);
                }
            }
        }

        public async Task<EncryptParamsDto> encryptParams(string userId, EncryptParamsDto paramsDto)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            paramsDto.fileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(paramsDto.fileName), Convert.FromBase64String(user.SymKey));
            paramsDto.fileKey = Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileKey), Convert.FromBase64String(user.SymKey));
            paramsDto.fileIv = Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileIv), Convert.FromBase64String(user.SymKey));

            return paramsDto;
        }
    }
}
