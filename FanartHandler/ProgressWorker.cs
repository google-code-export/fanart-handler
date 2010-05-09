namespace FanartHandler
{
    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    class ProgressWorker : BackgroundWorker
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        public ProgressWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            try
            {
                if (Utils.GetIsStopping() == false)
                {
                    Thread.CurrentThread.Priority = FanartHandlerSetup.ThreadPriority;
                    Thread.CurrentThread.Name = "ProgressWorker";
                    double iTot = Utils.GetDbm().TotArtistsBeingScraped;
                    double iCurr = Utils.GetDbm().CurrArtistsBeingScraped;
                    ReportProgress(0, "Starting");
                    while (Utils.GetDbm().GetIsScraping())
                    {
                        iTot = Utils.GetDbm().TotArtistsBeingScraped;
                        iCurr = Utils.GetDbm().CurrArtistsBeingScraped;
                        if (iTot > 0)
                        {
                            ReportProgress(Convert.ToInt32((iCurr / iTot) * 100), "Ongoing");
                        }
                        if (CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    ReportProgress(100, "Done");
                    e.Result = 0;
                }
            }
            catch (Exception ex)
            {
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
