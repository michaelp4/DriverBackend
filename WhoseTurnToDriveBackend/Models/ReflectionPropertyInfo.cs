using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WhoseTurnToDriveBackend.Models
{
    public class ReflectionPropertyInfo
    {
        public string Name { get; set; }
        public object DefaultValue { get; set; }
        public bool NotInDb { get; set; }
        public Action<object, object> SetterAction { get; set; }
    }
}