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
            if (FanartHandlerSetup.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture))
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
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\games";
                    if (FanartHandlerSetup.FR.UseAnyGamesUser)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "Game User", 0);
                    }
                    if (FanartHandlerSetup.UseVideoFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.FR.UseAnyMoviesUser || FanartHandlerSetup.FR.UseAnyMoviesScraper)
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\movies";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "Movie User", 0);                    
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\movies";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "Movie Scraper", 0);
                    }

                    //Add music images
                    path = String.Empty;
                    if (FanartHandlerSetup.UseAlbum.Equals("True", StringComparison.CurrentCulture))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", "MusicAlbum", 0);
                    }
                    if (FanartHandlerSetup.UseArtist.Equals("True", StringComparison.CurrentCulture))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", "MusicArtist", 0);
                    }
                    if (FanartHandlerSetup.UseFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.FR.UseAnyMusicUser || FanartHandlerSetup.FR.UseAnyMusicScraper)
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\music";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "MusicFanart User", 0);                    
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\Scraper\music";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "MusicFanart Scraper", 0);
                    }

                    //Add pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\pictures";
                    if (FanartHandlerSetup.FR.UseAnyPicturesUser)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "Picture User", 0);
                    }

                    //Add games images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\scorecenter";
                    if (FanartHandlerSetup.UseScoreCenterFanart.Equals("True", StringComparison.CurrentCulture) || FanartHandlerSetup.FR.UseAnyScoreCenterUser)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "ScoreCenter User", 0);
                    }

                    //Add tvseries images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    if (FanartHandlerSetup.FR.UseAnyTVSeries)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "TVSeries", 0);
                    }

                    //Add tvseries images external
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    if (FanartHandlerSetup.FR.UseAnyTVSeries)
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
                            FanartHandlerSetup.SetupFilenamesExternal(path, "*.jpg", "TVSeries", 0, seriesHt);
                            seriesHt.Clear();
                            seriesHt = null;
                        }
                    }
                    
                    //Add tv images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\tv";
                    if (FanartHandlerSetup.FR.UseAnyTVUser)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "TV User", 0);
                    }

                    //Add plugins images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\UserDef\plugins";
                    if (FanartHandlerSetup.FR.UseAnyPluginsUser)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", "Plugin User", 0);
                    }

                    try
                    {
                        UtilsExternal.GetMovingPicturesBackdrops();
                    }
                    catch 
                    {
                    }
                    
                    Utils.GetDbm().HTAnyGameFanart = null; //20200429
                    Utils.GetDbm().HTAnyMovieFanart = null; //20200429
                    Utils.GetDbm().HTAnyMovingPicturesFanart = null; //20200429
                    Utils.GetDbm().HTAnyMusicFanart = null; //20200429
                    Utils.GetDbm().HTAnyPictureFanart = null; //20200429
                    Utils.GetDbm().HTAnyScorecenter = null; //20200429
                    Utils.GetDbm().HTAnyTVSeries = null; //20200429
                    Utils.GetDbm().HTAnyTVFanart = null; //20200429
                    Utils.GetDbm().HTAnyPluginFanart = null; //20200429
                    FanartHandlerSetup.SyncPointDirectory = 0;
                }
                catch (Exception ex)
                {
                    Utils.SetDelayStop(false);
                    FanartHandlerSetup.SyncPointDirectory = 0;
                    logger.Error("OnDoWork: " + ex.ToString());
                }                
            }
            Utils.SetDelayStop(false);
            logger.Info("Refreshing local fanart is done.");
        }

        

        private void GetLatestMediaInfo()
        {
            int z = 1;
            string windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture);           

            if (FanartHandlerSetup.LatestTVRecordings.Equals("True", StringComparison.CurrentCulture))
            {
                //TV Recordings
                FanartHandler.LatestsCollection latestTVRecordings = null;
                try
                {
                    MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));
                    string use4TR = xmlreader.GetValue("plugins", "For The Record TV");
                    if (use4TR != null && use4TR.Equals("yes", StringComparison.CurrentCulture))
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
            if (FanartHandlerSetup.LatestPictures.Equals("True", StringComparison.CurrentCulture) && !(windowId.Equals("2", StringComparison.CurrentCulture)))
            {
                //Pictures            
                edbm = new ExternalDatabaseManager();
                if (edbm.InitDB("PictureDatabase.db3"))
                {
                    FanartHandler.LatestsCollection ht = edbm.GetLatestPictures();
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
                    edbm.Close();
                }
                catch { }
                edbm = null;
                z = 1;
            }

            if (FanartHandlerSetup.LatestMusic.Equals("True", StringComparison.CurrentCulture) && !(windowId.Equals("987656", StringComparison.CurrentCulture) || windowId.Equals("504", StringComparison.CurrentCulture) || windowId.Equals("501", StringComparison.CurrentCulture) || windowId.Equals("500", StringComparison.CurrentCulture)))
            {
                //Music
                FanartHandler.LatestsCollection hTable = Utils.GetDbm().GetLatestMusic();
                if (hTable != null)
                {
                    //logger.Debug("Updating Latest Media Info: New music added");
                    for (int i = 0; i < hTable.Count; i++)
                    {
                        string thumb = string.Empty;
                        thumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicAlbum, hTable[i].Artist + "-" + hTable[i].Album);
                        if (thumb == null || thumb.Length < 1 || !File.Exists(thumb))
                        {
                            string sArtist = hTable[i].Artist;
                            if (sArtist != null && sArtist.Length > 0 && sArtist.IndexOf("|")>0)
                            {
                                sArtist = sArtist.Substring(0, sArtist.IndexOf("|")).Trim();
                            }
                            thumb = MediaPortal.Util.Utils.GetLargeCoverArtName(Thumbs.MusicArtists, sArtist);
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
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".fanart1", hTable[i].Fanart1);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.latest" + z + ".fanart2", hTable[i].Fanart2);
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
