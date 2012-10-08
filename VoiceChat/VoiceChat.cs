using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound; 
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using g711audio;
using System.Runtime.InteropServices;

namespace VoiceChat
{

    #region JobObject

    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_ATTRIBUTES
    {
        public int nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public Int16 LimitFlags;
        public UInt32 MinimumWorkingSetSize;
        public UInt32 MaximumWorkingSetSize;
        public Int16 ActiveProcessLimit;
        public Int64 Affinity;
        public Int16 PriorityClass;
        public Int16 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UInt32 ProcessMemoryLimit;
        public UInt32 JobMemoryLimit;
        public UInt32 PeakProcessMemoryUsed;
        public UInt32 PeakJobMemoryUsed;
    }

    #endregion

    public partial class VoiceChat : Form, IDisposable
    {

        #region JOB OBJECT VARIABLES AND PROPERTIES
        
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CreateJobObject(object a, string lpName);

        [DllImport("kernel32.dll")]
        static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        private IntPtr m_handle;
        private bool m_disposed = false;

        #endregion


        private CaptureBufferDescription captureBufferDescription;
        private AutoResetEvent autoResetEvent;
        private Notify notify;        
        private WaveFormat waveFormat;
        private Capture capture;
        private int bufferSize;
        private CaptureBuffer captureBuffer;
        private UdpClient udpClient;                //Listens and sends data on port 1550, used in synchronous mode.
        private Device device;
        private SecondaryBuffer playbackBuffer;
        private BufferDescription playbackBufferDescription;
        private Socket clientSocket;
        private bool bStop;                         //Flag to end the Start and Receive threads.
        private IPEndPoint otherPartyIP;            //IP of party we want to make a call.
        private EndPoint otherPartyEP;
        private volatile bool bIsServerStarted;                 //Tells whether we have an active call.
        private Vocoder vocoder;
        private byte[] byteData = new byte[1024];   //Buffer to store the data received.
        private volatile int nUdpClientFlag;                 //Flag used to close the udpClient socket.

         List<IPEndPoint> otherPartyIPs = new List<IPEndPoint>();

         Thread receiverThread;
         Thread senderThread;

         bool IsThreadSendEnd = true;
         bool IsThreadReceiveEnd = true;
         bool IsThreadReceiveCommandsEnd = false;


         public Mode eMode = Mode.Stopped;


         public enum Mode
         {
             Server = 1, Client = 2, Stopped = 0
         }

         #region FormInits//Closing
        
         public void Dispose()
         {
             Dispose(true);
             GC.SuppressFinalize(this);
         }


         private void VoiceChat_FormClosing(object sender, FormClosingEventArgs e)
         {
             StopCall();

             //clientSocket.Disconnect(true);
             clientSocket.Shutdown(SocketShutdown.Both);

             udpClient.Close();

             while (!IsThreadReceiveEnd && !IsThreadSendEnd && !IsThreadReceiveCommandsEnd)
             {

             }

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
         }

         public VoiceChat()
         {

             m_handle = CreateJobObject(null, null);

             JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION();
             info.LimitFlags = 0x2000;

             JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();
             extendedInfo.BasicLimitInformation = info;

             int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
             IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
             Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

             if (!SetInformationJobObject(m_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                 throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));


             InitializeComponent();
             Initialize();
         }


         #endregion

         #region Call

         #region Server
         
         #endregion

         void StopCall()
         {
             if (bIsServerStarted)
             {
                 //StopCaptureSound();
                 UninitializeCall();
                 // DropCall();
             }
         }

         void StartServer()
         {
             if (!bIsServerStarted)
             {

                 //Reiniciamos los remotos
                 lock (otherPartyIPs)
                     otherPartyIPs.Clear();

                 eMode = Mode.Server;
                 InitializeCall();
             }
             else
                 StopCall();
         }

         public void InitializeCall()
         {

             InicializeCaptureSound();

             if (eMode == Mode.Server)
                 btnStartServer.Text = "Stop Server";

             //Start listening on port 1500.
             udpClient = new UdpClient(1550);

             bIsServerStarted = true;

             senderThread = new Thread(new ThreadStart(Send));
             receiverThread = new Thread(new ThreadStart(Receive));

             //Start the receiver and sender thread.
             receiverThread.Start();
             senderThread.Start();
             //btnCall.Enabled = false;
             //btnEndCall.Enabled = true;
             //}
             //catch (Exception ex)
             //{
             //    MessageBox.Show(ex.Message, "VoiceChat-InitializeCall ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             //}
         }

