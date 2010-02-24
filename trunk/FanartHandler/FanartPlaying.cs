using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.Collections;
using MediaPortal.GUI.Library;

namespace FanartHandler
{
    class FanartPlaying
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public Hashtable propertiesPlay; //used to hold properties to be updated (Play)
        public int currCountPlay = 0;
        public int updateVisibilityCountPlay = 0;
        public string currPlayMusicArtist = null;
        public int prevPlayMusic = 0;
        public string currPlayMusic = null;
        public ArrayList listPlayMusic = null;
        public bool fanartAvailablePlay = false;  //Holds if fanart is available (found) or not, controls visibility tag
        public bool doShowImageOnePlay = true; // Decides if property .1 or .2 should be set on next run
        public bool hasUpdatedCurrCountPlay = false; // CurrCountPlay have allready been updated this run
        public Hashtable windowsUsingFanartPlay;  //used to know what skin files that supports play fanart
        public Hashtable currentArtistsImageNames = null;
        public Random currentArtistsRandomizer;
        #endregion

        public FanartPlaying()
        {
            currentArtistsImageNames = new Hashtable();
            currentArtistsRandomizer = new Random();
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
                if (propertiesPlay.Contains(property))
                {
                    propertiesPlay[property] = value;
                }
                else
                {
                    propertiesPlay.Add(property, value);
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
                if (currPlayMusicArtist.Equals(FanartHandlerSetup.m_CurrentTrackTag) == false)
                {
                    currPlayMusic = "";
                    prevPlayMusic = -1;
                    string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart", this, true);
                    if (sFilename.Length == 0)
                    {
                        sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                        if (sFilename.Length == 0)
                        {
                            fanartAvailablePlay = false;
                        }
                        else
                        {
                            fanartAvailablePlay = true;
                            currPlayMusic = sFilename;
                        }
                    }
                    else
                    {
                        fanartAvailablePlay = true;
                    }
                    if (doShowImageOnePlay)
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    if (FanartHandlerSetup.useOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    ResetCurrCountPlay();
                }
                else if (currCountPlay >= FanartHandlerSetup.maxCountImage)
                {
                    string sFilenamePrev = currPlayMusic;
                    string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_CurrentTrackTag, ref currPlayMusic, ref prevPlayMusic, "MusicFanart", this, false);
                    if (sFilename.Length == 0)
                    {
                        sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                        if (sFilename.Length == 0)
                        {
                            fanartAvailablePlay = false;
                        }
                        else
                        {
                            fanartAvailablePlay = true;
                            currPlayMusic = sFilename;
                        }
                    }
                    else
                    {
                        fanartAvailablePlay = true;
                    }
                    if (doShowImageOnePlay)
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop1.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    else
                    {
                        AddPropertyPlay("#fanarthandler.music.backdrop2.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    if (FanartHandlerSetup.useOverlayFanart.Equals("True"))
                    {
                        AddPropertyPlay("#fanarthandler.music.overlay.play", sFilename, ref listPlayMusic, "MusicFanart");
                    }
                    if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                    {
                        ResetCurrCountPlay();
                    }
                }
                currPlayMusicArtist = FanartHandlerSetup.m_CurrentTrackTag;
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
            currCountPlay = 0;
            updateVisibilityCountPlay = 1;
            hasUpdatedCurrCountPlay = true;
        }

        /// <summary>
        /// Increase image change interval counter
        /// </summary>
        private void IncreaseCurrCountPlay()
        {
            if (hasUpdatedCurrCountPlay == false)
            {
                currCountPlay = currCountPlay + 1;
                hasUpdatedCurrCountPlay = true;
            }
        }

        /// <summary>
        /// Update image skin properties
        /// </summary>
        public void UpdatePropertiesPlay()
        {
            try
            {
                foreach (DictionaryEntry de in propertiesPlay)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                propertiesPlay.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdatePropertiesPlay: " + ex.ToString());
            }
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageOnePlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919295);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919296);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageTwoPlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919296);
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919295);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsAvailablePlay()
        {
            GUIControl.ShowControl(GUIWindowManager.ActiveWindow, 91919294);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsNotAvailablePlay()
        {
            GUIControl.HideControl(GUIWindowManager.ActiveWindow, 91919294);
        }


    }
}
