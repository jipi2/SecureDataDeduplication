using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileInfoFromCache
    {
        public float fileSize { get; set; }
        public string base64Tag { get; set; }
        public string fileName { get; set; }
        public string userEmail { get; set; }
        public string uploadDate { get; set; }
    }
}
