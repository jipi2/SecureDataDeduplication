using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class SimpleFileModelDto
    {
        public string FullPathName { get; set; }
        public string Name { get; set; }
        public bool IsFolder { get; set; }
        public  string Icon { get; set; }
        public  string IconColor { get; set; }
        public ICollection<SimpleFileModelDto> Children { get; set; }
    }
}
