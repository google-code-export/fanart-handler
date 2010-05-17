//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.Database;
    using MediaPortal.GUI.Library;
    using MediaPortal.Music.Database;
    using NLog;
    using SQLite.NET;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;    

    /// <summary>
    /// Class handling all database access.
    /// </summary>
    public class DatabaseManager
    {
        #region declarations
        private bool stopScraper = false;        
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SQLiteClient dbClient;
        private string dbFilename = "FanartHandler.db3";
        private string dbFilenameOrg = "FanartHandler.org";
        private Hashtable htAnyGameFanart;
        private Hashtable htAnyMovieFanart;
        private Hashtable htAnyMovingPicturesFanart;
        private Hashtable htAnyMusicFanart;
        private Hashtable htAnyPictureFanart;
        private Hashtable htAnyScorecenter;
        private Hashtable htAnyTVSeries;        
        private Hashtable htAnyTVFanart;
        private Hashtable htAnyPluginFanart;        
        private ArrayList musicDatabaseArtists;
        private MusicDatabase m_db = null;
        private bool isScraping = false;        
        private Scraper scraper;
        private double totArtistsBeingScraped = 0;
        private double currArtistsBeingScraped = 0;
        private bool isInitialized = false;        
        #endregion

        public bool IsScraping
        {
            get { return isScraping; }
            set { isScraping = value; }
        }

        public bool IsInitialized
        {
            get { return isInitialized; }
            set { isInitialized = value; }
        }

        public double CurrArtistsBeingScraped
        {
            get { return currArtistsBeingScraped; }
            set { currArtistsBeingScraped = value; }
        }

        public double TotArtistsBeingScraped
        {
            get { return totArtistsBeingScraped; }
            set { totArtistsBeingScraped = value; }
        }

        public Hashtable HtAnyPluginFanart
        {
            get { return htAnyPluginFanart; }
            set { htAnyPluginFanart = value; }
        }

        public Hashtable HtAnyTVSeries
        {
            get { return htAnyTVSeries; }
            set { htAnyTVSeries = value; }
        }

        public Hashtable HtAnyTVFanart
        {
            get { return htAnyTVFanart; }
            set { htAnyTVFanart = value; }
        }

        public Hashtable HtAnyScorecenter
        {
            get { return htAnyScorecenter; }
            set { htAnyScorecenter = value; }
        }

        public Hashtable HtAnyPictureFanart
        {
            get { return htAnyPictureFanart; }
            set { htAnyPictureFanart = value; }
        }

        public Hashtable HtAnyMusicFanart
        {
            get { return htAnyMusicFanart; }
            set { htAnyMusicFanart = value; }
        }

        public Hashtable HtAnyMovingPicturesFanart
        {
            get { return htAnyMovingPicturesFanart; }
            set { htAnyMovingPicturesFanart = value; }
        }

        public Hashtable HtAnyMovieFanart
        {
            get { return htAnyMovieFanart; }
            set { htAnyMovieFanart = value; }
        }

        public Hashtable HtAnyGameFanart
        {
            get { return htAnyGameFanart; }
            set { htAnyGameFanart = value; }
        }

        public bool StopScraper
        {
            get { return stopScraper; }
            set { stopScraper = value; }
        }
        /// <summary>
        /// Returns if scraping is running or not.
        /// </summary>
        /// <returns>True if scraping is running</returns>
        public bool GetIsScraping()
        {
            return IsScraping;
        }
        
        /// <summary>
        /// Initiation of the DatabaseManager.
        /// </summary>
        public void InitDB()
        {
            try
            {                
                this.IsScraping = false;
                String path = Config.GetFile(Config.Dir.Database, dbFilename);
                SetupDatabase();                
                dbClient = new SQLiteClient(path);
                dbClient.Execute("PRAGMA synchronous=OFF");                
                m_db = MusicDatabase.Instance;
                logger.Info("Successfully Opened Database: " + dbFilename);
                CheckIfToUpgradeDatabase();
                UpgradeDbMain();
                IsInitialized = true;
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
        private void SetupDatabase()
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
        public void Close()
        {
            try
            {
                if (dbClient != null)
                {
                    dbClient.Close();
                }

                dbClient = null;
                IsInitialized = false;
            }
            catch (Exception ex)
            {
                logger.Error("close: " + ex.ToString());
            }
        }

        /// <summary>
        /// Performs a scrape for artist now being played in MediaPortal.
        /// </summary>
        /// <param name="artist">Name of the artist</param>
        /// <param name="swnp">ScraperWorkerNowPlaying object</param>
        /// <returns>True if scraping has occured successfully</returns>
        public bool NowPlayingScrape(string artist, FanartHandler.ScraperNowWorker swnp)
        {
            try
            {
                logger.Info("NowPlayingScrape is starting for artist " + artist + ".");
                TotArtistsBeingScraped = 2;
                CurrArtistsBeingScraped = 0;
                if (DoScrape(artist, false, false) > 0)
                {
                    logger.Info("NowPlayingScrape is done.");
                    return true;
                }
                else
                {
                    logger.Info("NowPlayingScrape is done.");
                    return false;
                }    
            }
            catch (Exception ex)
            {
                logger.Error("NowPlayingScrape: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Get total artist count from MP music database
        /// </summary>
        /// <returns>Number of artists in MPs music database</returns>
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
            catch 
            { 
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Source_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Type;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Game_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Movie_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE MovingPicture_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Music_Artist;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Music_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Picture_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Plugin_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Scorecenter_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE TVSeries_Fanart;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE TV_Fanart;";
                result = dbClient.Execute(sqlQuery);            
            }
            catch 
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Version;";
                result = dbClient.Execute(sqlQuery);
            }
            catch 
            {
            }  

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
        /// <returns>Total number of artists in fanart handler database</returns>
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
        /// <returns>Total number of initialized artists in the fanart handler database</returns>
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
        /// <returns>Total number of movies in the fanart handler database</returns>
        public int GetTotalMoviesInFanartDatabase()
        {
            string sqlQuery = "SELECT count(Artist) FROM Movie_Fanart where type = 'Movie';";
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
        /// <returns>Total number of score center fanart in the fanart handler database</returns>
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
        /// <param name="type">The type to be returned</param>
        /// <returns>Total number of random fanart in the fanart handler database</returns>
        public int GetTotalRandomInFanartDatabase(string type)
        {
            string sqlQuery = "SELECT count(Artist) FROM " + GetTableName(type) + ";";
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
        /// <returns>Return total uninitilised artists in the Fanart Handler database</returns>
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
        /// <param name="artist">The artist name</param>
        /// <param name="dbArtist">The db artist name</param>
        /// <returns>Get total number of images an artist has.</returns>
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
        public void DoNewScrape()
        {
            if (StopScraper == false)
            {
                try
                {
                    scraper = new Scraper();                    
                    scraper.GetNewImages(Convert.ToInt32(Utils.GetScraperMaxImages()), this);
                    scraper = null;
                }
                catch (Exception ex)
                {
                    logger.Error("doNewScrape: " + ex.ToString());
                }
            }
        }      

        /// <summary>
        /// /// Deletes any entries in the fanarthandler database when the disk_image
        /// is missing on the harddrive.
        /// </summary>
        /// <param name="type">The type fo what is to be returned</param>
        /// <returns>Number of entries that was deleted</returns>
        public int SyncDatabase(string type)
        {
            int i = 0;
            try
            {            
                string filename;
                string sqlQuery = "SELECT Disk_Image FROM " + GetTableName(type) + ";";
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
        /// <param name="artist">Artist name</param>
        /// <param name="useSuccessfulScrape">Use the successfuls scrape flag in db or not</param>
        /// <param name="useStopScraper">Use the stop scraper parameter or not</param>
        /// <param name="swnp">ScraperWorkerNowPlaying object</param>
        /// <returns>Number of scraped images</returns>
        public int DoScrape(string artist, bool useSuccessfulScrape, bool useStopScraper)
        {
            if (StopScraper == false)
            {
                try
                {
                    string dbArtist = Utils.GetArtist(artist, "MusicFanart");
                    scraper = new Scraper();
                    string sqlQuery;
                    int totalImages = 0;
                    int iTmp = 0;
                    int successful_scrape = 0;
                    string tmp = String.Empty;
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
                                SetSuccessfulScrape(dbArtist);
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
                            totalImages = scraper.GetImages(artist, maxScrapes, this, useSuccessfulScrape);
                            if (totalImages == 0)
                            {
                                logger.Debug("No fanart found for artist " + artist + ".");
                            }
                        }
                        else
                        {
                            logger.Debug("Artist " + artist + " has already maximum number of images. Will not download anymore images for this artist.");
                        }

                        if (totalImages != 99)
                        {
                            SetSuccessfulScrape(dbArtist);                           
                        }      
                  
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
        /// /// Performs the intitial scrape (on htbackdrops.com) for any artist in the MP music
        /// database until max images per artist is meet or no more images exist for the artist.
        /// </summary>
        /// <param name="sw">ScraperWorker object</param>
//        public void InitialScrape(FanartHandler.FanartHandlerSetup.ScraperWorker sw)
        public void InitialScrape()
        {
            try
            {
                logger.Info("InitialScrape is starting...");
                bool firstRun = true;
                musicDatabaseArtists = new ArrayList();
                m_db.GetAllArtists(ref musicDatabaseArtists);  
                ArrayList al = Utils.GetMusicVideoArtists("MusicVids.db3");
                if (al != null && al.Count > 0)
                {
                    musicDatabaseArtists.AddRange(al);
                }

                string artist;
                if (FanartHandlerSetup.MyScraperWorker != null)
                {
                    FanartHandlerSetup.MyScraperWorker.ReportProgress(0, "Start");
                }
                
                TotArtistsBeingScraped = musicDatabaseArtists.Count;
                if (musicDatabaseArtists != null && musicDatabaseArtists.Count > 0)
                {
                    for (int i = 0; i < musicDatabaseArtists.Count; i++)
                    {
                        artist = musicDatabaseArtists[i].ToString();
                        if (StopScraper == true || Utils.GetIsStopping())
                        {                            
                            break;
                        }

                        if (this.DoScrape(artist, true, true) > 0 && firstRun)
                        {
                            AddScapedFanartToAnyHash();
                            if (FanartHandlerSetup.MyScraperNowWorker != null)
                            {
                                FanartHandlerSetup.MyScraperNowWorker.TriggerRefresh = true;
                                firstRun = false;
                            }                            
                        }
                        CurrArtistsBeingScraped++;
                        if (TotArtistsBeingScraped > 0 && FanartHandlerSetup.MyScraperWorker != null)
                        {
                            FanartHandlerSetup.MyScraperWorker.ReportProgress(Convert.ToInt32((CurrArtistsBeingScraped / TotArtistsBeingScraped) * 100), "Ongoing");
                        }
                    }
                }

                logger.Info("InitialScrape is done.");
//                IsScraping = false;
                musicDatabaseArtists = null;
                AddScapedFanartToAnyHash();                
            }
            catch (Exception ex)
            {
//                IsScraping = false;
                logger.Error("InitialScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Refreshes the music "any" fanart if no images at all was available upon 
        /// MP start.
        /// </summary>
        private void AddScapedFanartToAnyHash()
        {
            if (HtAnyMusicFanart == null || HtAnyMusicFanart.Count < 1)
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
                Utils.Shuffle(ref htTmp);
                HtAnyMusicFanart = htTmp;              
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
                string sqlQuery = "SELECT Version FROM Version;";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                string tmpS = String.Empty;
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    tmpS = result.GetField(i, 0);
                }
                
                if (tmpS != null && (tmpS.Equals("1.0") || tmpS.Equals("1.1") || tmpS.Equals("1.2")))
                {
                    logger.Info("Upgrading Database to version 1.3");
                    sqlQuery = "DELETE FROM Movie_Fanart;";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    sqlQuery = "DELETE FROM TVSeries_Fanart;";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    sqlQuery = "DELETE FROM MovingPicture_Fanart;";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    sqlQuery = "UPDATE Version SET Version = '1.3'";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                }
                if (tmpS != null && tmpS.Equals("1.3"))
                {
                    logger.Info("Upgrading Database to version 1.4");
                    sqlQuery = "CREATE TABLE TimeStamps (Id INTEGER PRIMARY KEY, Key TEXT, Value TEXT, Time_Stamp TEXT);";
                    result = dbClient.Execute(sqlQuery);
                    sqlQuery = "UPDATE Version SET Version = '1.4'";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                }                
                result = null;
                sqlQuery = null;
                tmpS = null;             
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
        /// <param name="type">The type to be returned</param>
        public void DeleteAllFanart(string type)
        {
            try
            {
                string sqlQuery = sqlQuery = "DELETE FROM " + GetTableName(type) + " WHERE Type = '" + Utils.PatchSQL(type) + "';";
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
        /// <param name="disk_image">The filename on disk</param>
        /// <param name="action">Enable or disable</param>
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
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
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
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
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
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
        /// <param name="type">The type to run the action on</param>
        public void EnableFanartRandom(string disk_image, bool action, string type)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE " + GetTableName(type) + " SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE " + GetTableName(type) + " SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
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
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="type">The type to delete</param>
        public void DeleteFanart(string disk_image, string type)
        {
            try
            {
                //delete music fanart
                string sqlQuery = "DELETE FROM " + GetTableName(type) + " WHERE Disk_Image = '" + Utils.PatchSQL(disk_image) + "';";
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
        /// <param name="lastID">Last id that this sql was run towards</param>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForTable(int lastID)
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
        /// <param name="lastID">Last id that this sql was run towards</param>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForTableMovie(int lastID)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Movie_Fanart WHERE Id > " + lastID + " AND type = 'Movie' order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTableMovie: " + ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Music Fanart (In MP configuration).
        /// </summary>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForTableMusicOverview()
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
        /// <param name="lastID">Last id that this sql was run towards</param>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForTableScoreCenter(int lastID)
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
        /// Returns all data used by datagridview in the "Scraper Settings" tab for Scorecenter (In MP configuration).
        /// </summary>
        /// <param name="lastID">Last id that this sql was run towards</param>
        /// <param name="type">The type to run the query on</param>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForTableRandom(int lastID, string type)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM " + GetTableName(type) + " WHERE Id > " + lastID + " order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTableRandom: " + ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Sets the timestamp for a given key
        /// </summary>
        /// <param name="key">Name of the key</param>
        /// <param name="value">Timestamp to set</param>
        public void SetTimeStamp(string key, string value)
        {
            try
            {
                string sqlQuery = "SELECT COUNT(Value) FROM TimeStamps WHERE Key = '" + Utils.PatchSQL(key) + "';";
                DateTime saveNow = DateTime.Now;
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "UPDATE TimeStamps SET Value = '" + Utils.PatchSQL(value) + "', Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd") + "' WHERE Key = '" + Utils.PatchSQL(key) + "';";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    sqlQuery = null;
                }
                else
                {
                    sqlQuery = "INSERT INTO TimeStamps (Id, Key, Value, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(key) + "','" + Utils.PatchSQL(value) + "','" + saveNow.ToString(@"yyyyMMdd") + "');";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    sqlQuery = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("SetTimeStamp: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get timestamp from a given key
        /// </summary>
        /// <param name="key">The name of the key</param>
        /// <returns>A string containing the timestamp value</returns>
        public string GetTimeStamp(string key)
        {
            try
            {
                string sqlQuery = "SELECT Value FROM TimeStamps WHERE Key = '"+Utils.PatchSQL(key)+"';";
                SQLiteResultSet result = dbClient.Execute(sqlQuery);
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    return result.GetField(i, 0);
                }

                result = null;
                sqlQuery = null;
            }
            catch (Exception ex)
            {
                logger.Error("GetTimeStamp: " + ex.ToString());
            }

            return null;
        }        

        /// <summary>
        /// Returns all images for an artist.
        /// </summary>
        /// <param name="artist">The artist name</param>
        /// <param name="type">The type to run the query on</param>
        /// <returns>A hashtable with fanarts</returns>
        public Hashtable GetFanart(string artist, string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                //string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + getTableName(type) + " WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True';";
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Artist IN (" + Utils.HandleMultipleArtistNamesForDBQuery(Utils.PatchSQL(artist)) + ") AND Enabled = 'True';";
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
        /// <param name="artist">The artist name</param>
        /// <param name="type">The type to run the query on</param>
        /// <returns>A hashtable with fanarts</returns>
        public Hashtable GetHigResFanart(string artist, string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                //string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True' AND Type = 'MusicFanart';";
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Artist IN (" + Utils.HandleMultipleArtistNamesForDBQuery(Utils.PatchSQL(artist)) + ") AND Enabled = 'True' AND Type = 'MusicFanart';";
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
        /// <param name="type">The type to run the query on</param>
        /// <returns>The table name</returns>
        private string GetTableName(string type)
        {
            if (type.Equals("Game"))
            {
                return "Game_Fanart";
            }
            else if (type.Equals("Movie"))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("myVideos"))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("Online Videos"))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("TV Section"))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("MusicAlbum"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("MusicArtist"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("MusicFanart"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Default"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Music Playlist"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Youtube.FM"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Music Videos"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("mVids"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Global Search"))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Picture"))
            {
                return "Picture_Fanart";
            }
            else if (type.Equals("ScoreCenter"))
            {
                return "Scorecenter_Fanart";
            }
            else if (type.Equals("MovingPicture"))
            {
                return "MovingPicture_Fanart";
            }
            else if (type.Equals("TVSeries"))
            {
                return "TVSeries_Fanart";
            }
            else if (type.Equals("TV"))
            {
                return "TV_Fanart";
            }
            else if (type.Equals("Plugin"))
            {
                return "Plugin_Fanart";
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Returns a hashtable.
        /// </summary>
        /// <param name="type">The type to run the query on</param>
        /// <returns>A hashtable with fanart data</returns>
        public Hashtable GetAnyHashtable(string type)
        {
            if (type.Equals("Game"))
            {
                return HtAnyGameFanart;
            }
            else if (type.Equals("Movie"))
            {
                return HtAnyMovieFanart;
            }
            else if (type.Equals("MusicAlbum"))
            {
                return HtAnyMusicFanart;
            }
            else if (type.Equals("MusicArtist"))
            {
                return HtAnyMusicFanart;
            }
            else if (type.Equals("MusicFanart"))
            {
                return HtAnyMusicFanart;
            }
            else if (type.Equals("Default"))
            {
                return HtAnyMusicFanart;
            }
            else if (type.Equals("Picture"))
            {
                return HtAnyPictureFanart;
            }
            else if (type.Equals("ScoreCenter"))
            {
                return HtAnyScorecenter;
            }
            else if (type.Equals("MovingPicture"))
            {
                return HtAnyMovingPicturesFanart;
            }
            else if (type.Equals("TVSeries"))
            {
                return HtAnyTVSeries;
            }
            else if (type.Equals("TV"))
            {
                return HtAnyTVFanart;
            }
            else if (type.Equals("Plugin"))
            {
                return HtAnyPluginFanart;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Adds to a hashtable.
        /// </summary>
        /// <param name="type">The type to run the query on</param>
        /// <param name="ht">Hashtable to add data to</param>
        private void AddToAnyHashtable(string type, Hashtable ht)
        {
            if (type.Equals("Game"))
            {
                HtAnyGameFanart = ht;
            }
            else if (type.Equals("Movie"))
            {
                HtAnyMovieFanart = ht;
            }
            else if (type.Equals("MusicAlbum"))
            {
                HtAnyMusicFanart = ht;
            }
            else if (type.Equals("MusicArtist"))
            {
                HtAnyMusicFanart = ht;
            }
            else if (type.Equals("MusicFanart"))
            {
                HtAnyMusicFanart = ht;
            }
            else if (type.Equals("Default"))
            {
                HtAnyMusicFanart = ht;
            }
            else if (type.Equals("Picture"))
            {
                HtAnyPictureFanart = ht;
            }
            else if (type.Equals("ScoreCenter"))
            {
                HtAnyScorecenter = ht;
            }
            else if (type.Equals("MovingPicture"))
            {
                HtAnyMovingPicturesFanart = ht;
            }
            else if (type.Equals("TVSeries"))
            {
                HtAnyTVSeries = ht;
            }
            else if (type.Equals("TV"))
            {
                HtAnyTVFanart = ht;
            }
            else if (type.Equals("Plugin"))
            {
                HtAnyPluginFanart = ht;
            }
        }

        /// <summary>
        /// Returns all random fanart for a specific type (like music or movies). First time builds hashtable, 
        /// then only returns that hashtable.
        /// </summary>
        /// <param name="type">The type to run the query on</param>
        /// <param name="types">Part of the sql statement</param>
        /// <returns>A hashtable with random fanart</returns>
        public Hashtable GetAnyFanart(string type, string types)
        {
            Hashtable ht = GetAnyHashtable(type);
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
                        sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Enabled = 'True' AND Type IN (" + types + ");";
                    }
                    else
                    {
                            sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Enabled = 'True' AND Type = '"+Utils.PatchSQL(type)+"';";
                    }
                    SQLiteResultSet result = dbClient.Execute(sqlQuery);

                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                        ht.Add(i, fi);                        
                    }
                    Utils.Shuffle(ref ht);

                    result = null;
                    AddToAnyHashtable(type, ht);
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
        /// <param name="artist">The artis name</param>
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="source_image">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadFanart(string artist, string disk_image, string source_image, string type)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM " + GetTableName(type) + " WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSQL(source_image) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do not allow updates
                }
                else
                {
                    sqlQuery = "INSERT INTO " + GetTableName(type) + " (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "','" + Utils.PatchSQL(disk_image) + "','" + Utils.PatchSQL(source_image) + "','" + Utils.PatchSQL(type) + "','www.htbackdrops.com', 'True', '" + saveNow.ToString(@"yyyyMMdd") + "');";
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
        /// Inserts new fanart into the database.
        /// </summary>
        /// <param name="artist">The artis name</param>
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="source_image">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadFanartExternal(string artist, string disk_image, string source_image, string type)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Movie_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSQL(source_image) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do not allow updates
                }
                else
                {
                    sqlQuery = "INSERT INTO Movie_Fanart (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp) VALUES(null, '" + Utils.PatchSQL(artist) + "','" + Utils.PatchSQL(disk_image) + "','" + Utils.PatchSQL(source_image) + "','" + Utils.PatchSQL(type) + "','" + Utils.PatchSQL(type) + "', 'True', '" + saveNow.ToString(@"yyyyMMdd") + "');";
                    lock (dbClient) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing external fanart into fanart handler database (" + disk_image + ").");
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
        /// <param name="artist">The artist name</param>
        /// <param name="source_image">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        /// <returns>Returns if an image exists or not</returns>
        public bool SourceImageExist(string artist, string source_image, string type)
        {
            try
            {
                string sqlQuery = String.Empty;
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
        /// <param name="artist">The artist name</param>
        /// <param name="disk_image">Filename on disk</param>
        /// <param name="source_image">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadMusicFanart(string artist, string disk_image, string source_image, string type)
        {
            try
            {
                string sqlQuery = String.Empty;
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
        /// <param name="artist">The artist name</param>
        /// <param name="type">The type to run the query on</param>
        public void InsertNewMusicArtist(string artist, string type)
        {
            try
            {
                string sqlQuery = String.Empty;
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
        /// <param name="artist">The artist name</param>
        public void SetSuccessfulScrape(string artist)
        {
            try
            {
                string sqlQuery = String.Empty;
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

            /// <summary>
            /// Initializes a new instance of the FanartImage class.
            /// </summary>
            /// <param name="id">Identifier number</param>
            /// <param name="artist">Artist name</param>
            /// <param name="disk_image">Filename on disk</param>
            /// <param name="source_image">Filename at source</param>
            /// <param name="type">Type of the file</param>
            /// <param name="source">Source name (like htbackdrops)</param>
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
