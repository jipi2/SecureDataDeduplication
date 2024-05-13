using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileModelDto
    {
        public string fileName { get; set; }
        public string fullPath { get; set; }
        public float fileSize { get; set; }
        public string fileSizeStr { get; set; }
        public string uploadDate { get; set; }
        public bool isFolder { get; set; }
    }
}
