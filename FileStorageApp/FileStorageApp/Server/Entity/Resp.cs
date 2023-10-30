using System.ComponentModel.DataAnnotations;

namespace FileStorageApp.Server.Entity
{
    public class Resp
    {
        [Key]
        public int Id { get; set; }
        public Challenge Challenge { get; set; }
        public string Answer { get; set; }
        public bool wasUsed { get; set; }
    }
}
