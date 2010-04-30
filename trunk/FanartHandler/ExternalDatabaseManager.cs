//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using NLog;
    using SQLite.NET;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;   
    using System.Text;
    
    /// <summary>
    /// Class handling all external (not fanart handler db) database access.
    /// </summary>
    class ExternalDatabaseManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger(); 
        private SQLiteClient dbClient;

        /// <summary>
        /// Initiation of the DatabaseManager.
        /// </summary>
        /// <param name="dbFilename">Database filename</param>
        /// <returns>if database was successfully or not</returns>
        public bool InitDB(string dbFilename)
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
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
        public SQLiteResultSet GetData(string type)
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
