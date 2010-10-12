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
        private const string LogFileName = "fanarthandler.log";  //log's filename
        private const string OldLogFileName = "fanarthandler.old.log";  //log's old filename

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
        private static bool isPlaying/* = false*/; //hold true if MP plays music        
        private static bool isSelectedMusic/* = false*/;
        private static bool isSelectedVideo/* = false*/;
        private static bool isSelectedScoreCenter/* = false*/;
        private static bool isRandom/* = false*/;        
        private static string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";        
        private static bool preventRefresh/* = false*/;        
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
        public static FanartPlaying FP = null;
        public static FanartSelected FS = null;
        public static FanartRandom FR = null;
        public static int SyncPointRefresh/* = 0*/;
        public static int SyncPointDirectory/* = 0*/;
        public static int SyncPointScraper/* = 0*/;
        private static int basichomeFadeTime = 5;        
        private static bool useBasichomeFade = true;
        private static string m_CurrentTitleTag = null;
        private static string scrapeThumbnails = null;
        private static string latestPictures = null;
        private static string latestMusic = null;
        private static string latestMovingPictures = null;
        private static string latestTVSeries = null;        
        private static string latestTVRecordings = null;        
        #endregion

        public static string LatestTVRecordings
        {
            get { return FanartHandlerSetup.latestTVRecordings; }
            set { FanartHandlerSetup.latestTVRecordings = value; }
        }


        public static string LatestTVSeries
        {
            get { return FanartHandlerSetup.latestTVSeries; }
            set { FanartHandlerSetup.latestTVSeries = value; }
        }

        public static string LatestMovingPictures
        {
            get { return FanartHandlerSetup.latestMovingPictures; }
            set { FanartHandlerSetup.latestMovingPictures = value; }
        }

        public static string LatestMusic
        {
            get { return FanartHandlerSetup.latestMusic; }
            set { FanartHandlerSetup.latestMusic = value; }
        }

        public static string LatestPictures
        {
            get { return FanartHandlerSetup.latestPictures; }
            set { FanartHandlerSetup.latestPictures = value; }
        }

        public static string ScrapeThumbnails
        {
            get { return FanartHandlerSetup.scrapeThumbnails; }
            set { FanartHandlerSetup.scrapeThumbnails = value; }
        }

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

        public static string FHThreadPriority
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

        public static MusicDatabase MDB
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
                        UNLoadImage(al[i].ToString());

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
                        UNLoadImage(obj.ToString());
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
                //if (String.IsNullOrEmpty(value)) 20101008
                //    value = " ";

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
                double mWidth = Convert.ToInt32(minResolution.Substring(0, minResolution.IndexOf("x", StringComparison.CurrentCulture)), CultureInfo.CurrentCulture);
                double mHeight = Convert.ToInt32(minResolution.Substring(minResolution.IndexOf("x", StringComparison.CurrentCulture) + 1), CultureInfo.CurrentCulture);
                double imageWidth = checkImage.Width;
                double imageHeight = checkImage.Height;
                checkImage.Dispose();
                checkImage = null;
                if (imageWidth >= mWidth && imageHeight >= mHeight)
                {
                    if (useAspectRatio.Equals("True", StringComparison.CurrentCulture))
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
        public static void SetupFilenames(string s, string filter, string type, int restricted)
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
                        useFilter = new DateTime(1970, 1, 1, 1, 1, 1).ToString(CultureInfo.CurrentCulture);
                    }
                    DirectoryInfo dir1 = new DirectoryInfo(s);
                    DateTime dt = Convert.ToDateTime(useFilter, CultureInfo.CurrentCulture);
                    FileInfo[] fileList = dir1.GetFiles(filter, SearchOption.AllDirectories);
                    var query = from fi in fileList
                                where fi.CreationTime >= dt
                                select fi.FullName;
                    foreach (string dir in query)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        artist = Utils.GetArtist(dir, type);
                        if (type.Equals("MusicAlbum", StringComparison.CurrentCulture) || type.Equals("MusicArtist", StringComparison.CurrentCulture) || type.Equals("MusicFanart", StringComparison.CurrentCulture))
                        {
                            if (Utils.GetFilenameNoPath(dir).ToLower(CultureInfo.CurrentCulture).StartsWith("default", StringComparison.CurrentCulture))
                            {
                                type = "Default";
                            }
                            Utils.GetDbm().LoadMusicFanart(artist, dir, dir, type,0);
                            type = typeOrg;
                        }
                        else
                        {
                            Utils.GetDbm().LoadFanart(artist, dir, dir, type, restricted);
                        }
                    }
                    fileList = null;
                    dir1 = null;
                    if (Utils.GetIsStopping() == false)
                    {
                        Utils.GetDbm().SetTimeStamp("Directory - " + s, DateTime.Now.ToString(CultureInfo.CurrentCulture));
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
        public static void SetupFilenamesExternal(string s, string filter, string type, int restricted, Hashtable ht)
        {
            string artist = String.Empty;
//            string typeOrg = type // COMMENTED BY CODEIT.RIGHT;
            try
            {
                if (Directory.Exists(s))
                {
                    string useFilter = Utils.GetDbm().GetTimeStamp("Directory Ext - " + s);
                    if (useFilter == null || useFilter.Length < 2)
                    {
                        useFilter = new DateTime(1970, 1, 1, 1, 1, 1).ToString(CultureInfo.CurrentCulture);
                    }
                    DirectoryInfo dir1 = new DirectoryInfo(s);
                    DateTime dt = Convert.ToDateTime(useFilter, CultureInfo.CurrentCulture);
                    FileInfo[] fileList = dir1.GetFiles(filter, SearchOption.AllDirectories);
                    var query = from fi in fileList
                                where fi.CreationTime >= dt
                                select fi.FullName;
                    foreach (string dir in query)
                    {
                        if (Utils.GetIsStopping())
                        {
                            break;
                        }
                        artist = Utils.GetArtist(dir, type);
                        if (ht != null && ht.Contains(artist))
                        {
                            Utils.GetDbm().LoadFanartExternal(ht[artist].ToString(), dir, dir, type, restricted);
                        }                        
                    }
                    fileList = null;
                    dir1 = null;
                    if (Utils.GetIsStopping() == false)
                    {
                        Utils.GetDbm().SetTimeStamp("Directory Ext - " + s, DateTime.Now.ToString(CultureInfo.CurrentCulture));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("SetupFilenamesExternal: " + ex.ToString());
            }
        }

        

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        public void SetupDefaultBackdrops(string startDir, ref int i)
        {
            if (useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
            {
                try
                {
                    // Process the list of files found in the directory
                    var files = Directory.GetFiles(startDir, "*.*").Where(s => s.EndsWith(".jpg", StringComparison.CurrentCulture) || s.EndsWith(".jpeg", StringComparison.CurrentCulture) || s.EndsWith(".png", StringComparison.CurrentCulture));
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
                    int sync = Interlocked.CompareExchange(ref SyncPointDirectory, 1, 0);
                    if (sync == 0)
                    {
                        // No other event was executing.                        
                        MyDirectoryWorker = new DirectoryWorker();
                        MyDirectoryWorker.RunWorkerAsync();
                    }
                }
                catch (Exception ex)
                {
                    SyncPointDirectory = 0;
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
                    if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && Utils.GetDbm().GetIsScraping() == false)
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
            int restricted = 0;
            if (type.Equals("Movie", StringComparison.CurrentCulture) || type.Equals("MovingPicture", StringComparison.CurrentCulture) || type.Equals("myVideos", StringComparison.CurrentCulture) || type.Equals("Online Videos", StringComparison.CurrentCulture) || type.Equals("TV Section", StringComparison.CurrentCulture))
            {
                try
                {
                    restricted = UtilsExternal.MovingPictureIsRestricted();
                }
                catch { }
            }
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    key = Utils.GetArtist(key, type);
                    Hashtable tmp = null;
                    if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                    {
                        tmp = FP.GetCurrentArtistsImageNames();
                    }
                    else
                    {
                        tmp = FS.GetCurrentArtistsImageNames();
                    }
                    if (newArtist || tmp == null || tmp.Count == 0)
                    {
                        if (isMusic && skipWhenHighResAvailable != null && skipWhenHighResAvailable.Equals("True", StringComparison.CurrentCulture))
                        {
                            tmp = Utils.GetDbm().GetHigResFanart(key, restricted);
                        }
                        if (tmp != null && tmp.Count > 0)
                        {
                            //do nothing
                        }
                        else
                        {
                            tmp = Utils.GetDbm().GetFanart(key, type, restricted);
                        }
                        Utils.Shuffle(ref tmp);
                        if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                        {
                            FP.SetCurrentArtistsImageNames(tmp);
                        }
                        else
                        {
                            FS.SetCurrentArtistsImageNames(tmp);
                        }
                    }
                    if (tmp != null && tmp.Count > 0)
                    {
                        ICollection valueColl = tmp.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (FanartHandler.FanartImage s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (CheckImageResolution(s.DiskImage, type, UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                {                                    
                                    sout = s.DiskImage;
                                    iFilePrev = iFile;
                                    currFile = s.DiskImage;
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
                            foreach (FanartHandler.FanartImage s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (CheckImageResolution(s.DiskImage, type, UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                    {
                                        sout = s.DiskImage;
                                        iFilePrev = iFile;
                                        currFile = s.DiskImage;
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
                    if (obj.Equals("FanartPlaying", StringComparison.CurrentCulture))
                    {
                        FP.SetCurrentArtistsImageNames(null);
                    }
                    else
                    {
                        FS.SetCurrentArtistsImageNames(null);
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
                if (Utils.GetIsStopping() == false && useDefaultBackdrop.Equals("True", StringComparison.CurrentCulture))
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
//  COMMENTED BY CODEIT.RIGHT
//        private string ParseArtist(string s)
//        {
//            try
//            {
//                if (s != null && s.Length > 0 && s.IndexOf(".") >= 0)
//                {
//                    s = s.Trim();
//                    s = s.Substring(s.IndexOf(".") + 1);
//                    s = s.Trim();
//                }
//                if (s != null && s.Length > 0 && s.IndexOf("-") >= 0)
//                {
//                    s = s.Trim();
//                    s = s.Substring(0, s.IndexOf("-"));
//                    s = s.Trim();
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.Error("ParseArtist: " + ex.ToString());  
//            }
//            return s;
//        }

        private static void ResetCounters()
        {
            if (FS.CurrCount > MaxCountImage)
            {
                FS.CurrCount = 0;
                FS.HasUpdatedCurrCount = false;
            }
            if (FS.UpdateVisibilityCount > 5)
            {
                FS.UpdateVisibilityCount = 1;
            }
            if (FP.CurrCountPlay > MaxCountImage)
            {
                FP.CurrCountPlay = 0;
                FP.HasUpdatedCurrCountPlay = false;
            }
            if (FP.UpdateVisibilityCountPlay > 5)
            {
                FP.UpdateVisibilityCountPlay = 1;
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
                if (FS.UpdateVisibilityCount == 2)  //after 2 sek
                {
                    FS.UpdateProperties();
                    if (FS.DoShowImageOne)
                    {
                        FS.ShowImageOne(windowId);
                        FS.DoShowImageOne = false;
                    }
                    else
                    {
                        FS.ShowImageTwo(windowId);
                        FS.DoShowImageOne = true;
                    }
                    if (FS.FanartAvailable)
                    {
                        FS.FanartIsAvailable(windowId);
                    }
                    else
                    {
                        FS.FanartIsNotAvailable(windowId);
                    }                    
                }
                else if (FS.UpdateVisibilityCount == 5) //after 4 sek
                {
                    FS.UpdateVisibilityCount = 0;
                    //release unused image resources
                    HandleOldImages(ref FS.ListSelectedMovies);
                    HandleOldImages(ref FS.ListSelectedMusic);
                    HandleOldImages(ref FS.ListSelectedScorecenter);
                }
                if (FP.UpdateVisibilityCountPlay == 2)  //after 2 sek
                {
                    FP.UpdatePropertiesPlay();
                    if (FP.DoShowImageOnePlay)
                    {
                        FP.ShowImageOnePlay(windowId);
                        FP.DoShowImageOnePlay = false;
                    }
                    else
                    {
                        FP.ShowImageTwoPlay(windowId);
                        FP.DoShowImageOnePlay = true;
                    }
                    if (FP.FanartAvailablePlay)
                    {
                        FP.FanartIsAvailablePlay(windowId);
                    }
                    else
                    {
                        FP.FanartIsNotAvailablePlay(windowId);
                    }
                }
                else if (FP.UpdateVisibilityCountPlay == 5) //after 4 sek
                {
                    FP.UpdateVisibilityCountPlay = 0;
                    //release unused image resources
                    HandleOldImages(ref FP.ListPlayMusic);
                }                       
               
             /*   logger.Debug("listAnyGames: " + fr.listAnyGames.Count);
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
            FS.WindowsUsingFanartSelected = new Hashtable();
            FP.WindowsUsingFanartPlay = new Hashtable();
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
                                if (sNodeValue.StartsWith("#useSelectedFanart:Yes", StringComparison.CurrentCulture))
                                {
                                    try
                                    {
                                        FS.WindowsUsingFanartSelected.Add(windowId, windowId);
                                    }
                                    catch { }
                                }
                                if (sNodeValue.StartsWith("#usePlayFanart:Yes", StringComparison.CurrentCulture))
                                {
                                    try
                                    {
                                        FP.WindowsUsingFanartPlay.Add(windowId, windowId);
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
                    if (FR.WindowsUsingFanartRandom.ContainsKey("35"))
                    {
                        IsRandom = true;
                        FR.RefreshRandomImageProperties(null);
                        if (FR.UpdateVisibilityCountRandom > 0)
                        {
                            FR.UpdateVisibilityCountRandom = FR.UpdateVisibilityCountRandom + 1;
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
        private void UpdateImageTimer(Object stateInfo, ElapsedEventArgs e)
        {
            if (Utils.GetIsStopping() == false && !PreventRefresh)
            {
                try
                {
                    int sync = Interlocked.CompareExchange(ref SyncPointRefresh, 1, 0);
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
                    SyncPointRefresh = 0;
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
            FS.DoShowImageOne = true;
            FP.DoShowImageOnePlay = true;
            FR.DoShowImageOneRandom = true;
            FR.FirstRandom = true;
            FS.FanartAvailable = false;
            FP.FanartAvailablePlay = false;
            FS.UpdateVisibilityCount = 0;
            FP.UpdateVisibilityCountPlay = 0;
            FR.UpdateVisibilityCountRandom = 0;
            FS.CurrCount = 0;
            FP.CurrCountPlay = 0;
            FR.CurrCountRandom = 0;
            MaxCountImage = Convert.ToInt32(imageInterval, CultureInfo.CurrentCulture);
            FS.HasUpdatedCurrCount = false;
            FP.HasUpdatedCurrCountPlay = false;
            FS.PrevSelectedGeneric = -1;
            FP.PrevPlayMusic = -1;
            FS.PrevSelectedMusic = -1;
            FS.PrevSelectedScorecenter = -1;
            FS.CurrSelectedMovieTitle = String.Empty;
            FP.CurrPlayMusicArtist = String.Empty;
            FS.CurrSelectedMusicArtist = String.Empty;
            FS.CurrSelectedScorecenterGenre = String.Empty;
            FS.CurrSelectedMovie = String.Empty;
            FP.CurrPlayMusic = String.Empty;
            FS.CurrSelectedMusic = String.Empty;
            FS.CurrSelectedScorecenter = String.Empty;
            SyncPointRefresh = 0;
            SyncPointDirectory = 0;
            SyncPointScraper = 0;
            m_CurrentTrackTag = null;
            m_CurrentTitleTag = null;
            m_SelectedItem = null;
            SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
            SetProperty("#fanarthandler.scraper.task", String.Empty);
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
            SetProperty("#fanarthandler.movingpicture.latest1.thumb", "");
            SetProperty("#fanarthandler.movingpicture.latest1.fanart", "");
            SetProperty("#fanarthandler.movingpicture.latest1.title", "");
            SetProperty("#fanarthandler.movingpicture.latest1.dateAdded", "");
            SetProperty("#fanarthandler.movingpicture.latest1.genre", "");
            SetProperty("#fanarthandler.movingpicture.latest1.rating", "");
            SetProperty("#fanarthandler.movingpicture.latest1.roundedRating", "");
            SetProperty("#fanarthandler.movingpicture.latest1.classification", "");
            SetProperty("#fanarthandler.movingpicture.latest1.runtime", "");
            SetProperty("#fanarthandler.movingpicture.latest1.year", "");
            SetProperty("#fanarthandler.movingpicture.latest2.thumb", "");
            SetProperty("#fanarthandler.movingpicture.latest2.fanart", "");
            SetProperty("#fanarthandler.movingpicture.latest2.title", "");
            SetProperty("#fanarthandler.movingpicture.latest2.dateAdded", "");
            SetProperty("#fanarthandler.movingpicture.latest2.genre", "");
            SetProperty("#fanarthandler.movingpicture.latest2.rating", "");
            SetProperty("#fanarthandler.movingpicture.latest2.roundedRating", "");
            SetProperty("#fanarthandler.movingpicture.latest2.classification", "");
            SetProperty("#fanarthandler.movingpicture.latest2.runtime", "");
            SetProperty("#fanarthandler.movingpicture.latest2.year", "");
            SetProperty("#fanarthandler.movingpicture.latest3.thumb", "");
            SetProperty("#fanarthandler.movingpicture.latest3.fanart", "");
            SetProperty("#fanarthandler.movingpicture.latest3.title", "");
            SetProperty("#fanarthandler.movingpicture.latest3.dateAdded", "");
            SetProperty("#fanarthandler.movingpicture.latest3.genre", "");
            SetProperty("#fanarthandler.movingpicture.latest3.rating", "");
            SetProperty("#fanarthandler.movingpicture.latest3.roundedRating", "");
            SetProperty("#fanarthandler.movingpicture.latest3.classification", "");
            SetProperty("#fanarthandler.movingpicture.latest3.runtime", "");
            SetProperty("#fanarthandler.movingpicture.latest3.year", "");
            SetProperty("#fanarthandler.music.latest1.thumb", "");
            SetProperty("#fanarthandler.music.latest1.artist", "");
            SetProperty("#fanarthandler.music.latest1.album", "");
            SetProperty("#fanarthandler.music.latest1.dateAdded", "");
            SetProperty("#fanarthandler.music.latest2.thumb", "");
            SetProperty("#fanarthandler.music.latest2.artist", "");
            SetProperty("#fanarthandler.music.latest2.album", "");
            SetProperty("#fanarthandler.music.latest2.dateAdded", "");
            SetProperty("#fanarthandler.music.latest3.thumb", "");
            SetProperty("#fanarthandler.music.latest3.artist", "");
            SetProperty("#fanarthandler.music.latest3.album", "");
            SetProperty("#fanarthandler.music.latest3.dateAdded", "");
            SetProperty("#fanarthandler.picture.latest1.thumb", "");
            SetProperty("#fanarthandler.picture.latest1.filename", "");
            SetProperty("#fanarthandler.picture.latest1.dateAdded", "");
            SetProperty("#fanarthandler.picture.latest2.thumb", "");
            SetProperty("#fanarthandler.picture.latest2.filename", "");
            SetProperty("#fanarthandler.picture.latest2.dateAdded", "");
            SetProperty("#fanarthandler.picture.latest3.thumb", "");
            SetProperty("#fanarthandler.picture.latest3.filename", "");
            SetProperty("#fanarthandler.picture.latest3.dateAdded", "");
            SetProperty("#fanarthandler.tvseries.latest1.thumb", "");
            SetProperty("#fanarthandler.tvseries.latest1.fanart", "");
            SetProperty("#fanarthandler.tvseries.latest1.serieName", "");
            SetProperty("#fanarthandler.tvseries.latest1.seasonIndex", "");
            SetProperty("#fanarthandler.tvseries.latest1.episodeName", "");
            SetProperty("#fanarthandler.tvseries.latest1.episodeIndex", "");
            SetProperty("#fanarthandler.tvseries.latest1.dateAdded", "");
            SetProperty("#fanarthandler.tvseries.latest1.genre", "");
            SetProperty("#fanarthandler.tvseries.latest1.rating", "");
            SetProperty("#fanarthandler.tvseries.latest1.roundedRating", "");
            SetProperty("#fanarthandler.tvseries.latest1.classification", "");
            SetProperty("#fanarthandler.tvseries.latest1.runtime", "");
            SetProperty("#fanarthandler.tvseries.latest1.firstAired", "");
            SetProperty("#fanarthandler.tvseries.latest2.thumb", "");
            SetProperty("#fanarthandler.tvseries.latest2.fanart", "");
            SetProperty("#fanarthandler.tvseries.latest2.serieName", "");
            SetProperty("#fanarthandler.tvseries.latest2.seasonIndex", "");
            SetProperty("#fanarthandler.tvseries.latest2.episodeName", "");
            SetProperty("#fanarthandler.tvseries.latest2.episodeIndex", "");
            SetProperty("#fanarthandler.tvseries.latest2.dateAdded", "");
            SetProperty("#fanarthandler.tvseries.latest2.genre", "");
            SetProperty("#fanarthandler.tvseries.latest2.rating", "");
            SetProperty("#fanarthandler.tvseries.latest2.roundedRating", "");
            SetProperty("#fanarthandler.tvseries.latest2.classification", "");
            SetProperty("#fanarthandler.tvseries.latest2.runtime", "");
            SetProperty("#fanarthandler.tvseries.latest2.firstAired", "");
            SetProperty("#fanarthandler.tvseries.latest3.thumb", "");
            SetProperty("#fanarthandler.tvseries.latest3.fanart", "");
            SetProperty("#fanarthandler.tvseries.latest3.serieName", "");
            SetProperty("#fanarthandler.tvseries.latest3.seasonIndex", "");
            SetProperty("#fanarthandler.tvseries.latest3.episodeName", "");
            SetProperty("#fanarthandler.tvseries.latest3.episodeIndex", "");
            SetProperty("#fanarthandler.tvseries.latest3.dateAdded", "");
            SetProperty("#fanarthandler.tvseries.latest3.genre", "");
            SetProperty("#fanarthandler.tvseries.latest3.rating", "");
            SetProperty("#fanarthandler.tvseries.latest3.roundedRating", "");
            SetProperty("#fanarthandler.tvseries.latest3.classification", "");
            SetProperty("#fanarthandler.tvseries.latest3.runtime", "");
            SetProperty("#fanarthandler.tvseries.latest3.firstAired", "");
            SetProperty("#fanarthandler.tvrecordings.latest1.thumb", "");
            SetProperty("#fanarthandler.tvrecordings.latest1.title", "");
            SetProperty("#fanarthandler.tvrecordings.latest1.dateAdded", "");
            SetProperty("#fanarthandler.tvrecordings.latest1.genre", "");
            SetProperty("#fanarthandler.tvrecordings.latest2.thumb", "");
            SetProperty("#fanarthandler.tvrecordings.latest2.title", "");
            SetProperty("#fanarthandler.tvrecordings.latest2.dateAdded", "");
            SetProperty("#fanarthandler.tvrecordings.latest2.genre", "");
            SetProperty("#fanarthandler.tvrecordings.latest3.thumb", "");
            SetProperty("#fanarthandler.tvrecordings.latest3.title", "");
            SetProperty("#fanarthandler.tvrecordings.latest3.dateAdded", "");
            SetProperty("#fanarthandler.tvrecordings.latest3.genre", "");
            FS.Properties = new Hashtable();
            FP.PropertiesPlay = new Hashtable();
            FR.PropertiesRandom = new Hashtable();
            DefaultBackdropImages = new Hashtable();
            FR.ListAnyGames = new ArrayList();
            FR.ListAnyMovies = new ArrayList();
            FS.ListSelectedMovies = new ArrayList();
            FR.ListAnyMovingPictures = new ArrayList();
            FR.ListAnyMusic = new ArrayList();
            FP.ListPlayMusic = new ArrayList();
            FR.ListAnyPictures = new ArrayList();
            FR.ListAnyScorecenter = new ArrayList();
            FS.ListSelectedMusic = new ArrayList();
            FS.ListSelectedScorecenter = new ArrayList();
            FR.ListAnyTVSeries = new ArrayList();
            FR.ListAnyTV = new ArrayList();
            FR.ListAnyPlugins = new ArrayList();
            FR.RandAnyGames = new Random();
            FR.RandAnyMovies = new Random();
            FR.RandAnyMovingPictures = new Random();
            FR.RandAnyMusic = new Random();
            FR.RandAnyPictures = new Random();
            FR.RandAnyScorecenter = new Random();
            FR.RandAnyTVSeries = new Random();
            FR.RandAnyTV = new Random();
            FR.RandAnyPlugins = new Random();
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
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, LogFileName));
                if (logFile.Exists)
                {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, OldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, OldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, OldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, LogFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));

            string myThreadPriority = xmlreader.GetValue("general", "ThreadPriority");

            if (myThreadPriority != null && myThreadPriority.Equals("Normal", StringComparison.CurrentCulture))
            {
                FHThreadPriority = "Lowest";
            }
            else if (myThreadPriority != null && myThreadPriority.Equals("BelowNormal", StringComparison.CurrentCulture))
            {
                FHThreadPriority = "Lowest";
            }
            else
            {
                FHThreadPriority = "BelowNormal";
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
                    scrapeThumbnails = xmlreader.GetValueAsString("FanartHandler", "scrapeThumbnails", String.Empty);
                    latestPictures = xmlreader.GetValueAsString("FanartHandler", "latestPictures", String.Empty);
                    latestMusic = xmlreader.GetValueAsString("FanartHandler", "latestMusic", String.Empty);
                    latestMovingPictures = xmlreader.GetValueAsString("FanartHandler", "latestMovingPictures", String.Empty);
                    latestTVSeries = xmlreader.GetValueAsString("FanartHandler", "latestTVSeries", String.Empty);
                    latestTVRecordings = xmlreader.GetValueAsString("FanartHandler", "latestTVRecordings", String.Empty);   
                }

                if (latestPictures != null && latestPictures.Length > 0)
                {
                    //donothing
                }
                else
                {
                    latestPictures = "True";
                }

                if (latestMusic != null && latestMusic.Length > 0)
                {
                    //donothing
                }
                else
                {
                    latestMusic = "True";
                }

                if (latestMovingPictures != null && latestMovingPictures.Length > 0)
                {
                    //donothing
                }
                else
                {
                    latestMovingPictures = "True";
                }

                if (latestTVSeries != null && latestTVSeries.Length > 0)
                {
                    //donothing
                }
                else
                {
                    latestTVSeries = "True";
                }

                if (latestTVRecordings != null && latestTVRecordings.Length > 0)
                {
                    //donothing
                }
                else
                {
                    latestTVRecordings = "True";
                }

                if (scrapeThumbnails != null && scrapeThumbnails.Length > 0)
                {
                    //donothing
                }
                else
                {
                    scrapeThumbnails = "True";
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
                            if (tmpUse.Equals("Enabled", StringComparison.CurrentCulture))
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
                            BasichomeFadeTime = Int32.Parse(tmpFadeTime, CultureInfo.CurrentCulture);
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
                    if (useProxy.Equals("True", StringComparison.CurrentCulture))
                    {
                        logger.Info("Proxy is used.");
                    }
                    else
                    {
                        logger.Info("Proxy is not used.");    
                    }
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
                FP = new FanartPlaying();
                FS = new FanartSelected();
                FR = new FanartRandom();
                FR.SetupWindowsUsingRandomImages();
                SetupWindowsUsingFanartHandlerVisibility();
                SetupVariables();
                SetupDirectories();
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Equals("True", StringComparison.CurrentCulture))
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
                Utils.ScrapeThumbnails = scrapeThumbnails;
                Utils.InitiateDbm();
                MDB = MusicDatabase.Instance;
                myDirectoryTimer = new TimerCallback(UpdateDirectoryTimer);
                directoryTimer = new System.Threading.Timer(myDirectoryTimer, null, 500, 900000);                       
                InitRandomProperties();
                if (ScraperMPDatabase != null && ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture))
                {
                    //startScraper();
                    myScraperTimer = new TimerCallback(UpdateScraperTimer);
                    int iScraperInterval = Convert.ToInt32(scraperInterval, CultureInfo.CurrentCulture);
                    iScraperInterval = iScraperInterval * 3600000;
                    scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, iScraperInterval);
                }
                Microsoft.Win32.SystemEvents.PowerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                GUIWindowManager.OnActivateWindow += new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
                g_Player.PlayBackStarted += new MediaPortal.Player.g_Player.StartedHandler(OnPlayBackStarted);
                refreshTimer = new System.Timers.Timer(1000);
                refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
                refreshTimer.Interval = 1000;
                string windowId = "35";
                if (FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelected.ContainsKey(windowId) || (FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))))
                {
                    refreshTimer.Start();                    
                }

                try
                {
                    UtilsExternal.MovingPictureUpdateLatest(true);
                }
                catch 
                {
                }

                try
                {
                    UtilsExternal.TVSeriesUpdateLatest(true);
                }
                catch
                {
                }

                try
                {
                    UtilsExternal.SetupMovingPicturesLatest();
                }
                catch
                {
                }

                try
                {
                    UtilsExternal.SetupTVSeriesLatest();
                }
                catch
                {
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

        public void GuiWindowManagerOnActivateWindow(int activeWindowId)
        {
            try
            {
                int ix = 0;
                while (SyncPointRefresh != 0 && ix < 20)
                {
                    System.Threading.Thread.Sleep(400);                    
                    ix++;
                }
                string windowId = String.Empty+activeWindowId;
                PreventRefresh = true;
                if ((FR.WindowsUsingFanartRandom.ContainsKey(windowId) || FS.WindowsUsingFanartSelected.ContainsKey(windowId) || FP.WindowsUsingFanartPlay.ContainsKey(windowId)) && AllowFanartInThisWindow(windowId))
                {
                    if (Utils.GetDbm().GetIsScraping())
                    {
                        GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);
                    }
                    else
                    {
                        GUIPropertyManager.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scraper.task", String.Empty);
                        GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919280);
                        Utils.GetDbm().TotArtistsBeingScraped = 0;
                        Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    }
                    if (FS.WindowsUsingFanartSelected.ContainsKey(windowId))
                    {                        
                        if (FS.DoShowImageOne)
                        {
                            FS.ShowImageTwo(activeWindowId);
                        }
                        else
                        {
                            FS.ShowImageOne(activeWindowId);
                        }
                        if (FS.FanartAvailable)
                        {
                            FS.FanartIsAvailable(activeWindowId);
                        }
                        else
                        {
                            FS.FanartIsNotAvailable(activeWindowId);
                        }
                        if (refreshTimer != null && !refreshTimer.Enabled)
                        {
                            refreshTimer.Start();
                        }
                    }
                    if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))) && AllowFanartInThisWindow(windowId))
                    {
                        //if (windowId.Equals("730718") || (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (CurrentTrackTag != null && CurrentTrackTag.Length > 0)))) )
                        if (((g_Player.Playing || g_Player.Paused) && (g_Player.IsCDA || g_Player.IsMusic || g_Player.IsRadio || (CurrentTrackTag != null && CurrentTrackTag.Length > 0))))
                        {
                            if (FP.DoShowImageOnePlay)
                            {
                                FP.ShowImageTwoPlay(activeWindowId);
                            }
                            else
                            {
                                FP.ShowImageOnePlay(activeWindowId);
                            }
                            if (FP.FanartAvailablePlay)
                            {
                                FP.FanartIsAvailablePlay(activeWindowId);
                            }
                            else
                            {
                                FP.FanartIsNotAvailablePlay(activeWindowId);
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
                                EmptyAllImages(ref FP.ListPlayMusic);
                                FP.SetCurrentArtistsImageNames(null);
                                FP.CurrPlayMusic = String.Empty;
                                FP.CurrPlayMusicArtist = String.Empty;
                                FP.FanartAvailablePlay = false;
                                FP.FanartIsNotAvailablePlay(activeWindowId);
                                FP.PrevPlayMusic = -1;
                                SetProperty("#fanarthandler.music.overlay.play", TmpImage);
                                SetProperty("#fanarthandler.music.backdrop1.play", TmpImage);
                                SetProperty("#fanarthandler.music.backdrop2.play", TmpImage);
                                FP.CurrCountPlay = 0;
                                FP.UpdateVisibilityCountPlay = 0;
                                IsPlaying = false;
                            }
                            else
                            {
                                FP.FanartIsNotAvailablePlay(activeWindowId);
                            }
                        }
                    }
                    if (FR.WindowsUsingFanartRandom.ContainsKey(windowId))
                    {
                        FR.WindowOpen = true;
                        if (FR.DoShowImageOneRandom)
                        {
                            FR.ShowImageTwoRandom(activeWindowId);
                        }
                        else
                        {
                            FR.ShowImageOneRandom(activeWindowId);
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
                    EmptyAllImages(ref FP.ListPlayMusic);
                    FP.SetCurrentArtistsImageNames(null);
                    FP.CurrPlayMusic = String.Empty;
                    FP.CurrPlayMusicArtist = String.Empty;
                    FP.FanartAvailablePlay = false;
                    FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                    FP.PrevPlayMusic = -1;
                    SetProperty("#fanarthandler.music.overlay.play", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop1.play", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop2.play", TmpImage);
                    FP.CurrCountPlay = 0;
                    FP.UpdateVisibilityCountPlay = 0;
                    IsPlaying = false;
                }
                if (IsSelectedMusic)
                {
                    EmptyAllImages(ref FS.ListSelectedMusic);
                    FS.CurrSelectedMusic = String.Empty;
                    FS.CurrSelectedMusicArtist = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
                    SetProperty("#fanarthandler.music.backdrop1.selected", TmpImage);
                    SetProperty("#fanarthandler.music.backdrop2.selected", TmpImage);
                    IsSelectedMusic = false;
                }
                if (IsSelectedVideo)
                {
                    EmptyAllImages(ref FS.ListSelectedMovies);
                    FS.CurrSelectedMovie = String.Empty;
                    FS.CurrSelectedMovieTitle = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
                    SetProperty("#fanarthandler.movie.backdrop1.selected", TmpImage);
                    SetProperty("#fanarthandler.movie.backdrop2.selected", TmpImage);
                    IsSelectedVideo = false;
                }
                if (IsSelectedScoreCenter)
                {
                    EmptyAllImages(ref FS.ListSelectedScorecenter);
                    FS.CurrSelectedScorecenter = String.Empty;
                    FS.CurrSelectedScorecenterGenre = String.Empty;
                    FS.SetCurrentArtistsImageNames(null);
                    FS.CurrCount = 0;
                    FS.UpdateVisibilityCount = 0;
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
                    FR.CurrCountRandom = 0;
                    FR.CountSetVisibility = 0;
                    FR.ClearPropertiesRandom();
                    FR.UpdateVisibilityCountRandom = 0;
                    EmptyAllImages(ref FR.ListAnyGames);
                    EmptyAllImages(ref FR.ListAnyMovies);
                    EmptyAllImages(ref FR.ListAnyMovingPictures);
                    EmptyAllImages(ref FR.ListAnyMusic);
                    EmptyAllImages(ref FR.ListAnyPictures);
                    EmptyAllImages(ref FR.ListAnyScorecenter);
                    EmptyAllImages(ref FR.ListAnyTVSeries);
                    EmptyAllImages(ref FR.ListAnyTV);
                    EmptyAllImages(ref FR.ListAnyPlugins);
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
            if (windowId != null && windowId.Equals("511", StringComparison.CurrentCulture))
                return false;
            else if (windowId != null && windowId.Equals("2005", StringComparison.CurrentCulture))
                return false;
            else if (windowId != null && windowId.Equals("602", StringComparison.CurrentCulture))
                return false;
            else
                return true;
        }
        
        public void OnPlayBackStarted(g_Player.MediaType type, string filename)
        {
            try
            {
                string windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture);                                
                IsPlaying = true;
                if ((FP.WindowsUsingFanartPlay.ContainsKey(windowId) || (UseOverlayFanart != null && UseOverlayFanart.Equals("True", StringComparison.CurrentCulture))) && AllowFanartInThisWindow(windowId))
                {
                    if (refreshTimer != null && !refreshTimer.Enabled && (type == g_Player.MediaType.Music || type == g_Player.MediaType.Radio || MediaPortal.Util.Utils.IsLastFMStream(filename) || windowId.Equals("730718", StringComparison.CurrentCulture)))
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
                if (Utils.GetIsStopping() == false)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.SetDelayStop(true);
                    MyScraperWorker = new ScraperWorker();
                    MyScraperWorker.ProgressChanged += MyScraperWorker.OnProgressChanged;
                    MyScraperWorker.RunWorkerCompleted += MyScraperWorker.OnRunWorkerCompleted;
                    MyScraperWorker.RunWorkerAsync();                    
                }
            }
            catch (Exception ex)
            {
                Utils.SetDelayStop(false);
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
                    Utils.SetDelayStop(true);
                    MyScraperNowWorker = new ScraperNowWorker();
                    MyScraperNowWorker.ProgressChanged += MyScraperNowWorker.OnProgressChanged;
                    MyScraperNowWorker.RunWorkerCompleted += MyScraperNowWorker.OnRunWorkerCompleted;
                    MyScraperNowWorker.RunWorkerAsync(artist);          
                }
            }
            catch (Exception ex)
            {
                Utils.SetDelayStop(false);
                logger.Error("startScraperNowPlaying: " + ex.ToString());
            }
        }

        public static void StopScraperNowPlaying()
        {
            try
            {
                if (MyScraperNowWorker != null)
                {
                    Utils.SetDelayStop(false);
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
        private static void UNLoadImage(string filename)
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
                GUIWindowManager.OnActivateWindow -= new GUIWindowManager.WindowActivationHandler(GuiWindowManagerOnActivateWindow);
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
                try
                {
                    UtilsExternal.DisposeMovingPicturesLatest();
                }
                catch
                {
                }
                try
                {
                    UtilsExternal.DisposeTVSeriesLatest();
                }
                catch
                {
                }
                if (FR != null && FR.ListAnyGames != null)
                {
                    EmptyAllImages(ref FR.ListAnyGames);
                }
                if (FR != null && FR.ListAnyMovies != null)
                {
                    EmptyAllImages(ref FR.ListAnyMovies);
                }
                if (FS != null && FS.ListSelectedMovies != null)
                {
                    EmptyAllImages(ref FS.ListSelectedMovies);
                }
                if (FR != null && FR.ListAnyMovingPictures != null)
                {
                    EmptyAllImages(ref FR.ListAnyMovingPictures);
                }
                if (FR != null && FR.ListAnyMusic != null)
                {
                    EmptyAllImages(ref FR.ListAnyMusic);
                }
                if (FP != null && FP.ListPlayMusic != null)
                {
                    EmptyAllImages(ref FP.ListPlayMusic);
                }
                if (FR != null && FR.ListAnyPictures != null)
                {
                    EmptyAllImages(ref FR.ListAnyPictures);
                }
                if (FR != null && FR.ListAnyScorecenter != null)
                {
                    EmptyAllImages(ref FR.ListAnyScorecenter);
                }
                if (FS != null && FS.ListSelectedMusic != null)
                {
                    EmptyAllImages(ref FS.ListSelectedMusic);
                }
                if (FS != null && FS.ListSelectedScorecenter != null)
                {
                    EmptyAllImages(ref FS.ListSelectedScorecenter);
                }
                if (FR != null && FR.ListAnyTVSeries != null)
                {
                    EmptyAllImages(ref FR.ListAnyTVSeries);
                }
                if (FR != null && FR.ListAnyTV != null)
                {
                    EmptyAllImages(ref FR.ListAnyTV);
                }
                if (FR != null && FR.ListAnyPlugins != null)
                {
                    EmptyAllImages(ref FR.ListAnyPlugins);
                }
                if (!suspending)
                {
                    Microsoft.Win32.SystemEvents.PowerModeChanged -= new Microsoft.Win32.PowerModeChangedEventHandler(OnSystemPowerModeChanged);
                }
                FP = null;
                FS = null;
                FR = null;
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
