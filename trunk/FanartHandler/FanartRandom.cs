using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using MediaPortal.GUI.Library;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using MediaPortal.Configuration;
using System.IO;

namespace FanartHandler
{
    class FanartRandom
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public int currCountRandom = 0;
        public int updateVisibilityCountRandom = 0;
        public int countSetVisibility = 0;
        public bool firstRandom = true;  //Special case on first random
        public bool windowOpen = false;
        public bool doShowImageOneRandom = true; // Decides if property .1 or .2 should be set on next run
        public Hashtable windowsUsingFanartRandom; //used to know what skin files that supports random fanart
        public Hashtable propertiesRandom; //used to hold properties to be updated (Random)

        public Random randAnyGames = null;
        public Random randAnyMovies = null;
        public Random randAnyMovingPictures = null;
        public Random randAnyMusic = null;
        public Random randAnyPictures = null;
        public Random randAnyScorecenter = null;
        public Random randAnyTVSeries = null;
        public Random randAnyTV = null;
        public Random randAnyPlugins = null;

        public bool useAnyGames = false;
        public bool useAnyMusic = false;
        public bool useAnyMovies = false;
        public bool useAnyMovingPictures = false;
        public bool useAnyPictures = false;
        public bool useAnyScoreCenter = false;
        public bool useAnyTVSeries = false;
        public bool useAnyTV = false;
        public bool useAnyPlugins = false;

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
                if ((currCountRandom >= FanartHandlerSetup.maxCountImage) || firstRandom || currCountRandom == 0)
                {
                    string sFilename = "";                                        
                    if (supportsRandomImages("useRandomMoviesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMovies, ref randAnyMovies, "Movie");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMovies);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.movie.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomMovingPicturesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMovingPictures, ref randAnyMovingPictures, "MovingPicture");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", "");
                        }                        
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMovingPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.movingpicture.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomMusicFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyMusic, ref randAnyMusic, "MusicFanart");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyMusic);
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.music.backdrop2.any", "");
                    }                    
                    if (supportsRandomImages("useRandomTVFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyTV, ref randAnyTV, "TV");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyTV);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.tv.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomTVSeriesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyTVSeries, ref randAnyTVSeries, "TVSeries");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyTVSeries);
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.tvseries.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomPicturesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyPictures, ref randAnyPictures, "Picture");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyPictures);
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.picture.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomGamesFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyGames, ref randAnyGames, "Game");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyGames);
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.games.backdrop2.any", "");
                    }
                    if (supportsRandomImages("useRandomScoreCenterFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyScorecenter, ref randAnyScorecenter, "ScoreCenter");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyScorecenter);
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.scorecenter.backdrop2.any", "");
                    }                    
                    if (supportsRandomImages("useRandomPluginsFanart").Equals("True"))
                    {
                        sFilename = GetRandomFilename(ref currCountRandom, ref currAnyPlugins, ref randAnyPlugins, "Plugin");
                        if (sFilename != null && sFilename.Length > 0)
                        {
                            if (doShowImageOneRandom)
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
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", "");
                            FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", "");
                        }
                    }
                    else
                    {
                        FanartHandlerSetup.EmptyAllImages(ref listAnyPlugins);
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop1.any", "");
                        FanartHandlerSetup.SetProperty("#fanarthandler.plugins.backdrop2.any", "");
                    }
                    ResetCurrCountRandom();
                    firstRandom = false;
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
        private String supportsRandomImages(string type)
        {
            SkinFile sf = (SkinFile)windowsUsingFanartRandom[GUIWindowManager.ActiveWindow.ToString()];
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
                if (propertiesRandom.Contains(property))
                {
                    propertiesRandom[property] = value;
                }
                else
                {
                    propertiesRandom.Add(property, value);
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
            string sout = "";
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    sout = prevImage;
                    string types = "";
                    if (type.Equals("MusicFanart"))
                    {
                        if (FanartHandlerSetup.useAlbum.Equals("True") && FanartHandlerSetup.disableMPTumbsForRandom.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicAlbum'";
                            else
                                types = "'MusicAlbum'";
                        }
                        if (FanartHandlerSetup.useArtist.Equals("True") && FanartHandlerSetup.disableMPTumbsForRandom.Equals("False"))
                        {
                            if (types.Length > 0)
                                types = types + ",'MusicArtist'";
                            else
                                types = "'MusicArtist'";
                        }
                        if (FanartHandlerSetup.useFanart.Equals("True"))
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
                    Hashtable ht = Utils.GetDbm().getAnyFanart(type, types);
                    if (ht != null && ht.Count > 0)
                    {
                        bool doRun = true;
                        int attempts = 0;
                        while (doRun && attempts < (ht.Count * 2))
                        {
                            int iHt = randNum.Next(0, ht.Count);
                            DatabaseManager.FanartImage imgFile = (DatabaseManager.FanartImage)ht[iHt];
                            sout = imgFile.disk_image;
                            if (FanartHandlerSetup.checkImageResolution(sout, type, FanartHandlerSetup.useAspectRatio) && Utils.IsFileValid(sout))
                            {
                                prevImage = sout;
                                //ResetCurrCountRandom();
                                if (countSetVisibility == 0)
                                {
                                    countSetVisibility = 1;
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
            updateVisibilityCountRandom = 1;
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
            if (propertiesRandom == null) return 0;

            return propertiesRandom.Count;
        }

        /// <summary>
        /// Clear total properties
        /// </summary>
        public void ClearPropertiesRandom()
        {
            if (propertiesRandom != null)
            {
                propertiesRandom.Clear();
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
                foreach (DictionaryEntry de in propertiesRandom)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                    ht.Add(x, de.Key.ToString());
                    x++;
                }
                for (int i = 0; i < ht.Count; i++)
                {
                    propertiesRandom.Remove(ht[i].ToString());
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
        public void setupWindowsUsingRandomImages()
        {
            XPathDocument myXPathDocument;
            windowsUsingFanartRandom = new Hashtable();
            string path = GUIGraphicsContext.Skin + @"\";            
            string windowId = "";
            string sNodeValue = "";
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            string s = "";
            foreach (FileInfo fi in rgFiles)
            {
                try
                {
                    s = fi.Name;
                    myXPathDocument = new XPathDocument(fi.FullName);
                    XPathNavigator myXPathNavigator = myXPathDocument.CreateNavigator();
                    XPathNodeIterator myXPathNodeIterator = myXPathNavigator.Select("/window/id");
                    windowId = getNodeValue(myXPathNodeIterator);
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
                                    sf.useRandomGamesFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMoviesFanart"))
                                    sf.useRandomMoviesFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMovingPicturesFanart"))
                                    sf.useRandomMovingPicturesFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomMusicFanart"))
                                    sf.useRandomMusicFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPicturesFanart"))
                                    sf.useRandomPicturesFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomScoreCenterFanart"))
                                    sf.useRandomScoreCenterFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVSeriesFanart"))
                                    sf.useRandomTVSeriesFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomTVFanart"))
                                    sf.useRandomTVFanart = parseNodeValue(sNodeValue);
                                if (sNodeValue.StartsWith("#useRandomPluginsFanart"))
                                    sf.useRandomPluginsFanart = parseNodeValue(sNodeValue);
                            }
                            if (sf.useRandomGamesFanart != null && sf.useRandomGamesFanart.Length > 0)
                            {
                                if (sf.useRandomGamesFanart.Equals("True"))
                                {
                                    useAnyGames = true;
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
                                    useAnyMovies = true;
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
                                    useAnyMovingPictures = true;
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
                                    useAnyMusic = true;
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
                                    useAnyPictures = true;
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
                                    useAnyScoreCenter = true;
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
                                    useAnyTVSeries = true;
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
                                    useAnyTV = true;
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
                                    useAnyPlugins = true;
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
                                windowsUsingFanartRandom.Add(windowId, sf);
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
        private string getNodeValue(XPathNodeIterator myXPathNodeIterator)
        {
            if (myXPathNodeIterator.Count > 0)
            {
                myXPathNodeIterator.MoveNext();
                return myXPathNodeIterator.Current.Value;
            }
            return "";
        }



        /// <summary>
        /// Parse node value
        /// </summary>
        private string parseNodeValue(string s)
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
