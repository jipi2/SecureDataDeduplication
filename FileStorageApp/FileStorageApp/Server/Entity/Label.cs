using Azure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class Label
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public List<UserFile> UserFiles { get; set; }
    }
}
