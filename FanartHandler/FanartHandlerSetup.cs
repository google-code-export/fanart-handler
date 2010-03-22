using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using System.Threading;
using MediaPortal.Configuration;
using System.Collections;
using System.IO;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.TagReader;
using System.Xml;
using System.Xml.XPath;
using System.Drawing;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Reflection;


namespace FanartHandler
{
    [PluginIcons("FanartHandler.FanartHandler_Icon.png", "FanartHandler.FanartHandler_Icon_Disabled.png")]
    public class FanartHandlerSetup : IPlugin, ISetupForm
    {
        #region declarations
        
        /*
         * Log declarations
         */ 
        private static Logger logger = LogManager.GetCurrentClassLogger();  //log
        private const string logFileName = "fanarthandler.log";  //log's filename
        private const string oldLogFileName = "fanarthandler.old.log";  //log's old filename

        /*
         * All Threads and Timers
         */ 
        private ScraperWorker scraperWorkerObject;
        private ScraperWorkerNowPlaying scraperWorkerObjectNowPlaying;
        private Thread scrapeWorkerThread;
        private Thread scrapeWorkerThreadNowPlaying;
        private System.Timers.Timer refreshTimer = null;
        public static System.Timers.Timer updateTimer = null;
        private TimerCallback myDirectoryTimer = null;
        private System.Threading.Timer directoryTimer = null;
        private TimerCallback myScraperTimer = null;
        private ThreadPriority threadPriority = ThreadPriority.BelowNormal;
        private System.Threading.Timer scraperTimer = null;
        private TimerCallback myProgressTimer = null;
        private System.Threading.Timer progressTimer = null;

        public static string m_CurrentTrackTag = null;  //is music playing and if so this holds current artist name                
        private bool isPlaying = false; //hold true if MP plays music
        private bool isSelectedMusic = false; 
        private bool isSelectedVideo = false;
        private bool isSelectedScoreCenter = false;
        private bool isRandom = false;
        private bool isActivatingWindow = false;
        public static Hashtable defaultBackdropImages;  //used to hold all the default backdrop images        
        private static Random randDefaultBackdropImages = null;  //For getting random default backdrops
        private FanartHandlerConfig xconfig = null;
        public static string scraperMaxImages = null;  // Holds info read from fanarthandler.xml settings file
        public static string scraperMusicPlaying = null;  // Holds info read from fanarthandler.xml settings file
        public static string scraperMPDatabase = null;  // Holds info read from fanarthandler.xml settings file
        private string scraperInterval = null;  // Holds info read from fanarthandler.xml settings file
        public static string useArtist = null;  // Holds info read from fanarthandler.xml settings file
        public static string useAlbum = null;  // Holds info read from fanarthandler.xml settings file
        private static string skipWhenHighResAvailable = null;  // Holds info read from fanarthandler.xml settings file
        public static string disableMPTumbsForRandom = null;  // Holds info read from fanarthandler.xml settings file
        private string defaultBackdropIsImage = null;  // Holds info read from fanarthandler.xml settings file
        public static string useFanart = null;  // Holds info read from fanarthandler.xml settings file
        public static string useOverlayFanart = null;  // Holds info read from fanarthandler.xml settings file
        private string useMusicFanart = null;  // Holds info read from fanarthandler.xml settings file
        private string useVideoFanart = null;  // Holds info read from fanarthandler.xml settings file
        private string useScoreCenterFanart = null;  // Holds info read from fanarthandler.xml settings file
        private string imageInterval = null;  // Holds info read from fanarthandler.xml settings file
        private static string minResolution = null;  // Holds info read from fanarthandler.xml settings file
        private string defaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        public static string useAspectRatio = null;  // Holds info read from fanarthandler.xml settings file
        private static string useDefaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        private string useProxy = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyHostname = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyPort = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyUsername = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyPassword = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyDomain = null;  // Holds info read from fanarthandler.xml settings file
        public static MusicDatabase m_db = null;  //handle to MP Music database                
        public static int maxCountImage = 30;          
        public static string m_SelectedItem = null; //artist, album, title        
        private static FanartPlaying fp = null;
        private static FanartSelected fs = null;
        private static FanartRandom fr = null;
        private static int syncPointRefresh = 0;
        private static int syncPointUpdate = 0;
        #endregion  
        

