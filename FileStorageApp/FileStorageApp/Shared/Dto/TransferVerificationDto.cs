﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class TransferVerificationDto
    {
        public string senderEmail { get; set; }
        public string receiverEmail { get; set; }
        public string fileName { get; set; }
    }
}
