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
        public Hashtable htAnyTVSeries;
        public Hashtable htAnyTVFanart;
        public Hashtable htAnyPluginFanart;
        private ArrayList musicDatabaseArtists;
        private MusicDatabase m_db = null;
        private int iMax;
        private string scraperMusicPlaying = null;
        private string scraperMPDatabase = null;
        private bool isScraping = false;
        private Scraper scraper;

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
        public void initDB(int iMax, string scraperMPDatabase, string scraperMusicPlaying)
        {
            try
            {
                this.iMax = iMax;
                this.scraperMPDatabase = scraperMPDatabase;
                this.scraperMusicPlaying = scraperMusicPlaying;
                this.isScraping = false;
                String path = Config.GetFile(Config.Dir.Database, dbFilename);
                setupDatabase();
                dbClient = new SQLiteClient(path);
                dbClient.Execute("PRAGMA synchronous=OFF");
                m_db = MusicDatabase.Instance;
                logger.Debug("Successfully Opened Database: " + dbFilename);
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
                    File.Move(pathOrg, path);
                }
                //delete org database
                if (File.Exists(pathOrg))
                {
                    File.Delete(pathOrg);
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
                dbClient.Close();
                dbClient = null;
            }
            catch (Exception ex)
            {
                logger.Error("close: " + ex.ToString());
            }
        }

        /// <summary>
        /// Performs a scrape for artist now being played in MediaPortal.
        /// </summary>
        public bool NowPlayingScrape(string artist, FanartHandler.Class1.ScraperWorkerNowPlaying swnp)
        {
            try
            {
                isScraping = true;
                logger.Info("NowPlayingScrape is starting for artist " + artist + ".");
                if (doScrape(artist, false, false, swnp) > 0)
                {
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
            try
            {
                scraper = new Scraper();
                scraper.getNewImages(iMax, this);
                scraper = null;
            }
            catch (Exception ex)
            {
                logger.Error("doNewScrape: " + ex.ToString());
            }
        }

        /// <summary>
        /// Deletes any entries in the fanarthandler database when the disk_image
        /// is missing on the harddrive.
        /// </summary>
        public int syncDatabase()
        {
            int i = 0;
            try
            {            
                string filename;
                string sqlQuery = "SELECT Disk_Image FROM Music_Fanart;";
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
        public int doScrape (string artist, bool useSuccessfulScrape, bool useStopScraper, FanartHandler.Class1.ScraperWorkerNowPlaying swnp)
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
                    int maxScrapes = iMax - iTmp;
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
                    lock (dbClient) dbClient.Execute("COMMIT;");
                    scraper = null;
                    return totalImages;
                }
            }
            catch (Exception ex)
            {
                lock (dbClient) dbClient.Execute("ROLLBACK;");
                logger.Error("doScrape: " + ex.ToString());
            }
            return 0;
        }

        /// <summary>
        /// Performs the intitial scrape (on htbackdrops.com) for any artist in the MP music
        /// database until max images per artist is meet or no more images exist for the artist.
        /// </summary>        
        public void InitialScrape(FanartHandler.Class1.ScraperWorker sw)
        {
            try
            {
                logger.Info("InitialScrape is starting...");
                bool firstRun = true;
                isScraping = true;                
                musicDatabaseArtists = new ArrayList();
                m_db.GetAllArtists(ref musicDatabaseArtists);                
                string artist;
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
            if (htAnyMusicFanart.Count < 1)
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
        public void EnableFanart(string disk_image, bool action)
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
                logger.Error("EnableFanart: " + ex.ToString());
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
        /// Returns all data used by datagridview in the "Scraper Settings" tab (In MP configuration).
        /// </summary>
        public SQLiteResultSet getDataForTable()
        {
            SQLiteResultSet result = null;
            try
            {
                string sqlQuery = "SELECT Artist, Enabled, Disk_Image FROM Music_Fanart WHERE Type = 'MusicFanart' order by Artist, Disk_Image;";
                result = dbClient.Execute(sqlQuery);
            }
            catch (Exception ex)
            {
                logger.Error("getDataForTable: " + ex.ToString());
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
            else if (type.Equals("Picture"))
                return htAnyPictureFanart;
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
            else if (type.Equals("Picture"))
                htAnyPictureFanart = ht;
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
                source_image = source_image.Replace("'", "''");
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
                disk_image = disk_image.Replace("'", "''");
                source_image = source_image.Replace("'", "''");
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
