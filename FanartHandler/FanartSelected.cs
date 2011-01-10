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

namespace FanartHandler
{
    using MediaPortal.Configuration;
    using MediaPortal.GUI.Library;
    using MediaPortal.Music.Database;
    using MediaPortal.Player;
    using MediaPortal.Services;
    using MediaPortal.TagReader;    
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    
    
    using System.Collections;
    
    /// <summary>
    /// Class handling fanart for selected items in MP.
    /// </summary>
    public class FanartSelected
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Hashtable windowsUsingFanartSelected; //used to know what skin files that supports selected fanart        
        private bool doShowImageOne = true;  // Decides if property .1 or .2 should be set on next run                        
        private bool hasUpdatedCurrCount/* = false*/; // CurrCount have allready been updated this run                        
        private bool fanartAvailable/* = false*/;  //Holds if fanart is available (found) or not, controls visibility tag                
        private int updateVisibilityCount/* = 0*/;        
        public string CurrSelectedMovieTitle = null;
        public string CurrSelectedMusicArtist = null;
        private string currSelectedScorecenterGenre = null;        
        //private string tmpImage = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\transparent.png";
        public int PrevSelectedGeneric/* = 0*/;
        public int PrevSelectedMusic/* = 0*/;
        public int PrevSelectedScorecenter/* = 0*/;
        public string CurrSelectedMovie = null;
        public string CurrSelectedMusic = null;
        public string CurrSelectedScorecenter = null;
        public ArrayList ListSelectedMovies = null;
        public ArrayList ListSelectedMusic = null;
        public ArrayList ListSelectedScorecenter = null;
        private int currCount/* = 0*/;
        private Hashtable properties; //used to hold properties to be updated (Selected or Any)                                    
        private Hashtable currentArtistsImageNames = null;
        private bool foundItem = false;        
        #endregion

        public bool FoundItem
        {
            get { return foundItem; }
            set { foundItem = value; }
        }

        public Hashtable CurrentArtistsImageNames
        {
            get { return currentArtistsImageNames; }
            set { currentArtistsImageNames = value; }
        }

        public Hashtable Properties
        {
            get { return properties; }
            set { properties = value; }
        }

        public int CurrCount
        {
            get { return currCount; }
            set { currCount = value; }
        }

        public string CurrSelectedScorecenterGenre
        {
            get { return currSelectedScorecenterGenre; }
            set { currSelectedScorecenterGenre = value; }
        }

        public int UpdateVisibilityCount
        {
            get { return updateVisibilityCount; }
            set { updateVisibilityCount = value; }
        }

        public bool FanartAvailable
        {
            get { return fanartAvailable; }
            set { fanartAvailable = value; }
        }

        public bool HasUpdatedCurrCount
        {
            get { return hasUpdatedCurrCount; }
            set { hasUpdatedCurrCount = value; }
        }

        public bool DoShowImageOne
        {
            get { return doShowImageOne; }
            set { doShowImageOne = value; }
        }

        public Hashtable WindowsUsingFanartSelected
        {
            get { return windowsUsingFanartSelected; }
            set { windowsUsingFanartSelected = value; }
        }

