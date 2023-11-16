using System.ComponentModel.DataAnnotations;

namespace FileStorageApp.Server.Entity
{
    public class FileMetadata
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }
        public string BlobLink { get; set; }
        public bool isDeleted { get; set; }
        public string? Key { get; set; }
        public string? Iv { get; set; }
        public string Tag { get; set; }
        public DateTime UploadDate { get; set; }

        //Navigation Challenge
        public List<Challenge> Challenges { get; set; }
        public List<User> Users { get; set; }
    }
}
