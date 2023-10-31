using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Data;

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
        public List<Role> Roles { get; set; }
    }
}
