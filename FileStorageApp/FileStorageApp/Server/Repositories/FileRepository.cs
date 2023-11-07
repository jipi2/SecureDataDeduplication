using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;

namespace FileStorageApp.Server.Repositories
{
    public class FileRepository
    {
        DataContext _context;
        public FileRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> GetFileMetaByTag(string hexTag)
        {
            FileMetadata fileMeta = _context.FilesMetadata.Where(f => f.Tag == hexTag).FirstOrDefault();
            if (fileMeta == null)
                return false;
            return true;
        }

        public async Task SaveFile(FileMetadata fileMeta)
        {
            _context.FilesMetadata.Add(fileMeta);
            await _context.SaveChangesAsync();
        }
    }
}
