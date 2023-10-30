using System.ComponentModel.DataAnnotations;

namespace FileStorageApp.Server.Entity
{
    public class FileMetadata
    {
        [Key]
        public int Id { get; set; }
        public string BlobLink { get; set; }
        public bool isDeleted { get; set; }
        public string? key { get; set; }
        //Navigation Challenge
        public List<Challenge> Challenges { get; set; }
    }
}
