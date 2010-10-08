//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

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
using MediaPortal.Plugins.MovingPictures;
using MediaPortal.Plugins.MovingPictures.Database;  
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using TvDatabase;
using ForTheRecord.Entities;
using ForTheRecord.ServiceAgents;
using ForTheRecord.ServiceContracts;
using ForTheRecord.UI.Process.Recordings;
using WindowPlugins.GUITVSeries;

namespace FanartHandler
{
    public static class UtilsExternal
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread = false;
        #endregion

        public static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsExternal._isGetTypeRunningOnThisThread; }
            set { UtilsExternal._isGetTypeRunningOnThisThread = value; }
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

        public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;
            // Only process events from the thread that started it, not any other thread
            if (_isGetTypeRunningOnThisThread)
            {
                // Extract assembly name, and checking it's the same as args.Name
                // to prevent an infinite loop
                var an = new AssemblyName(args.Name);
                if (an.Name != args.Name)
                    assembly = ((AppDomain)sender).Load(an.Name);
            }
            return assembly;
        }

        public static Latests Get4TRRecordings()
        {
            Latests latests = new Latests();
            Latests result = new Latests();

            try
            {
                RecordingsModel _model = new RecordingsModel();
                RecordingsController _controller = new RecordingsController(_model);
                _controller.Initialize();
                _controller.SetChannelType(ChannelType.Television);
                ITvControlService _tvControlAgent = new TvControlServiceAgent();
                _controller.ReloadRecordingGroups(_tvControlAgent, RecordingGroupMode.GroupByProgramTitle);
                int groupIndex = 0;
                int x = 0;
                foreach (RecordingGroup recordingGroup in _model.RecordingGroups)
                {
                    RecordingSummary[] recordings = null;
                    recordings = _controller.GetRecordingsForGroup(_tvControlAgent, groupIndex, false);
                    if (recordings != null)
                    {
                        foreach (RecordingSummary rec in recordings)
                        {
                            string thumbNail = rec.ThumbnailFileName;
                            //if (!File.Exists(thumbNail))
                            //{
                            //    thumbNail = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music\default.jpg";
                            // }
                            latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), thumbNail, null, rec.Title, null, null, null, rec.Category, null, null, null, null, null, null, null));
                        }

                    }
                    groupIndex++;
                }
                latests.Sort(new LatestAddedComparer());
                for (int x0 = 0; x0 < latests.Count; x0++)
                {
                    latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                    result.Add(latests[x0]);
                    x++;
                    if (x == 3)
                    {
                        break;
                    }
                }
                _model = null;
                _controller = null;
                _tvControlAgent = null;
                latests.Clear();
                latests = null;
            }
            catch
            {
                if (latests != null)
                {
                    latests.Clear();
                }
                latests = null;
            }
            return result;
        }

        public static Latests GetTVRecordings()
        {
            Latests result = new Latests();
            Latests latests = new Latests();
            try
            {
                IList<TvDatabase.Recording> recordings = TvDatabase.Recording.ListAll();
                int x = 0;
                foreach (TvDatabase.Recording rec in recordings)
                {
                    string thumbNail = string.Format("{0}\\{1}{2}", Thumbs.TVRecorded,
                                                 Path.ChangeExtension(MediaPortal.Util.Utils.SplitFilename(rec.FileName), null),
                                                 MediaPortal.Util.Utils.GetThumbExtension());
                    //if (!File.Exists(thumbNail))
                    //{
                    //    thumbNail = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music\default.jpg";                    
                    //}
                    latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), thumbNail, null, rec.Title, null, null, null, rec.Genre, null, null, null, null, null, null, null));
                }
                latests.Sort(new LatestAddedComparer());
                for (int x0 = 0; x0 < latests.Count; x0++)
                {
                    latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                    result.Add(latests[x0]);
                    x++;
                    if (x == 3)
                    {
                        break;
                    }
                }
                latests.Clear();
                latests = null;
            }
            catch //(Exception ex)
            {
                if (latests != null)
                {
                    latests.Clear();
                }
                latests = null;
                //logger.Error("GetTVRecordings: " + ex.ToString());
            }
            return result;
        }

        /// <summary>
        /// Returns latest added movie thumbs from MovingPictures db.
        /// </summary>
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
        public static Latests GetLatestMovingPictures()
        {
            Latests result = new Latests();
            Latests latests = new Latests();
            int x = 0;
            string sTimestamp = string.Empty;
            try
            {
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    var vMovies = MovingPicturesCore.Settings.ParentalControlsFilter.Filter(DBMovieInfo.GetAll());
                    foreach (var item in vMovies)
                    {
                        sTimestamp = item.DateAdded.ToString("yyyy-MM-dd HH:mm:ss");
                        string fanart = item.CoverThumbFullPath;
                        //                        if (fanart != null && fanart.Trim().Length > 0)
                        //                      {
                        latests.Add(new Latest(sTimestamp, fanart, item.BackdropFullPath, item.Title, null, null, null, item.Genres.ToPrettyString(2), item.Score.ToString(), Math.Round(item.Score, MidpointRounding.AwayFromZero).ToString(), item.Certification, GetMovieRuntime(item) + " mins", item.Year.ToString(), null, null));
                        //                    }
                    }
                    vMovies.Clear();
                    vMovies = null;
                    latests.Sort(new LatestAddedComparer());
                    for (int x0 = 0; x0 < latests.Count; x0++)
                    {
                        latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                        result.Add(latests[x0]);
                        x++;
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
                        sTimestamp = item.DateAdded.ToString("yyyy-MM-dd HH:mm:ss");
                        string fanart = item.CoverThumbFullPath;
                        if (fanart != null && fanart.Trim().Length > 0)
                        {
                            latests.Add(new Latest(sTimestamp, fanart, item.BackdropFullPath, item.Title, null, null, null, item.Genres.ToPrettyString(2), item.Score.ToString(), Math.Round(item.Score, MidpointRounding.AwayFromZero).ToString(), item.Certification, GetMovieRuntime(item) + " mins", item.Year.ToString(), null, null));
                        }
                    }
                    vMovies.Clear();
                    vMovies = null;
                    latests.Sort(new LatestAddedComparer());
                    for (int x0 = 0; x0 < latests.Count; x0++)
                    {
                        latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                        result.Add(latests[x0]);
                        x++;
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


        /// <summary>
        /// Returns latest added tvseries thumbs from TVSeries db.
        /// </summary>
        /// <param name="type">Type of data to fetch</param>
        /// <returns>Resultset of matching data</returns>
        public static Latests GetLatestTVSeries()
        {
            UtilsExternal.Latests result = new UtilsExternal.Latests();
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
                            string fanart = Fanart.getFanart(episode[DBEpisode.cSeriesID]).FanartFilename;
                            string dateAdded = DBEpisode.cFileDateAdded;
                            string seriesGenre = series[DBOnlineSeries.cGenre];
                            string episodeRating = episode[DBOnlineEpisode.cRating];
                            string contentRating = episode[DBOnlineSeries.cContentRating];
                            string episodeRuntime = DBEpisode.cLocalPlaytime;
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
                                    mathRoundToString = Math.Round(double.Parse(episodeRating, ni), MidpointRounding.AwayFromZero).ToString();
                                }
                                catch 
                                {
                                }
                            }
                            result.Add(new UtilsExternal.Latest(dateAdded, thumb, fanart, seriesTitle, episodeTitle, null, null, seriesGenre, episodeRating, mathRoundToString, contentRating, episodeRuntime, episodeFirstAired, seasonIdx, episodeIdx));
                        }
                        series = null;
                    }
                    episodes.Clear();
                    episodes = null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetLatestTVSeries: " + ex.ToString());
            }
            return result;
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
            string windowId = GUIWindowManager.ActiveWindow.ToString();
            if (FanartHandlerSetup.LatestTVSeries.Equals("True"))// && !(windowId.Equals("9811") || windowId.Equals("9812") || windowId.Equals("9813") || windowId.Equals("9814") || windowId.Equals("9815")))
            {
                //if (!initialSetup)
                //{
                //    logger.Debug("Updating Latest Media Info: New episode added in TVSeries");
                //}
                UtilsExternal.Latests ht = null;
                try
                {
                    ht = UtilsExternal.GetLatestTVSeries();
                }
                catch
                { }
                if (ht != null)
                {
                    for (int i = 0; i < ht.Count; i++)
                    {
                        logger.Debug("Updating Latest Media Info: Latest episode " + z + ": " + ht[i].Title + " - " + ht[i].Subtitle);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.latest" + z + ".thumb", ht[i].Thumb);
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
                //if (!initialSetup)
                //{
                //    logger.Debug("Updating Latest Media Info: New movie added in MovingPicture");
                //}
                int z = 1;
                string windowId = GUIWindowManager.ActiveWindow.ToString();
                if (FanartHandlerSetup.LatestMovingPictures.Equals("True"))// && !(windowId.Equals("96742")))
                {
                    //Moving Pictures
                    UtilsExternal.Latests latestMovingPictures = null;
                    try
                    {
                        latestMovingPictures = UtilsExternal.GetLatestMovingPictures();//edbm.GetLatestMovingPictures();
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.latest" + z + ".firstAired", latestMovingPictures[i].Year);
                            z++;
                        }
                        latestMovingPictures.Clear();
                    }
                    latestMovingPictures = null;
                    z = 1;
                }                
            }
            catch (Exception ex)
            {
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
            if (movie == null) return minutes;

            if (MovingPicturesCore.Settings.DisplayActualRuntime && movie.ActualRuntime > 0)
            {
                // Actual Runtime or (MediaInfo result) is in milliseconds
                // convert to minutes
                minutes = ((movie.ActualRuntime / 1000) / 60).ToString();
            }
            else
                minutes = movie.Runtime.ToString();

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
                        Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie"), fanart, fanart, "MovingPicture", 1);
                        Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie"), fanart, fanart, "MovingPicture", 1);
                    }
                }
                vMovies2.Clear();
                vMovies2 = null;
                if (MovingPicturesCore.Settings.ParentalControlsEnabled)
                {
                    var vMovies1 = MovingPicturesCore.Settings.ParentalControlsFilter.Filter(DBMovieInfo.GetAll());
                    foreach (var item in vMovies1)
                    {
                        string fanart = item.BackdropFullPath;
                        if (fanart != null && fanart.Trim().Length > 0)
                        {
                            Utils.GetDbm().LoadFanartExternal(Utils.GetArtist(item.Title, "Movie"), fanart, fanart, "MovingPicture", 0);
                            Utils.GetDbm().LoadFanart(Utils.GetArtist(item.Title, "Movie"), fanart, fanart, "MovingPicture", 0);
                        }
                    }
                    vMovies1.Clear();
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



        public static Hashtable GetTVSeriesName(string type)
        {
            Hashtable ht = new Hashtable();
            try
            {
                string artist = String.Empty;
                string seriesId = String.Empty;
                // Calculate date for querying database
                DateTime dt = new DateTime(1980, 01, 01);
                string date = dt.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
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
                    episodes.Clear();
                    episodes = null;
                }

            }
            catch (Exception ex)
            {
                logger.Error("GetTVSeriesName: " + ex.ToString());
            }
            return ht;
        }

        public class Latest
        {
            string dateAdded;
            string thumb;
            string fanart;
            string title;
            string subtitle;
            string artist;
            string album;
            string genre;
            string rating;
            string roundedRating;
            string classification;
            string runtime;
            string year;
            string seasonIndex;
            string episodeIndex;

            public string DateAdded
            {
                get { return dateAdded; }
                set { dateAdded = value; }
            }

            public string Thumb
            {
                get { return thumb; }
                set { thumb = value; }
            }

            public string Fanart
            {
                get { return fanart; }
                set { fanart = value; }
            }

            public string Title
            {
                get { return title; }
                set { title = value; }
            }

            public string Subtitle
            {
                get { return subtitle; }
                set { subtitle = value; }
            }

            public string Artist
            {
                get { return artist; }
                set { artist = value; }
            }

            public string Album
            {
                get { return album; }
                set { album = value; }
            }

            public string Genre
            {
                get { return genre; }
                set { genre = value; }
            }

            public string Rating
            {
                get { return rating; }
                set { rating = value; }
            }

            public string RoundedRating
            {
                get { return roundedRating; }
                set { roundedRating = value; }
            }

            public string Classification
            {
                get { return classification; }
                set { classification = value; }
            }

            public string Runtime
            {
                get { return runtime; }
                set { runtime = value; }
            }

            public string Year
            {
                get { return year; }
                set { year = value; }
            }

            public string SeasonIndex
            {
                get { return seasonIndex; }
                set { seasonIndex = value; }
            }

            public string EpisodeIndex
            {
                get { return episodeIndex; }
                set { episodeIndex = value; }
            }

            public Latest(string dateAdded, string thumb, string fanart, string title, string subtitle, string artist, string album, string genre, string rating, string roundedRating, string classification, string runtime, string year, string seasonIndex, string episodeIndex)
            {
                this.dateAdded = dateAdded;
                this.thumb = thumb;
                this.fanart = fanart;
                this.title = title;
                this.subtitle = subtitle;
                this.artist = artist;
                this.album = album;
                this.genre = genre;
                this.rating = rating;
                this.roundedRating = roundedRating;
                this.classification = classification;
                this.runtime = runtime;
                this.year = year;
                this.seasonIndex = seasonIndex;
                this.episodeIndex = episodeIndex;
            }

        }

        class LatestAddedComparer : IComparer<Latest>
        {

            #region IComparer<LatestMovingPicture> Members

            public int Compare(Latest latest1, Latest latest2)
            {
                int returnValue = 1;
                if (latest1 is Latest && latest2 is Latest)
                {
                    returnValue = latest2.DateAdded.CompareTo(latest1.DateAdded);
                }

                return returnValue;
            }

            #endregion
        }

        public class Latests : List<Latest>
        {
        }    

    }
}
