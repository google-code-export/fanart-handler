using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using MediaPortal.GUI.Library;
using System.Collections;
using MediaPortal.Music.Database;
using MediaPortal.Player;
using MediaPortal.Services;
using MediaPortal.TagReader;
using MediaPortal.Configuration;

namespace FanartHandler
{
    class FanartSelected
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public Hashtable windowsUsingFanartSelected; //used to know what skin files that supports selected fanart
        public bool doShowImageOne = true;  // Decides if property .1 or .2 should be set on next run                
        public bool hasUpdatedCurrCount = false; // CurrCount have allready been updated this run                
        public bool fanartAvailable = false;  //Holds if fanart is available (found) or not, controls visibility tag        
        public int updateVisibilityCount = 0;
        public string currSelectedMovieTitle = null;
        public string currSelectedMusicArtist = null;
        public string currSelectedScorecenterGenre = null;
        private string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";
        public int prevSelectedGeneric = 0;
        public int prevSelectedMusic = 0;
        public int prevSelectedScorecenter = 0;
        public string currSelectedMovie = null;
        public string currSelectedMusic = null;
        public string currSelectedScorecenter = null;
        public ArrayList listSelectedMovies = null;
        public ArrayList listSelectedMusic = null;
        public ArrayList listSelectedScorecenter = null;
        public int currCount = 0;
        public Hashtable properties; //used to hold properties to be updated (Selected or Any)                            
        public Hashtable currentArtistsImageNames = null;
        #endregion

        public FanartSelected()
        {
            currentArtistsImageNames = new Hashtable();
        }

        /// <summary>
        /// Get Hashtable with all filenames for current artist
        /// </summary>
        public Hashtable GetCurrentArtistsImageNames()
        {
            return currentArtistsImageNames;
        }

        /// <summary>
        /// Set Hashtable with all filenames for current artist
        /// </summary>
        public void SetCurrentArtistsImageNames(Hashtable ht)
        {
            currentArtistsImageNames = ht;
        }

