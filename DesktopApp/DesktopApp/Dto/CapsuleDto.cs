using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class CapsuleDto
    {
        public string base64KeyCapsule { get; set; }
        public string  base64IvCapsule { get; set; }
        public string fileName { get; set; }
        public  string destEmail { get; set; }
    }
}
