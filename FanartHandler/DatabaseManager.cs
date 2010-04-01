using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using SQLite.NET;
using MediaPortal.Music.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using MediaPortal.Configuration;
using NLog;
using System.IO;

namespace FanartHandler
{
    public class DatabaseManager
    {
        public bool stopScraper = false;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SQLiteClient dbClient;
        private string dbFilename = "FanartHandler.db3";
        private string dbFilenameOrg = "FanartHandler.org";
        public Hashtable htAnyGameFanart;
        public Hashtable htAnyMovieFanart;
        public Hashtable htAnyMovingPicturesFanart;
        public Hashtable htAnyMusicFanart;
        public Hashtable htAnyPictureFanart;
        public Hashtable htAnyScorecenter;
        public Hashtable htAnyTVSeries;
        public Hashtable htAnyTVFanart;
        public Hashtable htAnyPluginFanart;
        private ArrayList musicDatabaseArtists;
        private MusicDatabase m_db = null;
        private bool isScraping = false;
        private Scraper scraper;
        public int totArtistsBeingScraped = 0;
        public int currArtistsBeingScraped = 0;
        public bool isInitialized = false;        

        /// <summary>
        /// Returns if scraping is running or not.
        /// </summary>
        public bool GetIsScraping()
        {
            return isScraping;
        }
        

        /// <summary>
        /// Initiation of the DatabaseManager.
        /// </summary>
        public void initDB()
        {
            try
            {
                this.isScraping = false;
                String path = Config.GetFile(Config.Dir.Database, dbFilename);
                setupDatabase();                
                dbClient = new SQLiteClient(path);
                dbClient.Execute("PRAGMA synchronous=OFF");                
                m_db = MusicDatabase.Instance;
                logger.Info("Successfully Opened Database: " + dbFilename);
                CheckIfToUpgradeDatabase();
                UpgradeDbMain();
                isInitialized = true;
            }
            catch (Exception e)
            {
                logger.Error("initDB: Could Not Open Database: " + dbFilename + ". " + e.ToString());
                dbClient = null;
            }
        }

        /// <summary>
        /// Manages the database. If the database (.db3) is missing it creates a new based upon 
        /// the (.org). For first time installations or when a database has been deleted by an user. 
        /// </summary>
        private void setupDatabase()
        {
            try
            {                
                String path = Config.GetFile(Config.Dir.Database, dbFilename);
                String pathOrg = Config.GetFile(Config.Dir.Database, dbFilenameOrg);
                if (File.Exists(path))
                {
                    //do nothing
                }
                else
                {
                    File.Copy(pathOrg, path);
                }
            }
            catch (Exception ex)
            {
                logger.Error("setupDatabase: " + ex.ToString());
            }
        }

