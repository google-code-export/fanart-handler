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
    public class FanartRandom
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private int _currCountRandom; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public int CurrCountRandom
        {
            get
            {
                return _currCountRandom;
            }
            set
            {
                _currCountRandom = value;
            }
        }
        private int updateVisibilityCountRandom/* = 0*/;
        private int countSetVisibility/* = 0*/;
        private bool firstRandom = true;  //Special case on first random        
        private bool windowOpen/* = false*/;
        private bool doShowImageOneRandom = true; // Decides if property .1 or .2 should be set on next run        
        private Hashtable windowsUsingFanartRandom; //used to know what skin files that supports random fanart        
        private Hashtable propertiesRandom; //used to hold properties to be updated (Random)        
        private Hashtable htAny;

        private string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";

        private Random _randAnyGames = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyGames
        {
            get
            {
                return _randAnyGames;
            }
            set
            {
                _randAnyGames = value;
            }
        }
        private Random _randAnyMovies = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMovies
        {
            get
            {
                return _randAnyMovies;
            }
            set
            {
                _randAnyMovies = value;
            }
        }
        private Random _randAnyMovingPictures = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMovingPictures
        {
            get
            {
                return _randAnyMovingPictures;
            }
            set
            {
                _randAnyMovingPictures = value;
            }
        }
        private Random _randAnyMusic = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyMusic
        {
            get
            {
                return _randAnyMusic;
            }
            set
            {
                _randAnyMusic = value;
            }
        }
        private Random _randAnyPictures = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyPictures
        {
            get
            {
                return _randAnyPictures;
            }
            set
            {
                _randAnyPictures = value;
            }
        }
        private Random _randAnyScorecenter = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyScorecenter
        {
            get
            {
                return _randAnyScorecenter;
            }
            set
            {
                _randAnyScorecenter = value;
            }
        }
        private Random _randAnyTVSeries = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyTVSeries
        {
            get
            {
                return _randAnyTVSeries;
            }
            set
            {
                _randAnyTVSeries = value;
            }
        }
        private Random _randAnyTV = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyTV
        {
            get
            {
                return _randAnyTV;
            }
            set
            {
                _randAnyTV = value;
            }
        }
        private Random _randAnyPlugins = null; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Random RandAnyPlugins
        {
            get
            {
                return _randAnyPlugins;
            }
            set
            {
                _randAnyPlugins = value;
            }
        }

        private bool useAnyGames/* = false*/;
        private bool useAnyMusic/* = false*/;
        private bool useAnyMovies/* = false*/;
        private bool useAnyMovingPictures/* = false*/;
        private bool useAnyPictures/* = false*/;
        private bool useAnyScoreCenter/* = false*/;
        private bool useAnyTVSeries/* = false*/;
        private bool useAnyTV/* = false*/;
        private bool useAnyPlugins/* = false*/;        

        private string currAnyGames = null;
        private string currAnyMovies = null;
        private string currAnyMovingPictures = null;
        private string currAnyMusic = null;
        private string currAnyPictures = null;
        private string currAnyScorecenter = null;
        private string currAnyTVSeries = null;
        private string currAnyTV = null;
        private string currAnyPlugins = null;

        public ArrayList ListAnyGames = null;
        public ArrayList ListAnyMovies = null;
        public ArrayList ListAnyMovingPictures = null;
        public ArrayList ListAnyMusic = null;
        public ArrayList ListAnyPictures = null;
        public ArrayList ListAnyScorecenter = null;
        public ArrayList ListAnyTVSeries = null;
        public ArrayList ListAnyTV = null;
        public ArrayList ListAnyPlugins = null;

        public int PrevSelectedGames/* = 0*/;
        public int PrevSelectedMovies/* = 0*/;
        public int PrevSelectedMovingPictures/* = 0*/;
        public int PrevSelectedMusic/* = 0*/;
        public int PrevSelectedPictures/* = 0*/;
        public int PrevSelectedScorecenter/* = 0*/;
        public int PrevSelectedTVSeries/* = 0*/;
        public int PrevSelectedTV/* = 0*/;
        public int PrevSelectedPlugins/* = 0*/;

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
        private class SkinFile
        {
            public string Id;
            public string UseRandomGamesFanart;
            public string UseRandomMoviesFanart;
            public string UseRandomMovingPicturesFanart;
            public string UseRandomMusicFanart;
            public string UseRandomPicturesFanart;
            public string UseRandomScoreCenterFanart;
            public string UseRandomTVSeriesFanart;
            public string UseRandomTVFanart;
            public string UseRandomPluginsFanart;
        }

        /// <summary>
        /// Get and set properties for random images
        /// </summary>
        public void RefreshRandomImageProperties(RefreshWorker rw)
        {
            try
            {
                if ((CurrCountRandom >= FanartHandlerSetup.MaxCountImage) || FirstRandom || CurrCountRandom == 0)
                {
                    string sFilename = String.Empty;                    
                    if (SupportsRandomImages("useRandomMoviesFanart").Equals("True", StringComparison.CurrentCulture))
                    {                        
                        sFilename = GetRandomFilename(ref currAnyMovies, "Movie", ref PrevSelectedMovies);                        
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {                     
                                AddPropertyRandom("#fanarthandler.movie.backdrop1.any", sFilename, ref ListAnyMovies);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.movie.backdrop2.any", sFilename, ref ListAnyMovies);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movie.backdrop2.any", sFilename, ref ListAnyMovies);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movie.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.movie.backdrop1.any", sFilename, ref ListAnyMovies);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", tmpImage);
                            PrevSelectedMovies = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyMovies);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", tmpImage);
                        PrevSelectedMovies = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomMovingPicturesFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyMovingPictures, "MovingPicture", ref PrevSelectedMovingPictures);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.movingpicture.backdrop2.any", sFilename, ref ListAnyMovingPictures);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.movingpicture.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.movingpicture.backdrop1.any", sFilename, ref ListAnyMovingPictures);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", tmpImage);
                            PrevSelectedMovingPictures = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }                        
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyMovingPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", tmpImage);
                        PrevSelectedMovingPictures = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomMusicFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyMusic, "MusicFanart", ref PrevSelectedMusic);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop1.any", sFilename, ref ListAnyMusic);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.music.backdrop2.any", sFilename, ref ListAnyMusic);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.music.backdrop2.any", sFilename, ref ListAnyMusic);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.music.backdrop1.any", sFilename, ref ListAnyMusic);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", tmpImage);
                            PrevSelectedMusic = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyMusic);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", tmpImage);
                        PrevSelectedMusic = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }                    
                    if (SupportsRandomImages("useRandomTVFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyTV, "TV", ref PrevSelectedTV);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop1.any", sFilename, ref ListAnyTV);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.tv.backdrop2.any", sFilename, ref ListAnyTV);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tv.backdrop2.any", sFilename, ref ListAnyTV);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tv.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.tv.backdrop1.any", sFilename, ref ListAnyTV);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", tmpImage);
                            PrevSelectedTV = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyTV);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", tmpImage);
                        PrevSelectedTV = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomTVSeriesFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyTVSeries, "TVSeries", ref PrevSelectedTVSeries);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.tvseries.backdrop2.any", sFilename, ref ListAnyTVSeries);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.tvseries.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.tvseries.backdrop1.any", sFilename, ref ListAnyTVSeries);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", tmpImage);
                            PrevSelectedTVSeries = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyTVSeries);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", tmpImage);
                        PrevSelectedTVSeries = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomPicturesFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyPictures, "Picture", ref PrevSelectedPictures);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop1.any", sFilename, ref ListAnyPictures);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.picture.backdrop2.any", sFilename, ref ListAnyPictures);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.picture.backdrop2.any", sFilename, ref ListAnyPictures);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.picture.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.picture.backdrop1.any", sFilename, ref ListAnyPictures);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", tmpImage);
                            PrevSelectedPictures = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", tmpImage);
                        PrevSelectedPictures = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomGamesFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyGames, "Game", ref PrevSelectedGames);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop1.any", sFilename, ref ListAnyGames);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.games.backdrop2.any", sFilename, ref ListAnyGames);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.games.backdrop2.any", sFilename, ref ListAnyGames);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.games.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.games.backdrop1.any", sFilename, ref ListAnyGames);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", tmpImage);
                            PrevSelectedGames = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyGames);
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", tmpImage);
                        PrevSelectedGames = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    if (SupportsRandomImages("useRandomScoreCenterFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyScorecenter, "ScoreCenter", ref PrevSelectedScorecenter);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop1.any", sFilename, ref ListAnyScorecenter);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref ListAnyScorecenter);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.scorecenter.backdrop2.any", sFilename, ref ListAnyScorecenter);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.scorecenter.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.scorecenter.backdrop1.any", sFilename, ref ListAnyScorecenter);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", tmpImage);
                            PrevSelectedScorecenter = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyScorecenter);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", tmpImage);
                        PrevSelectedScorecenter = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }                    
                    if (SupportsRandomImages("useRandomPluginsFanart").Equals("True", StringComparison.CurrentCulture))
                    {
                        sFilename = GetRandomFilename(ref currAnyPlugins, "Plugin", ref PrevSelectedPlugins);
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (DoShowImageOneRandom)
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", sFilename, ref ListAnyPlugins);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.backdrop2.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", sFilename, ref ListAnyPlugins);
                                }
                            }
                            else
                            {
                                AddPropertyRandom("#fanarthandler.plugins.backdrop2.any", sFilename, ref ListAnyPlugins);
                                string sTag = GUIPropertyManager.GetProperty("#fanarthandler.plugins.backdrop1.any");
                                if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png", StringComparison.CurrentCulture))
                                {
                                    AddPropertyRandom("#fanarthandler.plugins.backdrop1.any", sFilename, ref ListAnyPlugins);
                                }
                            }
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                        else
                        {
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", tmpImage);
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", tmpImage);
                            PrevSelectedPlugins = -1;
                            if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                            {
                                rw.ReportProgress(100, "Updated Properties");
                            }
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref ListAnyPlugins);
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", tmpImage);
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", tmpImage);
                        PrevSelectedPlugins = -1;
                        if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                        {
                            rw.ReportProgress(100, "Updated Properties");
                        }
                    }
                    ResetCurrCountRandom();
                    FirstRandom = false;
                    if (rw != null && FanartHandlerSetup.FR.WindowOpen)
                    {
                        rw.ReportProgress(100, "Updated Properties");
                    }
                }
                IncreaseCurrCountRandom();
                if (rw != null)
                {
                    rw.ReportProgress(100, "Updated Properties");
                }
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
            SkinFile sf = (SkinFile)WindowsUsingFanartRandom[GUIWindowManager.ActiveWindow.ToString(CultureInfo.CurrentCulture)];
            if (sf != null)
            {
                if (type.Equals("useRandomGamesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomGamesFanart;
                else if (type.Equals("useRandomMoviesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMoviesFanart;
                else if (type.Equals("useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMovingPicturesFanart;
                else if (type.Equals("useRandomMusicFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomMusicFanart;
                else if (type.Equals("useRandomPicturesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomPicturesFanart;
                else if (type.Equals("useRandomScoreCenterFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomScoreCenterFanart;
                else if (type.Equals("useRandomTVSeriesFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomTVSeriesFanart;
                else if (type.Equals("useRandomTVFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomTVFanart;
                else if (type.Equals("useRandomPluginsFanart", StringComparison.CurrentCulture))
                    return sf.UseRandomPluginsFanart;
            }
            return "False";
        }

        /// <summary>
        /// Add image properties that later will update the skin properties
        /// </summary>
        private void AddPropertyRandom(string property, string value, ref ArrayList al)
        {
            try
            {
                if (value == null)
                    value = "";//20101008
                //if (String.IsNullOrEmpty(value))//20101008
                //    value = " ";

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
                            Utils.LoadImage(value);
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
        public string GetRandomFilename(ref string prevImage, string type, ref int iFilePrev)
        {
            string sout = String.Empty;
            int restricted = 0;
            if (type.Equals("Movie", StringComparison.CurrentCulture) || type.Equals("MovingPicture", StringComparison.CurrentCulture) || type.Equals("myVideos", StringComparison.CurrentCulture) || type.Equals("Online Videos", StringComparison.CurrentCulture) || type.Equals("TV Section", StringComparison.CurrentCulture))
            {
                try
                {
                    restricted = UtilsExternal.MovingPictureIsRestricted();
                }
                catch { }
            }
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    sout = prevImage;
                    string types = String.Empty;
                    if (type.Equals("MusicFanart", StringComparison.CurrentCulture))
                    {
                        if (FanartHandlerSetup.UseAlbum.Equals("True", StringComparison.CurrentCulture) && FanartHandlerSetup.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicAlbum'";
                            else
                                types = "'MusicAlbum'";
                        }
                        if (FanartHandlerSetup.UseArtist.Equals("True", StringComparison.CurrentCulture) && FanartHandlerSetup.DisableMPTumbsForRandom.Equals("False", StringComparison.CurrentCulture))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicArtist'";
                            else
                                types = "'MusicArtist'";
                        }
                        if (FanartHandlerSetup.UseFanart.Equals("True", StringComparison.CurrentCulture))
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

                    htAny = Utils.GetDbm().GetAnyFanart(type, types, restricted);
                    if (htAny != null && htAny.Count > 0)
                    {
                        ICollection valueColl = htAny.Values;
                        int iFile = 0;
                        int iStop = 0;
                        foreach (FanartHandler.FanartImage s in valueColl)
                        {
                            if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                            {
                                if (FanartHandlerSetup.CheckImageResolution(s.DiskImage, type, FanartHandlerSetup.UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                {
                                    sout = s.DiskImage;
                                    iFilePrev = iFile;
                                    prevImage = s.DiskImage;                                    
                                    iStop = 1;
                                    if (CountSetVisibility == 0)
                                    {
                                        CountSetVisibility = 1;
                                    }
                                    break;
                                }
                            }
                            iFile++;
                        }
                        valueColl = null;
                        if (iStop == 0)
                        {
                            valueColl = htAny.Values;
                            iFilePrev = -1;
                            iFile = 0;
                            iStop = 0;
                            foreach (FanartHandler.FanartImage s in valueColl)
                            {
                                if (((iFile > iFilePrev) || (iFilePrev == -1)) && (iStop == 0))
                                {
                                    if (FanartHandlerSetup.CheckImageResolution(s.DiskImage, type, FanartHandlerSetup.UseAspectRatio) && Utils.IsFileValid(s.DiskImage))
                                    {
                                        sout = s.DiskImage;
                                        iFilePrev = iFile;
                                        prevImage = s.DiskImage;                                        
                                        iStop = 1;
                                        if (CountSetVisibility == 0)
                                        {
                                            CountSetVisibility = 1;
                                        }
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
                logger.Error("GetRandomFilename: " + ex.ToString());
            }
            return sout;
        }

        /// <summary>
        /// Reset interval counter and trigger update of skinproperties
        /// </summary>
        public void ResetCurrCountRandom()
        {
            CurrCountRandom = 0;
            UpdateVisibilityCountRandom = 1;
        }

        /// <summary>
        /// Increase the interval counter
        /// </summary>
        private void IncreaseCurrCountRandom()
        {
            CurrCountRandom = CurrCountRandom + 1;
        }

        /// <summary>
        /// Get total properties
        /// </summary>
        public int GetPropertiesRandom()
        {
            if (PropertiesRandom == null)
            {
                return 0;
            }


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
                if (ht != null)
                {
                    ht.Clear();
                }
                ht = null;
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
                        sf.Id = windowId;
                        myXPathNodeIterator = myXPathNavigator.Select("/window/define");
                        if (myXPathNodeIterator.Count > 0)
                        {
                            while (myXPathNodeIterator.MoveNext())
                            {
                                sNodeValue = myXPathNodeIterator.Current.Value;
                                if (sNodeValue.StartsWith("#useRandomGamesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomGamesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMoviesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMoviesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMovingPicturesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMovingPicturesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMusicFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomMusicFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPicturesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomPicturesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomScoreCenterFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomScoreCenterFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVSeriesFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomTVSeriesFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomTVFanart = ParseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPluginsFanart", StringComparison.CurrentCulture))
                                    sf.UseRandomPluginsFanart = ParseNodeValue(sNodeValue);
                            }
                            if (sf.UseRandomGamesFanart != null && sf.UseRandomGamesFanart.Length > 0)
                            {
                                if (sf.UseRandomGamesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyGames = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomGamesFanart = "False";
                            }
                            if (sf.UseRandomMoviesFanart != null && sf.UseRandomMoviesFanart.Length > 0)
                            {
                                if (sf.UseRandomMoviesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMovies = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMoviesFanart = "False";
                            }
                            if (sf.UseRandomMovingPicturesFanart != null && sf.UseRandomMovingPicturesFanart.Length > 0)
                            {
                                if (sf.UseRandomMovingPicturesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMovingPictures = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMovingPicturesFanart = "False";
                            }
                            if (sf.UseRandomMusicFanart != null && sf.UseRandomMusicFanart.Length > 0)
                            {
                                if (sf.UseRandomMusicFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyMusic = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomMusicFanart = "False";
                            }
                            if (sf.UseRandomPicturesFanart != null && sf.UseRandomPicturesFanart.Length > 0)
                            {
                                if (sf.UseRandomPicturesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyPictures = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomPicturesFanart = "False";
                            }
                            if (sf.UseRandomScoreCenterFanart != null && sf.UseRandomScoreCenterFanart.Length > 0)
                            {
                                if (sf.UseRandomScoreCenterFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyScoreCenter = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomScoreCenterFanart = "False";
                            }
                            if (sf.UseRandomTVSeriesFanart != null && sf.UseRandomTVSeriesFanart.Length > 0)
                            {
                                if (sf.UseRandomTVSeriesFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyTVSeries = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomTVSeriesFanart = "False";
                            }
                            if (sf.UseRandomTVFanart != null && sf.UseRandomTVFanart.Length > 0)
                            {
                                if (sf.UseRandomTVFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyTV = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomTVFanart = "False";
                            }
                            if (sf.UseRandomPluginsFanart != null && sf.UseRandomPluginsFanart.Length > 0)
                            {
                                if (sf.UseRandomPluginsFanart.Equals("True", StringComparison.CurrentCulture))
                                {
                                    UseAnyPlugins = true;
                                }
                            }
                            else
                            {
                                sf.UseRandomPluginsFanart = "False";
                            }
                        }
                        try
                        {
                            if (sf.UseRandomGamesFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMoviesFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomMovingPicturesFanart.Equals("False", StringComparison.CurrentCulture)
                                && sf.UseRandomMusicFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomPicturesFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomScoreCenterFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomTVSeriesFanart.Equals("False", StringComparison.CurrentCulture)
                                 && sf.UseRandomTVFanart.Equals("False", StringComparison.CurrentCulture) && sf.UseRandomPluginsFanart.Equals("False", StringComparison.CurrentCulture))
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
                if (s.Substring(s.IndexOf(":", StringComparison.CurrentCulture) + 1).Equals("Yes", StringComparison.CurrentCulture))
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