        public FanartSelected()
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
        /// Get and set properties for selected video title
        /// </summary>
        public void RefreshGenericSelectedProperties(string property, ref ArrayList listSelectedGeneric, string type, ref string currSelectedGeneric, ref string currSelectedGenericTitle)
        {
            try
            {
                bool isMusic = false;
                FoundItem = false;
                if (property.Equals("music", StringComparison.CurrentCulture))
                {
                    isMusic = true;
                }

                if (GUIWindowManager.ActiveWindow == 6623)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#mvids.artist");
                }
                else if (GUIWindowManager.ActiveWindow == 759)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#TV.RecordedTV.Title");
                }
                else if (GUIWindowManager.ActiveWindow == 1)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#TV.View.title");
                }
                else if (GUIWindowManager.ActiveWindow == 600)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#TV.Guide.Title");
                }
                else if (GUIWindowManager.ActiveWindow == 880)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#MusicVids.ArtistName");
                }
                else if (GUIWindowManager.ActiveWindow == 510)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#artist");
                }
                else if (GUIWindowManager.ActiveWindow == 35)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#Play.Current.Title");
                }
                else if (GUIWindowManager.ActiveWindow == 6622)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem2");
                }
                else if (GUIWindowManager.ActiveWindow == 2003)
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#title");
                } 
                else
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");
                }
                if (FanartHandlerSetup.SelectedItem != null && FanartHandlerSetup.SelectedItem.Trim().Length > 0)
                {
                    if ((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || ((GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && FanartHandlerSetup.SelectedItem.Equals("..", StringComparison.CurrentCulture) == true))
                    {
                        //online videos or myvideo, do not update if in details view  
                    }
                    else
                    {
                        if (type.Equals("Global Search", StringComparison.CurrentCulture) || type.Equals("mVids", StringComparison.CurrentCulture) || type.Equals("Youtube.FM", StringComparison.CurrentCulture) || type.Equals("Music Playlist", StringComparison.CurrentCulture) || type.Equals("Music Trivia", StringComparison.CurrentCulture))
                        {
                            FanartHandlerSetup.SelectedItem = Utils.GetArtistLeftOfMinusSign(FanartHandlerSetup.SelectedItem);
                        }
                        if (currSelectedGenericTitle.Equals(FanartHandlerSetup.SelectedItem, StringComparison.CurrentCulture) == false)
                        {
                            string sFilenamePrev = CurrSelectedMusic;
                            currSelectedGeneric = String.Empty;
                            PrevSelectedGeneric = -1;
                            SetCurrentArtistsImageNames(null);
                            UpdateVisibilityCount = 0;
                            string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref currSelectedGeneric, ref PrevSelectedGeneric, type, "FanartSelected", true, isMusic);
                            if (sFilename.Length == 0)
                            {
                                if (property.Equals("music", StringComparison.CurrentCulture))
                                {
                                    sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref PrevSelectedGeneric);
                                    if (sFilename.Length == 0)
                                    {
                                        FanartAvailable = false;
                                    }
                                    else
                                    {
                                        FanartAvailable = true;
                                        FoundItem = true;
                                        currSelectedGeneric = sFilename;
                                    }
                                }
                                else
                                {
                                    FanartAvailable = false;
                                }
                            }
                            else
                            {
                                FoundItem = true;
                                FanartAvailable = true;
                            }
                            if (DoShowImageOne)
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric);
                            }
                            else
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric);
                            }
                            currSelectedGenericTitle = FanartHandlerSetup.SelectedItem;
                            if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                            {
                                ResetCurrCount();
                            }
                            //ResetCurrCount();
                        }
                        else if (CurrCount >= FanartHandlerSetup.MaxCountImage)
                        {                            
                            string sFilenamePrev = currSelectedGeneric;
                            string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref currSelectedGeneric, ref PrevSelectedGeneric, type, "FanartSelected", false, isMusic);
                            if (sFilename.Length == 0)
                            {
                                if (property.Equals("music", StringComparison.CurrentCulture))
                                {
                                    sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop(ref currSelectedGeneric, ref PrevSelectedGeneric);
                                    if (sFilename.Length == 0)
                                    {
                                        FanartAvailable = false;
                                    }
                                    else
                                    {
                                        FanartAvailable = true;
                                        FoundItem = true;
                                        currSelectedGeneric = sFilename;
                                    }
                                }
                                else
                                {
                                    FanartAvailable = false;
                                }
                            }
                            else
                            {
                                FoundItem = true;
                                FanartAvailable = true;
                            }
                            if (DoShowImageOne)
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop1.selected", sFilename, ref listSelectedGeneric);
                            }
                            else
                            {
                                AddProperty("#fanarthandler." + property + ".backdrop2.selected", sFilename, ref listSelectedGeneric);
                            }
                            currSelectedGenericTitle = FanartHandlerSetup.SelectedItem;
                            if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                            {
                                ResetCurrCount();
                            }
                        }
                        IncreaseCurrCount();
                    }
                }
                else if ((GUIWindowManager.ActiveWindow == 4755 && GUIWindowManager.GetWindow(4755).GetControl(51).IsVisible) || ((GUIWindowManager.ActiveWindow == 6 || GUIWindowManager.ActiveWindow == 25) && FanartHandlerSetup.SelectedItem.Equals("..", StringComparison.CurrentCulture) == true))
                {
                    //online videos or myvideo, do not update if in details view
                }
                else
                {
                    currSelectedGeneric = String.Empty;
                    PrevSelectedGeneric = -1;
                    FanartAvailable = false;
                    if (DoShowImageOne)
                    {
                        AddProperty("#fanarthandler." + property + ".backdrop1.selected", string.Empty, ref ListSelectedMusic);
                    }
                    else
                    {
                        AddProperty("#fanarthandler." + property + ".backdrop2.selected", string.Empty, ref ListSelectedMusic);
                    }
                    ResetCurrCount();
                    currSelectedGenericTitle = String.Empty;
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
            if (HasUpdatedCurrCount == false)
            {
                CurrCount = CurrCount + 1;
                HasUpdatedCurrCount = true;
            }
        }

        /// <summary>
        /// Reset count and trigger change of image (.1 and .2)
        /// </summary>
        public void ResetCurrCount()
        {
            CurrCount = 0;
            UpdateVisibilityCount = 1;
            HasUpdatedCurrCount = true;
        }


        /// <summary>
        /// Get and set properties for selected scorecenter title
        /// </summary>
        public void RefreshScorecenterSelectedProperties()
        {
            try
            {
                FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#ScoreCenter.Category");
                if (FanartHandlerSetup.SelectedItem != null && FanartHandlerSetup.SelectedItem.Equals("..", StringComparison.CurrentCulture) == false && FanartHandlerSetup.SelectedItem.Trim().Length > 0)
                {
                    if (CurrSelectedScorecenterGenre.Equals(FanartHandlerSetup.SelectedItem, StringComparison.CurrentCulture) == false)
                    {
                        string sFilenamePrev = CurrSelectedMusic;
                        CurrSelectedScorecenter = String.Empty;
                        PrevSelectedScorecenter = -1;
                        UpdateVisibilityCount = 0;
                        SetCurrentArtistsImageNames(null);
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, "ScoreCenter", "FanartSelected", true, false);
                        if (sFilename.Length == 0)
                        {
                            FanartAvailable = false;
                        }
                        else
                        {
                            FanartAvailable = true;
                            CurrSelectedScorecenter = sFilename;
                        }
                        if (DoShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref ListSelectedScorecenter);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref ListSelectedScorecenter);
                        }
                        CurrSelectedScorecenterGenre = FanartHandlerSetup.SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                        {
                            ResetCurrCount();
                        }
                        //ResetCurrCount();
                    }
                    else if (CurrCount >= FanartHandlerSetup.MaxCountImage)
                    {
                        string sFilenamePrev = CurrSelectedScorecenter;
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref CurrSelectedScorecenter, ref PrevSelectedScorecenter, "ScoreCenter", "FanartSelected", false, false);
                        if (sFilename.Length == 0)
                        {
                            FanartAvailable = false;
                        }
                        else
                        {
                            FanartAvailable = true;
                            CurrSelectedScorecenter = sFilename;
                        }
                        if (DoShowImageOne)
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop1.selected", sFilename, ref ListSelectedScorecenter);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.scorecenter.backdrop2.selected", sFilename, ref ListSelectedScorecenter);
                        }
                        CurrSelectedScorecenterGenre = FanartHandlerSetup.SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else
                {
                    CurrSelectedScorecenter = String.Empty;
                    CurrSelectedScorecenterGenre = String.Empty;
                    PrevSelectedScorecenter = -1;
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
        private string GetMusicArtistFromListControl()
        {
            try
            {
                GUIListItem gli = GUIControl.GetSelectedListItem(GUIWindowManager.ActiveWindow, 50);

                if (gli != null)
                {
                    if (gli.MusicTag == null && gli.Label.Equals("..", StringComparison.CurrentCulture))
                    {
                        //on .. entry in listcontrol?                        
                        return "..";
                    }
                    else if (gli.MusicTag == null)
                    {
                        List<SongMap> songsMap = new List<SongMap>();
                        FanartHandlerSetup.MDB.GetSongsByPath(gli.Path, ref songsMap);
                        if (songsMap != null)
                        {
                            foreach (SongMap song1 in songsMap)
                            {
                                return Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(song1.m_song.Artist));                                            
                            }
                        }
                             
                        string sArtistName = null;
                        string dbArtistName = null;
                        string currSelectedInList = GUIPropertyManager.GetProperty("#selecteditem");
                        currSelectedInList = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(Utils.GetArtistLeftOfMinusSign(currSelectedInList)));
                        ArrayList al = new ArrayList();
                        FanartHandlerSetup.MDB.GetAllArtists(ref al);
                        for (int i = 0; i < al.Count; i++)
                        {
                            dbArtistName = Utils.MovePrefixToBack(Utils.RemoveMPArtistPipes(al[i].ToString()));
                            if (currSelectedInList.IndexOf(dbArtistName, StringComparison.CurrentCulture) >= 0)
                            {
                                sArtistName = dbArtistName;
                                break;
                            }
                        }
                        if (al != null)
                        {
                            al.Clear();
                        }
                        al = null;
                        currSelectedInList = Utils.GetArtistLeftOfMinusSign(GUIPropertyManager.GetProperty("#selecteditem"));
                        if (sArtistName == null)
                        {
                            al = new ArrayList();
                            if (FanartHandlerSetup.MDB.GetAlbums(3, currSelectedInList, ref al))
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
                            if (FanartHandlerSetup.MDB.GetAlbums(3, currSelectedInList, ref al))
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
                        if (al != null)
                        {
                            al.Clear();
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
                FanartHandlerSetup.SelectedItem = GetMusicArtistFromListControl();
                if (FanartHandlerSetup.SelectedItem != null && FanartHandlerSetup.SelectedItem.Length > 0)
                {
                    //do nothing
                }
                else
                {
                    FanartHandlerSetup.SelectedItem = GUIPropertyManager.GetProperty("#selecteditem");                 
                }
                //FanartHandlerSetup.SelectedItem = MediaPortal.Util.Utils.MakeFileName(FanartHandlerSetup.SelectedItem);
                if (FanartHandlerSetup.SelectedItem != null && FanartHandlerSetup.SelectedItem.Equals("..", StringComparison.CurrentCulture) == false && FanartHandlerSetup.SelectedItem.Trim().Length > 0)
                {
                    if (CurrSelectedMusicArtist.Equals(FanartHandlerSetup.SelectedItem, StringComparison.CurrentCulture) == false)
                    {
                        string sFilenamePrev = CurrSelectedMusic;
                        CurrSelectedMusic = String.Empty;
                        PrevSelectedMusic = -1;
                        UpdateVisibilityCount = 0;
                        SetCurrentArtistsImageNames(null);
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref CurrSelectedMusic, ref PrevSelectedMusic, "MusicFanart Scraper", "FanartSelected", true, true);
                        if (sFilename.Length == 0)
                        {
                            sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
                            if (sFilename.Length == 0)
                            {
                                FanartAvailable = false;
                            }
                            else
                            {
                                FanartAvailable = true;
                                FoundItem = true;
                                CurrSelectedMusic = sFilename;
                            }
                        }
                        else
                        {
                            FoundItem = true;
                            FanartAvailable = true;
                        }
                        if (DoShowImageOne)
                        {
                            AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref ListSelectedMusic);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref ListSelectedMusic);
                        }
                        CurrSelectedMusicArtist = FanartHandlerSetup.SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    else if (CurrCount >= FanartHandlerSetup.MaxCountImage)
                    {
                        string sFilenamePrev = CurrSelectedMusic;
                        string sFilename = FanartHandlerSetup.GetFilename(FanartHandlerSetup.SelectedItem, ref CurrSelectedMusic, ref PrevSelectedMusic, "MusicFanart Scraper", "FanartSelected", false, true);
                        if (sFilename.Length == 0)
                        {
                            sFilename = FanartHandlerSetup.GetRandomDefaultBackdrop(ref CurrSelectedMusic, ref PrevSelectedMusic);
                            if (sFilename.Length == 0)
                            {
                                FanartAvailable = false;
                            }
                            else
                            {
                                FanartAvailable = true;
                                FoundItem = true;
                                CurrSelectedMusic = sFilename;
                            }
                        }
                        else
                        {
                            FoundItem = true;
                            FanartAvailable = true;
                        }
                        if (DoShowImageOne)
                        {
                            AddProperty("#fanarthandler.music.backdrop1.selected", sFilename, ref ListSelectedMusic);
                        }
                        else
                        {
                            AddProperty("#fanarthandler.music.backdrop2.selected", sFilename, ref ListSelectedMusic);
                        }
                        CurrSelectedMusicArtist = FanartHandlerSetup.SelectedItem;
                        if ((sFilename.Length == 0) || (sFilename.Equals(sFilenamePrev, StringComparison.CurrentCulture) == false))
                        {
                            ResetCurrCount();
                        }
                    }
                    IncreaseCurrCount();
                }
                else if (FanartHandlerSetup.SelectedItem != null && FanartHandlerSetup.SelectedItem.Equals("..", StringComparison.CurrentCulture))
                {
                    CurrSelectedMusic = String.Empty;
                    CurrSelectedMusicArtist = String.Empty;
                }
                else
                {
                    CurrSelectedMusic = String.Empty;
                    PrevSelectedMusic = -1;
                    FanartAvailable = false;
                    if (DoShowImageOne)
                        AddProperty("#fanarthandler.music.backdrop1.selected", string.Empty, ref ListSelectedMusic);
                    else
                        AddProperty("#fanarthandler.music.backdrop2.selected", string.Empty, ref ListSelectedMusic);
                    ResetCurrCount();
                    CurrSelectedMusicArtist = String.Empty;
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
                foreach (DictionaryEntry de in Properties)
                {
                    FanartHandlerSetup.SetProperty(de.Key.ToString(), de.Value.ToString());
                }
                if (Properties != null)
                {
                    Properties.Clear();
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateProperties: " + ex.ToString());
            }
        }

        /// <summary>
        /// Add new images that later will update skin properties
        /// </summary>
        private void AddProperty(string property, string value, ref ArrayList al)
        {
            try
            {
                if (value == null)
                    value = "";

                if (Properties.Contains(property))
                {
                    Properties[property] = value;
                }
                else
                {
                    Properties.Add(property, value);
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
                            Utils.LoadImage(value);
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
