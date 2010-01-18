namespace FanartHandler
{
    partial class FanartHandlerConfig
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FanartHandlerConfig));
            this.buttonSave = new System.Windows.Forms.Button();
            this.checkBoxThumbsArtist = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxThumbsAlbum = new System.Windows.Forms.CheckBox();
            this.checkBoxXFactorFanart = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxOverlayFanart = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxThumbsArtistDisabled = new System.Windows.Forms.CheckBox();
            this.checkBoxThumbsAlbumDisabled = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableMusicFanart = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableVideoFanart = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.checkBoxEnableScoreCenterFanart = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxInterval = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.textBoxDefaultBackdrop = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxMinResolution = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.comboBoxScraperInterval = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxMaxImages = new System.Windows.Forms.ComboBox();
            this.checkBoxScraperMusicPlaying = new System.Windows.Forms.CheckBox();
            this.checkBoxEnableScraperMPDatabase = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button2 = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(621, 476);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(94, 28);
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "Save and Close";
            this.toolTip1.SetToolTip(this.buttonSave, "Press the save button to save all changes that\r\nyou have done. Pressing the X at " +
                    "top right will\r\nclose without saving.");
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // checkBoxThumbsArtist
            // 
            this.checkBoxThumbsArtist.AutoSize = true;
            this.checkBoxThumbsArtist.Checked = true;
            this.checkBoxThumbsArtist.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThumbsArtist.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxThumbsArtist.Location = new System.Drawing.Point(9, 44);
            this.checkBoxThumbsArtist.Name = "checkBoxThumbsArtist";
            this.checkBoxThumbsArtist.Size = new System.Drawing.Size(131, 20);
            this.checkBoxThumbsArtist.TabIndex = 1;
            this.checkBoxThumbsArtist.Text = "MP Artist Thumbs";
            this.toolTip1.SetToolTip(this.checkBoxThumbsArtist, resources.GetString("checkBoxThumbsArtist.ToolTip"));
            this.checkBoxThumbsArtist.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(183, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Selected Fanart Sources:";
            // 
            // checkBoxThumbsAlbum
            // 
            this.checkBoxThumbsAlbum.AutoSize = true;
            this.checkBoxThumbsAlbum.Checked = true;
            this.checkBoxThumbsAlbum.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThumbsAlbum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxThumbsAlbum.Location = new System.Drawing.Point(9, 67);
            this.checkBoxThumbsAlbum.Name = "checkBoxThumbsAlbum";
            this.checkBoxThumbsAlbum.Size = new System.Drawing.Size(140, 20);
            this.checkBoxThumbsAlbum.TabIndex = 3;
            this.checkBoxThumbsAlbum.Text = "MP Album Thumbs";
            this.toolTip1.SetToolTip(this.checkBoxThumbsAlbum, resources.GetString("checkBoxThumbsAlbum.ToolTip"));
            this.checkBoxThumbsAlbum.UseVisualStyleBackColor = true;
            this.checkBoxThumbsAlbum.CheckedChanged += new System.EventHandler(this.checkBoxThumbsAlbum_CheckedChanged);
            // 
            // checkBoxXFactorFanart
            // 
            this.checkBoxXFactorFanart.AutoSize = true;
            this.checkBoxXFactorFanart.Checked = true;
            this.checkBoxXFactorFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxXFactorFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxXFactorFanart.Location = new System.Drawing.Point(9, 90);
            this.checkBoxXFactorFanart.Name = "checkBoxXFactorFanart";
            this.checkBoxXFactorFanart.Size = new System.Drawing.Size(160, 20);
            this.checkBoxXFactorFanart.TabIndex = 4;
            this.checkBoxXFactorFanart.Text = "Music Fanart Directory";
            this.toolTip1.SetToolTip(this.checkBoxXFactorFanart, resources.GetString("checkBoxXFactorFanart.ToolTip"));
            this.checkBoxXFactorFanart.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(6, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(252, 16);
            this.label2.TabIndex = 5;
            this.label2.Text = "Use Fanart In Now Playing Overlay:";
            // 
            // checkBoxOverlayFanart
            // 
            this.checkBoxOverlayFanart.AutoSize = true;
            this.checkBoxOverlayFanart.Checked = true;
            this.checkBoxOverlayFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxOverlayFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxOverlayFanart.Location = new System.Drawing.Point(9, 148);
            this.checkBoxOverlayFanart.Name = "checkBoxOverlayFanart";
            this.checkBoxOverlayFanart.Size = new System.Drawing.Size(156, 20);
            this.checkBoxOverlayFanart.TabIndex = 6;
            this.checkBoxOverlayFanart.Text = "Use Fanart In Overlay";
            this.toolTip1.SetToolTip(this.checkBoxOverlayFanart, "This option enables fanart as background in the now\r\nplaying window in MediaPorta" +
                    "l if your current skin\r\nsupports it.");
            this.checkBoxOverlayFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxThumbsArtistDisabled);
            this.groupBox1.Controls.Add(this.checkBoxThumbsAlbumDisabled);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBoxOverlayFanart);
            this.groupBox1.Controls.Add(this.checkBoxThumbsArtist);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkBoxThumbsAlbum);
            this.groupBox1.Controls.Add(this.checkBoxXFactorFanart);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 7);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(358, 266);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Music Fanart Options";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // checkBoxThumbsArtistDisabled
            // 
            this.checkBoxThumbsArtistDisabled.AutoSize = true;
            this.checkBoxThumbsArtistDisabled.Checked = true;
            this.checkBoxThumbsArtistDisabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThumbsArtistDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxThumbsArtistDisabled.Location = new System.Drawing.Point(9, 205);
            this.checkBoxThumbsArtistDisabled.Name = "checkBoxThumbsArtistDisabled";
            this.checkBoxThumbsArtistDisabled.Size = new System.Drawing.Size(131, 20);
            this.checkBoxThumbsArtistDisabled.TabIndex = 8;
            this.checkBoxThumbsArtistDisabled.Text = "MP Artist Thumbs";
            this.toolTip1.SetToolTip(this.checkBoxThumbsArtistDisabled, resources.GetString("checkBoxThumbsArtistDisabled.ToolTip"));
            this.checkBoxThumbsArtistDisabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxThumbsAlbumDisabled
            // 
            this.checkBoxThumbsAlbumDisabled.AutoSize = true;
            this.checkBoxThumbsAlbumDisabled.Checked = true;
            this.checkBoxThumbsAlbumDisabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThumbsAlbumDisabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxThumbsAlbumDisabled.Location = new System.Drawing.Point(9, 228);
            this.checkBoxThumbsAlbumDisabled.Name = "checkBoxThumbsAlbumDisabled";
            this.checkBoxThumbsAlbumDisabled.Size = new System.Drawing.Size(140, 20);
            this.checkBoxThumbsAlbumDisabled.TabIndex = 9;
            this.checkBoxThumbsAlbumDisabled.Text = "MP Album Thumbs";
            this.toolTip1.SetToolTip(this.checkBoxThumbsAlbumDisabled, resources.GetString("checkBoxThumbsAlbumDisabled.ToolTip"));
            this.checkBoxThumbsAlbumDisabled.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 186);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(290, 16);
            this.label4.TabIndex = 7;
            this.label4.Text = "Disable Fanart Soures (Random Images)";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBoxEnableMusicFanart);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(403, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(341, 56);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Music Plugins Fanart Options";
            // 
            // checkBoxEnableMusicFanart
            // 
            this.checkBoxEnableMusicFanart.AutoSize = true;
            this.checkBoxEnableMusicFanart.Checked = true;
            this.checkBoxEnableMusicFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableMusicFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableMusicFanart.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableMusicFanart.Name = "checkBoxEnableMusicFanart";
            this.checkBoxEnableMusicFanart.Size = new System.Drawing.Size(226, 20);
            this.checkBoxEnableMusicFanart.TabIndex = 7;
            this.checkBoxEnableMusicFanart.Text = "Enable Fanart For Selected Items";
            this.toolTip1.SetToolTip(this.checkBoxEnableMusicFanart, resources.GetString("checkBoxEnableMusicFanart.ToolTip"));
            this.checkBoxEnableMusicFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.checkBoxEnableVideoFanart);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox4.Location = new System.Drawing.Point(403, 75);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(341, 56);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Video Plugins Fanart Options";
            // 
            // checkBoxEnableVideoFanart
            // 
            this.checkBoxEnableVideoFanart.AutoSize = true;
            this.checkBoxEnableVideoFanart.Checked = true;
            this.checkBoxEnableVideoFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableVideoFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableVideoFanart.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableVideoFanart.Name = "checkBoxEnableVideoFanart";
            this.checkBoxEnableVideoFanart.Size = new System.Drawing.Size(226, 20);
            this.checkBoxEnableVideoFanart.TabIndex = 7;
            this.checkBoxEnableVideoFanart.Text = "Enable Fanart For Selected Items";
            this.toolTip1.SetToolTip(this.checkBoxEnableVideoFanart, "Check this option to enable fanart for selected items \r\nwhen browsing your movies" +
                    " using the myVideos plugin.");
            this.checkBoxEnableVideoFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.checkBoxEnableScoreCenterFanart);
            this.groupBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox5.Location = new System.Drawing.Point(404, 145);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(341, 56);
            this.groupBox5.TabIndex = 11;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "myScoreCenter Fanart Options";
            // 
            // checkBoxEnableScoreCenterFanart
            // 
            this.checkBoxEnableScoreCenterFanart.AutoSize = true;
            this.checkBoxEnableScoreCenterFanart.Checked = true;
            this.checkBoxEnableScoreCenterFanart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxEnableScoreCenterFanart.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableScoreCenterFanart.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableScoreCenterFanart.Name = "checkBoxEnableScoreCenterFanart";
            this.checkBoxEnableScoreCenterFanart.Size = new System.Drawing.Size(238, 20);
            this.checkBoxEnableScoreCenterFanart.TabIndex = 7;
            this.checkBoxEnableScoreCenterFanart.Text = "Enable Fanart For Selected Genres";
            this.toolTip1.SetToolTip(this.checkBoxEnableScoreCenterFanart, "Check this option to enable fanart for selected genres \r\nwhen browsing your sport" +
                    "s results in the myScoreCenter\r\nplugin.");
            this.checkBoxEnableScoreCenterFanart.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.comboBoxInterval);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(403, 279);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(342, 61);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Show Each Image For";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(142, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 16);
            this.label3.TabIndex = 1;
            this.label3.Text = "seconds";
            // 
            // comboBoxInterval
            // 
            this.comboBoxInterval.FormattingEnabled = true;
            this.comboBoxInterval.Location = new System.Drawing.Point(12, 25);
            this.comboBoxInterval.Name = "comboBoxInterval";
            this.comboBoxInterval.Size = new System.Drawing.Size(124, 28);
            this.comboBoxInterval.TabIndex = 0;
            this.toolTip1.SetToolTip(this.comboBoxInterval, "Select the number of seconds each image will be displayed\r\nbefore trying to switc" +
                    "h to next image for selected or\r\nplayed artist (or next randomg image or next se" +
                    "lected \r\nmove and so on).");
            this.comboBoxInterval.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.textBoxDefaultBackdrop);
            this.groupBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox6.Location = new System.Drawing.Point(9, 279);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(358, 61);
            this.groupBox6.TabIndex = 13;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Default Backdrop";
            this.groupBox6.Visible = false;
            // 
            // textBoxDefaultBackdrop
            // 
            this.textBoxDefaultBackdrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxDefaultBackdrop.Location = new System.Drawing.Point(11, 29);
            this.textBoxDefaultBackdrop.Name = "textBoxDefaultBackdrop";
            this.textBoxDefaultBackdrop.Size = new System.Drawing.Size(341, 22);
            this.textBoxDefaultBackdrop.TabIndex = 2;
            this.textBoxDefaultBackdrop.TextChanged += new System.EventHandler(this.textBoxDefaultBackdrop_TextChanged);
            this.textBoxDefaultBackdrop.Click += new System.EventHandler(this.textBoxDefaultBackdrop_Click);
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label5);
            this.groupBox7.Controls.Add(this.comboBoxMinResolution);
            this.groupBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox7.Location = new System.Drawing.Point(402, 212);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(342, 61);
            this.groupBox7.TabIndex = 14;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Minimum Resolution For Images";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(227, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 16);
            this.label5.TabIndex = 1;
            this.label5.Text = "pixels";
            // 
            // comboBoxMinResolution
            // 
            this.comboBoxMinResolution.FormattingEnabled = true;
            this.comboBoxMinResolution.Location = new System.Drawing.Point(12, 25);
            this.comboBoxMinResolution.Name = "comboBoxMinResolution";
            this.comboBoxMinResolution.Size = new System.Drawing.Size(209, 28);
            this.comboBoxMinResolution.TabIndex = 0;
            this.toolTip1.SetToolTip(this.comboBoxMinResolution, resources.GetString("comboBoxMinResolution.ToolTip"));
            this.comboBoxMinResolution.SelectedIndexChanged += new System.EventHandler(this.comboBoxMinResolution_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(768, 458);
            this.tabControl1.TabIndex = 15;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.groupBox6);
            this.tabPage1.Controls.Add(this.groupBox7);
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Controls.Add(this.groupBox4);
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(760, 432);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Common Settings";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label8);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.groupBox8);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(760, 432);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Scraper Setting";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.Red;
            this.label8.Location = new System.Drawing.Point(126, 211);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(445, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "WARNING! Pressing this button will cause a complete new music scrape on next MP s" +
                "tartup.";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 206);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 23);
            this.button1.TabIndex = 11;
            this.button1.Text = "Reset Scraper";
            this.toolTip1.SetToolTip(this.button1, resources.GetString("button1.ToolTip"));
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.label13);
            this.groupBox8.Controls.Add(this.label12);
            this.groupBox8.Controls.Add(this.comboBoxScraperInterval);
            this.groupBox8.Controls.Add(this.label7);
            this.groupBox8.Controls.Add(this.label6);
            this.groupBox8.Controls.Add(this.comboBoxMaxImages);
            this.groupBox8.Controls.Add(this.checkBoxScraperMusicPlaying);
            this.groupBox8.Controls.Add(this.checkBoxEnableScraperMPDatabase);
            this.groupBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox8.Location = new System.Drawing.Point(15, 16);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(581, 171);
            this.groupBox8.TabIndex = 10;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Scraper Options";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(249, 118);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(52, 16);
            this.label13.TabIndex = 14;
            this.label13.Text = "(Hours)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(6, 118);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(102, 16);
            this.label12.TabIndex = 13;
            this.label12.Text = "Scraper Interval";
            // 
            // comboBoxScraperInterval
            // 
            this.comboBoxScraperInterval.FormattingEnabled = true;
            this.comboBoxScraperInterval.Location = new System.Drawing.Point(109, 112);
            this.comboBoxScraperInterval.Name = "comboBoxScraperInterval";
            this.comboBoxScraperInterval.Size = new System.Drawing.Size(124, 28);
            this.comboBoxScraperInterval.TabIndex = 12;
            this.toolTip1.SetToolTip(this.comboBoxScraperInterval, "Select the  number of hours between each new scraper attempt.");
            this.comboBoxScraperInterval.SelectedIndexChanged += new System.EventHandler(this.comboBoxScraperInterval_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 84);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 16);
            this.label7.TabIndex = 11;
            this.label7.Text = "Download Max";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(249, 84);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(109, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "Images Per Artist";
            // 
            // comboBoxMaxImages
            // 
            this.comboBoxMaxImages.FormattingEnabled = true;
            this.comboBoxMaxImages.Location = new System.Drawing.Point(109, 78);
            this.comboBoxMaxImages.Name = "comboBoxMaxImages";
            this.comboBoxMaxImages.Size = new System.Drawing.Size(124, 28);
            this.comboBoxMaxImages.TabIndex = 9;
            this.toolTip1.SetToolTip(this.comboBoxMaxImages, "Choose how many images the scraper will try to\r\ndownload for every artist. Choosi" +
                    "ng a higher number\r\nwill consume more harddisk space. ");
            this.comboBoxMaxImages.SelectedIndexChanged += new System.EventHandler(this.comboBoxMaxImages_SelectedIndexChanged);
            // 
            // checkBoxScraperMusicPlaying
            // 
            this.checkBoxScraperMusicPlaying.AutoSize = true;
            this.checkBoxScraperMusicPlaying.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxScraperMusicPlaying.Location = new System.Drawing.Point(9, 52);
            this.checkBoxScraperMusicPlaying.Name = "checkBoxScraperMusicPlaying";
            this.checkBoxScraperMusicPlaying.Size = new System.Drawing.Size(467, 20);
            this.checkBoxScraperMusicPlaying.TabIndex = 8;
            this.checkBoxScraperMusicPlaying.Text = "Enable Automatic Download Of Music Fanart For Artists Now Being Played";
            this.toolTip1.SetToolTip(this.checkBoxScraperMusicPlaying, resources.GetString("checkBoxScraperMusicPlaying.ToolTip"));
            this.checkBoxScraperMusicPlaying.UseVisualStyleBackColor = true;
            // 
            // checkBoxEnableScraperMPDatabase
            // 
            this.checkBoxEnableScraperMPDatabase.AutoSize = true;
            this.checkBoxEnableScraperMPDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxEnableScraperMPDatabase.Location = new System.Drawing.Point(9, 26);
            this.checkBoxEnableScraperMPDatabase.Name = "checkBoxEnableScraperMPDatabase";
            this.checkBoxEnableScraperMPDatabase.Size = new System.Drawing.Size(518, 20);
            this.checkBoxEnableScraperMPDatabase.TabIndex = 7;
            this.checkBoxEnableScraperMPDatabase.Text = "Enable Automatic Download Of Music Fanart For Artists In Your MP MusicDatabase";
            this.toolTip1.SetToolTip(this.checkBoxEnableScraperMPDatabase, resources.GetString("checkBoxEnableScraperMPDatabase.ToolTip"));
            this.checkBoxEnableScraperMPDatabase.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button5);
            this.tabPage3.Controls.Add(this.button4);
            this.tabPage3.Controls.Add(this.button3);
            this.tabPage3.Controls.Add(this.pictureBox1);
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.dataGridView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(760, 432);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Manage Fanart";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(204, 403);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(185, 23);
            this.button5.TabIndex = 5;
            this.button5.Text = "Cleanup Missing Fanart";
            this.toolTip1.SetToolTip(this.button5, "Press this button to sync fanart database and images \r\non your harddrive. Any ent" +
                    "ries in the fanart database\r\nthat has no matching image stored on your harddrive" +
                    "\r\nwill be removed.");
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(204, 374);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(185, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "Enable/Disable Selected Fanart";
            this.toolTip1.SetToolTip(this.button4, resources.GetString("button4.ToolTip"));
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(395, 403);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(185, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "Delete All Fanart";
            this.toolTip1.SetToolTip(this.button3, resources.GetString("button3.ToolTip"));
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox1.Location = new System.Drawing.Point(596, 337);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(158, 89);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(395, 374);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(185, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "Delete Selected Fanart";
            this.toolTip1.SetToolTip(this.button2, resources.GetString("button2.ToolTip"));
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 6);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(748, 325);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label11);
            this.tabPage4.Controls.Add(this.label10);
            this.tabPage4.Controls.Add(this.label9);
            this.tabPage4.Controls.Add(this.richTextBox1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(760, 432);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "About";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(659, 5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(83, 16);
            this.label11.TabIndex = 3;
            this.label11.Text = "Version RC1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(262, 47);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(158, 24);
            this.label10.TabIndex = 2;
            this.label10.Text = "Created by cul8er";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial Black", 24F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(201, 3);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(293, 44);
            this.label9.TabIndex = 1;
            this.label9.Text = "Fanart Handler";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Location = new System.Drawing.Point(6, 86);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(748, 331);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 30000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.ReshowDelay = 100;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(721, 476);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(55, 28);
            this.buttonCancel.TabIndex = 16;
            this.buttonCancel.Text = "Cancel";
            this.toolTip1.SetToolTip(this.buttonCancel, "Press the save button to save all changes that\r\nyou have done. Pressing the X at " +
                    "top right will\r\nclose without saving.");
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FanartHandlerConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 516);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FanartHandlerConfig";
            this.Text = "Fanart Handler";
            this.Load += new System.EventHandler(this.FanartHandlerConfig_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.CheckBox checkBoxThumbsArtist;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxThumbsAlbum;
        private System.Windows.Forms.CheckBox checkBoxXFactorFanart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxOverlayFanart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox checkBoxEnableMusicFanart;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.CheckBox checkBoxEnableVideoFanart;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.CheckBox checkBoxEnableScoreCenterFanart;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBoxInterval;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox textBoxDefaultBackdrop;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBoxMinResolution;
        private System.Windows.Forms.CheckBox checkBoxThumbsArtistDisabled;
        private System.Windows.Forms.CheckBox checkBoxThumbsAlbumDisabled;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.CheckBox checkBoxEnableScraperMPDatabase;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxMaxImages;
        private System.Windows.Forms.CheckBox checkBoxScraperMusicPlaying;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox comboBoxScraperInterval;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button buttonCancel;
    }
}