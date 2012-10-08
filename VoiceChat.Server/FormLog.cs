using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;

namespace VoiceChat.Server
{
    public partial class FormLog : Form
    {

       
        public bool IsClosing = false;
        Thread logThread;
        public delegate void LogAppendCallback(string text);

        public FormLog()
        {
            InitializeComponent();
        }

        private void FormLog_Load(object sender, EventArgs e)
        {
            logThread = new Thread(new ThreadStart(ThreadStartLog));
            logThread.Priority = ThreadPriority.Normal;
            logThread.Start();
            
        }

        private void FormLog_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }


        private void button2_Click(object sender, EventArgs e)
        {

        }
  
        public void LogAppend(string text)
        {
            if (!this.InvokeRequired)
            {
                if (text.Length > 0)
                {
                    m_rtb.SelectionStart = m_rtb.Text.Length;
                    m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Regular);

                    if (text[0] == '1')
                    {

                        m_rtb.SelectionColor = Color.Black;
                    }
                    else if (text[0] == '2')
                    {
                        //m_rtb.SelectionFont.Bold = true;
                        m_rtb.SelectionFont = new Font("Courier New", 10, FontStyle.Bold);
                        m_rtb.SelectionColor = Color.Red;
                    }
                    else
                        m_rtb.SelectionColor = Color.FromArgb(0x33, 0x66, 0x66);

                    m_rtb.SelectedText = text.Substring(1) + Environment.NewLine;
                    m_rtb.AppendText(m_rtb.SelectedText);
                }
            }
            else
            {
                LogAppendCallback d = new LogAppendCallback(LogAppend);
                this.Invoke(d, new object[] { text });
            }
       }

        private void btnClear_Click(object sender, EventArgs e)
        {
            m_rtb.Clear();
        }

        public void ThreadStartLog()
        {
            while (!cGlobalVars.IsFinished)
            {
                if (cGlobalVars.LogMessages.Count > 0)
                {
                    lock (cGlobalVars.LogMessages)
                    {
                        while (cGlobalVars.LogMessages.Count > 0)
                        {
                            LogAppend(cGlobalVars.LogMessages[0]);
                            cGlobalVars.LogMessages.RemoveAt(0);
                        }
                    }
                }
            }
        }
    }
}
