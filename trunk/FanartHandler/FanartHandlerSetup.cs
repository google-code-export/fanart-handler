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
        private bool isStopping = false;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string logFileName = "fanarthandler.log";
        private const string oldLogFileName = "fanarthandler.old.log";
        private ScraperWorker scraperWorkerObject;
        private ScraperWorkerNowPlaying scraperWorkerObjectNowPlaying;
        private Thread scrapeWorkerThread;
        private Thread scrapeWorkerThreadNowPlaying;
        private System.Timers.Timer refreshTimer = null;
        private TimerCallback myDirectoryTimer = null;
        private System.Threading.Timer directoryTimer = null;
        private TimerCallback myScraperTimer = null;
        private System.Threading.Timer scraperTimer = null;
        private TimerCallback myProgressTimer = null;
        private System.Threading.Timer progressTimer = null;
        private string m_CurrentTrackTag = null;  //is music playing and if so this holds current artist name
        private Hashtable randomWindows; //used to know what skin files that supports random images
        private Hashtable properties; //used to hold properties to be updated (Selected or Any)
        private Hashtable propertiesPlay; //used to hold properties to be updated (Play)
        private Hashtable propertiesRandom; //used to hold properties to be updated (Random)
        private DatabaseManager dbm;

        private Random randAnyGames = null;
        private Random randAnyMovies = null;
        private Random randAnyMovingPictures = null;
        private Random randAnyMusic = null;
        private Random randAnyPictures = null;
        private Random randAnyScorecenter = null;
        private Random randAnyTVSeries = null;
        private Random randAnyTV = null;
        private Random randAnyPlugins = null;

        private string scraperMaxImages = null;
        private string scraperMusicPlaying = null;
        private string scraperMPDatabase = null;
        private string scraperInterval = null;

        private bool doShowImageOne = true;
        private bool doShowImageOnePlay = true;
        private bool doShowImageOneRandom = true;        
        private bool hasUpdatedCurrCount = false;
        private bool hasUpdatedCurrCountPlay = false;
        private bool firstRandom = true;
        private bool fanartAvailable = false;
        private bool fanartAvailablePlay = false;
       

        private bool useAnyGames = false;
        private bool useAnyMusic = false;
        private bool useAnyMovies = false;
        private bool useAnyMovingPictures = false;
        private bool useAnyPictures = false;
        private bool useAnyScoreCenter = false;
        private bool useAnyTVSeries = false;
        private bool useAnyTV = false;
        private bool useAnyPlugins = false;
        
        private int maxCountImage = 30;
        private int currCount = 0;
        private int currCountPlay = 0;
        private int currCountRandom = 0;
        private int updateVisibilityCount = 0;
        private int updateVisibilityCountPlay = 0;
        private int updateVisibilityCountRandom = 0;

        private string currSelectedMovieTitle = null;
        private string currPlayMusicArtist = null;
        private string currSelectedMusicArtist = null;
        private string currSelectedScorecenterGenre = null;

        private string m_SelectedItem = null; //artist, album, title
        private string m_SelectedItem2 = null;  //artist in mymusicgenres

        private int prevSelectedGeneric = 0;
        private int prevPlayMusic = 0;
        private int prevSelectedMusic = 0;
        private int prevSelectedScorecenter = 0;

        private string currAnyGames = null;
        private string currAnyMovies = null;
        private string currSelectedMovie = null;
        private string currAnyMovingPictures = null;
        private string currAnyMusic = null;
        private string currPlayMusic = null;
        private string currAnyPictures = null;
        private string currAnyScorecenter = null;
        private string currSelectedMusic = null;
        private string currSelectedScorecenter = null;
        private string currAnyTVSeries = null;
        private string currAnyTV = null;
        private string currAnyPlugins = null;

        private ArrayList listAnyGames = null;
        private ArrayList listAnyMovies = null;
        private ArrayList listSelectedMovies = null;
        private ArrayList listAnyMovingPictures = null;
        private ArrayList listAnyMusic = null;
        private ArrayList listPlayMusic = null;
        private ArrayList listAnyPictures = null;
        private ArrayList listAnyScorecenter = null;
        private ArrayList listSelectedMusic = null;
        private ArrayList listSelectedScorecenter = null;
        private ArrayList listAnyTVSeries = null;
        private ArrayList listAnyTV = null;
        private ArrayList listAnyPlugins = null;

        private FanartHandlerConfig xconfig = null;
        private string useArtist = null;
        private string useAlbum = null;
        private string useArtistDisabled = null;
        private string useAlbumDisabled = null;
        private string useFanart = null;
        private string useOverlayFanart = null;
        private string useMusicFanart = null;
        private string useVideoFanart = null;
        private string useScoreCenterFanart = null;
        private string imageInterval = null;
        private string minResolution = null;
        private string defaultBackdrop = null;
        private string useAspectRatio = null;
        private MusicDatabase m_db = null;
        private MediaInfo mi = null;
        private List<Song> songInfo = null;        

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



        private void EmptyAllImages(ref ArrayList al)
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
        
        private void UpdateProperties()
        {
            try
            {
                foreach (DictionaryEntry de in properties)
                {
                    SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                properties.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdateProperties: " + ex.ToString());
            }
        }

        private void UpdatePropertiesPlay()
        {
            try
            {
                foreach (DictionaryEntry de in propertiesPlay)
                {
                    SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                propertiesPlay.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesPlay: " + ex.ToString());
            }
        }

        private void UpdatePropertiesRandom()
        {
            try
            {
                foreach (DictionaryEntry de in propertiesRandom)
                {
                    SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                propertiesRandom.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesRandom: " + ex.ToString());
            }
        }

        private void AddProperty(string property, string value, ref ArrayList al)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";
                if (properties.Contains(property))
                {
                    properties[property] = value;
                }
                else
                {
                    properties.Add(property, value);
                }

                //load images as MP resource
                if (value != null && value.Length > 0)
                {
                    //add new filename to list
                    if (al != null)
                    {
                        if (al.Contains(value) == false)
                        {
                            try
                            {
                                al.Add(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("AddProperty: " + ex.ToString());
                            }
                            LoadImage(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddProperty: " + ex.ToString());
            }
        }

        private void AddPropertyPlay(string property, string value, ref ArrayList al)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";
                if (propertiesPlay.Contains(property))
                {
                    propertiesPlay[property] = value;
                }
                else
                {
                    propertiesPlay.Add(property, value);
                }

                if (value != null && value.Length > 0)
                {
                    if (al != null)
                    {
                        if (al.Contains(value) == false)
                        {
                            try
                            {
                                al.Add(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("AddPropertyPlay: " + ex.ToString());
                            }
                            LoadImage(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddPropertyPlay: " + ex.ToString());
            }
        }

        private void AddPropertyRandom(string property, string value, ref ArrayList al)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";
                if (propertiesRandom.Contains(property))
                {
                    propertiesRandom[property] = value;
                }
                else
                {
                    propertiesRandom.Add(property, value);
                }

                if (value != null && value.Length > 0)
                {
                    if (al != null)
                    {
                        if (al.Contains(value) == false)
                        {
                            try
                            {
                                al.Add(value);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("AddPropertyRandom: " + ex.ToString());
                            }
                            LoadImage(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddPropertyRandom: " + ex.ToString());
            }
        }

        private void SetProperty(string property, string value)
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
        private bool checkImageResolution(string filename, string type, string useAspectRatio)
        {
            try
            {
                if (File.Exists(filename) == false)
                {
                    dbm.DeleteFanart(filename, type);
                    return false;
                }

                Image checkImage = Image.FromFile(filename);
                double mWidth = Convert.ToInt32(minResolution.Substring(0, minResolution.IndexOf("x")));
                double mHeight = Convert.ToInt32(minResolution.Substring(minResolution.IndexOf("x") + 1));

                if (checkImage.Width >= mWidth && checkImage.Height >= mHeight)
                {
                    if (useAspectRatio.Equals("True"))
                    {
                        if (mHeight > 0 && ((mWidth / mHeight) >= 1.55))
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
                                dbm.loadMusicFanart(artist, dir, dir, type);
                            }
                            else
                            {
                                dbm.loadFanart(artist, dir, dir, type);                                
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
        /// Update the filenames keept by the plugin if files are added since start of MP
        /// </summary>
        public void UpdateDirectoryTimer()
        {
            try
            {
                //Add games images
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                int i = 0;
                if (useAnyGames)
                {
                    SetupFilenames(path, "*.jpg", ref i, "Game");
                }
                else
                {
                    dbm.DeleteAllFanart("Game");
                }
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                i = 0;
                if (useVideoFanart.Equals("True") || useAnyMovies)
                {
                    SetupFilenames(path, "*.jpg", ref i, "Movie");
                }
                else
                {
                    dbm.DeleteAllFanart("Movie");
                }
                //Add music images
                path = "";
                i = 0;
                if (useAlbum.Equals("True"))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                    SetupFilenames(path, "*L.jpg", ref i, "MusicAlbum");
                }
                else
                {
                    dbm.DeleteAllFanart("MusicAlbum");
                }
                if (useArtist.Equals("True"))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                    SetupFilenames(path, "*L.jpg", ref i, "MusicArtist");
                }
                else
                {
                    dbm.DeleteAllFanart("MusicArtist");
                }
                if (useFanart.Equals("True") || useAnyMusic)
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                    SetupFilenames(path, "*.jpg", ref i, "MusicFanart");
                }
                else
                {
                    dbm.DeleteAllFanart("MusicFanart");
                }                
                //Add pictures images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                i = 0;
                if (useAnyPictures)
                {
                    SetupFilenames(path, "*.jpg", ref i, "Picture");
                }
                else
                {
                    dbm.DeleteAllFanart("Picture");
                }
                //Add games images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                i = 0;
                if (useScoreCenterFanart.Equals("True") || useAnyScoreCenter)
                {
                    SetupFilenames(path, "*.jpg", ref i, "ScoreCenter");
                }
                else
                {
                    dbm.DeleteAllFanart("ScoreCenter");
                }
                //Add moving pictures images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\MovingPictures\Backdrops\FullSize";
                i = 0;
                if (useAnyMovingPictures)
                {
                    SetupFilenames(path, "*.jpg", ref i, "MovingPicture");
                }
                else
                {
                    dbm.DeleteAllFanart("MovingPicture");
                }
                //Add tvseries images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                i = 0;
                if (useAnyTVSeries)
                {
                    SetupFilenames(path, "*.jpg", ref i, "TVSeries");
                }
                else
                {
                    dbm.DeleteAllFanart("TVSeries");
                }
                //Add tv images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                i = 0;
                if (useAnyTV)
                {
                    SetupFilenames(path, "*.jpg", ref i, "TV");
                }
                else
                {
                    dbm.DeleteAllFanart("TV");
                }
                //Add plugins images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                i = 0;
                if (useAnyPlugins)
                {
                    SetupFilenames(path, "*.jpg", ref i, "Plugin");
                }
                else
                {
                    dbm.DeleteAllFanart("Plugin");
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDirectoryTimer: " + ex.ToString());
            }
        }

        /// <summary>
        /// Update the filenames keept by the plugin if files are added since start of MP
        /// </summary>
        public void UpdateDirectoryTimer(Object stateInfo)
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

        public void UpdateScraperTimer(Object stateInfo)
        {
            try
            {
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True") && dbm.GetIsScraping() == false)
                {
                    startScraper();
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateScraperTimer: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        public string GetFilename(string key, ref string currFile, ref int iFilePrev, string type)
        {
            string sout = currFile;
            try
            {
                if (isStopping == false)
                {
                    key = Utils.GetArtist(key, type);
                    Hashtable tmp = dbm.getFanart(key, type);
                    if (tmp != null && tmp.Count > 0)
                    {
                        ICollection valueColl = tmp.Values;
                        int iFile = 0;
                        int iStop = 0;
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
        public string GetRandomFilename(ref int randomCounter, ref string prevImage, ref Random randNum, string type)
        {
            string sout = "";
            try
            {
                if (isStopping == false)
                {
                    sout = prevImage;
                    string types = "";                    
                    if (type.Equals("MusicFanart"))
                    {
                        if (useAlbum.Equals("True") && useAlbumDisabled.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicAlbum'";
                            else
                                types = "'MusicAlbum'";
                        }
                        if (useArtist.Equals("True") && useArtistDisabled.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicArtist'";
                            else
                                types = "'MusicArtist'";
                        }
                        if (useFanart.Equals("True"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicFanart'";
                            else
                                types = "'MusicFanart'";
                        }
                    }
                    else
                    {
                        types = null;
                    }
                    Hashtable ht = dbm.getAnyFanart(type, types);
                    if (ht != null && ht.Count > 0)
                    {
                        bool doRun = true;
                        int attempts = 0;
                        while (doRun && attempts < (ht.Count*2))
                        {
                            int iHt = randNum.Next(0, ht.Count);
                            DatabaseManager.FanartImage imgFile = (DatabaseManager.FanartImage)ht[iHt];
                            sout = imgFile.disk_image;
//                            if (!(imgFile.type.Equals("MusicAlbum") && useAlbumDisabled.Equals("True")) && !(imgFile.type.Equals("MusicArtist") && useArtistDisabled.Equals("True")))
//                            {
                            if (checkImageResolution(sout, type, useAspectRatio))
                                {
                                    prevImage = sout;
                                    //ResetCurrCount(false);
                                    ResetCurrCountRandom();
                                    doRun = false;
                                }
                                attempts++;
//                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetRandomFilename: " + ex.ToString());
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

        /// <summary>
        /// Check if user are in myMusicGenre and track view (not artist or album)
        /// </summary>
        private Song IsSong(string track, string type, string property)
        {
            Song songOut = null;
            try
            {
                songInfo = new List<Song>();
                m_db.GetSongsByArtist(ParseArtist(m_SelectedItem), ref songInfo);
                if (songInfo != null && songInfo.Count > 0)
                {
                    for (int i = 0; i < songInfo.Count; i++)
                    {
                        Song s = (Song)songInfo[i];
                        if (s != null)
                        {
                            mi.Open(s.FileName);
                            if (mi != null)
                            {
                                string sAlbum = mi.Get(StreamKind.General, 0, "Track");
                                if (track != null && sAlbum != null && track.IndexOf(sAlbum) >= 0)
                                {
                                    songOut = s;
                                    i = 999999;
                                }
                                mi.Close();
                                s = null;
                            }
                        }
                    }
                }
                songInfo = null;
            }
            catch (Exception ex)
            {
                logger.Error("IsSong: " + ex.ToString());
            }
            return songOut;

        }

        /// <summary>
        /// Get MediaInfo for song
        /// </summary>
        private string GetMediaInfoSong(Song mySong, string type, string property)
        {
            string sout = "";
            try
            {                
                if (mySong != null)
                {
                    mi.Open(mySong.FileName);
                    if (type.Equals("General"))
                        sout = mi.Get(StreamKind.General, 0, property);
                    else
                        sout = mi.Get(StreamKind.Audio, 0, property);
                    mi.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetMediaInfoSong: " + ex.ToString());
            }
            return sout;
        }

        private void ResetCounters()
        {
            if (currCount > maxCountImage)
            {
                currCount = 0;
                hasUpdatedCurrCount = false;
            }            
            if (updateVisibilityCount > 5)
            {
                updateVisibilityCount = 1;
            }
            if (currCountPlay > maxCountImage)
            {
                currCountPlay = 0;
                hasUpdatedCurrCountPlay = false;
            }            
            if (updateVisibilityCountPlay > 5)
            {
                updateVisibilityCountPlay = 1;
            }
            if (currCountRandom > maxCountImage)
            {
                currCountRandom = 0;
            }            
            if (updateVisibilityCountRandom > 5)
            {
                updateVisibilityCountRandom = 1;
            }

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
                if (updateVisibilityCount == 2)  //after 2 sek
                {
                    UpdateProperties();
                    if (doShowImageOne)
                    {                        
                        ShowImageOne();
                        doShowImageOne = false;
                    }
                    else
                    {
                        ShowImageTwo();
                        doShowImageOne = true;
                    }
                    if (fanartAvailable)
                    {
                        FanartIsAvailable();
                    }
                    else
                    {
                        FanartIsNotAvailable();
                    }                    
                }
                else if (updateVisibilityCount == 5) //after 4 sek
                {
                    updateVisibilityCount = 0;
                    //release unused image resources
                    HandleOldImages(ref listSelectedMovies);
                    HandleOldImages(ref listSelectedMusic);
                    HandleOldImages(ref listSelectedScorecenter);
                }
                if (updateVisibilityCountPlay == 2)  //after 2 sek
                {
                    UpdatePropertiesPlay();
                    if (doShowImageOnePlay)
                    {
                        ShowImageOnePlay();
                        doShowImageOnePlay = false;
                    }
                    else
                    {
                        ShowImageTwoPlay();
                        doShowImageOnePlay = true;
                    }
                    if (fanartAvailablePlay)
                    {
                        FanartIsAvailablePlay();
                    }
                    else
                    {
                        FanartIsNotAvailablePlay();
                    }

                }
                else if (updateVisibilityCountPlay == 5) //after 4 sek
                {
                    updateVisibilityCountPlay = 0;
                    //release unused image resources
                    HandleOldImages(ref listPlayMusic);
                }
                if (updateVisibilityCountRandom == 2)  //after 2 sek
                {
                    UpdatePropertiesRandom();
                    if (doShowImageOneRandom)
                    {
                        ShowImageOneRandom();
                        doShowImageOneRandom = false;
                    }
                    else
                    {
                        ShowImageTwoRandom();
                        doShowImageOneRandom = true;
                    }                    
                }
                else if (updateVisibilityCountRandom == 5) //after 4 sek
                {
                    updateVisibilityCountRandom = 0;
                    //release unused image resources
                    HandleOldImages(ref listAnyGames);
                    HandleOldImages(ref listAnyMovies);
                    HandleOldImages(ref listAnyMovingPictures);
                    HandleOldImages(ref listAnyMusic);
                    HandleOldImages(ref listAnyPictures);
                    HandleOldImages(ref listAnyScorecenter);
                    HandleOldImages(ref listAnyPlugins);
                    HandleOldImages(ref listAnyTV);
                    HandleOldImages(ref listAnyTVSeries);
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateDummyControls: " + ex.ToString());
            }
        }

        private void IncreaseCurrCount()
        {            
            if (hasUpdatedCurrCount == false)
            {
                currCount = currCount + 1;
                hasUpdatedCurrCount = true;
            }
        }

        private void IncreaseCurrCountPlay()
        {
            if (hasUpdatedCurrCountPlay == false)
            {
                currCountPlay = currCountPlay + 1;
                hasUpdatedCurrCountPlay = true;
            }
        }

        private void IncreaseCurrCountRandom()
        {
            currCountRandom = currCountRandom + 1;
        }
        

        public void ResetCurrCount()
        {
            currCount = 0;
            updateVisibilityCount = 1;
            hasUpdatedCurrCount = true;
        }

        public void ResetCurrCountPlay()
        {
            currCountPlay = 0;
            updateVisibilityCountPlay = 1;
            hasUpdatedCurrCountPlay = true;
        }

        public void ResetCurrCountRandom()
        {
            currCountRandom = 0;
            updateVisibilityCountRandom = 1;
        }
  

        /// <summary>
        /// Get and set properties for selected artis, album or track in myMusicGenres window
        /// </summary>
        private void RefreshMusicSelectedProperties()
        {
            try
            {
                m_SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
                m_SelectedItem2 = GUIPropertyManager.GetProperty("#selecteditem2");
                mi = new MediaInfo();
                if (m_SelectedItem != null && m_SelectedItem.Equals("..") == false && m_SelectedItem.Length > 0)
                {
                    if (m_SelectedItem2 != null && m_SelectedItem2.Length > 1)
                    {
                        Song mySong = IsSong(m_SelectedItem, "General", "Track");
                        if (mySong != null)
                        {
                            m_SelectedItem2 = GetMediaInfoSong(mySong, "General", "Performer");
                        }
                        if (currSelectedMusicArtist.Equals(m_SelectedItem2) == false)
                        {
                            currSelectedMusic = "";
                            prevSelectedMusic = -1;
                            string sFilename = GetFilename(m_SelectedItem2, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart");
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic);
                            }
                            else
                            {
                                AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic);
                            }                            
                            currSelectedMusicArtist = m_SelectedItem2;
                            ResetCurrCount();
                        }
                        else if (currCount >= maxCountImage)
                        {
                            string sFilenamePrev = currSelectedMusic;
                            string sFilename = GetFilename(m_SelectedItem2, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart");
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic);
                            }
                            else
                            {
                                AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic);
                            }
                            currSelectedMusicArtist = m_SelectedItem2;
                            if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                            {
                                ResetCurrCount();
                            }
                        }
                        mySong = null;
                        IncreaseCurrCount();
                    }
                    else if (m_SelectedItem != null && m_SelectedItem.Length > 1)
                    {
                        if (currSelectedMusicArtist.Equals(m_SelectedItem) == false)
                        {
                            currSelectedMusic = "";
                            prevSelectedMusic = -1;
                            string sFilename = GetFilename(m_SelectedItem, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart");
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic);
                            }
                            else
                            {
                                AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic);
                            }                            
                            currSelectedMusicArtist = m_SelectedItem;
                            ResetCurrCount();
                        }
                        else if (currCount >= maxCountImage)
                        {
                            string sFilenamePrev = currSelectedMusic;
                            string sFilename = GetFilename(m_SelectedItem, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart");
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic);
                            }
                            else
                            {
                                AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic);
                            }
                            currSelectedMusicArtist = m_SelectedItem;
                            if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                            {
                                ResetCurrCount();
                            }
                        }
                        IncreaseCurrCount();
                    }
                    else
                    {
                        currSelectedMusic = "";
                        fanartAvailable = false;
                        if (doShowImageOne)
                            AddProperty("#fanarthandler.music.backdrop1.selected", "", ref listSelectedMusic);
                        else
                            AddProperty("#fanarthandler.music.backdrop2.selected", "", ref listSelectedMusic);
                        //ResetCurrCount(true);
                        ResetCurrCount();
                        currSelectedMusicArtist = "";
                    }
                }
                else
                {
                    currSelectedMusic = "";
                    currSelectedMusicArtist = "";
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshMusicSelectedProperties: " + ex.ToString());
            }
        }

 

        /// <summary>
        /// Get and set properties for selected video title
        /// </summary>
        private void RefreshGenericSelectedProperties(string property, ref ArrayList listSelectedGeneric, string type, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
        {
            try
            {
                if (GUIWindowManager.ActiveWindow == 6623)
                {
                    m_SelectedItem = GUIPropertyManager.GetProperty("#mvids.artist");
                }
                else if (GUIWindowManager.ActiveWindow == 880)
                {
                    m_SelectedItem = GUIPropertyManager.GetProperty("#MusicVids.ArtistName");
                }
                else if (GUIWindowManager.ActiveWindow == 510)
                {
                    m_SelectedItem = GUIPropertyManager.GetProperty("#artist");
                }
                else
                {
                    m_SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
                }                
                if (m_SelectedItem != null && m_SelectedItem.Equals("..") == false && m_SelectedItem.Length > 0)
                {
                    if (type.Equals("Global Search") || type.Equals("mVids") || type.Equals("Youtube.FM") || type.Equals("Music Playlist"))
                    {
                        m_SelectedItem = Utils.GetArtistLeftOfMinusSign(m_SelectedItem);
                    }
                    if (currSelectedGenericTitle.Equals(m_SelectedItem) == false)
                    {
                        currSelectedGeneric = "";
                        prevSelectedGeneric = -1;
                        string sFilename = GetFilename(m_SelectedItem, ref currSelectedGeneric, ref prevSelectedGeneric, type);
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric);
                        }
                        else
                        {
                            AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric);
                        }                        
                        currSelectedGenericTitle = m_SelectedItem;
                        ResetCurrCount();
                    }
                    else if (currCount >= maxCountImage)
                    {
                        string sFilenamePrev = currSelectedGeneric;
                        string sFilename = GetFilename(m_SelectedItem, ref currSelectedGeneric, ref prevSelectedGeneric, type);
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric);
                        }
                        else
                        {
                            AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric);
                        }
                        currSelectedGenericTitle = m_SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else
                {
                    currSelectedGeneric = "";                    
                    fanartAvailable = false;
                    if (doShowImageOne)
                        AddProperty("#fanarthandler." + property + ".backdrop1.selected", "", ref listSelectedMusic);
                    else
                        AddProperty("#fanarthandler." + property + ".backdrop2.selected", "", ref listSelectedMusic);
                    ResetCurrCount();
                    currSelectedGenericTitle = "";
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshGenericSelectedProperties: " + ex.ToString());
            }
        }


        /// <summary>
        /// Get and set properties for selected scorecenter title
        /// </summary>
        private void RefreshScorecenterSelectedProperties()
        {
            try
            {
                m_SelectedItem = GUIPropertyManager.GetProperty("#ScoreCenter.Category");
                if (m_SelectedItem != null && m_SelectedItem.Equals("..") == false && m_SelectedItem.Length > 0)
                {
                    if (currSelectedScorecenterGenre.Equals(m_SelectedItem) == false)
                    {
                        currSelectedScorecenter = "";
                        prevSelectedScorecenter = -1;
                        string sFilename = GetFilename(m_SelectedItem, ref currSelectedScorecenter, ref prevSelectedScorecenter, "ScoreCenter");
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref listSelectedScorecenter);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref listSelectedScorecenter);
                        }                        
                        currSelectedScorecenterGenre = m_SelectedItem;
                        ResetCurrCount();
                    }
                    else if (currCount >= maxCountImage)
                    {
                        string sFilenamePrev = currSelectedScorecenter;
                        string sFilename = GetFilename(m_SelectedItem, ref currSelectedScorecenter, ref prevSelectedScorecenter, "ScoreCenter");
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref listSelectedScorecenter);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref listSelectedScorecenter);
                        }
                        currSelectedScorecenterGenre = m_SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else
                {
                    currSelectedScorecenter = "";
                    currSelectedScorecenterGenre = "";
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshScorecenterSelectedProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get and set properties for now playing music
        /// </summary>
        private void RefreshMusicPlayingProperties()
        {
            try
            {
                if (currPlayMusicArtist.Equals(m_CurrentTrackTag) == false)
                {
                    currPlayMusic = "";
                    prevPlayMusic = -1;
                    string sFilename = GetFilename(m_CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart");
                    if (sFilename.Length == 0)
                    {
                        fanartAvailablePlay = false;
                    }
                    else
                    {
                        fanartAvailablePlay = true;
                    }
                    if (doShowImageOnePlay)
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic);
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic);
                    }
                    if (useOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic);
                    }
                    ResetCurrCountPlay();
                }
                else if (currCountPlay >= maxCountImage)
                {                    
                    string sFilenamePrev = currPlayMusic;
                    string sFilename = GetFilename(m_CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart");
                    if (sFilename.Length == 0)
                    {
                        fanartAvailablePlay = false;
                    }
                    else
                    {
                        fanartAvailablePlay = true;
                    }
                    if (doShowImageOnePlay)
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic);
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic);
                    }
                    if (useOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic);
                    }
                    if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                    {
                        ResetCurrCountPlay();
                    }                 
                }
                currPlayMusicArtist = m_CurrentTrackTag;
                IncreaseCurrCountPlay();
            }
            catch (Exception ex)
            {
                logger.Error("RefreshMusicPlayingProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get and set properties for random images
        /// </summary>
        private void RefreshRandomImageProperties()
        {
            try
            {
                if ((currCountRandom >= maxCountImage) || firstRandom || currCountRandom == 0)
                {
                    string sFilename = "";
                    if (supportsRandomImages("useRandomGamesFanart").Equals("True"))
                    {
                        if (dbm.htAnyGameFanart != null && dbm.htAnyGameFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.games.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyGameFanart[0]).disk_image, ref listAnyGames);
                            AddPropertyRandom("#fanarthandler.games.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyGameFanart[0]).disk_image, ref listAnyGames);
                        }
                        else if (dbm.htAnyGameFanart != null && dbm.htAnyGameFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.games.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyGameFanart[0]).disk_image, ref listAnyGames);
                            AddPropertyRandom("#fanarthandler.games.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyGameFanart[1]).disk_image, ref listAnyGames);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyGames, ref randAnyGames, "Game");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop1.any", sFilename, ref listAnyGames);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop2.any", sFilename, ref listAnyGames);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyGames);
                    }
                    if (supportsRandomImages("useRandomMoviesFanart").Equals("True"))
                    {
                        if (dbm.htAnyMovieFanart != null && dbm.htAnyMovieFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.movie.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMovieFanart[0]).disk_image, ref listAnyMovies);
                            AddPropertyRandom("#fanarthandler.movie.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMovieFanart[0]).disk_image, ref listAnyMovies);
                        }
                        else if (dbm.htAnyMovieFanart != null && dbm.htAnyMovieFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.movie.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMovieFanart[0]).disk_image, ref listAnyMovies);
                            AddPropertyRandom("#fanarthandler.movie.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMovieFanart[1]).disk_image, ref listAnyMovies);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyMovies, ref randAnyMovies, "Movie");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.movie.backdrop1.any", sFilename, ref listAnyMovies);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movie.backdrop2.any", sFilename, ref listAnyMovies);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyMovies);
                    }
                    if (supportsRandomImages("useRandomMovingPicturesFanart").Equals("True"))
                    {
                        if (dbm.htAnyMovingPicturesFanart != null && dbm.htAnyMovingPicturesFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMovingPicturesFanart[0]).disk_image, ref listAnyMovingPictures);
                            AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMovingPicturesFanart[0]).disk_image, ref listAnyMovingPictures);
                        }
                        else if (dbm.htAnyMovingPicturesFanart != null && dbm.htAnyMovingPicturesFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMovingPicturesFanart[0]).disk_image, ref listAnyMovingPictures);
                            AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMovingPicturesFanart[1]).disk_image, ref listAnyMovingPictures);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyMovingPictures, ref randAnyMovingPictures, "MovingPicture");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref listAnyMovingPictures);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref listAnyMovingPictures);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyMovingPictures);
                    }
                    if (supportsRandomImages("useRandomMusicFanart").Equals("True"))
                    {
                        if (dbm.htAnyMusicFanart != null && dbm.htAnyMusicFanart.Count == 1)
                        {

                            AddPropertyRandom("#fanarthandler.music.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMusicFanart[0]).disk_image, ref listAnyMusic);
                            AddPropertyRandom("#fanarthandler.music.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMusicFanart[0]).disk_image, ref listAnyMusic);
                        }
                        else if (dbm.htAnyMusicFanart != null && dbm.htAnyMusicFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.music.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyMusicFanart[0]).disk_image, ref listAnyMusic);
                            AddPropertyRandom("#fanarthandler.music.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyMusicFanart[1]).disk_image, ref listAnyMusic);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyMusic, ref randAnyMusic, "MusicFanart");                            
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop1.any", sFilename, ref listAnyMusic);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop2.any", sFilename, ref listAnyMusic);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyMusic);
                    }
                    if (supportsRandomImages("useRandomPicturesFanart").Equals("True"))
                    {
                        if (dbm.htAnyPictureFanart != null && dbm.htAnyPictureFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.picture.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyPictureFanart[0]).disk_image, ref listAnyPictures);
                            AddPropertyRandom("#fanarthandler.picture.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyPictureFanart[0]).disk_image, ref listAnyPictures);
                        }
                        else if (dbm.htAnyPictureFanart != null && dbm.htAnyPictureFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.picture.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyPictureFanart[0]).disk_image, ref listAnyPictures);
                            AddPropertyRandom("#fanarthandler.picture.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyPictureFanart[1]).disk_image, ref listAnyPictures);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyPictures, ref randAnyPictures, "Picture");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop1.any", sFilename, ref listAnyPictures);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop2.any", sFilename, ref listAnyPictures);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyPictures);
                    }                    
                    if (supportsRandomImages("useRandomScoreCenterFanart").Equals("True"))
                    {
                        if (dbm.htAnyScorecenter != null && dbm.htAnyScorecenter.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyScorecenter[0]).disk_image, ref listAnyScorecenter);
                            AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyScorecenter[0]).disk_image, ref listAnyScorecenter);
                        }
                        else if (dbm.htAnyScorecenter != null && dbm.htAnyScorecenter.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyScorecenter[0]).disk_image, ref listAnyScorecenter);
                            AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyScorecenter[1]).disk_image, ref listAnyScorecenter);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyScorecenter, ref randAnyScorecenter, "ScoreCenter");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref listAnyScorecenter);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref listAnyScorecenter);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyScorecenter);
                    }
                    if (supportsRandomImages("useRandomTVSeriesFanart").Equals("True"))
                    {
                        if (dbm.htAnyTVSeries != null && dbm.htAnyTVSeries.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyTVSeries[0]).disk_image, ref listAnyTVSeries);
                            AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyTVSeries[0]).disk_image, ref listAnyTVSeries);
                        }
                        else if (dbm.htAnyTVSeries != null && dbm.htAnyTVSeries.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyTVSeries[0]).disk_image, ref listAnyTVSeries);
                            AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyTVSeries[1]).disk_image, ref listAnyTVSeries);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyTVSeries, ref randAnyTVSeries, "TVSeries");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref listAnyTVSeries);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref listAnyTVSeries);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyTVSeries);
                    }
                    if (supportsRandomImages("useRandomTVFanart").Equals("True"))
                    {
                        if (dbm.htAnyTVFanart != null && dbm.htAnyTVFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.tv.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyTVFanart[0]).disk_image, ref listAnyTV);
                            AddPropertyRandom("#fanarthandler.tv.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyTVFanart[0]).disk_image, ref listAnyTV);
                        }
                        else if (dbm.htAnyTVFanart != null && dbm.htAnyTVFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.tv.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyTVFanart[0]).disk_image, ref listAnyTV);
                            AddPropertyRandom("#fanarthandler.tv.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyTVFanart[1]).disk_image, ref listAnyTV);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyTV, ref randAnyTV, "TV");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop1.any", sFilename, ref listAnyTV);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop2.any", sFilename, ref listAnyTV);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyTV);
                    }
                    if (supportsRandomImages("useRandomPluginsFanart").Equals("True"))
                    {
                        if (dbm.htAnyPluginFanart != null && dbm.htAnyPluginFanart.Count == 1)
                        {
                            AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyPluginFanart[0]).disk_image, ref listAnyPlugins);
                            AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyPluginFanart[0]).disk_image, ref listAnyPlugins);
                        }
                        else if (dbm.htAnyPluginFanart != null && dbm.htAnyPluginFanart.Count == 2)
                        {
                            AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", ((DatabaseManager.FanartImage)dbm.htAnyPluginFanart[0]).disk_image, ref listAnyPlugins);
                            AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", ((DatabaseManager.FanartImage)dbm.htAnyPluginFanart[1]).disk_image, ref listAnyPlugins);
                        }
                        else
                        {
                            sFilename = GetRandomFilename(ref currCount, ref currAnyPlugins, ref randAnyPlugins, "Plugin");
                            if (doShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", sFilename, ref listAnyPlugins);
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", sFilename, ref listAnyPlugins);
                            }
                        }
                    }
                    else
                    {
                        EmptyAllImages(ref listAnyPlugins);
                    }
                    //ResetCurrCount(false);
                    ResetCurrCountRandom();
                    firstRandom = false;
                }
                IncreaseCurrCountRandom();
            }
            catch (Exception ex)
            {
                logger.Error("RefreshRandomImageProperties: " + ex.ToString());
            }
        }
        

        /// <summary>
        /// checks all xml files in the current skin directory to see if it uses random property
        /// </summary>
        private void setupWindowsUsingRandomImages()
        {
            XPathDocument myXPathDocument;
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
                    SkinFile sf = new SkinFile();
                    sf.id = windowId;
                    myXPathNodeIterator = myXPathNavigator.Select("/window/define");
                    if (myXPathNodeIterator.Count > 0)
                    {
                        while (myXPathNodeIterator.MoveNext())
                        {
                            sNodeValue = myXPathNodeIterator.Current.Value;
                            if (sNodeValue.StartsWith("#useRandomGamesFanart"))
                                sf.useRandomGamesFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomMoviesFanart"))
                                sf.useRandomMoviesFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomMovingPicturesFanart"))
                                sf.useRandomMovingPicturesFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomMusicFanart"))
                                sf.useRandomMusicFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomPicturesFanart"))
                                sf.useRandomPicturesFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomScoreCenterFanart"))
                                sf.useRandomScoreCenterFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomTVSeriesFanart"))
                                sf.useRandomTVSeriesFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomTVFanart"))
                                sf.useRandomTVFanart = parseNodeValue(sNodeValue);
                            if (sNodeValue.StartsWith("#useRandomPluginsFanart"))
                                sf.useRandomPluginsFanart = parseNodeValue(sNodeValue);
                        }
                        if (sf.useRandomGamesFanart != null && sf.useRandomGamesFanart.Length > 0)
                        {
                            if (sf.useRandomGamesFanart.Equals("True"))
                            {
                                useAnyGames = true;
                            }                    
                        }
                        else 
                        {
                            sf.useRandomGamesFanart = "False";                            
                        }
                        if (sf.useRandomMoviesFanart != null && sf.useRandomMoviesFanart.Length > 0)
                        {
                            if (sf.useRandomMoviesFanart.Equals("True"))
                            {
                                useAnyMovies = true;
                            }                        
                        }
                        else
                        {
                            sf.useRandomMoviesFanart = "False";
                        }
                        if (sf.useRandomMovingPicturesFanart != null && sf.useRandomMovingPicturesFanart.Length > 0)
                        {
                            if (sf.useRandomMovingPicturesFanart.Equals("True"))
                            {
                                useAnyMovingPictures = true;
                            }
                        }
                        else
                        {
                            sf.useRandomMovingPicturesFanart = "False";
                        }
                        if (sf.useRandomMusicFanart != null && sf.useRandomMusicFanart.Length > 0)
                        {
                            if (sf.useRandomMusicFanart.Equals("True"))
                            {
                                useAnyMusic = true;
                            }
                        }
                        else
                        {
                            sf.useRandomMusicFanart = "False";
                        }
                        if (sf.useRandomPicturesFanart != null && sf.useRandomPicturesFanart.Length > 0)
                        {
                            if (sf.useRandomPicturesFanart.Equals("True"))
                            {
                                useAnyPictures = true;
                            }
                        }
                        else
                        {
                            sf.useRandomPicturesFanart = "False";
                        }
                        if (sf.useRandomScoreCenterFanart != null && sf.useRandomScoreCenterFanart.Length > 0)
                        {
                            if (sf.useRandomScoreCenterFanart.Equals("True"))
                            {
                                useAnyScoreCenter = true;
                            }
                        }
                        else
                        {
                            sf.useRandomScoreCenterFanart = "False";
                        }
                        if (sf.useRandomTVSeriesFanart != null && sf.useRandomTVSeriesFanart.Length > 0)
                        {
                            if (sf.useRandomTVSeriesFanart.Equals("True"))
                            {
                                useAnyTVSeries = true;
                            }
                        }
                        else
                        {
                            sf.useRandomTVSeriesFanart = "False";
                        }
                        if (sf.useRandomTVFanart != null && sf.useRandomTVFanart.Length > 0)
                        {
                            if (sf.useRandomTVFanart.Equals("True"))
                            {
                                useAnyTV = true;
                            }
                        }
                        else
                        {
                            sf.useRandomTVFanart = "False";
                        }
                        if (sf.useRandomPluginsFanart != null && sf.useRandomPluginsFanart.Length > 0)
                        {
                            if (sf.useRandomPluginsFanart.Equals("True"))
                            {
                                useAnyPlugins = true;
                            }
                        }
                        else
                        {
                            sf.useRandomPluginsFanart = "False";
                        }                       
                    }
                    try
                    {
                        if (sf.useRandomGamesFanart.Equals("False") && sf.useRandomMoviesFanart.Equals("False") && sf.useRandomMovingPicturesFanart.Equals("False")
                            && sf.useRandomMusicFanart.Equals("False") && sf.useRandomPicturesFanart.Equals("False") && sf.useRandomScoreCenterFanart.Equals("False") && sf.useRandomTVSeriesFanart.Equals("False")
                             && sf.useRandomTVFanart.Equals("False") && sf.useRandomPluginsFanart.Equals("False"))
                        {
                            //do nothing
                        }
                        else
                        {
                            randomWindows.Add(windowId, sf);
                        }
                    }
                    catch 
                    {
                        //do nothing
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("SetupWindowsUsingRandomImages, filename:" + s + "): " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Check if current skin file supports random images
        /// </summary>
        private String supportsRandomImages(string type)
        {
            SkinFile sf = (SkinFile)randomWindows[GUIWindowManager.ActiveWindow.ToString()];
            if (sf != null)
            {
                if (type.Equals("useRandomGamesFanart"))
                    return sf.useRandomGamesFanart;
                else if (type.Equals("useRandomMoviesFanart"))
                    return sf.useRandomMoviesFanart;
                else if (type.Equals("useRandomMovingPicturesFanart"))
                    return sf.useRandomMovingPicturesFanart;
                else if (type.Equals("useRandomMusicFanart"))
                    return sf.useRandomMusicFanart;
                else if (type.Equals("useRandomPicturesFanart"))
                    return sf.useRandomPicturesFanart;
                else if (type.Equals("useRandomScoreCenterFanart"))
                    return sf.useRandomScoreCenterFanart;
                else if (type.Equals("useRandomTVSeriesFanart"))
                    return sf.useRandomTVSeriesFanart;
                else if (type.Equals("useRandomTVFanart"))
                    return sf.useRandomTVFanart;
                else if (type.Equals("useRandomPluginsFanart"))
                    return sf.useRandomPluginsFanart;
            }
            return "False";
        }

        /// <summary>
        /// Parse node value
        /// </summary>
        private string parseNodeValue(string s)
        {
            if (s != null && s.Length > 0)
            {
                if (s.Substring(s.IndexOf(":") + 1).Equals("Yes"))
                    return "True";
                else
                    return "False";
            }
            return "False";
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

        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        public void UpdateImageTimer()
        {
            try
            {                
                bool resetFanartAvailableFlags = true;
                hasUpdatedCurrCount = false;
                hasUpdatedCurrCountPlay = false;            
                m_CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");                                
                if (scraperWorkerObject != null && scraperWorkerObject.getRefreshFlag())
                {
                    currCount = maxCountImage;
                    scraperWorkerObject.setRefreshFlag(false);
                }
                if (m_CurrentTrackTag != null && m_CurrentTrackTag.Length > 0)   // music is playing
                {
                    if (scraperWorkerObjectNowPlaying != null && scraperWorkerObjectNowPlaying.getRefreshFlag())
                    {
                        currCountPlay = maxCountImage;
                        scraperWorkerObjectNowPlaying.setRefreshFlag(false);
                    }                    
                    if (currPlayMusicArtist.Equals(m_CurrentTrackTag) == false)
                    {
                        if (m_CurrentTrackTag != null)
                        {
                            if (scraperMusicPlaying != null && scraperMusicPlaying.Equals("True") && dbm.GetIsScraping() == false && ((scrapeWorkerThreadNowPlaying != null && scrapeWorkerThreadNowPlaying.IsAlive == false) || scrapeWorkerThreadNowPlaying == null))
                            {
                                startScraperNowPlaying(m_CurrentTrackTag);
                            }
                        }
                    }
                    RefreshMusicPlayingProperties();
                }
                else
                {
                    stopScraperNowPlaying();
                    EmptyAllImages(ref listPlayMusic);
                    currPlayMusic = "";
                    currPlayMusicArtist = "";
                    fanartAvailablePlay = false;
                    SetProperty("#fanarthandler.music.overlay.play", "");
                    SetProperty("#fanarthandler.music.backdrop1.play", "");
                    SetProperty("#fanarthandler.music.backdrop2.play", "");                    
                    //ResetCurrCountPlay();
                }
                if (useMusicFanart.Equals("True"))
                {
                    if (GUIWindowManager.ActiveWindow == 504)
                    {                        
                        //User are in myMusicGenres window
                        resetFanartAvailableFlags = false;
                        RefreshMusicSelectedProperties();
                    }
                    else if (GUIWindowManager.ActiveWindow == 500)
                    {
                        //User are in music playlist
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("music", ref listSelectedMusic, "Music Playlist", ref currSelectedMusic, ref currSelectedMusicArtist);                        
                    }
                    else if (GUIWindowManager.ActiveWindow == 29050 || GUIWindowManager.ActiveWindow == 29051 || GUIWindowManager.ActiveWindow == 29052)
                    {
                        //User are in youtubefm search window
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("music", ref listSelectedMusic, "Youtube.FM", ref currSelectedMusic, ref currSelectedMusicArtist);
                    }
                    else if (GUIWindowManager.ActiveWindow == 880)
                    {
                        //User are in music videos window
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("music", ref listSelectedMusic, "Music Videos", ref currSelectedMusic, ref currSelectedMusicArtist);
                    }
                    else if (GUIWindowManager.ActiveWindow == 6623)
                    {
                        //User are in mvids window
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("music", ref listSelectedMusic, "mVids", ref currSelectedMusic, ref currSelectedMusicArtist);
                    }
                    else if (GUIWindowManager.ActiveWindow == 30885)
                    {
                        //User are in global search window
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("music", ref listSelectedMusic, "Global Search", ref currSelectedMusic, ref currSelectedMusicArtist);
                    }
                    else
                    {
                        EmptyAllImages(ref listSelectedMusic);
                        currSelectedMusic = "";
                        currSelectedMusicArtist = "";
                        SetProperty("#fanarthandler.music.backdrop1.selected", "");
                        SetProperty("#fanarthandler.music.backdrop2.selected", "");                                                
                    }
                }
                if (useVideoFanart.Equals("True"))
                {                    
                    if (GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25)
                    {
                        //User are in myVideo, myVideoTitle window
                        resetFanartAvailableFlags = false;
                        RefreshGenericSelectedProperties("movie", ref listSelectedMovies, "myVideos", ref currSelectedMovie, ref currSelectedMovieTitle);
                    }                    
                    else
                    {
                        EmptyAllImages(ref listSelectedMovies);
                        currSelectedMovie = "";
                        currSelectedMovieTitle = "";
                        SetProperty("#fanarthandler.movie.backdrop1.selected", "");
                        SetProperty("#fanarthandler.movie.backdrop2.selected", "");                                                
                    }
                }
                if (useScoreCenterFanart.Equals("True"))
                {
                    if (GUIWindowManager.ActiveWindow == 42000)  //User are in myScorecenter window
                    {
                        resetFanartAvailableFlags = false;
                        RefreshScorecenterSelectedProperties();
                    }
                    else
                    {
                        EmptyAllImages(ref listSelectedScorecenter);
                        currSelectedScorecenter = "";
                        currSelectedScorecenterGenre = "";
                        SetProperty("#fanarthandler.scorecenter.backdrop1.selected", "");
                        SetProperty("#fanarthandler.scorecenter.backdrop2.selected", "");
                    }
                }
                if (resetFanartAvailableFlags)
                {
                    fanartAvailable = false;
                }
                RefreshRandomImageProperties();
                if (updateVisibilityCount > 0)
                {
                    updateVisibilityCount = updateVisibilityCount + 1;
                }
                if (updateVisibilityCountPlay > 0)
                {
                    updateVisibilityCountPlay = updateVisibilityCountPlay + 1;
                }
                if (updateVisibilityCountRandom > 0)
                {
                    updateVisibilityCountRandom = updateVisibilityCountRandom + 1;
                }
                UpdateDummyControls();
            }
            catch (Exception ex)
            {
                logger.Error("UpdateImageTimer: " + ex.ToString());
            }
        }

        /// <summary>
        /// Run new check and return updated images to user
        /// </summary>
        public void UpdateImageTimer(Object stateInfo, ElapsedEventArgs e)
        {
            try
            {
                refreshTimer.Stop(); 
                UpdateImageTimer();         
                refreshTimer.Start(); 
            }
            catch (Exception ex)
            {
                logger.Error("UpdateImageTimer: " + ex.ToString());                 
            }
        }

        /// <summary>
        /// Set start values on variables
        /// </summary>
        private void SetupVariables()
        {
            isStopping = false;
            doShowImageOne = true;
            doShowImageOnePlay = true;
            doShowImageOneRandom = true;
            firstRandom = true;
            fanartAvailable = false;
            fanartAvailablePlay = false;
            updateVisibilityCount = 0;
            updateVisibilityCountPlay = 0;
            updateVisibilityCountRandom = 0;
            currCount = 0;
            currCountPlay = 0;
            currCountRandom = 0;
            ShowImageOne();
            FanartIsNotAvailable();
            ManageScraperProperties();
            maxCountImage = Convert.ToInt32(imageInterval);
            hasUpdatedCurrCount = false;
            hasUpdatedCurrCountPlay = false;
            prevSelectedGeneric = -1;
            prevPlayMusic = -1;
            prevSelectedMusic = -1;
            prevSelectedScorecenter = -1;
            currSelectedMovieTitle = "";
            currPlayMusicArtist = "";
            currSelectedMusicArtist = "";
            currSelectedScorecenterGenre = "";
            currSelectedMovie = "";
            currPlayMusic = "";
            currSelectedMusic = "";
            currSelectedScorecenter = "";
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
            randomWindows = new Hashtable();
            properties = new Hashtable();
            propertiesPlay = new Hashtable();
            propertiesRandom = new Hashtable();
            listAnyGames = new ArrayList();
            listAnyMovies = new ArrayList();
            listSelectedMovies = new ArrayList();
            listAnyMovingPictures = new ArrayList();
            listAnyMusic = new ArrayList();
            listPlayMusic = new ArrayList();
            listAnyPictures = new ArrayList();
            listAnyScorecenter = new ArrayList();            
            listSelectedMusic = new ArrayList();
            listSelectedScorecenter = new ArrayList();
            listAnyTVSeries = new ArrayList();
            listAnyTV = new ArrayList();
            listAnyPlugins = new ArrayList();
            randAnyGames = new Random();
            randAnyMovies = new Random();
            randAnyMovingPictures = new Random();
            randAnyMusic = new Random();
            randAnyPictures = new Random();
            randAnyScorecenter = new Random();
            randAnyTVSeries = new Random();
            randAnyTV = new Random();
            randAnyPlugins = new Random();
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
                initLogger();             
                logger.Info("Fanart Handler starting.");
                using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", "");
                    useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", "");
                    useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", "");
                    useAlbumDisabled = xmlreader.GetValueAsString("FanartHandler", "useAlbumDisabled", "");
                    useArtistDisabled = xmlreader.GetValueAsString("FanartHandler", "useArtistDisabled", "");
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
                }
                if (useFanart != null && useFanart.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useFanart = "True";
                }
                if (useAlbum != null && useAlbum.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useAlbum = "True";
                }
                if (useArtist != null && useArtist.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useArtist = "True";
                }
                if (useAlbumDisabled != null && useAlbumDisabled.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useAlbumDisabled = "True";
                }
                if (useArtistDisabled != null && useArtistDisabled.Length > 0)
                {
                    //donothing
                }
                else
                {
                    useArtistDisabled = "True";
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
                    defaultBackdrop = "background.png";
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
                    useAspectRatio = "True";
                }                
                setupDirectories();
                logger.Debug("Fanart Handler is using Fanart: " + useFanart + ", Album Thumbs: " + useAlbum + ", Artist Thumbs: " + useArtist + ".");
                dbm = new DatabaseManager();
                dbm.initDB(Convert.ToInt32(scraperMaxImages), scraperMPDatabase, scraperMusicPlaying);                
                m_db = MusicDatabase.Instance;
                SetupVariables();
                setupWindowsUsingRandomImages();
                UpdateDirectoryTimer();
                UpdateImageTimer();
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True"))
                {
                    //startScraper();
                    myScraperTimer = new TimerCallback(UpdateScraperTimer);
                    int iScraperInterval = Convert.ToInt32(scraperInterval);
                    iScraperInterval = iScraperInterval * 3600000;
                    scraperTimer = new System.Threading.Timer(myScraperTimer, null, 1000, iScraperInterval);
                    myProgressTimer = new TimerCallback(ManageScraperProperties);
                    progressTimer = new System.Threading.Timer(myProgressTimer, null, 5000, 10000);                        
                }
                myDirectoryTimer = new TimerCallback(UpdateDirectoryTimer);
                directoryTimer = new System.Threading.Timer(myDirectoryTimer, null, 3600000, 3600000);
                refreshTimer = new System.Timers.Timer(1000);
                refreshTimer.Elapsed += new ElapsedEventHandler(UpdateImageTimer);
                refreshTimer.Interval = 1000;
                refreshTimer.Enabled = true;
                logger.Info("Fanart Handler started.");
            }
            catch (Exception ex)
            {
                logger.Error("Start: " + ex.ToString());                
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
                scraperWorkerObject = new ScraperWorker(dbm);
                scrapeWorkerThread = new Thread(scraperWorkerObject.DoWork);

                // Start the worker thread.
                scrapeWorkerThread.Start();

                // Loop until worker thread activates.
                while (!scrapeWorkerThread.IsAlive) ;
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
                if (isStopping == false)
                {
                    scraperWorkerObjectNowPlaying = new ScraperWorkerNowPlaying(dbm, artist);
                    scrapeWorkerThreadNowPlaying = new Thread(scraperWorkerObjectNowPlaying.DoWork);

                    // Start the worker thread.
                    scrapeWorkerThreadNowPlaying.Start();

                    // Loop until worker thread activates.
                    while (!scrapeWorkerThreadNowPlaying.IsAlive) ;
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
        /// Load image
        /// </summary>
        private void LoadImage(string filename)
        {
            try
            {
                if (filename != null && filename.Length > 0)
                {
                    GUITextureManager.Load(filename, 0, 0, 0, false);
                }
            }
            catch (NullReferenceException nre)
            {
                logger.Error("LoadImage: " + nre.ToString());
            }
            catch (Exception ex)
            {
                logger.Error("LoadImage: " + ex.ToString());
            }
        }

        /// <summary>
        /// UnLoad image (free memory)
        /// </summary>
        private void UnLoadImage(string filename)
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

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        private void FanartIsAvailable()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919293);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        private void FanartIsNotAvailable()
        {
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919293);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        private void FanartIsAvailablePlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919294);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        private void FanartIsNotAvailablePlay()
        {
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919294);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageOne()
        {            
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919291);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919292);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageTwo()
        {            
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919292);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919291);   
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageOnePlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919295);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919296);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageTwoPlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919296);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919295);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageOneRandom()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919297);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919298);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        private void ShowImageTwoRandom()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919298);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919297);
        }

        private void ManageScraperProperties()
        {
            try
            {
                if (dbm.GetIsScraping())
                {
                    GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919280);                  
                    double iTot = dbm.totArtistsBeingScraped;
                    double iCurr = dbm.currArtistsBeingScraped;
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
            try
            {
                ManageScraperProperties();
            }
            catch (Exception ex)
            {
                logger.Error("ManageScraperProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// The Plugin is stopped
        /// </summary>
        public void Stop()
        {
            try
            {
                isStopping = true;
                stopScraper();
                stopScraperNowPlaying();
                progressTimer.Dispose();
                dbm.close();
                if (scraperTimer != null)
                {
                    scraperTimer.Dispose();
                }
                refreshTimer.Stop();
                directoryTimer.Dispose();                
                EmptyAllImages(ref listAnyGames);
                EmptyAllImages(ref listAnyMovies);
                EmptyAllImages(ref listSelectedMovies);
                EmptyAllImages(ref listAnyMovingPictures);
                EmptyAllImages(ref listAnyMusic);
                EmptyAllImages(ref listPlayMusic);
                EmptyAllImages(ref listAnyPictures);
                EmptyAllImages(ref listAnyScorecenter);
                EmptyAllImages(ref listSelectedMusic);
                EmptyAllImages(ref listSelectedScorecenter);
                EmptyAllImages(ref listAnyTVSeries);
                EmptyAllImages(ref listAnyTV);
                EmptyAllImages(ref listAnyPlugins);
            }
            catch (Exception ex)
            {
                logger.Error("Stop: " + ex.ToString());
            }
            logger.Debug("Fanart Handler stopped.");          
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

        public class SkinFile
        {
            public string id;
            public string useRandomGamesFanart;
            public string useRandomMoviesFanart;
            public string useRandomMovingPicturesFanart;
            public string useRandomMusicFanart;
            public string useRandomPicturesFanart;
            public string useRandomScoreCenterFanart;
            public string useRandomTVSeriesFanart;
            public string useRandomTVFanart;
            public string useRandomPluginsFanart;
        }

       
    
        public class ScraperWorker
        {
            DatabaseManager dbm;
            private bool triggerRefresh = false;

        
            public ScraperWorker(DatabaseManager dbm)
            {
                this.dbm = dbm;
                this.triggerRefresh = false;
            }
            
            // This method will be called when the thread is started.
            public void DoWork()
            {
                while (!_shouldStop)
                {
                    dbm.InitialScrape(this);
                    dbm.doNewScrape();                    
                    RequestStop();
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
                dbm.stopScraper = true;
                _shouldStop = true;
            }
            // Volatile is used as hint to the compiler that this data
            // member will be accessed by multiple threads.
            private volatile bool _shouldStop;
        }

        public class ScraperWorkerNowPlaying
        {
            private DatabaseManager dbm;
            private string artist;
            private bool triggerRefresh = false;

            public ScraperWorkerNowPlaying(DatabaseManager dbm, string artist)
            {
                this.dbm = dbm;
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
                    dbm.NowPlayingScrape(artist, this);
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
