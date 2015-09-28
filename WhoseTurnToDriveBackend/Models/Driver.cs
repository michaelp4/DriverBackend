using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Web;
using WhoseTurnToDriveBackend.Attributes;

namespace WhoseTurnToDriveBackend.Models
{
    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanDrive { get; set; }
        public bool HasCar { get; set; }
        [DefaultValue("0")]
        public int TimesBeenADriver { get; set; }
        [DefaultValue("0")]
        public int TimesBeenAPassenger { get; set; }
        [NotInDb]
        public bool IsDriving { get; set; }
    }
}