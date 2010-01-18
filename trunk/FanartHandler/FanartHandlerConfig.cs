﻿using System;
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
using SQLite.NET;
using System.IO;

namespace FanartHandler
{
    public partial class FanartHandlerConfig : Form
    {
        private DatabaseManager dbm;
        private string useArtist = null;
        private string useAlbum = null;
        private string useArtistDisabled = null;
        private string useAlbumDisabled = null;
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
            if ((checkBoxXFactorFanart.Checked == false) && (checkBoxThumbsAlbumDisabled.Checked == true && checkBoxThumbsArtistDisabled.Checked == true))
                sout = false;
            if (checkBoxThumbsAlbum.Checked == false && checkBoxXFactorFanart.Checked == false && checkBoxThumbsArtistDisabled.Checked == true)
                sout = false;
            if (checkBoxThumbsArtist.Checked == false && checkBoxXFactorFanart.Checked == false && checkBoxThumbsAlbumDisabled.Checked == true)
                sout = false;

            return sout;
        }
        
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
/*            if (tabControl1.SelectedIndex == 2 || tabControl1.SelectedIndex == 3)
            {
                buttonSave.Visible = false;
            }
            else
            {
                buttonSave.Visible = true;
            }*/
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (checkValidity())
            {
                using (MediaPortal.Profile.Settings xmlwriter = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
                {
                    xmlwriter.SetValue("FanartHandler", "useFanart", checkBoxXFactorFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useAlbum", checkBoxThumbsAlbum.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useArtist", checkBoxThumbsArtist.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useAlbumDisabled", checkBoxThumbsAlbumDisabled.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useArtistDisabled", checkBoxThumbsArtistDisabled.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useOverlayFanart", checkBoxOverlayFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useMusicFanart", checkBoxEnableMusicFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useVideoFanart", checkBoxEnableVideoFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "useScoreCenterFanart", checkBoxEnableScoreCenterFanart.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "imageInterval", comboBoxInterval.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "minResolution", comboBoxMinResolution.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "defaultBackdrop", textBoxDefaultBackdrop.Text);
                    xmlwriter.SetValue("FanartHandler", "scraperMaxImages", comboBoxMaxImages.SelectedItem);
                    xmlwriter.SetValue("FanartHandler", "scraperMusicPlaying", checkBoxScraperMusicPlaying.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperMPDatabase", checkBoxEnableScraperMPDatabase.Checked ? true : false);
                    xmlwriter.SetValue("FanartHandler", "scraperInterval", comboBoxScraperInterval.SelectedItem);
                }
                MessageBox.Show("Settings has been successfully saved!");
                this.Close();
            }
            else
            {
                MessageBox.Show("Error: You have to select at least on of the checkboxes under headline \"Selected Fanart Sources\". Also you cannot disable both album and artist thumbs if you also have disabled fanart. If you do not want to use fanart you still have to check at least one of the checkboxes and the disable the plugin.");
            }
        }

        private void FanartHandlerConfig_Load(object sender, EventArgs e)
        {
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
            comboBoxMinResolution.Items.Add("200x200");
            comboBoxMinResolution.Items.Add("400x400");
            comboBoxMinResolution.Items.Add("640x360");
            comboBoxMinResolution.Items.Add("960x540");
            comboBoxMinResolution.Items.Add("1024x576");
            comboBoxMinResolution.Items.Add("1280x720");
            comboBoxMinResolution.Items.Add("1920x1080");

            using (MediaPortal.Profile.Settings xmlreader = new MediaPortal.Profile.Settings(Config.GetFile(Config.Dir.Config, "FanartHandler.xml")))
            {
                useFanart = xmlreader.GetValueAsString("FanartHandler", "useFanart", "");
                useAlbum = xmlreader.GetValueAsString("FanartHandler", "useAlbum", "");
                useArtist = xmlreader.GetValueAsString("FanartHandler", "useArtist", "");
                useAlbumDisabled = xmlreader.GetValueAsString("FanartHandler", "useAlbumDisabled", "");
                useArtistDisabled = xmlreader.GetValueAsString("FanartHandler", "useArtistDisabled", "");
                useOverlayFanart = xmlreader.GetValueAsString("FanartHandler", "useOverlayFanart", "");
                useMusicFanart = xmlreader.GetValueAsString("FanartHandler", "useMusicFanart", "");
                useVideoFanart = xmlreader.GetValueAsString("FanartHandler", "useVideoFanart", "");
                useScoreCenterFanart = xmlreader.GetValueAsString("FanartHandler", "useScoreCenterFanart", "");
                imageInterval = xmlreader.GetValueAsString("FanartHandler", "imageInterval", "");
                minResolution = xmlreader.GetValueAsString("FanartHandler", "minResolution", "");
                defaultBackdrop = xmlreader.GetValueAsString("FanartHandler", "defaultBackdrop", "");
                scraperMaxImages = xmlreader.GetValueAsString("FanartHandler", "scraperMaxImages", "");
                scraperMusicPlaying = xmlreader.GetValueAsString("FanartHandler", "scraperMusicPlaying", "");
                scraperMPDatabase = xmlreader.GetValueAsString("FanartHandler", "scraperMPDatabase", "");
                scraperInterval = xmlreader.GetValueAsString("FanartHandler", "scraperInterval", "");                         
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
                if (useAlbumDisabled != null && useAlbumDisabled.Length > 0)
                {
                    if (useAlbumDisabled.Equals("True"))
                        checkBoxThumbsAlbumDisabled.Checked = true;
                    else
                        checkBoxThumbsAlbumDisabled.Checked = false;
                }
                else
                {
                    useAlbumDisabled = "True";
                    checkBoxThumbsAlbumDisabled.Checked = true;
                }
                if (useArtistDisabled != null && useArtistDisabled.Length > 0)
                {
                    if (useArtistDisabled.Equals("True"))
                        checkBoxThumbsArtistDisabled.Checked = true;
                    else
                        checkBoxThumbsArtistDisabled.Checked = false;
                }
                else
                {
                    useArtistDisabled = "True";
                    checkBoxThumbsArtistDisabled.Checked = true;
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
                    defaultBackdrop = "background.png";
                    textBoxDefaultBackdrop.Text = "background.png";
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
                dbm = new DatabaseManager();
                dbm.initDB(Convert.ToInt32(scraperMaxImages), scraperMPDatabase, scraperMusicPlaying);                

            DataTable d = new DataTable();
            d.Columns.Add("Artist");
            d.Columns.Add("Enabled");
            d.Columns.Add("Image");
            d.Columns.Add("Image Path");
            SQLiteResultSet result = dbm.getDataForTable();
            for (int i = 0; i < result.Rows.Count; i++)
            {                
                DataRow myDataRow = d.NewRow();
                myDataRow["Artist"] = result.GetField(i, 0);
                myDataRow["Enabled"] = result.GetField(i, 1);
                myDataRow["Image"] = getFilenameOnly(result.GetField(i, 2));
                myDataRow["Image Path"] = result.GetField(i, 2);
                d.Rows.Add(myDataRow);
            }
            result = null;
            dataGridView1.DataSource = d;
            dataGridView1.AutoResizeColumn(1, DataGridViewAutoSizeColumnMode.AllCells);
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
            OpenFileDialog openFD = new OpenFileDialog();
            //FolderBrowserDialog
            openFD.InitialDirectory = GUIGraphicsContext.Skin + @"\";
            openFD.Title = "Select Default Backdrop (used when no fanart is found)";
            openFD.FileName = textBoxDefaultBackdrop.Text;
            openFD.Filter = "Image Files(*.JPG;*.PNG)|*.JPG;*.PNG";
            if (openFD.ShowDialog() == DialogResult.Cancel)
            {
            }
            else
            {
                textBoxDefaultBackdrop.Text = openFD.FileName;
            }
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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset the initial scrap flag? This will cause a complete new music scrape on next MP startup.", "Reset Initial Scrape", MessageBoxButtons.YesNo);
            if (result == DialogResult.No) 
            {
                MessageBox.Show("Operation was aborted!");
            }

            if (result == DialogResult.Yes) 
            {
                dbm.ResetInitialScrape();
                MessageBox.Show("Done!");
            }
            
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {                
                Bitmap img = (Bitmap)Image.FromFile(dataGridView1[3, e.RowIndex].Value.ToString());
                Size imgSize = new Size(158, 89);
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
            catch //(Exception ex)
            {
            //    MessageBox.Show(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index > 0)
                {
                    pictureBox1.Image = null;
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    dbm.DeleteFanart(sFileName, "MusicFanart");
                    if (File.Exists(sFileName) == true)
                    {
                        File.Delete(sFileName);
                    }
                    dataGridView1.Rows.Remove(dataGridView1.CurrentRow);
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }                        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete all fanart? This will cause all fanart stored in your music fanart folder to be deleted.", "Delete All Music Fanart", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                MessageBox.Show("Operation was aborted!");
            }

            if (result == DialogResult.Yes)
            {
                dbm.DeleteAllFanart("MusicFanart");
                string path = Config.GetFolder(Config.Dir.Config) + @"\thumbs\Skin FanArt\music";
                string[] dirs = Directory.GetFiles(path, "*.jpg");
                foreach (string dir in dirs)
                {
                    File.Delete(dir);
                }
                int rowCount = dataGridView1.Rows.Count;
                for (int i = 0; i < rowCount; i++)
                {
                    dataGridView1.Rows.RemoveAt(i);
                }
                dbm.ResetInitialScrape();
                MessageBox.Show("Done!");
            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.CurrentRow.Index > 0)
                {                    
                    string sFileName = dataGridView1.CurrentRow.Cells[3].Value.ToString();
                    string enabled = dataGridView1.CurrentRow.Cells[1].Value.ToString();
                    if (enabled != null && enabled.Equals("True"))
                    {
                        dbm.EnableFanart(sFileName, false);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "False";
                    }
                    else
                    {
                        dbm.EnableFanart(sFileName, true);
                        dataGridView1.Rows[dataGridView1.CurrentRow.Index].Cells[1].Value = "True";
                    }
                }
            }
            catch //(Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int i = dbm.syncDatabase();
            MessageBox.Show("Successfully synchronised your fanart database. Removed " + i + " entries from your fanart database.");
        }

        private void comboBoxScraperInterval_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
