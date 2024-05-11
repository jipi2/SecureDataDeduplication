using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.Dto
{
    public class Pkcs12Dto
    {
        public string base64PubKey { get; set; }
        public string base64EncPrivKey { get; set; }

        public string base64pbkey_d { get; set; }
    }
}
