using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;


using System.Runtime.InteropServices;
using NAudio.Wave;
using NAudio.Mixer;
using System.Threading.Tasks;
using g711audio;
using VoiceChat;
using VoiceChat.Library;
using VoiceChat.Library.Common;

namespace VoiceChat.Server
{
  
        //#region JobObject

        //public enum JobObjectInfoType
        //{
        //    AssociateCompletionPortInformation = 7,
        //    BasicLimitInformation = 2,
        //    BasicUIRestrictions = 4,
        //    EndOfJobTimeInformation = 6,
        //    ExtendedLimitInformation = 9,
        //    SecurityLimitInformation = 5,
        //    GroupInformation = 11
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //public struct SECURITY_ATTRIBUTES
        //{
        //    public int nLength;
        //    public IntPtr lpSecurityDescriptor;
        //    public int bInheritHandle;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        //{
        //    public Int64 PerProcessUserTimeLimit;
        //    public Int64 PerJobUserTimeLimit;
        //    public Int16 LimitFlags;
        //    public UInt32 MinimumWorkingSetSize;
        //    public UInt32 MaximumWorkingSetSize;
        //    public Int16 ActiveProcessLimit;
        //    public Int64 Affinity;
        //    public Int16 PriorityClass;
        //    public Int16 SchedulingClass;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //struct IO_COUNTERS
        //{
        //    public UInt64 ReadOperationCount;
        //    public UInt64 WriteOperationCount;
        //    public UInt64 OtherOperationCount;
        //    public UInt64 ReadTransferCount;
        //    public UInt64 WriteTransferCount;
        //    public UInt64 OtherTransferCount;
        //}

        //[StructLayout(LayoutKind.Sequential)]
        //struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        //{
        //    public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        //    public IO_COUNTERS IoInfo;
        //    public UInt32 ProcessMemoryLimit;
        //    public UInt32 JobMemoryLimit;
        //    public UInt32 PeakProcessMemoryUsed;
        //    public UInt32 PeakJobMemoryUsed;
        //}

        //#endregion

        public partial class VoiceChat : Form, IDisposable
        {

            //#region JOB OBJECT VARIABLES AND PROPERTIES

            //[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            //static extern IntPtr CreateJobObject(object a, string lpName);

            //[DllImport("kernel32.dll")]
            //static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

            //[DllImport("kernel32.dll", SetLastError = true)]
            //static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

            ////private IntPtr m_handle;
            ////private bool m_disposed = false;

            //#endregion


            FormLog _window_log;

            Thread receiverThread;
            Thread senderThread;

            cChatServer _chat_engine;

            //bool IsThreadReceiveEnd = true;
            //bool IsThreadReceiveCommandsEnd = false;

            #region FormInits//Closing

            //public void Dispose()
            //{
            //    Dispose(true);
            //    GC.SuppressFinalize(this);
            //}



            /// <summary>
            /// SE ARRANCA PARA SERVER Y CLIENTE 
            /// </summary>
            public void InitializeCall()
            {

                //Bloqueamos los botones
                StartFormActions(true);

                //Inicializamos el dispositivo de captura
                //InicializeCaptureSound();
                _chat_engine.InitializeCall();

             
                 _chat_engine.IsServiceStarted  = true;

                senderThread = new Thread(new ThreadStart(_chat_engine.Send));
                receiverThread = new Thread(new ThreadStart(_chat_engine.Receive));

                //Start the receiver and sender thread.
                receiverThread.Start();
                senderThread.Start();

            }


         
            private void VoiceChat_FormClosing(object sender, FormClosingEventArgs e)
            {

                _chat_engine.IsServiceStarted = false;

                _window_log.IsClosing = true;
                _window_log.Close();
                _window_log.Dispose();

                try
                {
                    if (receiverThread.IsAlive)
                        receiverThread.Abort();
                }
                catch (Exception)
                {
                }

                try
                {
                    if (senderThread.IsAlive)
                        senderThread.Abort();
                }
                catch (Exception)
                {
                }

            }

            private void VoiceChat_Load(object sender, EventArgs e)
            {

                FillCombos();
                _chat_engine.StartRecord();
            }

            public VoiceChat()
            {

                //m_handle = CreateJobObject(null, null);

                //JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
                //info.LimitFlags = 0x2000;

                //JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
                //extendedInfo.BasicLimitInformation = info;

                //int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
                //IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
                //Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                //if (!SetInformationJobObject(m_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                //    throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));


                InitializeComponent();
                Initialize();
            }

            #endregion

            void CreateLogWindowAndFormVisivility()
            {
                //Ponemos en primer plano las dos ventanas
                _window_log = new FormLog();
                _window_log.Show();
                this.TopMost = true;
                this.TopMost = false;
                _window_log.TopMost = true;
                _window_log.TopMost = false;
            }

        
            /*
             * Initializes all the data members.
             */
            private void Initialize()
            {

                CreateLogWindowAndFormVisivility();
                _chat_engine = new cChatServer();

                _chat_engine.CallChangeButtonText += new cChatServer.ChangeButtonTextHandler(_chat_engine_CallChangeButtonText);
                _chat_engine.CallModeClientConnected += new cChatServer.ModeClientConnectedHandler(_chat_engine_CallModeClientConnected);
                _chat_engine.CallTrackBarValueChanged += new cChatServer.TrackBarValueChangedHandler(_chat_engine_CallTrackBarValueChanged);
                _chat_engine.Initialize();

            
            }

