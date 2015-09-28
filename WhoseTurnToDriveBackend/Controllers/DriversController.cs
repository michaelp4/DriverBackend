using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using WhoseTurnToDriveBackend.Models;
using WhoseTurnToDriveBackend.Utils;

namespace WhoseTurnToDriveBackend.Controllers
{
    public class DriversController : ApiController
    {
        private MySqlConnection _connection;

        public DriversController()
        {
            _connection =
                new MySqlConnection(SqlCommander.ConnectionString);

            _connection.Open();
        }
        public IEnumerable<Driver> Get()
        {
            var mySqlCommand = _connection.CreateCommand();
            mySqlCommand.CommandText = "select * from drivers";
            var adapter = new MySqlDataAdapter(mySqlCommand);
            var dataSet = new DataSet();
            adapter.Fill(dataSet);
            return dataSet.Tables[0].DefaultView.OfType<DataRowView>().Select(PropertiesGetter<Driver>.GetObjectByDataRow);
        }

        public Driver Get(int id)
        {
            return SqlCommander.GetDriverById(_connection, id);
        }

        public void Post([FromBody]JObject data)
        {
            var sqlCommand = _connection.CreateCommand();
            var endOfSqlQueryText = PropertiesGetter<Driver>.SetParametersFromObject(sqlCommand.Parameters, data,
                new[] { "Id" }, "@{0}");
            var columnsNames = PropertiesGetter<Driver>.SetParametersFromObject(null, null, new[] { "Id" }, "{0}");
            sqlCommand.CommandText = string.Format("insert into drivers ({0},id) values ({1},id=null)", columnsNames,
                endOfSqlQueryText);
            sqlCommand.ExecuteNonQuery();
        }

        public void Put(int id, [FromBody] JObject data)
        {
            var sqlCommand = _connection.CreateCommand();
            var endOfSqlQueryText = PropertiesGetter<Driver>.SetParametersFromObject(sqlCommand.Parameters, data,
                new[] { "Id" }, "{0}=@{0}");
            sqlCommand.CommandText = string.Format("update drivers set {0} where id = @id", endOfSqlQueryText);
            sqlCommand.Parameters.AddWithValue("@id", id);
            sqlCommand.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            var sqlCommandText = "delete from drivers where id=@id";
            SqlCommander.ExcecuteQuery(_connection, sqlCommandText, new KeyValuePair<string, object>("@id", id));
        }
    }
}
