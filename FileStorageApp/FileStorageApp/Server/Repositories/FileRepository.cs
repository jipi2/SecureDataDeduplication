using FileStorageApp.Server.Database;

namespace FileStorageApp.Server.Repositories
{
    public class FileRepository
    {
        DataContext _context;
        public FileRepository(DataContext context)
        {
            _context = context;
        }

    }
}
