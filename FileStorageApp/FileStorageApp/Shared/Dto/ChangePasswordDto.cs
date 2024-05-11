using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class ChangePasswordDto
    {
        public string oldPass { get; set; }
        public string newPass { get; set; }
        public string confirmNewPass { get; set; }
        public string base64pkcs12 { get; set; }
    }
}
