using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Models
{
    public class Folder
    {
        public string name { get; set; }
        public string fullpath { get; set; }
        public List<File>  folderFiles { get; set; }
    }
}