        /// <summary>
        /// Get and set properties for selected video title
        /// </summary>
        public void RefreshGenericSelectedProperties(string property, ref ArrayList listSelectedGeneric, string type, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
        {
            try
            {
                if (GUIWindowManager.ActiveWindow == 6623)
                {
                    FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#mvids.artist");
                }
                else if (GUIWindowManager.ActiveWindow == 880)
                {
                    FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#MusicVids.ArtistName");
                }
                else if (GUIWindowManager.ActiveWindow == 510)
                {
                    FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#artist");
                }
                else if (GUIWindowManager.ActiveWindow == 35)
                {
                    FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#Play.Current.Title");
                }
                else
                {
                    FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
                }
                if (FanartHandlerSetup.m_SelectedItem != null && FanartHandlerSetup.m_SelectedItem.Length > 0)
                {
                    if ((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || ((GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && FanartHandlerSetup.m_SelectedItem.Equals("..") == true))
                    {
                        //online videos or myvideo, do not update if in details view
                    }
                    else
                    {
                        if (type.Equals("Global Search") || type.Equals("mVids") || type.Equals("Youtube.FM") || type.Equals("Music Playlist"))
                        {
                            FanartHandlerSetup.m_SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.m_SelectedItem);
                        }
                        if (currSelectedGenericTitle.Equals(FanartHandlerSetup.m_SelectedItem) == false)
                        {
                            currSelectedGeneric = "";
                            prevSelectedGeneric = -1;
                            SetCurrentArtistsImageNames(null);
                            updateVisibilityCount = 0;
                            string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedGeneric, ref prevSelectedGeneric, type, "FanartSelected", true);
                            if (sFilename.Length == 0)
                            {
                                if (property.Equals("music"))
                                {
                                    sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                                    if (sFilename.Length == 0)
                                    {
                                        fanartAvailable = false;
                                    }
                                    else
                                    {
                                        fanartAvailable = true;
                                        currSelectedGeneric = sFilename;
                                    }
                                }
                                else
                                {
                                    fanartAvailable = false;
                                }
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric, type);
                            }
                            else
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric, type);
                            }
                            currSelectedGenericTitle = FanartHandlerSetup.m_SelectedItem;
                            ResetCurrCount();
                        }
                        else if (currCount >= FanartHandlerSetup.maxCountImage)
                        {
                            string sFilenamePrev = currSelectedGeneric;
                            string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedGeneric, ref prevSelectedGeneric, type, "FanartSelected", false);
                            if (sFilename.Length == 0)
                            {
                                if (property.Equals("music"))
                                {
                                    sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                                    if (sFilename.Length == 0)
                                    {
                                        fanartAvailable = false;
                                    }
                                    else
                                    {
                                        fanartAvailable = true;
                                        currSelectedGeneric = sFilename;
                                    }
                                }
                                else
                                {
                                    fanartAvailable = false;
                                }
                            }
                            else
                            {
                                fanartAvailable = true;
                            }
                            if (doShowImageOne)
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric, type);
                            }
                            else
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric, type);
                            }
                            currSelectedGenericTitle = FanartHandlerSetup.m_SelectedItem;
                            if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                            {
                                ResetCurrCount();
                            }
                        }
                        IncreaseCurrCount();
                    }
                }
                else if ((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || ((GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && FanartHandlerSetup.m_SelectedItem.Equals("..") == true))
                {
                    //online videos or myvideo, do not update if in details view
                }
                else
                {
                    currSelectedGeneric = "";
                    prevSelectedGeneric = -1;
                    fanartAvailable = false;
                    if (doShowImageOne)
                        AddProperty("#fanarthandler." + property + ".backdrop1.selected", tmpImage, ref listSelectedMusic, type);
                    else
                        AddProperty("#fanarthandler." + property + ".backdrop2.selected", tmpImage, ref listSelectedMusic, type);
                    ResetCurrCount();
                    currSelectedGenericTitle = "";
                    SetCurrentArtistsImageNames(null);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshGenericSelectedProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Increase interval counter 
        /// </summary>
        private void IncreaseCurrCount()
        {
            if (hasUpdatedCurrCount == false)
            {
                currCount = currCount + 1;
                hasUpdatedCurrCount = true;
            }
        }

        /// <summary>
        /// Reset count and trigger change of image (.1 and .2)
        /// </summary>
        public void ResetCurrCount()
        {
            currCount = 0;
            updateVisibilityCount = 1;
            hasUpdatedCurrCount = true;
        }


        /// <summary>
        /// Get and set properties for selected scorecenter title
        /// </summary>
        public void RefreshScorecenterSelectedProperties()
        {
            try
            {
                FanartHandlerSetup.m_SelectedItem = GUIPropertyManager.GetProperty("#ScoreCenter.Category");
                if (FanartHandlerSetup.m_SelectedItem != null && FanartHandlerSetup.m_SelectedItem.Equals("..") == false && FanartHandlerSetup.m_SelectedItem.Length > 0)
                {
                    if (currSelectedScorecenterGenre.Equals(FanartHandlerSetup.m_SelectedItem) == false)
                    {
                        currSelectedScorecenter = "";
                        prevSelectedScorecenter = -1;
                        updateVisibilityCount = 0;
                        SetCurrentArtistsImageNames(null);
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedScorecenter, ref prevSelectedScorecenter, "ScoreCenter", "FanartSelected", true);
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                            currSelectedScorecenter = sFilename;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref listSelectedScorecenter, "ScoreCenter");
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref listSelectedScorecenter, "ScoreCenter");
                        }
                        currSelectedScorecenterGenre = FanartHandlerSetup.m_SelectedItem;
                        ResetCurrCount();
                    }
                    else if (currCount >= FanartHandlerSetup.maxCountImage)
                    {
                        string sFilenamePrev = currSelectedScorecenter;
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedScorecenter, ref prevSelectedScorecenter, "ScoreCenter", "FanartSelected", false);
                        if (sFilename.Length == 0)
                        {
                            fanartAvailable = false;
                        }
                        else
                        {
                            fanartAvailable = true;
                            currSelectedScorecenter = sFilename;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref listSelectedScorecenter, "ScoreCenter");
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref listSelectedScorecenter, "ScoreCenter");
                        }
                        currSelectedScorecenterGenre = FanartHandlerSetup.m_SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else
                {
                    currSelectedScorecenter = "";
                    currSelectedScorecenterGenre = "";
                    prevSelectedScorecenter = -1;
                    SetCurrentArtistsImageNames(null);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshScorecenterSelectedProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Try to get artist. 
        /// </summary>
        private string getMusicArtistFromListControl()
        {
            try
            {
                GUIListItem gli = GUIControl.GetSelectedListItem(GUIWindowManager.ActiveWindow, 50);

                if (gli != null)
                {
                    if (gli.MusicTag == null && gli.Label.Equals(".."))
                    {
                        //on .. entry in listcontrol?                        
                        return "..";
                    }
                    else if (gli.MusicTag == null)
                    {
                        string sArtistName = null;
                        string dbArtistName = null;
                        string currSelectedInList = GUIPropertyManager.GetProperty("#selecteditem");
                        currSelectedInList = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(currSelectedInList)));
                        ArrayList al = new ArrayList();
                        FanartHandlerSetup.m_db.GetAllArtists(ref al);
                        for (int i = 0; i < al.Count; i++)
                        {
                            dbArtistName = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(al[i].ToString()));
                            if (currSelectedInList.IndexOf(dbArtistName) >= 0)
                            {
                                sArtistName = dbArtistName;
                                break;
                            }
                        }
                        al = null;
                        currSelectedInList = Utils.GetArtistLeftOfMinusSign(GUIPropertyManager.GetProperty("#selecteditem"));
                        if (sArtistName == null)
                        {
                            al = new ArrayList();
                            if (FanartHandlerSetup.m_db.GetAlbums(3, currSelectedInList, ref al))
                            {
                                AlbumInfo ai = (AlbumInfo)al[0];
                                if (ai != null)
                                {
                                    if (ai.Artist != null && ai.Artist.Length > 0)
                                    {
                                        sArtistName = ai.Artist;
                                    }
                                    else
                                    {
                                        sArtistName = ai.AlbumArtist;
                                    }
                                }
                            }
                        }
                        currSelectedInList = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(currSelectedInList)));
                        if (sArtistName == null)
                        {
                            al = new ArrayList();
                            if (FanartHandlerSetup.m_db.GetAlbums(3, currSelectedInList, ref al))
                            {
                                AlbumInfo ai = (AlbumInfo)al[0];
                                if (ai != null)
                                {
                                    if (ai.Artist != null && ai.Artist.Length > 0)
                                    {
                                        sArtistName = ai.Artist;
                                    }
                                    else
                                    {
                                        sArtistName = ai.AlbumArtist;
                                    }
                                }
                            }
                        }
                        al = null;
                        return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(sArtistName));
                    }
                    else
                    {
                        //on artist, album or track entry
                        MusicTag tag1 = (MusicTag)gli.MusicTag;
                        if (tag1 != null)
                        {
                            if (tag1.Artist != null && tag1.Artist.Length > 0)
                            {
                                return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(tag1.Artist));
                            }
                            else
                            {
                                return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(tag1.AlbumArtist));
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Error("getMusicArtistFromListControl: " + ex.ToString());
            }
            return null;
        }


        /// <summary>
        /// Get and set properties for selected artis, album or track in myMusicGenres window
        /// </summary>
        public void RefreshMusicSelectedProperties()
        {
            try
            {
                FanartHandlerSetup.m_SelectedItem = getMusicArtistFromListControl();
                if (FanartHandlerSetup.m_SelectedItem != null && FanartHandlerSetup.m_SelectedItem.Equals("..") == false && FanartHandlerSetup.m_SelectedItem.Length > 0)
                {

                    if (currSelectedMusicArtist.Equals(FanartHandlerSetup.m_SelectedItem) == false)
                    {
                        currSelectedMusic = "";
                        prevSelectedMusic = -1;
                        updateVisibilityCount = 0;
                        SetCurrentArtistsImageNames(null);
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart", "FanartSelected", true);
                        if (sFilename.Length == 0)
                        {
                            sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                                currSelectedMusic = sFilename;
                            }
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic, "MusicFanart");
                        }
                        else
                        {
                            AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic, "MusicFanart");
                        }
                        currSelectedMusicArtist = FanartHandlerSetup.m_SelectedItem;
                        ResetCurrCount();
                    }
                    else if (currCount >= FanartHandlerSetup.maxCountImage)
                    {
                        string sFilenamePrev = currSelectedMusic;
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.m_SelectedItem, ref currSelectedMusic, ref prevSelectedMusic, "MusicFanart", "FanartSelected", false);
                        if (sFilename.Length == 0)
                        {
                            sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop();
                            if (sFilename.Length == 0)
                            {
                                fanartAvailable = false;
                            }
                            else
                            {
                                fanartAvailable = true;
                                currSelectedMusic = sFilename;
                            }
                        }
                        else
                        {
                            fanartAvailable = true;
                        }
                        if (doShowImageOne)
                        {
                            AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref listSelectedMusic, "MusicFanart");
                        }
                        else
                        {
                            AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref listSelectedMusic, "MusicFanart");
                        }
                        currSelectedMusicArtist = FanartHandlerSetup.m_SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else if (FanartHandlerSetup.m_SelectedItem != null && FanartHandlerSetup.m_SelectedItem.Equals(".."))
                {
                    currSelectedMusic = "";
                    currSelectedMusicArtist = "";
                }
                else
                {
                    currSelectedMusic = "";
                    prevSelectedMusic = -1;
                    fanartAvailable = false;
                    if (doShowImageOne)
                        AddProperty("#fanarthandler.music.backdrop1.selected", tmpImage, ref listSelectedMusic, "MusicFanart");
                    else
                        AddProperty("#fanarthandler.music.backdrop2.selected", tmpImage, ref listSelectedMusic, "MusicFanart");
                    ResetCurrCount();
                    currSelectedMusicArtist = "";
                    SetCurrentArtistsImageNames(null);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RefreshMusicSelectedProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Update skin properties with new images
        /// </summary>
        public void UpdateProperties()
        {
            try
            {
                foreach (DictionaryEntry de in properties)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                properties.Clear();
            }
            catch (Exception ex)
            {
                logger.Error("UpdateProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Add new images that later will update skin properties
        /// </summary>
        private void AddProperty(string property, string value, ref ArrayList al, string type)
        {
            try
            {
                if (value == null)
                    value = " ";
                if (String.IsNullOrEmpty(value))
                    value = " ";
                if (properties.Contains(property))
                {
                    properties[property] = value;
                }
                else
                {
                    properties.Add(property, value);
                }

                //load images as MP resource
                if (value != null && value.Length > 0)
                {
                    //add new filename to list
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
                                logger.Error("AddProperty: " + ex.ToString());
                            }
                            Utils.LoadImage(value, type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("AddProperty: " + ex.ToString());
            }
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsAvailable(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919293);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for deciding if fanart is available
        /// </summary>
        public void FanartIsNotAvailable(int windowId)
        {
            GUIControl.HideControl(windowId, 91919293);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageOne(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919291);
            GUIControl.HideControl(windowId, 91919292);
        }

        /// <summary>
        /// Set visibility on dummy controls that is used in skins for fading of images
        /// </summary>
        public void ShowImageTwo(int windowId)
        {
            GUIControl.ShowControl(windowId, 91919292);
            GUIControl.HideControl(windowId, 91919291);
        }
    }
}
