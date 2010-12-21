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
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    public class ScraperNowWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string artist;
        private string album;  
        private bool triggerRefresh/* = false*/;        
        #endregion

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string Album
        {
            get { return album; }
            set { album = value; }
        }

        public bool TriggerRefresh
        {
            get { return triggerRefresh; }
            set { triggerRefresh = value; }
        }

        public ScraperNowWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                int sync = Interlocked.CompareExchange(ref FanartHandlerSetup.SyncPointScraper, 1, 0);
                if (Utils.GetIsStopping() == false && sync == 0)
                {
                    if (FanartHandlerSetup.FHThreadPriority.Equals("Lowest", StringComparison.CurrentCulture))
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    }
                    else
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    }
                    Thread.CurrentThread.Name = "ScraperNowWorker";
                    string [] s = e.Argument as string[];
                    this.artist = s[0];
                    this.album = s[1];                    
                    this.triggerRefresh = false;
                    Utils.GetDbm().IsScraping = true;
                    FanartHandlerSetup.ShowScraperProgressIndicator();
                    FanartHandlerSetup.SetProperty("#fanarthandler.scraper.task", "Now Playing Scrape");
                    Utils.GetDbm().NowPlayingScrape(artist, album);
                    Utils.GetDbm().IsScraping = false;
                    ReportProgress(100, "Done");
                    Utils.SetDelayStop(false);
                    //FanartHandlerSetup.SetProperty("#fanarthandler.scraper.task", String.Empty);
                    FanartHandlerSetup.SyncPointScraper = 0;
                    e.Result = 0;
                }
            }
            catch (Exception ex)
            {
                FanartHandlerSetup.SyncPointScraper = 0;
                logger.Error("OnDoWork: " + ex.ToString());
            }
        }

        internal void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {                
                if (Utils.GetIsStopping() == false)
                {
                    FanartHandlerSetup.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty + e.ProgressPercentage);
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnProgressChanged: " + ex.ToString());
            }
        }

        internal void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Thread.Sleep(1000);
                    FanartHandlerSetup.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
                    FanartHandlerSetup.SetProperty("#fanarthandler.scraper.task", String.Empty);
                    FanartHandlerSetup.HideScraperProgressIndicator();
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                }
            }
            catch (Exception ex)
            {
                logger.Error("OnRunWorkerCompleted: " + ex.ToString());
            }
        }

    }
}

