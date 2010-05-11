using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using System.Threading;
using System.Timers;
using SQLite.NET;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;
using MediaPortal.Services;
using MediaPortal.Music.Database;

namespace FanartHandler
{    
    public partial class FanartHandlerConfig : Form
    {
        private DataTable myDataTable = null;
        private DataTable myDataTable2 = null;
        private DataTable myDataTable3 = null;
        private DataTable myDataTable4 = null;
        private DataTable myDataTable5 = null;
        private DataTable myDataTable6 = null;
        private DataTable myDataTable7 = null;
        private DataTable myDataTable8 = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string logFileName = "fanarthandler_config.log";
        private const string oldLogFileName = "fanarthandler_config.old.log";
        //private FanartHandlerSetup.ScraperWorker scraperWorkerObject;        
        //private Thread scrapeWorkerThread;
        private ScraperWorker myScraperWorker = null;        
        private System.Timers.Timer scraperTimer = null;
        private string useArtist = null;
        private string useAlbum = null;
        private string disableMPTumbsForRandom = null;
        private string skipWhenHighResAvailable = null;
        private string defaultBackdropIsImage = null;
        private string useFanart = null;
        private string useOverlayFanart = null;
        private string useMusicFanart = null;
        private string useVideoFanart = null;
        private string useScoreCenterFanart = null;
        private string imageInterval = null;
        private string minResolution = null;
        private string defaultBackdrop = null;
        private string scraperMaxImages = null;
        private string scraperMusicPlaying = null;
        private string scraperMPDatabase = null;
        private string scraperInterval = null;
        private string useAspectRatio = null;
        private string useDefaultBackdrop = null;
        private bool isScraping = false;
        public delegate void ScrollDelegate();
        private bool isStopping = false;
        private int lastID = 0;
        private int lastIDMovie = 0;
        private int lastIDScoreCenter = 0;
        private int lastIDGame = 0;
        private int lastIDPicture = 0;
        private int lastIDPlugin = 0;
        private int lastIDTV = 0; 
        private string proxyHostname = null;
        private string proxyPort = null;
        private string proxyUsername = null;
        private string proxyPassword = null;
        private string proxyDomain = null;
        private string useProxy = null;
        private System.Text.StringBuilder sb = null;
        private ScrollDelegate s_del;      

        public FanartHandlerConfig()
        {
            InitializeComponent();
        }

        public bool getCheckBoxXFactorFanart()
        {
            return checkBoxXFactorFanart.Checked;
        }

        public bool getCheckBoxThumbsAlbum()
        {
            return checkBoxThumbsAlbum.Checked;
        }

        public bool getCheckBoxThumbsArtist()
        {
            return checkBoxThumbsArtist.Checked;
        }

        private bool checkValidity()
        {
            bool sout = false;
            if ((checkBoxXFactorFanart.Checked == false) && (checkBoxThumbsAlbum.Checked == false) && (checkBoxThumbsArtist.Checked == false))
                sout = false;
            else
                sout = true;
            if ((checkBoxXFactorFanart.Checked == false) && (checkBoxThumbsDisabled.Checked == true))
                sout = false;

            return sout;
        }
        
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void setupConfigFile()
        {
            try
            {
                String path = Config.GetFile(Config.Dir.Config, "FanartHandler.xml");
                String pathOrg = Config.GetFile(Config.Dir.Config, "FanartHandler.org");
                if (File.Exists(path))
                {
                    //do nothing
                }
                else
                {
                    File.Copy(pathOrg, path);
                }
            }
            catch (Exception ex)
            {
                logger.Error("setupConfigFile: " + ex.ToString());
            }
        }

