
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

        public FileFolderService(FileFolderRepo fileFolderRepo, UserService userService, UserRepository userRepository) 
        {
            _fileFolderRepo = fileFolderRepo;
            _userService = userService;
            _userRepository = userRepository;
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
                folderDto.folderFiles = new List<FileModelDto>();
                foreach (var f in folder.ChildFileFolders)
                {
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
                    }
                    else
                    {
                        if (f.UserFile.FileMetadata.Size == null) throw new Exception("The file has no size");
                        fileModelDto.fileSize = (float)f.UserFile.FileMetadata.Size;
                        fileModelDto.fileSizeStr = String.Format("{0}", fileModelDto.fileSize);
                        fileModelDto.isFolder = false;

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
    }
}
