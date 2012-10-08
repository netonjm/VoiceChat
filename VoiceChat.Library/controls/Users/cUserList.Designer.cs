namespace VoiceChat.Library.Controls
{
    partial class cUserList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lst_users = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // lst_users
            // 
            this.lst_users.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lst_users.Location = new System.Drawing.Point(3, 3);
            this.lst_users.Name = "lst_users";
            this.lst_users.Size = new System.Drawing.Size(359, 250);
            this.lst_users.TabIndex = 25;
            this.lst_users.UseCompatibleStateImageBehavior = false;
            this.lst_users.View = System.Windows.Forms.View.List;
            // 
            // cUserList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lst_users);
            this.Name = "cUserList";
            this.Size = new System.Drawing.Size(365, 256);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lst_users;
    }
}
