using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using WhoseTurnToDriveBackend.Models;

namespace WhoseTurnToDriveBackend.Utils
{
    public static class SqlCommander
    {
        public readonly static string ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
            //"Server=tcp:hvt4f1he8g.database.windows.net,1433;Database=WhoseTurnToDrive;User ID=mm19924@hvt4f1he8g;Password=Michaelp123;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
        //old - Server=localhost;Database=whoseturntodrive;uid=root;password=michaelp123
        public static void ExcecuteQuery(MySqlConnection connection, string commandText, params KeyValuePair<string,object>[] parameters)
        {
            var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = commandText;
            foreach (var parameter in parameters)
            {
                sqlCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
            sqlCommand.ExecuteNonQuery();
        }

        public static Driver GetDriverById(MySqlConnection connection, int id)
        {
            var mySqlCommand = connection.CreateCommand();
            mySqlCommand.CommandText = "select * from drivers where id=@id";
            mySqlCommand.Parameters.AddWithValue("@id", id);
            var adapter = new MySqlDataAdapter(mySqlCommand);
            var dataSet = new DataSet();
            adapter.Fill(dataSet);
            var result = dataSet.Tables[0].DefaultView.OfType<DataRowView>().Select(PropertiesGetter<Driver>.GetObjectByDataRow).SingleOrDefault();
            if (result == null)
            {
                throw new InvalidDataException("there are probably more than one " + id);
            }
            return result;
        }
    }
}