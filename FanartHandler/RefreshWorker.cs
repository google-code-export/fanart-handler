﻿//***********************************************************************
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
        private static int syncPointProgressChange/* = 0*/;
        #endregion

        public RefreshWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        public void Report(DoWorkEventArgs e)
        {
            if (CancellationPending)
            {
                e.Cancel = true;
                return;
            }
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
            Thread.CurrentThread.Name = "RefreshWorker";
            Utils.AllocateDelayStop("RefreshWorker-OnDoWork");
            if (Utils.GetIsStopping() == false)
            {
                bool isIdle = Utils.IsIdle();
                try
                {
                    bool resetFanartAvailableFlags = true;
                    FanartHandlerSetup.FS.HasUpdatedCurrCount = false;
                    FanartHandlerSetup.FP.HasUpdatedCurrCountPlay = false;
                    int windowId = GUIWindowManager.ActiveWindow;
                    if (windowId == 730718)
                    {
                        FanartHandlerSetup.CurrentTrackTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.artist");
                        FanartHandlerSetup.CurrentAlbumTag = GUIPropertyManager.GetProperty("#mpgrooveshark.current.album");
                    }
                    else
                    {
                        FanartHandlerSetup.CurrentTrackTag = GUIPropertyManager.GetProperty("#Play.Current.Artist");
                        FanartHandlerSetup.CurrentAlbumTag = GUIPropertyManager.GetProperty("#Play.Current.Album");
                    }
                    if (FanartHandlerSetup.ScraperMPDatabase != null && FanartHandlerSetup.ScraperMPDatabase.Equals("True", StringComparison.CurrentCulture) && ((FanartHandlerSetup.MyScraperWorker != null && FanartHandlerSetup.MyScraperWorker.TriggerRefresh) || (FanartHandlerSetup.MyScraperNowWorker != null && FanartHandlerSetup.MyScraperNowWorker.TriggerRefresh)))
                    {
                        FanartHandlerSetup.FS.CurrCount = FanartHandlerSetup.MaxCountImage;
                        FanartHandlerSetup.FS.SetCurrentArtistsImageNames(null);
                        FanartHandlerSetup.MyScraperWorker.TriggerRefresh = false;
                    }
                    if (FanartHandlerSetup.CurrentTrackTag != null && FanartHandlerSetup.CurrentTrackTag.Trim().Length > 0 && (g_Player.Playing || g_Player.Paused))   // music is playing
                    {
                        if (FanartHandlerSetup.ScraperMusicPlaying != null && FanartHandlerSetup.ScraperMusicPlaying.Equals("True", StringComparison.CurrentCulture) && (FanartHandlerSetup.MyScraperNowWorker != null && FanartHandlerSetup.MyScraperNowWorker.TriggerRefresh))
                        {
                            FanartHandlerSetup.FP.CurrCountPlay = FanartHandlerSetup.MaxCountImage;
                            FanartHandlerSetup.FP.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.MyScraperNowWorker.TriggerRefresh = false;
                        }
                        if (FanartHandlerSetup.FP.CurrPlayMusicArtist.Equals(FanartHandlerSetup.CurrentTrackTag, StringComparison.CurrentCulture) == false)
                        {
                            if (FanartHandlerSetup.ScraperMusicPlaying != null && FanartHandlerSetup.ScraperMusicPlaying.Equals("True", StringComparison.CurrentCulture) && Utils.GetDbm().GetIsScraping() == false && ((FanartHandlerSetup.MyScraperNowWorker != null && FanartHandlerSetup.MyScraperNowWorker.IsBusy == false) || FanartHandlerSetup.MyScraperNowWorker == null))
                            {
                                FanartHandlerSetup.StartScraperNowPlaying(FanartHandlerSetup.CurrentTrackTag, FanartHandlerSetup.CurrentAlbumTag);
                            }
                        }
                        FanartHandlerSetup.FP.RefreshMusicPlayingProperties();
                        FanartHandlerSetup.IsPlaying = true;
                        Report(e);
                    }
                    else
                    {
                        if (FanartHandlerSetup.IsPlaying)
                        {
                            FanartHandlerSetup.StopScraperNowPlaying();
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FP.ListPlayMusic);
                            FanartHandlerSetup.FP.SetCurrentArtistsImageNames(null);
                            FanartHandlerSetup.FP.CurrPlayMusic = String.Empty;
                            FanartHandlerSetup.FP.CurrPlayMusicArtist = String.Empty;
                            FanartHandlerSetup.FP.FanartAvailablePlay = false;
                            FanartHandlerSetup.FP.FanartIsNotAvailablePlay(GUIWindowManager.ActiveWindow);
                            FanartHandlerSetup.FP.PrevPlayMusic = -1;
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.artisthumb.play", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.overlay.play", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.play", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.play", string.Empty);
                            FanartHandlerSetup.FP.CurrCountPlay = 0;
                            FanartHandlerSetup.FP.UpdateVisibilityCountPlay = 0;
                            FanartHandlerSetup.IsPlaying = false;
                            Report(e);
                        }
                    }
                    if (GUIWindowManager.ActiveWindow == 35 && FanartHandlerSetup.UseBasichomeFade)
                    {
                        try
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
                        catch
                        {
                        }
                    }
                    if (FanartHandlerSetup.UseMusicFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        if (FanartHandlerSetup.FS.WindowsUsingFanartSelected.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                        {
                            if (windowId == 504 || windowId == 501 || windowId == 500)
                            {
                                //User are in myMusicGenres window
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshMusicSelectedProperties();
                                Report(e);
                            }
                            else if (windowId == 6622)
                            {
                                //User are in music playlist
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.FS.ListSelectedMusic, "Music Trivia", ref FanartHandlerSetup.FS.CurrSelectedMusic, ref FanartHandlerSetup.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 29050 || windowId == 29051 || windowId == 29052)
                            {
                                //User are in youtubefm search window
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.FS.ListSelectedMusic, "Youtube.FM", ref FanartHandlerSetup.FS.CurrSelectedMusic, ref FanartHandlerSetup.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 880)
                            {
                                //User are in music videos window
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.FS.ListSelectedMusic, "Music Videos", ref FanartHandlerSetup.FS.CurrSelectedMusic, ref FanartHandlerSetup.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }
                            else if (windowId == 6623)
                            {
                                //User are in mvids window
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.FS.ListSelectedMusic, "mVids", ref FanartHandlerSetup.FS.CurrSelectedMusic, ref FanartHandlerSetup.FS.CurrSelectedMusicArtist);
                                Report(e);
                            }                            
                            else
                            {
                                //User are in global search window or UNKNOW/NOT SPECIFIED plugin that supports fanart handler
                                FanartHandlerSetup.IsSelectedMusic = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("music", ref FanartHandlerSetup.FS.ListSelectedMusic, "Global Search", ref FanartHandlerSetup.FS.CurrSelectedMusic, ref FanartHandlerSetup.FS.CurrSelectedMusicArtist);
                                Report(e);                                
                            }
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedMusic)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FS.ListSelectedMusic);
                                FanartHandlerSetup.FS.CurrSelectedMusic = String.Empty;
                                FanartHandlerSetup.FS.CurrSelectedMusicArtist = String.Empty;
                                FanartHandlerSetup.FS.PrevSelectedMusic = -1;
                                FanartHandlerSetup.FS.PrevSelectedGeneric = -1;
                                FanartHandlerSetup.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.IsSelectedMusic = false;
                                Report(e);
                            }
                        }
                    }

                    if (FanartHandlerSetup.UseVideoFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        if (FanartHandlerSetup.FS.WindowsUsingFanartSelected.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                        {
                            if (windowId == 6 || windowId == 25 || windowId == 28 || windowId == 2003 || windowId == 9813)
                            {
                                //User are in myVideo, myVideoTitle window or myvideoplaylist
                                FanartHandlerSetup.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.FS.ListSelectedMovies, "myVideos", ref FanartHandlerSetup.FS.CurrSelectedMovie, ref FanartHandlerSetup.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }                          
                            else if (windowId == 601 || windowId == 605 || windowId == 606 || windowId == 603 || windowId == 759 || windowId == 1 || windowId == 600 || windowId == 747 || windowId == 49849 || windowId == 49848 || windowId == 49850)
                            {
                                //tv section
                                FanartHandlerSetup.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.FS.ListSelectedMovies, "TV Section", ref FanartHandlerSetup.FS.CurrSelectedMovie, ref FanartHandlerSetup.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                            else if (windowId == 35)
                            {
                                //User are in basichome
                                FanartHandlerSetup.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.FS.ListSelectedMovies, "myVideos", ref FanartHandlerSetup.FS.CurrSelectedMovie, ref FanartHandlerSetup.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                            else if (!FanartHandlerSetup.FS.FoundItem)
                            {
                                //User are in myonlinevideos, mytrailers or UNKNOW/NOT SPECIFIED plugin that supports fanart handler
                                FanartHandlerSetup.IsSelectedVideo = true;
                                resetFanartAvailableFlags = false;
                                FanartHandlerSetup.FS.RefreshGenericSelectedProperties("movie", ref FanartHandlerSetup.FS.ListSelectedMovies, "Online Videos", ref FanartHandlerSetup.FS.CurrSelectedMovie, ref FanartHandlerSetup.FS.CurrSelectedMovieTitle);
                                Report(e);
                            }
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedVideo)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FS.ListSelectedMovies);
                                FanartHandlerSetup.FS.CurrSelectedMovie = String.Empty;
                                FanartHandlerSetup.FS.CurrSelectedMovieTitle = String.Empty;
                                FanartHandlerSetup.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.FS.PrevSelectedGeneric = -1;
                                FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.IsSelectedVideo = false;
                                Report(e);
                            }
                        }
                    }
                    if (FanartHandlerSetup.UseScoreCenterFanart.Equals("True", StringComparison.CurrentCulture) && isIdle)
                    {
                        if (windowId == 42000)  //User are in myScorecenter window
                        {
                            FanartHandlerSetup.IsSelectedScoreCenter = true;
                            resetFanartAvailableFlags = false;
                            FanartHandlerSetup.FS.RefreshScorecenterSelectedProperties();
                            Report(e);
                        }
                        else
                        {
                            if (FanartHandlerSetup.IsSelectedScoreCenter)
                            {
                                FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FS.ListSelectedScorecenter);
                                FanartHandlerSetup.FS.CurrSelectedScorecenter = String.Empty;
                                FanartHandlerSetup.FS.CurrSelectedScorecenterGenre = String.Empty;
                                FanartHandlerSetup.FS.SetCurrentArtistsImageNames(null);
                                FanartHandlerSetup.FS.PrevSelectedScorecenter = -1;
                                FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.selected", string.Empty);
                                FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.selected", string.Empty);
                                FanartHandlerSetup.IsSelectedScoreCenter = false;
                                Report(e);
                            }
                        }
                    }
                    if (resetFanartAvailableFlags && isIdle)
                    {
                        FanartHandlerSetup.FS.FanartAvailable = false;
                        FanartHandlerSetup.FS.FanartIsNotAvailable(windowId);
                    }
                    if (FanartHandlerSetup.FR.WindowsUsingFanartRandom.ContainsKey(windowId.ToString(CultureInfo.CurrentCulture)))
                    {
                        FanartHandlerSetup.IsRandom = true;
                        FanartHandlerSetup.FR.RefreshRandomImageProperties(this);
                    }
                    else
                    {
                        if (FanartHandlerSetup.IsRandom)
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.scraper.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.scraper.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.scraper.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.userdef.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.userdef.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", string.Empty);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", string.Empty);
                            FanartHandlerSetup.FR.CurrCountRandom = 0;
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyGamesUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyMoviesUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyMoviesScraper);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyMovingPictures);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyMusicUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyMusicScraper);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyPicturesUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyScorecenterUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyTVSeries);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyTVUser);
                            FanartHandlerSetup.EmptyAllImages(ref FanartHandlerSetup.FR.ListAnyPluginsUser);
                            FanartHandlerSetup.IsRandom = false;
                            Report(e);
                        }
                    }
                    if (FanartHandlerSetup.FS.UpdateVisibilityCount > 0)
                    {
                        FanartHandlerSetup.FS.UpdateVisibilityCount = FanartHandlerSetup.FS.UpdateVisibilityCount + 1;
                    }
                    if (FanartHandlerSetup.FP.UpdateVisibilityCountPlay > 0)
                    {
                        FanartHandlerSetup.FP.UpdateVisibilityCountPlay = FanartHandlerSetup.FP.UpdateVisibilityCountPlay + 1;
                    }
                    if (FanartHandlerSetup.FR.UpdateVisibilityCountRandom > 0)
                    {
                        FanartHandlerSetup.FR.UpdateVisibilityCountRandom = FanartHandlerSetup.FR.UpdateVisibilityCountRandom + 1;
                    }
                    FanartHandlerSetup.UpdateDummyControls();
                    Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
                    Report(e);
                    e.Result = 0;
                    // Release control of syncPoint.
                    FanartHandlerSetup.SyncPointRefresh = 0;
                }
                catch (Exception ex)
                {
                    Utils.ReleaseDelayStop("RefreshWorker-OnDoWork");
                    FanartHandlerSetup.SyncPointRefresh = 0;
                    logger.Error("OnDoWork: " + ex.ToString());
                }
            }
        }



        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    
                    int sync = Interlocked.CompareExchange(ref syncPointProgressChange, 1, 0);
                    if (sync == 0)
                    {
                        FanartHandlerSetup.PreventRefresh = true;
                        int windowId = GUIWindowManager.ActiveWindow;
                        if (FanartHandlerSetup.FR.CountSetVisibility == 1 && FanartHandlerSetup.FR.GetPropertiesRandom() > 0)  //after 2 sek
                        {
                            FanartHandlerSetup.FR.CountSetVisibility = 2;
                            FanartHandlerSetup.FR.UpdatePropertiesRandom();

                            if (FanartHandlerSetup.FR.DoShowImageOneRandom)
                            {
                                FanartHandlerSetup.FR.ShowImageOneRandom(windowId);
                            }
                            else
                            {
                                FanartHandlerSetup.FR.ShowImageTwoRandom(windowId);
                            }

                        }
                        else if (FanartHandlerSetup.FR.UpdateVisibilityCountRandom > 2 && FanartHandlerSetup.FR.GetPropertiesRandom() > 0) //after 2 sek
                        {
                            FanartHandlerSetup.FR.UpdatePropertiesRandom();
                        }
                        else if (FanartHandlerSetup.FR.UpdateVisibilityCountRandom >= 5 && FanartHandlerSetup.FR.GetPropertiesRandom() == 0) //after 4 sek
                        {
                            if (FanartHandlerSetup.FR.DoShowImageOneRandom)
                            {
                                FanartHandlerSetup.FR.DoShowImageOneRandom = false;
                            }
                            else
                            {
                                FanartHandlerSetup.FR.DoShowImageOneRandom = true;
                            }
                            FanartHandlerSetup.FR.CountSetVisibility = 0;
                            FanartHandlerSetup.FR.UpdateVisibilityCountRandom = 0;
                            //release unused image resources
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyGamesUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyMoviesUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyMoviesScraper);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyMovingPictures);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyMusicUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyMusicScraper);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyPicturesUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyScorecenterUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyPluginsUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyTVUser);
                            FanartHandlerSetup.HandleOldImages(ref FanartHandlerSetup.FR.ListAnyTVSeries);
                            FanartHandlerSetup.FR.WindowOpen = false;
                        }
                        FanartHandlerSetup.PreventRefresh = false;
                        syncPointProgressChange = 0;
                    }
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