        private void HandleOldImages(ref ArrayList al)
        {
            try
            {
                if (al != null && al.Count > 1)
                {
                    int i = 0;
                    while (i < (al.Count - 1))
                    {
                        //unload old image to free MP resource
                        UnLoadImage(al[i].ToString());

                        //remove old no longer used image
                        al.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("HandleOldImages: " + ex.ToString());
            }
        }

        public static void EmptyAllImages(ref ArrayList al)
        {
            try
            {
                if (al != null)
                {
                    foreach (Object obj in al)
                    {
                        //unload old image to free MP resource
                        UnLoadImage(obj.ToString());
                    }
                    //remove old no longer used image
                    al.Clear();
                }
            }
            catch (Exception ex)
            {
                //do nothing
                logger.Error("EmptyAllImages: " + ex.ToString());
            }
        }
        

        

        public static void SetProperty(string property, string value)
        {
            try
            {
                if (property == null)
                    return;
                if (String.IsNullOrEmpty(value))
                    value = " ";

                GUIPropertyManager.SetProperty(property, value);
            }
            catch (Exception ex)
            {
                logger.Error("SetProperty: " + ex.ToString());
            }
        }
       

        /// <summary>
        /// Check if minimum resolution is used
        /// </summary>
        public static bool checkImageResolution(string filename, string type, string useAspectRatio)
        {
            try
            {
                if (File.Exists(filename) == false)
                {
                    Utils.GetDbm().DeleteFanart(filename, type);
                    return false;
                }
                Image checkImage = Image.FromFile(filename);
                double mWidth = Convert.ToInt32(minResolution.Substring(0, minResolution.IndexOf("x")));
                double mHeight = Convert.ToInt32(minResolution.Substring(minResolution.IndexOf("x") + 1));
                double imageWidth = checkImage.Width;
                double imageHeight = checkImage.Height;
                checkImage.Dispose();
                checkImage = null;
                if (imageWidth >= mWidth && imageHeight >= mHeight)
                {
                    if (useAspectRatio.Equals("True"))
                    {
                        if (imageHeight > 0 && ((imageWidth / imageHeight) >= 1.3))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CheckImageResolution: " + ex.ToString());
            }
            return false;
        }


        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        public void SetupFilenames(string s, string filter, ref int i, string type)
        {
            string artist = "";
            string typeOrg = type;
            try
            {
                // Process the list of files found in the directory
                string[] dirs = Directory.GetFiles(s, filter);
                foreach (string dir in dirs)
                {
                    try
                    {
                        try
                        {
                            artist = Utils.GetArtist(dir, type);
                            if (type.Equals("MusicAlbum") || type.Equals("MusicArtist") || type.Equals("MusicFanart"))
                            {
                                if (Utils.GetFilenameNoPath(dir).ToLower().StartsWith("default"))
                                {
                                    type = "Default";
                                }
                                Utils.GetDbm().loadMusicFanart(artist, dir, dir, type);
                                type = typeOrg;
                            }
                            else
                            {
                                Utils.GetDbm().loadFanart(artist, dir, dir, type);                                
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("SetupFilenames: " + ex.ToString());
                        }
                        i++;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("SetupFilenames: " + ex.ToString());
                    }
                }
                // Recurse into subdirectories of this directory.
                string[] subdirs = Directory.GetDirectories(s);
                foreach (string subdir in subdirs)
                {
                    SetupFilenames(subdir, filter, ref i, type);
                }

            }
            catch (Exception ex)
            {
                logger.Error("SetupFilenames: " + ex.ToString());                
            }
        }

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        public void SetupDefaultBackdrops(string startDir, ref int i)
        {
            if (useDefaultBackdrop.Equals("True"))
            {
                try
                {
                    // Process the list of files found in the directory
                    var files = Directory.GetFiles(startDir, "*.*").Where(s => s.EndsWith(".jpg") || s.EndsWith(".jpeg") || s.EndsWith(".png"));
                    foreach (string file in files)
                    {
                        try
                        {
                            try
                            {
                                defaultBackdropImages.Add(i, file);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                        }
                        i++;
                    }
                    // Recurse into subdirectories of this directory.
                    string[] subdirs = Directory.GetDirectories(startDir);
                    foreach (string subdir in subdirs)
                    {
                        SetupDefaultBackdrops(subdir, ref i);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error("SetupDefaultBackdrops: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Update the filenames keept by the plugin if files are added since start of MP
        /// </summary>
        public void UpdateDirectoryTimer()
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    //Add games images
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                    int i = 0;
                    if (fr.useAnyGames)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "Game");
                    }
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                    i = 0;
                    if (useVideoFanart.Equals("True") || fr.useAnyMovies)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "Movie");
                    }
                    //Add music images
                    path = "";
                    i = 0;
                    if (useAlbum.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                        SetupFilenames(path, "*L.jpg", ref i, "MusicAlbum");
                    }
                    if (useArtist.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                        SetupFilenames(path, "*L.jpg", ref i, "MusicArtist");
                    }
                    if (useFanart.Equals("True") || fr.useAnyMusic)
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                        SetupFilenames(path, "*.jpg", ref i, "MusicFanart");
                    }
                    //Add pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                    i = 0;
                    if (fr.useAnyPictures)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "Picture");
                    }
                    //Add games images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                    i = 0;
                    if (useScoreCenterFanart.Equals("True") || fr.useAnyScoreCenter)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "ScoreCenter");
                    }
                    //Add moving pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\MovingPictures\Backdrops\FullSize";
                    i = 0;
                    if (fr.useAnyMovingPictures)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "MovingPicture");
                    }
                    //Add tvseries images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    i = 0;
                    if (fr.useAnyTVSeries)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "TVSeries");
                    }
                    //Add tv images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                    i = 0;
                    if (fr.useAnyTV)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "TV");
                    }
                    //Add plugins images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                    i = 0;
                    if (fr.useAnyPlugins)
                    {
                        SetupFilenames(path, "*.jpg", ref i, "Plugin");
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateDirectoryTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Update the filenames keept by the plugin if files are added since start of MP
        /// </summary>
        public void UpdateDirectoryTimer(Object stateInfo)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    UpdateDirectoryTimer();
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateDirectoryTimer: " + ex.ToString());
                }
            }
        }

        public void UpdateScraperTimer(Object stateInfo)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    if (scraperMPDatabase != null && scraperMPDatabase.Equals("True") && Utils.GetDbm().GetIsScraping() == false)
                    {
                        startScraper();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateScraperTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        public static string GetFilename(string key, ref string currFile, ref int iFilePrev, string type, string obj, bool newArtist)
        {
            string sout = currFile;
            try
            {                
                if (Utils.GetIsStopping() == false)
                {
                    key = Utils.GetArtist(key, type);
                    Hashtable tmp = null;
                    if (obj.Equals("FanartPlaying"))
                    {
                        tmp = fp.GetCurrentArtistsImageNames();
                    }
                    else
                    {
                        tmp = fs.GetCurrentArtistsImageNames();
                    }
                    if (newArtist || tmp == null || tmp.Count == 0)
                    {
                        if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Equals("True"))
                        {
                            tmp = Utils.GetDbm().getHigResFanart(key, type);
                        }
                        if (tmp != null && tmp.Count > 0)
                        {
                            //do nothing
                        }
                        else
                        {
                            tmp = Utils.GetDbm().getFanart(key, type);
                        }
                        Utils.Shuffle(ref tmp);
                        if (obj.Equals("FanartPlaying"))
                        {
                            fp.SetCurrentArtistsImageNames(tmp);
                        }
                        else
                        {
                            fs.SetCurrentArtistsImageNames(tmp);
                        }
                    }
                    if (tmp != null && tmp.Count > 0)
                    {
                        ICollection valueColl = tmp.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (DatabaseManager.FanartImage s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (checkImageResolution(s.disk_image, type, useAspectRatio) && Utils.IsFileValid(s.disk_image))
                                {
                                    sout = s.disk_image;
                                    iFilePrev = iFile;
                                    currFile = s.disk_image;
                                    iStop = 1;
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = tmp.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (DatabaseManager.FanartImage s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (checkImageResolution(s.disk_image, type, useAspectRatio))
                                    {
                                        sout = s.disk_image;
                                        iFilePrev = iFile;
                                        currFile = s.disk_image;
                                        iStop = 1;
                                        break;
                                    }
                                }
                                iFile++;
                            }
                        }
                        valueColl = null;
                    }
                }
                else
                {
                    if (obj.Equals("FanartPlaying"))
                    {
                        fp.SetCurrentArtistsImageNames(null);
                    }
                    else
                    {
                        fs.SetCurrentArtistsImageNames(null);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetFilename: " + ex.ToString());
            }
            return sout;
        }

        

          /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        public static string GetRandomDefaultBackdrop()
        {
            string sout = "";
            try
            {
                if (Utils.GetIsStopping() == false && useDefaultBackdrop.Equals("True"))
                {
                    if (defaultBackdropImages != null && defaultBackdropImages.Count > 0)
                    {
                        bool doRun = true;
                        int attempts = 0;
                        while (doRun && attempts < (defaultBackdropImages.Count * 2))
                        {
                            int iHt = randDefaultBackdropImages.Next(0, defaultBackdropImages.Count);
                            if (checkImageResolution((defaultBackdropImages[iHt].ToString()), "MusicFanart", useAspectRatio) && Utils.IsFileValid(defaultBackdropImages[iHt].ToString()))
                            {
                                sout = defaultBackdropImages[iHt].ToString();
                                doRun = false;
                            }
                            attempts++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRandomDefaultBackdrop: " + ex.ToString());
            }
            return sout;
        }

        /// <summary>
        /// Parse Music Artist Name
        /// </summary>
        private string ParseArtist(string s)
        {
            try
            {
                if (s != null && s.Length > 0 && s.IndexOf(".") >= 0)
                {
                    s = s.Trim();
                    s = s.Substring(s.IndexOf(".") + 1);
                    s = s.Trim();
                }
                if (s != null && s.Length > 0 && s.IndexOf("-") >= 0)
                {
                    s = s.Trim();
                    s = s.Substring(0, s.IndexOf("-"));
                    s = s.Trim();
                }
            }
            catch (Exception ex)
            {
                logger.Error("ParseArtist: " + ex.ToString());  
            }
            return s;
        }

        private void ResetCounters()
        {
            if (fs.currCount > maxCountImage)
            {
                fs.currCount = 0;
                fs.hasUpdatedCurrCount = false;
            }
            if (fs.updateVisibilityCount > 5)
            {
                fs.updateVisibilityCount = 1;
            }
            if (fp.currCountPlay > maxCountImage)
            {
                fp.currCountPlay = 0;
                fp.hasUpdatedCurrCountPlay = false;
            }
            if (fp.updateVisibilityCountPlay > 5)
            {
                fp.updateVisibilityCountPlay = 1;
            }
            /*if (fr.currCountRandom > maxCountImage)
            {
                fr.currCountRandom = 0;
            }
            if (fr.updateVisibilityCountRandom > 5)
            {
                fr.updateVisibilityCountRandom = 1;
            }
            */
        }

        /// <summary>
        /// Update visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void UpdateDummyControls()
        {
            try
            {
                //something has gone wrong
                ResetCounters();
                int windowId = GUIWindowManager.ActiveWindow;
                if (fs.updateVisibilityCount == 2)  //after 2 sek
                {
                    fs.UpdateProperties();
                    if (fs.doShowImageOne)
                    {
                        fs.ShowImageOne(windowId);
                        fs.doShowImageOne = false;
                    }
                    else
                    {
                        fs.ShowImageTwo(windowId);
                        fs.doShowImageOne = true;
                    }
                    if (fs.fanartAvailable)
                    {
                        fs.FanartIsAvailable(windowId);
                    }
                    else
                    {
                        fs.FanartIsNotAvailable(windowId);
                    }                    
                }
                else if (fs.updateVisibilityCount == 5) //after 4 sek
                {
                    fs.updateVisibilityCount = 0;
                    //release unused image resources
                    HandleOldImages(ref fs.listSelectedMovies);
                    HandleOldImages(ref fs.listSelectedMusic);
                    HandleOldImages(ref fs.listSelectedScorecenter);
                }
                if (fp.updateVisibilityCountPlay == 2)  //after 2 sek
                {
                    fp.UpdatePropertiesPlay();
                    if (fp.doShowImageOnePlay)
                    {
                        fp.ShowImageOnePlay(windowId);
                        fp.doShowImageOnePlay = false;
                    }
                    else
                    {
                        fp.ShowImageTwoPlay(windowId);
                        fp.doShowImageOnePlay = true;
                    }
                    if (fp.fanartAvailablePlay)
                    {
                        fp.FanartIsAvailablePlay(windowId);
                    }
                    else
                    {
                        fp.FanartIsNotAvailablePlay(windowId);
                    }
                }
                else if (fp.updateVisibilityCountPlay == 5) //after 4 sek
                {
                    fp.updateVisibilityCountPlay = 0;
                    //release unused image resources
                    HandleOldImages(ref fp.listPlayMusic);
                }
                if (fr.windowOpen == false)
                {
                    if (fr.updateVisibilityCountRandom == 2)  //after 2 sek
                    {
                        fr.UpdatePropertiesRandom();
                        if (fr.doShowImageOneRandom)
                        {
                            fr.ShowImageOneRandom(windowId);
                            fr.doShowImageOneRandom = false;
                        }
                        else
                        {
                            fr.ShowImageTwoRandom(windowId);
                            fr.doShowImageOneRandom = true;
                        }
                    }
                    else if (fr.updateVisibilityCountRandom == 5) //after 4 sek                    
                    {
                        fr.countSetVisibility = 0;
                        fr.updateVisibilityCountRandom = 0;
                        //release unused image resources
                        HandleOldImages(ref fr.listAnyGames);
                        HandleOldImages(ref fr.listAnyMovies);
                        HandleOldImages(ref fr.listAnyMovingPictures);
                        HandleOldImages(ref fr.listAnyMusic);
                        HandleOldImages(ref fr.listAnyPictures);
                        HandleOldImages(ref fr.listAnyScorecenter);
                        HandleOldImages(ref fr.listAnyPlugins);
                        HandleOldImages(ref fr.listAnyTV);
                        HandleOldImages(ref fr.listAnyTVSeries);
                    }
                }
/*                
                logger.Debug("listAnyGames: " + fr.listAnyGames.Count);
                logger.Debug("listAnyMovies: " + fr.listAnyMovies.Count);
                logger.Debug("listAnyMovingPictures: " + fr.listAnyMovingPictures.Count);
                logger.Debug("listAnyMusic: " + fr.listAnyMusic.Count);
                logger.Debug("listAnyPictures: " + fr.listAnyPictures.Count);
                logger.Debug("listAnyScorecenter: " + fr.listAnyScorecenter.Count);
                logger.Debug("listAnyTVSeries: " + fr.listAnyTVSeries.Count);
                logger.Debug("listAnyTV: " + fr.listAnyTV.Count);
                logger.Debug("listAnyPlugins: " + fr.listAnyPlugins.Count); 
  */              
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDummyControls: " + ex.ToString());
            }
        }

        /// <summary>
        /// Update visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void UpdateDummyControlsRandom()
        {
            try
            {
                int windowId = GUIWindowManager.ActiveWindow;
                if (fr.countSetVisibility == 1 && fr.GetPropertiesRandom() > 0)  //after 2 sek
                {
                    fr.countSetVisibility = 2;
                    fr.UpdatePropertiesRandom();
                    if (fr.doShowImageOneRandom)
                    {
                        fr.ShowImageOneRandom(windowId);
                    }
                    else
                    {
                        fr.ShowImageTwoRandom(windowId);
                    }
                }
                else if (fr.updateVisibilityCountRandom > 2 && fr.GetPropertiesRandom() > 0) //after 2 sek
                {
                    fr.UpdatePropertiesRandom();
                }
                //else if (fr.updateVisibilityCountRandom == 5) //after 4 sek
                else if (fr.updateVisibilityCountRandom >= 5 && fr.GetPropertiesRandom() == 0) //after 4 sek
                {
                    if (updateTimer != null && updateTimer.Enabled)
                    {
                        updateTimer.Stop();
                    }                    
                    if (fr.doShowImageOneRandom)
                    {
                        fr.doShowImageOneRandom = false;
                    }
                    else
                    {
                        fr.doShowImageOneRandom = true;
                    }
                    fr.countSetVisibility = 0;
                    fr.updateVisibilityCountRandom = 0;
                    //release unused image resources
                    HandleOldImages(ref fr.listAnyGames);
                    HandleOldImages(ref fr.listAnyMovies);
                    HandleOldImages(ref fr.listAnyMovingPictures);
                    HandleOldImages(ref fr.listAnyMusic);
                    HandleOldImages(ref fr.listAnyPictures);
                    HandleOldImages(ref fr.listAnyScorecenter);
                    HandleOldImages(ref fr.listAnyPlugins);
                    HandleOldImages(ref fr.listAnyTV);
                    HandleOldImages(ref fr.listAnyTVSeries);
                    fr.windowOpen = false;
/*                    logger.Debug("listAnyGames: " + fr.listAnyGames.Count);
                    logger.Debug("listAnyMovies: " + fr.listAnyMovies.Count);
                    logger.Debug("listAnyMovingPictures: " + fr.listAnyMovingPictures.Count);
                    logger.Debug("listAnyMusic: " + fr.listAnyMusic.Count);
                    logger.Debug("listAnyPictures: " + fr.listAnyPictures.Count);
                    logger.Debug("listAnyScorecenter: " + fr.listAnyScorecenter.Count);
                    logger.Debug("listAnyTVSeries: " + fr.listAnyTVSeries.Count);
                    logger.Debug("listAnyTV: " + fr.listAnyTV.Count);
                    logger.Debug("listAnyPlugins: " + fr.listAnyPlugins.Count);
  */                  

                }
                
                
                
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDummyControlsRandom: " + ex.ToString());
            }
        }

        
        

 

       

        /// <summary>
        /// Get value from xml node
        /// </summary>
        private string getNodeValue(XPathNodeIterator myXPathNodeIterator)
        {
            if (myXPathNodeIterator.Count > 0)
            {
                myXPathNodeIterator.MoveNext();
                return myXPathNodeIterator.Current.Value;
            }
            return "";
        }
 
        private void setupWindowsUsingFanartHandlerVisibility()
        {
            XPathDocument myXPathDocument;
            fs.windowsUsingFanartSelected = new Hashtable();
            fp.windowsUsingFanartPlay = new Hashtable();
            string path = GUIGraphicsContext.Skin + @"\";
            string windowId = "";
            string sNodeValue = "";
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            string s = "";
            foreach (FileInfo fi in rgFiles)
            {
                try
                {
                    s = fi.Name;
                    myXPathDocument = new XPathDocument(fi.FullName);
                    XPathNavigator myXPathNavigator = myXPathDocument.CreateNavigator();
                    XPathNodeIterator myXPathNodeIterator = myXPathNavigator.Select("/window/id");
                    windowId = getNodeValue(myXPathNodeIterator);
                    if (windowId != null && windowId.Length > 0)
                    {
                        myXPathNodeIterator = myXPathNavigator.Select("/window/define");
                        if (myXPathNodeIterator.Count > 0)
                        {
                            while (myXPathNodeIterator.MoveNext())
                            {
                                sNodeValue = myXPathNodeIterator.Current.Value;
                                if (sNodeValue.StartsWith("#useSelectedFanart:Yes"))
                                {
                                    try
                                    {
                                        fs.windowsUsingFanartSelected.Add(windowId, windowId);
                                    }
                                    catch { }
                                }
                                if (sNodeValue.StartsWith("#usePlayFanart:Yes"))
                                {
                                    try
                                    {
                                        fp.windowsUsingFanartPlay.Add(windowId, windowId);
                                    }
                                    catch { }
                                }                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("setupWindowsUsingFanartHandlerVisibility, filename:" + s + "): " + ex.ToString());
                }
            }
        }



        public void InitRandomProperties()
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    if (fr.windowsUsingFanartRandom.ContainsKey("35"))
                    {
                        isRandom = true;
                        fr.RefreshRandomImageProperties();
                        if (fr.updateVisibilityCountRandom > 0)
                        {
                            fr.updateVisibilityCountRandom = fr.updateVisibilityCountRandom + 1;
                        }
                        UpdateDummyControls();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("InitRandomProperties: " + ex.ToString());
                }
            }            
        }

        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        public void UpdateImageTimer()
        {
            Utils.SetDelayStop(true);
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    bool resetFanartAvailableFlags = true;
                    fs.hasUpdatedCurrCount = false;
                    fp.hasUpdatedCurrCountPlay = false;
                    int windowId = GUIWindowManager.ActiveWindow;
                    m_CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                    if (scraperMPDatabase != null && scraperMPDatabase.Equals("True") && scraperWorkerObject != null && scraperWorkerObject.getRefreshFlag())
                    {
                        fs.currCount = maxCountImage;
                        fs.SetCurrentArtistsImageNames(null);
                        scraperWorkerObject.setRefreshFlag(false);
                    }
                    if (m_CurrentTrackTag != null && m_CurrentTrackTag.Length > 0 && (g_Player.Playing || g_Player.Paused))   // music is playing
                    {
                        if (scraperMusicPlaying != null && scraperMusicPlaying.Equals("True") && scraperWorkerObjectNowPlaying != null && scraperWorkerObjectNowPlaying.getRefreshFlag())
                        {
                            fp.currCountPlay = maxCountImage;
                            fp.SetCurrentArtistsImageNames(null);
                            scraperWorkerObjectNowPlaying.setRefreshFlag(false);
                        }
                        if (fp.currPlayMusicArtist.Equals(m_CurrentTrackTag) == false)
                        {
                            if (scraperMusicPlaying != null && scraperMusicPlaying.Equals("True") && Utils.GetDbm().GetIsScraping() == false && ((scrapeWorkerThreadNowPlaying != null && scrapeWorkerThreadNowPlaying.IsAlive == false) || scrapeWorkerThreadNowPlaying == null))
                            {
                                startScraperNowPlaying(m_CurrentTrackTag);
                            }
                        }
                        fp.RefreshMusicPlayingProperties();
                    }
                    else
                    {
                        if (isPlaying)
                        {
                            stopScraperNowPlaying();
                            EmptyAllImages(ref fp.listPlayMusic);
                            fp.SetCurrentArtistsImageNames(null);
                            fp.currPlayMusic = "";
                            fp.currPlayMusicArtist = "";
                            fp.fanartAvailablePlay = false;
                            fp.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                            fp.prevPlayMusic = -1;
                            SetProperty("#fanarthandler.music.overlay.play", "");
                            SetProperty("#fanarthandler.music.backdrop1.play", "");
                            SetProperty("#fanarthandler.music.backdrop2.play", "");
                            fp.currCountPlay = 0;
                            fp.updateVisibilityCountPlay = 0;
                            isPlaying = false;
                        }
                    }
                    if (useMusicFanart.Equals("True"))
                    {
                        if (windowId == 504 || windowId == 501)
                        {
                            //User are in myMusicGenres window
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshMusicSelectedProperties();
                        }
                        else if (windowId == 500)
                        {
                            //User are in music playlist
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("music", ref fs.listSelectedMusic, "Music Playlist", ref fs.currSelectedMusic, ref fs.currSelectedMusicArtist);
                        }
                        else if (windowId == 29050 || windowId == 29051 || windowId == 29052)
                        {
                            //User are in youtubefm search window
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("music", ref fs.listSelectedMusic, "Youtube.FM", ref fs.currSelectedMusic, ref fs.currSelectedMusicArtist);
                        }
                        else if (windowId == 880)
                        {
                            //User are in music videos window
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("music", ref fs.listSelectedMusic, "Music Videos", ref fs.currSelectedMusic, ref fs.currSelectedMusicArtist);
                        }
                        else if (windowId == 6623)
                        {
                            //User are in mvids window
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("music", ref fs.listSelectedMusic, "mVids", ref fs.currSelectedMusic, ref fs.currSelectedMusicArtist);
                        }
                        else if (windowId == 30885)
                        {
                            //User are in global search window
                            isSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("music", ref fs.listSelectedMusic, "Global Search", ref fs.currSelectedMusic, ref fs.currSelectedMusicArtist);
                        }
                        else
                        {
                            if (isSelectedMusic)
                            {
                                EmptyAllImages(ref fs.listSelectedMusic);
                                fs.currSelectedMusic = "";
                                fs.currSelectedMusicArtist = "";
                                fs.prevSelectedMusic = -1;
                                fs.prevSelectedGeneric = -1;
                                fs.SetCurrentArtistsImageNames(null);
                                SetProperty("#fanarthandler.music.backdrop1.selected", "");
                                SetProperty("#fanarthandler.music.backdrop2.selected", "");
                                isSelectedMusic = false;
                            }
                        }
                    }
                    if (useVideoFanart.Equals("True"))
                    {
                        if (windowId == 6 || windowId == 25)
                        {
                            //User are in myVideo, myVideoTitle window
                            isSelectedVideo = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshGenericSelectedProperties("movie", ref fs.listSelectedMovies, "myVideos", ref fs.currSelectedMovie, ref fs.currSelectedMovieTitle);
                        }
                        else
                        {
                            if (isSelectedVideo)
                            {
                                EmptyAllImages(ref fs.listSelectedMovies);
                                fs.currSelectedMovie = "";
                                fs.currSelectedMovieTitle = "";
                                fs.SetCurrentArtistsImageNames(null);
                                fs.prevSelectedGeneric = -1;
                                SetProperty("#fanarthandler.movie.backdrop1.selected", "");
                                SetProperty("#fanarthandler.movie.backdrop2.selected", "");
                                isSelectedVideo = false;
                            }
                        }
                    }
                    if (useScoreCenterFanart.Equals("True"))
                    {
                        if (windowId == 42000)  //User are in myScorecenter window
                        {
                            isSelectedScoreCenter = true;
                            resetFanartAvailableFlags = false;
                            fs.RefreshScorecenterSelectedProperties();
                        }
                        else
                        {
                            if (isSelectedScoreCenter)
                            {
                                EmptyAllImages(ref fs.listSelectedScorecenter);
                                fs.currSelectedScorecenter = "";
                                fs.currSelectedScorecenterGenre = "";
                                fs.SetCurrentArtistsImageNames(null);
                                fs.prevSelectedScorecenter = -1;
                                SetProperty("#fanarthandler.scorecenter.backdrop1.selected", "");
                                SetProperty("#fanarthandler.scorecenter.backdrop2.selected", "");
                                isSelectedScoreCenter = false;
                            }
                        }
                    }
                    if (resetFanartAvailableFlags)
                    {
                        fs.fanartAvailable = false;
                        fs.FanartIsNotAvailable(windowId);
                    }
                    if (fr.windowsUsingFanartRandom.ContainsKey(windowId.ToString()))
                    {
                        isRandom = true;                        
                        fr.RefreshRandomImageProperties();
                    }
                    else
                    {
                        if (isRandom)
                        {
                            SetProperty("#fanarthandler.games.backdrop1.any", "");
                            SetProperty("#fanarthandler.movie.backdrop1.any", "");
                            SetProperty("#fanarthandler.movingpicture.backdrop1.any", "");
                            SetProperty("#fanarthandler.music.backdrop1.any", "");
                            SetProperty("#fanarthandler.picture.backdrop1.any", "");
                            SetProperty("#fanarthandler.scorecenter.backdrop1.any", "");
                            SetProperty("#fanarthandler.tvseries.backdrop1.any", "");
                            SetProperty("#fanarthandler.tv.backdrop1.any", "");
                            SetProperty("#fanarthandler.plugins.backdrop1.any", "");
                            SetProperty("#fanarthandler.games.backdrop2.any", "");
                            SetProperty("#fanarthandler.movie.backdrop2.any", "");
                            SetProperty("#fanarthandler.movingpicture.backdrop2.any", "");
                            SetProperty("#fanarthandler.music.backdrop2.any", "");
                            SetProperty("#fanarthandler.picture.backdrop2.any", "");
                            SetProperty("#fanarthandler.scorecenter.backdrop2.any", "");
                            SetProperty("#fanarthandler.tvseries.backdrop2.any", "");
                            SetProperty("#fanarthandler.tv.backdrop2.any", "");
                            SetProperty("#fanarthandler.plugins.backdrop2.any", "");
                            fr.currCountRandom = 0;
                            EmptyAllImages(ref fr.listAnyGames);
                            EmptyAllImages(ref fr.listAnyMovies);
                            EmptyAllImages(ref fr.listAnyMovingPictures);
                            EmptyAllImages(ref fr.listAnyMusic);
                            EmptyAllImages(ref fr.listAnyPictures);
                            EmptyAllImages(ref fr.listAnyScorecenter);
                            EmptyAllImages(ref fr.listAnyTVSeries);
                            EmptyAllImages(ref fr.listAnyTV);
                            EmptyAllImages(ref fr.listAnyPlugins);
                            isRandom = false;
                        }
                    }
                    if (fs.updateVisibilityCount > 0)
                    {
                        fs.updateVisibilityCount = fs.updateVisibilityCount + 1;
                    }
                    if (fp.updateVisibilityCountPlay > 0)
                    {
                        fp.updateVisibilityCountPlay = fp.updateVisibilityCountPlay + 1;
                    }
                    if (fr.updateVisibilityCountRandom > 0)
                    {
                        fr.updateVisibilityCountRandom = fr.updateVisibilityCountRandom + 1;
                    }
                    UpdateDummyControls();
                    Utils.SetDelayStop(false);
                }
                catch (Exception ex)
                {
                    logger.Error("UpdateImageTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        public void UpdateDummyControlsRandom(Object stateInfo, ElapsedEventArgs e)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref syncPointUpdate, 1, 0);
                    if (sync == 0)
                    {
                        // No other event was executing.
                        UpdateDummyControlsRandom();

                        // Release control of syncPoint.
                        syncPointUpdate = 0;
                    }
                }
                catch (Exception ex)
                {
                    syncPointUpdate = 0;
                    logger.Error("UpdateDummyControls: " + ex.ToString());
                }
            }
        }
        

        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        public void UpdateImageTimer(Object stateInfo, ElapsedEventArgs e)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    if (!isActivatingWindow)
                    {
                        int sync = Interlocked.CompareExchange(ref syncPointRefresh, 1, 0);
                        if (sync == 0)
                        {
                            // No other event was executing.
                            UpdateImageTimer();

                            // Release control of syncPoint.
                            syncPointRefresh = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    syncPointRefresh = 0;
                    logger.Error("UpdateImageTimer: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Set start values on variables
        /// </summary>
        private void SetupVariables()
        {
            Utils.SetIsStopping(false);
            isPlaying = false;
            isSelectedMusic = false;
            isSelectedVideo = false;
            isSelectedScoreCenter = false;
            isRandom = false;
            fs.doShowImageOne = true;
            fp.doShowImageOnePlay = true;
            fr.doShowImageOneRandom = true;
            fr.firstRandom = true;
            fs.fanartAvailable = false;
            fp.fanartAvailablePlay = false;
            fs.updateVisibilityCount = 0;
            fp.updateVisibilityCountPlay = 0;
            fr.updateVisibilityCountRandom = 0;
            fs.currCount = 0;
            fp.currCountPlay = 0;
            fr.currCountRandom = 0;
            fs.ShowImageOne(35);
            fr.ShowImageOneRandom(35);
            fp.ShowImageOnePlay(35);
            fs.FanartIsNotAvailable(35);
            SetProperty("#fanarthandler.scraper.percent.completed", "");
            GUIControl.HideControl(35, 91919280);            
            maxCountImage = Convert.ToInt32(imageInterval);
            fs.hasUpdatedCurrCount = false;
            fp.hasUpdatedCurrCountPlay = false;
            fs.prevSelectedGeneric = -1;
            fp.prevPlayMusic = -1;
            fs.prevSelectedMusic = -1;
            fs.prevSelectedScorecenter = -1;
            fs.currSelectedMovieTitle = "";
            fp.currPlayMusicArtist = "";
            fs.currSelectedMusicArtist = "";
            fs.currSelectedScorecenterGenre = "";
            fs.currSelectedMovie = "";
            fp.currPlayMusic = "";
            fs.currSelectedMusic = "";
            fs.currSelectedScorecenter = "";
            SetProperty("#fanarthandler.scraper.percent.completed", "");
            SetProperty("#fanarthandler.games.backdrop1.any", "");
            SetProperty("#fanarthandler.games.backdrop2.any", "");
            SetProperty("#fanarthandler.movie.backdrop1.any", "");
            SetProperty("#fanarthandler.movie.backdrop2.any", "");
            SetProperty("#fanarthandler.movie.backdrop1.selected", "");
            SetProperty("#fanarthandler.movie.backdrop2.selected", "");
            SetProperty("#fanarthandler.movingpicture.backdrop1.any", "");
            SetProperty("#fanarthandler.movingpicture.backdrop2.any", "");
            SetProperty("#fanarthandler.music.backdrop1.any", "");
            SetProperty("#fanarthandler.music.backdrop2.any", "");
            SetProperty("#fanarthandler.music.overlay.play", "");
            SetProperty("#fanarthandler.music.backdrop1.play", "");
            SetProperty("#fanarthandler.music.backdrop2.play", "");            
            SetProperty("#fanarthandler.music.backdrop1.selected", "");
            SetProperty("#fanarthandler.music.backdrop2.selected", "");
            SetProperty("#fanarthandler.picture.backdrop1.any", "");
            SetProperty("#fanarthandler.picture.backdrop2.any", "");
            SetProperty("#fanarthandler.scorecenter.backdrop1.selected", "");
            SetProperty("#fanarthandler.scorecenter.backdrop2.selected", "");
            SetProperty("#fanarthandler.scorecenter.backdrop1.any", "");
            SetProperty("#fanarthandler.scorecenter.backdrop2.any", "");
            SetProperty("#fanarthandler.tvseries.backdrop1.any", "");
            SetProperty("#fanarthandler.tvseries.backdrop2.any", "");
            SetProperty("#fanarthandler.tv.backdrop1.any", "");
            SetProperty("#fanarthandler.tv.backdrop2.any", "");
            SetProperty("#fanarthandler.plugins.backdrop1.any", "");
            SetProperty("#fanarthandler.plugins.backdrop2.any", "");
            fs.properties = new Hashtable();
            fp.propertiesPlay = new Hashtable();
            fr.propertiesRandom = new Hashtable();
            defaultBackdropImages = new Hashtable();
            fr.listAnyGames = new ArrayList();
            fr.listAnyMovies = new ArrayList();
            fs.listSelectedMovies = new ArrayList();
            fr.listAnyMovingPictures = new ArrayList();
            fr.listAnyMusic = new ArrayList();
            fp.listPlayMusic = new ArrayList();
            fr.listAnyPictures = new ArrayList();
            fr.listAnyScorecenter = new ArrayList();
            fs.listSelectedMusic = new ArrayList();
            fs.listSelectedScorecenter = new ArrayList();
            fr.listAnyTVSeries = new ArrayList();
            fr.listAnyTV = new ArrayList();
            fr.listAnyPlugins = new ArrayList();
            fr.randAnyGames = new Random();
            fr.randAnyMovies = new Random();
            fr.randAnyMovingPictures = new Random();
            fr.randAnyMusic = new Random();
            fr.randAnyPictures = new Random();
            fr.randAnyScorecenter = new Random();
            fr.randAnyTVSeries = new Random();
            fr.randAnyTV = new Random();
            fr.randAnyPlugins = new Random();
            randDefaultBackdropImages = new Random();
        }

        /// <summary>
        /// Setup logger. This funtion made by the team behind Moving Pictures 
        /// (http://code.google.com/p/moving-pictures/)
        /// </summary>
        private void initLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            try
            {
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, logFileName));
                if (logFile.Exists)
                {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, oldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, oldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, oldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, logFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));

            string myThreadPriority = xmlreader.GetValue("general", "ThreadPriority");
            if (myThreadPriority != null && myThreadPriority.Equals("High"))
                threadPriority = ThreadPriority.AboveNormal;
            else if (myThreadPriority != null && myThreadPriority.Equals("AboveNormal"))
                threadPriority = ThreadPriority.Normal;
            else
                threadPriority = ThreadPriority.BelowNormal;


            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
            {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

            #if DEBUG
            logLevel = LogLevel.Debug;
            #endif

            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }


        /// <summary>
        /// The plugin is started by Mediaportal
        /// </summary>
        public void Start()
        {
            try
            {
                Utils.SetIsStopping(false); 
                initLogger();             
                logger.Info("Fanart Handler is starting.");
                logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());
                setupConfigFile();
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", "");
                    useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", "");
                    useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", "");
                    skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", "");
                    disableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", "");
                    useOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", "");
                    useMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", "");
                    useVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", "");
                    useScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", "");
                    imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", "");
                    minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", "");
                    defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", "");
                    scraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", "");
                    scraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", "");
                    scraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", "");   
                    scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", "");
                    useAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", "");
                    defaultBackdropIsImage = xmlreader.GetValueAsString("FanartHandler", "defaultBackdropIsImage", "");
                    useDefaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "useDefaultBackdrop", "");
                    proxyHostname = xmlreader.GetValueAsString("FanartHandler", "proxyHostname", "");
                    proxyPort = xmlreader.GetValueAsString("FanartHandler", "proxyPort", "");
                    proxyUsername = xmlreader.GetValueAsString("FanartHandler", "proxyUsername", "");
                    proxyPassword = xmlreader.GetValueAsString("FanartHandler", "proxyPassword", "");
                    proxyDomain = xmlreader.GetValueAsString("FanartHandler", "proxyDomain", "");
                    useProxy = xmlreader.GetValueAsString("FanartHandler", "useProxy", "");                   
                }
                if (useFanart != null && useFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useFanart = "True";
                }
                if (useProxy != null && useProxy.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useProxy = "False";
                }
                if (proxyHostname != null && proxyHostname.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyHostname = "";
                }
                if (proxyPort != null && proxyPort.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyPort = "";
                }
                if (proxyUsername != null && proxyUsername.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyUsername = "";
                }
                if (proxyPassword != null && proxyPassword.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyPassword = "";
                }
                if (proxyDomain != null && proxyDomain.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyDomain = "";
                }                
                if (useAlbum != null && useAlbum.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useAlbum = "True";
                }
                if (useDefaultBackdrop != null && useDefaultBackdrop.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useDefaultBackdrop = "True";
                }                
                if (useArtist != null && useArtist.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useArtist = "True";
                }
                if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Length > 0)
                {
                    //donothing
                }
                else
                {
                    skipWhenHighResAvailable = "True";
                }
                if (disableMPTumbsForRandom != null && disableMPTumbsForRandom.Length > 0)
                {
                    //donothing
                }
                else
                {
                    disableMPTumbsForRandom = "True";
                }
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Length > 0)
                {
                    //donothing
                }
                else
                {
                    defaultBackdropIsImage = "True";
                }                
                if (useOverlayFanart != null && useOverlayFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useOverlayFanart = "True";
                }
                if (useMusicFanart != null && useMusicFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useMusicFanart = "True";
                }
                if (useVideoFanart != null && useVideoFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useVideoFanart = "True";
                }
                if (useScoreCenterFanart != null && useScoreCenterFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useScoreCenterFanart = "True";
                }
                if (imageInterval != null && imageInterval.Length > 0)
                {
                    //donothing
                }
                else
                {
                    imageInterval = "30";
                }
                if (minResolution != null && minResolution.Length > 0)
                {
                    //donothing
                }
                else
                {
                    minResolution = "0x0";
                }
                if (defaultBackdrop != null && defaultBackdrop.Length > 0)
                {
                    //donothing
                }
                else
                {
                    string tmpPath = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music\default.jpg";
                    defaultBackdrop = tmpPath;
                }
                if (scraperMaxImages != null && scraperMaxImages.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperMaxImages = "2";
                }
                if (scraperMusicPlaying != null && scraperMusicPlaying.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperMusicPlaying = "False";
                }
                if (scraperMPDatabase != null && scraperMPDatabase.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperMPDatabase = "False";
                }
                if (scraperInterval != null && scraperInterval.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperInterval = "24";
                }
                if (useAspectRatio != null && useAspectRatio.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useAspectRatio = "False";
                }
                fp = new FanartPlaying();
                fs = new FanartSelected();
                fr = new FanartRandom();
                fr.setupWindowsUsingRandomImages();
                setupWindowsUsingFanartHandlerVisibility();
                SetupVariables();
                setupDirectories();
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Equals("True"))
                {
                    defaultBackdropImages.Add(0, defaultBackdrop);
                }
                else
                {
                    int i = 0;
                    SetupDefaultBackdrops(defaultBackdrop, ref i);
                }
                logger.Info("Fanart Handler is using Fanart: " + useFanart + ", Album Thumbs: " + useAlbum + ", Artist Thumbs: " + useArtist + ".");
                Utils.SetUseProxy(useProxy);
                Utils.SetProxyHostname(proxyHostname);
                Utils.SetProxyPort(proxyPort);
                Utils.SetProxyUsername(proxyUsername);
                Utils.SetProxyPassword(proxyPassword);
                Utils.SetProxyDomain(proxyDomain);
                Utils.SetScraperMaxImages(scraperMaxImages);
                Utils.InitiateDbm();
                m_db = MusicDatabase.Instance;
                ManageScraperProperties();                
                UpdateDirectoryTimer();
                //UpdateImageTimer();
                InitRandomProperties();
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True"))
                {
                    //startScraper();
                    myScraperTimer = new TimerCallback(UpdateScraperTimer);
                    int iScraperInterval = Convert.ToInt32(scraperInterval);
                    iScraperInterval = iScraperInterval * 3600000;
                    scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, iScraperInterval);
                    myProgressTimer = new TimerCallback(ManageScraperProperties);
                    progressTimer = new System.Threading.Timer(myProgressTimer, null, 2000, 3000);                        
                }
                myDirectoryTimer = new TimerCallback(UpdateDirectoryTimer);
                directoryTimer = new System.Threading.Timer(myDirectoryTimer, null, 3600000, 3600000);
                Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(onSystemPowerModeChanged);
                GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
                g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                refreshTimer = new System.Timers.Timer(1000);
                refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
                refreshTimer.Interval = 1000;
                updateTimer = new System.Timers.Timer(500);
                updateTimer.Elapsed += new ElapsedEventHandler(UpdateDummyControlsRandom);
                updateTimer.Interval = 500;
                string windowId = "35";// GUIWindowManager.ActiveWindow.ToString();
                if (fr.windowsUsingFanartRandom.ContainsKey(windowId) || fs.windowsUsingFanartSelected.ContainsKey(windowId) || (fp.windowsUsingFanartPlay.ContainsKey(windowId) || (useOverlayFanart != null && useOverlayFanart.Equals("True"))))
                {
                    refreshTimer.Start();                    
                }
                logger.Info("Fanart Handler is started.");
            }
            catch (Exception ex)
            {
                logger.Error("Start: " + ex.ToString());                
            }
        }               


            
        private void onSystemPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e) 
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Resume) 
            {
                logger.Info("Fanart Handler is resuming from standby/hibernate.");
                Start();
            }
            else if (e.Mode == Microsoft.Win32.PowerModes.Suspend) 
            {
                logger.Info("Fanart Handler is suspending/hibernating...");
                StopTasks(true);
                logger.Info("Fanart Handler is suspended/hibernated.");
            }
        }

        public void GUIWindowManager_OnActivateWindow(int activeWindowId)
        {
            try
            {
                int ix = 0;
                while (syncPointRefresh != 0 && ix < 20)
                {
                    System.Threading.Thread.Sleep(400);                    
                    ix++;
                }
                string windowId = ""+activeWindowId;
                isActivatingWindow = true;
                if ((fr.windowsUsingFanartRandom.ContainsKey(windowId) || fs.windowsUsingFanartSelected.ContainsKey(windowId) || fp.windowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId))
                {
                    if (fs.windowsUsingFanartSelected.ContainsKey(windowId))
                    {                        
                        if (fs.doShowImageOne)
                        {
                            fs.ShowImageTwo(activeWindowId);
                        }
                        else
                        {
                            fs.ShowImageOne(activeWindowId);
                        }
                        if (fs.fanartAvailable)
                        {
                            fs.FanartIsAvailable(activeWindowId);
                        }
                        else
                        {
                            fs.FanartIsNotAvailable(activeWindowId);
                        }
                        if (refreshTimer != null && !refreshTimer.Enabled)
                        {
                            refreshTimer.Start();
                        }
                    }
                    if ((fp.windowsUsingFanartPlay.ContainsKey(windowId) || (useOverlayFanart != null && useOverlayFanart.Equals("True"))) && AllowFanartInThisWindow(windowId))
                    {
                        if (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (m_CurrentTrackTag != null && m_CurrentTrackTag.Length > 0))))
                        {
                            if (fp.doShowImageOnePlay)
                            {
                                fp.ShowImageTwoPlay(activeWindowId);
                            }
                            else
                            {
                                fp.ShowImageOnePlay(activeWindowId);
                            }
                            if (fp.fanartAvailablePlay)
                            {
                                fp.FanartIsAvailablePlay(activeWindowId);
                            }
                            else
                            {
                                fp.FanartIsNotAvailablePlay(activeWindowId);
                            }
                            if (refreshTimer != null && !refreshTimer.Enabled)
                            {
                                refreshTimer.Start();
                            } 
                            
                        }
                        else
                        {                           
                            if (isPlaying)
                            {
                                stopScraperNowPlaying();
                                EmptyAllImages(ref fp.listPlayMusic);
                                fp.SetCurrentArtistsImageNames(null);
                                fp.currPlayMusic = "";
                                fp.currPlayMusicArtist = "";
                                fp.fanartAvailablePlay = false;
                                fp.FanartIsNotAvailablePlay(activeWindowId);
                                fp.prevPlayMusic = -1;
                                SetProperty("#fanarthandler.music.overlay.play", "");
                                SetProperty("#fanarthandler.music.backdrop1.play", "");
                                SetProperty("#fanarthandler.music.backdrop2.play", "");
                                fp.currCountPlay = 0;
                                fp.updateVisibilityCountPlay = 0;
                                isPlaying = false;
                            }
                        }
                    }
                    if (fr.windowsUsingFanartRandom.ContainsKey(windowId))
                    {
                        fr.windowOpen = true;                        
                        if (fr.doShowImageOneRandom)
                        {
                            fr.ShowImageTwoRandom(activeWindowId);
                        }
                        else
                        {
                            fr.ShowImageOneRandom(activeWindowId);
                        }
                        if (refreshTimer != null && !refreshTimer.Enabled)
                        {
                            refreshTimer.Start();
                        }
                        if (updateTimer != null && !updateTimer.Enabled)
                        {
                            updateTimer.Start();
                        }
                    }
                }
                else
                {
                    if (refreshTimer != null && refreshTimer.Enabled)
                    {
                        refreshTimer.Stop();                        
                        EmptyAllFanartHandlerProperties();
                    }
                    if (updateTimer != null && updateTimer.Enabled)
                    {
                        updateTimer.Stop();
                    }
                }
                isActivatingWindow = false;
            }
            catch (Exception ex)
            {
                isActivatingWindow = false;
                logger.Error("GUIWindowManager_OnActivateWindow: " + ex.ToString());
            }
        }

        private void EmptyAllFanartHandlerProperties()
        {
            try
            {
                if (isPlaying)
                {
                    stopScraperNowPlaying();
                    EmptyAllImages(ref fp.listPlayMusic);
                    fp.SetCurrentArtistsImageNames(null);
                    fp.currPlayMusic = "";
                    fp.currPlayMusicArtist = "";
                    fp.fanartAvailablePlay = false;
                    fp.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                    fp.prevPlayMusic = -1;
                    SetProperty("#fanarthandler.music.overlay.play", "");
                    SetProperty("#fanarthandler.music.backdrop1.play", "");
                    SetProperty("#fanarthandler.music.backdrop2.play", "");
                    fp.currCountPlay = 0;
                    fp.updateVisibilityCountPlay = 0;
                    isPlaying = false;
                }
                if (isSelectedMusic)
                {
                    EmptyAllImages(ref fs.listSelectedMusic);
                    fs.currSelectedMusic = "";
                    fs.currSelectedMusicArtist = "";
                    fs.SetCurrentArtistsImageNames(null);
                    fs.currCount = 0;
                    fs.updateVisibilityCount = 0;
                    SetProperty("#fanarthandler.music.backdrop1.selected", "");
                    SetProperty("#fanarthandler.music.backdrop2.selected", "");
                    isSelectedMusic = false;
                }
                if (isSelectedVideo)
                {
                    EmptyAllImages(ref fs.listSelectedMovies);
                    fs.currSelectedMovie = "";
                    fs.currSelectedMovieTitle = "";
                    fs.SetCurrentArtistsImageNames(null);
                    fs.currCount = 0;
                    fs.updateVisibilityCount = 0;
                    SetProperty("#fanarthandler.movie.backdrop1.selected", "");
                    SetProperty("#fanarthandler.movie.backdrop2.selected", "");
                    isSelectedVideo = false;
                }
                if (isSelectedScoreCenter)
                {
                    EmptyAllImages(ref fs.listSelectedScorecenter);
                    fs.currSelectedScorecenter = "";
                    fs.currSelectedScorecenterGenre = "";
                    fs.SetCurrentArtistsImageNames(null);
                    fs.currCount = 0;
                    fs.updateVisibilityCount = 0;
                    SetProperty("#fanarthandler.scorecenter.backdrop1.selected", "");
                    SetProperty("#fanarthandler.scorecenter.backdrop2.selected", "");
                    isSelectedScoreCenter = false;
                }
                if (isRandom)
                {
                    SetProperty("#fanarthandler.games.backdrop1.any", "");
                    SetProperty("#fanarthandler.movie.backdrop1.any", "");
                    SetProperty("#fanarthandler.movingpicture.backdrop1.any", "");
                    SetProperty("#fanarthandler.music.backdrop1.any", "");
                    SetProperty("#fanarthandler.picture.backdrop1.any", "");
                    SetProperty("#fanarthandler.scorecenter.backdrop1.any", "");
                    SetProperty("#fanarthandler.tvseries.backdrop1.any", "");
                    SetProperty("#fanarthandler.tv.backdrop1.any", "");
                    SetProperty("#fanarthandler.plugins.backdrop1.any", "");
                    SetProperty("#fanarthandler.games.backdrop2.any", "");
                    SetProperty("#fanarthandler.movie.backdrop2.any", "");
                    SetProperty("#fanarthandler.movingpicture.backdrop2.any", "");
                    SetProperty("#fanarthandler.music.backdrop2.any", "");
                    SetProperty("#fanarthandler.picture.backdrop2.any", "");
                    SetProperty("#fanarthandler.scorecenter.backdrop2.any", "");
                    SetProperty("#fanarthandler.tvseries.backdrop2.any", "");
                    SetProperty("#fanarthandler.tv.backdrop2.any", "");
                    SetProperty("#fanarthandler.plugins.backdrop2.any", "");
                    fr.currCountRandom = 0;
                    fr.countSetVisibility = 0;
                    fr.ClearPropertiesRandom();
                    fr.updateVisibilityCountRandom = 0;
                    EmptyAllImages(ref fr.listAnyGames);
                    EmptyAllImages(ref fr.listAnyMovies);
                    EmptyAllImages(ref fr.listAnyMovingPictures);
                    EmptyAllImages(ref fr.listAnyMusic);
                    EmptyAllImages(ref fr.listAnyPictures);
                    EmptyAllImages(ref fr.listAnyScorecenter);
                    EmptyAllImages(ref fr.listAnyTVSeries);
                    EmptyAllImages(ref fr.listAnyTV);
                    EmptyAllImages(ref fr.listAnyPlugins);
                    isRandom = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("EmptyAllFanartHandlerProperties: " + ex.ToString());
            }
        }

 

        private bool AllowFanartInThisWindow(string windowId)
        {            
            if (windowId != null && windowId.Equals("511"))
                return false;
            else if (windowId != null && windowId.Equals("2005"))
                return false;
            else if (windowId != null && windowId.Equals("602"))
                return false;
            else
                return true;
        }
        
        public void OnPlayBackStarted(g_Player.MediaType type, string filename)
        {
            try
            {
                string windowId = GUIWindowManager.ActiveWindow.ToString();                                
                isPlaying = true;
                if ((fp.windowsUsingFanartPlay.ContainsKey(windowId) || (useOverlayFanart != null && useOverlayFanart.Equals("True"))) && AllowFanartInThisWindow(windowId))
                {
                    if (refreshTimer != null && !refreshTimer.Enabled && (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename)))
                    {
                        refreshTimer.Start();
                    }
                }                    
            }
            catch (Exception ex)
            {
                logger.Error("OnPlayBackStarted: " + ex.ToString());
            }
        }



        private void createDirectoryIfMissing(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void setupDirectories()
        {
            try
            {
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                createDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                createDirectoryIfMissing(path);
            }
            catch (Exception ex)
            {
                logger.Error("setupDirectories: " + ex.ToString());
            }
        }

        private void startScraper()
        {
            try
            {
                Utils.GetDbm().totArtistsBeingScraped = 0;
                Utils.GetDbm().currArtistsBeingScraped = 0;
                scraperWorkerObject = new ScraperWorker();
                scrapeWorkerThread = new Thread(scraperWorkerObject.DoWork);
                scrapeWorkerThread.Priority = threadPriority;
                // Start the worker thread.
                scrapeWorkerThread.Start();

                // Loop until worker thread activates.
                int ix = 0;
                while (!scrapeWorkerThread.IsAlive && ix < 30)
                {
                    System.Threading.Thread.Sleep(500);
                    ix++;
                }
            }
            catch (Exception ex)
            {
                logger.Error("startScraper: " + ex.ToString());
            }
        }

        private void stopScraper()
        {
            try
            {
                // Request that the worker thread stop itself:            
                if (scraperWorkerObject != null && scrapeWorkerThread != null && scrapeWorkerThread.IsAlive)
                {
                    scraperWorkerObject.RequestStop();

                    // Use the Join method to block the current thread 
                    // until the object's thread terminates.
                    scrapeWorkerThread.Join();
                }
                scraperWorkerObject = null;
                scrapeWorkerThread = null;
            }
            catch (Exception ex)
            {
                logger.Error("stopScraper: " + ex.ToString());
            }
        }

        private void startScraperNowPlaying(string artist)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Utils.GetDbm().totArtistsBeingScraped = 0;
                    Utils.GetDbm().currArtistsBeingScraped = 0;
                    scraperWorkerObjectNowPlaying = new ScraperWorkerNowPlaying(artist);
                    scrapeWorkerThreadNowPlaying = new Thread(scraperWorkerObjectNowPlaying.DoWork);
                    scrapeWorkerThreadNowPlaying.Priority = threadPriority;
                    // Start the worker thread.
                    scrapeWorkerThreadNowPlaying.Start();

                    // Loop until worker thread activates.                    
                    int ix = 0;
                    while (!scrapeWorkerThreadNowPlaying.IsAlive && ix < 30)
                    {
                        System.Threading.Thread.Sleep(500);
                        ix++;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("startScraperNowPlaying: " + ex.ToString());
            }
        }

        private void stopScraperNowPlaying()
        {
            try
            {
                // Request that the worker thread stop itself:            
                if (scraperWorkerObjectNowPlaying != null && scrapeWorkerThreadNowPlaying != null && scrapeWorkerThreadNowPlaying.IsAlive)
                {
                    scraperWorkerObjectNowPlaying.RequestStop();

                    // Use the Join method to block the current thread 
                    // until the object's thread terminates.
                    scrapeWorkerThreadNowPlaying.Join();
                }
                scraperWorkerObjectNowPlaying = null;
                scrapeWorkerThreadNowPlaying = null;
            }
            catch (Exception ex)
            {
                logger.Error("stopScraperNowPlaying: " + ex.ToString());
            }
        }



        


        /// <summary>
        /// UnLoad image (free memory)
        /// </summary>
        private static void UnLoadImage(string filename)
        {
            try
            {
                GUITextureManager.ReleaseTexture(filename);
            }
            catch (Exception ex)
            {
                logger.Error("UnLoadImage: " + ex.ToString());
            }
        }


        private void ManageScraperProperties()
        {
            try
            {
                if (Utils.GetDbm().GetIsScraping())
                {
                    GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
                    double iTot = Utils.GetDbm().totArtistsBeingScraped;
                    double iCurr = Utils.GetDbm().currArtistsBeingScraped;
                    if (iTot > 0)
                    {
                        SetProperty("#fanarthandler.scraper.percent.completed", ""+Convert.ToInt32((iCurr / iTot)*100));
                    }
                    else
                    {
                        SetProperty("#fanarthandler.scraper.percent.completed", "");
                        GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
                    }
                }
                else
                {
                    GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", "");
                    GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
                    Utils.GetDbm().totArtistsBeingScraped = 0;
                    Utils.GetDbm().currArtistsBeingScraped = 0;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ManageScraperProperties: " + ex.ToString());
                GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", "");
                GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
            }            
        }

        private void ManageScraperProperties(Object stateInfo)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    ManageScraperProperties();
                }
                catch (Exception ex)
                {
                    logger.Error("ManageScraperProperties: " + ex.ToString());
                }
            }
        }

        private void setupConfigFile()
        {
            try
            {
                String path = Config.GetFile(Config.Dir.Config, "FanartHandler.xml");
                String pathOrg = Config.GetFile(Config.Dir.Config, "FanartHandler.org");
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
                logger.Error("setupConfigFile: " + ex.ToString());
            }
        }

        /// <summary>
        /// The Plugin is stopped
        /// </summary>
        public void Stop()
        {
            try
            {
                StopTasks(false);
                logger.Info("Fanart Handler is stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("Stop: " + ex.ToString());
            }
        }

        private void StopTasks(bool suspending)
        {
            try
            {
                Utils.SetIsStopping(true);
                int ix = 0;
                while (Utils.GetDelayStop() && ix < 20)
                {
                    System.Threading.Thread.Sleep(500);
                    ix++;
                }
                stopScraper();
                stopScraperNowPlaying();
                if (progressTimer != null)
                {
                    progressTimer.Dispose();
                }
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().close();
                }
                if (scraperTimer != null)
                {
                    scraperTimer.Dispose();
                }
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                }
                if (updateTimer != null)
                {
                    updateTimer.Stop();
                }
                if (directoryTimer != null)
                {
                    directoryTimer.Dispose();
                }
                EmptyAllImages(ref fr.listAnyGames);
                EmptyAllImages(ref fr.listAnyMovies);
                EmptyAllImages(ref fs.listSelectedMovies);
                EmptyAllImages(ref fr.listAnyMovingPictures);
                EmptyAllImages(ref fr.listAnyMusic);
                EmptyAllImages(ref fp.listPlayMusic);
                EmptyAllImages(ref fr.listAnyPictures);
                EmptyAllImages(ref fr.listAnyScorecenter);
                EmptyAllImages(ref fs.listSelectedMusic);
                EmptyAllImages(ref fs.listSelectedScorecenter);
                EmptyAllImages(ref fr.listAnyTVSeries);
                EmptyAllImages(ref fr.listAnyTV);
                EmptyAllImages(ref fr.listAnyPlugins);
                if (!suspending)
                {
                    Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(onSystemPowerModeChanged);
                }
                GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
                g_Player.PlayBackStarted -= new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                fp = null;
                fs = null;
                fr = null;
            }
            catch (Exception ex)
            {
                logger.Error("Stop: " + ex.ToString());
            }
        }

        #region ISetupForm Members

        // Returns the name of the plugin which is shown in the plugin menu
        public string PluginName()
        {
            return "Fanart Handler";
        }

        // Returns the description of the plugin is shown in the plugin menu
        public string Description()
        {
            return "Fanart handler for MediaPortal.";
        }

        // Returns the author of the plugin which is shown in the plugin menu
        public string Author()
        {
            return "cul8er";
        }

        // show the setup dialog
        public void ShowPlugin()
        {
            xconfig = new FanartHandlerConfig();
            xconfig.ShowDialog();
        }

        // Indicates whether plugin can be enabled/disabled
        public bool CanEnable()
        {
            return true;
        }

        // Get Windows-ID
        public int GetWindowId()
        {
            // WindowID of windowplugin belonging to this setup
            // enter your own unique code
            return 730716;
        }

        // Indicates if plugin is enabled by default;
        public bool DefaultEnabled()
        {
            return true;
        }

        // indicates if a plugin has it's own setup screen
        public bool HasSetup()
        {
            return true;
        }

        /// <summary>
        /// If the plugin should have it's own button on the main menu of MediaPortal then it
        /// should return true to this method, otherwise if it should not be on home
        /// it should return false
        /// </summary>
        /// <param name="strButtonText">text the button should have</param>
        /// <param name="strButtonImage">image for the button, or empty for default</param>
        /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
        /// <param name="strPictureImage">subpicture for the button or empty for none</param>
        /// <returns>true : plugin needs it's own button on home
        /// false : plugin does not need it's own button on home</returns>

        public bool GetHome(out string strButtonText, out string strButtonImage,
          out string strButtonImageFocus, out string strPictureImage)
        {
            strButtonText = String.Empty;// strButtonText = PluginName();
            strButtonImage = String.Empty;
            strButtonImageFocus = String.Empty;
            strPictureImage = String.Empty;
            return false;
        }

        #endregion
       
    
        public class ScraperWorker
        {
            private bool triggerRefresh = false;

        
            public ScraperWorker()
            {
                this.triggerRefresh = false;
            }
            
            // This method will be called when the thread is started.
            public void DoWork()
            {
                
                while (!_shouldStop) 
                {
                    Utils.GetDbm().InitialScrape(this);
                    Utils.GetDbm().doNewScrape();                    
                    RequestStop();
                    Utils.GetDbm().stopScraper = false;
                }
            }

            public bool getRefreshFlag()
            {
                return triggerRefresh;
            }

            public void setRefreshFlag(bool flag)
            {
                this.triggerRefresh = flag;
            }

            public void RequestStop()
            {
                Utils.GetDbm().stopScraper = true;
                _shouldStop = true;
            }
            // Volatile is used as hint to the compiler that this data
            // member will be accessed by multiple threads.
            private volatile bool _shouldStop;
        }

        public class ScraperWorkerNowPlaying
        {
            private string artist;
            private bool triggerRefresh = false;

            public ScraperWorkerNowPlaying(string artist)
            {
                this.artist = artist;
                this.triggerRefresh = false;
            }

            public bool getRefreshFlag()
            {
                return triggerRefresh;
            }

            public void setRefreshFlag(bool flag)
            {
                this.triggerRefresh = flag;
            }

            // This method will be called when the thread is started.
            public void DoWork()
            {
                while (!_shouldStop)
                {
                    Utils.GetDbm().NowPlayingScrape(artist, this);
                    RequestStop();
                }
            }
            public void RequestStop()
            {
                _shouldStop = true;
            }
            // Volatile is used as hint to the compiler that this data
            // member will be accessed by multiple threads.
            private volatile bool _shouldStop;
        }
    }
}