        /// <summary>
        /// Close the database clien.
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
                isInitialized = false;
            }
            catch (Exception ex)
            {
                logger.Error("close: " + ex.ToString());
            }
        }

        /// <summary>
        /// Performs a scrape for artist now being played in MediaPortal.
        /// </summary>
        public bool NowPlayingScrape(string artist, FanartHandler.FanartHandlerSetup.ScraperWorkerNowPlaying swnp)
        {
            try
            {
                isScraping = true;
                logger.Info("NowPlayingScrape is starting for artist " + artist + ".");
                totArtistsBeingScraped = 2;
                currArtistsBeingScraped = 0;
                currArtistsBeingScraped++;
                if (doScrape(artist, false, false, swnp) > 0)
                {
                    currArtistsBeingScraped++;
                    logger.Info("NowPlayingScrape is done.");
                    isScraping = false;
                    return true;
                }
                else
                {
                    logger.Info("NowPlayingScrape is done.");
                    isScraping = false;
                    return false;
                }    
            }
            catch (Exception ex)
            {
                isScraping = false;
                logger.Error("NowPlayingScrape: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get total artist count from MP music database
        /// </summary>
        public int GetTotalArtistsInMPMusicDatabase()
        {
            ArrayList al = new ArrayList();
            m_db.GetAllArtists(ref al);
            return al.Count;
        }

        /// <summary>
        /// Upgrade database script
        /// </summary>
        public void UpgradeDatabase()
        {
            logger.Info("Upgrading Database: " + dbFilename);
            string sqlQuery = "BEGIN TRANSACTION;";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Artist_Artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Source_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Type;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Game_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Movie_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE MovingPicture_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Music_Artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Music_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Picture_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Plugin_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Scorecenter_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE TVSeries_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE TV_Fanart;";
                result = dbClient.Execute(sqlQuery);            
            }
            catch { }
            try
            {
                sqlQuery = "DROP TABLE Version;";
                result = dbClient.Execute(sqlQuery);
            }
            catch { }  
            sqlQuery = "CREATE TABLE Game_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Movie_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE MovingPicture_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Music_Artist (Id INTEGER PRIMARY KEY, Artist TEXT, Successful_Scrape NUMERIC, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Music_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Picture_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Plugin_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Scorecenter_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE TVSeries_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE TV_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "INSERT INTO Version (Id, Version, Time_Stamp) VALUES (null,'1.2', '" + DateTime.Now.ToString(@"yyyyMMdd") + "');";
            result = dbClient.Execute(sqlQuery);   
            sqlQuery = "CREATE INDEX Idx_Music_Artist_Artist ON Music_Artist(Artist ASC);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Artist ON Music_Fanart(Artist ASC);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Disk_Image ON Music_Fanart(Disk_Image ASC);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Source_Image ON Music_Fanart(Source_Image ASC);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Type ON Music_Fanart(Type ASC);";
            result = dbClient.Execute(sqlQuery);
            sqlQuery = "COMMIT;";
            result = dbClient.Execute(sqlQuery);
            logger.Info("Upgrade of database is completed successfully.");
            logger.Info("Database version is verified: " + dbFilename);
        }

        /// <summary>
        /// Check if database is in current version or needs to be upgraded
        /// </summary>
        public void CheckIfToUpgradeDatabase()
        {
            try
            {
                string sqlQuery = "SELECT count(Artist) FROM Music_Fanart Where Enabled = 'True';";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                logger.Info("Database version is verified: " + dbFilename);
            }
            catch (SQLiteException sle)
            {
                string sErr = sle.ToString();
                if ((sErr != null && sErr.IndexOf("no such column: Enabled") >= 0) ||
                    (sErr != null && sErr.IndexOf("no column named Enabled") >= 0) ||
                    (sErr != null && sErr.IndexOf("no such table") >= 0) ||                      
                    (sErr != null && sErr.IndexOf("file is encrypted or is not a database query") >= 0))
                {
                    UpgradeDatabase();
                }                
            }
            catch (Exception ex)
            {
                logger.Error("UpgradeDatabase: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get total artists in Fanart Handler's database
        /// </summary>
        public int GetTotalArtistsInFanartDatabase()
        {
            string sqlQuery = "SELECT count(Artist) FROM Music_Artist;";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }

        /// <summary>
        /// Get total initilised artists in the Fanart Handler database
        /// </summary>
        public int GetTotalArtistsInitialisedInFanartDatabase()
        {
            string sqlQuery = "SELECT count(t1.Artist) FROM Music_Artist t1 WHERE t1.Artist in (SELECT distinct(t2.Artist) FROM Music_Fanart t2 WHERE t2.type = 'MusicFanart');";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }

        /// <summary>
        /// Get total movie images in the Fanart Handler database
        /// </summary>
        public int GetTotalMoviesInFanartDatabase()
        {
            string sqlQuery = "SELECT count(Artist) FROM Movie_Fanart;";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }

        /// <summary>
        /// Get total scorecenter images in the Fanart Handler database
        /// </summary>
        public int GetTotalScoreCenterInFanartDatabase()
        {
            string sqlQuery = "SELECT count(Artist) FROM ScoreCenter_Fanart;";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }

        /// <summary>
        /// Get total scorecenter images in the Fanart Handler database
        /// </summary>
        public int GetTotalRandomInFanartDatabase(string type)
        {
            string sqlQuery = "SELECT count(Artist) FROM " + getTableName(type) + ";";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }

        /// <summary>
        /// Get total uninitilised artists in the Fanart Handler database
        /// </summary>
        public int GetTotalArtistsUnInitialisedInFanartDatabase()
        {
            string sqlQuery = "SELECT count(t1.Artist) FROM Music_Artist t1 WHERE t1.Artist not in (SELECT distinct(t2.Artist) FROM Music_Fanart t2 WHERE t2.type = 'MusicFanart');";
            SQLiteResultSet result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0));
            }
            return i;
        }
        

        /// <summary>
        /// Return the current number of images an artist has.
        /// </summary>
        public int GetArtistCount(string artist, string dbArtist)
        {    
            try
            {
                int y = m_db.GetArtistId(artist);
                if (y > 0)
                {                   
                    string sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicFanart';";
                    SQLiteResultSet result = dbClient.Execute(sqlQuery);
                    int i = 0;
                    for (int x = 0; x < result.Rows.Count; x++)
                    {
                        i = Int32.Parse(result.GetField(x, 0));
                    }
                    return i;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetArtistCount: " + ex.ToString());
            }
            return 0;
        }

        /// <summary>
        /// Performs a scrape on the "new" pages on htbackdrops.com.
        /// </summary>
        public void doNewScrape()
        {
            if (stopScraper == false)
            {
                try
                {
                    isScraping = true;
                    scraper = new Scraper();                    
                    scraper.getNewImages(Convert.ToInt32(Utils.GetScraperMaxImages()), this);
                    scraper = null;
                    isScraping = false;
                }
                catch (Exception ex)
                {
                    isScraping = false;
                    logger.Error("doNewScrape: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Deletes any entries in the fanarthandler database when the disk_image
        /// is missing on the harddrive.
        /// </summary>
        public int syncDatabase(string type)
        {
            int i = 0;
            try
            {            
                string filename;
                string sqlQuery = "SELECT Disk_Image FROM " + getTableName(type) + ";";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                for (int x = 0; x < result.Rows.Count; x++)
                {
                    filename = result.GetField(x, 0);
                    if (File.Exists(filename) == false)
                    {
                        DeleteFanart(filename, "MusicFanart");
                        i++;
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("syncDatabase: " + ex.ToString());
            }
            return i;
        }



        /// <summary>
        /// Performs the scrape (now playing or intitial).
        /// </summary>
        public int doScrape (string artist, bool useSuccessfulScrape, bool useStopScraper, FanartHandler.FanartHandlerSetup.ScraperWorkerNowPlaying swnp)
        {
            if (stopScraper == false)
            {
                try
                {
                    string dbArtist = Utils.GetArtist(artist, "MusicFanart");
                    scraper = new Scraper();
                    string sqlQuery;
                    int totalImages = 0;
                    int iTmp = 0;
                    int successful_scrape = 0;
                    string tmp = "";
                    lock (dbClient) dbClient.Execute("BEGIN TRANSACTION;");
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        
                        InsertNewMusicArtist(dbArtist, "MusicFanart");
                        sqlQuery = "SELECT Successful_Scrape FROM Music_Artist WHERE Artist = '" + Utils.PatchSQL(dbArtist) + "';";
                        SQLiteResultSet result = dbClient.Execute(sqlQuery);
                        tmp = result.GetField(0, 0);
                        if (tmp != null && tmp.Length > 0)
                        {
                            successful_scrape = Int32.Parse(tmp);
                        }
                        else
                        {
                            successful_scrape = 0;
                        }
                        sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicFanart';";
                        result = dbClient.Execute(sqlQuery);
                        if (useSuccessfulScrape)
                        {

                            if (successful_scrape == 1)
                            {
                                //iTmp = 9999;
                                setSuccessfulScrape(dbArtist);
                                lock (dbClient) dbClient.Execute("COMMIT;");
                                scraper = null;
                                return 0;
                            }
                            else
                            {
                                iTmp = Int32.Parse(result.GetField(0, 0));
                            }
                        }
                        else
                        {
                            iTmp = Int32.Parse(result.GetField(0, 0));
                        }
                        int maxScrapes = Convert.ToInt32(Utils.GetScraperMaxImages()) - iTmp;
                        if (maxScrapes > 0)
                        {
                            totalImages = scraper.getImages(artist, maxScrapes, this, swnp);
                            if (totalImages == 0)
                            {
                                logger.Debug("No fanart found for artist " + artist + ".");
                            }
                        }
                        else
                        {
                            logger.Debug("Artist " + artist + " has already maximum number of images. Will not download anymore images for this artist.");
                        }
                        setSuccessfulScrape(dbArtist);                        
                        scraper = null;                        
                    }
                    lock (dbClient) dbClient.Execute("COMMIT;");
                    return totalImages;
                }
                catch (Exception ex)
                {
                    lock (dbClient) dbClient.Execute("ROLLBACK;");
                    logger.Error("doScrape: " + ex.ToString());
                }
            }
           
            return 0;
        }

        /// <summary>
        /// Performs the intitial scrape (on htbackdrops.com) for any artist in the MP music
        /// database until max images per artist is meet or no more images exist for the artist.
        /// </summary>        
        public void InitialScrape(FanartHandler.FanartHandlerSetup.ScraperWorker sw)
        {
            try
            {
                logger.Info("InitialScrape is starting...");
                bool firstRun = true;
                isScraping = true;                
                musicDatabaseArtists = new ArrayList();
                m_db.GetAllArtists(ref musicDatabaseArtists);                
                string artist;
                totArtistsBeingScraped = musicDatabaseArtists.Count;
                if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
                {
                    for (int i = 0; i < musicDatabaseArtists.Count; i++)
                    {
                        artist = musicDatabaseArtists[i].ToString();
                        if (stopScraper == true)
                        {                            
                            break;
                        }
                        if (doScrape(artist, true, true, null) > 0 && firstRun)
                        {
                            addScapedFanartToAnyHash();
                            if (sw != null)
                            {
                                sw.setRefreshFlag(true);
                                firstRun = false;
                            }                            
                        }
                        currArtistsBeingScraped++;
                    }
                }
                logger.Info("InitialScrape is done.");
                isScraping = false;
                musicDatabaseArtists = null;
                addScapedFanartToAnyHash();                
            }
            catch (Exception ex)
            {
                isScraping = false;
                logger.Error("InitialScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Refreshes the music "any" fanart if no images at all was available upon 
        /// MP start.
        /// </summary>
        private void addScapedFanartToAnyHash()
        {
            if (htAnyMusicFanart == null || htAnyMusicFanart.Count < 1)
            {
                Hashtable htTmp = new Hashtable();
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Enabled = 'True' AND Type IN ('MusicFanart');";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                    htTmp.Add(i, fi);
                }
                result = null;
                sqlQuery = null;
                htAnyMusicFanart = htTmp;              
            }
        }


        /// <summary>
        /// Upgrade db to version 1.0
        /// </summary>
        public void UpgradeDbMain()
        {
            DateTime saveNow = DateTime.Now;
            try
            {
                string sqlQuery = "SELECT count(Version) FROM Version;";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);                
            }
            catch (SQLiteException sle)
            {                
                string sErr = sle.ToString();
                if (sErr != null && sErr.IndexOf("no such table: Version") >= 0)
                {
                    string sqlQuery = "BEGIN TRANSACTION;";
                    SQLiteResultSet result = dbClient.Execute(sqlQuery);
                    sqlQuery = "CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);";
                    result = dbClient.Execute(sqlQuery);
                    sqlQuery = "COMMIT;";
                    result = dbClient.Execute(sqlQuery);
                    sqlQuery = "INSERT INTO Version (Id, Version, Time_Stamp) VALUES (null,'1.2', '" + saveNow.ToString(@"yyyyMMdd") + "');";
                    result = dbClient.Execute(sqlQuery);                    
                }
            }
            catch
            {
                //do nothing
            }            
        }

        /// <summary>
        /// Deletes all fanart in the database and resets the initial flag.
        /// </summary>
        public void DeleteAllFanart(string type)
        {
            try
            {
                string sqlQuery = sqlQuery = "DELETE FROM " + getTableName(type) + " WHERE Type = '" + Utils.PatchSQL(type) + "';";
                lock (dbClient) dbClient.Execute(sqlQuery);
                if (type.StartsWith("MusicFanart"))
                {
                    DateTime saveNow = DateTime.Now;
                    sqlQuery = "UPDATE Music_Artist SET Successful_Scrape = 0, Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd") + "';";                
                    lock (dbClient) dbClient.Execute(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteAllFanart: " + ex.ToString());
            }
        }

        /// <summary>
        /// Resets the initial flag. To prepare the database for a complete new intitial scrape.
        /// </summary>
        public void ResetInitialScrape()
        {     
            try
            {
                string sqlQuery = "UPDATE Music_Artist SET Successful_Scrape = 0;";
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("ResetInitialScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        public void EnableFanartMusic(string disk_image, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE Music_Fanart SET Enabled = 'True' WHERE Disk_Image = '"+Utils.PatchSQL(disk_image)+"';";                
                }
                else
                {
                    sqlQuery = "UPDATE Music_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartMusic: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        public void EnableFanartMovie(string disk_image, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE Movie_Fanart SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE Movie_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartMovie: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        public void EnableFanartScoreCenter(string disk_image, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE ScoreCenter_Fanart SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE ScoreCenter_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartScoreCenter: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        public void EnableFanartRandom(string disk_image, bool action, string type)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE " + getTableName(type) + " SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE " + getTableName(type) + " SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartRandom: " + ex.ToString());
            }
        }

        /// <summary>
        /// Delete a specific image from the database.
        /// </summary>
        public void DeleteFanart(string disk_image, string type)
        {
            try
            {
                //delete music fanart
                string sqlQuery = "DELETE FROM " + getTableName(type) + " WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                lock (dbClient) dbClient.Execute(sqlQuery);            
            }
            catch (Exception ex)
            {
                logger.Error("DeleteFanart: " + ex.ToString());
            }
        }
   

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Music (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTable(int lastID)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Music_Fanart WHERE Id > " + lastID + " AND Type = 'MusicFanart' order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTable: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Scorecenter (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTableMovie(int lastID)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Movie_Fanart WHERE Id > " + lastID + " order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTableMovie: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Music Fanart Overview (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTableMusicOverview()
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "select t1.artist, count(t2.artist) from music_artist t1 LEFT OUTER JOIN music_fanart t2  ON t1.artist = t2.artist group by t1.artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTableMusicOverview: " + ex.ToString());
            }
            return result;
        }
        

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Movies (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTableScoreCenter(int lastID)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM ScoreCenter_Fanart WHERE Id > " + lastID + " order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForScoreCenter: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Random Fanart (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTableRandom(int lastID, string type)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM " + getTableName(type) + " WHERE Id > " + lastID + " order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTableRandom: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// Returns all images for an artist.
        /// </summary>
        public Hashtable getFanart(string artist, string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + getTableName(type) + " WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True';";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                    ht.Add(i, fi);
                }
                result = null;
                sqlQuery = null;
            }
            catch (Exception ex)
            {
                logger.Error("getFanart: " + ex.ToString());
            }
            return ht;
        }

        /// <summary>
        /// Returns all images for an artist.
        /// </summary>
        public Hashtable getHigResFanart(string artist, string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True' AND Type = 'MusicFanart';";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                    ht.Add(i, fi);
                }
                result = null;
                sqlQuery = null;
            }
            catch (Exception ex)
            {
                logger.Error("getHigResFanart: " + ex.ToString());
            }
            return ht;
        }


        /// <summary>
        /// Returns table names for use in sql statements.
        /// </summary>
        private string getTableName(string type)
        {
            if (type.Equals("Game"))
                return "Game_Fanart";
            else if (type.Equals("Movie"))
                return "Movie_Fanart";
            else if (type.Equals("myVideos"))
                return "Movie_Fanart";
            else if (type.Equals("MusicAlbum"))
                return "Music_Fanart";
            else if (type.Equals("MusicArtist"))
                return "Music_Fanart";
            else if (type.Equals("MusicFanart"))
                return "Music_Fanart";
            else if (type.Equals("Default"))
                return "Music_Fanart";
            else if (type.Equals("Music Playlist"))
                return "Music_Fanart";
            else if (type.Equals("Youtube.FM"))
                return "Music_Fanart";
            else if (type.Equals("Music Videos"))
                return "Music_Fanart";
            else if (type.Equals("mVids"))
                return "Music_Fanart";
            else if (type.Equals("Global Search"))
                return "Music_Fanart";
            else if (type.Equals("Picture"))
                return "Picture_Fanart";
            else if (type.Equals("ScoreCenter"))
                return "Scorecenter_Fanart";
            else if (type.Equals("MovingPicture"))
                return "MovingPicture_Fanart";
            else if (type.Equals("TVSeries"))
                return "TVSeries_Fanart";
            else if (type.Equals("TV"))
                return "TV_Fanart";
            else if (type.Equals("Plugin"))
                return "Plugin_Fanart";
            else 
                return "";
        }

        /// <summary>
        /// Returns a hashtable.
        /// </summary>
        private Hashtable getAnyHashtable(string type)
        {
            if (type.Equals("Game"))            
                return htAnyGameFanart;
            else if (type.Equals("Movie"))
                return htAnyMovieFanart;
            else if (type.Equals("MusicAlbum"))
                return htAnyMusicFanart;
            else if (type.Equals("MusicArtist"))
                return htAnyMusicFanart;
            else if (type.Equals("MusicFanart"))
                return htAnyMusicFanart;
            else if (type.Equals("Default"))
                return htAnyMusicFanart;
            else if (type.Equals("Picture"))
                return htAnyPictureFanart;
            else if (type.Equals("ScoreCenter"))
                return htAnyScorecenter;
            else if (type.Equals("MovingPicture"))
                return htAnyMovingPicturesFanart;
            else if (type.Equals("TVSeries"))
                return htAnyTVSeries;
            else if (type.Equals("TV"))
                return htAnyTVFanart;
            else if (type.Equals("Plugin"))
                return htAnyPluginFanart;
            else
                return null;            
        }

        /// <summary>
        /// Adds to a hashtable.
        /// </summary>
        private void addToAnyHashtable(string type, Hashtable ht)
        {
            if (type.Equals("Game"))           
                htAnyGameFanart = ht;
            else if (type.Equals("Movie"))
                htAnyMovieFanart = ht;
            else if (type.Equals("MusicAlbum"))
                htAnyMusicFanart = ht;
            else if (type.Equals("MusicArtist"))
                htAnyMusicFanart = ht;
            else if (type.Equals("MusicFanart"))
                htAnyMusicFanart = ht;
            else if (type.Equals("Default"))
                htAnyMusicFanart = ht;
            else if (type.Equals("Picture"))
                htAnyPictureFanart = ht;
            else if (type.Equals("ScoreCenter"))
                htAnyScorecenter = ht;                
            else if (type.Equals("MovingPicture"))
                htAnyMovingPicturesFanart = ht;
            else if (type.Equals("TVSeries"))
                htAnyTVSeries = ht;
            else if (type.Equals("TV"))
                htAnyTVFanart = ht;
            else if (type.Equals("Plugin"))
                htAnyPluginFanart = ht;            
        }

        /// <summary>
        /// Returns all random fanart for a specific type (like music or movies). First time builds hashtable, 
        /// then only returns that hashtable.
        /// </summary>
        public Hashtable getAnyFanart(string type, string types)
        {
            Hashtable ht = getAnyHashtable(type);
            try
            {
                if (ht != null)
                {
                    return ht;
                }
                else
                {
                    ht = new Hashtable();
                    string sqlQuery;
                    if (types != null && types.Length > 0)
                    {
                        sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + getTableName(type) + " WHERE Enabled = 'True' AND Type IN (" + types + ");";
                    }
                    else
                    {
                        sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + getTableName(type) + " WHERE Enabled = 'True';";
                    }                    
                    SQLiteResultSet result = dbClient.Execute(sqlQuery);
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                        ht.Add(i, fi);
                    }
                    result = null;
                    addToAnyHashtable(type, ht);
                    return ht;
                }   
            }
            catch (Exception ex)
            {
                logger.Error("getAnyFanart: " + ex.ToString());
                return ht;
            }
        }

        /// <summary>
        /// Inserts new fanart into the database.
        /// </summary>
        public void loadFanart(string artist, string disk_image, string source_image, string type)
        {
            try
            {
                string sqlQuery = "";
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM " + getTableName(type) + " WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSQL(source_image) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do not allow updates
                }
                else
                {
                    sqlQuery = "INSERT INTO " + getTableName(type) + " (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "','" + Utils.PatchSQL(disk_image) + "','" + Utils.PatchSQL(source_image) + "','" + Utils.PatchSQL(type) + "','www.htbackdrops.com', 'True', '" + saveNow.ToString(@"yyyyMMdd") + "');";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing local fanart into fanart handler database (" + disk_image + ").");
                }                                            
            }
            catch (Exception ex)
            {
                logger.Error("loadFanart: " + ex.ToString());
            }
        }

        /// <summary>
        /// Returns if an image exist in the database or not.
        /// </summary>
        public bool SourceImageExist(string artist, string source_image, string type)
        {
            try
            {
                string sqlQuery = "";
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSQL(source_image) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {                
                    return true;
                }
                else
                {
                    return false;   
                }
            }
            catch (Exception ex)
            {
                logger.Error("SourceImageExist: " + ex.ToString());
                return true;
            }
        }

        /// <summary>
        /// Inserts music fanart into the database. If artist is missing the artist is added also.
        /// </summary>
        public void loadMusicFanart(string artist, string disk_image, string source_image, string type)
        {
            try
            {
                string sqlQuery = "";
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND (SOURCE_IMAGE = '" + Utils.PatchSQL(source_image) + "' OR DISK_IMAGE = '" + Utils.PatchSQL(disk_image) + "');";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do not allow updates
                }
                else
                {
                    sqlQuery = "INSERT INTO Music_Fanart (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "','" + Utils.PatchSQL(disk_image) + "','" + Utils.PatchSQL(source_image) + "','" + Utils.PatchSQL(type) + "','www.htbackdrops.com','True','" + saveNow.ToString(@"yyyyMMdd") + "');";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing local fanart into fanart handler database (" + disk_image + ").");
                    if (type.Equals("MusicFanart"))
                    {
                        sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSQL(artist) + "';";
                        if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                        {
                            //DO NOTHING
                        }
                        else
                        {
                            sqlQuery = "INSERT INTO Music_Artist (Id, Artist, Successful_Scrape, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "',0,'" + saveNow.ToString(@"yyyyMMdd") + "');";
                        }
                        lock (dbClient) dbClient.Execute(sqlQuery);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("loadMusicFanart: " + ex.ToString());
            }
        }

        /// <summary>
        /// Inserts a new artist into the database.
        /// </summary>
        public void InsertNewMusicArtist(string artist, string type)
        {
            try
            {
                string sqlQuery = "";
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSQL(artist) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do nothing
                }
                else
                {
                    sqlQuery = "INSERT INTO Music_Artist (Id, Artist, Successful_Scrape, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "',0,'" + saveNow.ToString(@"yyyyMMdd") + "');";
                }
                lock (dbClient) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("InsertNewMusicArtist: " + ex.ToString());
            }
        }

        /// <summary>
        /// Flags an artist as being done with the initial scrape.
        /// </summary>
        public void setSuccessfulScrape(string artist)
        {
            try
            {
                string sqlQuery = "";
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSQL(artist) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "UPDATE Music_Artist SET Successful_Scrape = 1, Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd") + "' WHERE Artist = '" + Utils.PatchSQL(artist) + "';";
                    lock (dbClient) dbClient.Execute(sqlQuery);                
                }
                else
                {
                    //do not allow insert                
                }
            }
            catch (Exception ex)
            {
                logger.Error("setSuccessfulScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Container for fanart data.
        /// </summary>
        public class FanartImage
        {
            public string id;
            public string artist;
            public string disk_image;
            public string source_image;
            public string type;
            public string source;

            public FanartImage(string id, string artist, string disk_image, string source_image, string type, string source)
            {
                this.id = id;
                this.artist = artist;
                this.disk_image = disk_image;
                this.source_image = source_image;
                this.type = type;
                this.source = source;
            }
        }
    }
}
