//***********************************************************************
// Assembly         : FanartHandler
// Author           : cul8er
// Created          : 05-09-2010
//
// Last Modified By : cul8er
// Last Modified On : 10-05-2010
// Description      : 
//
// Copyright        : Open Source software licensed under the GNU/GPL agreement.
//***********************************************************************

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using NLog;    
    using SQLite.NET;
    using System;
    using System.Collections.Generic;
    using System.Collections;
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
                    FileInfo f = new FileInfo(path);
                    if (f.Length > 0)
                    {
                        dbClient = new SQLiteClient(path);
                        dbClient.Execute("PRAGMA synchronous=OFF");
                        return true;
                    }
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
        public void Close()
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
                if (type.Equals("TVSeries", StringComparison.CurrentCulture))
                {
                    sqlQuery = "select SortName, id from online_series;";
                }
                else
                {
                    sqlQuery = "select artistName from Artists;";
                }
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }
            return result;
        }

        
        /// <summary>
        /// Returns latest added movie thumbs from MovingPictures db.
        /// </summary>
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
       public FanartHandler.LatestsCollection GetLatestPictures()
        {
            FanartHandler.LatestsCollection result = new FanartHandler.LatestsCollection();
            string sqlQuery = null;
            int x = 0;
            try
            {
                sqlQuery = "select strFile, strDateTaken from picture where strFile not like '%kindgirls%' order by strDateTaken desc limit 10;";
                SQLiteResultSet resultSet = dbClient.Execute(sqlQuery);
                if (resultSet != null)
                {
                    if (resultSet.Rows.Count > 0)
                    {                        
                        for (int i = 0; i < resultSet.Rows.Count; i++)
                        {                            
                            string thumb = resultSet.GetField(i, 0);
                            string dateAdded = resultSet.GetField(i, 1);
                            string title = Utils.GetFilenameNoPath(thumb).ToUpperInvariant();
                            if (thumb != null && thumb.Trim().Length > 0)
                            {
                                if (File.Exists(thumb))
                                {
                                    result.Add(new FanartHandler.Latest(dateAdded, thumb, null, title, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null));
                                    x++;
                                }
                            }
                            if (x == 3)
                            {
                                break;
                            }
                        }
                    }
                }
                resultSet = null;
            }
            catch //(Exception ex)
            {
                //logger.Error("getData: " + ex.ToString());
            }
            return result;
        }        

    }
}
