using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class RenameFileDto
    {
        public string oldFullPath { get; set; }
        public string newFullPath { get; set; }
        public bool isFolder { get; set; }
    }
}
