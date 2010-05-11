
namespace FanartHandler
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    public class ScraperWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private bool triggerRefresh = false;        
        #endregion

        public bool TriggerRefresh
        {
            get { return triggerRefresh; }
            set { triggerRefresh = value; }
        }

        public ScraperWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                int sync = Interlocked.CompareExchange(ref FanartHandlerSetup.syncPointScraper, 1, 0);
                if (Utils.GetIsStopping() == false && sync == 0)
                {                                        
                    if (FanartHandlerSetup.FhThreadPriority.Equals("Lowest"))
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Lowest;
                    }
                    else
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    }
                    Thread.CurrentThread.Name = "ScraperWorker";
                    this.TriggerRefresh = false;
                    Utils.GetDbm().IsScraping = true;
                    //FanartHandlerSetup.ManageScraperProperties();
                    FanartHandlerSetup.ShowScraperProgressIndicator();
                    Utils.GetDbm().InitialScrape();
                    Thread.Sleep(2000);
                    //Utils.GetDbm().IsScraping = true;
                    //FanartHandlerSetup.ManageScraperProperties();
                    Utils.GetDbm().DoNewScrape();
                    Utils.GetDbm().StopScraper = true;
                    Utils.GetDbm().StopScraper = false;
                    Utils.GetDbm().IsScraping = false;
                    ReportProgress(100, "Done");
                    FanartHandlerSetup.syncPointScraper = 0;
                    e.Result = 0;
                }
            }
            catch (Exception ex)
            {
                FanartHandlerSetup.syncPointScraper = 0;
                logger.Error("OnDoWork: " + ex.ToString());
            }
        }

        public void OnProgressChanged(object sender, ProgressChangedEventArgs e)
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

        public void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Thread.Sleep(1000);
                    FanartHandlerSetup.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
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
