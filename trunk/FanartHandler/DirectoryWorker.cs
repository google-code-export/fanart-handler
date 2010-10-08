namespace FanartHandler
{
    using MediaPortal.Configuration;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using MediaPortal.Util;
    using MediaPortal.GUI.Library;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.IO;
    using System.Threading;
    using System.Reflection;
    
    class DirectoryWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();        
        #endregion

        public DirectoryWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            if (FanartHandlerSetup.FhThreadPriority.Equals("Lowest"))
            {
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;
            }
            else
            {
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
            }
            Thread.CurrentThread.Name = "DirectoryWorker";
            Utils.SetDelayStop(true);
            logger.Info("Refreshing local fanart is starting.");
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    //Get latest media added to MP
                    GetLatestMediaInfo();

                    //Add games images
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                    int i = 0;
                    if (FanartHandlerSetup.fr.UseAnyGames)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Game", 0);
                    }
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                    i = 0;
                    if (FanartHandlerSetup.UseVideoFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyMovies)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Movie", 0);
                    }

                    //Add music images
                    path = String.Empty;
                    i = 0;
                    if (FanartHandlerSetup.UseAlbum.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", ref i, "MusicAlbum", 0);
                    }
                    if (FanartHandlerSetup.UseArtist.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", ref i, "MusicArtist", 0);
                    }
                    if (FanartHandlerSetup.UseFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyMusic)
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "MusicFanart", 0);
                    }

                    //Add pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyPictures)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Picture", 0);
                    }

                    //Add games images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                    i = 0;
                    if (FanartHandlerSetup.UseScoreCenterFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyScoreCenter)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "ScoreCenter", 0);
                    }

                    //Add tvseries images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyTVSeries)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "TVSeries", 0);
                    }

                    //Add tvseries images external
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyTVSeries)
                    {
                        Hashtable seriesHt = null;
                        try
                        {
                            seriesHt = UtilsExternal.GetTVSeriesName("TVSeries");
                        }
                        catch
                        {
                        }
                        if (seriesHt != null)
                        {
                            FanartHandlerSetup.SetupFilenamesExternal(path, "*.jpg", ref i, "TVSeries", 0, seriesHt);
                            seriesHt.Clear();
                            seriesHt = null;
                        }
                    }
                    
                    //Add tv images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyTV)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "TV", 0);
                    }

                    //Add plugins images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyPlugins)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Plugin", 0);
                    }

                    try
                    {
                        UtilsExternal.GetMovingPicturesBackdrops();
                    }
                    catch 
                    {
                    }
                    
                    Utils.GetDbm().HtAnyGameFanart = null; //20200429
                    Utils.GetDbm().HtAnyMovieFanart = null; //20200429
                    Utils.GetDbm().HtAnyMovingPicturesFanart = null; //20200429
                    Utils.GetDbm().HtAnyMusicFanart = null; //20200429
                    Utils.GetDbm().HtAnyPictureFanart = null; //20200429
                    Utils.GetDbm().HtAnyScorecenter = null; //20200429
                    Utils.GetDbm().HtAnyTVSeries = null; //20200429
                    Utils.GetDbm().HtAnyTVFanart = null; //20200429
                    Utils.GetDbm().HtAnyPluginFanart = null; //20200429
                    FanartHandlerSetup.syncPointDirectory = 0;
                }
                catch (Exception ex)
                {
                    Utils.SetDelayStop(false);
                    FanartHandlerSetup.syncPointDirectory = 0;
                    logger.Error("OnDoWork: " + ex.ToString());
                }                
            }
            Utils.SetDelayStop(false);
            logger.Info("Refreshing local fanart is done.");
        }

        

        private void GetLatestMediaInfo()
        {
            int z = 1;
            string windowId = GUIWindowManager.ActiveWindow.ToString();           

            if (FanartHandlerSetup.LatestTVRecordings.Equals("True"))
            {
                //TV Recordings
                UtilsExternal.Latests latestTVRecordings = null;
                try
                {
                    MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
                    string use4TR = xmlreader.GetValue("plugins", "For The Record TV");
                    if (use4TR != null && use4TR.Equals("yes"))
                    {
                        ResolveEventHandler assemblyResolve = UtilsExternal.OnAssemblyResolve;
                        try
                        {
                            AppDomain currentDomain = AppDomain.CurrentDomain;
                            currentDomain.AssemblyResolve += new ResolveEventHandler(UtilsExternal.OnAssemblyResolve);
                            UtilsExternal.IsGetTypeRunningOnThisThread = true;
                            //Type.GetType("ForTheRecord.UI.Process");
                            latestTVRecordings = UtilsExternal.Get4TRRecordings();
                            AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolve;
                        }
                        catch
                        {                            
                            AppDomain.CurrentDomain.AssemblyResolve -= assemblyResolve;
                        }
                    }
                    else
                    {
                        latestTVRecordings = UtilsExternal.GetTVRecordings();                        
                    }
                }
                catch// (Exception ex)
                {
                    //logger.Error("GetLatestMediaInfo: "+ex.ToString());
                }
                if (latestTVRecordings != null)
                {
                    //logger.Debug("Updating Latest Media Info: New tv recordings added");
                    for (int i = 0; i < latestTVRecordings.Count; i++)
                    {
                        logger.Debug("Updating Latest Media Info: Latest tv recording " + z + ": " + latestTVRecordings[i].Title);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvrecordings.latest" + z + ".thumb", latestTVRecordings[i].Thumb);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvrecordings.latest" + z + ".title", latestTVRecordings[i].Title);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvrecordings.latest" + z + ".dateAdded", latestTVRecordings[i].DateAdded);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvrecordings.latest" + z + ".genre", latestTVRecordings[i].Genre);
                        z++;
                    }
                    latestTVRecordings.Clear();
                }                
                latestTVRecordings = null;
                z = 1;
            }                       

            ExternalDatabaseManager edbm;
            if (FanartHandlerSetup.LatestPictures.Equals("True") && !(windowId.Equals("2")))
            {
                //Pictures            
                edbm = new ExternalDatabaseManager();
                if (edbm.InitDB("PictureDatabase.db3"))
                {
                    UtilsExternal.Latests ht = edbm.GetLatestPictures();
                    if (ht != null)
                    {
                        //logger.Debug("Updating Latest Media Info: New pictures added");
                        for (int i = 0; i < ht.Count; i++)
                        {
                            logger.Debug("Updating Latest Media Info: Latest picture " + z + ": " + ht[i].Thumb);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.latest" + z + ".thumb", ht[i].Thumb);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.latest" + z + ".filename", ht[i].Thumb);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.latest" + z + ".dateAdded", ht[i].DateAdded);
                            z++;
                        }
                        ht.Clear();
                    }                    
                    ht = null;
                }
                try
                {
                    edbm.close();
                }
                catch { }
                edbm = null;
                z = 1;
            }

            if (FanartHandlerSetup.LatestMusic.Equals("True") && !(windowId.Equals("987656") || windowId.Equals("504") || windowId.Equals("501") || windowId.Equals("500")))
            {
                //Music
                UtilsExternal.Latests hTable = Utils.GetDbm().GetLatestMusic();
                if (hTable != null)
                {
                    //logger.Debug("Updating Latest Media Info: New music added");
                    for (int i = 0; i < hTable.Count; i++)
                    {
                        string thumb = string.Empty;
                        thumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicAlbum, hTable[i].Artist + "-" + hTable[i].Album);
                        if (thumb == null || thumb.Length < 1 || !File.Exists(thumb))
                        {
                            thumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicArtists, hTable[i].Artist);
                        }
                        if (thumb == null || thumb.Length < 1 || !File.Exists(thumb))
                        {
                            thumb = "";
                        }
                        logger.Debug("Updating Latest Media Info: Latest music album " + z + ": " + hTable[i].Artist + " - " + hTable[i].Album);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".thumb", thumb);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".artist", hTable[i].Artist);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".album", hTable[i].Album);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".dateAdded", hTable[i].DateAdded);
                        z++;
                    }
                    hTable.Clear();
                }                
                hTable = null;
                z = 1;
            }
        }
    }
}
