using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoseTurnToDriveBackend.Models
{
    public class CurrentDriver
    {
        public int DriverId { get; set; }
        public bool IsDriving { get; set; }
        public DateTime DateTime { get; set; }
        public string Destination { get; set; }
    }
}