namespace FanartHandler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Threading;

    class ProgressWorker : BackgroundWorker
    {
        public ProgressWorker()
        {
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;            
        }

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            Thread.CurrentThread.Priority = FanartHandlerSetup.ThreadPriority;            
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
            ReportProgress (100, "Done");
            e.Result = 0;
        }

        public void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            FanartHandlerSetup.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty + e.ProgressPercentage);
        }

        public void OnRunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(1000);
            FanartHandlerSetup.SetProperty("#fanarthandler.scraper.percent.completed", String.Empty);
            FanartHandlerSetup.HideScraperProgressIndicator();
            Utils.GetDbm().TotArtistsBeingScraped = 0;
            Utils.GetDbm().CurrArtistsBeingScraped = 0;
        }
        
    }



}
