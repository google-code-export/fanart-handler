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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using NLog; 
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Playlists;
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;
using MediaPortal.Plugins.MovingPictures.MainUI;
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using TvDatabase;
using ForTheRecord.Entities;
using ForTheRecord.ServiceAgents;
using ForTheRecord.ServiceContracts;
using ForTheRecord.UI.Process.Recordings;
using WindowPlugins.GUITVSeries;
using System.Globalization;


namespace FanartHandler
{
    public static class UtilsLatestMovingPictures 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
        private static MoviePlayer moviePlayer;
        private static Hashtable latestMovingPictures;        
        #endregion

        public static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsLatestMovingPictures._isGetTypeRunningOnThisThread; }
            set { UtilsLatestMovingPictures._isGetTypeRunningOnThisThread = value; }
        }   

        public static int MovingPictureIsRestricted()
        {
            try
            {
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }

            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Returns latest added movie thumbs from MovingPictures db.
        /// </summary>
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
        public static LatestsCollection GetLatestMovingPictures()
        {
            LatestsCollection result = new LatestsCollection();
            LatestsCollection latests = new LatestsCollection();
            int x = 0;
            string sTimestamp = string.Empty;
            try
            {
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    var vMovies = MovingPicturesCore.Settings.ParentalControlsFilter.Filter(DBMovieInfo.GetAll());
                    foreach (var item in vMovies)
                    {
                        sTimestamp = item.DateAdded.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                        string fanart = item.CoverThumbFullPath;
                        latests.Add(new Latest(sTimestamp, fanart, item.BackdropFullPath, item.Title, null, null, null, item.Genres.ToPrettyString(2), item.Score.ToString(CultureInfo.CurrentCulture), Math.Round(item.Score, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture), item.Certification, GetMovieRuntime(item) + " mins", item.Year.ToString(CultureInfo.CurrentCulture), null, null, null, item, null, null, item.ID.ToString(),item.Summary));                        
                    }
                    if (vMovies != null)
                    {
                        vMovies.Clear();
                    }
                    vMovies = null;
                    latestMovingPictures = new Hashtable();
                    int i0 = 1;
                    latests.Sort(new LatestAddedComparer());
                    for (int x0 = 0; x0 < latests.Count; x0++)
                    {
                        latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                        result.Add(latests[x0]);
                        x++;
                        latestMovingPictures.Add(i0, latests[x0].Playable);
                        i0++;
                        if (x == 3)
                        {
                            break;
                        }
                    }

                }
                else
                {
                    var vMovies = DBMovieInfo.GetAll();                    
                    foreach (var item in vMovies)
                    {
                        sTimestamp = item.DateAdded.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                        string fanart = item.CoverThumbFullPath;
                        if (fanart != null && fanart.Trim().Length > 0)
                        {
                            latests.Add(new Latest(sTimestamp, fanart, item.BackdropFullPath, item.Title, null, null, null, item.Genres.ToPrettyString(2), item.Score.ToString(CultureInfo.CurrentCulture), Math.Round(item.Score, MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture), item.Certification, GetMovieRuntime(item) + " mins", item.Year.ToString(CultureInfo.CurrentCulture), null, null, null, item, null, null, item.ID.ToString(), item.Summary));
                        }
                    }
                    if (vMovies != null)
                    {
                        vMovies.Clear();
                    }
                    vMovies = null;
                    latestMovingPictures = new Hashtable();
                    int i0 = 1;
                    latests.Sort(new LatestAddedComparer());
                    for (int x0 = 0; x0 < latests.Count; x0++)
                    {
                        latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                        result.Add(latests[x0]);
                        x++;
                        latestMovingPictures.Add(i0, latests[x0].Playable);
                        i0++;
                        if (x == 3)
                        {
                            break;
                        }
                    }
                }
            }
            catch //(Exception ex
            {
                if (latests != null)
                {
                    latests.Clear();
                }
                latests = null;
                //logger.Error("getData: " + ex.ToString());
            }
            if (latests != null)
            {
                latests.Clear();
            }
            latests = null;
            return result;
        }

        public static void PlayMovingPicture(int index)
        {
            if (moviePlayer == null)
            {
                moviePlayer = new MoviePlayer(new MovingPicturesGUI());
            }

            moviePlayer.Play((DBMovieInfo)latestMovingPictures[index]);
        }

        public static void SetupMovingPicturesLatest()
        {
            MovingPicturesCore.DatabaseManager.ObjectInserted += new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
        }

        public static void DisposeMovingPicturesLatest()
        {
            MovingPicturesCore.DatabaseManager.ObjectInserted -= new Cornerstone.Database.DatabaseManager.ObjectAffectedDelegate(MovingPictureOnObjectInserted);
        }

        private static void MovingPictureOnObjectInserted(DatabaseTable obj)
        {
            if (obj.GetType() == typeof(DBMovieInfo))
            {
                MovingPictureUpdateLatest(false);
            }
        }

        public static void MovingPictureUpdateLatest(bool initialSetup)
        {
            try
            {
                int z = 1;
                if (FanartHandlerSetup.LatestMovingPictures.Equals("True", StringComparison.CurrentCulture))// && !(windowId.Equals("96742")))
                {
                    //Moving Pictures
                    LatestsCollection latestMovingPictures = null;
                    try
                    {
                        latestMovingPictures = GetLatestMovingPictures();//edbm.GetLatestMovingPictures();
                    }
                    catch
                    { }
                    if (latestMovingPictures != null)
                    {
                        for (int i = 0; i < latestMovingPictures.Count; i++)
                        {
                            logger.Debug("Updating Latest Media Info: Latest movie " + z + ": " + latestMovingPictures[i].Title);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".thumb", latestMovingPictures[i].Thumb);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".fanart", latestMovingPictures[i].Fanart);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".title", latestMovingPictures[i].Title);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".dateAdded", latestMovingPictures[i].DateAdded);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".genre", latestMovingPictures[i].Genre);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".rating", latestMovingPictures[i].Rating);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".roundedRating", latestMovingPictures[i].RoundedRating);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".classification", latestMovingPictures[i].Classification);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".runtime", latestMovingPictures[i].Runtime);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".year", latestMovingPictures[i].Year);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".id", latestMovingPictures[i].Id);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".plot", latestMovingPictures[i].Summary);
                            z++;
                        }
                        latestMovingPictures.Clear();
                    }
                    latestMovingPictures = null;
                    z = 1;
                    FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest.enabled", "true");
                }
                else
                {
                    FanartHandlerSetup.EmptyLatestMediaPropsMovingPictures();
                }
            }
            catch (Exception ex)
            {
                FanartHandlerSetup.EmptyLatestMediaPropsMovingPictures();
                logger.Error("MovingPictureOnObjectInserted: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get the runtime of a movie using its MediaInfo property
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private static string GetMovieRuntime(DBMovieInfo movie)
        {
            string minutes = string.Empty;
            if (movie == null)
            {
                return minutes;
            }


            if (MovingPicturesCore.Settings.DisplayActualRuntime && movie.ActualRuntime > 0)
            {
                // Actual Runtime or (MediaInfo result) is in milliseconds
                // convert to minutes
                minutes = ((movie.ActualRuntime / 1000) / 60).ToString(CultureInfo.CurrentCulture);
            }
            else
                minutes = movie.Runtime.ToString(CultureInfo.CurrentCulture);

            return minutes;
        }

        public static void GetMovingPicturesBackdrops()
        {
            try
            {
                var vMovies2 = DBMovieInfo.GetAll();
                foreach (var item in vMovies2)
                {
                    string fanart = item.BackdropFullPath;
                    if (fanart != null && fanart.Trim().Length > 0)
                    {
                        Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 1);
                        Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 1);                        
                    }
                }
                if (vMovies2 != null)
                {
                    vMovies2.Clear();
                }
                vMovies2 = null;
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    var vMovies1 = MovingPicturesCore.Settings.ParentalControlsFilter.Filter(DBMovieInfo.GetAll());
                    foreach (var item in vMovies1)
                    {
                        string fanart = item.BackdropFullPath;
                        if (fanart != null && fanart.Trim().Length > 0)
                        {
                            Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 0);
                            Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie Scraper"), fanart, fanart, "MovingPicture", 0);                            
                        }
                    }
                    if (vMovies1 != null)
                    {
                        vMovies1.Clear();
                    }
                    vMovies1 = null;
                }
            }
            catch (MissingMethodException)
            {

            }
            catch //(Exception ex
            {
                //logger.Error("GetMovingPicturesBackdrops: " + ex.ToString());
            }
        }

        class LatestAddedComparer : IComparer<Latest>
        {
            public int Compare(Latest latest1, Latest latest2)
            {
                int returnValue = 1;
                if (latest1 is Latest && latest2 is Latest)
                {
                    returnValue = latest2.DateAdded.CompareTo(latest1.DateAdded);
                }

                return returnValue;
            }
        }



    }




}
