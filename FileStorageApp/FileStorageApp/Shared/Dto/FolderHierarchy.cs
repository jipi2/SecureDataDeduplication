using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FolderHierarchy
    {
        public string FullPathName { get; set; }
        public string Name { get; set; }    
        public ICollection<FolderHierarchy> Children { get; set; }
    }
}
