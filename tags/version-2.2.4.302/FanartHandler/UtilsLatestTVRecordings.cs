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
using TvPlugin;
using TvControl;

namespace FanartHandler
{
    public static class UtilsLatestTVRecordings 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
        private static Hashtable latestTVRecordings;
        private static TvDatabase.Recording _rec;
        private static String _filename;
     
        public static Hashtable LatestTVRecordings
        {
            get { return UtilsLatestTVRecordings.latestTVRecordings; }
            set { UtilsLatestTVRecordings.latestTVRecordings = value; }
        }
        #endregion

        public static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsLatestTVRecordings._isGetTypeRunningOnThisThread; }
            set { UtilsLatestTVRecordings._isGetTypeRunningOnThisThread = value; }
        }        

        public static LatestsCollection GetTVRecordings()
        {
            LatestsCollection result = new LatestsCollection();
            LatestsCollection latests = new LatestsCollection();
            try
            {
                IList<TvDatabase.Recording> recordings = TvDatabase.Recording.ListAll();
                int x = 0;
                foreach (TvDatabase.Recording rec in recordings)
                {
                    latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture), "thumbNail", null, rec.Title, null, null, null, rec.Genre, null, null, null, null, null, null, null, null, rec, null, null, null, null));
                }
                latests.Sort(new LatestAddedComparer());
                latestTVRecordings = new Hashtable();
                for (int x0 = 0; x0 < latests.Count; x0++)
                {
                    latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                    string _filename = ((TvDatabase.Recording)latests[x0].Playable).FileName;
                    string thumbNail = string.Format(CultureInfo.CurrentCulture, "{0}\\{1}{2}", Thumbs.TVRecorded,
                                                Path.ChangeExtension(MediaPortal.Util.Utils.SplitFilename(_filename), null),
                                                MediaPortal.Util.Utils.GetThumbExtension());
                    if (File.Exists(thumbNail))
                    {
                        string tmpThumbNail = MediaPortal.Util.Utils.ConvertToLargeCoverArt(thumbNail);
                        if (File.Exists(tmpThumbNail))
                        {
                            thumbNail = tmpThumbNail;
                        }
                    }
                    if (!File.Exists(thumbNail))
                    {
                        thumbNail = string.Format(CultureInfo.CurrentCulture, "{0}{1}",
                                                 Path.ChangeExtension(_filename, null),
                                                 MediaPortal.Util.Utils.GetThumbExtension());
                    }
                    latests[x0].Thumb = thumbNail;
                    result.Add(latests[x0]);
                    latestTVRecordings.Add(x, latests[x].Playable);
                    x++;
                    if (x == 3)
                    {
                        break;
                    }
                }
                if (latests != null)
                {
                    latests.Clear();
                }
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

        public static bool IsRecordingActual(TvDatabase.Recording aRecording)
        {
            return aRecording.IsRecording;
        }             
      
        public static bool PlayRecording(int index)
        {
            TvDatabase.Recording rec = (TvDatabase.Recording)latestTVRecordings[index];
            _rec = rec;
            _filename = rec.FileName;            

            bool _bIsLiveRecording = false;
            IList<TvDatabase.Recording> itemlist = TvDatabase.Recording.ListAll();

            TvServer server = new TvServer();
            foreach (TvDatabase.Recording recItem in itemlist)
            {
              if (rec.IdRecording == recItem.IdRecording && IsRecordingActual(recItem))
              {
                  _bIsLiveRecording = true;
                  break;
              }
            }

            int stoptime = rec.StopTime;
            if (_bIsLiveRecording)
            {
                stoptime = -1;
            }

            if (TVHome.Card != null)
            {
                TVHome.Card.StopTimeShifting();
            }

            string fileName = TVUtil.GetFileNameForRecording(rec);

            bool useRTSP = TVHome.UseRTSP();

            Log.Info("TvRecorded Play:{0} - using rtsp mode:{1}", fileName, useRTSP);
            if (g_Player.Play(fileName, g_Player.MediaType.Recording))
            {
                if (MediaPortal.Util.Utils.IsVideo(fileName) && !g_Player.IsRadio)
                {
                    g_Player.ShowFullScreenWindow();
                }
                if (stoptime > 0)
                {
                    g_Player.SeekAbsolute(stoptime); 
                }
                else if (stoptime == -1)
                {
                    double dTime = g_Player.Duration - 5;
                    g_Player.SeekAbsolute(dTime);
                }
                g_Player.currentFileName = rec.FileName;
                g_Player.currentTitle = rec.Title;
                g_Player.currentDescription = rec.Description;

                rec.TimesWatched++;
                rec.Persist();

                return true;
            }
            return false;          
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
