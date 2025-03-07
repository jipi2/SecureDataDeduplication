﻿using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;


namespace FileStorageApp.Server.Services
{
    public class FileService
    {
        public UserFileRepository _userFileRepo { get; set; }
        public FileRepository _fileRepo { get; set; }
        public RespRepository _respRepo { get; set; }
        public UserService _userService { get; set; }
        public UserRepository _userRepo { get; set; }
        public AzureBlobService _azureBlobService { get; set; } 
        public FileTransferRepo _fileTransferRepo { get; set; }
        IConfiguration _configuration { get; set; }

        public FileFolderService _fileFolderService { get; set; }

        public FileFolderRepo _fileFolderRepo { get; set; }
        public FileService(FileRepository fileRepository, UserService userService, AzureBlobService azureBlobService, IConfiguration configuration, RespRepository respRepo, UserRepository userRepo, UserFileRepository userFileRepo, FileTransferRepo fileTransferRepo, FileFolderService fileFolderService, FileFolderRepo fileFolderRepo)
        {
            _fileRepo = fileRepository;
            _userService = userService;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _respRepo = respRepo;
            _userRepo = userRepo;
            _userFileRepo = userFileRepo;
            _fileTransferRepo = fileTransferRepo;
            _fileFolderService = fileFolderService;
            _fileFolderRepo = fileFolderRepo;
        }

        public async Task<string> GetFileFromBlobWithoutEncryption(string base64tag)
        {
            string base64file = await _azureBlobService.GetContentFileFromBlob(base64tag);
            return base64file;
        }

