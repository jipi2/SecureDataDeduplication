using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class DeleteFileInfoDto
    {
        public string base64Tag { get; set; }
        public string fileName { get; set; }
        public string userEmail { get; set; }
    }
}
