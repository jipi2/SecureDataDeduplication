using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class FSRC
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int SenderId { get; set; }
        public User? Sender { get; set; }

        [ForeignKey("User")]
        public int ReceiverId { get; set; }

        public string Base64KFrag { get; set; }
    }
}
