using System;
using System.Configuration;
using System.IO;
using Microsoft.Data.Sqlite;

namespace lib
{
    public class SqliteHelper
    {
        //
        // Get DB Connection String
        //
        static public SqliteConnection DBContext(String dbName)
        {
            string dir = ConfigurationManager.AppSettings["dir"]; 

            //If folder not exist, create it.
            if(!Directory.Exists(dir)){
                Directory.CreateDirectory(dir);
            }

            var connectionStringBuilder = new SqliteConnectionStringBuilder();

            //Use DB in project directory.  If it does not exist, create it
            connectionStringBuilder.DataSource = dir + "/" + dbName;
            var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

            return connection;
        }
    }   
}