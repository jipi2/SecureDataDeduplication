
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared.Dto;
using System.IO;

namespace FileStorageApp.Server.Services
{
    public class FileFolderService
    {

        public FileFolderRepo _fileFolderRepo { get; set; }
        public UserService _userService { get; set; }
        public UserRepository _userRepository { get; set; }
        public UserFileRepository _userFileRepo { get; set; }
        public FileRepository _fileRepo { get; set; }
        public AzureBlobService _azureBlobService { get; set; }
        public FileFolderService(FileFolderRepo fileFolderRepo, UserService userService, UserRepository userRepository, UserFileRepository userFileRepository, FileRepository fileRepository, AzureBlobService azureBlobService) 
        {
            _fileFolderRepo = fileFolderRepo;
            _userService = userService;
            _userRepository = userRepository;
            _userFileRepo = userFileRepository;
            _fileRepo = fileRepository;
            _azureBlobService = azureBlobService;
        }
        public async Task CreateFolder(string userId ,string fullFolderPath)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");

            string[] segments = fullFolderPath.Split('/');

            string folderPath = string.Join("/", segments.Take(segments.Length - 1));

            if(folderPath == "") folderPath = "/";

            FileFolder? parentFolder = await _fileFolderRepo.GetFileFolderByUserAndName(user, folderPath);
            if (parentFolder == null) throw new Exception("Parent folder not found");

            FileFolder newFolder = new FileFolder
            {
                FullPathName = fullFolderPath,
                UserId = user.Id,
                UserFileId = null,
                CreationDate = DateTime.Now,
            };

            await _fileFolderRepo.CreateFolder(parentFolder, newFolder);
        }

        public async Task SaveFileToFolder(User user, UserFile uf)
        {
            string[] segments = uf.FileName.Split('/');
            string folderPath = string.Join("/", segments.Take(segments.Length - 1));
            if (folderPath == "") folderPath = "/";

            FileFolder? folder = await _fileFolderRepo.GetFileFolderByUserAndName(user, folderPath);
            if (folder == null) throw new Exception("Parent folder not found");

            FileFolder newFolder = new FileFolder
            {
                FullPathName = uf.FileName,
                UserId = user.Id,
                UserFileId = uf.Id,
                ParentFolder = folder,
                CreationDate = uf.UploadDate
            };

            await _fileFolderRepo.SaveFileToFolder(folder, newFolder);
        }

        public async Task<FolderDto?> GetFolderWithFiles(string userId, string path)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");
            FileFolder? folder = await _fileFolderRepo.GetFileFolderByUserAndName(user, path);
            if (folder == null) throw new Exception("Folder not found");

            string folderName;

            if (folder.FullPathName == "/")
            {
                folderName = path;
            }
            else
            {
                string[] segments = folder.FullPathName.Split('/');
                folderName = segments.Last();
            }
            FolderDto folderDto = new FolderDto
            {
                fullpath = folder.FullPathName,
                name = folderName,
                folderFiles = null
            };
            if (folder.ChildFileFolders != null)
            {
                folder.ChildFileFolders = folder.ChildFileFolders.OrderBy(f => f.UserFileId != null)
                                            .ThenBy(f => f.FullPathName).ToList();          
                folderDto.folderFiles = new List<FileModelDto>();
                foreach (var f in folder.ChildFileFolders)
                {
                    string icon = "";
                    string iconColor = "";
                    FileModelDto fileModelDto = new FileModelDto
                    {
                        fileName = f.FullPathName.Split('/').Last(),
                        fullPath = f.FullPathName,
                        uploadDate = f.CreationDate.ToString("yyyy-MM-dd")
                    };

                    if (f.UserFile == null)
                    {
                        fileModelDto.fileSize = 0;
                        fileModelDto.fileSizeStr = "";
                        fileModelDto.isFolder = true;
                        fileModelDto.icon = "";
                        fileModelDto.iconColor = "";
                    }
                    else
                    {
                        if (f.UserFile.FileMetadata.Size == null) throw new Exception("The file has no size");
                        fileModelDto.fileSize = (float)f.UserFile.FileMetadata.Size;
                        fileModelDto.fileSizeStr = String.Format("{0}", fileModelDto.fileSize);
                        fileModelDto.isFolder = false;
                        _fileFolderRepo.GetIconAndIconColor(fileModelDto.fileName, out icon, out iconColor);
                        fileModelDto.icon = icon;
                        fileModelDto.iconColor = iconColor;

                    }

                    folderDto.folderFiles.Add(fileModelDto);
                }
            }
            return folderDto;
        }

        public async Task<FolderHierarchy?> GetAllFolders(string userId)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");

            FolderHierarchy? folders = await _fileFolderRepo.GetFolderHierarchyForUser(user);
            return folders;

        }

        public async Task<SimpleFileModelDto> GetAllFoldersWithFiles(string userId)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");

            SimpleFileModelDto? folders = await _fileFolderRepo.GetFolderWithFilesHierarchyForUser(user);
            return folders;
        }

        private async Task DeleteUserFile(User user, string fileName)
        {
            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            if (userFile == null)
                throw new Exception("File does not exist!");
            List<UserFile>? luf = await _userFileRepo.GetUserFilesByFileId(userFile.FileId);
            if (luf == null)
                throw new Exception("File does not exist!");
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

        public async Task DeleteFileFolderChildren(FileFolder f, User user)
        {
            f.ChildFileFolders = await _fileFolderRepo.GetChildren(f);

            if (f.ChildFileFolders == null || f.ChildFileFolders.Count() == 0)
            {
                if(f.UserFileId != null)
                {
                    await this.DeleteUserFile(user, f.FullPathName);
                }
                else
                {
                    await _fileFolderRepo.DeleteFile(user, f.FullPathName);
                }
            }
            else
            {
                while(f.ChildFileFolders != null && f.ChildFileFolders.Count() > 0)
                {
                    var ff = f.ChildFileFolders.First();
                    await DeleteFileFolderChildren(ff, user);
                    f.ChildFileFolders = await _fileFolderRepo.GetChildren(f);
                }
                if (f.UserFile != null)
                {
                    await this.DeleteUserFile(user, f.FullPathName);
                }
                else
                {
                    await _fileFolderRepo.DeleteFile(user, f.FullPathName);
                }
            }

        }
        public async Task DeleteFolder(string userId, string path)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");

            if (path == "/") throw new Exception("You cannot delete the root folder!");

            FileFolder? folder = await _fileFolderRepo.GetFileFolderByUserAndName(user, path);
            //foreach (var f in folder.ChildFileFolders)
            //{
            //    if (f.UserFile != null)
            //    {
            //       await this.DeleteUserFile(user, f.FullPathName);
            //    }
            //    else
            //    {
            //        await _fileFolderRepo.DeleteFile(user, f.FullPathName);
            //    }
            //}

            //await _fileFolderRepo.DeleteFile(user, path);
            if(folder != null)
                await DeleteFileFolderChildren(folder, user);
        }

        public async Task<bool> CheckFolderNameDuplicate(string userId, string fullPath)
        {
            User? user = await _userRepository.GetUserById(Convert.ToInt32(userId));
            if (user == null) throw new Exception("User not found");

            FileFolder? folder = await _fileFolderRepo.GetFileFolderByUserAndName(user, fullPath);
            if (folder != null) return true;
            return false;
        }
    }
}
