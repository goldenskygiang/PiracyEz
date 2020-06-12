using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiracyEz.Models
{
    public class FacebookAPI
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string AccessToken { get; set; }
    }

    public class AppConfig
    {
        public string AdminEmail { get; set; }
    }
}
