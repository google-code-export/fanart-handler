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

using System.Globalization;
namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.Database;
    using MediaPortal.GUI.Library;
    using MediaPortal.Music.Database;
    using MediaPortal.Picture.Database;
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
        private bool stopScraper/* = false*/;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SQLiteClient dbClient;
        private readonly object lockObject = new object();
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
        private bool isScraping/* = false*/;
        private Scraper scraper;
        private double totArtistsBeingScraped/* = 0*/;
        private double currArtistsBeingScraped/* = 0*/;
        private bool isInitialized/* = false*/;
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

        public Hashtable HTAnyPluginFanart
        {
            get { return htAnyPluginFanart; }
            set { htAnyPluginFanart = value; }
        }

        public Hashtable HTAnyTVSeries
        {
            get { return htAnyTVSeries; }
            set { htAnyTVSeries = value; }
        }

        public Hashtable HTAnyTVFanart
        {
            get { return htAnyTVFanart; }
            set { htAnyTVFanart = value; }
        }

        public Hashtable HTAnyScorecenter
        {
            get { return htAnyScorecenter; }
            set { htAnyScorecenter = value; }
        }

        public Hashtable HTAnyPictureFanart
        {
            get { return htAnyPictureFanart; }
            set { htAnyPictureFanart = value; }
        }

        public Hashtable HTAnyMusicFanart
        {
            get { return htAnyMusicFanart; }
            set { htAnyMusicFanart = value; }
        }

        public Hashtable HTAnyMovingPicturesFanart
        {
            get { return htAnyMovingPicturesFanart; }
            set { htAnyMovingPicturesFanart = value; }
        }

        public Hashtable HTAnyMovieFanart
        {
            get { return htAnyMovieFanart; }
            set { htAnyMovieFanart = value; }
        }

        public Hashtable HTAnyGameFanart
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
                UpgradeDBMain();
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
                    dbClient.Dispose();
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
        public bool NowPlayingScrape(string artist)
        {
            try
            {
                logger.Info("NowPlayingScrape is starting for artist " + artist + ".");
                TotArtistsBeingScraped = 2;
                CurrArtistsBeingScraped = 0;
                bool bFound = false;
                if (artist.Contains("|"))
                {
                    string[] artists = artist.Split('|');
                    foreach (string thisArtist in artists)
                    {
                        if (DoScrapeNew(thisArtist) > 0)
                        {
                            bFound = true;
                        }
                    }
                    logger.Info("NowPlayingScrape is done.");
                    return bFound;
                }
                else
                {
                    if (DoScrapeNew(artist) > 0)
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
            dbClient.Execute(sqlQuery);
            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Artist_Artist;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Artist;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Disk_Image;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Source_Image;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP INDEX Idx_Music_Fanart_Type;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Game_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Movie_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE MovingPicture_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Music_Artist;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Music_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Picture_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Plugin_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Scorecenter_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE TVSeries_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE TV_Fanart;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            try
            {
                sqlQuery = "DROP TABLE Version;";
                dbClient.Execute(sqlQuery);
            }
            catch
            {
            }

            sqlQuery = "CREATE TABLE Game_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Movie_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE MovingPicture_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Music_Artist (Id INTEGER PRIMARY KEY, Artist TEXT, Successful_Scrape NUMERIC, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Music_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Picture_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Plugin_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Scorecenter_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE TVSeries_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE TV_Fanart (Id INTEGER PRIMARY KEY, Artist TEXT, Disk_Image TEXT, Source_Image TEXT, Source TEXT, Type TEXT, Enabled TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "INSERT INTO Version (Id, Version, Time_Stamp) VALUES (null,'1.2', '" + DateTime.Now.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "');";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Artist_Artist ON Music_Artist(Artist ASC);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Artist ON Music_Fanart(Artist ASC);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Disk_Image ON Music_Fanart(Disk_Image ASC);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Source_Image ON Music_Fanart(Source_Image ASC);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "CREATE INDEX Idx_Music_Fanart_Type ON Music_Fanart(Type ASC);";
            dbClient.Execute(sqlQuery);
            sqlQuery = "COMMIT;";
            dbClient.Execute(sqlQuery);
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
                dbClient.Execute(sqlQuery);
            }
            catch (SQLiteException sle)
            {
                string sErr = sle.ToString();
                if ((sErr != null && sErr.IndexOf("no such column: Enabled", StringComparison.CurrentCulture) >= 0) ||
                    (sErr != null && sErr.IndexOf("no column named Enabled", StringComparison.CurrentCulture) >= 0) ||
                    (sErr != null && sErr.IndexOf("no such table", StringComparison.CurrentCulture) >= 0) ||
                    (sErr != null && sErr.IndexOf("file is encrypted or is not a database query", StringComparison.CurrentCulture) >= 0))
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
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
            }

            return i;
        }

        /// <summary>
        /// Get total initilised artists in the Fanart Handler database
        /// </summary>
        /// <returns>Total number of initialized artists in the fanart handler database</returns>
        public int GetTotalArtistsInitialisedInFanartDatabase()
        {
            string sqlQuery = "SELECT count(t1.Artist) FROM Music_Artist t1 WHERE t1.Artist in (SELECT distinct(t2.Artist) FROM Music_Fanart t2 WHERE t2.type = 'MusicFanart Scraper');";
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
            }

            return i;
        }

        /// <summary>
        /// Get total movie images in the Fanart Handler database
        /// </summary>
        /// <returns>Total number of movies in the fanart handler database</returns>
        public int GetTotalMoviesInFanartDatabase()
        {
            string sqlQuery = "SELECT count(Artist) FROM Movie_Fanart where type = 'Movie User';";
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
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
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
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
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
            }

            return i;
        }

        /// <summary>
        /// Get total uninitilised artists in the Fanart Handler database
        /// </summary>
        /// <returns>Return total uninitilised artists in the Fanart Handler database</returns>
        public int GetTotalArtistsUNInitialisedInFanartDatabase()
        {
            string sqlQuery = "SELECT count(t1.Artist) FROM Music_Artist t1 WHERE t1.Artist not in (SELECT distinct(t2.Artist) FROM Music_Fanart t2 WHERE t2.type = 'MusicFanart Scraper');";
            SQLiteResultSet result;
            lock (lockObject) result = dbClient.Execute(sqlQuery);
            int i = 0;
            if (result != null)
            {
                i = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
            }
            result = null;
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
                    string sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicFanart Scraper';";
                    SQLiteResultSet result;
                    lock (lockObject) result = dbClient.Execute(sqlQuery);
                    int i = 0;
                    for (int x = 0; x < result.Rows.Count; x++)
                    {
                        i = Int32.Parse(result.GetField(x, 0), CultureInfo.CurrentCulture);
                    }
                    return i;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetArtistCount: " + ex.ToString());
            }

            return 999;
        }

        /// <summary>
        /// Return the current number of thumbnail images an artist has.
        /// </summary>
        /// <param name="artist">The artist name</param>
        /// <param name="dbArtist">The db artist name</param>
        /// <returns>Get total number of images an artist has.</returns>
        public int GetArtistThumbsCount(string artist, string dbArtist)
        {
            try
            {
                int y = m_db.GetArtistId(artist);
                if (y > 0)
                {
                    string sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicThumbnails';";
                    SQLiteResultSet result;
                    lock (lockObject) result = dbClient.Execute(sqlQuery);
                    int i = 0;
                    for (int x = 0; x < result.Rows.Count; x++)
                    {
                        i = Int32.Parse(result.GetField(x, 0), CultureInfo.CurrentCulture);
                    }
                    return i;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetArtistThumbsCount: " + ex.ToString());
            }

            return 999;
        }



        /// <summary>
        /// Performs a scrape on the "new" pages on htbackdrops.com.
        /// </summary>
        public void DoNewScrape()
        {
            if (StopScraper == false)
            {
                Utils.SetDelayStop(true);
                try
                {
                    scraper = new Scraper();
                    scraper.GetNewImages(Convert.ToInt32(Utils.GetScraperMaxImages(), CultureInfo.CurrentCulture), this);
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
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
                for (int x = 0; x < result.Rows.Count; x++)
                {
                    filename = result.GetField(x, 0);
                    if (File.Exists(filename) == false)
                    {
                        DeleteFanart(filename, "MusicFanart Scraper");
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
        /// Performs the scrape (now playing or initial).
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <param name="useSuccessfulScrape">Use the successfuls scrape flag in db or not</param>
        /// <param name="useStopScraper">Use the stop scraper parameter or not</param>
        /// <param name="swnp">ScraperWorkerNowPlaying object</param>
        /// <returns>Number of scraped images</returns>
        public int DoScrape(string artist)
        {
            if (StopScraper == false)
            {
                Utils.SetDelayStop(true);
                try
                {
                    string dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                    scraper = new Scraper();
                    string sqlQuery;
                    int totalImages = 0;
                    int iTmp = 0;
                    int successful_scrape = 0;
                    string succFanart = String.Empty;
                    string succThumb = String.Empty;
                    lock (lockObject) dbClient.Execute("BEGIN TRANSACTION;");
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        InsertNewMusicArtist(dbArtist);
                        sqlQuery = "SELECT Successful_Scrape, successful_thumb_scrape FROM Music_Artist WHERE Artist = '" + Utils.PatchSql(dbArtist) + "';";
                        SQLiteResultSet result;
                        lock (lockObject) result = dbClient.Execute(sqlQuery);
                        succFanart = result.GetField(0, 0);
                        succThumb = result.GetField(0, 1);
                        if (succFanart != null && succFanart.Length > 0)
                        {
                            successful_scrape = Int32.Parse(succFanart, CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            successful_scrape = 0;
                        }
                        if (Utils.ScrapeThumbnails.Equals("True", StringComparison.CurrentCulture))
                        {
                            if (succThumb != null && succThumb.Length > 0)
                            {
                                //do nothing
                            }
                            else
                            {
                                succThumb = "0";
                            }
                        }
                        else
                        {
                            succThumb = "1";
                        }
                        sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicFanart Scraper';";
                        result = dbClient.Execute(sqlQuery);
                        if (successful_scrape == 1 && (succThumb.Equals("1", StringComparison.CurrentCulture) || succThumb.Equals("2", StringComparison.CurrentCulture)))
                        {
                            SetSuccessfulScrape(dbArtist);
                            lock (lockObject) dbClient.Execute("COMMIT;");
                            scraper = null;
                            return 0;
                        }
                        else
                        {
                            iTmp = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);
                        }
                        int maxScrapes = Convert.ToInt32(Utils.GetScraperMaxImages(), CultureInfo.CurrentCulture) - iTmp;
                        if (maxScrapes > 0)
                        {
                            totalImages = scraper.GetImages(artist, maxScrapes, this, true);
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
                        result = null;
                        scraper = null;
                    }
                    lock (lockObject) dbClient.Execute("COMMIT;");
                    return totalImages;
                }
                catch (Exception ex)
                {
                    lock (lockObject) dbClient.Execute("ROLLBACK;");
                    logger.Error("doScrape: " + ex.ToString());
                }
            }

            return 0;
        }

        /// <summary>
        /// Performs the scrape (now playing or initial).
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <param name="useSuccessfulScrape">Use the successfuls scrape flag in db or not</param>
        /// <param name="useStopScraper">Use the stop scraper parameter or not</param>
        /// <param name="swnp">ScraperWorkerNowPlaying object</param>
        /// <returns>Number of scraped images</returns>
        public int DoScrapeNew(string artist)
        {
            if (StopScraper == false)
            {
                Utils.SetDelayStop(true);
                try
                {
                    string dbArtist = Utils.GetArtist(artist, "MusicFanart Scraper");
                    scraper = new Scraper();
                    string sqlQuery;
                    int totalImages = 0;
                    int iTmp = 0;
                    //                    string tmp = String.Empty // COMMENTED BY CODEIT.RIGHT;
                    lock (lockObject) dbClient.Execute("BEGIN TRANSACTION;");
                    if (artist != null && artist.Trim().Length > 0)
                    {
                        InsertNewMusicArtist(dbArtist);

                        sqlQuery = "SELECT count(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(dbArtist) + "' AND Enabled = 'True' AND Type = 'MusicFanart Scraper';";
                        SQLiteResultSet result;
                        lock (lockObject) result = dbClient.Execute(sqlQuery);
                        iTmp = Int32.Parse(result.GetField(0, 0), CultureInfo.CurrentCulture);

                        if (iTmp == 0)
                        {
                            int iMax = Convert.ToInt32(Utils.GetScraperMaxImages(), CultureInfo.CurrentCulture);
                            totalImages = scraper.GetImages(artist, iMax, this, false);
                            if (totalImages == 0)
                            {
                                logger.Debug("No fanart found for artist " + artist + ".");
                            }
                        }
                        if (totalImages != 99)
                        {
                            SetSuccessfulScrape(dbArtist);
                        }
                        result = null;
                        scraper = null;
                    }

                    lock (lockObject) dbClient.Execute("COMMIT;");
                    return totalImages;
                }
                catch (Exception ex)
                {
                    lock (lockObject) dbClient.Execute("ROLLBACK;");
                    logger.Error("DoScrapeNew: " + ex.ToString());
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
                        if (this.DoScrape(artist) > 0 && firstRun)
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
                //                Utils.GetDbm().SetTimeStamp("Fanart Handler Last Scrape", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
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
            if (HTAnyMusicFanart == null || HTAnyMusicFanart.Count < 1)
            {
                Hashtable htTmp = new Hashtable();
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Enabled = 'True' AND Type IN ('MusicFanart Scraper');";
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    FanartImage fi = new FanartImage(result.GetField(i, 0), result.GetField(i, 1), result.GetField(i, 2), result.GetField(i, 3), result.GetField(i, 4), result.GetField(i, 5));
                    htTmp.Add(i, fi);
                }

                result = null;
                sqlQuery = null;
                Utils.Shuffle(ref htTmp);
                HTAnyMusicFanart = htTmp;
            }
        }

        /// <summary>
        /// Upgrade db to version 1.0
        /// </summary>
        public void UpgradeDBMain()
        {
            DateTime saveNow = DateTime.Now;
            bool justUpgraded = false;
            string currVersion = string.Empty;
            try
            {
                string sqlQuery = "SELECT Version FROM Version;";
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
                string tmpS = String.Empty;
                for (int i = 0; i < result.Rows.Count; i++)
                {
                    tmpS = result.GetField(i, 0);
                    currVersion = tmpS;
                }

                if (tmpS != null && (tmpS.Equals("1.0", StringComparison.CurrentCulture) || tmpS.Equals("1.1", StringComparison.CurrentCulture) || tmpS.Equals("1.2", StringComparison.CurrentCulture)))
                {
                    logger.Info("Upgrading Database to version 1.3");
                    sqlQuery = "DELETE FROM Movie_Fanart;";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    sqlQuery = "DELETE FROM TVSeries_Fanart;";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    sqlQuery = "DELETE FROM MovingPicture_Fanart;";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    sqlQuery = "UPDATE Version SET Version = '1.3'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgraded Database to version 1.3");
                    currVersion = "1.3";
                }
                if (tmpS != null && tmpS.Equals("1.3", StringComparison.CurrentCulture))
                {
                    logger.Info("Upgrading Database to version 1.4");
                    sqlQuery = "CREATE TABLE TimeStamps (Id INTEGER PRIMARY KEY, Key TEXT, Value TEXT, Time_Stamp TEXT);";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.4'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    logger.Info("Upgraded Database to version 1.4");
                    justUpgraded = true;
                    currVersion = "1.4";
                }
                if ((tmpS != null && tmpS.Equals("1.4", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 1.5");
                    sqlQuery = "alter table music_artist add successful_thumb_scrape NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.5'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 1.5");
                    currVersion = "1.5";
                }
                if ((tmpS != null && tmpS.Equals("1.5", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 1.6");
                    sqlQuery = "alter table game_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "update game_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    sqlQuery = "alter table movie_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 3 - finished");
                    sqlQuery = "alter table movingpicture_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 4 - finished");
                    sqlQuery = "alter table music_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 5 - finished");
                    sqlQuery = "update music_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 6 - finished");
                    sqlQuery = "alter table picture_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 7 - finished");
                    sqlQuery = "update picture_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 8 - finished");
                    sqlQuery = "alter table plugin_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 9 - finished");
                    sqlQuery = "update plugin_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 10 - finished");
                    sqlQuery = "alter table scorecenter_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 11 - finished");
                    sqlQuery = "update scorecenter_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 12 - finished");
                    sqlQuery = "alter table tvseries_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 13 - finished");
                    sqlQuery = "update tvseries_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 14 - finished");
                    sqlQuery = "alter table tv_fanart add restricted NUMERIC;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 15 - finished");
                    sqlQuery = "update tv_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 16 - finished");
                    sqlQuery = "DELETE FROM Timestamps;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 17 - finished");
                    sqlQuery = "DELETE FROM Movie_Fanart WHERE Artist <> 'default';";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 18 - finished");
                    sqlQuery = "DELETE FROM MovingPicture_Fanart;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 19 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.6'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 20 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 1.6");
                    currVersion = "1.6";
                }
                if ((tmpS != null && tmpS.Equals("1.6", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 1.7");
                    sqlQuery = "DELETE FROM Timestamps;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "DELETE FROM Movie_Fanart WHERE Artist <> 'default';";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    sqlQuery = "DELETE FROM MovingPicture_Fanart;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 3 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.7'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 4 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 1.7");
                    currVersion = "1.7";
                }
                if ((tmpS != null && tmpS.Equals("1.7", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 1.8");
                    sqlQuery = "update music_fanart set restricted = 0;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.8'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 1.8");
                    currVersion = "1.8";
                }
                if ((tmpS != null && tmpS.Equals("1.8", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 1.9");
                    sqlQuery = "DELETE FROM Timestamps;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "DELETE FROM Movie_Fanart WHERE Artist <> 'default';";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    sqlQuery = "DELETE FROM MovingPicture_Fanart;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 3 - finished");
                    sqlQuery = "DELETE FROM TVSeries_Fanart;";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 4 - finished");
                    sqlQuery = "UPDATE Version SET Version = '1.9'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 5 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 1.9");
                    currVersion = "1.9";
                }
                if ((tmpS != null && tmpS.Equals("1.9", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 2.0");
                    sqlQuery = "DELETE FROM Timestamps WHERE Key = 'Fanart Handler Last Scrape';";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE Version SET Version = '2.0'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 2.0");
                    currVersion = "2.0";
                }
                if ((tmpS != null && tmpS.Equals("2.0", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 2.1");
                    sqlQuery = "DELETE FROM Timestamps WHERE Key = 'Fanart Handler Last Scrape';";
                    result = dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE Version SET Version = '2.1'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 2.1");
                    currVersion = "2.1";
                }
                if ((tmpS != null && tmpS.Equals("2.1", StringComparison.CurrentCulture)) || justUpgraded)
                {
                    logger.Info("Upgrading Database to version 2.2");
                    sqlQuery = "UPDATE game_fanart SET Type = 'Game User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef')";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 1 - finished");
                    sqlQuery = "UPDATE movie_fanart SET Type = 'Movie User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef') WHERE Type = 'Movie'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 2 - finished");
                    sqlQuery = "UPDATE music_fanart SET Type = 'MusicFanart Scraper', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\Scraper'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\Scraper') WHERE Type = 'MusicFanart'"; 
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 3 - finished");
                    sqlQuery = "UPDATE music_fanart SET disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef') WHERE Type = 'Default'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 4 - finished");
                    sqlQuery = "UPDATE picture_fanart SET Type = 'Picture User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef')";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 5 - finished");
                    sqlQuery = "UPDATE plugin_fanart SET Type = 'Plugin User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef')";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 6 - finished");
                    sqlQuery = "UPDATE scorecenter_fanart SET Type = 'Scorecenter User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef')";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 7 - finished");
                    sqlQuery = "UPDATE tv_fanart SET Type = 'TV User', disk_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef'), source_image = replace(disk_image, 'Skin FanArt','Skin FanArt\\UserDef')";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 8 - finished");
                    sqlQuery = "DELETE FROM Timestamps WHERE Key LIKE 'Directory -%';";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Info("Upgrading Step 9 - finished");
                    sqlQuery = "UPDATE Version SET Version = '2.2'";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    MigrateFanart();
                    justUpgraded = true;
                    logger.Info("Upgraded Database to version 2.2");
                    currVersion = "2.2";
                }
                result = null;
                sqlQuery = null;
                tmpS = null;
                logger.Info("Database version is verified: " + currVersion);
            }
            catch (SQLiteException sle)
            {
                string sErr = sle.ToString();
                if (sErr != null && sErr.IndexOf("no such table: Version", StringComparison.CurrentCulture) >= 0)
                {
                    string sqlQuery = "BEGIN TRANSACTION;";
                    dbClient.Execute(sqlQuery);
                    sqlQuery = "CREATE TABLE Version (Id INTEGER PRIMARY KEY, Version TEXT, Time_Stamp TEXT);";
                    dbClient.Execute(sqlQuery);
                    sqlQuery = "COMMIT;";
                    dbClient.Execute(sqlQuery);
                    sqlQuery = "INSERT INTO Version (Id, Version, Time_Stamp) VALUES (null,'1.2', '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "');";
                    dbClient.Execute(sqlQuery);
                }
            }
            catch (Exception ex)
            {
                //do nothing
                logger.Debug(ex.ToString());
            }
        }

        private void MigrateFanart()
        {
            FanartHandlerSetup.SetupDirectoriesOLD();
            string pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
            string pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 10 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 11 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
            MigrateFanartMusic(pathOld, pathNew);
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 12 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 13 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 14 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 15 - finished");
            pathOld = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
            pathNew = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
            MigrateFanart(pathOld, pathNew);
            logger.Info("Upgrading Step 16 - finished");
        }

        private void MigrateFanartMusic(string pathOld, string pathNew)
        {
            if (File.Exists(pathOld+"\\default.jpg"))
            {
                File.Copy(pathOld + "\\default.jpg", pathNew.Replace("Scraper", "UserDef") + "\\default.jpg",true);
            }
        }

        private void MigrateFanart(string pathOld, string pathNew)
        {
            DirectoryInfo diSource = new DirectoryInfo(pathOld);
            foreach (FileInfo fi in diSource.GetFiles())
            {
                fi.CopyTo(Path.Combine(pathNew, fi.Name), true);
                fi.Delete();
            }
            Directory.Delete(pathOld);
        }

        /// <summary>
        /// Deletes all fanart in the database and resets the initial flag.
        /// </summary>
        /// <param name="type">The type to be returned</param>
        public void DeleteAllFanart(string type)
        {
            try
            {
                string sqlQuery = sqlQuery = "DELETE FROM " + GetTableName(type) + " WHERE Type = '" + Utils.PatchSql(type) + "';";
                lock (lockObject) dbClient.Execute(sqlQuery);
                if (type.StartsWith("MusicFanart Scraper", StringComparison.CurrentCulture))
                {
                    DateTime saveNow = DateTime.Now;
                    sqlQuery = "UPDATE Music_Artist SET Successful_Scrape = 0, Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "';";
                    lock (lockObject) dbClient.Execute(sqlQuery);
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
                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("ResetInitialScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        /// <param name="diskImage">The filename on disk</param>
        /// <param name="action">Enable or disable</param>
        public void EnableFanartMusic(string diskImage, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE Music_Fanart SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE Music_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }

                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartMusic: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
        public void EnableFanartMovie(string diskImage, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE Movie_Fanart SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE Movie_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }

                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartMovie: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
        public void EnableFanartScoreCenter(string diskImage, bool action)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE ScoreCenter_Fanart SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE ScoreCenter_Fanart SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }

                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartScoreCenter: " + ex.ToString());
            }
        }

        /// <summary>
        /// Sets the enabled column in the database. Controls if fanart is enabled or disabled.
        /// </summary>
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="action">Enable or disable</param>
        /// <param name="type">The type to run the action on</param>
        public void EnableFanartRandom(string diskImage, bool action, string type)
        {
            try
            {
                string sqlQuery;
                if (action == true)
                {
                    sqlQuery = "UPDATE " + GetTableName(type) + " SET Enabled = 'True' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }
                else
                {
                    sqlQuery = "UPDATE " + GetTableName(type) + " SET Enabled = 'False' WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                }

                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("EnableFanartRandom: " + ex.ToString());
            }
        }

        /// <summary>
        /// Delete a specific image from the database.
        /// </summary>
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="type">The type to delete</param>
        public void DeleteFanart(string diskImage, string type)
        {
            try
            {
                //delete music fanart
                string sqlQuery = "DELETE FROM " + GetTableName(type) + " WHERE Disk_Image = '" + Utils.PatchSql(diskImage) + "';";
                lock (lockObject) dbClient.Execute(sqlQuery);
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
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Music_Fanart WHERE Id > " + lastID + " AND Type = 'MusicFanart Scraper' order by Artist, Disk_Image;";
                lock (lockObject) result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTable: " + ex.ToString());
            }

            return result;
        }

        /// <summary>
        /// Returns all data used by datagridview in the "Thumbnails" tab for Music (In MP configuration).
        /// </summary>
        /// <param name="lastID">Last id that this sql was run towards</param>
        /// <returns>Resultset containg requested data</returns>
        public SQLiteResultSet GetDataForThumbTable(int lastID)
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Music_Fanart WHERE Id > " + lastID + " AND Type = 'MusicThumbnails' order by Artist, Disk_Image;";
                lock (lockObject) result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("GetDataForThumbTable: " + ex.ToString());
            }

            return result;
        }



        /// <summary>
        /// Returns last music track info added to MP music database.
        /// </summary>
        /// <returns>Hashtable containg artist names</returns>
        public FanartHandler.LatestsCollection GetLatestMusic()
        {
            FanartHandler.LatestsCollection result = new FanartHandler.LatestsCollection();
            int x = 0;
            try
            {
                string sqlQuery = "select distinct strArtist, strAlbum, dateAdded, strGenre, strPath from tracks order by dateAdded desc limit 50;";
                List<Song> songInfo = new List<Song>();
                m_db.GetSongsByFilter(sqlQuery, out songInfo, "tracks");
                Hashtable ht = new Hashtable();
                string key = string.Empty;
                UtilsExternal.latestMusicAlbums = new Hashtable();
                int i0 = 1;
                foreach (Song mySong in songInfo)
                {
                    string fanart = mySong.Artist;
                    string album = mySong.Album;
                    string sPath = mySong.FileName;
                    if (sPath != null && sPath.Length > 0 && sPath.IndexOf("\\") > 0)
                    {
                        sPath = sPath.Substring(0, sPath.LastIndexOf("\\"));
                    }
                    string dateAdded = mySong.DateTimeModified.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
                    if (album == null || album.Trim().Length == 0)
                    {
                        album = " ";
                    }

                    key = fanart + "#" + album;
                    if (!ht.Contains(key))
                    {
                        //Get Fanart
                        string CurrSelectedMusic = String.Empty;
                        int PrevSelectedMusic = -1;
                        string myArtist = mySong.Artist;//MediaPortal.Util.Utils.MakeFileName(mySong.Artist);
                        CurrSelectedMusic = String.Empty;
                        PrevSelectedMusic = -1;
                        string sFilename1 = FanartHandlerSetup.GetFilename(myArtist, ref CurrSelectedMusic, ref PrevSelectedMusic, "MusicFanart Scraper", "FanartSelected", true, true);
                        if (sFilename1.Length == 0)
                        {
                            sFilename1 = FanartHandlerSetup.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
                        }
                        //string sFilenamePrev = CurrSelectedMusic;
                        string sFilename2 = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref CurrSelectedMusic, ref PrevSelectedMusic, "MusicFanart Scraper", "FanartSelected", false, true);
                        if (sFilename2.Length == 0)
                        {
                            sFilename2 = FanartHandlerSetup.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
                        }

                        result.Add(new FanartHandler.Latest(dateAdded, null, null, null, null, fanart, mySong.Album, mySong.Genre.Replace("|", ""), null, null, null, null, null, null, null, null, null, sFilename1, sFilename2));
                        ht.Add(key, key);
                        x++;
                        UtilsExternal.latestMusicAlbums.Add(i0, sPath);
                        i0++;
                    }

                    if (x == 3)
                    {
                        break;
                    }
                }
                if (ht != null)
                {
                    ht.Clear();
                }
                ht = null;

            }
            catch (Exception ex)
            {
                logger.Error("GetLatestMusic: " + ex.ToString());
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
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image, Id FROM Movie_Fanart WHERE Id > " + lastID + " AND type = 'Movie User' order by Artist, Disk_Image;";
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
                string sqlQuery = "select music_artist.artist, count(music_fanart.type) from music_artist LEFT OUTER JOIN music_fanart ON music_artist.artist = music_fanart.artist and music_fanart.type = 'MusicFanart Scraper' group by music_artist.artist;";
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
                string sqlQuery = "SELECT COUNT(Value) FROM TimeStamps WHERE Key = '" + Utils.PatchSql(key) + "';";
                DateTime saveNow = DateTime.Now;
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "UPDATE TimeStamps SET Value = '" + Utils.PatchSql(value) + "', Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "' WHERE Key = '" + Utils.PatchSql(key) + "';";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    sqlQuery = null;
                }
                else
                {
                    sqlQuery = "INSERT INTO TimeStamps (Id, Key, Value, Time_Stamp) VALUES(null, '" + Utils.PatchSql(key) + "','" + Utils.PatchSql(value) + "','" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "');";
                    lock (lockObject) dbClient.Execute(sqlQuery);
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
                string sqlQuery = "SELECT Value FROM TimeStamps WHERE Key = '" + Utils.PatchSql(key) + "';";
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
        public Hashtable GetFanart(string artist, string type, int restricted)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string sRestricted = string.Empty;
                if (restricted == 1)
                {
                    sRestricted = "AND (restricted = 0 OR restricted = 1)";
                }
                else
                {
                    sRestricted = "AND restricted = 0";
                }
                //string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + getTableName(type) + " WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True';";
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Artist IN (" + Utils.HandleMultipleArtistNamesForDBQuery(Utils.PatchSql(artist)) + ") AND Enabled = 'True' " + sRestricted + ";";
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
        public Hashtable GetHigResFanart(string artist, int restricted)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string sRestricted = string.Empty;
                if (restricted == 1)
                {
                    sRestricted = "AND (restricted = 0 OR restricted = 1)";
                }
                else
                {
                    sRestricted = "AND restricted = 0";
                }
                //string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Artist = '" + Utils.PatchSQL(artist) + "' AND Enabled = 'True' AND Type = 'MusicFanart';";
                string sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM Music_Fanart WHERE Artist IN (" + Utils.HandleMultipleArtistNamesForDBQuery(Utils.PatchSql(artist)) + ") AND Enabled = 'True' AND Type = 'MusicFanart Scraper' " + sRestricted + ";";
                SQLiteResultSet result;
                lock (lockObject) result = dbClient.Execute(sqlQuery);
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
            if (type.Equals("Game User", StringComparison.CurrentCulture))
            {
                return "Game_Fanart";
            }
            else if (type.Equals("Movie User", StringComparison.CurrentCulture))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("Movie Scraper", StringComparison.CurrentCulture))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("myVideos", StringComparison.CurrentCulture))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("Online Videos", StringComparison.CurrentCulture))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("TV Section", StringComparison.CurrentCulture))
            {
                return "Movie_Fanart";
            }
            else if (type.Equals("MusicAlbum", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("MusicArtist", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("MusicFanart User", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Default", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Music Playlist", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Music Trivia", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Youtube.FM", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Music Videos", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("mVids", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Global Search", StringComparison.CurrentCulture))
            {
                return "Music_Fanart";
            }
            else if (type.Equals("Picture User", StringComparison.CurrentCulture))
            {
                return "Picture_Fanart";
            }
            else if (type.Equals("ScoreCenter User", StringComparison.CurrentCulture))
            {
                return "Scorecenter_Fanart";
            }
            else if (type.Equals("MovingPicture", StringComparison.CurrentCulture))
            {
                return "MovingPicture_Fanart";
            }
            else if (type.Equals("TVSeries", StringComparison.CurrentCulture))
            {
                return "TVSeries_Fanart";
            }
            else if (type.Equals("TV User", StringComparison.CurrentCulture))
            {
                return "TV_Fanart";
            }
            else if (type.Equals("Plugin User", StringComparison.CurrentCulture))
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
            if (type.Equals("Game User", StringComparison.CurrentCulture))
            {
                return HTAnyGameFanart;
            }
            else if (type.Equals("Movie User", StringComparison.CurrentCulture))
            {
                return HTAnyMovieFanart;
            }
            else if (type.Equals("Movie Scraper", StringComparison.CurrentCulture))
            {
                return HTAnyMovieFanart;
            }
            else if (type.Equals("MusicAlbum", StringComparison.CurrentCulture))
            {
                return HTAnyMusicFanart;
            }
            else if (type.Equals("MusicArtist", StringComparison.CurrentCulture))
            {
                return HTAnyMusicFanart;
            }
            else if (type.Equals("MusicFanart User", StringComparison.CurrentCulture))
            {
                return HTAnyMusicFanart;
            }
            else if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
            {
                return HTAnyMusicFanart;
            }
            else if (type.Equals("Default", StringComparison.CurrentCulture))
            {
                return HTAnyMusicFanart;
            }
            else if (type.Equals("Picture User", StringComparison.CurrentCulture))
            {
                return HTAnyPictureFanart;
            }
            else if (type.Equals("ScoreCenter User", StringComparison.CurrentCulture))
            {
                return HTAnyScorecenter;
            }
            else if (type.Equals("MovingPicture", StringComparison.CurrentCulture))
            {
                return HTAnyMovingPicturesFanart;
            }
            else if (type.Equals("TVSeries", StringComparison.CurrentCulture))
            {
                return HTAnyTVSeries;
            }
            else if (type.Equals("TV User", StringComparison.CurrentCulture))
            {
                return HTAnyTVFanart;
            }
            else if (type.Equals("Plugin User", StringComparison.CurrentCulture))
            {
                return HTAnyPluginFanart;
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
            if (type.Equals("Game User", StringComparison.CurrentCulture))
            {
                HTAnyGameFanart = ht;
            }
            else if (type.Equals("Movie User", StringComparison.CurrentCulture))
            {
                HTAnyMovieFanart = ht;
            }
            else if (type.Equals("Movie Scraper", StringComparison.CurrentCulture))
            {
                HTAnyMovieFanart = ht;
            }
            else if (type.Equals("MusicAlbum", StringComparison.CurrentCulture))
            {
                HTAnyMusicFanart = ht;
            }
            else if (type.Equals("MusicArtist", StringComparison.CurrentCulture))
            {
                HTAnyMusicFanart = ht;
            }
            else if (type.Equals("MusicFanart User", StringComparison.CurrentCulture))
            {
                HTAnyMusicFanart = ht;
            }
            else if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
            {
                HTAnyMusicFanart = ht;
            }
            else if (type.Equals("Default", StringComparison.CurrentCulture))
            {
                HTAnyMusicFanart = ht;
            }
            else if (type.Equals("Picture User", StringComparison.CurrentCulture))
            {
                HTAnyPictureFanart = ht;
            }
            else if (type.Equals("ScoreCenter User", StringComparison.CurrentCulture))
            {
                HTAnyScorecenter = ht;
            }
            else if (type.Equals("MovingPicture", StringComparison.CurrentCulture))
            {
                HTAnyMovingPicturesFanart = ht;
            }
            else if (type.Equals("TVSeries", StringComparison.CurrentCulture))
            {
                HTAnyTVSeries = ht;
            }
            else if (type.Equals("TV User", StringComparison.CurrentCulture))
            {
                HTAnyTVFanart = ht;
            }
            else if (type.Equals("Plugin User", StringComparison.CurrentCulture))
            {
                HTAnyPluginFanart = ht;
            }
        }

        /// <summary>
        /// Returns all random fanart for a specific type (like music or movies). First time builds hashtable, 
        /// then only returns that hashtable.
        /// </summary>
        /// <param name="type">The type to run the query on</param>
        /// <param name="types">Part of the sql statement</param>
        /// <returns>A hashtable with random fanart</returns>
        public Hashtable GetAnyFanart(string type, string types, int restricted)
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
                    string sRestricted = string.Empty;
                    if (restricted == 1)
                    {
                        sRestricted = "AND (restricted = 0 OR restricted = 1)";
                    }
                    else
                    {
                        sRestricted = "AND restricted = 0";
                    }
                    ht = new Hashtable();
                    string sqlQuery;
                    if (types != null && types.Length > 0)
                    {
                        sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Enabled = 'True' AND Type IN (" + types + ") " + sRestricted + ";";
                    }
                    else
                    {
                        sqlQuery = "SELECT Id, Artist, Disk_Image, Source_Image, Type, Source FROM " + GetTableName(type) + " WHERE Enabled = 'True' AND Type = '" + Utils.PatchSql(type) + "' " + sRestricted + ";";
                    }
                    SQLiteResultSet result;
                    lock (lockObject) result = dbClient.Execute(sqlQuery);

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
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="sourceImage">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadFanart(string artist, string diskImage, string sourceImage, string type, int restricted)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM " + GetTableName(type) + " WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "SELECT Restricted FROM " + GetTableName(type) + " WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                    SQLiteResultSet result;
                    lock (lockObject) result = dbClient.Execute(sqlQuery);
                    string sRestricted = result.GetField(0, 0);
                    if (!sRestricted.Equals(restricted.ToString(CultureInfo.CurrentCulture), StringComparison.CurrentCulture))
                    {
                        sqlQuery = "UPDATE " + GetTableName(type) + " set Restricted = " + restricted + " WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                        lock (lockObject) dbClient.Execute(sqlQuery);
                    }
                }
                else
                {
                    sqlQuery = "INSERT INTO " + GetTableName(type) + " (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp, Restricted) VALUES(null, '" + Utils.PatchSql(artist) + "','" + Utils.PatchSql(diskImage) + "','" + Utils.PatchSql(sourceImage) + "','" + Utils.PatchSql(type) + "','www.htbackdrops.com', 'True', '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "'," + restricted + ");";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing local fanart into fanart handler database (" + diskImage + ").");
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
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="sourceImage">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadFanartExternal(string artist, string diskImage, string sourceImage, string type, int restricted)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Movie_Fanart WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "SELECT Restricted FROM Movie_Fanart WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                    SQLiteResultSet result;
                    lock (lockObject) result = dbClient.Execute(sqlQuery);
                    string sRestricted = result.GetField(0, 0);
                    if (!sRestricted.Equals(restricted.ToString(CultureInfo.CurrentCulture), StringComparison.CurrentCulture))
                    {
                        sqlQuery = "UPDATE Movie_Fanart set Restricted = " + restricted + " WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";
                        lock (lockObject) dbClient.Execute(sqlQuery);
                    }
                }
                else
                {
                    sqlQuery = "INSERT INTO Movie_Fanart (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp, Restricted) VALUES(null, '" + Utils.PatchSql(artist) + "','" + Utils.PatchSql(diskImage) + "','" + Utils.PatchSql(sourceImage) + "','" + Utils.PatchSql(type) + "','" + Utils.PatchSql(type) + "', 'True', '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "'," + restricted + ");";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing external fanart into fanart handler database (" + diskImage + ").");
                }
            }
            catch (Exception ex)
            {
                logger.Error("LoadFanartExternal: " + ex.ToString());
            }
        }

        /// <summary>
        /// Returns if an image exist in the database or not.
        /// </summary>
        /// <param name="artist">The artist name</param>
        /// <param name="sourceImage">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        /// <returns>Returns if an image exists or not</returns>
        public bool SourceImageExist(string artist, string sourceImage)
        {
            try
            {
                string sqlQuery = String.Empty;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(artist) + "' AND SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "';";

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
        /// <param name="diskImage">Filename on disk</param>
        /// <param name="sourceImage">Filename at source</param>
        /// <param name="type">The type to run the query on</param>
        public void LoadMusicFanart(string artist, string diskImage, string sourceImage, string type, int restricted)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Fanart WHERE Artist = '" + Utils.PatchSql(artist) + "' AND (SOURCE_IMAGE = '" + Utils.PatchSql(sourceImage) + "' OR DISK_IMAGE = '" + Utils.PatchSql(diskImage) + "');";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do not allow updates
                }
                else
                {
                    sqlQuery = "INSERT INTO Music_Fanart (Id, Artist, Disk_Image, Source_Image, Type, Source, Enabled, Time_Stamp, Restricted) VALUES(null, '" + Utils.PatchSql(artist) + "','" + Utils.PatchSql(diskImage) + "','" + Utils.PatchSql(sourceImage) + "','" + Utils.PatchSql(type) + "','www.htbackdrops.com','True','" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "'," + restricted + ");";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                    logger.Debug("Importing local fanart into fanart handler database (" + diskImage + ").");
                    if (type.Equals("MusicFanart Scraper", StringComparison.CurrentCulture))
                    {
                        sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                        if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                        {
                            //DO NOTHING
                        }
                        else
                        {
                            sqlQuery = "INSERT INTO Music_Artist (Id, Artist, Successful_Scrape, Time_Stamp) VALUES(null, '" + Utils.PatchSql(artist) + "',0,'" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "');";
                            lock (lockObject) dbClient.Execute(sqlQuery);
                        }
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
        public void InsertNewMusicArtist(string artist)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    //do nothing
                }
                else
                {
                    sqlQuery = "INSERT INTO Music_Artist (Id, Artist, Successful_Scrape, Time_Stamp) VALUES(null, '" + Utils.PatchSql(artist) + "',0,'" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "');";
                }

                lock (lockObject) dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("InsertNewMusicArtist: " + ex.ToString());
            }
        }

        /// <summary>
        /// Flags an artist as being done with the thumb scrape.
        /// </summary>
        /// <param name="artist">The artist name</param>
        public void SetSuccessfulScrapeThumb(string artist, int value)
        {
            try
            {
                string sqlQuery = String.Empty;
                DateTime saveNow = DateTime.Now;
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "UPDATE Music_Artist SET Successful_Thumb_Scrape = " + value + ", Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "' WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                    lock (lockObject) dbClient.Execute(sqlQuery);
                }
                else
                {
                    //do not allow insert                
                }
            }
            catch (Exception ex)
            {
                logger.Error("SetSuccessfulScrapeThumb: " + ex.ToString());
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
                sqlQuery = "SELECT COUNT(Artist) FROM Music_Artist WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                if (DatabaseUtility.GetAsInt(dbClient.Execute(sqlQuery), 0, 0) > 0)
                {
                    sqlQuery = "UPDATE Music_Artist SET Successful_Scrape = 1, Time_Stamp = '" + saveNow.ToString(@"yyyyMMdd", CultureInfo.CurrentCulture) + "' WHERE Artist = '" + Utils.PatchSql(artist) + "';";
                    lock (lockObject) dbClient.Execute(sqlQuery);
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


    }


}