         private void UninitializeCall()
         {

             eMode = Mode.Stopped;

             try
             {
                 udpClient.Close();
             }
             catch (Exception)
             {
             }

             captureBuffer.Stop();

             btnStartServer.Text = "Start Server";
             //Set the flag to end the Send and Receive threads.
             bStop = true;
             bIsServerStarted = false;
             btnCall.Enabled = true;
             //btnEndCall.Enabled = false;
         }

         private void DropCall()
         {
             try
             {
                 //Send a Bye message to the user to end the call.
                 // SendMessage(Command.Bye, otherPartyEP);
                 UninitializeCall();
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "VoiceChat-DropCall ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }

         /*
          * Initializes all the data members.
          */
         private void Initialize()
         {
             try
             {

                 bIsServerStarted = false;
                 nUdpClientFlag = 0;

                 //Using UDP sockets
                 clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                 EndPoint ourEP = new IPEndPoint(IPAddress.Any, 1450);
                 //Listen asynchronously on port 1450 for coming messages (Invite, Bye, etc).
                 clientSocket.Bind(ourEP);

                 //Receive data from any IP.
                 EndPoint remoteEP = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));

                 byteData = new byte[1024];

                 //Receive data asynchornously.
                 clientSocket.BeginReceiveFrom(byteData,
                                            0, byteData.Length,
                                            SocketFlags.None,
                                            ref remoteEP,
                                            new AsyncCallback(OnReceive),
                                            null);
             }
             catch (Exception ex)
             {
                 //sssMessageBox.Show(ex.Message, "VoiceChat-Initialize ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 LogAppend("VoiceChat-Initialize > " + ex.Message);
             }
         }

