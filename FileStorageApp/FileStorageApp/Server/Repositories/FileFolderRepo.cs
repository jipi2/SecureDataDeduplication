using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using FileStorageApp.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FileStorageApp.Server.Repositories
{
    public class FileFolderRepo
    {
        DataContext _context;
        public FileFolderRepo(DataContext context)
        {
            _context = context;
        }

        public async Task CreateRootFolderForUser(User user)
        {
            try
            {
                FileFolder rootFolder = new FileFolder
                {
                    FullPathName="/",
                    UserId = user.Id,
                    UserFileId = null
                };
                _context.FileFolders.Add(rootFolder);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<FileFolder?> GetFileFolderByUserAndName(User user, string name)
        {
            try
            {
                FileFolder? fileFolder = await _context.FileFolders
                    .Include(f => f.ChildFileFolders)
                        .ThenInclude(child => child.UserFile)
                            .ThenInclude(userFile => userFile.FileMetadata)
                    .Include(f => f.UserFile)
                        .ThenInclude(userFile => userFile.FileMetadata)
                    .Where(f => f.UserId == user.Id && f.FullPathName == name).FirstOrDefaultAsync();
                if(fileFolder == null)
                {
                    return null;
                }
                return fileFolder;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task CreateFolder(FileFolder parentFolder, FileFolder newFolder)
        {
            try
            {
               
                if(parentFolder.ChildFileFolders == null)
                {
                    parentFolder.ChildFileFolders = new List<FileFolder>();
                }

                parentFolder.ChildFileFolders.Add(newFolder);

                _context.FileFolders.Add(newFolder);
                _context.FileFolders.Update(parentFolder);

                await _context.SaveChangesAsync();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task SaveFileToFolder(FileFolder folder, FileFolder file)
        {
            try
            {
                if (folder.ChildFileFolders == null)
                {
                    folder.ChildFileFolders = new List<FileFolder>();
                }
                folder.ChildFileFolders.Add(file);
                _context.FileFolders.Add(file);
                _context.FileFolders.Update(folder);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task DeleteFile(User user, string fullpath)
        {
            try
            {
                FileFolder? fileFolder = await GetFileFolderByUserAndName(user, fullpath);
                if (fileFolder == null) throw new Exception("File not found");
                _context.FileFolders.Remove(fileFolder);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        //public async Task<FileFolder?> GetFolderHierarchyForUser(User user)
        //{
        //    try
        //    {
        //        // Fetch the root folder for the user ("/")
        //        var rootFolder = await _context.FileFolders
        //            .Include(f => f.ChildFileFolders) // Include child folders
        //            .FirstOrDefaultAsync(f => f.UserId == user.Id && f.FullPathName == "/" && f.UserFileId == null); // Exclude files

        //        // Populate child folders recursively
        //        await PopulateChildFolders(rootFolder);

        //        // Return the root folder with populated child folders
        //        return rootFolder;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        //private async Task PopulateChildFolders(FileFolder parentFolder)
        //{
        //    if (parentFolder != null)
        //    {
        //        // Fetch child folders for the parent folder
        //        parentFolder.ChildFileFolders = await _context.FileFolders
        //            .Where(f => f.ParentId == parentFolder.Id && f.UserFileId == null) // Exclude files
        //            .ToListAsync();

        //        // Recursively populate child folders
        //        foreach (var childFolder in parentFolder.ChildFileFolders)
        //        {
        //            await PopulateChildFolders(childFolder);
        //        }
        //    }
        //}

        public async Task<FolderHierarchy?> GetFolderHierarchyForUser(User user)
        {
            try
            {
                // Fetch the root folder for the user ("/")
                var rootFolder = await _context.FileFolders
                    .Include(f => f.ChildFileFolders) 
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.FullPathName == "/" && f.UserFileId == null); // Exclude files

                // Populate child folders recursively
                if(rootFolder.ChildFileFolders != null)
                    rootFolder.ChildFileFolders = rootFolder.ChildFileFolders?
                        .OrderBy(f => f.FullPathName).ToList();
                var rootFolderDTO = await PopulateJustChildFolders(rootFolder);

                // Return the root folder DTO with populated child folders
                return rootFolderDTO;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<FolderHierarchy?> PopulateJustChildFolders(FileFolder parentFolder)
        {
            if (parentFolder != null)
            {
                // Create a new DTO for the parent folder
                string name = parentFolder.FullPathName.Split('/').Last();
                if (name == "") name = "/";
                var parentFolderDTO = new FolderHierarchy
                {
                    FullPathName = parentFolder.FullPathName,
                    Name = name,
                    Children = new List<FolderHierarchy>()
                };

                // Fetch child folders for the parent folder
                var childFolders = await _context.FileFolders
                    .Where(f => f.ParentId == parentFolder.Id && f.UserFileId == null)
                    .OrderBy(f => f.FullPathName)
                    .ToListAsync();

                // Recursively populate child folders
                foreach (var childFolder in childFolders)
                {
                    // Populate child folders recursively
                    var childFolderDTO = await PopulateJustChildFolders(childFolder);

                    // Add the child folder DTO to the parent folder DTO
                    parentFolderDTO.Children.Add(childFolderDTO);
                }

                return parentFolderDTO;
            }

            return null;
        }

        public async Task<SimpleFileModelDto?> GetFolderWithFilesHierarchyForUser(User user)
        {
            try
            {
                // Fetch the root folder for the user ("/")
                var rootFolder = await _context.FileFolders
                    .Include(f => f.ChildFileFolders) // Include child folders
                    .FirstOrDefaultAsync(f => f.UserId == user.Id && f.FullPathName == "/"); // Exclude files

                // Populate child folders recursively
                if (rootFolder.ChildFileFolders != null)
                    rootFolder.ChildFileFolders = rootFolder.ChildFileFolders
                       .OrderByDescending(f => f.UserFileId != null)
                       .ThenBy(f => f.FullPathName)
                       .ToList();
                var rootFolderDTO = await PopulateJustChildFoldersAndFiles(rootFolder);


                // Return the root folder DTO with populated child folders
                return rootFolderDTO;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<SimpleFileModelDto?> PopulateJustChildFoldersAndFiles(FileFolder parentFolder)
        {
            if (parentFolder != null)
            {
                // Create a new DTO for the parent folder
                string name = parentFolder.FullPathName.Split('/').Last();
                if (name == "") name = "/";
                bool isFolder = parentFolder.UserFileId == null;
                var parentFolderDTO = new SimpleFileModelDto
                {
                    FullPathName = parentFolder.FullPathName,
                    Name = name,
                    IsFolder = isFolder,
                    Children = new List<SimpleFileModelDto>()
                };

                // Fetch child folders for the parent folder
                var childFolders = await _context.FileFolders
                    .Where(f => f.ParentId == parentFolder.Id )
                    .OrderBy(f => f.UserFileId != null) // Folders (UserFileId == null) first
                    .ThenBy(f => f.FullPathName) // Then alphabetical
                    .ToListAsync();

                // Recursively populate child folders
                foreach (var childFolder in childFolders)
                {
                    // Populate child folders recursively
                    var childFolderDTO = await PopulateJustChildFoldersAndFiles(childFolder);

                    // Add the child folder DTO to the parent folder DTO
                    parentFolderDTO.Children.Add(childFolderDTO);
                }

                return parentFolderDTO;
            }

            return null;
        }

       public async Task<List<FileFolder>?> GetChildren(FileFolder folder)
        {
            if (folder != null)
            {
                var childFolders = await _context.FileFolders
                  .Where(f => f.ParentId == folder.Id)
                  .OrderBy(f => f.UserFileId != null) // Folders (UserFileId == null) first
                  .ThenBy(f => f.FullPathName) // Then alphabetical
                  .ToListAsync();
                return childFolders;
            }
            return null;
        }
    }
}
