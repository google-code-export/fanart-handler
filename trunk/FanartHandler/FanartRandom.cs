//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using NLog;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;   

    /// <summary>
    /// Class handling fanart for random backdrops.
    /// </summary>
    class FanartRandom
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int currCountRandom = 0;
        private int updateVisibilityCountRandom = 0;
        private int countSetVisibility = 0;
        private bool firstRandom = true;  //Special case on first random        
        private bool windowOpen = false;
        private bool doShowImageOneRandom = true; // Decides if property .1 or .2 should be set on next run        
        private Hashtable windowsUsingFanartRandom; //used to know what skin files that supports random fanart        
        private Hashtable propertiesRandom; //used to hold properties to be updated (Random)        

        private string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";

        public Random randAnyGames = null;
        public Random randAnyMovies = null;
        public Random randAnyMovingPictures = null;
        public Random randAnyMusic = null;
        public Random randAnyPictures = null;
        public Random randAnyScorecenter = null;
        public Random randAnyTVSeries = null;
        public Random randAnyTV = null;
        public Random randAnyPlugins = null;

        private bool useAnyGames = false;
        private bool useAnyMusic = false;
        private bool useAnyMovies = false;
        private bool useAnyMovingPictures = false;
        private bool useAnyPictures = false;
        private bool useAnyScoreCenter = false;
        private bool useAnyTVSeries = false;
        private bool useAnyTV = false;
        private bool useAnyPlugins = false;        

        private string currAnyGames = null;
        private string currAnyMovies = null;
        private string currAnyMovingPictures = null;
        private string currAnyMusic = null;
        private string currAnyPictures = null;
        private string currAnyScorecenter = null;
        private string currAnyTVSeries = null;
        private string currAnyTV = null;
        private string currAnyPlugins = null;

        public ArrayList listAnyGames = null;
        public ArrayList listAnyMovies = null;
        public ArrayList listAnyMovingPictures = null;
        public ArrayList listAnyMusic = null;
        public ArrayList listAnyPictures = null;
        public ArrayList listAnyScorecenter = null;
        public ArrayList listAnyTVSeries = null;
        public ArrayList listAnyTV = null;
        public ArrayList listAnyPlugins = null;
        #endregion

        public bool UseAnyPlugins
        {
            get { return useAnyPlugins; }
            set { useAnyPlugins = value; }
        }

        public bool UseAnyTV
        {
            get { return useAnyTV; }
            set { useAnyTV = value; }
        }

        public bool UseAnyTVSeries
        {
            get { return useAnyTVSeries; }
            set { useAnyTVSeries = value; }
        }

        public bool UseAnyScoreCenter
        {
            get { return useAnyScoreCenter; }
            set { useAnyScoreCenter = value; }
        }

        public bool UseAnyPictures
        {
            get { return useAnyPictures; }
            set { useAnyPictures = value; }
        }

        public bool UseAnyMovingPictures
        {
            get { return useAnyMovingPictures; }
            set { useAnyMovingPictures = value; }
        }

        public bool UseAnyMovies
        {
            get { return useAnyMovies; }
            set { useAnyMovies = value; }
        }

        public bool UseAnyMusic
        {
            get { return useAnyMusic; }
            set { useAnyMusic = value; }
        }

        public bool UseAnyGames
        {
            get { return useAnyGames; }
            set { useAnyGames = value; }
        }

        public Hashtable PropertiesRandom
        {
            get { return propertiesRandom; }
            set { propertiesRandom = value; }
        }

        public Hashtable WindowsUsingFanartRandom
        {
            get { return windowsUsingFanartRandom; }
            set { windowsUsingFanartRandom = value; }
        }

        public bool DoShowImageOneRandom
        {
            get { return doShowImageOneRandom; }
            set { doShowImageOneRandom = value; }
        }

        public bool WindowOpen
        {
            get { return windowOpen; }
            set { windowOpen = value; }
        }

        public bool FirstRandom
        {
            get { return firstRandom; }
            set { firstRandom = value; }
        }

        public int CountSetVisibility
        {
            get { return countSetVisibility; }
            set { countSetVisibility = value; }
        }

        public int UpdateVisibilityCountRandom
        {
            get { return updateVisibilityCountRandom; }
            set { updateVisibilityCountRandom = value; }
        }

        /// <summary>
        /// Class for the skin define tags
        /// </summary>
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
 



        /// <summary>
        /// Get and set properties for random images
        /// </summary>
        public void RefreshRandomImageProperties()
        {
            try
            {                
                if ((currCountRandom >= FanartHandlerSetup.MaxCountImage) || FirstRandom || currCountRandom == 0)
                {
                    string sFilename = String.Empty;                    
                    if (SupportsRandomImages("useRandomMoviesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMovies, ref randAnyMovies, "Movie");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.movie.backdrop1.any", sFilename, ref listAnyMovies, "Movie");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.backdrop2.any", sFilename, ref listAnyMovies, "Movie");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movie.backdrop2.any", sFilename, ref listAnyMovies, "Movie");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.movie.backdrop1.any", sFilename, ref listAnyMovies, "Movie");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMovies);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMovingPictures, ref randAnyMovingPictures, "MovingPicture");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref listAnyMovingPictures, "MovingPicture");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref listAnyMovingPictures, "MovingPicture");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref listAnyMovingPictures, "MovingPicture");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref listAnyMovingPictures, "MovingPicture");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", tmpImage);
                        }                        
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMovingPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomMusicFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMusic, ref randAnyMusic, "MusicFanart");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop1.any", sFilename, ref listAnyMusic, "MusicFanart");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.music.backdrop2.any", sFilename, ref listAnyMusic, "MusicFanart");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop2.any", sFilename, ref listAnyMusic, "MusicFanart");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.music.backdrop1.any", sFilename, ref listAnyMusic, "MusicFanart");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMusic);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", tmpImage);
                    }                    
                    if (SupportsRandomImages("useRandomTVFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyTV, ref randAnyTV, "TV");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop1.any", sFilename, ref listAnyTV, "TV");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.tv.backdrop2.any", sFilename, ref listAnyTV, "TV");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop2.any", sFilename, ref listAnyTV, "TV");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.tv.backdrop1.any", sFilename, ref listAnyTV, "TV");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyTV);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyTVSeries, ref randAnyTVSeries, "TVSeries");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref listAnyTVSeries, "TVSeries");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref listAnyTVSeries, "TVSeries");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref listAnyTVSeries, "TVSeries");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref listAnyTVSeries, "TVSeries");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyTVSeries);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomPicturesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyPictures, ref randAnyPictures, "Picture");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop1.any", sFilename, ref listAnyPictures, "Picture");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.picture.backdrop2.any", sFilename, ref listAnyPictures, "Picture");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop2.any", sFilename, ref listAnyPictures, "Picture");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.picture.backdrop1.any", sFilename, ref listAnyPictures, "Picture");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomGamesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyGames, ref randAnyGames, "Game");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop1.any", sFilename, ref listAnyGames, "Game");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.games.backdrop2.any", sFilename, ref listAnyGames, "Game");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop2.any", sFilename, ref listAnyGames, "Game");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.games.backdrop1.any", sFilename, ref listAnyGames, "Game");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyGames);
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", tmpImage);
                    }
                    if (SupportsRandomImages("useRandomScoreCenterFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyScorecenter, ref randAnyScorecenter, "ScoreCenter");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop1.any", sFilename, ref listAnyScorecenter, "ScoreCenter");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref listAnyScorecenter, "ScoreCenter");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref listAnyScorecenter, "ScoreCenter");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.backdrop1.any", sFilename, ref listAnyScorecenter, "ScoreCenter");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyScorecenter);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", tmpImage);
                    }                    
                    if (SupportsRandomImages("useRandomPluginsFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyPlugins, ref randAnyPlugins, "Plugin");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", sFilename, ref listAnyPlugins, "Plugin");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.backdrop2.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", sFilename, ref listAnyPlugins, "Plugin");
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", sFilename, ref listAnyPlugins, "Plugin");
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.backdrop1.any");
                                if (sTag == null || sTag.Length < 2)
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", sFilename, ref listAnyPlugins, "Plugin");
                                }
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", tmpImage);
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyPlugins);
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", tmpImage);
                    }
                    ResetCurrCountRandom();
                    FirstRandom = false;
                }
                IncreaseCurrCountRandom();
            }
            catch (Exception ex)
            {
                logger.Error("RefreshRandomImageProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Check if current skin file supports random images
        /// </summary>
        private String SupportsRandomImages(string type)
        {
            SkinFile sf = (SkinFile)WindowsUsingFanartRandom[GUIWindowManager.ActiveWindow.ToString()];
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
        /// Add image properties that later will update the skin properties
        /// </summary>
        private void AddPropertyRandom(string property, string value, ref ArrayList al, string type)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";

                if (PropertiesRandom.Contains(property))
                {
                    PropertiesRandom[property] = value;
                }
                else
                {
                    PropertiesRandom.Add(property, value);
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
                            Utils.LoadImage(value, type);
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                logger.Error("AddPropertyRandom: " + ex.ToString());
            }
        }

        /// <summary>
        /// Get next filename to return as property to skin
        /// </summary>
        public string GetRandomFilename(ref int randomCounter, ref string prevImage, ref Random randNum, string type)
        {
            string sout = String.Empty;
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    sout = prevImage;
                    string types = String.Empty;
                    if (type.Equals("MusicFanart"))
                    {
                        if (FanartHandlerSetup.UseAlbum.Equals("True") && FanartHandlerSetup.DisableMPTumbsForRandom.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicAlbum'";
                            else
                                types = "'MusicAlbum'";
                        }
                        if (FanartHandlerSetup.UseArtist.Equals("True") && FanartHandlerSetup.DisableMPTumbsForRandom.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicArtist'";
                            else
                                types = "'MusicArtist'";
                        }
                        if (FanartHandlerSetup.UseFanart.Equals("True"))
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
                    Hashtable ht = Utils.GetDbm().GetAnyFanart(type, types);
                    if (ht != null && ht.Count > 0)
                    {
                        bool doRun = true;
                        int attempts = 0;
                        while (doRun && attempts < (ht.Count * 2))
                        {
                            int iHt = randNum.Next(0, ht.Count);
                            DatabaseManager.FanartImage imgFile = (DatabaseManager.FanartImage)ht[iHt];
                            sout = imgFile.disk_image;
                            if (FanartHandlerSetup.CheckImageResolution(sout, type, FanartHandlerSetup.UseAspectRatio) && Utils.IsFileValid(sout))
                            {
                                prevImage = sout;
                                //ResetCurrCountRandom();
                                if (CountSetVisibility == 0)
                                {
                                    CountSetVisibility = 1;
                                }
                                doRun = false;
                            }
                            attempts++;
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
        /// Reset interval counter and trigger update of skinproperties
        /// </summary>
        public void ResetCurrCountRandom()
        {
            currCountRandom = 0;
            UpdateVisibilityCountRandom = 1;
        }

        /// <summary>
        /// Increase the interval counter
        /// </summary>
        private void IncreaseCurrCountRandom()
        {
            currCountRandom = currCountRandom + 1;
        }

        /// <summary>
        /// Get total properties
        /// </summary>
        public int GetPropertiesRandom()
        {
            if (PropertiesRandom == null) return 0;

            return PropertiesRandom.Count;
        }

        /// <summary>
        /// Clear total properties
        /// </summary>
        public void ClearPropertiesRandom()
        {
            if (PropertiesRandom != null)
            {
                PropertiesRandom.Clear();
            }
        }


        /// <summary>
        /// Update the skin image properties
        /// </summary>
        public void UpdatePropertiesRandom()
        {
            try
            {
                Hashtable ht = new Hashtable();
                int x = 0;
                foreach (DictionaryEntry de in PropertiesRandom)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                    ht.Add(x, de.Key.ToString());
                    x++;
                }
                for (int i = 0; i < ht.Count; i++)
                {
                    PropertiesRandom.Remove(ht[i].ToString());
                }                
                ht = null;
                //propertiesRandom.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesRandom: " + ex.ToString());
            }
        }

        /// <summary>
        /// checks all xml files in the current skin directory to see if it uses random property
        /// </summary>
        public void SetupWindowsUsingRandomImages()
        {
            XPathDocument myXPathDocument;
            WindowsUsingFanartRandom = new Hashtable();
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
                        SkinFile sf = new SkinFile();
                        sf.id = windowId;
                        myXPathNodeIterator = myXPathNavigator.Select("/window/define");
                        if (myXPathNodeIterator.Count > 0)
                        {
                            while (myXPathNodeIterator.MoveNext())
                            {
                                sNodeValue = myXPathNodeIterator.Current.Value;
                                if (sNodeValue.StartsWith("#useRandomGamesFanart"))
                                    sf.useRandomGamesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMoviesFanart"))
                                    sf.useRandomMoviesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMovingPicturesFanart"))
                                    sf.useRandomMovingPicturesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMusicFanart"))
                                    sf.useRandomMusicFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPicturesFanart"))
                                    sf.useRandomPicturesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomScoreCenterFanart"))
                                    sf.useRandomScoreCenterFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVSeriesFanart"))
                                    sf.useRandomTVSeriesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVFanart"))
                                    sf.useRandomTVFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPluginsFanart"))
                                    sf.useRandomPluginsFanart = ParseNodeValue(sNodeValue);
                            }
                            if (sf.useRandomGamesFanart != null && sf.useRandomGamesFanart.Length > 0)
                            {
                                if (sf.useRandomGamesFanart.Equals("True"))
                                {
                                    UseAnyGames = true;
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
                                    UseAnyMovies = true;
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
                                    UseAnyMovingPictures = true;
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
                                    UseAnyMusic = true;
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
                                    UseAnyPictures = true;
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
                                    UseAnyScoreCenter = true;
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
                                    UseAnyTVSeries = true;
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
                                    UseAnyTV = true;
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
                                    UseAnyPlugins = true;
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
                                WindowsUsingFanartRandom.Add(windowId, sf);
                            }
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("SetupWindowsUsingRandomImages, filename:" + s + "): " + ex.ToString());
                }
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



        /// <summary>
        /// Parse node value
        /// </summary>
        private string ParseNodeValue(string s)
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
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageOneRandom(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919297);
            GUIControl.HideControl(windowId, 91919298);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageTwoRandom(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919298);
            GUIControl.HideControl(windowId, 91919297);
        }

    }
}
