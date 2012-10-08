using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


public class cChannelUser : ListViewItem
    {
        public string _username { get; set; }
        public string _ip { get; set; }

        public eTipoUsuario _tipo { get; set; }

        public enum eTipoUsuario
        {
            ServerChat = 0, Normal = 1, Banned = 2
        }

        public cChannelUser(string username, string ip, eTipoUsuario tipo)
        {
            _username = username;
            _ip =ip;
            _tipo = tipo;
        }

        public void ChangeNickName(string new_nickname)
        {
            _username = new_nickname;
            Text = _username;
        }

        public void ChangeType(eTipoUsuario tipo)
        {
            if (tipo == eTipoUsuario.ServerChat)
            {
                ImageIndex = 0;
                ToolTipText = "ServerChat";
            }else if (tipo == eTipoUsuario.Normal)
            {
                ImageIndex = 1;
                ToolTipText = "Usuario";
            } else if (tipo == eTipoUsuario.Banned)
            {
                ImageIndex = 2;
                ToolTipText = "Banned";
            }
        }


    }

