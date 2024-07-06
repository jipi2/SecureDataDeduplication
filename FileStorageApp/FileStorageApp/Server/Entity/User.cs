using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStorageApp.Server.Entity
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [StringLength(20)]
        public string LastName { get; set; }
        [StringLength(20)]
        public string FirstName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [PasswordPropertyText]
        public string Password { get; set; }
        public string Salt { get; set; }
        public bool isDeleted { get; set; }
        public string? Base64RSAPublicKey { get; set; }
        [Column(TypeName ="VARBINARY(MAX)")]
        public byte[]? Pkcs12File { get; set; }
        public string? Base64PublicKey { get; set; }
        public List<Role>? Roles { get; set; }
        public string? VerificationCode { get; set; }
        public bool? IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
