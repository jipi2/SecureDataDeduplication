
namespace FileStorageApp.Server.Utils
{
    public class MTMember
    {
        public byte[] _hash { get; set; }
        public int _level { get; set; }

        public MTMember()
        {
            _hash = new byte[64];
            _level = 0;
        }

        public MTMember(int level, byte[] hash)
        {
            _hash = hash.ToArray();
            _level = level;
        }
    }
}
