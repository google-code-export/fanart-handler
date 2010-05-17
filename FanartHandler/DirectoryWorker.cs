namespace FanartHandler
{
    using MediaPortal.Configuration;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;
    
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
                    //Add games images
                    string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                    int i = 0;
                    if (FanartHandlerSetup.fr.UseAnyGames)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Game");
                    }
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                    i = 0;
                    if (FanartHandlerSetup.UseVideoFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyMovies)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Movie");
                    }

                    //Add music images
                    path = String.Empty;
                    i = 0;
                    if (FanartHandlerSetup.UseAlbum.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", ref i, "MusicAlbum");
                    }
                    if (FanartHandlerSetup.UseArtist.Equals("True"))
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                        FanartHandlerSetup.SetupFilenames(path, "*L.jpg", ref i, "MusicArtist");
                    }
                    if (FanartHandlerSetup.UseFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyMusic)
                    {
                        path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "MusicFanart");
                    }

                    //Add pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyPictures)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Picture");
                    }

                    //Add games images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                    i = 0;
                    if (FanartHandlerSetup.UseScoreCenterFanart.Equals("True") || FanartHandlerSetup.fr.UseAnyScoreCenter)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "ScoreCenter");
                    }

                    //Add moving pictures images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\MovingPictures\Backdrops\FullSize";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyMovingPictures)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "MovingPicture");
                    }

                    //Add tvseries images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyTVSeries)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "TVSeries");
                    }

                    //Add tv images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyTV)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "TV");
                    }

                    //Add plugins images
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                    i = 0;
                    if (FanartHandlerSetup.fr.UseAnyPlugins)
                    {
                        FanartHandlerSetup.SetupFilenames(path, "*.jpg", ref i, "Plugin");
                    }

                    Utils.ImportExternalDbFanart("movingpictures.db3", "MovingPicture"); //20200429
                    Utils.ImportExternalDbFanart("TVSeriesDatabase4.db3", "TVSeries"); //20200429

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
                    FanartHandlerSetup.syncPointDirectory = 0;
                    logger.Error("OnDoWork: " + ex.ToString());
                }                
            }
            Utils.SetDelayStop(true);
            logger.Info("Refreshing local fanart is done.");
        }
    }
}