         private void Call()
         {
             try
             {
                 //Get the IP we want to call.
                 otherPartyIP = new IPEndPoint(IPAddress.Parse(txtCallToIP.Text), 1450);
                 otherPartyEP = (EndPoint)otherPartyIP;

                 //Send an invite message.
                 SendMessage(Command.Invite, otherPartyEP);
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "VoiceChat-Call ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }

         /*
      * Receive audio data coming on port 1550 and feed it to the speakers to be played.
      */
         private void Receive()
         {
             try
             {

                 IsThreadReceiveEnd = false;

                 byte[] byteData;
                 bStop = false;
                 IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                 if (eMode == Mode.Server)
                 {
                     LogAppend("Server Started");
                     LogUsersConnected();
                 }
                 else
                     LogAppend("Client Audio Connected");

                 while (!bStop)
                 {
                     //Receive data.
                     try
                     {

                         //bytes_received = udp_socket.ReceiveFrom(data, ref ep);

                         try
                         {
                             byteData = udpClient.Receive(ref remoteEP);
                         }
                         catch (Exception)
                         {
                             return;
                         }

                         //G711 compresses the data by 50%, so we allocate a buffer of double
                         //the size to store the decompressed data.
                         byte[] byteDecodedData = new byte[byteData.Length * 2];

                         if (vocoder == Vocoder.ALaw)
                             ALawDecoder.ALawDecode(byteData, out byteDecodedData); //Vocoder.ALaw
                         else if (vocoder == Vocoder.uLaw)
                             MuLawDecoder.MuLawDecode(byteData, out byteDecodedData); //Vocoder.uLaw
                         else
                         {
                             byteDecodedData = new byte[byteData.Length];
                             byteDecodedData = byteData;
                         }


                         if (eMode == Mode.Server)
                         {
                             lock (otherPartyIPs)
                             {
                                 for (int i = 0; i < otherPartyIPs.Count; i++)
                                     udpClient.Send(byteDecodedData, byteDecodedData.Length, otherPartyIPs[i].Address.ToString(), 1550);

                             }

                         }

                         //Play the data received to the user.
                         playbackBuffer = new SecondaryBuffer(playbackBufferDescription, device);
                         playbackBuffer.Write(0, byteDecodedData, LockFlag.None);
                         playbackBuffer.Play(0, BufferPlayFlags.Default);
                     }
                     catch (Exception)
                     {

                     }

                 }

                 if (eMode == Mode.Server)
                 {
                     LogAppend("Server Stopped");
                     LogUsersConnected();
                 }
                 else
                     LogAppend("Client Audio Disconnected");

             }
             catch (Exception ex)
             {

                 LogAppend("Voice Receive > " + ex.Message);
                 //MessageBox.Show(ex.Message, "VoiceChat-Receive ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
             finally
             {
                 nUdpClientFlag += 1;
             }

             IsThreadReceiveEnd = true;

         }

         /*
         * Commands are received asynchronously. OnReceive is the handler for them.
         */
         private void OnReceive(IAsyncResult ar)
         {
             try
             {

                 EndPoint receivedFromEP = new IPEndPoint(IPAddress.Any, 0);

                 //Get the IP from where we got a message.
                 clientSocket.EndReceiveFrom(ar, ref receivedFromEP);

                 //Convert the bytes received into an object of type Data.
                 Data msgReceived = new Data(byteData);

                 //Act according to the received message.
                 switch (msgReceived.cmdCommand)
                 {

                     case Command.ChangeVocoderALaw:
                         LogAppend("El servidor cambio el modo a ALaw");
                         break;
                     case Command.ChangeVocoderNone:
                         LogAppend("El servidor cambio el modo a None");
                         break;
                     case Command.ChangeVocoderuLaw:
                         LogAppend("El servidor cambio el modo a uLaw");
                         break;

                     //We have an incoming call.
                     case Command.Invite:
                         {
                             if (bIsServerStarted)
                             {

                                 LogAppend("Se ha conectado: " + receivedFromEP.ToString());
                                 vocoder = msgReceived.vocoder;

                                 lock (otherPartyIPs)
                                 {
                                     if (!ExistsIp((IPEndPoint)receivedFromEP))
                                         otherPartyIPs.Add((IPEndPoint)receivedFromEP);
                                 }

                                 SendMessage(Command.OK, receivedFromEP);

                             }

                             break;
                         }

                     //OK is received in response to an Invite.
                     case Command.OK:
                         {

                             if (!bIsServerStarted)
                             {

                                 eMode = Mode.Client;

                                 //Solamente puede recibir este parametro si no está en modo servidor
                                 lock (otherPartyIPs)
                                 {
                                     otherPartyIPs.Clear();
                                     otherPartyIPs.Add((IPEndPoint)receivedFromEP);
                                 }

                                 LogAppend("Connectado a : " + receivedFromEP.ToString());

                                 InitializeCall();

                             }

                             break;
                         }

                     //Remote party is busy.

                     case Command.Bye:
                         {
                             //Check if the Bye command has indeed come from the user/IP with which we have
                             //a call established. This is used to prevent other users from sending a Bye, which
                             //would otherwise end the call.
                             if (receivedFromEP.Equals(otherPartyEP) == true)
                             {
                                 //End the call.
                                 UninitializeCall();
                             }
                             break;
                         }
                 }

                 byteData = new byte[1024];
                 //Get ready to receive more commands.

                 if (!bStop)
                     clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref receivedFromEP, new AsyncCallback(OnReceive), null);
                 else
                     IsThreadReceiveCommandsEnd = true;
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "VoiceChat-OnReceive ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }

         /*
          * Send synchronously sends data captured from microphone across the network on port 1550.
          */
         private void Send()
         {
             try
             {

                 IsThreadSendEnd = false;

                 //The following lines get audio from microphone and then send them 
                 //across network.

                 int users_count = 0;

                 captureBuffer = new CaptureBuffer(captureBufferDescription, capture);
                 CreateNotifyPositions();
                 int halfBuffer = bufferSize / 2;
                 captureBuffer.Start(true);
                 bool readFirstBufferPart = true;
                 int offset = 0;
                 MemoryStream memStream = new MemoryStream(halfBuffer);
                 bStop = false;

                 LogAppend("Sending Started");

                 while (!bStop)
                 {

                     lock (otherPartyIPs)
                     {

                         users_count = otherPartyIPs.Count;

                         if (users_count > 0)
                         {

                             autoResetEvent.WaitOne();
                             memStream.Seek(0, SeekOrigin.Begin);
                             captureBuffer.Read(offset, memStream, halfBuffer, LockFlag.None);
                             readFirstBufferPart = !readFirstBufferPart;
                             offset = readFirstBufferPart ? 0 : halfBuffer;

                             //TODO: Fix this ugly way of initializing differently.
                             //Choose the vocoder. And then send the data to other party at port 1550.
                             //if (vocoder == Vocoder.ALaw)
                             //{            

                             //byte[] dataToWrite = MuLawEncoder.MuLawEncode(memStream.GetBuffer()); //MULAW
                             //byte[] dataToWrite = ALawEncoder.ALawEncode(memStream.GetBuffer()); //ALAW (RECOMENdADO)
                             byte[] dataToWrite = memStream.GetBuffer(); //NORMAL

                             if (bStop)
                                 return;

                             for (int i = 0; i < users_count; i++)
                                 udpClient.Send(dataToWrite, dataToWrite.Length, otherPartyIPs[i].Address.ToString(), 1550);
                         }




                     }
                 }

                 IsThreadSendEnd = true;
                 LogAppend("Sending Ended");


             }
             catch (Exception ex)
             {
                 // MessageBox.Show(ex.Message, "VoiceChat-Send ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 LogAppend("VoiceChat-Send >> " + ex.Message);
             }
             finally
             {
                 captureBuffer.Stop();

                 //Increment flag by one.
                 nUdpClientFlag += 1;

                 //When flag is two then it means we have got out of loops in Send and Receive.
                 while (nUdpClientFlag != 2)
                 { }

                 //Clear the flag.
                 nUdpClientFlag = 0;

                 //Close the socket.
                 udpClient.Close();
             }
         }


         private void OnSend(IAsyncResult ar)
         {
             try
             {
                 clientSocket.EndSendTo(ar);
             }
             catch (Exception ex)
             {
                 MessageBox.Show(ex.Message, "VoiceChat-OnSend ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
             }
         }


         bool ExistsIp(IPEndPoint receivedFromEP)
         {
             for (int i = 0; i < otherPartyIPs.Count; i++)
             {
                 if (otherPartyIPs[i].Address == receivedFromEP.Address)
                     return true;
             }
             return false;
         }

         #endregion

         private void btnStartServer_Click(object sender, EventArgs e)
         {
             StartServer();
         }


        private void btnCall_Click(object sender, EventArgs e)
        {
            //DropCall();
            Call();
        }

   


        #region Log

        void LogUsersConnected()
        {
            LogAppend((otherPartyIPs.Count + 1) + " users connected.");
        }


        public delegate void appedLogCallback(string message);
        void LogAppend(string message)
        {
            if (txt_log.InvokeRequired)
            {
                try
                {
                    appedLogCallback d = new appedLogCallback(LogAppend);
                    this.Invoke(d, new object[] { message });
                }
                catch (Exception)
                {

                }
            }
            else
            {
                try
                {
                    txt_log.AppendText(DateTime.Now.ToShortTimeString() + ": " + message + Environment.NewLine);
                    txt_log.ScrollToCaret();
                }
                catch (Exception)
                {
                }
            }
        }

        #endregion
    


        #region WaveFormat


        private void InicializeCaptureSound()
        {
            device = new Device();
            device.SetCooperativeLevel(this, CooperativeLevel.Normal);

            //CaptureDevicesCollection captureDeviceCollection = new CaptureDevicesCollection();
            //capture = new Capture(captureDeviceCollection[0].DriverGuid);
            //DeviceInformation deviceInfo = (DeviceInformation) cmbRecordDevices.SelectedItem;  //captureDeviceCollection[0];
            capture = new Capture(record_source);

            SetWaveFormat();

            captureBufferDescription = new CaptureBufferDescription();
            captureBufferDescription.BufferBytes = waveFormat.AverageBytesPerSecond / 5;//approx 200 milliseconds of PCM data.
            captureBufferDescription.Format = waveFormat;

            playbackBufferDescription = new BufferDescription();
            playbackBufferDescription.BufferBytes = waveFormat.AverageBytesPerSecond / 5;
            playbackBufferDescription.Format = waveFormat;

            playbackBuffer = new SecondaryBuffer(playbackBufferDescription, device);
            bufferSize = captureBufferDescription.BufferBytes;
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {

        }

        private void cmbBitsPerSample_SelectedIndexChanged(object sender, EventArgs e)
        {
          bitsPerSample=   Int16.Parse(cmbBitsPerSample.SelectedValue.ToString()); //16Bit, alternatively use 8Bits.

        }

        private void cmbSoundChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
         channels= Int16.Parse(cmbSoundChannels.SelectedValue.ToString()); //Stereo.


        }

        private void cmbSoundSamplesxsec_SelectedIndexChanged(object sender, EventArgs e)
        {
           samplesPerSecond= (int)cmbSoundSamplesxsec.SelectedValue; //11KHz use 11025 , 22KHz use 22050, 44KHz use 44100 etc.
        }


        short channels = 1;
        short bitsPerSample = 16;
        int samplesPerSecond = 22050;

        Guid record_source;

        void SetWaveFormat()
        {
            //short channels = 1; //Stereo.
            //short bitsPerSample = 16; //16Bit, alternatively use 8Bits.
            //int samplesPerSecond = 22050; //11KHz use 11025 , 22KHz use 22050, 44KHz use 44100 etc.

           

            //Set up the wave format to be captured.
            waveFormat = new WaveFormat();
            waveFormat.Channels = channels;
            waveFormat.FormatTag = WaveFormatTag.Pcm;
            waveFormat.SamplesPerSecond = samplesPerSecond;
            waveFormat.BitsPerSample = bitsPerSample;
            waveFormat.BlockAlign = (short)(channels * (bitsPerSample / (short)8));
            waveFormat.AverageBytesPerSecond = waveFormat.BlockAlign * samplesPerSecond;

        }

        #endregion


        private void CreateNotifyPositions()
        {
            try
            {
                autoResetEvent = new AutoResetEvent(false);
                notify = new Notify(captureBuffer);
                BufferPositionNotify bufferPositionNotify1 = new BufferPositionNotify();
                bufferPositionNotify1.Offset = bufferSize / 2 - 1;
                bufferPositionNotify1.EventNotifyHandle = autoResetEvent.SafeWaitHandle.DangerousGetHandle();
                BufferPositionNotify bufferPositionNotify2 = new BufferPositionNotify();
                bufferPositionNotify2.Offset = bufferSize - 1;
                bufferPositionNotify2.EventNotifyHandle = autoResetEvent.SafeWaitHandle.DangerousGetHandle();

                notify.SetNotificationPositions(new BufferPositionNotify[] { bufferPositionNotify1, bufferPositionNotify2 });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "VoiceChat-CreateNotifyPositions ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
         * Send a message to the remote party.
         */
        private void SendMessage(Command cmd, EndPoint sendToEP)
        {
            try
            {
                //Create the message to send.
                Data msgToSend = new Data();

                msgToSend.strName = txtName.Text;   //Name of the user.
                msgToSend.cmdCommand = cmd;         //Message to send.
                msgToSend.vocoder = vocoder;        //Vocoder to be used.
                
                byte[] message = msgToSend.ToByte();

                //Send the message asynchronously.
                clientSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, sendToEP, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "VoiceChat-SendMessage ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        class combo_properties
        {
            public int value { get; set; }
            public string description { get; set; }
        }

        class capture_device
        {
            public Guid DriverGuid { get; set; }
            public string Description { get; set; }
            public string ModuleName { get; set; }
        }

        List<capture_device> _devices = new List<capture_device>();

        List<combo_properties> _combo_bits_sample = new List<combo_properties>();
        List<combo_properties> _combo_samples_second = new List<combo_properties>();
        List<combo_properties> _combo_channels = new List<combo_properties>();


         private void cmbRecordDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            record_source = (Guid)cmbRecordDevices.SelectedValue;
        }

        void FillCombos()
        {
            //Record Devices
            CaptureDevicesCollection captureDeviceCollection = new CaptureDevicesCollection();
            cmbRecordDevices.ValueMember = "DriverGuid";
            cmbRecordDevices.DisplayMember = "Description";
            foreach (DeviceInformation item in captureDeviceCollection)
            {
                _devices.Add(new capture_device()
                {
                     Description = item.Description, ModuleName = item.ModuleName, 
                     DriverGuid = item.DriverGuid
                });
            }
            cmbRecordDevices.DataSource = _devices;

            cmbBitsPerSample.DisplayMember = cmbSoundChannels.DisplayMember = cmbSoundSamplesxsec.DisplayMember = "description";
            cmbBitsPerSample.ValueMember = cmbSoundChannels.ValueMember = cmbSoundSamplesxsec.ValueMember = "value";
            
            //BitsXSample
            _combo_channels.Add(new combo_properties() { description = "Mono", value = 0 });
            _combo_channels.Add(new combo_properties() { description = "Estereo", value = 1 });
            cmbSoundChannels.DataSource = _combo_channels;

            //Channels
            _combo_bits_sample.Add(new combo_properties() { description = "8Bit", value = 8 });
            _combo_bits_sample.Add(new combo_properties() { description = "16Bit", value = 16 });
            cmbBitsPerSample.DataSource = _combo_bits_sample;

            //Samplesxsec
            _combo_samples_second.Add(new combo_properties() { description = "11KHz", value = 11025 });
            _combo_samples_second.Add(new combo_properties() { description = "22KHz", value = 22050 });
            _combo_samples_second.Add(new combo_properties() { description = "44KHz", value = 44100 });
            cmbSoundSamplesxsec.DataSource = _combo_samples_second;

           cmbBitsPerSample.SelectedIndex = cmbSoundChannels.SelectedIndex = cmbSoundSamplesxsec.SelectedIndex = 1;
           cmbCodecs.SelectedIndex = 0;
           cmbRecordDevices.SelectedIndex = 0;
            //short channels = 1; //Stereo.
            //short bitsPerSample = 16; //16Bit, alternatively use 8Bits.
            //int samplesPerSecond = 22050; //11KHz use 11025 , 22KHz use 22050, 44KHz use 44100 etc.

        }




        private void cmbCodecs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCodecs.SelectedIndex == 1)
            {
                vocoder = Vocoder.ALaw;
                LogAppend("Se ha modificado codificación a: ALaw");

                if (eMode == Mode.Server)
                {
                    lock (otherPartyIPs)
                    {
                        for (int i = 0; i < otherPartyIPs.Count; i++)
                            SendMessage(Command.ChangeVocoderALaw, otherPartyIPs[i]);
                    }

                }

            }
            else if (cmbCodecs.SelectedIndex == 1)
            {
                vocoder = Vocoder.uLaw;
                LogAppend("Se ha modificado codificación a: uLaw");

                if (eMode == Mode.Server)
                {
                    lock (otherPartyIPs)
                    {
                        for (int i = 0; i < otherPartyIPs.Count; i++)
                            SendMessage(Command.ChangeVocoderuLaw, otherPartyIPs[i]);
                    }

                }

            }
            else
            {
                vocoder = Vocoder.None;
                LogAppend("Se ha modificado codificación a: None");
                if (eMode == Mode.Server)
                {
                    lock (otherPartyIPs)
                    {
                        for (int i = 0; i < otherPartyIPs.Count; i++)
                            SendMessage(Command.ChangeVocoderNone, otherPartyIPs[i]);
                    }

                }
              
            }
        }

    

    

    
    }

    //The commands for interaction between the two parties.
    enum Command
    {
        Invite, //Make a call.
        Bye,    //End a call.
        Busy,   //User busy.
        OK,     //Response to an invite message. OK is send to indicate that call is accepted.
        Null,   //No command.
        ChangeVocoderNone,
        ChangeVocoderALaw,
        ChangeVocoderuLaw//Make a call.
    }

    //Vocoder
    enum Vocoder
    {
        ALaw,   //A-Law vocoder.
        uLaw,   //u-Law vocoder.
        None,   //Don't use any vocoder.
    }

    //The data structure by which the server and the client interact with 
    //each other.
    class Data
    {
        //Default constructor.
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strName = null;
            vocoder = Vocoder.ALaw;
        }

        //Converts the bytes into an object of type Data.
        public Data(byte[] data)
        {
            //The first four bytes are for the Command.
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            //The next four store the length of the name.
            int nameLen = BitConverter.ToInt32(data, 4);

            //This check makes sure that strName has been passed in the array of bytes.
            if (nameLen > 0)
                this.strName = Encoding.UTF8.GetString(data, 8, nameLen);
            else
                this.strName = null;
        }

        //Converts the Data structure into an array of bytes.
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command.
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            //Add the length of the name.
            if (strName != null)
                result.AddRange(BitConverter.GetBytes(strName.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //Add the name.
            if (strName != null)
                result.AddRange(Encoding.UTF8.GetBytes(strName));

            return result.ToArray();
        }

        public string strName;      //Name by which the client logs into the room.
        public Command cmdCommand;  //Command type (login, logout, send message, etc).
        public Vocoder vocoder;
    }
}