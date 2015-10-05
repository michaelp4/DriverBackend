using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using WhoseTurnToDriveBackend.Attributes;
using WhoseTurnToDriveBackend.Models;

namespace WhoseTurnToDriveBackend.Utils
{
    public static class PropertiesGetter<T> where T : new()
    {
        private static readonly IDictionary<string, Action<object, object>> PropertyNameToGetValueFunc = typeof(T)
            .GetProperties().Where(p => Attribute.IsDefined(p, typeof(NotInDbAttribute)) == false)
            .ToDictionary(x => x.Name, x => new Action<object, object>(x.SetValue));

        private static readonly IEnumerable<ReflectionPropertyInfo> Properties =
            typeof(T).GetProperties().Select(p => new ReflectionPropertyInfo
            {
                Name = p.Name,
                NotInDb = Attribute.IsDefined(p, typeof(NotInDbAttribute)),
                SetterAction = p.SetValue,
                DefaultValue = p.GetCustomAttributes(true).OfType<DefaultValueAttribute>().Select(x => x.Value).SingleOrDefault()
            });
        public static T GetObjectByDataRow(DataRowView dataRow)
        {
            var setableObject = new T();
            foreach (var property in Properties.Where(x => !x.NotInDb))
            {
                var value = dataRow.Row[property.Name];
                if (value is DBNull == false)
                {
                    property.SetterAction(setableObject, value);
                }
                
            }
            return setableObject;
        }

        public static string SetParametersFromObject(MySqlParameterCollection paramsCollection, JObject jObject,
            IEnumerable<string> excludedProps, string formatToReturn)
        {
            var props = Properties.Where(x => excludedProps.Contains(x.Name) == false &&
                                        x.NotInDb == false).ToList();
            Func<string, string> propStringFormat = prop => string.Format(formatToReturn, prop);
            if (paramsCollection != null)
            {
                foreach (var property in props)
                {
                    var propertyToSet = GetPropertyValue(property, jObject);
                    paramsCollection.AddWithValue(string.Format("@{0}", property.Name), propertyToSet);
                }

            }
            return string.Join(",", props.Select(x => x.Name).Select(propStringFormat));
        }

        private static object GetPropertyValue(ReflectionPropertyInfo property, JObject jObject)
        {
            try
            {
                return GetValueFromJObjectValue(((JValue) jObject[property.Name]).Value);
            }
            catch (Exception)
            {
                return property.DefaultValue;
            }
        }

        private static object GetValueFromJObjectValue(object jObjectValue)
        {
            switch ((string)jObjectValue)
            {
                case "true":
                    return true;
                case "false":
                    return false;
                default:
                    return jObjectValue;
            }
        }
    }
}