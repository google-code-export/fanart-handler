namespace FanartHandler
{
    using MediaPortal.GUI.Library;
    using MediaPortal.Player;
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    public class RefreshWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static int syncPointProgressChange = 0;
        #endregion

        public RefreshWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        public void Report(DoWorkEventArgs e)
        {
            //ReportProgress(100, "Updated Properties");
            if (CancellationPending)
            {
                e.Cancel = true;
                return;
            }
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            Thread.CurrentThread.Priority = FanartHandlerSetup.ThreadPriority;
            bool isIdle = Utils.IsIdle();
            Utils.SetDelayStop(true);
            if (Utils.GetIsStopping() == false)
            {
                try
                {
                    bool resetFanartAvailableFlags = true;
                    FanartHandlerSetup.fs.HasUpdatedCurrCount = false;
                    FanartHandlerSetup.fp.HasUpdatedCurrCountPlay = false;
                    int windowId = GUIWindowManager.ActiveWindow;
                    FanartHandlerSetup.CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                    if (FanartHandlerSetup.ScraperMPDatabase != null && FanartHandlerSetup.ScraperMPDatabase.Equals("True") && FanartHandlerSetup.ScraperWorkerObject != null && FanartHandlerSetup.ScraperWorkerObject.GetRefreshFlag())
                    {
                        FanartHandlerSetup.fs.CurrCount = FanartHandlerSetup.MaxCountImage;
                        FanartHandlerSetup.fs.SetCurrentArtistsImageNames(null);
                        FanartHandlerSetup.ScraperWorkerObject.SetRefreshFlag(false);
                    }
                    if (FanartHandlerSetup.CurrentTrackTag != null && FanartHandlerSetup.CurrentTrackTag.Trim().Length > 0 && (g_Player.Playing || g_Player.Paused))   // music is playing
                    {
                        if (FanartHandlerSetup.ScraperMusicPlaying != null && FanartHandlerSetup.ScraperMusicPlaying.Equals("True") && FanartHandlerSetup.ScraperWorkerObjectNowPlaying != null && FanartHandlerSetup.ScraperWorkerObjectNowPlaying.GetRefreshFlag())
                        {
                            FanartHandlerSetup.fp.CurrCountPlay = FanartHandlerSetup.MaxCountImage;
                            FanartHandlerSetup.fp.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.ScraperWorkerObjectNowPlaying.SetRefreshFlag(false);
                        }
                        if (FanartHandlerSetup.fp.CurrPlayMusicArtist.Equals(FanartHandlerSetup.CurrentTrackTag) == false)
                        {
                            if (FanartHandlerSetup.ScraperMusicPlaying != null && FanartHandlerSetup.ScraperMusicPlaying.Equals("True") && Utils.GetDbm().GetIsScraping() == false && ((FanartHandlerSetup.ScrapeWorkerThreadNowPlaying != null && FanartHandlerSetup.ScrapeWorkerThreadNowPlaying.IsAlive == false) || FanartHandlerSetup.ScrapeWorkerThreadNowPlaying == null))
                            {
                                FanartHandlerSetup.StartScraperNowPlaying(FanartHandlerSetup.CurrentTrackTag);
                            }
                        }
                        FanartHandlerSetup.fp.RefreshMusicPlayingProperties();
                        Report(e);
                    }
                    else
                    {
                        if (FanartHandlerSetup.IsPlaying)
                        {
                            FanartHandlerSetup.StopScraperNowPlaying();
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fp.listPlayMusic);
                            FanartHandlerSetup.fp.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.fp.currPlayMusic = String.Empty;
                            FanartHandlerSetup.fp.CurrPlayMusicArtist = String.Empty;
                            FanartHandlerSetup.fp.FanartAvailablePlay = false;
                            FanartHandlerSetup.fp.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                            FanartHandlerSetup.fp.prevPlayMusic = -1;
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.overlay.play", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.play", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.play", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.fp.CurrCountPlay = 0;
                            FanartHandlerSetup.fp.UpdateVisibilityCountPlay = 0;
                            FanartHandlerSetup.IsPlaying = false;
                            Report(e);
                        }
                    }
                    if (GUIWindowManager.ActiveWindow == 35 && FanartHandlerSetup.UseBasichomeFade)
                    {
                        FanartHandlerSetup.CurrentTitleTag = GUIPropertyManager.GetProperty("#Play.Current.Title");
                        if (((FanartHandlerSetup.CurrentTrackTag != null && FanartHandlerSetup.CurrentTrackTag.Trim().Length > 0) || (FanartHandlerSetup.CurrentTitleTag != null && FanartHandlerSetup.CurrentTitleTag.Trim().Length > 0)) && Utils.IsIdle(FanartHandlerSetup.BasichomeFadeTime))
                        {
                            GUIButtonControl.ShowControl(GUIWindowManager.ActiveWindow, 98761234);
                        }
                        else
                        {
                            GUIButtonControl.HideControl(GUIWindowManager.ActiveWindow, 98761234);
                        }
                    }
                    if (FanartHandlerSetup.UseMusicFanart.Equals("True") && isIdle)
                    {
                        if (windowId == 504 || windowId == 501)
                        {
                            //User are in myMusicGenres window
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshMusicSelectedProperties();
                            Report(e);
                        }
                        else if (windowId == 500)
                        {
                            //User are in music playlist
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.fs.listSelectedMusic, "Music Playlist", ref FanartHandlerSetup.fs.currSelectedMusic, ref FanartHandlerSetup.fs.currSelectedMusicArtist);
                            Report(e);
                        }
                        else if (windowId == 29050 || windowId == 29051 || windowId == 29052)
                        {
                            //User are in youtubefm search window
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.fs.listSelectedMusic, "Youtube.FM", ref FanartHandlerSetup.fs.currSelectedMusic, ref FanartHandlerSetup.fs.currSelectedMusicArtist);
                            Report(e);
                        }
                        else if (windowId == 880)
                        {
                            //User are in music videos window
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.fs.listSelectedMusic, "Music Videos", ref FanartHandlerSetup.fs.currSelectedMusic, ref FanartHandlerSetup.fs.currSelectedMusicArtist);
                            Report(e);
                        }
                        else if (windowId == 6623)
                        {
                            //User are in mvids window
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.fs.listSelectedMusic, "mVids", ref FanartHandlerSetup.fs.currSelectedMusic, ref FanartHandlerSetup.fs.currSelectedMusicArtist);
                            Report(e);
                        }
                        else if (windowId == 30885)
                        {
                            //User are in global search window
                            FanartHandlerSetup.IsSelectedMusic = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.fs.listSelectedMusic, "Global Search", ref FanartHandlerSetup.fs.currSelectedMusic, ref FanartHandlerSetup.fs.currSelectedMusicArtist);
                            Report(e);
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedMusic)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fs.listSelectedMusic);
                                FanartHandlerSetup.fs.currSelectedMusic = String.Empty;
                                FanartHandlerSetup.fs.currSelectedMusicArtist = String.Empty;
                                FanartHandlerSetup.fs.prevSelectedMusic = -1;
                                FanartHandlerSetup.fs.prevSelectedGeneric = -1;
                                FanartHandlerSetup.fs.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.IsSelectedMusic = false;
                                Report(e);
                            }
                        }
                    }
                    if (FanartHandlerSetup.UseVideoFanart.Equals("True") && isIdle)
                    {
                        if (windowId == 6 || windowId == 25 || windowId == 28)
                        {
                            //User are in myVideo, myVideoTitle window or myvideoplaylist
                            FanartHandlerSetup.IsSelectedVideo = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.fs.listSelectedMovies, "myVideos", ref FanartHandlerSetup.fs.currSelectedMovie, ref FanartHandlerSetup.fs.currSelectedMovieTitle);
                            Report(e);
                        }
                        else if (windowId == 4755)
                        {
                            //User are in myonlinevideos
                            FanartHandlerSetup.IsSelectedVideo = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.fs.listSelectedMovies, "Online Videos", ref FanartHandlerSetup.fs.currSelectedMovie, ref FanartHandlerSetup.fs.currSelectedMovieTitle);
                            Report(e);
                        }
                        else if (windowId == 35)
                        {
                            //User are in basichome
                            FanartHandlerSetup.IsSelectedVideo = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.fs.listSelectedMovies, "myVideos", ref FanartHandlerSetup.fs.currSelectedMovie, ref FanartHandlerSetup.fs.currSelectedMovieTitle);
                            Report(e);
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedVideo)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fs.listSelectedMovies);
                                FanartHandlerSetup.fs.currSelectedMovie = String.Empty;
                                FanartHandlerSetup.fs.currSelectedMovieTitle = String.Empty;
                                FanartHandlerSetup.fs.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.fs.prevSelectedGeneric = -1;
                                FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.IsSelectedVideo = false;
                                Report(e);
                            }
                        }
                    }
                    if (FanartHandlerSetup.UseScoreCenterFanart.Equals("True") && isIdle)
                    {
                        if (windowId == 42000)  //User are in myScorecenter window
                        {
                            FanartHandlerSetup.IsSelectedScoreCenter = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.fs.RefreshScorecenterSelectedProperties();
                            Report(e);
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedScoreCenter)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fs.listSelectedScorecenter);
                                FanartHandlerSetup.fs.currSelectedScorecenter = String.Empty;
                                FanartHandlerSetup.fs.CurrSelectedScorecenterGenre = String.Empty;
                                FanartHandlerSetup.fs.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.fs.prevSelectedScorecenter = -1;
                                FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.selected", FanartHandlerSetup.TmpImage);
                                FanartHandlerSetup.IsSelectedScoreCenter = false;
                                Report(e);
                            }
                        }
                    }
                    if (resetFanartAvailableFlags && isIdle)
                    {
                        FanartHandlerSetup.fs.FanartAvailable = false;
                        FanartHandlerSetup.fs.FanartIsNotAvailable(windowId);
                    }
                    if (FanartHandlerSetup.fr.WindowsUsingFanartRandom.ContainsKey(windowId.ToString()))
                    {
                        FanartHandlerSetup.IsRandom = true;
                        FanartHandlerSetup.fr.RefreshRandomImageProperties(this);
                    }
                    else
                    {
                        if (FanartHandlerSetup.IsRandom)
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", FanartHandlerSetup.TmpImage);
                            FanartHandlerSetup.fr.currCountRandom = 0;
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyGames);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyMovies);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyMovingPictures);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyMusic);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyPictures);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyScorecenter);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyTVSeries);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyTV);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.fr.listAnyPlugins);
                            FanartHandlerSetup.IsRandom = false;
                            Report(e);
                        }
                    }
                    if (FanartHandlerSetup.fs.UpdateVisibilityCount > 0)
                    {
                        FanartHandlerSetup.fs.UpdateVisibilityCount = FanartHandlerSetup.fs.UpdateVisibilityCount + 1;
                    }
                    if (FanartHandlerSetup.fp.UpdateVisibilityCountPlay > 0)
                    {
                        FanartHandlerSetup.fp.UpdateVisibilityCountPlay = FanartHandlerSetup.fp.UpdateVisibilityCountPlay + 1;
                    }
                    if (FanartHandlerSetup.fr.UpdateVisibilityCountRandom > 0)
                    {
                        FanartHandlerSetup.fr.UpdateVisibilityCountRandom = FanartHandlerSetup.fr.UpdateVisibilityCountRandom + 1;
                    }
                    FanartHandlerSetup.UpdateDummyControls();
                    Utils.SetDelayStop(false);
                    Report(e);
                    e.Result = 0;
                }
                catch (Exception ex)
                {
                    logger.Error("OnDoWork: " + ex.ToString());
                }
            }
        }



        public void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                int sync = Interlocked.CompareExchange(ref syncPointProgressChange, 1, 0);
                if (sync == 0)
                {
                    FanartHandlerSetup.PreventRefresh = true;
                    int windowId = GUIWindowManager.ActiveWindow;
                    if (FanartHandlerSetup.fr.CountSetVisibility == 1 && FanartHandlerSetup.fr.GetPropertiesRandom() > 0)  //after 2 sek
                    {
                        FanartHandlerSetup.fr.CountSetVisibility = 2;
                        FanartHandlerSetup.fr.UpdatePropertiesRandom();

                        if (FanartHandlerSetup.fr.DoShowImageOneRandom)
                        {
                            FanartHandlerSetup.fr.ShowImageOneRandom(windowId);
                        }
                        else
                        {
                            FanartHandlerSetup.fr.ShowImageTwoRandom(windowId);
                        }

                    }
                    else if (FanartHandlerSetup.fr.UpdateVisibilityCountRandom > 2 && FanartHandlerSetup.fr.GetPropertiesRandom() > 0) //after 2 sek
                    {
                        FanartHandlerSetup.fr.UpdatePropertiesRandom();
                    }
                    else if (FanartHandlerSetup.fr.UpdateVisibilityCountRandom >= 5 && FanartHandlerSetup.fr.GetPropertiesRandom() == 0) //after 4 sek
                    {
                        if (FanartHandlerSetup.fr.DoShowImageOneRandom)
                        {
                            FanartHandlerSetup.fr.DoShowImageOneRandom = false;
                        }
                        else
                        {
                            FanartHandlerSetup.fr.DoShowImageOneRandom = true;
                        }
                        FanartHandlerSetup.fr.CountSetVisibility = 0;
                        FanartHandlerSetup.fr.UpdateVisibilityCountRandom = 0;
                        //release unused image resources
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyGames);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyMovies);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyMovingPictures);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyMusic);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyPictures);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyScorecenter);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyPlugins);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyTV);
                        FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.fr.listAnyTVSeries);
                        FanartHandlerSetup.fr.WindowOpen = false;
                        /*                    logger.Debug("listAnyGames: " + FanartHandlerSetup.fr.listAnyGames.Count);
                                            logger.Debug("listAnyMovies: " + FanartHandlerSetup.fr.listAnyMovies.Count);
                                            logger.Debug("listAnyMovingPictures: " + FanartHandlerSetup.fr.listAnyMovingPictures.Count);
                                            logger.Debug("listAnyMusic: " + FanartHandlerSetup.fr.listAnyMusic.Count);
                                            logger.Debug("listAnyPictures: " + FanartHandlerSetup.fr.listAnyPictures.Count);
                                            logger.Debug("listAnyScorecenter: " + FanartHandlerSetup.fr.listAnyScorecenter.Count);
                                            logger.Debug("listAnyTVSeries: " + FanartHandlerSetup.fr.listAnyTVSeries.Count);
                                            logger.Debug("listAnyTV: " + FanartHandlerSetup.fr.listAnyTV.Count);
                                            logger.Debug("listAnyPlugins: " + FanartHandlerSetup.fr.listAnyPlugins.Count);
                          */
                    }
                    FanartHandlerSetup.PreventRefresh = false;
                    syncPointProgressChange = 0;
                }
            }
            catch (Exception ex)
            {
                syncPointProgressChange = 0;
                logger.Error("OnProgressChanged: " + ex.ToString());
            }
        }

    }

}
