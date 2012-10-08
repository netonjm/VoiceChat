using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace app_VoiceChatServer
{
    public class cChatWindow : System.Windows.Forms.RichTextBox
    {

        public void LogAppend(string text)
        {

            if (text.Length > 0)
            {
                this.SelectionStart = this.Text.Length;
                this.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);

                if (text[0] == '1')
                {

                    this.SelectionColor = Color.Black;
                }
                else if (text[0] == '2')
                {
                    //m_rtb.SelectionFont.Bold = true;
                    this.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                    this.SelectionColor = Color.Red;
                }
                else
                    this.SelectionColor = Color.FromArgb(0x33, 0x66, 0x66);

                this.SelectedText = text.Substring(1) + Environment.NewLine;
                this.AppendText(this.SelectedText);
            }

        }


    }
}
