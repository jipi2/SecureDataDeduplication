using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
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
        
    }
}
