//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.Dialogs;
    using MediaPortal.GUI.Library;
    using MediaPortal.Music.Database;
    using MediaPortal.Player;
    using MediaPortal.Services;
    using MediaPortal.TagReader;
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Timers;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.XPath;

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
        private static ScraperWorker myScraperWorker = null;
        private static ScraperNowWorker myScraperNowWorker = null;   
        private RefreshWorker MyRefreshWorker = null;
        private DirectoryWorker MyDirectoryWorker = null;
        private System.Timers.Timer refreshTimer = null;
        private TimerCallback myDirectoryTimer = null;
        private System.Threading.Timer directoryTimer = null;
        private TimerCallback myScraperTimer = null;     
        private static string fhThreadPriority = "Lowest";                
        private System.Threading.Timer scraperTimer = null;

        private static string m_CurrentTrackTag = null;  //is music playing and if so this holds current artist name                        
        private static bool isPlaying = false; //hold true if MP plays music        
        private static bool isSelectedMusic = false;
        private static bool isSelectedVideo = false;
        private static bool isSelectedScoreCenter = false;
        private static bool isRandom = false;        
        private static string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";        
        private static bool preventRefresh = false;        
        private static Hashtable defaultBackdropImages;  //used to hold all the default backdrop images                
        private static Random randDefaultBackdropImages = null;  //For getting random default backdrops
        private FanartHandlerConfig xconfig = null;
        private static string scraperMaxImages = null;  // Holds info read from fanarthandler.xml settings file        
        private static string scraperMusicPlaying = null;  // Holds info read from fanarthandler.xml settings file        
        private static string scraperMPDatabase = null;  // Holds info read from fanarthandler.xml settings file        
        private string scraperInterval = null;  // Holds info read from fanarthandler.xml settings file
        private static string useArtist = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useAlbum = null;  // Holds info read from fanarthandler.xml settings file        
        private static string skipWhenHighResAvailable = null;  // Holds info read from fanarthandler.xml settings file
        private static string disableMPTumbsForRandom = null;  // Holds info read from fanarthandler.xml settings file        
        private string defaultBackdropIsImage = null;  // Holds info read from fanarthandler.xml settings file
        private static string useFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useOverlayFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useMusicFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useVideoFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useScoreCenterFanart = null;  // Holds info read from fanarthandler.xml settings file        
        private string imageInterval = null;  // Holds info read from fanarthandler.xml settings file
        private static string minResolution = null;  // Holds info read from fanarthandler.xml settings file
        private string defaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        private static string useAspectRatio = null;  // Holds info read from fanarthandler.xml settings file        
        private static string useDefaultBackdrop = null;  // Holds info read from fanarthandler.xml settings file
        private string useProxy = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyHostname = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyPort = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyUsername = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyPassword = null;  // Holds info read from fanarthandler.xml settings file
        private string proxyDomain = null;  // Holds info read from fanarthandler.xml settings file
        private static MusicDatabase m_db = null;  //handle to MP Music database                        
        private static int maxCountImage = 30;        
        private static string m_SelectedItem = null; //artist, album, title                
        public static FanartPlaying fp = null;
        public static FanartSelected fs = null;
        public static FanartRandom fr = null;
        public static int syncPointRefresh = 0;
        public static int syncPointDirectory = 0;
        public static int syncPointScraper = 0;
        private static int basichomeFadeTime = 5;        
        private static bool useBasichomeFade = true;
        private static string m_CurrentTitleTag = null;        
        #endregion

        public static ScraperNowWorker MyScraperNowWorker
        {
            get { return FanartHandlerSetup.myScraperNowWorker; }
            set { FanartHandlerSetup.myScraperNowWorker = value; }
        }        

        public static ScraperWorker MyScraperWorker
        {
            get { return FanartHandlerSetup.myScraperWorker; }
            set { FanartHandlerSetup.myScraperWorker = value; }
        }        

        public static string FhThreadPriority
        {
            get { return FanartHandlerSetup.fhThreadPriority; }
            set { FanartHandlerSetup.fhThreadPriority = value; }
        }

        public static bool IsRandom
        {
            get { return FanartHandlerSetup.isRandom; }
            set { FanartHandlerSetup.isRandom = value; }
        }

        public static bool IsSelectedScoreCenter
        {
            get { return FanartHandlerSetup.isSelectedScoreCenter; }
            set { FanartHandlerSetup.isSelectedScoreCenter = value; }
        }

        public static string UseScoreCenterFanart
        {
            get { return FanartHandlerSetup.useScoreCenterFanart; }
            set { FanartHandlerSetup.useScoreCenterFanart = value; }
        }

        public static bool IsSelectedVideo
        {
            get { return FanartHandlerSetup.isSelectedVideo; }
            set { FanartHandlerSetup.isSelectedVideo = value; }
        }

        public static string UseVideoFanart
        {
            get { return FanartHandlerSetup.useVideoFanart; }
            set { FanartHandlerSetup.useVideoFanart = value; }
        }

        public static bool IsSelectedMusic
        {
            get { return FanartHandlerSetup.isSelectedMusic; }
            set { FanartHandlerSetup.isSelectedMusic = value; }
        }

        public static string UseMusicFanart
        {
            get { return useMusicFanart; }
            set { useMusicFanart = value; }
        }

        public static string TmpImage
        {
            get { return FanartHandlerSetup.tmpImage; }
            set { FanartHandlerSetup.tmpImage = value; }
        }

        public static bool IsPlaying
        {
            get { return isPlaying; }
            set { isPlaying = value; }
        }

        public static bool UseBasichomeFade
        {
            get { return useBasichomeFade; }
            set { useBasichomeFade = value; }
        }

        public static string CurrentTitleTag
        {
            get { return m_CurrentTitleTag; }
            set { m_CurrentTitleTag = value; }
        }

        public static int BasichomeFadeTime
        {
            get { return basichomeFadeTime; }
            set { basichomeFadeTime = value; }
        }

        public static string CurrentTrackTag
        {
            get { return FanartHandlerSetup.m_CurrentTrackTag; }
            set { FanartHandlerSetup.m_CurrentTrackTag = value; }
        }

        public static bool PreventRefresh
        {
            get { return preventRefresh; }
            set { preventRefresh = value; }
        }

        public static Hashtable DefaultBackdropImages
        {
            get { return FanartHandlerSetup.defaultBackdropImages; }
            set { FanartHandlerSetup.defaultBackdropImages = value; }
        }

        public static string ScraperMusicPlaying
        {
            get { return FanartHandlerSetup.scraperMusicPlaying; }
            set { FanartHandlerSetup.scraperMusicPlaying = value; }
        }

        public static string ScraperMPDatabase
        {
            get { return FanartHandlerSetup.scraperMPDatabase; }
            set { FanartHandlerSetup.scraperMPDatabase = value; }
        }

        public static string UseArtist
        {
            get { return FanartHandlerSetup.useArtist; }
            set { FanartHandlerSetup.useArtist = value; }
        }

        public static string UseAlbum
        {
            get { return FanartHandlerSetup.useAlbum; }
            set { FanartHandlerSetup.useAlbum = value; }
        }

        public static string DisableMPTumbsForRandom
        {
            get { return FanartHandlerSetup.disableMPTumbsForRandom; }
            set { FanartHandlerSetup.disableMPTumbsForRandom = value; }
        }

        public static string UseFanart
        {
            get { return FanartHandlerSetup.useFanart; }
            set { FanartHandlerSetup.useFanart = value; }
        }

        public static string UseOverlayFanart
        {
            get { return FanartHandlerSetup.useOverlayFanart; }
            set { FanartHandlerSetup.useOverlayFanart = value; }
        }

        public static string UseAspectRatio
        {
            get { return FanartHandlerSetup.useAspectRatio; }
            set { FanartHandlerSetup.useAspectRatio = value; }
        }

        public static MusicDatabase M_Db
        {
            get { return FanartHandlerSetup.m_db; }
            set { FanartHandlerSetup.m_db = value; }
        }

        public static int MaxCountImage
        {
            get { return FanartHandlerSetup.maxCountImage; }
            set { FanartHandlerSetup.maxCountImage = value; }
        }

        public static string SelectedItem
        {
            get { return FanartHandlerSetup.m_SelectedItem; }
            set { FanartHandlerSetup.m_SelectedItem = value; }
        }

        public static string ScraperMaxImages
        {
            get { return FanartHandlerSetup.scraperMaxImages; }
            set { FanartHandlerSetup.scraperMaxImages = value; }
        }


        public static void HandleOldImages(ref ArrayList al)
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
        public static bool CheckImageResolution(string filename, string type, string useAspectRatio)
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
        public static void SetupFilenames(string s, string filter, ref int i, string type)
        {
            string artist = String.Empty;
            string typeOrg = type;
            try
            {
                if (Directory.Exists(s))
                {
                    string useFilter = Utils.GetDbm().GetTimeStamp("Directory - " + s);
                    if (useFilter == null || useFilter.Length < 2)
                    {
                        useFilter = new DateTime(1970, 1, 1, 1, 1, 1).ToString();
                    }
                    DirectoryInfo Dir = new DirectoryInfo(s);
                    DateTime dt = Convert.ToDateTime(useFilter);
                    FileInfo[] FileList = Dir.GetFiles(filter, SearchOption.AllDirectories);
                    var query = from FI in FileList
                                where FI.CreationTime >= dt
                                select FI.FullName;
                    foreach (string dir in query)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        artist = Utils.GetArtist(dir, type);
                        if (type.Equals("MusicAlbum") || type.Equals("MusicArtist") || type.Equals("MusicFanart"))
                        {
                            if (Utils.GetFilenameNoPath(dir).ToLower().StartsWith("default"))
                            {
                                type = "Default";
                            }
                            Utils.GetDbm().LoadMusicFanart(artist, dir, dir, type);
                            type = typeOrg;
                        }
                        else
                        {
                            Utils.GetDbm().LoadFanart(artist, dir, dir, type);
                        }
                    }
                    if (Utils.GetIsStopping() == false)
                    {
                        Utils.GetDbm().SetTimeStamp("Directory - " + s, DateTime.Now.ToString());
                    }
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
                                DefaultBackdropImages.Add(i, file);
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
        public void UpdateDirectoryTimer(Object stateInfo)
        {
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref syncPointDirectory, 1, 0);
                    if (sync == 0)
                    {
                        // No other event was executing.                        
                        MyDirectoryWorker = new DirectoryWorker();
                        MyDirectoryWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    syncPointDirectory = 0;
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
                    if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True") && Utils.GetDbm().GetIsScraping() == false)
                    {
                        StartScraper();
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
        public static string GetFilename(string key, ref string currFile, ref int iFilePrev, string type, string obj, bool newArtist, bool isMusic)
        {
            string sout = String.Empty;//currFile; 20100515
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
                        if (isMusic && skipWhenHighResAvailable != null && skipWhenHighResAvailable.Equals("True"))
                        {
                            tmp = Utils.GetDbm().GetHigResFanart(key, type);
                        }
                        if (tmp != null && tmp.Count > 0)
                        {
                            //do nothing
                        }
                        else
                        {
                            tmp = Utils.GetDbm().GetFanart(key, type);
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
                                if (CheckImageResolution(s.disk_image, type, UseAspectRatio) && Utils.IsFileValid(s.disk_image))
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
                                    if (CheckImageResolution(s.disk_image, type, UseAspectRatio) && Utils.IsFileValid(s.disk_image))
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
        public static string GetRandomDefaultBackdrop(ref string currFile, ref int iFilePrev)
        {
            string sout = String.Empty;
            try
            {
                if (Utils.GetIsStopping() == false && useDefaultBackdrop.Equals("True"))
                {
                    if (DefaultBackdropImages != null && DefaultBackdropImages.Count > 0)
                    {
                        ICollection valueColl = DefaultBackdropImages.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (string s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (CheckImageResolution(s, "MusicFanart", UseAspectRatio) && Utils.IsFileValid(s))
                                {
                                    sout = s;
                                    iFilePrev = iFile;
                                    currFile = s;
                                    iStop = 1;
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = DefaultBackdropImages.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (string s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (CheckImageResolution(s, "MusicFanart", UseAspectRatio) && Utils.IsFileValid(s))
                                    {
                                        sout = s;
                                        iFilePrev = iFile;
                                        currFile = s;
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

        private static void ResetCounters()
        {
            if (fs.CurrCount > MaxCountImage)
            {
                fs.CurrCount = 0;
                fs.HasUpdatedCurrCount = false;
            }
            if (fs.UpdateVisibilityCount > 5)
            {
                fs.UpdateVisibilityCount = 1;
            }
            if (fp.CurrCountPlay > MaxCountImage)
            {
                fp.CurrCountPlay = 0;
                fp.HasUpdatedCurrCountPlay = false;
            }
            if (fp.UpdateVisibilityCountPlay > 5)
            {
                fp.UpdateVisibilityCountPlay = 1;
            }
        }

        /// <summary>
        /// Update visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public static void UpdateDummyControls()
        {
            try
            {
                //something has gone wrong
                ResetCounters();
                int windowId = GUIWindowManager.ActiveWindow;
                if (fs.UpdateVisibilityCount == 2)  //after 2 sek
                {
                    fs.UpdateProperties();
                    if (fs.DoShowImageOne)
                    {
                        fs.ShowImageOne(windowId);
                        fs.DoShowImageOne = false;
                    }
                    else
                    {
                        fs.ShowImageTwo(windowId);
                        fs.DoShowImageOne = true;
                    }
                    if (fs.FanartAvailable)
                    {
                        fs.FanartIsAvailable(windowId);
                    }
                    else
                    {
                        fs.FanartIsNotAvailable(windowId);
                    }                    
                }
                else if (fs.UpdateVisibilityCount == 5) //after 4 sek
                {
                    fs.UpdateVisibilityCount = 0;
                    //release unused image resources
                    HandleOldImages(ref fs.listSelectedMovies);
                    HandleOldImages(ref fs.listSelectedMusic);
                    HandleOldImages(ref fs.listSelectedScorecenter);
                }
                if (fp.UpdateVisibilityCountPlay == 2)  //after 2 sek
                {
                    fp.UpdatePropertiesPlay();
                    if (fp.DoShowImageOnePlay)
                    {
                        fp.ShowImageOnePlay(windowId);
                        fp.DoShowImageOnePlay = false;
                    }
                    else
                    {
                        fp.ShowImageTwoPlay(windowId);
                        fp.DoShowImageOnePlay = true;
                    }
                    if (fp.FanartAvailablePlay)
                    {
                        fp.FanartIsAvailablePlay(windowId);
                    }
                    else
                    {
                        fp.FanartIsNotAvailablePlay(windowId);
                    }
                }
                else if (fp.UpdateVisibilityCountPlay == 5) //after 4 sek
                {
                    fp.UpdateVisibilityCountPlay = 0;
                    //release unused image resources
                    HandleOldImages(ref fp.listPlayMusic);
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
                logger.Debug("listSelectedMovies: " + fs.listSelectedMovies.Count); 
                logger.Debug("listSelectedMusic: " + fs.listSelectedMusic.Count); 
                logger.Debug("listSelectedScorecenter: " + fs.listSelectedScorecenter.Count);
                logger.Debug("listPlayMusic: " + fp.listPlayMusic.Count);               
 */ 
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDummyControls: " + ex.ToString());
            }
        }

 
        
        /// <summary>
        /// Get value from xml node
        /// </summary>
        private string GetNodeValue(XPathNodeIterator myXPathNodeIterator)
        {
            if (myXPathNodeIterator.Count > 0)
            {
                myXPathNodeIterator.MoveNext();
                return myXPathNodeIterator.Current.Value;
            }
            return String.Empty;
        }
 
        private void SetupWindowsUsingFanartHandlerVisibility()
        {
            XPathDocument myXPathDocument;
            fs.WindowsUsingFanartSelected = new Hashtable();
            fp.WindowsUsingFanartPlay = new Hashtable();
            string path = GUIGraphicsContext.Skin + @"\";
            string windowId = String.Empty;
            string sNodeValue = String.Empty;
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            string s = String.Empty;
            foreach (FileInfo fi in rgFiles)
            {
                try
                {
                    s = fi.Name;
                    myXPathDocument = new XPathDocument(fi.FullName);
                    XPathNavigator myXPathNavigator = myXPathDocument.CreateNavigator();
                    XPathNodeIterator myXPathNodeIterator = myXPathNavigator.Select("/window/id");
                    windowId = GetNodeValue(myXPathNodeIterator);
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
                                        fs.WindowsUsingFanartSelected.Add(windowId, windowId);
                                    }
                                    catch { }
                                }
                                if (sNodeValue.StartsWith("#usePlayFanart:Yes"))
                                {
                                    try
                                    {
                                        fp.WindowsUsingFanartPlay.Add(windowId, windowId);
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
                    if (fr.WindowsUsingFanartRandom.ContainsKey("35"))
                    {
                        IsRandom = true;
                        fr.RefreshRandomImageProperties(null);
                        if (fr.UpdateVisibilityCountRandom > 0)
                        {
                            fr.UpdateVisibilityCountRandom = fr.UpdateVisibilityCountRandom + 1;
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
        public void UpdateImageTimer(Object stateInfo, ElapsedEventArgs e)
        {
            if (Utils.GetIsStopping() == false && !PreventRefresh)
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref syncPointRefresh, 1, 0);
                    if (sync == 0)
                    {
                        // No other event was executing.                        
                        MyRefreshWorker = new RefreshWorker();
                        MyRefreshWorker.ProgressChanged += MyRefreshWorker.OnProgressChanged;
                        MyRefreshWorker.RunWorkerAsync();  

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
            PreventRefresh = false;
            IsPlaying = false;
            IsSelectedMusic = false;
            IsSelectedVideo = false;
            IsSelectedScoreCenter = false;
            IsRandom = false;
            fs.DoShowImageOne = true;
            fp.DoShowImageOnePlay = true;
            fr.DoShowImageOneRandom = true;
            fr.FirstRandom = true;
            fs.FanartAvailable = false;
            fp.FanartAvailablePlay = false;
            fs.UpdateVisibilityCount = 0;
            fp.UpdateVisibilityCountPlay = 0;
            fr.UpdateVisibilityCountRandom = 0;
            fs.CurrCount = 0;
            fp.CurrCountPlay = 0;
            fr.currCountRandom = 0;
            SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
            MaxCountImage = Convert.ToInt32(imageInterval);
            fs.HasUpdatedCurrCount = false;
            fp.HasUpdatedCurrCountPlay = false;
            fs.prevSelectedGeneric = -1;
            fp.prevPlayMusic = -1;
            fs.prevSelectedMusic = -1;
            fs.prevSelectedScorecenter = -1;
            fs.currSelectedMovieTitle = String.Empty;
            fp.CurrPlayMusicArtist = String.Empty;
            fs.currSelectedMusicArtist = String.Empty;
            fs.CurrSelectedScorecenterGenre = String.Empty;
            fs.currSelectedMovie = String.Empty;
            fp.currPlayMusic = String.Empty;
            fs.currSelectedMusic = String.Empty;
            fs.currSelectedScorecenter = String.Empty;
            syncPointRefresh = 0;
            syncPointDirectory = 0;
            syncPointScraper = 0;
            m_CurrentTrackTag = null;
            m_CurrentTitleTag = null;
            m_SelectedItem = null;
            SetProperty("#fanarthandler.scraper.percent.completed", TmpImage);
            SetProperty("#fanarthandler.games.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.games.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.movie.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.movie.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.movie.backdrop1.selected", TmpImage);
            SetProperty("#fanarthandler.movie.backdrop2.selected", TmpImage);
            SetProperty("#fanarthandler.movingpicture.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.movingpicture.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.music.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.music.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.music.overlay.play", TmpImage);
            SetProperty("#fanarthandler.music.backdrop1.play", TmpImage);
            SetProperty("#fanarthandler.music.backdrop2.play", TmpImage);
            SetProperty("#fanarthandler.music.backdrop1.selected", TmpImage);
            SetProperty("#fanarthandler.music.backdrop2.selected", TmpImage);
            SetProperty("#fanarthandler.picture.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.picture.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.scorecenter.backdrop1.selected", TmpImage);
            SetProperty("#fanarthandler.scorecenter.backdrop2.selected", TmpImage);
            SetProperty("#fanarthandler.scorecenter.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.scorecenter.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.tvseries.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.tvseries.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.tv.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.tv.backdrop2.any", TmpImage);
            SetProperty("#fanarthandler.plugins.backdrop1.any", TmpImage);
            SetProperty("#fanarthandler.plugins.backdrop2.any", TmpImage);
            fs.Properties = new Hashtable();
            fp.PropertiesPlay = new Hashtable();
            fr.PropertiesRandom = new Hashtable();
            DefaultBackdropImages = new Hashtable();
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
        private void InitLogger()
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

            if (myThreadPriority != null && myThreadPriority.Equals("Normal"))
            {
                FhThreadPriority = "Lowest";
            }
            else if (myThreadPriority != null && myThreadPriority.Equals("BelowNormal"))
            {
                FhThreadPriority = "Lowest";
            }
            else
            {
                FhThreadPriority = "BelowNormal";
            }

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
                InitLogger();             
                logger.Info("Fanart Handler is starting.");
                logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());                
                SetupConfigFile();
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    UseFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", String.Empty);
                    UseAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", String.Empty);
                    UseArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", String.Empty);
                    skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", String.Empty);
                    DisableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", String.Empty);
                    UseOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", String.Empty);
                    UseMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", String.Empty);
                    UseVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", String.Empty);
                    UseScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", String.Empty);
                    imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", String.Empty);
                    minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", String.Empty);
                    defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", String.Empty);
                    ScraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", String.Empty);
                    ScraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", String.Empty);
                    ScraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", String.Empty);   
                    scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", String.Empty);
                    UseAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", String.Empty);
                    defaultBackdropIsImage = xmlreader.GetValueAsString("FanartHandler", "defaultBackdropIsImage", String.Empty);
                    useDefaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "useDefaultBackdrop", String.Empty);
                    proxyHostname = xmlreader.GetValueAsString("FanartHandler", "proxyHostname", String.Empty);
                    proxyPort = xmlreader.GetValueAsString("FanartHandler", "proxyPort", String.Empty);
                    proxyUsername = xmlreader.GetValueAsString("FanartHandler", "proxyUsername", String.Empty);
                    proxyPassword = xmlreader.GetValueAsString("FanartHandler", "proxyPassword", String.Empty);
                    proxyDomain = xmlreader.GetValueAsString("FanartHandler", "proxyDomain", String.Empty);
                    useProxy = xmlreader.GetValueAsString("FanartHandler", "useProxy", String.Empty);                   
                }
                
                string tmpFile = Config.GetFolder(Config.Dir.Config) + @"\XFactor.xml";
                if (File.Exists(tmpFile))
                {
                    using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "XFactor.xml")))
                    {

                        string tmpUse = xmlreader.GetValueAsString("XFactor", "useBasichomeFade", "");
                        if (tmpUse == null || tmpUse.Length < 1)
                        {
                            UseBasichomeFade = true;
                        }
                        else
                        {
                            if (tmpUse.Equals("Enabled"))
                            {
                                UseBasichomeFade = true;
                            }
                            else
                            {
                                UseBasichomeFade = false;
                            }
                        }

                        string tmpFadeTime = xmlreader.GetValueAsString("XFactor", "basichomeFadeTime", "");
                        if (tmpFadeTime == null || tmpFadeTime.Length < 1)
                        {
                            BasichomeFadeTime = 5;
                        }
                        else
                        {
                            BasichomeFadeTime = Int32.Parse(tmpFadeTime);
                        }
                    }
                }
                else
                {
                    UseBasichomeFade = false;
                    BasichomeFadeTime = 5;
                }

                if (UseFanart != null && UseFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseFanart = "True";
                }
                if (useProxy != null && useProxy.Length > 0)
                {
                    logger.Info("Proxy is used.");    
                }
                else
                {
                    logger.Info("Proxy is not used.");    
                    useProxy = "False";
                }
                if (proxyHostname != null && proxyHostname.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyHostname = String.Empty;
                }
                if (proxyPort != null && proxyPort.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyPort = String.Empty;
                }
                if (proxyUsername != null && proxyUsername.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyUsername = String.Empty;
                }
                if (proxyPassword != null && proxyPassword.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyPassword = String.Empty;
                }
                if (proxyDomain != null && proxyDomain.Length > 0)
                {
                    //donothing
                }
                else
                {
                    proxyDomain = String.Empty;
                }                
                if (UseAlbum != null && UseAlbum.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseAlbum = "True";
                }
                if (useDefaultBackdrop != null && useDefaultBackdrop.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useDefaultBackdrop = "True";
                }                
                if (UseArtist != null && UseArtist.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseArtist = "True";
                }
                if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Length > 0)
                {
                    //donothing
                }
                else
                {
                    skipWhenHighResAvailable = "True";
                }
                if (DisableMPTumbsForRandom != null && DisableMPTumbsForRandom.Length > 0)
                {
                    //donothing
                }
                else
                {
                    DisableMPTumbsForRandom = "True";
                }
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Length > 0)
                {
                    //donothing
                }
                else
                {
                    defaultBackdropIsImage = "True";
                }                
                if (UseOverlayFanart != null && UseOverlayFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseOverlayFanart = "True";
                }
                if (UseMusicFanart != null && UseMusicFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseMusicFanart = "True";
                }
                if (UseVideoFanart != null && UseVideoFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseVideoFanart = "True";
                }
                if (UseScoreCenterFanart != null && UseScoreCenterFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseScoreCenterFanart = "True";
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
                if (ScraperMaxImages != null && ScraperMaxImages.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMaxImages = "2";
                }
                if (ScraperMusicPlaying != null && ScraperMusicPlaying.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMusicPlaying = "False";
                }
                if (ScraperMPDatabase != null && ScraperMPDatabase.Length > 0)
                {
                    //donothing
                }
                else
                {
                    ScraperMPDatabase = "False";
                }
                if (scraperInterval != null && scraperInterval.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scraperInterval = "24";
                }
                if (UseAspectRatio != null && UseAspectRatio.Length > 0)
                {
                    //donothing
                }
                else
                {
                    UseAspectRatio = "False";
                }
                fp = new FanartPlaying();
                fs = new FanartSelected();
                fr = new FanartRandom();
                fr.SetupWindowsUsingRandomImages();
                SetupWindowsUsingFanartHandlerVisibility();
                SetupVariables();
                SetupDirectories();
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Equals("True"))
                {
                    DefaultBackdropImages.Add(0, defaultBackdrop);
                }
                else
                {
                    int i = 0;
                    SetupDefaultBackdrops(defaultBackdrop, ref i);
                    Utils.Shuffle(ref defaultBackdropImages);
                }
                logger.Info("Fanart Handler is using Fanart: " + UseFanart + ", Album Thumbs: " + UseAlbum + ", Artist Thumbs: " + UseArtist + ".");
                Utils.SetUseProxy(useProxy);
                Utils.SetProxyHostname(proxyHostname);
                Utils.SetProxyPort(proxyPort);
                Utils.SetProxyUsername(proxyUsername);
                Utils.SetProxyPassword(proxyPassword);
                Utils.SetProxyDomain(proxyDomain);
                Utils.SetScraperMaxImages(ScraperMaxImages);
                Utils.InitiateDbm();
                M_Db = MusicDatabase.Instance;
                myDirectoryTimer = new TimerCallback(UpdateDirectoryTimer);
                directoryTimer = new System.Threading.Timer(myDirectoryTimer, null, 60000, 3600000);                       
                InitRandomProperties();
                if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True"))
                {
                    //startScraper();
                    myScraperTimer = new TimerCallback(UpdateScraperTimer);
                    int iScraperInterval = Convert.ToInt32(scraperInterval);
                    iScraperInterval = iScraperInterval * 3600000;
                    scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, iScraperInterval);
                }
                Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
                g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                refreshTimer = new System.Timers.Timer(1000);
                refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
                refreshTimer.Interval = 1000;
                string windowId = "35";
                if (fr.WindowsUsingFanartRandom.ContainsKey(windowId) || fs.WindowsUsingFanartSelected.ContainsKey(windowId) || (fp.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True"))))
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


            
        private void OnSystemPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e) 
        {
            if (e.Mode == Microsoft.Win32.PowerModes.Resume) 
            {
                logger.Info("Fanart Handler is resuming from standby/hibernate.");
                StopTasks(false);
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
                string windowId = String.Empty+activeWindowId;
                PreventRefresh = true;
                if ((fr.WindowsUsingFanartRandom.ContainsKey(windowId) || fs.WindowsUsingFanartSelected.ContainsKey(windowId) || fp.WindowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId))
                {
                    if (Utils.GetDbm().GetIsScraping())
                    {
                        GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
                    }
                    else
                    {
                        GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
                        GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
                        Utils.GetDbm().TotArtistsBeingScraped = 0;
                        Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    }
                    if (fs.WindowsUsingFanartSelected.ContainsKey(windowId))
                    {                        
                        if (fs.DoShowImageOne)
                        {
                            fs.ShowImageTwo(activeWindowId);
                        }
                        else
                        {
                            fs.ShowImageOne(activeWindowId);
                        }
                        if (fs.FanartAvailable)
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
                    if ((fp.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True"))) && AllowFanartInThisWindow(windowId))
                    {
                        //if (windowId.Equals("730718") || (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (CurrentTrackTag != null && CurrentTrackTag.Length > 0)))) )
                        if (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (CurrentTrackTag != null && CurrentTrackTag.Length > 0))))
                        {
                            if (fp.DoShowImageOnePlay)
                            {
                                fp.ShowImageTwoPlay(activeWindowId);
                            }
                            else
                            {
                                fp.ShowImageOnePlay(activeWindowId);
                            }
                            if (fp.FanartAvailablePlay)
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
                            if (IsPlaying)
                            {
                                StopScraperNowPlaying();
                                EmptyAllImages(ref fp.listPlayMusic);
                                fp.SetCurrentArtistsImageNames(null);
                                fp.currPlayMusic = String.Empty;
                                fp.CurrPlayMusicArtist = String.Empty;
                                fp.FanartAvailablePlay = false;
                                fp.FanartIsNotAvailablePlay(activeWindowId);
                                fp.prevPlayMusic = -1;
                                SetProperty("#fanarthandler.music.overlay.play", TmpImage);
                                SetProperty("#fanarthandler.music.backdrop1.play", TmpImage);
                                SetProperty("#fanarthandler.music.backdrop2.play", TmpImage);
                                fp.CurrCountPlay = 0;
                                fp.UpdateVisibilityCountPlay = 0;
                                IsPlaying = false;
                            }
                            else
                            {
                                fp.FanartIsNotAvailablePlay(activeWindowId);
                            }
                        }
                    }
                    if (fr.WindowsUsingFanartRandom.ContainsKey(windowId))
                    {
                        fr.WindowOpen = true;
                        if (fr.DoShowImageOneRandom)
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
                    }
                }
                else
                {
                    if (refreshTimer != null && refreshTimer.Enabled)
                    {
                        refreshTimer.Stop();                 
                        EmptyAllFanartHandlerProperties();
                    }
                }
                PreventRefresh = false;
            }
            catch (Exception ex)
            {
                PreventRefresh = false;
                logger.Error("GUIWindowManager_OnActivateWindow: " + ex.ToString());
            }
        }

        private void EmptyAllFanartHandlerProperties()
        {
            try
            {
                if (IsPlaying)
                {
                    StopScraperNowPlaying();
                    EmptyAllImages(ref fp.listPlayMusic);
                    fp.SetCurrentArtistsImageNames(null);
                    fp.currPlayMusic = String.Empty;
                    fp.CurrPlayMusicArtist = String.Empty;
                    fp.FanartAvailablePlay = false;
                    fp.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                    fp.prevPlayMusic = -1;
                    SetProperty("#fanarthandler.music.overlay.play", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop1.play", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop2.play", TmpImage);
                    fp.CurrCountPlay = 0;
                    fp.UpdateVisibilityCountPlay = 0;
                    IsPlaying = false;
                }
                if (IsSelectedMusic)
                {
                    EmptyAllImages(ref fs.listSelectedMusic);
                    fs.currSelectedMusic = String.Empty;
                    fs.currSelectedMusicArtist = String.Empty;
                    fs.SetCurrentArtistsImageNames(null);
                    fs.CurrCount = 0;
                    fs.UpdateVisibilityCount = 0;
                    SetProperty("#fanarthandler.music.backdrop1.selected", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop2.selected", TmpImage);
                    IsSelectedMusic = false;
                }
                if (IsSelectedVideo)
                {
                    EmptyAllImages(ref fs.listSelectedMovies);
                    fs.currSelectedMovie = String.Empty;
                    fs.currSelectedMovieTitle = String.Empty;
                    fs.SetCurrentArtistsImageNames(null);
                    fs.CurrCount = 0;
                    fs.UpdateVisibilityCount = 0;
                    SetProperty("#fanarthandler.movie.backdrop1.selected", TmpImage);
                    SetProperty("#fanarthandler.movie.backdrop2.selected", TmpImage);
                    IsSelectedVideo = false;
                }
                if (IsSelectedScoreCenter)
                {
                    EmptyAllImages(ref fs.listSelectedScorecenter);
                    fs.currSelectedScorecenter = String.Empty;
                    fs.CurrSelectedScorecenterGenre = String.Empty;
                    fs.SetCurrentArtistsImageNames(null);
                    fs.CurrCount = 0;
                    fs.UpdateVisibilityCount = 0;
                    SetProperty("#fanarthandler.scorecenter.backdrop1.selected", TmpImage);
                    SetProperty("#fanarthandler.scorecenter.backdrop2.selected", TmpImage);
                    IsSelectedScoreCenter = false;
                }
                if (IsRandom)
                {
                    SetProperty("#fanarthandler.games.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.movie.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.movingpicture.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.picture.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.scorecenter.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.tvseries.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.tv.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.plugins.backdrop1.any", TmpImage);
                    SetProperty("#fanarthandler.games.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.movie.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.movingpicture.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.picture.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.scorecenter.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.tvseries.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.tv.backdrop2.any", TmpImage);
                    SetProperty("#fanarthandler.plugins.backdrop2.any", TmpImage);
                    fr.currCountRandom = 0;
                    fr.CountSetVisibility = 0;
                    fr.ClearPropertiesRandom();
                    fr.UpdateVisibilityCountRandom = 0;
                    EmptyAllImages(ref fr.listAnyGames);
                    EmptyAllImages(ref fr.listAnyMovies);
                    EmptyAllImages(ref fr.listAnyMovingPictures);
                    EmptyAllImages(ref fr.listAnyMusic);
                    EmptyAllImages(ref fr.listAnyPictures);
                    EmptyAllImages(ref fr.listAnyScorecenter);
                    EmptyAllImages(ref fr.listAnyTVSeries);
                    EmptyAllImages(ref fr.listAnyTV);
                    EmptyAllImages(ref fr.listAnyPlugins);
                    IsRandom = false;
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
                IsPlaying = true;
                if ((fp.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True"))) && AllowFanartInThisWindow(windowId))
                {
                    if (refreshTimer != null && !refreshTimer.Enabled && (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename) || windowId.Equals("730718")))
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



        private void CreateDirectoryIfMissing(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
        }

        private void SetupDirectories()
        {
            try
            {
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                CreateDirectoryIfMissing(path);
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                CreateDirectoryIfMissing(path);
            }
            catch (Exception ex)
            {
                logger.Error("setupDirectories: " + ex.ToString());
            }
        }

        private void StartScraper()
        {
            try
            {
                Utils.GetDbm().TotArtistsBeingScraped = 0;
                Utils.GetDbm().CurrArtistsBeingScraped = 0;

                MyScraperWorker = new ScraperWorker();
                MyScraperWorker.ProgressChanged += MyScraperWorker.OnProgressChanged;
                MyScraperWorker.RunWorkerCompleted += MyScraperWorker.OnRunWorkerCompleted;
                MyScraperWorker.RunWorkerAsync();                    

            }
            catch (Exception ex)
            {
                logger.Error("startScraper: " + ex.ToString());
            }
        }

        public static void StartScraperNowPlaying(string artist)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;

                    MyScraperNowWorker = new ScraperNowWorker();
                    MyScraperNowWorker.ProgressChanged += MyScraperNowWorker.OnProgressChanged;
                    MyScraperNowWorker.RunWorkerCompleted += MyScraperNowWorker.OnRunWorkerCompleted;
                    MyScraperNowWorker.RunWorkerAsync(artist);          
                }
            }
            catch (Exception ex)
            {
                logger.Error("startScraperNowPlaying: " + ex.ToString());
            }
        }

        public static void StopScraperNowPlaying()
        {
            try
            {
                if (MyScraperNowWorker != null)
                {
                    MyScraperNowWorker.CancelAsync();
                    MyScraperNowWorker.Dispose();
                }

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

        public static void HideScraperProgressIndicator()
        {
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
        }

        public static void ShowScraperProgressIndicator()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
        }

        private void SetupConfigFile()
        {
            try
            {                
                String path = Config.GetFolder(Config.Dir.Config) + @"\FanartHandler.xml";
                String pathOrg = Config.GetFolder(Config.Dir.Config) + @"\FanartHandler.org";
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
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().StopScraper = true;
                }
                GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GUIWindowManager_OnActivateWindow);
                g_Player.PlayBackStarted -= new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                int ix = 0;
                while (Utils.GetDelayStop() && ix < 12)
                {
                    System.Threading.Thread.Sleep(500);
                    ix++;
                }
                StopScraperNowPlaying();
                if (scraperTimer != null)
                {
                    scraperTimer.Dispose();
                }
                if (refreshTimer != null)
                {
                    refreshTimer.Stop();
                    refreshTimer.Dispose();
                }
                if (MyScraperWorker != null)
                {
                    MyScraperWorker.CancelAsync();
                    MyScraperWorker.Dispose();
                }
                if (MyScraperNowWorker != null)
                {
                    MyScraperNowWorker.CancelAsync();
                    MyScraperNowWorker.Dispose();
                }
                if (MyDirectoryWorker != null)
                {
                    MyDirectoryWorker.CancelAsync();
                    MyDirectoryWorker.Dispose();
                }
                if (MyRefreshWorker != null)
                {
                    MyRefreshWorker.CancelAsync();
                    MyRefreshWorker.Dispose();
                }
                if (directoryTimer != null)
                {                    
                    directoryTimer.Dispose();
                }
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().Close();
                }
                if (fr != null && fr.listAnyGames != null)
                {
                    EmptyAllImages(ref fr.listAnyGames);
                }
                if (fr != null && fr.listAnyMovies != null)
                {
                    EmptyAllImages(ref fr.listAnyMovies);
                }
                if (fs != null && fs.listSelectedMovies != null)
                {
                    EmptyAllImages(ref fs.listSelectedMovies);
                }
                if (fr != null && fr.listAnyMovingPictures != null)
                {
                    EmptyAllImages(ref fr.listAnyMovingPictures);
                }
                if (fr != null && fr.listAnyMusic != null)
                {
                    EmptyAllImages(ref fr.listAnyMusic);
                }
                if (fp != null && fp.listPlayMusic != null)
                {
                    EmptyAllImages(ref fp.listPlayMusic);
                }
                if (fr != null && fr.listAnyPictures != null)
                {
                    EmptyAllImages(ref fr.listAnyPictures);
                }
                if (fr != null && fr.listAnyScorecenter != null)
                {
                    EmptyAllImages(ref fr.listAnyScorecenter);
                }
                if (fs != null && fs.listSelectedMusic != null)
                {
                    EmptyAllImages(ref fs.listSelectedMusic);
                }
                if (fs != null && fs.listSelectedScorecenter != null)
                {
                    EmptyAllImages(ref fs.listSelectedScorecenter);
                }
                if (fr != null && fr.listAnyTVSeries != null)
                {
                    EmptyAllImages(ref fr.listAnyTVSeries);
                }
                if (fr != null && fr.listAnyTV != null)
                {
                    EmptyAllImages(ref fr.listAnyTV);
                }
                if (fr != null && fr.listAnyPlugins != null)
                {
                    EmptyAllImages(ref fr.listAnyPlugins);
                }
                if (!suspending)
                {
                    Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                }
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
          
    }
}
