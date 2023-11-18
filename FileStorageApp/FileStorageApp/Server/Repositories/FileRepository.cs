using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;

namespace FileStorageApp.Server.Repositories
{
    public class FileRepository
    {
        DataContext _context;
        public FileRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> GetFileMetaByTag(string base64Tag)
        {
            FileMetadata fileMeta = _context.FilesMetadata.Where(f => f.Tag == base64Tag).FirstOrDefault();
            if (fileMeta == null)
                return false;
            return true;
        }

        public async Task<FileMetadata?> GetFileMetaWithResp(string base64Tag)
        {
            FileMetadata? fileMetadata = _context.FilesMetadata.Include(r => r.Resps)
                                        .Where(f => f.Tag == base64Tag)
                                        .FirstOrDefault();
            return fileMetadata;
        }

        public async Task SaveFile(FileMetadata fileMeta)
        {
            _context.FilesMetadata.Add(fileMeta);
            await _context.SaveChangesAsync();
        }

        public async Task<FileMetadata?> GetFileMetaById(int id)
        {
            FileMetadata? fileMetadata = await _context.FilesMetadata.Where(f => f.Id == id).FirstOrDefaultAsync();
            return fileMetadata;
        }
    }
}
