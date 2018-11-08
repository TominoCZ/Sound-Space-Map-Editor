namespace Blox_Saber_Editor
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnLoadSong = new System.Windows.Forms.Button();
            this.tbID = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.sfd = new System.Windows.Forms.SaveFileDialog();
            this.ofd = new System.Windows.Forms.OpenFileDialog();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tbVolume = new System.Windows.Forms.TrackBar();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnPrev = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.nudTimeStamp = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.pnlMap = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblNote = new System.Windows.Forms.Label();
            this.timeline1 = new Blox_Saber_Editor.Timeline();
            this.chbSmooth = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeStamp)).BeginInit();
            this.panel4.SuspendLayout();
            this.pnlMap.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadSong
            // 
            this.btnLoadSong.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadSong.Location = new System.Drawing.Point(5, 79);
            this.btnLoadSong.Margin = new System.Windows.Forms.Padding(5);
            this.btnLoadSong.Name = "btnLoadSong";
            this.btnLoadSong.Size = new System.Drawing.Size(158, 31);
            this.btnLoadSong.TabIndex = 0;
            this.btnLoadSong.Text = "LOAD";
            this.btnLoadSong.UseVisualStyleBackColor = true;
            this.btnLoadSong.Click += new System.EventHandler(this.btnLoadSong_Click);
            // 
            // tbID
            // 
            this.tbID.Location = new System.Drawing.Point(5, 30);
            this.tbID.Margin = new System.Windows.Forms.Padding(5);
            this.tbID.Name = "tbID";
            this.tbID.Size = new System.Drawing.Size(158, 29);
            this.tbID.TabIndex = 1;
            this.tbID.Text = "0";
            this.tbID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "Music";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.tbID);
            this.panel1.Controls.Add(this.btnLoadSong);
            this.panel1.Location = new System.Drawing.Point(12, 33);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(170, 117);
            this.panel1.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "Asset ID:";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Controls.Add(this.btnCopy);
            this.panel2.Controls.Add(this.btnClear);
            this.panel2.Controls.Add(this.btnLoadFile);
            this.panel2.Location = new System.Drawing.Point(356, 33);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(170, 117);
            this.panel2.TabIndex = 4;
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(88, 5);
            this.btnSave.Margin = new System.Windows.Forms.Padding(5);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 39);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "SAVE";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(5, 46);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(5);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(158, 31);
            this.btnCopy.TabIndex = 0;
            this.btnCopy.Text = "COPY";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnClear
            // 
            this.btnClear.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.Location = new System.Drawing.Point(5, 79);
            this.btnClear.Margin = new System.Windows.Forms.Padding(5);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(158, 31);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "CLEAR";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLoadFile.Location = new System.Drawing.Point(5, 5);
            this.btnLoadFile.Margin = new System.Windows.Forms.Padding(5);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(75, 39);
            this.btnLoadFile.TabIndex = 0;
            this.btnLoadFile.Text = "LOAD";
            this.btnLoadFile.UseVisualStyleBackColor = true;
            this.btnLoadFile.Click += new System.EventHandler(this.btnLoadFile_Click);
            // 
            // sfd
            // 
            this.sfd.Filter = "Text file (*.txt)|*.txt";
            // 
            // ofd
            // 
            this.ofd.Filter = "Text file (*.txt)|*.txt";
            // 
            // panel3
            // 
            this.panel3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.tbVolume);
            this.panel3.Controls.Add(this.btnStop);
            this.panel3.Controls.Add(this.btnPlay);
            this.panel3.Controls.Add(this.btnPause);
            this.panel3.Location = new System.Drawing.Point(181, 33);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(176, 117);
            this.panel3.TabIndex = 5;
            // 
            // tbVolume
            // 
            this.tbVolume.AutoSize = false;
            this.tbVolume.Location = new System.Drawing.Point(0, 0);
            this.tbVolume.Maximum = 100;
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(175, 21);
            this.tbVolume.TabIndex = 2;
            this.tbVolume.TickFrequency = 0;
            this.tbVolume.TickStyle = System.Windows.Forms.TickStyle.None;
            this.tbVolume.Value = 50;
            this.tbVolume.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStop.Location = new System.Drawing.Point(92, 36);
            this.btnStop.Margin = new System.Windows.Forms.Padding(5);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(77, 33);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "STOP";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Enabled = false;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPlay.Location = new System.Drawing.Point(5, 36);
            this.btnPlay.Margin = new System.Windows.Forms.Padding(5);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(77, 33);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "PLAY";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnPause
            // 
            this.btnPause.Enabled = false;
            this.btnPause.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPause.Location = new System.Drawing.Point(5, 79);
            this.btnPause.Margin = new System.Windows.Forms.Padding(5);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(164, 31);
            this.btnPause.TabIndex = 0;
            this.btnPause.Text = "PAUSE";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label3.Location = new System.Drawing.Point(181, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(176, 21);
            this.label3.TabIndex = 2;
            this.label3.Text = "Playback";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(356, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(170, 21);
            this.label4.TabIndex = 2;
            this.label4.Text = "Map";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPrev
            // 
            this.btnPrev.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnPrev.FlatAppearance.BorderSize = 0;
            this.btnPrev.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPrev.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrev.Location = new System.Drawing.Point(144, 252);
            this.btnPrev.Name = "btnPrev";
            this.btnPrev.Size = new System.Drawing.Size(38, 162);
            this.btnPrev.TabIndex = 6;
            this.btnPrev.Text = "<";
            this.btnPrev.UseVisualStyleBackColor = true;
            this.btnPrev.Click += new System.EventHandler(this.btnPrev_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnNext.FlatAppearance.BorderSize = 0;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNext.Location = new System.Drawing.Point(356, 252);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(38, 162);
            this.btnNext.TabIndex = 6;
            this.btnNext.Text = ">";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // nudTimeStamp
            // 
            this.nudTimeStamp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.nudTimeStamp.Location = new System.Drawing.Point(274, 420);
            this.nudTimeStamp.Maximum = new decimal(new int[] {
            1215752191,
            23,
            0,
            0});
            this.nudTimeStamp.Name = "nudTimeStamp";
            this.nudTimeStamp.Size = new System.Drawing.Size(76, 29);
            this.nudTimeStamp.TabIndex = 7;
            this.nudTimeStamp.ValueChanged += new System.EventHandler(this.nudTimeStamp_ValueChanged);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(0, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(50, 50);
            this.button1.TabIndex = 9;
            this.button1.Text = "Q";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button2.FlatAppearance.BorderSize = 0;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(56, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(50, 50);
            this.button2.TabIndex = 9;
            this.button2.Text = "W";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Location = new System.Drawing.Point(112, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(50, 50);
            this.button3.TabIndex = 9;
            this.button3.Text = "E";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button4.FlatAppearance.BorderSize = 0;
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button4.Location = new System.Drawing.Point(0, 56);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(50, 50);
            this.button4.TabIndex = 9;
            this.button4.Text = "A";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.Location = new System.Drawing.Point(56, 56);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(50, 50);
            this.button5.TabIndex = 9;
            this.button5.Text = "S";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button6.FlatAppearance.BorderSize = 0;
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.Location = new System.Drawing.Point(112, 56);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(50, 50);
            this.button6.TabIndex = 9;
            this.button6.Text = "D";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button7.FlatAppearance.BorderSize = 0;
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7.Location = new System.Drawing.Point(0, 112);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(50, 50);
            this.button7.TabIndex = 9;
            this.button7.Text = "Z";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button8.FlatAppearance.BorderSize = 0;
            this.button8.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button8.Location = new System.Drawing.Point(56, 112);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(50, 50);
            this.button8.TabIndex = 9;
            this.button8.Text = "X";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.SystemColors.ControlLight;
            this.button9.FlatAppearance.BorderSize = 0;
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button9.Location = new System.Drawing.Point(112, 112);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(50, 50);
            this.button9.TabIndex = 9;
            this.button9.Text = "C";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.gridButton_Click);
            // 
            // panel4
            // 
            this.panel4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panel4.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.panel4.Controls.Add(this.pnlMap);
            this.panel4.Controls.Add(this.button7);
            this.panel4.Controls.Add(this.button1);
            this.panel4.Controls.Add(this.button2);
            this.panel4.Controls.Add(this.button4);
            this.panel4.Controls.Add(this.button6);
            this.panel4.Controls.Add(this.button8);
            this.panel4.Controls.Add(this.button3);
            this.panel4.Controls.Add(this.button9);
            this.panel4.Controls.Add(this.button5);
            this.panel4.Location = new System.Drawing.Point(188, 252);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(162, 162);
            this.panel4.TabIndex = 10;
            // 
            // pnlMap
            // 
            this.pnlMap.Controls.Add(this.label7);
            this.pnlMap.Controls.Add(this.label6);
            this.pnlMap.Location = new System.Drawing.Point(0, 0);
            this.pnlMap.Name = "pnlMap";
            this.pnlMap.Size = new System.Drawing.Size(162, 162);
            this.pnlMap.TabIndex = 13;
            this.pnlMap.Visible = false;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(0, 106);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(156, 50);
            this.label7.TabIndex = 0;
            this.label7.Text = "ESC or BACKSPACE to cancel";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(0, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(156, 156);
            this.label6.TabIndex = 0;
            this.label6.Text = "Press a key";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(184, 422);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 21);
            this.label5.TabIndex = 11;
            this.label5.Text = "Time [ms]:";
            // 
            // lblNote
            // 
            this.lblNote.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblNote.Location = new System.Drawing.Point(189, 228);
            this.lblNote.Name = "lblNote";
            this.lblNote.Size = new System.Drawing.Size(161, 21);
            this.lblNote.TabIndex = 2;
            this.lblNote.Text = "Note";
            this.lblNote.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timeline1
            // 
            this.timeline1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timeline1.CurrentTime = System.TimeSpan.Parse("00:00:00");
            this.timeline1.Location = new System.Drawing.Point(12, 155);
            this.timeline1.Margin = new System.Windows.Forms.Padding(2);
            this.timeline1.Name = "timeline1";
            this.timeline1.Size = new System.Drawing.Size(513, 71);
            this.timeline1.Smooth = false;
            this.timeline1.TabIndex = 8;
            this.timeline1.TotalTime = System.TimeSpan.Parse("00:00:00");
            this.timeline1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.timeline1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            // 
            // chbSmooth
            // 
            this.chbSmooth.AutoSize = true;
            this.chbSmooth.Location = new System.Drawing.Point(12, 231);
            this.chbSmooth.Name = "chbSmooth";
            this.chbSmooth.Size = new System.Drawing.Size(89, 25);
            this.chbSmooth.TabIndex = 12;
            this.chbSmooth.Text = "Smooth";
            this.chbSmooth.UseVisualStyleBackColor = true;
            this.chbSmooth.CheckedChanged += new System.EventHandler(this.chbSmooth_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 461);
            this.Controls.Add(this.chbSmooth);
            this.Controls.Add(this.timeline1);
            this.Controls.Add(this.nudTimeStamp);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnPrev);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblNote);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.label5);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(554, 500);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Blox Saber Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tbVolume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTimeStamp)).EndInit();
            this.panel4.ResumeLayout(false);
            this.pnlMap.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnLoadSong;
        private System.Windows.Forms.TextBox tbID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.SaveFileDialog sfd;
        private System.Windows.Forms.OpenFileDialog ofd;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.TrackBar tbVolume;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnPrev;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.NumericUpDown nudTimeStamp;
        private Timeline timeline1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.CheckBox chbSmooth;
        private System.Windows.Forms.Panel pnlMap;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnCopy;
    }
}