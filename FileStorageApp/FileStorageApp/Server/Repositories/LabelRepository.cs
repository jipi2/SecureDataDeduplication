using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;

namespace FileStorageApp.Server.Repositories
{
    public class LabelRepository
    {
        DataContext _context;
        public LabelRepository(DataContext context)
        {
            _context = context;
        }

        public async Task SaveLabel(Label label)
        {
            _context.Labels.Add(label);
            await _context.SaveChangesAsync();
        }

        public async Task<Label?> GetLabelByUserIdAndName(int userId, string labelName)
        {
            Label? label = await _context.Labels.Include(uf => uf.UserFiles).Where(l => l.Name == labelName && l.UserId == userId).FirstOrDefaultAsync();
            if (label == null)
                return null;
            return label;
        }

        public async Task UpdateLabel(Label label)
        {
            _context.Labels.Update(label);
            await _context.SaveChangesAsync();
        }

        internal async Task<List<Label>> GetLabelsByUserId(int userId)
        {
            List<Label> list = await _context.Labels.Include(uf => uf.UserFiles).Where(l => l.UserId == userId).ToListAsync();
            return list;
        }

        public async Task RemoveLabel(Label label)
        {
            _context.Labels.Remove(label);
            await _context.SaveChangesAsync();
        }
    }
}
