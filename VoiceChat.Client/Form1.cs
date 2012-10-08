using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace app_VoiceChatClient
{
    public partial class Form1 : Form
    {

        //cChatSer _engine;

        public Form1()
        {
            InitializeComponent();
            
           


        }

        private void button1_Click(object sender, EventArgs e)
        {

            //_engine = new DevelopStudios.VoiceChat.cChatEngine();
            //_engine.Initialize();
            ////_engine.eMode = DevelopStudios.VoiceChat.cChatEngine.Mode.Client;
            //_engine.Call("10.67.1.25");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //_engine.DropCall();
        }
    }
}
