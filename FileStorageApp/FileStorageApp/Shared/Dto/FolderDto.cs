using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FolderDto
    {
        public string name { get; set; }
        public string fullpath { get; set; }
        public List<FileModelDto> folderFiles { get; set; }
    }
}