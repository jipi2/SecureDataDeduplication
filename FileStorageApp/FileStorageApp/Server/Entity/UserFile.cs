namespace FileStorageApp.Server.Entity
{
    public class UserFile
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int FileMetadataId { get; set; }
        public FileMetadata FileMetadata { get; set; }

        public DateTime UploadDate { get; set; }
    }
}