            void _chat_engine_CallTrackBarValueChanged(int value)
            {
                //trackBar1.Value = 0;
                trackBar1.Value = value;
                // btnCall.Text = ;
                //UserCallFormActions(true, true);
            }

            void _chat_engine_CallModeClientConnected()
            {
                StartFormActions(true);
            }

            void _chat_engine_CallChangeButtonText(string text)
            {
                btnCall.Text = text;
            }

            private void btnStartServer_Click(object sender, EventArgs e)
            {
                txtName.Text = "Magu - Server";
                _chat_engine.StartServer();
            }

            private void btnCall_Click(object sender, EventArgs e)
            {
                //txtName.Text = "Magu - Client";
                //if (!_chat_engine.IsServiceStarted)
                //{
                //    btnCall.Enabled = true;
                //    btnCall.Text = "Connecting...";
                //    _chat_engine.Call(txtCallToIP.Text);
                //}
                //else
                //{
                //    //Send a Bye message to the user to end the call.
                //    StartFormActions(false);

                //    _chat_engine.DropCall();
                //}
            }

            #region Log

          

            public delegate void SetCodecComboCallback(int index);
            void SetCodecCombo(int index)
            {
                if (this.InvokeRequired)
                {
                    SetCodecComboCallback d = new SetCodecComboCallback(SetCodecCombo);
                    this.Invoke(d, new object[] { index });
                }
                else
                {
                    cmbCodecs.SelectedIndex = index;
                }
            }


            public void LogAppend(string message)
            {
                LogAppend(message, 1);
            }


            public delegate void appedLogCallback(string message,int tipo);
            void LogAppend(string message, int tipo)
            {
                if (this.InvokeRequired)
                {
                    try
                    {
                        appedLogCallback d = new appedLogCallback(LogAppend);
                        this.Invoke(d, new object[] { message, tipo });
                    }
                    catch (Exception)
                    {

                    }
                }
                else
                {
                    try
                    {

                        if (_window_log != null)
                        {
                            _window_log.LogAppend(tipo.ToString() + message);
                        }

                        //txt_log.AppendText(DateTime.Now.ToShortTimeString() + ": " + message + Environment.NewLine);
                        //txt_log.ScrollToCaret();
                    }
                    catch (Exception ex)
                    {
                        cGlobalVars.AddLogChat(ex.Message);
                    }
                }
            }

            #endregion



            #region WaveFormat



            private void btnRefrescar_Click(object sender, EventArgs e)
            {

            }


            #endregion



            #region FormActionsEvents

            void StartFormActions(bool state)
            {

                if (_chat_engine.eMode == cChatServer.Mode.Server)
                {
                    if (state)
                    {
                        cmbCodecs.Enabled = true;
                        btnStartServer.Text = "Stop Chat Server";
                        //btnStartServer.Enabled = state;
                    }
                    else
                    {
                        cmbCodecs.Enabled = false;
                        btnStartServer.Text = " Start Chat Server";
                    }


                }
                //else if (_chat_engine.eMode == cChatServer.Mode.Client)
                //{
                //    if (state)
                //    {
                //        btnCall.Text = "Disconnect";
                //    }
                //    else
                //    {
                //        btnCall.Text = "Connect to server";
                //    }

                //}

                cmbRecordDevices.Enabled = txtName.Enabled = txtCallToIP.Enabled = btnCall.Enabled = !state;

            }

            #endregion

          
            private void cmbRecordDevices_SelectedIndexChanged(object sender, EventArgs e)
            {
                _chat_engine.SetRecordSource((int)cmbRecordDevices.SelectedValue);
            }

            void FillCombos()
            {

                cmbRecordDevices.ValueMember = "WaveDevice";
                cmbRecordDevices.DisplayMember = "Description";

                cmbRecordDevices.DataSource = cChatServer.GetDevices();

                if (cmbCodecs.Items.Count > 0)
                cmbCodecs.SelectedIndex = 0;

                if (cmbRecordDevices.Items.Count>0)
                cmbRecordDevices.SelectedIndex = 0;
            
            }


            private void cmbCodecs_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (cmbCodecs.SelectedIndex == 0)
                    _chat_engine.ChangeCodification(VoiceCommon.Vocoder.ALaw);
                else if (cmbCodecs.SelectedIndex == 1)
                    _chat_engine.ChangeCodification(VoiceCommon.Vocoder.uLaw);
                else
                    _chat_engine.ChangeCodification(VoiceCommon.Vocoder.None);
            }

            private void btnShowLog_Click(object sender, EventArgs e)
            {
                if (_window_log != null)
                {
                    _window_log.Visible = !_window_log.Visible;
                }
            }

            private void button1_Click(object sender, EventArgs e)
            {
                //if (!test_phone)
                //{
                //    //Inicializamos el dispositivo de captura
                //    //InicializeCaptureSound();

                 

                //    if (testThread != null && testThread.IsAlive)
                //        testThread.Abort();
                //    testThread = new Thread(new ThreadStart(TestPhone));
                //    testThread.Priority = ThreadPriority.BelowNormal;
                //    testThread.Start();
                //}

                //test_phone = !test_phone;
             
            }

            private void trackBar1_ValueChanged(object sender, EventArgs e)
            {
                _chat_engine.ChangeVol((float)trackBar1.Value / 100);
            }

        }

     
    }

