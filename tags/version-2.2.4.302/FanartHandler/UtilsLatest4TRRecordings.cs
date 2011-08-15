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
using MediaPortal.Dialogs;
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
    public static class UtilsLatest4TRRecordings 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
        private static ForTheRecord.Entities.Recording _playingRecording;
        private static string _playingRecordingFileName;
        private static Hashtable latest4TRRecordings;

        public static Hashtable Latest4TRRecordings
        {
            get { return UtilsLatest4TRRecordings.latest4TRRecordings; }
            set { UtilsLatest4TRRecordings.latest4TRRecordings = value; }
        }      
        #endregion

        public static bool IsGetTypeRunningOnThisThread
        {
            get { return UtilsLatest4TRRecordings._isGetTypeRunningOnThisThread; }
            set { UtilsLatest4TRRecordings._isGetTypeRunningOnThisThread = value; }
        }        

        internal static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
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

  /*      public static LatestsCollection Get4TRRecordings()
        {
            LatestsCollection latests = new LatestsCollection();
            LatestsCollection result = new LatestsCollection();
            string _serverName = string.Empty;
            string _port = string.Empty;
            try
            {
                using (global::MediaPortal.Profile.Settings xmlreader = new global::MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
                {
                    _serverName = xmlreader.GetValueAsString("fortherecord", "server", String.Empty);
                    _port = xmlreader.GetValueAsString("fortherecord", "tcpPort", String.Empty);
                }
                ServerSettings serverSettings = new ServerSettings();
                serverSettings.ServerName = _serverName; 
                serverSettings.Transport = ServiceTransport.NetTcp;
                serverSettings.Port = Int32.Parse(_port);
                ServiceChannelFactories.Initialize(serverSettings, true);

                using (TvControlServiceAgent tvControlAgent = new TvControlServiceAgent())
                {                    

                    RecordingGroup[] _recordings = tvControlAgent.GetAllRecordingGroups(ChannelType.Television, RecordingGroupMode.GroupByProgramTitle);
                    foreach (RecordingGroup recordingGroup in _recordings)
                    {
                        string thumbNail = recordingGroup....ThumbnailFileName;
                        latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture), thumbNail, null, rec.Title, null, null, null, rec.Category, null, null, null, null, null, null, null, null, null, null, null));                                               
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
                }
                if (latests != null)
                {
                    latests.Clear();
                }

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
        }*/
        
      public static LatestsCollection Get4TRRecordings()
        {
            LatestsCollection latests = new LatestsCollection();
            LatestsCollection result = new LatestsCollection();            

            try
            {
                if (ServiceChannelFactories.IsInitialized)
                {
                    RecordingsModel _model = new RecordingsModel();
                    RecordingsController _controller = new RecordingsController(_model);
                    _controller.Initialize();
                    _controller.SetChannelType(ChannelType.Television);
                    using (TvControlServiceAgent _tvControlAgent = new TvControlServiceAgent())
                    {
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
                                    latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture), thumbNail, null, rec.Title, null, null, null, rec.Category, null, null, null, null, null, null, null, null, rec, null, null, null,null));                                    
                                }

                            }
                            groupIndex++;
                        }
                        latests.Sort(new LatestAddedComparer());
                        latest4TRRecordings = new Hashtable();
                        for (int x0 = 0; x0 < latests.Count; x0++)
                        {
                            latests[x0].DateAdded = latests[x0].DateAdded.Substring(0, 10);
                            result.Add(latests[x0]);
                            latest4TRRecordings.Add(x, latests[x].Playable);
                            x++;
                            if (x == 3)
                            {
                                break;
                            }
                        }
                    }
                    _model = null;
                }
                if (latests != null)
                {
                    latests.Clear();
                }
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
  

      public static bool PlayRecording(int index)
      {
          ForTheRecord.Entities.RecordingSummary recSummary = (ForTheRecord.Entities.RecordingSummary)latest4TRRecordings[index];
          ForTheRecord.Entities.Recording rec = null;
          
          g_Player.Stop(true);

          using (TvControlServiceAgent _tvControlAgent = new TvControlServiceAgent())
          {              
              LiveStream[] _ls = _tvControlAgent.GetLiveStreams();
              for (int i = 0; i < _ls.Count(); i++)
              {
                  _tvControlAgent.StopLiveStream(_ls[i]);
              }
              rec = _tvControlAgent.GetRecordingById(recSummary.RecordingId);
          }         

          int jumpToTime = 0;

          if (rec.LastWatchedPosition.HasValue)
          {
              if (rec.LastWatchedPosition.Value > 10)
              {
                  jumpToTime = rec.LastWatchedPosition.Value;
              }
          }
          if (jumpToTime == 0)
          {
              DateTime startTime = DateTime.Now.AddSeconds(-10);
              if (rec.ProgramStartTime < startTime)
              {
                  startTime = rec.ProgramStartTime;
              }
              TimeSpan preRecordSpan = startTime - rec.RecordingStartTime;
              jumpToTime = (int)preRecordSpan.TotalSeconds;
          }

          rec.LastWatchedTime = DateTime.Now;          
          
          string fileName = rec.RecordingFileName;

          RememberActiveRecordingPosition();

          g_Player.currentFileName = fileName;
          g_Player.currentTitle = rec.Title;
          
          g_Player.currentDescription = rec.CreateCombinedDescription(true);

          _playingRecording = rec;
          _playingRecordingFileName = fileName;

          if (g_Player.Play(fileName,
              rec.ChannelType == ChannelType.Television ? g_Player.MediaType.Recording : g_Player.MediaType.Radio))
          {           
              if (MediaPortal.Util.Utils.IsVideo(fileName))
              {
                  g_Player.ShowFullScreenWindow();

                  if (jumpToTime > 0
                      && jumpToTime > g_Player.Duration - 3)
                  {
                      jumpToTime = (int)g_Player.Duration - 3;
                  }
                  g_Player.SeekAbsolute(jumpToTime <= 0 ? 0 : jumpToTime);                  
              }
              g_Player.PlayBackStopped += new MediaPortal.Player.g_Player.StoppedHandler(OnPlayRecordingBackStopped);
              g_Player.PlayBackEnded += new MediaPortal.Player.g_Player.EndedHandler(OnPlayRecordingBackEnded);
              return true;
          }
          else
          {
              _playingRecording = null;
              _playingRecordingFileName = null;
          }
          return false;
      }
      
      private static void OnPlayRecordingBackStopped(MediaPortal.Player.g_Player.MediaType type, int stoptime, string filename)
      {
          if (g_Player.currentFileName == _playingRecordingFileName)
          {
              _playingRecordingFileName = null;
              _playingRecording = null;

              if (stoptime >= g_Player.Duration)
              {
                  // Temporary workaround before end of stream gets properly implemented.
                  stoptime = 0;
              }
              using (TvControlServiceAgent tvControlAgent = new TvControlServiceAgent())
              {
                  tvControlAgent.SetRecordingLastWatchedPosition(filename, stoptime);
              }
          }
      }

      private static void OnPlayRecordingBackEnded(MediaPortal.Player.g_Player.MediaType type, string filename)
      {
          if (g_Player.currentFileName == _playingRecordingFileName)
          {
              g_Player.Stop();
              _playingRecordingFileName = null;
              _playingRecording = null;

              using (TvControlServiceAgent tvControlAgent = new TvControlServiceAgent())
              {
                  tvControlAgent.SetRecordingLastWatchedPosition(filename, 0);
              }
          }
      }
   

      private static void RememberActiveRecordingPosition()
      {
          if (_playingRecording != null)
          {
              if (g_Player.IsTVRecording
                  && g_Player.currentFileName == _playingRecordingFileName)
              {
                  if (g_Player.CurrentPosition < g_Player.Duration)
                  {
                      using (TvControlServiceAgent tvControlAgent = new TvControlServiceAgent())
                      {
                          tvControlAgent.SetRecordingLastWatchedPosition(_playingRecordingFileName, (int)g_Player.CurrentPosition);
                      }
                  }
              }
          }
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
