namespace VoiceChat
{
    partial class VoiceChat
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
            this.btnCall = new System.Windows.Forms.Button();
            this.txtCallToIP = new System.Windows.Forms.TextBox();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblCallTo = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.cmbCodecs = new System.Windows.Forms.ComboBox();
            this.lblCodec = new System.Windows.Forms.Label();
            this.btnStartServer = new System.Windows.Forms.Button();
            this.txt_log = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbRecordDevices = new System.Windows.Forms.ComboBox();
            this.btnRefrescar = new System.Windows.Forms.Button();
            this.cmbSoundChannels = new System.Windows.Forms.ComboBox();
            this.cmbBitsPerSample = new System.Windows.Forms.ComboBox();
            this.cmbSoundSamplesxsec = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnCall
            // 
            this.btnCall.Location = new System.Drawing.Point(327, 10);
            this.btnCall.Name = "btnCall";
            this.btnCall.Size = new System.Drawing.Size(130, 23);
            this.btnCall.TabIndex = 0;
            this.btnCall.Text = "Connect to server";
            this.btnCall.UseVisualStyleBackColor = true;
            this.btnCall.Click += new System.EventHandler(this.btnCall_Click);
            // 
            // txtCallToIP
            // 
            this.txtCallToIP.Location = new System.Drawing.Point(107, 13);
            this.txtCallToIP.Name = "txtCallToIP";
            this.txtCallToIP.Size = new System.Drawing.Size(205, 20);
            this.txtCallToIP.TabIndex = 3;
            this.txtCallToIP.Text = "10.67.1.10";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(107, 41);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(205, 20);
            this.txtName.TabIndex = 4;
            // 
            // lblCallTo
            // 
            this.lblCallTo.AutoSize = true;
            this.lblCallTo.Location = new System.Drawing.Point(14, 18);
            this.lblCallTo.Name = "lblCallTo";
            this.lblCallTo.Size = new System.Drawing.Size(43, 13);
            this.lblCallTo.TabIndex = 5;
            this.lblCallTo.Text = "Call &To:";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(14, 44);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 6;
            this.lblName.Text = "&Name";
            // 
            // cmbCodecs
            // 
            this.cmbCodecs.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodecs.FormattingEnabled = true;
            this.cmbCodecs.Items.AddRange(new object[] {
            "None",
            "A-Law",
            "u-Law"});
            this.cmbCodecs.Location = new System.Drawing.Point(107, 67);
            this.cmbCodecs.Name = "cmbCodecs";
            this.cmbCodecs.Size = new System.Drawing.Size(205, 21);
            this.cmbCodecs.TabIndex = 7;
            this.cmbCodecs.SelectedIndexChanged += new System.EventHandler(this.cmbCodecs_SelectedIndexChanged);
            // 
            // lblCodec
            // 
            this.lblCodec.AutoSize = true;
            this.lblCodec.Location = new System.Drawing.Point(14, 75);
            this.lblCodec.Name = "lblCodec";
            this.lblCodec.Size = new System.Drawing.Size(41, 13);
            this.lblCodec.TabIndex = 8;
            this.lblCodec.Text = "C&odec:";
            // 
            // btnStartServer
            // 
            this.btnStartServer.Location = new System.Drawing.Point(17, 183);
            this.btnStartServer.Name = "btnStartServer";
            this.btnStartServer.Size = new System.Drawing.Size(75, 23);
            this.btnStartServer.TabIndex = 9;
            this.btnStartServer.Text = "Start Server";
            this.btnStartServer.UseVisualStyleBackColor = true;
            this.btnStartServer.Click += new System.EventHandler(this.btnStartServer_Click);
            // 
            // txt_log
            // 
            this.txt_log.Location = new System.Drawing.Point(17, 212);
            this.txt_log.Multiline = true;
            this.txt_log.Name = "txt_log";
            this.txt_log.Size = new System.Drawing.Size(543, 231);
            this.txt_log.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Record Devices:";
            // 
            // cmbRecordDevices
            // 
            this.cmbRecordDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecordDevices.FormattingEnabled = true;
            this.cmbRecordDevices.Items.AddRange(new object[] {
            "None",
            "A-Law",
            "u-Law"});
            this.cmbRecordDevices.Location = new System.Drawing.Point(107, 101);
            this.cmbRecordDevices.Name = "cmbRecordDevices";
            this.cmbRecordDevices.Size = new System.Drawing.Size(205, 21);
            this.cmbRecordDevices.TabIndex = 12;
            this.cmbRecordDevices.SelectedIndexChanged += new System.EventHandler(this.cmbRecordDevices_SelectedIndexChanged);
            // 
            // btnRefrescar
            // 
            this.btnRefrescar.Location = new System.Drawing.Point(318, 101);
            this.btnRefrescar.Name = "btnRefrescar";
            this.btnRefrescar.Size = new System.Drawing.Size(75, 23);
            this.btnRefrescar.TabIndex = 13;
            this.btnRefrescar.Text = "Refrescar";
            this.btnRefrescar.UseVisualStyleBackColor = true;
            this.btnRefrescar.Click += new System.EventHandler(this.btnRefrescar_Click);
            // 
            // cmbSoundChannels
            // 
            this.cmbSoundChannels.FormattingEnabled = true;
            this.cmbSoundChannels.Location = new System.Drawing.Point(188, 128);
            this.cmbSoundChannels.Name = "cmbSoundChannels";
            this.cmbSoundChannels.Size = new System.Drawing.Size(75, 21);
            this.cmbSoundChannels.TabIndex = 14;
            this.cmbSoundChannels.SelectedIndexChanged += new System.EventHandler(this.cmbSoundChannels_SelectedIndexChanged);
            // 
            // cmbBitsPerSample
            // 
            this.cmbBitsPerSample.FormattingEnabled = true;
            this.cmbBitsPerSample.Location = new System.Drawing.Point(107, 128);
            this.cmbBitsPerSample.Name = "cmbBitsPerSample";
            this.cmbBitsPerSample.Size = new System.Drawing.Size(75, 21);
            this.cmbBitsPerSample.TabIndex = 15;
            this.cmbBitsPerSample.SelectedIndexChanged += new System.EventHandler(this.cmbBitsPerSample_SelectedIndexChanged);
            // 
            // cmbSoundSamplesxsec
            // 
            this.cmbSoundSamplesxsec.FormattingEnabled = true;
            this.cmbSoundSamplesxsec.Location = new System.Drawing.Point(269, 128);
            this.cmbSoundSamplesxsec.Name = "cmbSoundSamplesxsec";
            this.cmbSoundSamplesxsec.Size = new System.Drawing.Size(75, 21);
            this.cmbSoundSamplesxsec.TabIndex = 16;
            this.cmbSoundSamplesxsec.SelectedIndexChanged += new System.EventHandler(this.cmbSoundSamplesxsec_SelectedIndexChanged);
            // 
            // VoiceChat
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(637, 468);
            this.Controls.Add(this.cmbSoundSamplesxsec);
            this.Controls.Add(this.cmbBitsPerSample);
            this.Controls.Add(this.cmbSoundChannels);
            this.Controls.Add(this.btnRefrescar);
            this.Controls.Add(this.cmbRecordDevices);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txt_log);
            this.Controls.Add(this.btnStartServer);
            this.Controls.Add(this.lblCodec);
            this.Controls.Add(this.cmbCodecs);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblCallTo);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.txtCallToIP);
            this.Controls.Add(this.btnCall);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "VoiceChat";
            this.Text = "VoiceChat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VoiceChat_FormClosing);
            this.Load += new System.EventHandler(this.VoiceChat_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCall;
        private System.Windows.Forms.TextBox txtCallToIP;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblCallTo;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.ComboBox cmbCodecs;
        private System.Windows.Forms.Label lblCodec;
        private System.Windows.Forms.Button btnStartServer;
        private System.Windows.Forms.TextBox txt_log;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbRecordDevices;
        private System.Windows.Forms.Button btnRefrescar;
        private System.Windows.Forms.ComboBox cmbSoundChannels;
        private System.Windows.Forms.ComboBox cmbBitsPerSample;
        private System.Windows.Forms.ComboBox cmbSoundSamplesxsec;
    }
}

