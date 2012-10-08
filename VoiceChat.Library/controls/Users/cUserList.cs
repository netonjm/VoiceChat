using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VoiceChat.Library.Controls
{
    public partial class cUserList : UserControl
    {

        //public cChannel _channel_actual { get; set; }

        public cUserList()
        {
            InitializeComponent();
           // _channel_actual = new cChannel();
           // lst_users.Items = _channel_actual;
        }


        public void AddUser(string username, string ip, cChannelUser.eTipoUsuario tipo)
        {
            lst_users.Items.Add(new cChannelUser(username, ip, tipo));

        }
     

    }
}
