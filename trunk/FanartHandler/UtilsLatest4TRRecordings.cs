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


namespace FanartHandler
{
    public static class UtilsLatest4TRRecordings 
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static bool _isGetTypeRunningOnThisThread/* = false*/;
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

        public static LatestsCollection Get4TRRecordings()
        {
            LatestsCollection latests = new LatestsCollection();
            LatestsCollection result = new LatestsCollection();

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
                            latests.Add(new Latest(rec.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture), thumbNail, null, rec.Title, null, null, null, rec.Category, null, null, null, null, null, null, null, null, null, null, null));
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