        private void DoSave()
        {
            if (checkValidity())
            {
                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    try
                    {
                        xmlwriter.RemoveEntry("FanartHandler", "useAlbumDisabled");
                        xmlwriter.RemoveEntry("FanartHandler", "useArtistDisabled");                        
                    }
                    catch
                    { }
                    xmlwriter.SetValue("FanartHandler", "useFanart", checkBoxXFactorFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useAlbum", checkBoxThumbsAlbum.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useArtist", checkBoxThumbsArtist.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "skipWhenHighResAvailable", checkBoxSkipMPThumbsIfFanartAvailble.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "disableMPTumbsForRandom", checkBoxThumbsDisabled.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useOverlayFanart", checkBoxOverlayFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useMusicFanart", checkBoxEnableMusicFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useVideoFanart", checkBoxEnableVideoFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useScoreCenterFanart", checkBoxEnableScoreCenterFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "imageInterval", comboBoxInterval.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "minResolution", comboBoxMinResolution.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "defaultBackdrop", textBoxDefaultBackdrop.Text);
                    xmlwriter.SetValue("FanartHandler", "defaultBackdropIsImage", radioButtonBackgroundIsFile.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperMaxImages", comboBoxMaxImages.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "scraperMusicPlaying", checkBoxScraperMusicPlaying.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperMPDatabase", checkBoxEnableScraperMPDatabase.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperInterval", comboBoxScraperInterval.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "useAspectRatio", checkBoxAspectRatio.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useDefaultBackdrop", checkBoxEnableDefaultBackdrop.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "proxyHostname", textBoxProxyHostname.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyPort", textBoxProxyPort.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyUsername", textBoxProxyUsername.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyPassword", textBoxProxyPassword.Text);
                    xmlwriter.SetValue("FanartHandler", "proxyDomain", textBoxProxyDomain.Text);
                    xmlwriter.SetValue("FanartHandler", "useProxy", checkBoxProxy.Checked ? true : false);
                    
                }
                MessageBox.Show("Settings is stored in memory. Make sure to press Ok when exiting MP Configuration. Pressing Cancel when exiting MP Configuration will result in these setting NOT being saved!");
            }
            else
            {
                MessageBox.Show("Error: You have to select at least on of the checkboxes under headline \"Selected Fanart Sources\". Also you cannot disable both album and artist thumbs if you also have disabled fanart. If you do not want to use fanart you still have to check at least one of the checkboxes and the disable the plugin.");
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void FanartHandlerConfig_FormClosing(object sender, FormClosedEventArgs e)
        {
            if (!DesignMode)
            {
                DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo);
                stopScraper();
                if (result == DialogResult.No)
                {
                    //do nothing
                }

                if (result == DialogResult.Yes)
                {                    
                    DoSave();
                }
                logger.Info("Fanart Handler configuration is stopped.");          
                this.Close();
            }
        }

        private void FanartHandlerConfig_Load(object sender, EventArgs e)
        {
            label11.Text = "Version "+Utils.GetAllVersionNumber();
            comboBoxInterval.Enabled = true;
            comboBoxInterval.Items.Clear();
            comboBoxInterval.Items.Add("20");
            comboBoxInterval.Items.Add("30");
            comboBoxInterval.Items.Add("40");
            comboBoxInterval.Items.Add("60");
            comboBoxInterval.Items.Add("90");
            comboBoxInterval.Items.Add("120");
            comboBoxMaxImages.Enabled = true;
            comboBoxMaxImages.Items.Clear();
            comboBoxMaxImages.Items.Add("1");
            comboBoxMaxImages.Items.Add("2");
            comboBoxMaxImages.Items.Add("3");
            comboBoxMaxImages.Items.Add("4");
            comboBoxMaxImages.Items.Add("5");
            comboBoxMaxImages.Items.Add("6");
            comboBoxMaxImages.Items.Add("8");
            comboBoxMaxImages.Items.Add("10");
            comboBoxScraperInterval.Enabled = true;
            comboBoxScraperInterval.Items.Clear();
            comboBoxScraperInterval.Items.Add("6");
            comboBoxScraperInterval.Items.Add("12");
            comboBoxScraperInterval.Items.Add("18");
            comboBoxScraperInterval.Items.Add("24");
            comboBoxScraperInterval.Items.Add("48");
            comboBoxScraperInterval.Items.Add("72");
            comboBoxMinResolution.Enabled = true;
            comboBoxMinResolution.Items.Clear();
            comboBoxMinResolution.Items.Add("0x0");
            comboBoxMinResolution.Items.Add("300x300");
            comboBoxMinResolution.Items.Add("350x350");
            comboBoxMinResolution.Items.Add("400x400");
            comboBoxMinResolution.Items.Add("500x500");
            comboBoxMinResolution.Items.Add("960x540");
            comboBoxMinResolution.Items.Add("1024x576");
            comboBoxMinResolution.Items.Add("1280x720");
            comboBoxMinResolution.Items.Add("1920x1080");

            setupConfigFile();

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
            {

                useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", String.Empty);
                useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", String.Empty);
                useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", String.Empty);
                skipWhenHighResAvailable = xmlreader.GetValueAsString("FanartHandler", "skipWhenHighResAvailable", String.Empty);
                disableMPTumbsForRandom = xmlreader.GetValueAsString("FanartHandler", "disableMPTumbsForRandom", String.Empty);
                useOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", String.Empty);
                useMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", String.Empty);
                useVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", String.Empty);
                useScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", String.Empty);
                imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", String.Empty);
                minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", String.Empty);
                defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", String.Empty);
                scraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", String.Empty);
                scraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", String.Empty);
                scraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", String.Empty);
                scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", String.Empty);                         
                useAspectRatio = xmlreader.GetValueAsString("FanartHandler", "useAspectRatio", String.Empty);
                defaultBackdropIsImage = xmlreader.GetValueAsString("FanartHandler", "defaultBackdropIsImage", String.Empty);
                useDefaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "useDefaultBackdrop", String.Empty);
                proxyHostname = xmlreader.GetValueAsString("FanartHandler", "proxyHostname", String.Empty);
                proxyPort = xmlreader.GetValueAsString("FanartHandler", "proxyPort", String.Empty);
                proxyUsername = xmlreader.GetValueAsString("FanartHandler", "proxyUsername", String.Empty);
                proxyPassword = xmlreader.GetValueAsString("FanartHandler", "proxyPassword", String.Empty);
                proxyDomain = xmlreader.GetValueAsString("FanartHandler", "proxyDomain", String.Empty);
                useProxy = xmlreader.GetValueAsString("FanartHandler", "useProxy", String.Empty);        
            }
                if (useFanart != null && useFanart.Length > 0)
                {
                    if (useFanart.Equals("True"))
                        checkBoxXFactorFanart.Checked = true;
                    else
                        checkBoxXFactorFanart.Checked = false;
                }
                else
                {
                    useFanart = "True";
                    checkBoxXFactorFanart.Checked = true;
                }
                if (useProxy != null && useProxy.Length > 0)
                {
                    if (useProxy.Equals("True"))
                    {
                        checkBoxProxy.Checked = true;
                        textBoxProxyHostname.Enabled = true;
                        textBoxProxyPort.Enabled = true;
                        textBoxProxyUsername.Enabled = true;
                        textBoxProxyPassword.Enabled = true;
                        textBoxProxyDomain.Enabled = true;
                    }
                    else
                    {
                        checkBoxProxy.Checked = false;
                        textBoxProxyHostname.Enabled = false;
                        textBoxProxyPort.Enabled = false;
                        textBoxProxyUsername.Enabled = false;
                        textBoxProxyPassword.Enabled = false;
                        textBoxProxyDomain.Enabled = false;
                    }
                }
                else
                {
                    useProxy = "False";
                    checkBoxProxy.Checked = false;
                    textBoxProxyHostname.Enabled = false;
                    textBoxProxyPort.Enabled = false;
                    textBoxProxyUsername.Enabled = false;
                    textBoxProxyPassword.Enabled = false;
                    textBoxProxyDomain.Enabled = false;
                }            
                if (useAlbum != null && useAlbum.Length > 0)
                {
                    if (useAlbum.Equals("True"))
                        checkBoxThumbsAlbum.Checked = true;
                    else
                        checkBoxThumbsAlbum.Checked = false;
                }
                else
                {
                    useAlbum = "False";
                    checkBoxThumbsAlbum.Checked = false;
                }
                if (useArtist != null && useArtist.Length > 0)
                {
                    if (useArtist.Equals("True"))
                        checkBoxThumbsArtist.Checked = true;
                    else
                        checkBoxThumbsArtist.Checked = false;
                }
                else
                {
                    useAlbum = "True";
                    checkBoxThumbsArtist.Checked = true;
                }
                if (skipWhenHighResAvailable != null && skipWhenHighResAvailable.Length > 0)
                {
                    if (skipWhenHighResAvailable.Equals("True"))
                        checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
                    else
                        checkBoxSkipMPThumbsIfFanartAvailble.Checked = false;
                }
                else
                {
                    skipWhenHighResAvailable = "True";
                    checkBoxSkipMPThumbsIfFanartAvailble.Checked = true;
                }
                if (disableMPTumbsForRandom != null && disableMPTumbsForRandom.Length > 0)
                {
                    if (disableMPTumbsForRandom.Equals("True"))
                        checkBoxThumbsDisabled.Checked = true;
                    else
                        checkBoxThumbsDisabled.Checked = false;
                }
                else
                {
                    disableMPTumbsForRandom = "True";
                    checkBoxThumbsDisabled.Checked = true;
                }
                if (defaultBackdropIsImage != null && defaultBackdropIsImage.Length > 0)
                {
                    if (defaultBackdropIsImage.Equals("True"))
                    {
                        radioButtonBackgroundIsFile.Checked = true;
                        radioButtonBackgroundIsFolder.Checked = false;
                    }
                    else
                    {
                        radioButtonBackgroundIsFile.Checked = false;
                        radioButtonBackgroundIsFolder.Checked = true;
                    }
                }
                else
                {
                    defaultBackdropIsImage = "True";
                    radioButtonBackgroundIsFile.Checked = true;
                    radioButtonBackgroundIsFolder.Checked = false;
                }            
                if (useOverlayFanart != null && useOverlayFanart.Length > 0)
                {
                    if (useOverlayFanart.Equals("True"))
                        checkBoxOverlayFanart.Checked = true;
                    else
                        checkBoxOverlayFanart.Checked = false;
                }
                else
                {
                    useOverlayFanart = "True";
                    checkBoxOverlayFanart.Checked = true;
                }
                if (useDefaultBackdrop != null && useDefaultBackdrop.Length > 0)
                {
                    if (useDefaultBackdrop.Equals("True"))
                        checkBoxEnableDefaultBackdrop.Checked = true;
                    else
                        checkBoxEnableDefaultBackdrop.Checked = false;
                }
                else
                {
                    useDefaultBackdrop = "True";
                    checkBoxEnableDefaultBackdrop.Checked = true;
                }            
                if (useMusicFanart != null && useMusicFanart.Length > 0)
                {
                    if (useMusicFanart.Equals("True"))
                        checkBoxEnableMusicFanart.Checked = true;
                    else
                        checkBoxEnableMusicFanart.Checked = false;
                }
                else
                {
                    useMusicFanart = "True";
                    checkBoxEnableMusicFanart.Checked = true;
                }
                if (useVideoFanart != null && useVideoFanart.Length > 0)
                {
                    if (useVideoFanart.Equals("True"))
                        checkBoxEnableVideoFanart.Checked = true;
                    else
                        checkBoxEnableVideoFanart.Checked = false;
                }
                else
                {
                    useVideoFanart = "True";
                    checkBoxEnableVideoFanart.Checked = true;
                }
                if (useScoreCenterFanart != null && useScoreCenterFanart.Length > 0)
                {
                    if (useScoreCenterFanart.Equals("True"))
                        checkBoxEnableScoreCenterFanart.Checked = true;
                    else
                        checkBoxEnableScoreCenterFanart.Checked = false;
                }
                else
                {
                    useScoreCenterFanart = "True";
                    checkBoxEnableScoreCenterFanart.Checked = true;
                }
                if (imageInterval != null && imageInterval.Length > 0)
                {
                    comboBoxInterval.SelectedItem = imageInterval;
                }
                else
                {
                    imageInterval = "30";
                    comboBoxInterval.SelectedItem = "30";
                }
                if (minResolution != null && minResolution.Length > 0)
                {
                    comboBoxMinResolution.SelectedItem = minResolution;
                }
                else
                {
                    minResolution = "0x0";
                    comboBoxMinResolution.SelectedItem = "0x0";
                }
                if (defaultBackdrop != null && defaultBackdrop.Length > 0)
                {
                    textBoxDefaultBackdrop.Text = defaultBackdrop;
                }
                else
                {
                    string tmpPath = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music\default.jpg";
                    defaultBackdrop = tmpPath;
                    textBoxDefaultBackdrop.Text = tmpPath;
                }
                if (proxyHostname != null && proxyHostname.Length > 0)
                {
                    textBoxProxyHostname.Text = proxyHostname;
                }
                else
                {
                    textBoxProxyHostname.Text = String.Empty;
                }
                if (proxyPort != null && proxyPort.Length > 0)
                {
                    textBoxProxyPort.Text = proxyPort;
                }
                else
                {
                    textBoxProxyPort.Text = String.Empty;
                }
                if (proxyUsername != null && proxyUsername.Length > 0)
                {
                    textBoxProxyUsername.Text = proxyUsername;
                }
                else
                {
                    textBoxProxyUsername.Text = String.Empty;
                }
                if (proxyPassword != null && proxyPassword.Length > 0)
                {
                    textBoxProxyPassword.Text = proxyPassword;
                }
                else
                {
                    textBoxProxyPassword.Text = String.Empty;
                }
                if (proxyDomain != null && proxyDomain.Length > 0)
                {
                    textBoxProxyDomain.Text = proxyDomain;
                }
                else
                {
                    textBoxProxyDomain.Text = String.Empty;
                }
                if (scraperMaxImages != null && scraperMaxImages.Length > 0)
                {
                    comboBoxMaxImages.SelectedItem = scraperMaxImages;
                }
                else
                {
                    scraperMaxImages = "2";
                    comboBoxMaxImages.SelectedItem = "2";
                }
                if (scraperMusicPlaying != null && scraperMusicPlaying.Length > 0)
                {
                    if (scraperMusicPlaying.Equals("True"))
                        checkBoxScraperMusicPlaying.Checked = true;
                    else
                        checkBoxScraperMusicPlaying.Checked = false;
                }
                else
                {
                    scraperMusicPlaying = "False";
                    checkBoxScraperMusicPlaying.Checked = false;
                }
                if (scraperMPDatabase != null && scraperMPDatabase.Length > 0)
                {
                    if (scraperMPDatabase.Equals("True"))
                        checkBoxEnableScraperMPDatabase.Checked = true;
                    else
                        checkBoxEnableScraperMPDatabase.Checked = false;
                }
                else
                {
                    scraperMPDatabase = "False";
                    checkBoxEnableScraperMPDatabase.Checked = false;
                }
                if (scraperInterval != null && scraperInterval.Length > 0)
                {
                    comboBoxScraperInterval.SelectedItem = scraperInterval;
                }
                else
                {
                    scraperInterval = "24";
                    comboBoxScraperInterval.SelectedItem = "24";
                }
                if (useAspectRatio != null && useAspectRatio.Length > 0)
                {
                    if (useAspectRatio.Equals("True"))
                        checkBoxAspectRatio.Checked = true;
                    else
                        checkBoxAspectRatio.Checked = false;
                }
                else
                {
                    useAspectRatio = "False";
                    checkBoxAspectRatio.Checked = false;
                }
                try
                {
                    initLogger();
                    //System.Net.ServicePointManager.Expect100Continue = false;
                    logger.Info("Fanart Handler configuration is starting.");
                    logger.Info("Fanart Handler version is " + Utils.GetAllVersionNumber());                                        
                    Utils.SetUseProxy(useProxy);
                    Utils.SetProxyHostname(proxyHostname);
                    Utils.SetProxyPort(proxyPort);
                    Utils.SetProxyUsername(proxyUsername);
                    Utils.SetProxyPassword(proxyPassword);
                    Utils.SetProxyDomain(proxyDomain);
                    Utils.SetScraperMaxImages(scraperMaxImages);
                    Utils.InitiateDbm();
                    ImportLocalFanartAtStartup();
                    Utils.ImportExternalDbFanart("movingpictures.db3", "MovingPicture");
                    Utils.ImportExternalDbFanart("TVSeriesDatabase4.db3", "TVSeries");
                    myDataTable = new DataTable();
                    myDataTable.Columns.Add("Artist");
                    myDataTable.Columns.Add("Enabled");
                    myDataTable.Columns.Add("Image");
                    myDataTable.Columns.Add("Image Path");
                    dataGridView1.DataSource = myDataTable;
                    UpdateFanartTableOnStartup();
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");
                    s_del = new ScrollDelegate(ScrollText);
                    s_del.BeginInvoke(null, null);
                    logger.Info("Fanart Handler configuration is started.");
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable = new DataTable();
                    myDataTable.Columns.Add("Artist");
                    myDataTable.Columns.Add("Enabled");
                    myDataTable.Columns.Add("Image");
                    myDataTable.Columns.Add("Image Path");
                    dataGridView1.DataSource = myDataTable;
                    dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable2 = new DataTable();
                    myDataTable2.Columns.Add("Title");
                    myDataTable2.Columns.Add("Enabled");
                    myDataTable2.Columns.Add("Image");
                    myDataTable2.Columns.Add("Image Path");
                    dataGridView2.DataSource = myDataTable2;
                    UpdateFanartTableMovie();
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView2.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable2 = new DataTable();
                    myDataTable2.Columns.Add("Title");
                    myDataTable2.Columns.Add("Enabled");
                    myDataTable2.Columns.Add("Image");
                    myDataTable2.Columns.Add("Image Path");
                    dataGridView2.DataSource = myDataTable2;
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable3 = new DataTable();
                    myDataTable3.Columns.Add("Genre");
                    myDataTable3.Columns.Add("Enabled");
                    myDataTable3.Columns.Add("Image");
                    myDataTable3.Columns.Add("Image Path");
                    dataGridView3.DataSource = myDataTable3;
                    UpdateFanartTableScoreCenter(); 
                    dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView3.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView3.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable3 = new DataTable();
                    myDataTable3.Columns.Add("Genre");
                    myDataTable3.Columns.Add("Enabled");
                    myDataTable3.Columns.Add("Image");
                    myDataTable3.Columns.Add("Image Path");
                    dataGridView3.DataSource = myDataTable3;
                    dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable4 = new DataTable();
                    myDataTable4.Columns.Add("Genre");
                    myDataTable4.Columns.Add("Enabled");
                    myDataTable4.Columns.Add("Image");
                    myDataTable4.Columns.Add("Image Path");
                    dataGridView4.DataSource = myDataTable4;
                    UpdateFanartTableGame();
                    dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView4.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView4.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable4 = new DataTable();
                    myDataTable4.Columns.Add("Genre");
                    myDataTable4.Columns.Add("Enabled");
                    myDataTable4.Columns.Add("Image");
                    myDataTable4.Columns.Add("Image Path");
                    dataGridView4.DataSource = myDataTable4;
                    dataGridView4.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable5 = new DataTable();
                    myDataTable5.Columns.Add("Genre");
                    myDataTable5.Columns.Add("Enabled");
                    myDataTable5.Columns.Add("Image");
                    myDataTable5.Columns.Add("Image Path");
                    dataGridView5.DataSource = myDataTable5;
                    UpdateFanartTablePicture();
                    dataGridView5.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView5.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView5.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable5 = new DataTable();
                    myDataTable5.Columns.Add("Genre");
                    myDataTable5.Columns.Add("Enabled");
                    myDataTable5.Columns.Add("Image");
                    myDataTable5.Columns.Add("Image Path");
                    dataGridView5.DataSource = myDataTable5;
                    dataGridView5.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable6 = new DataTable();
                    myDataTable6.Columns.Add("Genre");
                    myDataTable6.Columns.Add("Enabled");
                    myDataTable6.Columns.Add("Image");
                    myDataTable6.Columns.Add("Image Path");
                    dataGridView6.DataSource = myDataTable6;
                    UpdateFanartTablePlugin();
                    dataGridView6.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView6.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView6.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable6 = new DataTable();
                    myDataTable6.Columns.Add("Genre");
                    myDataTable6.Columns.Add("Enabled");
                    myDataTable6.Columns.Add("Image");
                    myDataTable6.Columns.Add("Image Path");
                    dataGridView6.DataSource = myDataTable6;
                    dataGridView6.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable7 = new DataTable();
                    myDataTable7.Columns.Add("Genre");
                    myDataTable7.Columns.Add("Enabled");
                    myDataTable7.Columns.Add("Image");
                    myDataTable7.Columns.Add("Image Path");
                    dataGridView7.DataSource = myDataTable7;
                    UpdateFanartTableTV();
                    dataGridView7.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView7.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView7.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable7 = new DataTable();
                    myDataTable7.Columns.Add("Genre");
                    myDataTable7.Columns.Add("Enabled");
                    myDataTable7.Columns.Add("Image");
                    myDataTable7.Columns.Add("Image Path");
                    dataGridView7.DataSource = myDataTable7;
                    dataGridView7.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                try
                {
                    myDataTable8 = new DataTable();
                    myDataTable8.Columns.Add("Artist");
                    myDataTable8.Columns.Add("Fanart Images (#)");
                    dataGridView8.DataSource = myDataTable8;
                    UpdateFanartTableMusicOverview();
                    dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView8.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    dataGridView8.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                catch (Exception ex)
                {
                    logger.Error("FanartHandlerConfig_Load: " + ex.ToString());
                    myDataTable8 = new DataTable();
                    myDataTable8.Columns.Add("Artist");
                    myDataTable8.Columns.Add("Fanart Images (#)");
                    dataGridView8.DataSource = myDataTable8;
                    dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
        }

        

        /// <summary>
        /// Setup logger. This funtion made by the team behind Moving Pictures 
        /// (http://code.google.com/p/moving-pictures/)
        /// </summary>
        private void initLogger()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            try
            {
                FileInfo logFile = new FileInfo(Config.GetFile(Config.Dir.Log, logFileName));
                if (logFile.Exists)
                {
                    if (File.Exists(Config.GetFile(Config.Dir.Log, oldLogFileName)))
                        File.Delete(Config.GetFile(Config.Dir.Log, oldLogFileName));

                    logFile.CopyTo(Config.GetFile(Config.Dir.Log, oldLogFileName));
                    logFile.Delete();
                }
            }
            catch (Exception) { }


            FileTarget fileTarget = new FileTarget();
            fileTarget.FileName = Config.GetFile(Config.Dir.Log, logFileName);
            fileTarget.Layout = "${date:format=dd-MMM-yyyy HH\\:mm\\:ss} " +
                                "${level:fixedLength=true:padding=5} " +
                                "[${logger:fixedLength=true:padding=20:shortName=true}]: ${message} " +
                                "${exception:format=tostring}";

            config.AddTarget("file", fileTarget);

            // Get current Log Level from MediaPortal 
            LogLevel logLevel;
            MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml"));             

            switch ((Level)xmlreader.GetValueAsInt("general", "loglevel", 0))
            {
                case Level.Error:
                    logLevel = LogLevel.Error;
                    break;
                case Level.Warning:
                    logLevel = LogLevel.Warn;
                    break;
                case Level.Information:
                    logLevel = LogLevel.Info;
                    break;
                case Level.Debug:
                default:
                    logLevel = LogLevel.Debug;
                    break;
            }

#if DEBUG
            logLevel = LogLevel.Debug;
#endif

            LoggingRule rule = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule);

            LogManager.Configuration = config;
        }

        private string getFilenameOnly(string filename)
        {
            filename = filename.Replace("/", "\\");
            if (filename.IndexOf("\\") >= 0)
            {
                return filename.Substring(filename.LastIndexOf("\\") + 1);
            }
            return filename;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBoxMinResolution_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDefaultBackdrop_Click(object sender, EventArgs e)

        {
            
        }

        private void checkBoxThumbsAlbum_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBoxDefaultBackdrop_TextChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxMaxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            scraperMaxImages = comboBoxMaxImages.SelectedItem.ToString();
            if (Utils.GetDbm() != null && Utils.GetDbm().IsInitialized)
            {
                Utils.SetScraperMaxImages(scraperMaxImages);
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetScrape();            
        }

        private void ResetScrape()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset the initial scrap flag? This will cause a complete new music scrape on next MP startup.", "Reset Initial Scrape", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                MessageBox.Show("Operation was aborted!");
            }

            if (result == DialogResult.Yes)
            {
                Utils.GetDbm().ResetInitialScrape();
                MessageBox.Show("Done!");
            }
        }
     

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1 != null && dataGridView1.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView1[3, dgv.CurrentRow.Index].Value.ToString());
                    label30.Text = "Resolution: " + img.Width + "x" + img.Height;
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox1.Image = null;
                    pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox1.Image = finalImg;                    
                    img.Dispose();
                    img = null;
                    gfx = null;                    
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
            catch //(Exception ex)
            {
                    //MessageBox.Show(ex.ToString());
            }
        }

        private void DataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2 != null && dataGridView2.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView2[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox3.Image = null;
                    pictureBox3.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox3.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox3.Image = null;
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private void DataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3 != null && dataGridView3.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView3[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox4.Image = null;
                    pictureBox4.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox4.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox4.Image = null;
                }
            }
            catch 
            {                
            }
        }

        private void DataGridView4_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4 != null && dataGridView4.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView4[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox5.Image = null;
                    pictureBox5.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox5.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox5.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5 != null && dataGridView5.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView5[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox6.Image = null;
                    pictureBox6.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox6.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox6.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView6_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6 != null && dataGridView6.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView6[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox7.Image = null;
                    pictureBox7.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox7.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox7.Image = null;
                }
            }
            catch
            {
            }
        }

        private void DataGridView7_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7 != null && dataGridView7.RowCount > 0)
                {
                    DataGridView dgv = (DataGridView)sender;
                    Bitmap img = (Bitmap)Image.FromFile(dataGridView7[3, dgv.CurrentRow.Index].Value.ToString());
                    Size imgSize = new Size(182, 110);
                    Bitmap finalImg = new Bitmap(img, imgSize.Width, imgSize.Height);
                    Graphics gfx = Graphics.FromImage(finalImg);
                    gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gfx.Dispose();
                    pictureBox8.Image = null;
                    pictureBox8.SizeMode = PictureBoxSizeMode.CenterImage;
                    pictureBox8.Image = finalImg;
                    img.Dispose();
                    img = null;
                    gfx = null;
                }
                else
                {
                    pictureBox8.Image = null;
                }
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DeleteSelectedFanart(true);                          
        }

        private void DeleteSelectedFanart(bool doRemove)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index >= 0)
                {
                    pictureBox1.Image = null;
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    
                    Utils.GetDbm().DeleteFanart(sFileName, "MusicFanart");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    if (doRemove)
                    {
                        dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteSelectedFanart: " + ex.ToString());
            } 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteAllFanart();
        }

        private void DeleteAllFanart()
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your music fanart folder to be deleted.", "Delete All Music Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastID = 0;
                    Utils.GetDbm().DeleteAllFanart("MusicFanart");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\music";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        if (Utils.GetFilenameNoPath(dir).ToLower().StartsWith("default") == false)
                        {
                            File.Delete(dir);
                        }
                    }
                    Utils.GetDbm().ResetInitialScrape();
                    myDataTable.Rows.Clear();
                    myDataTable.AcceptChanges();
                    labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                    labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                    labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                    labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUnInitialisedInFanartDatabase();
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("DeleteAllFanart: " + ex.ToString());
            }
        }

        private void DataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {                
                EnableDisableFanart();
            }
            else if (e.KeyData == Keys.Delete)
            {                
                DeleteSelectedFanart(false);
            }
            else if (e.KeyData == Keys.X)
            {
                DeleteAllFanart();
            }
            else if (e.KeyData == Keys.C)
            {
                CleanupMusicFanart();
            }
            else if (e.KeyData == Keys.R)
            {
                ResetScrape();
            }
            else if (e.KeyData == Keys.S)
            {
                StartScrape();
            }
            else if (e.KeyData == Keys.I)
            {
                ImportMusicFanart();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            EnableDisableFanart();
        }

        private void EnableDisableFanart()
        {
            try
            {
                if (dataGridView1.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartMusic(sFileName, false);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartMusic(sFileName, true);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("EnableDisableFanart: " + ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            CleanupMusicFanart();
        }

        private void CleanupMusicFanart()
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("MusicFanart");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("CleanupMusicFanart: " + ex.ToString());
            }
        }

        private void comboBoxScraperInterval_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save your changes?", "Save Changes?", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                //do nothing
            }

            if (result == DialogResult.Yes)
            {
                DoSave();
            }
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Add files in directory to hashtable
        /// </summary>
        public void SetupFilenames(string s, string filter, ref int i, string type)
        {
            string artist = String.Empty;
            string typeOrg = type;
            try
            {
                // Process the list of files found in the directory
                string[] dirs = Directory.GetFiles(s, filter);
                foreach (string dir in dirs)
                {
                    try
                    {
                        try
                        {
                            artist = Utils.GetArtist(dir, type);
                            if (type.Equals("MusicAlbum") || type.Equals("MusicArtist") || type.Equals("MusicFanart"))
                            {
                                if (Utils.GetFilenameNoPath(dir).ToLower().StartsWith("default"))
                                {
                                    type = "Default";
                                }
                                Utils.GetDbm().LoadMusicFanart(artist, dir, dir, type);
                                type = typeOrg;
                            }
                            else
                            {
                                Utils.GetDbm().LoadFanart(artist, dir, dir, type);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("SetupFilenames: " + ex.ToString());
                        }
                        i++;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("SetupFilenames: " + ex.ToString());
                    }
                }
                // Recurse into subdirectories of this directory.
                string[] subdirs = Directory.GetDirectories(s);
                foreach (string subdir in subdirs)
                {
                    SetupFilenames(subdir, filter, ref i, type);
                }

            }
            catch (Exception ex)
            {
                logger.Error("SetupFilenames: " + ex.ToString());
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            StartScrape();
        }

        private void StartScrape()
        {
            try
            {
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True"))
                {
                    if (isScraping == false)
                    {
                        isScraping = true;
                        if (useFanart.Equals("True"))
                        {
                            int i = 0;
                            SetupFilenames(Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music", "*.jpg", ref i, "MusicFanart");
                        }
                        dataGridView1.Enabled = false;
                        button6.Text = "Stop Scraper";
                        button1.Enabled = false;
                        button2.Enabled = false;
                        button3.Enabled = false;
                        button4.Enabled = false;
                        button5.Enabled = false;
                        button15.Enabled = false;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 0;
                        progressBar1.Value = 0;
                        UpdateScraperTimer();
                        Thread progressTimer = new Thread(new ThreadStart(AddToDataGridView));
                        progressTimer.Start();
                        dataGridView1.Enabled = true;
                    }
                    else
                    {
                        button6.Text = "Start Scraper";
                        dataGridView1.Enabled = false;
                        stopScraper();
                        isScraping = false;
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;
                        button4.Enabled = true;
                        button5.Enabled = true;
                        button15.Enabled = true;
                        Utils.GetDbm().StopScraper = false;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = 0;
                        progressBar1.Value = 0;
                        dataGridView1.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("StartScrape: " + ex.ToString());
                dataGridView1.Enabled = true;
            }
        }

        private void AddToDataGridView()
        {
            //Thread.Sleep(5000);
            while (myScraperWorker != null && myScraperWorker.IsBusy)//   scrapeWorkerThread != null && scrapeWorkerThread.IsAlive)
            {                
                UpdateFanartTable();
                Thread.Sleep(3000);
            }
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 1;
            progressBar1.Value = 1;
            Thread.Sleep(1000);
            stopScraper();
        }


        private void UpdateFanartTableScoreCenter()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableScoreCenter(lastIDScoreCenter);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable3.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDScoreCenter)
                            {
                                lastIDScoreCenter = tmpID;
                            }
                            myDataTable3.Rows.Add(myDataRow);
                        }
                        labelTotalScoreCenterFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalScoreCenterInFanartDatabase();
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableScoreCenter: " + ex.ToString());
                dataGridView3.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView3.DataSource = d;
                dataGridView3.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableGame()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDGame, "Game");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable4.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDGame)
                            {
                                lastIDGame = tmpID;
                            }
                            myDataTable4.Rows.Add(myDataRow);
                        }
                        label22.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Game");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableGame: " + ex.ToString());
                dataGridView4.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView4.DataSource = d;
                dataGridView4.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTablePicture()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDPicture, "Picture");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable5.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDPicture)
                            {
                                lastIDPicture = tmpID;
                            }
                            myDataTable5.Rows.Add(myDataRow);
                        }
                        label24.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Picture");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTablePicture: " + ex.ToString());
                dataGridView5.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView5.DataSource = d;
                dataGridView5.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTablePlugin()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDPlugin, "Plugin");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable6.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDPlugin)
                            {
                                lastIDPlugin = tmpID;
                            }
                            myDataTable6.Rows.Add(myDataRow);
                        }
                        label26.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Plugin");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTablePlugin: " + ex.ToString());
                dataGridView6.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView6.DataSource = d;
                dataGridView6.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableTV()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableRandom(lastIDTV, "TV");
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable7.NewRow();
                            myDataRow["Genre"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDTV)
                            {
                                lastIDTV = tmpID;
                            }
                            myDataTable7.Rows.Add(myDataRow);
                        }
                        label28.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("TV");
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableTV: " + ex.ToString());
                dataGridView7.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Genre");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView7.DataSource = d;
                dataGridView7.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableMusicOverview()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableMusicOverview();
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        myDataTable8.Rows.Clear();
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable8.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Fanart Images (#)"] = result.GetField(i, 1);
                            myDataTable8.Rows.Add(myDataRow);
                        }
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableMusicOverview: " + ex.ToString());
                dataGridView8.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = d;
                dataGridView8.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableMovie()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTableMovie(lastIDMovie);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable2.NewRow();
                            myDataRow["Title"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastIDMovie)
                            {
                                lastIDMovie = tmpID;
                            }
                            myDataTable2.Rows.Add(myDataRow);
                        }
                        labelTotalMovieFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalMoviesInFanartDatabase();                        
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableMovie: " + ex.ToString());
                dataGridView2.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Title");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView2.DataSource = d;
                dataGridView2.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private delegate void UpdateFanartTableDelegate();
        private void UpdateFanartTable()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    // Pass the same function to BeginInvoke,
                    // but the call would come on the correct
                    // thread and InvokeRequired will be false.
                    this.BeginInvoke(new UpdateFanartTableDelegate(UpdateFanartTable));
                    return;
                }
                SQLiteResultSet result = Utils.GetDbm().GetDataForTable(lastID);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastID)
                            {
                                lastID = tmpID;
                            }
                            myDataTable.Rows.Add(myDataRow);
                        }
                        labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                        labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                        labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                        labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUnInitialisedInFanartDatabase();
                    }
                }
                result = null;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = Convert.ToInt32(Utils.GetDbm().TotArtistsBeingScraped);
                progressBar1.Value = Convert.ToInt32(Utils.GetDbm().CurrArtistsBeingScraped);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTable: " + ex.ToString());
                dataGridView1.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView1.DataSource = d;
                dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        private void UpdateFanartTableOnStartup()
        {
            try
            {
                SQLiteResultSet result = Utils.GetDbm().GetDataForTable(lastID);
                int tmpID = 0;
                if (result != null)
                {
                    if (result.Rows.Count > 0)
                    {
                        for (int i = 0; i < result.Rows.Count; i++)
                        {
                            DataRow myDataRow = myDataTable.NewRow();
                            myDataRow["Artist"] = result.GetField(i, 0);
                            myDataRow["Enabled"] = result.GetField(i, 1);
                            myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                            myDataRow["Image Path"] = result.GetField(i, 2);
                            tmpID = Convert.ToInt32(result.GetField(i, 3));
                            if (tmpID > lastID)
                            {
                                lastID = tmpID;
                            }
                            myDataTable.Rows.Add(myDataRow);
                        }
                        labelTotalMPArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInMPMusicDatabase();
                        labelTotalFanartArtistCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInFanartDatabase();
                        labelTotalFanartArtistInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsInitialisedInFanartDatabase();
                        labelTotalFanartArtistUnInitCount.Text = String.Empty + Utils.GetDbm().GetTotalArtistsUnInitialisedInFanartDatabase();
                    }
                }
                result = null;
            }
            catch (Exception ex)
            {
                logger.Error("UpdateFanartTableOnStartup: " + ex.ToString());
                dataGridView1.DataSource = null;
                DataTable d = new DataTable();
                d.Columns.Add("Artist");
                d.Columns.Add("Enabled");
                d.Columns.Add("Image");
                d.Columns.Add("Image Path");
                dataGridView1.DataSource = d;
                dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
            }
        }

        public void UpdateScraperTimer()
        {
            try
            {
                if (scraperMPDatabase != null && scraperMPDatabase.Equals("True") && Utils.GetDbm().GetIsScraping() == false)
                {
                    startScraper();
                }
            }
            catch (Exception ex)
            {
                logger.Error("UpdateScraperTimer: " + ex.ToString());
            }
        }

        private void stopScraper()
        {
            try
            {
                if (button6 != null)
                {
                    button6.Enabled = false;
                }
                // Request that the worker thread stop itself:            
                /*if (scraperWorkerObject != null && scrapeWorkerThread != null && scrapeWorkerThread.IsAlive)
                {
                    scraperWorkerObject.RequestStop();

                    // Use the Join method to block the current thread 
                    // until the object's thread terminates.
                    scrapeWorkerThread.Join();
                }*/
                if (myScraperWorker != null)
                {
                    myScraperWorker.CancelAsync();
                    myScraperWorker.Dispose();
                }
                Utils.GetDbm().StopScraper = true;
                if (button6 != null)
                {
                    button6.Text = "Start Scraper";
                }
                isScraping = false;
                if (Utils.GetDbm() != null)
                {
                    Utils.GetDbm().TotArtistsBeingScraped = 0;
                    Utils.GetDbm().CurrArtistsBeingScraped = 0;
                    Utils.GetDbm().StopScraper = false;
                }
                if (progressBar1 != null)
                {
                    progressBar1.Value = 0;
                }
                //scraperWorkerObject = null;
                //scrapeWorkerThread = null;                
                if (button6 != null)
                {
                    button6.Enabled = true;
                }
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;
                button15.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Error("stopScraper: " + ex.ToString());
            }
        }

        
        

        private void startScraper()
        {
            try
            {
                button6.Enabled = false;
                Utils.GetDbm().TotArtistsBeingScraped = 0;
                Utils.GetDbm().CurrArtistsBeingScraped = 0;

                myScraperWorker = new ScraperWorker();
                myScraperWorker.ProgressChanged += myScraperWorker.OnProgressChanged;
                myScraperWorker.RunWorkerCompleted += myScraperWorker.OnRunWorkerCompleted;
                myScraperWorker.RunWorkerAsync();  

/*                scraperWorkerObject = new FanartHandlerSetup.ScraperWorker();
                scrapeWorkerThread = new Thread(scraperWorkerObject.DoWork);
                scrapeWorkerThread.Priority = ThreadPriority.Lowest;
                // Start the worker thread.
                scrapeWorkerThread.Start();
                // Loop until worker thread activates.
                int ix = 0;
                while (!scrapeWorkerThread.IsAlive && ix < 30)
                {
                    System.Threading.Thread.Sleep(500);
                    ix++;
                }
 */
                button6.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Error("startScraper: " + ex.ToString());
            }
        }

        private void checkBoxAspectRatio_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxThumbsAlbumDisabled_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButtonBackgroundIsFile_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFile.Checked)
            {
                labelDefaultBackgroundPathOrFile.Text = "File";
            }
            else
            {
                labelDefaultBackgroundPathOrFile.Text = "Folder";
            }
        }

        private void radioButtonBackgroundIsFolder_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFolder.Checked)
            {
                labelDefaultBackgroundPathOrFile.Text = "Folder";
            }
            else
            {
                labelDefaultBackgroundPathOrFile.Text = "File";
            }
        }

        private void ScrollText()
        {            
            while (isStopping == false)
            {
                char ch = sb[0];
                sb.Remove(0, 1);
                sb.Insert((sb.Length - 1), ch);
                textBox1.Text = sb.ToString();
                textBox1.Refresh();
                System.Threading.Thread.Sleep(160);
            }
        }

        private void buttonBrowseDefaultBackground_Click(object sender, EventArgs e)
        {
            if (radioButtonBackgroundIsFile.Checked)
            {
                OpenFileDialog openFD = new OpenFileDialog();
                openFD.InitialDirectory = Config.GetFolder(Config.Dir.Thumbs);
                openFD.Title = "Select Default Background Image";
                openFD.FileName = textBoxDefaultBackdrop.Text;
                openFD.Filter = "Image Files(*.JPG;*.PNG)|*.JPG;*.PNG";
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    textBoxDefaultBackdrop.Text = openFD.FileName;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");
                }
            }
            else
            {
                FolderBrowserDialog openFD = new FolderBrowserDialog();
                openFD.Description = "Select Default Background Folder";
                openFD.SelectedPath = Config.GetFolder(Config.Dir.Thumbs);
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    textBoxDefaultBackdrop.Text = openFD.SelectedPath;
                    sb = new System.Text.StringBuilder(textBoxDefaultBackdrop.Text + "                                          ");                    
                }
            }
        }

        private void checkBoxEnableDefaultBackdrop_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableDefaultBackdrop.Checked)
            {
                radioButtonBackgroundIsFile.Enabled = true;
                radioButtonBackgroundIsFolder.Enabled = true;
                labelDefaultBackgroundPathOrFile.Enabled = true;
                textBox1.Enabled = true;
                buttonBrowseDefaultBackground.Enabled = true;
            }
            else
            {
                radioButtonBackgroundIsFile.Enabled = false;
                radioButtonBackgroundIsFolder.Enabled = false;
                labelDefaultBackgroundPathOrFile.Enabled = false;
                textBox1.Enabled = false;
                buttonBrowseDefaultBackground.Enabled = false;
            }
        }

        private void checkBoxEnableScraperMPDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableScraperMPDatabase.Checked)
            {
                scraperMPDatabase = "True";
                button6.Enabled = true;
            }
            else
            {
                scraperMPDatabase = "False";
                button6.Enabled = false;
            }
        }

        private void tabPage8_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void tabPage6_Click(object sender, EventArgs e)
        {

        }

        private void checkBoxProxy_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxProxy.Checked)
            {
                textBoxProxyHostname.Enabled = true;
                textBoxProxyPort.Enabled = true;
                textBoxProxyUsername.Enabled = true;
                textBoxProxyPassword.Enabled = true;
                textBoxProxyDomain.Enabled = true;
            }
            else
            {
                textBoxProxyHostname.Enabled = false;
                textBoxProxyPort.Enabled = false;
                textBoxProxyUsername.Enabled = false;
                textBoxProxyPassword.Enabled = false;
                textBoxProxyDomain.Enabled = false;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView2.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView2.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartMovie(sFileName, false);
                        dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartMovie(sFileName, true);
                        dataGridView2.Rows[dataGridView2.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button8_Click: " + ex.ToString());
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.CurrentRow.Index >= 0)
                {
                    pictureBox3.Image = null;
                    string sFileName = dataGridView2.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Movie");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView2.Rows.Remove(dataGridView2.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button10_Click: " + ex.ToString());
            }      
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your movie fanart folder to be deleted.", "Delete All Movie Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDMovie = 0;
                    Utils.GetDbm().DeleteAllFanart("Movie");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\movies";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir); 
                    }
                    myDataTable2.Rows.Clear();
                    myDataTable2.AcceptChanges();
                    labelTotalMovieFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalMoviesInFanartDatabase();                        
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button9_Click: " + ex.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Movie");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button7_Click: " + ex.ToString());
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView3.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView3.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartScoreCenter(sFileName, false);
                        dataGridView3.Rows[dataGridView3.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartScoreCenter(sFileName, true);
                        dataGridView3.Rows[dataGridView3.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button12_Click: " + ex.ToString());
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView3.CurrentRow.Index >= 0)
                {
                    pictureBox4.Image = null;
                    string sFileName = dataGridView3.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "ScoreCenter");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView3.Rows.Remove(dataGridView3.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button14_Click: " + ex.ToString());
            }    
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your scorecenter fanart folder to be deleted.", "Delete All ScoreCenter Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDScoreCenter = 0;
                    Utils.GetDbm().DeleteAllFanart("ScoreCenter");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\scorecenter";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable3.Rows.Clear();
                    myDataTable3.AcceptChanges();
                    labelTotalScoreCenterFanartImages.Text = String.Empty + Utils.GetDbm().GetTotalScoreCenterInFanartDatabase();
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button9_Click: " + ex.ToString());
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("ScoreCenter");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button7_Click: " + ex.ToString());
            }
        }




        private void ImportLocalFanartAtStartup()
        {
            try
            {
                //Add games images
                string path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\games";
                int i = 0;
                SetupFilenames(path, "*.jpg", ref i, "Game");
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\movies";
                i = 0;
                if (useVideoFanart.Equals("True"))
                {
                    SetupFilenames(path, "*.jpg", ref i, "Movie");
                }
                //Add music images
                path = String.Empty;
                i = 0;
                if (useAlbum.Equals("True"))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Albums";
                    SetupFilenames(path, "*L.jpg", ref i, "MusicAlbum");
                }
                if (useArtist.Equals("True"))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                    SetupFilenames(path, "*L.jpg", ref i, "MusicArtist");
                }
                if (useFanart.Equals("True"))
                {
                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                    SetupFilenames(path, "*.jpg", ref i, "MusicFanart");
                }
                //Add pictures images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\pictures";
                i = 0;
                SetupFilenames(path, "*.jpg", ref i, "Picture");
                //Add games images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\scorecenter";
                i = 0;
                if (useScoreCenterFanart.Equals("True"))
                {
                    SetupFilenames(path, "*.jpg", ref i, "ScoreCenter");
                }
                //Add moving pictures images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\MovingPictures\Backdrops\FullSize";
                i = 0;
                SetupFilenames(path, "*.jpg", ref i, "MovingPicture");
                //Add tvseries images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Fan Art\fanart\original";
                i = 0;
                SetupFilenames(path, "*.jpg", ref i, "TVSeries");
                //Add tv images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\tv";
                i = 0;
                SetupFilenames(path, "*.jpg", ref i, "TV");
                //Add plugins images
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\plugins";
                i = 0;
                SetupFilenames(path, "*.jpg", ref i, "Plugin");
            }
            catch (Exception ex)
            {
                logger.Error("ImportLocalFanartAtStartup: " + ex.ToString());
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //import local fanart movie
            try
            {
                if (useVideoFanart.Equals("True"))
                {
                    ImportLocalFanart("Movie");
                    ImportLocalFanartAtStartup();
                    UpdateFanartTableMovie();
                    
                }
            }
            catch (Exception ex)
            {
                logger.Error("button16_Click: " + ex.ToString());
            }
        }        

        private void button15_Click(object sender, EventArgs e)
        {
            //import local fanart music
            ImportMusicFanart();
        }

        private void ImportMusicFanart()
        {
            try
            {
                if (isScraping == false)
                {
                    isScraping = true;
                    if (useMusicFanart.Equals("True"))
                    {
                        ImportLocalFanart("MusicFanart");
                        ImportLocalFanartAtStartup();
                        UpdateFanartTableOnStartup();
                    }
                    isScraping = false;
                }
            }
            catch (Exception ex)
            {
                logger.Error("ImportMusicFanart: " + ex.ToString());
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            //import local fanart scorecenter
            try
            {
                if (useScoreCenterFanart.Equals("True"))
                {
                    ImportLocalFanart("ScoreCenter");
                    ImportLocalFanartAtStartup();
                    UpdateFanartTableScoreCenter(); 
                }                
            }
            catch (Exception ex)
            {
                logger.Error("button17_Click: " + ex.ToString());
            }
        }

        private void ImportLocalFanart(string type)
        {
            try
            {
                string artist = null;
                string path = Config.GetFolder(Config.Dir.Thumbs);
                string newFilename = null;
                Random randNumber = new Random();
                OpenFileDialog openFD = new OpenFileDialog();
                openFD.InitialDirectory = Config.GetFolder(Config.Dir.Thumbs);
                openFD.Title = "Select Fanart Images To Import";
                openFD.Filter = "Image Files(*.JPG)|*.JPG";
                openFD.Multiselect = true;
                if (openFD.ShowDialog() == DialogResult.Cancel)
                {
                }
                else
                {
                    foreach (String file in openFD.FileNames)
                    {
                        artist = Utils.GetArtist(file, type);
                        if (type.Equals("MusicFanart"))
                        {
                            newFilename = path + @"\Skin FanArt\music\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Movie"))
                        {
                            newFilename = path + @"\Skin FanArt\movies\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Game"))
                        {
                            newFilename = path + @"\Skin FanArt\games\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Picture"))
                        {
                            newFilename = path + @"\Skin FanArt\pictures\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("Plugin"))
                        {
                            newFilename = path + @"\Skin FanArt\plugins\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else if (type.Equals("TV"))
                        {
                            newFilename = path + @"\Skin FanArt\tv\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        else
                        {
                            newFilename = path + @"\Skin FanArt\scorecenter\" + artist + " (" + randNumber.Next(10000, 99999) + ").jpg";
                        }
                        File.Copy(file,newFilename);                       
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ImportLocalFanart: " + ex.ToString());
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView4.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView4.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Game");
                        dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Game");
                        dataGridView4.Rows[dataGridView4.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button20_Click: " + ex.ToString());
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView4.CurrentRow.Index >= 0)
                {
                    pictureBox5.Image = null;
                    string sFileName = dataGridView4.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Game");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView4.Rows.Remove(dataGridView4.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button22_Click: " + ex.ToString());
            } 
        }

        private void button21_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your game fanart folder to be deleted.", "Delete All Game Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDGame = 0;
                    Utils.GetDbm().DeleteAllFanart("Game");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\games";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable4.Rows.Clear();
                    myDataTable4.AcceptChanges();
                    label22.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Game");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button21_Click: " + ex.ToString());
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Game");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button19_Click: " + ex.ToString());
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Game");
                ImportLocalFanartAtStartup();
                UpdateFanartTableGame();
            }
            catch (Exception ex)
            {
                logger.Error("button18_Click: " + ex.ToString());
            }
        }

        private void button25_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView5.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView5.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Picture");
                        dataGridView5.Rows[dataGridView5.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Picture");
                        dataGridView5.Rows[dataGridView5.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button25_Click: " + ex.ToString());
            }
        }

        private void button27_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView5.CurrentRow.Index >= 0)
                {
                    pictureBox6.Image = null;
                    string sFileName = dataGridView5.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "ScoreCenter");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView5.Rows.Remove(dataGridView5.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button27_Click: " + ex.ToString());
            } 
        }

        private void button26_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your picture fanart folder to be deleted.", "Delete All Picture Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDPicture = 0;
                    Utils.GetDbm().DeleteAllFanart("Picture");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\pictures";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable5.Rows.Clear();
                    myDataTable5.AcceptChanges();
                    label24.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Picture");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button26_Click: " + ex.ToString());
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Picture");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button24_Click: " + ex.ToString());
            }
        }

        private void button23_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Picture");
                ImportLocalFanartAtStartup();
                UpdateFanartTablePicture();
            }
            catch (Exception ex)
            {
                logger.Error("button23_Click: " + ex.ToString());
            }
        }

        private void button30_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView6.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView6.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "Plugin");
                        dataGridView6.Rows[dataGridView6.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "Plugin");
                        dataGridView6.Rows[dataGridView6.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button30_Click: " + ex.ToString());
            }
        }

        private void button32_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView6.CurrentRow.Index >= 0)
                {
                    pictureBox7.Image = null;
                    string sFileName = dataGridView6.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "Plugin");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView6.Rows.Remove(dataGridView6.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button32_Click: " + ex.ToString());
            } 
        }

        private void button31_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your plugins fanart folder to be deleted.", "Delete All Plugin Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDPlugin = 0;
                    Utils.GetDbm().DeleteAllFanart("Plugin");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\plugins";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable6.Rows.Clear();
                    myDataTable6.AcceptChanges();
                    label26.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("Plugin");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button31_Click: " + ex.ToString());
            }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("Plugin");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button29_Click: " + ex.ToString());
            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("Plugin");
                ImportLocalFanartAtStartup();
                UpdateFanartTablePlugin();
            }
            catch (Exception ex)
            {
                logger.Error("button28_Click: " + ex.ToString());
            }
        }

        private void button35_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7.CurrentRow.Index >= 0)
                {
                    string sFileName = dataGridView7.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView7.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, false, "TV");
                        dataGridView7.Rows[dataGridView7.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        Utils.GetDbm().EnableFanartRandom(sFileName, true, "TV");
                        dataGridView7.Rows[dataGridView7.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("button20_Click: " + ex.ToString());
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView7.CurrentRow.Index >= 0)
                {
                    pictureBox8.Image = null;
                    string sFileName = dataGridView7.CurrentRow.Cells[3].Value.ToString();
                    Utils.GetDbm().DeleteFanart(sFileName, "TV");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView7.Rows.Remove(dataGridView7.CurrentRow);
                }
            }
            catch (Exception ex)
            {
                logger.Error("button37_Click: " + ex.ToString());
            } 
        }

        private void button36_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your tv fanart folder to be deleted.", "Delete All TV Fanart", MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    MessageBox.Show("Operation was aborted!");
                }

                if (result == DialogResult.Yes)
                {
                    lastIDTV = 0;
                    Utils.GetDbm().DeleteAllFanart("TV");
                    string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\tv";
                    string[] dirs = Directory.GetFiles(path, "*.jpg");
                    foreach (string dir in dirs)
                    {
                        File.Delete(dir);
                    }
                    myDataTable7.Rows.Clear();
                    myDataTable7.AcceptChanges();
                    label28.Text = String.Empty + Utils.GetDbm().GetTotalRandomInFanartDatabase("TV");
                    MessageBox.Show("Done!");
                }
            }
            catch (Exception ex)
            {
                logger.Error("button36_Click: " + ex.ToString());
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            try
            {
                int i = Utils.GetDbm().SyncDatabase("TV");
                MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
            }
            catch (Exception ex)
            {
                logger.Error("button34_Click: " + ex.ToString());
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            try
            {
                ImportLocalFanart("TV");
                ImportLocalFanartAtStartup();
                UpdateFanartTableTV();
            }
            catch (Exception ex)
            {
                logger.Error("button33_Click: " + ex.ToString());
            }
        }

        private void button38_Click(object sender, EventArgs e)
        {
            try
            {
                myDataTable8 = new DataTable();
                myDataTable8.Columns.Add("Artist");
                myDataTable8.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = myDataTable8;
                UpdateFanartTableMusicOverview();
                dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView8.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dataGridView8.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            catch (Exception ex)
            {
                logger.Error("button38_Click: " + ex.ToString());
                myDataTable8 = new DataTable();
                myDataTable8.Columns.Add("Artist");
                myDataTable8.Columns.Add("Fanart Images (#)");
                dataGridView8.DataSource = myDataTable8;
                dataGridView8.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
        }
    }
}
