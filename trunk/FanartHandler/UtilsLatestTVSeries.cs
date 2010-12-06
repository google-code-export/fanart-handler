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
    public static class UtilsLatestTVSeries 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
        private static VideoHandler episodePlayer = null;
        private static Hashtable latestTVSeries;        
        #endregion

        public static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsLatestTVSeries._isGetTypeRunningOnThisThread; }
            set { UtilsLatestTVSeries._isGetTypeRunningOnThisThread = value; }
        }        

        /// <summary>
        /// Returns latest added tvseries thumbs from TVSeries db.
        /// </summary>
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
        public static LatestsCollection GetLatestTVSeries()
        {
            FanartHandler.LatestsCollection result = new FanartHandler.LatestsCollection();
            try
            {
                // Calculate date for querying database
                /*DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                dt = dt.Subtract(new TimeSpan(60, 0, 0, 0, 0));
                string date = dt.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");

                // Get a list of the most recently added episodes in the database
                SQLCondition conditions = new SQLCondition();
                conditions.Add(new DBEpisode(), DBEpisode.cFileDateCreated, date, SQLConditionType.GreaterEqualThan);
                conditions.AddOrderItem(DBEpisode.Q(DBEpisode.cFileDateCreated), SQLCondition.orderType.Descending);
                List<DBEpisode> episodes = DBEpisode.Get(conditions, false);
                 */
                latestTVSeries = new Hashtable();
                int i0 = 1;
                List<DBEpisode> episodes = DBEpisode.GetMostRecent(MostRecentType.Created, 60, 3);
                if (episodes != null)
                {
                    //if (episodes.Count > 3) episodes.RemoveRange(3, episodes.Count - 3);
                    //episodes.Reverse();

                    foreach (DBEpisode episode in episodes)
                    {
                        DBSeries series = Helper.getCorrespondingSeries(episode[DBEpisode.cSeriesID]);
                        if (series != null)
                        {
                            string episodeTitle = episode[DBEpisode.cEpisodeName];
                            string seasonIdx = episode[DBEpisode.cSeasonIndex];
                            string episodeIdx = episode[DBEpisode.cEpisodeIndex];
                            string seriesTitle = series.ToString();
                            string thumb = ImageAllocator.GetEpisodeImage(episode);
                            string thumbSeries = ImageAllocator.GetSeriesPoster(series);
                            string fanart = Fanart.getFanart(episode[DBEpisode.cSeriesID]).FanartFilename;
                            string dateAdded = episode[DBEpisode.cFileDateAdded];
                            string seriesGenre = series[DBOnlineSeries.cGenre];
                            string episodeRating = episode[DBOnlineEpisode.cRating];
                            string contentRating = episode[DBOnlineSeries.cContentRating];
                            string episodeRuntime = episode[DBEpisode.cLocalPlaytime];
                            string episodeFirstAired = episode[DBOnlineEpisode.cFirstAired];

                            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InstalledUICulture;
                            System.Globalization.NumberFormatInfo ni = (System.Globalization.NumberFormatInfo)ci.NumberFormat.Clone();
                            ni.NumberDecimalSeparator = ".";
                            string mathRoundToString = string.Empty;
                            if (episodeRating != null && episodeRating.Length > 0)
                            {
                                try
                                {
                                    episodeRating = episodeRating.Replace(",", ".");
                                    mathRoundToString = Math.Round(double.Parse(episodeRating, ni), MidpointRounding.AwayFromZero).ToString(CultureInfo.CurrentCulture);
                                }
                                catch
                                {
                                }
                            }
                            result.Add(new FanartHandler.Latest(dateAdded, thumb, fanart, seriesTitle, episodeTitle, null, null, seriesGenre, episodeRating, mathRoundToString, contentRating, episodeRuntime, episodeFirstAired, seasonIdx, episodeIdx, thumbSeries, null, null, null));
                            latestTVSeries.Add(i0, episode);
                            i0++;
                        }
                        series = null;
                    }
                    if (episodes != null)
                    {
                        episodes.Clear();
                    }
                    episodes = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetLatestTVSeries: " + ex.ToString());
            }
            return result;
        }

        public static void PlayTVSeries(int index)
        {
            if (episodePlayer == null)
            {
                episodePlayer = new VideoHandler();
            }

            episodePlayer.ResumeOrPlay((DBEpisode)latestTVSeries[index]);
        }

        public static void SetupTVSeriesLatest()
        {
            OnlineParsing.OnlineParsingCompleted += new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
        }

        public static void DisposeTVSeriesLatest()
        {
            OnlineParsing.OnlineParsingCompleted -= new OnlineParsing.OnlineParsingCompletedHandler(TVSeriesOnObjectInserted);
        }

        private static void TVSeriesOnObjectInserted(bool dataUpdated)
        {
            if (dataUpdated)
            {
                TVSeriesUpdateLatest(false);
            }
        }

        public static void TVSeriesUpdateLatest(bool initialSetup)
        {
            int z = 1;
            //            string windowId = GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture) // COMMENTED BY CODEIT.RIGHT;
            if (FanartHandlerSetup.LatestTVSeries.Equals("True", StringComparison.CurrentCulture))// && !(windowId.Equals("9811") || windowId.Equals("9812") || windowId.Equals("9813") || windowId.Equals("9814") || windowId.Equals("9815")))
            {
                //if (!initialSetup)
                //{
                //    logger.Debug("Updating Latest Media Info: New episode added in TVSeries");
                //}
                FanartHandler.LatestsCollection ht = null;
                try
                {
                    ht = UtilsLatestTVSeries.GetLatestTVSeries();
                }
                catch
                { }
                if (ht != null)
                {
                    for (int i = 0; i < ht.Count; i++)
                    {
                        logger.Debug("Updating Latest Media Info: Latest episode " + z + ": " + ht[i].Title + " - " + ht[i].Subtitle);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".thumb", ht[i].Thumb);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".serieThumb", ht[i].ThumbSeries);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".fanart", ht[i].Fanart);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".serieName", ht[i].Title);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".seasonIndex", ht[i].SeasonIndex);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".episodeName", ht[i].Subtitle);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".episodeIndex", ht[i].EpisodeIndex);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".dateAdded", ht[i].DateAdded);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".genre", ht[i].Genre);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".rating", ht[i].Rating);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".roundedRating", ht[i].RoundedRating);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".classification", ht[i].Classification);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".runtime", ht[i].Runtime);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".firstAired", ht[i].Year);                        
                        z++;
                    }
                    ht.Clear();
                }
                ht = null;
                z = 1;
            }

        }

        public static Hashtable GetTVSeriesName(string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string artist = String.Empty;
                string seriesId = String.Empty;
                // Calculate date for querying database
                DateTime dt = new DateTime(1980, 01, 01);
                string date = dt.ToString("yyyy'-'MM'-'dd HH':'mm':'ss", CultureInfo.CurrentCulture);
                SQLCondition conditions = new SQLCondition();
                conditions.Add(new DBEpisode(), DBEpisode.cFileDateCreated, date, SQLConditionType.GreaterEqualThan);
                conditions.AddOrderItem(DBEpisode.Q(DBEpisode.cFileDateCreated), SQLCondition.orderType.Descending);
                List<DBEpisode> episodes = DBEpisode.Get(conditions, false);

                if (episodes != null)
                {
                    //if (episodes.Count > 3) episodes.RemoveRange(3, episodes.Count - 3);
                    //episodes.Reverse();

                    foreach (DBEpisode episode in episodes)
                    {
                        DBSeries series = Helper.getCorrespondingSeries(episode[DBEpisode.cSeriesID]);
                        if (series != null)
                        {
                            artist = Utils.GetArtist(series.ToString(), type);
                            seriesId = series[DBOnlineSeries.cID];
                            if (!ht.Contains(seriesId))
                            {
                                ht.Add(seriesId, artist);
                            }
                        }
                        series = null;
                    }
                    if (episodes != null)
                    {
                        episodes.Clear();
                    }
                    episodes = null;
                }

            }
            catch (Exception ex)
            {
                logger.Error("GetTVSeriesName: " + ex.ToString());
            }
            return ht;
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