        private async Task GenerateMerkleTreeChallenges(int fileMetadataId, Utils.MerkleTree mt)
        {
            int J = Convert.ToInt32(_configuration["J"]);
            List<Resp> respList = new List<Resp>(J);
            int pos1=0, pos2=0, pos3=0;
            byte[] answ = new byte[mt.HashTree[0]._hash.Length];

            for (int i = 0; i < J; i++)
            {
                answ = Utils.Utils.GenerateResp(mt, ref pos1, ref pos2, ref pos3);
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

        private async Task<Utils.MerkleTree> GetMerkleTree(string base64EncFile)
        {
            byte[] bytesEncFile = Convert.FromBase64String(base64EncFile);
            Stream stream = new MemoryStream(bytesEncFile);
            Utils.MerkleTree mt = Utils.Utils.GetMerkleTree(stream);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }

        private async Task<Utils.MerkleTree> GetMerkleTree_v2(string filePath)
        {
            Utils.MerkleTree mt = Utils.Utils.GetMerkleTreeFromFilePath(filePath);
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
                BlobLink = blobUrl,
                isDeleted = false,
                Tag = base64Tag
            };

            //gata cod modificat

            await _fileRepo.SaveFile(fileMeta);

            //si am mai adaugat asta

            UserFile userFile = new UserFile
            {
                FileName = fileName,
                Key = base64Key,
                Iv = base64Iv,
                UploadDate = DateTime.UtcNow,
                FileId = fileMeta.Id,
                UserId = Convert.ToInt32(userId)
            };

          
            await _userFileRepo.SaveUserFile(userFile);
            //gata cu adaugatul
            
/*            await _userService.AddFile(userId, fileMeta);*/
            Utils.MerkleTree mt = await GetMerkleTree(base64EncFile);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
        }

        private async Task<bool> CheckIfFileNameExists(User user, string fileName)
        {
            UserFile uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            if(uf == null)
                return false;
            return true;

            //gata cod modificat
        
        }   

        public async Task<List<FilesNameDate>?> GetFileNamesAndDatesOfUser(string id)
        {
            User? user = await _userService.GetUserById(id);
            if (user == null)
                return null;

            //List<FilesNameDate>? result = user.Select(fileMeta => new FilesNameDate
            //{
            //    FileName = fileMeta.FileName,
            //    UploadDate = fileMeta.UploadDate,

            //}).ToList();

            //am modificat codul
            List<UserFile>? list = await _userFileRepo.GetUserFileByUserId(Convert.ToInt32(id));
            List<FilesNameDate> result = new List<FilesNameDate>();


            //codul asta era, nu-l modifica
            if (list == null)
            {
                result = new List<FilesNameDate>();
                result.Add(new FilesNameDate
                {
                    FileName = "No Files.",
                    UploadDate = DateTime.UtcNow
                });
            }
            //gata codul care era, restul poti modifica
            else
            {
                foreach (UserFile uf in list)
                {
                    FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(uf.FileId);
                    if (fileMeta != null)
                    {
                        if (fileMeta.isInCache == false)
                        {
                            result.Add(new FilesNameDate
                            {
                                FileName = uf.FileName,
                                FileSize = (float)uf.FileMetadata.Size,
                                UploadDate = uf.UploadDate
                            });
                        }
                    }
                }

            }

            return result;
        }

        //proxy logic
       public async Task<bool> CheckTagAvailabilityInCloud(string base64tag)
        {
            return await _fileRepo.GetFileMetaByTag(base64tag);
        }

        public async Task reloadChall(FileMetadata fileMeta)
        {
            Utils.MerkleTree mt = await Utils.Utils.GetMerkleTreeFromBlob(fileMeta.Tag, _azureBlobService);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
        }
        public async Task<FileMetaChallenge?> GetChallengeForTag(string base64Tag)
        {
            try
            {
                //string base64encFile = await GetFileFromBlobWithoutEncryption(base64Tag);
                FileMetadata fileMeta = await _fileRepo.GetFileMetaWithResp(base64Tag); 
                List<Resp?> resp = fileMeta.Resps.Where(r => r.wasUsed == false).ToList<Resp>(); 
                if (resp.Count() < 5)
                {
                    //reload the challenges
                    //aici va trebui sa completam noi
                    //MerkleTree mt = await GetMerkleTree(base64encFile);
                    //await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
                    //resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                    await reloadChall(fileMeta);
                }
                FileMetaChallenge fmc = new FileMetaChallenge();
                int random = (new Random()).Next(resp.Count());
                while(random > resp.Count()) random = (new Random()).Next(Convert.ToInt32(_configuration["J"]));
                fmc.id = resp[random].Id.ToString();
                fmc.n1 = resp[random].Position_1.ToString();
                fmc.n2 = resp[random].Position_2.ToString();
                fmc.n3 = resp[random].Position_3.ToString();
                resp[random].wasUsed = true;
                await _respRepo.UpdateResp(resp[random]);
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


        public async Task SaveDedupFile(FileDedupDto fileDedupDto)
        {
            User? user = await _userRepo.GetUserByEmail(fileDedupDto.userEmail);
            if(user == null)
                throw new Exception("User does not exist!");

            FileMetadata? fileMeta = await _fileRepo.GetFileMetaByTagIfExists(fileDedupDto.base64tag);
            if (fileMeta == null)
                throw new Exception("The file with this tag does not exists!");

            UserFile? uf = new UserFile
            {
                FileName = fileDedupDto.fileName,
                Key = fileDedupDto.base64key,
                Iv = fileDedupDto.base64iv,
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
                FileId = fileMeta.Id
            };

            await _userFileRepo.SaveUserFile(uf);
            await _fileFolderService.SaveFileToFolder(user, uf);
        }

        public async Task SaveFileFromCache(FileFromCacheDto cacheFile)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud(cacheFile.base64EncFile, cacheFile.base64Tag);

            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            List<string> fileNamesUsed = new List<string>();

            FileMetadata fileMeta = new FileMetadata
            {
                BlobLink = blobUrl,
                isDeleted = false,
                Tag = cacheFile.base64Tag
            };
            await _fileRepo.SaveFile(fileMeta);
            Utils.MerkleTree mt = await GetMerkleTree(cacheFile.base64EncFile);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);

            foreach (PersonalisedInfoDto pid in cacheFile.personalisedList)
            {
                User? user = await _userRepo.GetUserByEmail(pid.email);
                if (user == null)
                    throw new Exception("User does not exist!");

                UserFile uf = new UserFile
                {
                    FileName = pid.fileName,
                    Key = pid.base64key,
                    Iv = pid.base64iv,
                    UploadDate = DateTime.Parse(pid.UploadDate),
                    UserId = user.Id,
                    FileId = fileMeta.Id
                };

                await _userFileRepo.SaveUserFile(uf);
            }
        }

        private string GetFilePathByTag(string base64Tag)
        {
            string base64TagForFilePath = base64Tag.Replace("/", "_");
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            string filePath = Path.Combine(uploadsFolder, base64TagForFilePath);
            return filePath;

        }

        public async Task SaveFileFromCache(FileFromCacheDto_v2 fileParams)
        {
            string? blobUrl = await _azureBlobService.GetUri(fileParams.base64Tag);
            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            //il luam din baza de date daca exista deja, dar e gol pt ca era in cache
            FileMetadata? fileMeta = await _fileRepo.GetFileMetaByTagIfExists(fileParams.base64Tag);
            if (fileMeta == null)
            {
                //fileMeta = new FileMetadata
                //{
                //    BlobLink = blobUrl,
                //    isDeleted = false,
                //    Tag = fileParams.base64Tag,
                //    Size = fileParams.fileSize,
                //};
                //await _fileRepo.SaveFile(fileMeta);
                return;
            }
            else
            {
                fileMeta.BlobLink = blobUrl;
                fileMeta.isInCache = false;
                await _fileRepo.UpdateFile(fileMeta);
            }

            string filePath = GetFilePathByTag(fileParams.base64Tag);
            Utils.MerkleTree mt = await GetMerkleTree_v2(filePath);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);

            //aici stergem fisierul din folder pentru ca am terminat de generat mekrle tree-ul
            File.Delete(filePath);
            //gata stergerea

            List<FileTransfer> ftList = await _fileTransferRepo.GetAllFileTransfer();

            foreach (PersonalisedInfoDto pid in fileParams.personalisedList)
            {
                User? user = await _userRepo.GetUserByEmail(pid.email);
                if (user == null)
                    throw new Exception("User does not exist!");

                //il luam din baza de date daca exista si este gol pt ca era in cache
                UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, pid.fileName);
                if (uf == null)
                {
                    //uf = new UserFile
                    //{
                    //    FileName = pid.fileName,
                    //    Key = pid.base64key,
                    //    Iv = pid.base64iv,
                    //    UploadDate = DateTime.Parse(pid.UploadDate),
                    //    UserId = user.Id,
                    //    FileId = fileMeta.Id
                    //};
                    //await _userFileRepo.SaveUserFile(uf);
                    return;
                }
                else
                {
                    uf.Key = pid.base64key;
                    uf.Iv = pid.base64iv;
                    uf.UploadDate = DateTime.Parse(pid.UploadDate);
                    await _userFileRepo.UpdateUserFile(uf);
                }

                FileTransfer? ft = ftList.Where(f => f.FileName == pid.fileName && f.SenderId == user.Id).FirstOrDefault();
                if(ft != null)
                {
                    ft.isInCache = false;
                    await _fileTransferRepo.UpdateFileTransfer(ft);
                }

            }
        }

