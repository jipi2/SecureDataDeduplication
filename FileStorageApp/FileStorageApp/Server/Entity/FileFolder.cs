using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class FileFolder
    {
        [Key]
        public int Id { get; set; }
        public string FullPathName { get; set; }
        public int? ParentId { get; set; }
        public DateTime CreationDate { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        [ForeignKey("UserFile")]
        public int? UserFileId { get; set; }
        public UserFile? UserFile { get; set; }
        public virtual FileFolder? ParentFolder { get; set; }
        public virtual ICollection<FileFolder>? ChildFileFolders { get; set; }
    }
}
