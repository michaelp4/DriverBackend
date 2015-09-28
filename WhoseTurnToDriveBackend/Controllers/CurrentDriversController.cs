using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using WebGrease;
using WhoseTurnToDriveBackend.Models;
using WhoseTurnToDriveBackend.Utils;

namespace WhoseTurnToDriveBackend.Controllers
{
    public class CurrentDriversController : ApiController
    {
        private MySqlConnection _connection;

        public CurrentDriversController()
        {
            _connection =
                new MySqlConnection(SqlCommander.ConnectionString);


            _connection.Open();
        }
        // GET api/values
        public IEnumerable<CurrentDriver> Get(bool isExpired)
        {
            var sqlCommand = _connection.CreateCommand();
            sqlCommand.CommandText = "select * from drivershistory where isExpired = @isExpired";
            sqlCommand.Parameters.AddWithValue("@isExpired", isExpired);
            var adapter = new MySqlDataAdapter(sqlCommand);
            var dataSet = new DataSet();
            adapter.Fill(dataSet);
            return dataSet.Tables[0].DefaultView.OfType<DataRowView>().Select(PropertiesGetter<CurrentDriver>.GetObjectByDataRow);
        }

        public void Post([FromBody] JObject data)
        {
            var commandText = "insert into drivershistory (driverId,isDriving,dateTime, isExpired,destination) values (@id,false,null,false,null)";
            SqlCommander.ExcecuteQuery(_connection, commandText, new KeyValuePair<string, object>("@id", data["Id"]));
        }
        /// <summary>
        /// Change the is driving status of the driver.
        /// </summary>
        /// <param name="driverId"></param>
        public void Put(int driverId)
        {
            var commandText = "update driversHistory set isDriving = !isDriving where isexpired = false and driverId = @id";
            SqlCommander.ExcecuteQuery(_connection, commandText, new KeyValuePair<string, object>("@id", driverId));
        }

        public void Delete(int driverId)
        {
            var commadText = "delete from drivershistory where isexpired = false and driverid = @id";
            SqlCommander.ExcecuteQuery(_connection, commadText, new KeyValuePair<string, object>("@id", driverId));
        }
        public void Delete(string destination)
        {
            var currentDrivers = Get(false);
            foreach (var currentDriver in currentDrivers)
            {
                IncreadDriverFieldByOne(currentDriver.DriverId,
                    currentDriver.IsDriving ? "timesBeenADriver" : "timesBeenAPassenger");
            }
            var commandText = "update drivershistory set isExpired = true,destination = @destination, dateTime = @currentTime where isexpired = false";
            SqlCommander.ExcecuteQuery(_connection, commandText,
                new KeyValuePair<string, object>("@currentTime", DateTime.Now),
                new KeyValuePair<string, object>("@destination", destination));
        }

        public void Get(string fuck)
        {
            var currentDrivers = Get(false).ToList();
            var numberOfDrivers = (int)Math.Ceiling((double)currentDrivers.Count() / 5);
            var drivers =
                currentDrivers.Select(currentDriver => SqlCommander.GetDriverById(_connection, currentDriver.DriverId)).Where(x => x.CanDrive && x.HasCar);
            var driversScoresAndIds =
                drivers.Select(d => new { Score = d.TimesBeenAPassenger == 0 ? (double)d.TimesBeenADriver + 1 : (double)d.TimesBeenADriver / d.TimesBeenAPassenger, d.Id })
                    .OrderBy(x => x.Score);
            var selectedDriversIds = driversScoresAndIds.Take(numberOfDrivers).Select(x => x.Id);
            var commandText = "update driversHistory set isDriving = false where isExpired = false";
            SqlCommander.ExcecuteQuery(_connection, commandText);
            foreach (var selectedDriverId in selectedDriversIds)
            {
                Put(selectedDriverId);
            }
        }
        private void IncreadDriverFieldByOne(int driverId, string field)
        {
            var commadText = string.Format("update drivers set {0} = {0} + 1 where id = @id", field);
            SqlCommander.ExcecuteQuery(_connection, commadText, new KeyValuePair<string, object>("@id", driverId));
        }
    }
}
