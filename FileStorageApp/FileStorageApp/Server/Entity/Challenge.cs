namespace FileStorageApp.Server.Entity
{
    public class Challenge
    {
        public int Id { get; set; }
        public int Seed { get; set; }

        //Navigation FileInfo
        public FileMetadata FileMetadata { get; set; }
        public ICollection<Resp> Resps { get; set; }
    }
}