        public async Task WriteFileOnDiskAndOnCloud(IFormFile file, string base64Tag)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud_v2(file, base64Tag);
            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            //va trebui sa modificam tagul sa nu avem "/"
            string base64tagForFilePath = base64Tag.Replace("/", "_");
            string filePath = Path.Combine(uploadsFolder, base64tagForFilePath);
           
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                await file.CopyToAsync(stream);
            }
        }

        //public async Task<EncryptParamsDto> encryptParams(string userId, EncryptParamsDto paramsDto)
        //{
        //    User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
        //    paramsDto.fileName = Utils.Utils.EncryptAes(Encoding.UTF8.GetBytes(paramsDto.fileName), Convert.FromBase64String(user.SymKey));
        //    paramsDto.fileKey = Utils.Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileKey), Convert.FromBase64String(user.SymKey));
        //    paramsDto.fileIv = Utils.Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileIv), Convert.FromBase64String(user.SymKey));

        //    return paramsDto;
        //}

        public async Task DeleteFile(string userId, string fileName)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            if (userFile == null)
                throw new Exception("File does not exist!");
            List<UserFile>? luf = await _userFileRepo.GetUserFilesByFileId(userFile.FileId);
            if(luf == null)
                throw new Exception("File does not exist!");

            List<FileTransfer>? ft_list = await _fileTransferRepo.GetFileTransferBySenderAndTag(user, userFile.FileMetadata.Tag);
            if (ft_list != null)
            {
                foreach (FileTransfer ft in ft_list)
                {
                    await _fileTransferRepo.DeleteFileTransfer(ft);
                }
            }

            if (luf.Count > 1)
            {
                await _fileFolderRepo.DeleteFile(user, fileName);
                await _userFileRepo.DeleteUserFile(userFile);
            }
            else
            {
                //aici trebuie sa stergem de pe peste tot
                FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(userFile.FileId);
                bool res = await _azureBlobService.DeleteFileFromCloud(fileMeta.Tag);
                if (res == false)
                {
                    throw new Exception("Could not delete from cloud");
                }
                await _fileFolderRepo.DeleteFile(user, fileName);
                await _userFileRepo.DeleteUserFile(userFile);
                await _fileRepo.DeleteFile(fileMeta);
            }
        }

        public async Task SendFile(FileTransferDto ftdto, string senderid)
        {
            User? sender = await _userRepo.GetUserById(Convert.ToInt32(senderid));
            User? reciever = await _userRepo.GetUserByEmail(ftdto.recieverEmail);
            if(reciever == null)
                throw new Exception("Reciever does not exist!");
            if (sender == null)
                throw new Exception("User does not exist!");
            if (sender == reciever)
                throw new Exception("You cannot send a file to yourself!");

            bool isInCache = ftdto.isInCache;

            if(isInCache == false)
                if (await CheckIfFileNameExists(sender, ftdto.fullPath) == false)
                    throw new Exception("The sender does not have that file!");

            //UserFile? ufrec = await _userFileRepo.GetUserFileByUserIdAndFileName(reciever.Id, ftdto.fileName);
            //if (ufrec != null)
            //    throw new Exception("Reciever already has this file!");

            FileTransfer fte = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, reciever.Id, ftdto.fileName);
            if (fte != null)
                throw new Exception("You already sent this file to this user!");

            FileTransfer ft = new FileTransfer()
            {
                ReceiverId = reciever.Id,
                SenderId = sender.Id,
                FileName = ftdto.fileName,
                base64EncKey = ftdto.base64EncKey,
                base64EncIv = ftdto.base64EncIv,
                isDeleted = false,
                Tag = ftdto.base64Tag,
                isInCache = isInCache
            };
            await _fileTransferRepo.SaveFileTransfer(ft);
        }

        public async Task<RsaKeyFileKeyDto> GetRsaPubKeyAndFileKey(EmailFilenameDto ef, string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if(user == null)
                throw new Exception("User does not exist!");
            User? reciever = await _userRepo.GetUserByEmail(ef.userEmail);
            if(reciever == null)
                throw new Exception("Reciever does not exist!");
            UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, ef.fileName);        
            if(uf == null)
                throw new Exception("File does not exist!");

            RsaKeyFileKeyDto rsaKeyFileKeyDto = new RsaKeyFileKeyDto
            {
                pubKey = reciever.Base64RSAPublicKey,
                fileKey = uf.Key,
                fileIv = uf.Iv
            };
            return rsaKeyFileKeyDto;
        }

        public async Task<List<RecievedFilesDto>?> GetRecievedFiles(string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");
            List<RecievedFilesDto> lrfd = new List<RecievedFilesDto>();
            List<FileTransfer>? listFileTransfer = await _fileTransferRepo.GetFileTransferByRecId(user.Id);
            foreach(FileTransfer ft in listFileTransfer)
            {
                User? sender = await _userRepo.GetUserById(Convert.ToInt32(ft.SenderId));
                lrfd.Add(new RecievedFilesDto
                {
                    senderEmail =  sender.Email,
                    fileName = ft.FileName,
                    base64EncKey = ft.base64EncKey,
                    base64EncIv = ft.base64EncIv,
                    base64Tag = ft.Tag
                });
            }

            return lrfd;
        }

        public async Task RemoveRecievedFile(RecievedFilesDto rfd, string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");

            User? sender = await _userRepo.GetUserByEmail(rfd.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, user.Id, rfd.fileName);
            if (ft == null)
                throw new Exception("File transfer does not exist!");
            await _fileTransferRepo.DeleteFileTransfer(ft);
        }

        public async Task AcceptRecievedFile(AcceptFileTransferDto aft)
        {
            User? user = await _userRepo.GetUserByEmail(aft.receiverEmail);
            if (user == null)
                throw new Exception("User does not exist!");

            User? sender = await _userRepo.GetUserByEmail(aft.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");

            //UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(sender.Id, aft.fileName);
            //if (uf == null)
            //    throw new Exception("Error file");

            FileMetadata? fm = await _fileRepo.GetFileMetaWithResp(aft.base64Tag);
            if(fm == null)
                throw new Exception("Error file");

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, user.Id, aft.fileName);
            if(ft == null)
                throw new Exception("Error file transfer");
            await _fileTransferRepo.DeleteFileTransfer(ft);

            UserFile userFile = new UserFile
            {
                FileName = aft.fullPath,
                Key = aft.base64FileKey,
                Iv = aft.base64FileIv,
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
                FileId = fm.Id
            };
            await _userFileRepo.SaveUserFile(userFile);
            await _fileFolderService.SaveFileToFolder(user, userFile);
        }

        public async Task<BlobFileParamsDto> GetUrlFileFromStorage(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
;
            FileMetadata? file = await _fileRepo.GetFileMetaById(userFile.FileId);

            BlobFileParamsDto serverFile = new BlobFileParamsDto
            {
                FileName = fileName,
                FileKey = userFile.Key,
                FileIv = userFile.Iv,
                base64tag = file.Tag
            };

            return serverFile;
        }

        public async Task<FileKeyAndIvDto> GetKeyAndIvForFile(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);

            FileKeyAndIvDto dto = new FileKeyAndIvDto
            {
                base64key = userFile.Key,
                base64iv = userFile.Iv,
                base64Tag = userFile.FileMetadata.Tag
            };
            return dto;
        }

        public async Task<bool> VerifyFileTransfer(TransferVerificationDto tvd)
        {
            User? sender = await _userRepo.GetUserByEmail(tvd.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");
            User? receiver = await _userRepo.GetUserByEmail(tvd.receiverEmail);
            if (receiver == null)
                throw new Exception("Receiver does not exist!");

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, receiver.Id, tvd.fileName);
            if (ft == null)
                throw new Exception("File transfer does not exist!");
            return ft.isInCache;
        }

        public async Task DeleteFileTransfer(TransferVerificationDto tvt)
        {
            User? sender = await _userRepo.GetUserByEmail(tvt.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");
            User? receiver = await _userRepo.GetUserByEmail(tvt.receiverEmail);
            if (receiver == null)
                throw new Exception("Receiver does not exist!");
            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, receiver.Id, tvt.fileName);
            if (ft == null)
                throw new Exception("File transfer does not exist!");

            await _fileTransferRepo.DeleteFileTransfer(ft);
        }

        public async Task SaveFileInfoFromCache(FileInfoFromCache dto)
        {
            User? user = await _userRepo.GetUserByEmail(dto.userEmail);
            if (user == null)
                throw new Exception("User does not exist!");
            FileMetadata? fileMetadata = await _fileRepo.GetFileMetaByTagIfExists(dto.base64Tag);
            if (fileMetadata == null)
            {
                fileMetadata = new FileMetadata
                {
                    Tag = dto.base64Tag,
                    isDeleted = false,
                    isInCache = true,
                    Size = dto.fileSize
                };
                await _fileRepo.SaveFile(fileMetadata);
            }

            UserFile uf = new UserFile
            {
                FileName = dto.fileName,
                UploadDate = DateTime.Parse(dto.uploadDate),
                UserId = user.Id,
                FileId = fileMetadata.Id
            };

            await _userFileRepo.SaveUserFile(uf);
            await _fileFolderService.SaveFileToFolder(user, uf);
        }

        public async Task DeleteFileInfoFromServer(DeleteFileInfoDto dto)
        {
            User? user = await _userRepo.GetUserByEmail(dto.userEmail);
            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, dto.fileName);
            if (userFile == null)
                throw new Exception("File does not exist!");
            List<UserFile>? luf = await _userFileRepo.GetUserFilesByFileId(userFile.FileId);
            if (luf == null)
                throw new Exception("File does not exist!");

            List<FileTransfer>? ft_list = await _fileTransferRepo.GetFileTransferBySenderAndTag(user, userFile.FileMetadata.Tag);
            if (ft_list != null)
            {
                foreach (FileTransfer ft in ft_list)
                {
                    await _fileTransferRepo.DeleteFileTransfer(ft);
                }
            }

            if (luf.Count > 1)
            {
                await _fileFolderRepo.DeleteFile(user, dto.fileName);
                await _userFileRepo.DeleteUserFile(userFile);    
            }
            else
            {
                //aici trebuie sa stergem de pe peste tot
                await _fileFolderRepo.DeleteFile(user, dto.fileName);
                FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(userFile.FileId);
                await _userFileRepo.DeleteUserFile(userFile);
                await _fileRepo.DeleteFile(fileMeta);
            }
        }

        public async Task SaveFileInfoRecievedFromAnotherUser(AcceptFileTransferDto aft)
        {
            User? user = await _userRepo.GetUserByEmail(aft.receiverEmail);
            if (user == null)
                throw new Exception("User does not exist!");

            User? sender = await _userRepo.GetUserByEmail(aft.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");

            //UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(sender.Id, aft.fileName);
            //if (uf == null)
            //    throw new Exception("Error file");

            FileMetadata? fm = await _fileRepo.GetFileMetaWithResp(aft.base64Tag);
            if (fm == null)
                throw new Exception("Error file");

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, user.Id, aft.fileName);
            if (ft == null)
                throw new Exception("Error file transfer");
            await _fileTransferRepo.DeleteFileTransfer(ft);

            UserFile userFile = new UserFile
            {
                FileName = aft.fullPath,
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
                FileId = fm.Id
            };
            await _userFileRepo.SaveUserFile(userFile);
            await _fileFolderService.SaveFileToFolder(user, userFile);
        }

        public async Task<bool> VerifyNameDuplicate(string userId, string fullPathName)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");

            UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fullPathName);
            if(uf != null)
                return true;
            return false;
        }

        public async Task RenameFile(string userId, RenameFileDto rfd)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");

            UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, rfd.oldFullPath);
            if (uf == null)
                throw new Exception("File does not exist!");
            uf.FileName = rfd.newFullPath;

            FileFolder? ff = await _fileFolderRepo.GetFileFolderByUserAndName(user, rfd.oldFullPath);
            if(ff == null)
                throw new Exception("File does not exist in folder!");

            ff.FullPathName = rfd.newFullPath;
            await _userFileRepo.UpdateUserFile(uf);
            await _fileFolderRepo.UpdateFileFolder(ff);
        }

        public async Task RenameFolder(string userId, RenameFileDto rfd)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User does not exist!");

            List<FileFolder>? list = await _fileFolderRepo.GetFileFolderALike(user, rfd.oldFullPath);
            if (list != null)
            {
                foreach (FileFolder ff in list)
                {
                    ff.FullPathName = ff.FullPathName.Replace(rfd.oldFullPath, rfd.newFullPath);
                    if (ff.UserFile != null)
                    {
                        ff.UserFile.FileName =  ff.UserFile.FileName.Replace(rfd.oldFullPath, rfd.newFullPath);
                        await _userFileRepo.UpdateUserFile(ff.UserFile);
                    }
                    await _fileFolderRepo.UpdateFileFolder(ff);
                }
            }
        }
        }
}
