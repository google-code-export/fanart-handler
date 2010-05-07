//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.GUI.Library;
    using NLog;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;  

    /// <summary>
    /// Class handling fanart for now playing music.
    /// </summary>
    public class FanartPlaying
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Hashtable propertiesPlay; //used to hold properties to be updated (Play)        
        private int currCountPlay = 0;
        private int updateVisibilityCountPlay = 0;
        private string currPlayMusicArtist = null;        
        public int prevPlayMusic = 0;
        public string currPlayMusic = null;
        public ArrayList listPlayMusic = null;
        private bool fanartAvailablePlay = false;  //Holds if fanart is available (found) or not, controls visibility tag        
        private bool doShowImageOnePlay = true; // Decides if property .1 or .2 should be set on next run        
        private bool hasUpdatedCurrCountPlay = false; // CurrCountPlay have allready been updated this run        
        private Hashtable windowsUsingFanartPlay;  //used to know what skin files that supports play fanart        
        private Hashtable currentArtistsImageNames = null;        
        #endregion

        public Hashtable CurrentArtistsImageNames
        {
            get { return currentArtistsImageNames; }
            set { currentArtistsImageNames = value; }
        }

        public Hashtable WindowsUsingFanartPlay
        {
            get { return windowsUsingFanartPlay; }
            set { windowsUsingFanartPlay = value; }
        }

        public bool HasUpdatedCurrCountPlay
        {
            get { return hasUpdatedCurrCountPlay; }
            set { hasUpdatedCurrCountPlay = value; }
        }

        public bool DoShowImageOnePlay
        {
            get { return doShowImageOnePlay; }
            set { doShowImageOnePlay = value; }
        }

        public bool FanartAvailablePlay
        {
            get { return fanartAvailablePlay; }
            set { fanartAvailablePlay = value; }
        }

        public string CurrPlayMusicArtist
        {
            get { return currPlayMusicArtist; }
            set { currPlayMusicArtist = value; }
        }

        public int UpdateVisibilityCountPlay
        {
            get { return updateVisibilityCountPlay; }
            set { updateVisibilityCountPlay = value; }
        }

        public int CurrCountPlay
        {
            get { return currCountPlay; }
            set { currCountPlay = value; }
        }

        public Hashtable PropertiesPlay
        {
            get { return propertiesPlay; }
            set { propertiesPlay = value; }
        }

        public FanartPlaying()
        {
            currentArtistsImageNames = new Hashtable();
        }

        /// <summary>
        /// Get Hashtable with all filenames for current artist
        /// </summary>
        public Hashtable GetCurrentArtistsImageNames()
        {
            return CurrentArtistsImageNames;
        }

        /// <summary>
        /// Set Hashtable with all filenames for current artist
        /// </summary>
        public void SetCurrentArtistsImageNames(Hashtable ht)
        {
            CurrentArtistsImageNames = ht;
        }

        /// <summary>
        /// Add properties for now playing music
        /// </summary>
        private void AddPropertyPlay(string property, string value, ref ArrayList al, string type)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";
                if (PropertiesPlay.Contains(property))
                {
                    PropertiesPlay[property] = value;
                }
                else
                {
                    PropertiesPlay.Add(property, value);
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
                                logger.Error("AddPropertyPlay: " + ex.ToString());
                            }
                            Utils.LoadImage(value, type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddPropertyPlay: " + ex.ToString());
            }
        }


        /// <summary>
        /// Get and set properties for now playing music
        /// </summary>
        public void RefreshMusicPlayingProperties()
        {
            try
            {
                if (CurrPlayMusicArtist.Equals(FanartHandlerSetup.CurrentTrackTag) == false)
                {
                    currPlayMusic = String.Empty;
                    prevPlayMusic = -1;
                    UpdateVisibilityCountPlay = 0;
                    SetCurrentArtistsImageNames(null);
                    string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart", "FanartPlaying", true, true);
                    if (sFilename.Length == 0)
                    {
                        sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                        if (sFilename.Length == 0)
                        {
                            FanartAvailablePlay = false;
                        }
                        else
                        {
                            FanartAvailablePlay = true;
                            currPlayMusic = sFilename;
                        }
                    }
                    else
                    {
                        FanartAvailablePlay = true;
                    }                    
                    if (DoShowImageOnePlay)
                    {                        
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic, "MusicFanart");
                        string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop2.play");
                        if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png"))
                        {
                            AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic, "MusicFanart");
                        }
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic, "MusicFanart");
                        string sTag = GUIPropertyManager.GetProperty("#fanarthandler.music.backdrop1.play");
                        if (sTag == null || sTag.Length < 2 || sTag.EndsWith("transparent.png"))
                        {
                            AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic, "MusicFanart");
                        }
                    }
                    if (FanartHandlerSetup.UseOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    ResetCurrCountPlay();
                }
                else if (CurrCountPlay >= FanartHandlerSetup.MaxCountImage)
                {
                    string sFilenamePrev = currPlayMusic;
                    string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart", "FanartPlaying", false, true);
                    if (sFilename.Length == 0)
                    {
                        sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                        if (sFilename.Length == 0)
                        {
                            FanartAvailablePlay = false;
                        }
                        else
                        {
                            FanartAvailablePlay = true;
                            currPlayMusic = sFilename;
                        }
                    }
                    else
                    {
                        FanartAvailablePlay = true;
                    }
                    if (DoShowImageOnePlay)
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    if (FanartHandlerSetup.UseOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                    {
                        ResetCurrCountPlay();
                    }
                }
                CurrPlayMusicArtist = FanartHandlerSetup.CurrentTrackTag;
                IncreaseCurrCountPlay();
            }
            catch (Exception ex)
            {
                logger.Error("RefreshMusicPlayingProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Reset couners and trigger new image change
        /// </summary>
        public void ResetCurrCountPlay()
        {
            CurrCountPlay = 0;
            UpdateVisibilityCountPlay = 1;
            HasUpdatedCurrCountPlay = true;
        }

        /// <summary>
        /// Increase image change interval counter
        /// </summary>
        private void IncreaseCurrCountPlay()
        {
            if (HasUpdatedCurrCountPlay == false)
            {
                CurrCountPlay = CurrCountPlay + 1;
                HasUpdatedCurrCountPlay = true;
            }
        }

        /// <summary>
        /// Update image skin properties
        /// </summary>
        public void UpdatePropertiesPlay()
        {
            try
            {
                foreach (DictionaryEntry de in PropertiesPlay)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                PropertiesPlay.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesPlay: " + ex.ToString());
            }
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageOnePlay(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919295);
            GUIControl.HideControl(windowId, 91919296);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageTwoPlay(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919296);
            GUIControl.HideControl(windowId, 91919295);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsAvailablePlay(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919294);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsNotAvailablePlay(int windowId)
        {
            GUIControl.HideControl(windowId, 91919294);
        }


    }
}
