using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SQLite.NET;
using MediaPortal.Configuration;
using NLog;


namespace FanartHandler
{
    class ExternalDatabaseManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SQLiteClient dbClient;

        /// <summary>
        /// Initiation of the DatabaseManager.
        /// </summary>
        public bool initDB(string dbFilename)
        {
            try
            {
                String path = Config.GetFolder(Config.Dir.Database) + @"\"+ dbFilename;
                if (File.Exists(path))
                {
                    dbClient = new SQLiteClient(path);
                    dbClient.Execute("PRAGMA synchronous=OFF");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch //(Exception e)
            {
                //logger.Error("initDB: Could Not Open Database: " + dbFilename + ". " + e.ToString());
                dbClient = null;
            }
            return false;
        }

        /// <summary>
        /// Close the database client.
        /// </summary>
        public void close()
        {
            try
            {
                if (dbClient != null)
                {
                    dbClient.Close();
                }
                dbClient = null;
            }
            catch (Exception ex)
            {
                logger.Error("close: " + ex.ToString());
            }
        }


        /// <summary>
        /// Returns movie fanart data from Moving Picture or TVSeries db.
        /// </summary>
        public SQLiteResultSet getData(string type)
        {
            SQLiteResultSet result = null;
            string sqlQuery = null;
            try
            {
                if (type.Equals("MovingPicture"))
                {
                    sqlQuery = "select title, backdropfullpath from movie_info;";
                }
                else if (type.Equals("TVSeries"))
                {
                    sqlQuery = "select SortName, fanart from online_series;";
                }
                else
                {
                    sqlQuery = "select artistName from Artists;";
                }
                result = dbClient.Execute(sqlQuery);
            }
            catch //(Exception ex)
            {
                //logger.Error("getData: " + ex.ToString());
            }
            return result;
        }

    }
}
