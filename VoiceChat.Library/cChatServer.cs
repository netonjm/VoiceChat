﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using g711audio;
using NAudio.Wave;

using VoiceChat.Library.Common;


namespace VoiceChat.Library
{
    public class cChatServer
    {

        private byte[] byteData = new byte[1024];   //Buffer to store the data received.
        private UdpClient udpClient;                //Listens and sends data on port 1550, used in synchronous mode.

        private Socket clientSocket;
        private bool bStop;                         //Flag to end the Start and Receive threads.
        private IPEndPoint serverIP;            //IP of party we want to make a call.
        private EndPoint serverEP;

        public VoiceCommon.Vocoder vocoder;

        int MaxTime = 10;

        int _record_source = 0;

        private volatile int nUdpClientFlag;                 //Flag used to close the udpClient socket.

        List<IPEndPoint> clientIPs = new List<IPEndPoint>();
        List<IPEndPoint> clientBanIPs = new List<IPEndPoint>();
       
        public Mode eMode = Mode.Stopped;

        private volatile bool bIsServiceStarted;                 //Tells whether we have an active call.

        public enum Mode
        {
            Server = 1, Stopped = 0
        }

        public bool IsServiceStarted = false;

        string Nickname = "";

        public void SetRecordSource(int source)
        {
            _record_source = source;
        }

        public void Initialize()
        {

            try
            {

                IsServiceStarted = false;
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
                cGlobalVars.AddLogChat("VoiceChat-Initialize > " + ex.Message);
            }
        }


        #region Server

        public void StartServer()
        {
            // //btnCall.Enabled = false;
            //btnEndCall.Enabled = true;


            if (!bIsServiceStarted)
            {
                //Reiniciamos los remotos
                lock (clientIPs)
                    clientIPs.Clear();

                eMode = Mode.Server;
                InitializeCall();
            }
            else
                DropCall();
        }

        #endregion


        void SetCodecCombo(int value)
        {
            //TODO
        }

        /*
        * Commands are received asynchronously. OnReceive is the handler for them.
        */
        private void OnReceive(IAsyncResult ar)
        {

            //TODO: Switch Receive Commands

            try
            {

                EndPoint receivedFromEP = new IPEndPoint(IPAddress.Any, 0);

                //Get the IP from where we got a message.
                clientSocket.EndReceiveFrom(ar, ref receivedFromEP);

                //Convert the bytes received into an object of type Data.
                VoiceCommon.Data msgReceived = new VoiceCommon.Data(byteData);

                //Act according to the received message.
                switch (msgReceived.cmdCommand)
                {


                    //MODO SERVER: recepción de invitación por parte de cliente.
                    case VoiceCommon.Command.Invite:
                        {
                            if (IsServiceStarted && eMode == Mode.Server)
                            {

                                cGlobalVars.AddLogChat("Se ha conectado: " + receivedFromEP.ToString());
                                vocoder = msgReceived.vocoder;

                                lock (clientIPs)
                                {
                                    if (!ExistsIp((IPEndPoint)receivedFromEP))
                                        clientIPs.Add((IPEndPoint)receivedFromEP);
                                }

                                SendMessage(VoiceCommon.Command.OK, receivedFromEP);

                            }

                            break;
                        }
                
                }

                byteData = new byte[1024];
                //Get ready to receive more commands.

                if (!bStop)
                    clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref receivedFromEP, new AsyncCallback(OnReceive), null);

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "VoiceChat-OnReceive ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cGlobalVars.AddLogChat("VoiceChat-OnReceive () > " + ex.Message);
            }
        }




        /// <summary>
        /// SERVER + CLIENT 
        /// </summary>
        private void UninitializeCall()
        {

            try
            {
                udpClient.Close();
            }
            catch (Exception)
            {
            }

            //if(captureBuffer!=null)
            //captureBuffer.Stop();
            bStop = true;
            bIsServiceStarted = false;


        }


        public void InitializeCall()
        {

            //Start listening on port 1500.
            udpClient = new UdpClient(1550);

        }



        void Dispose()
        {

            DestroyRecording();

            DropCall();

            //clientSocket.Disconnect(true);
            clientSocket.Shutdown(SocketShutdown.Both);

            if (udpClient != null)
                udpClient.Close();

        }




        public void DropCall()
        {
            try
            {

                if (eMode == Mode.Server)
                {
                    lock (clientIPs)
                    {
                        foreach (var item in clientIPs)
                            SendMessage(VoiceCommon.Command.Bye, item);
                    }
                }

                eMode = Mode.Stopped;
                UninitializeCall();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "VoiceChat-DropCall ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cGlobalVars.AddLogChat("VoiceChat-DropCall () > " + ex.Message);
            }
        }


        void LogUsersConnected()
        {
            cGlobalVars.AddLogChat(GetUsersCount() + " users connected.");
        }

        /*
     * Receive audio data coming on port 1550 and feed it to the speakers to be played.
     */
        public void Receive()
        {
            //TODO: Receive Sound DATA

            try
            {
                byte[] byteData;
                bStop = false;
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                if (eMode == Mode.Server)
                {
                    cGlobalVars.AddLogChat("Server Started");
                    LogUsersConnected();
                }
                else
                    cGlobalVars.AddLogChat("Client Audio Connected");

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

                        if (vocoder == VoiceCommon.Vocoder.ALaw)
                            ALawDecoder.ALawDecode(byteData, out byteDecodedData); //Vocoder.ALaw
                        else if (vocoder == VoiceCommon.Vocoder.uLaw)
                            MuLawDecoder.MuLawDecode(byteData, out byteDecodedData); //Vocoder.uLaw
                        else
                        {
                            byteDecodedData = new byte[byteData.Length];
                            byteDecodedData = byteData;
                        }

                        if (eMode == Mode.Server)
                        {
                            lock (clientIPs)
                            {
                                // udpClient.Send(byteData, byteData.Length, clientIPs[i].Address.ToString(), 1550);


                                //Parallel.For(0, clientIPs.Count, delegate(int i)
                                //     {
                                //        
                                //     }
                                // );
                            }

                        }

                        //LogAppend("2Receibed Data!");

                        bwp_internet.AddSamples(byteData, 0, byteData.Length);

                    }
                    catch (Exception)
                    {

                    }

                }

                if (eMode == Mode.Server)
                {
                    cGlobalVars.AddLogChat("Server Stopped");
                    LogUsersConnected();
                }
                else
                    cGlobalVars.AddLogChat("Client Audio Disconnected");

            }
            catch (Exception ex)
            {

                cGlobalVars.AddLogChat("Voice Receive > " + ex.Message);
                //MessageBox.Show(ex.Message, "VoiceChat-Receive ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                nUdpClientFlag += 1;
            }

        }

        List<byte[]> _music_data = new List<byte[]>();

        byte[] DispatchData()
        {
            lock (_music_data)
            {

                if (_music_data.Count > 0)
                {
                    //LogAppend(_music_data.Count.ToString());
                    var tmp = _music_data[0];
                    _music_data.RemoveAt(0);
                    return tmp;
                }
                return null;
            }

        }

        bool ExistsIp(IPEndPoint receivedFromEP)
        {
            for (int i = 0; i < clientIPs.Count; i++)
            {
                if (clientIPs[i].Address == receivedFromEP.Address)
                    return true;
            }
            return false;
        }

        /*
         * Send synchronously sends data captured from microphone across the network on port 1550.
         */
        public void Send()
        {

            //TODO: Send Microphone Data
            //Esperamos hasta que recivamos el start del server

            try
            {

                //The following lines get audio from microphone and then send them 
                //across network.

                int users_count = 0;

                //captureBuffer = new CaptureBuffer(captureBufferDescription, capture);
                //CreateNotifyPositions();
                //int halfBuffer = bufferSize / 2;
                //captureBuffer.Start(true);
                //bool readFirstBufferPart = true;
                //int offset = 0;
                //MemoryStream memStream = new MemoryStream(halfBuffer);
                //bStop = false;

                cGlobalVars.AddLogChat("Sending Started");

                lock (_music_data)
                {
                    _music_data.Clear();
                }

                while (!bStop)
                {

                    lock (clientIPs)
                    {
                        
                        users_count = clientIPs.Count;

                        if (users_count > 0)
                        {

                            //autoResetEvent.WaitOne();
                            //memStream.Seek(0, SeekOrigin.Begin);
                            ////captureBuffer.Read(offset, memStream, halfBuffer, LockFlag.None);
                            //readFirstBufferPart = !readFirstBufferPart;
                            //offset = readFirstBufferPart ? 0 : halfBuffer;

                            ////TODO: Fix this ugly way of initializing differently.
                            ////Choose the vocoder. And then send the data to other party at port 1550.
                            ////if (vocoder == Vocoder.ALaw)
                            ////{            

                            ////byte[] dataToWrite = MuLawEncoder.MuLawEncode(memStream.GetBuffer()); //MULAW
                            ////byte[] dataToWrite = ALawEncoder.ALawEncode(memStream.GetBuffer()); //ALAW (RECOMENdADO)
                            ////byte[] dataToWrite = memStream.GetBuffer(); //NORMAL

                            //byte[] dataToWrite;

                            //if (vocoder == Vocoder.ALaw)
                            //    dataToWrite = ALawEncoder.ALawEncode(memStream.GetBuffer()); //ALAW (RECOMENdADO)
                            //else if (vocoder == Vocoder.uLaw)
                            //    dataToWrite = MuLawEncoder.MuLawEncode(memStream.GetBuffer()); //MULAW
                            //else
                            //    dataToWrite = memStream.GetBuffer();

                            //if (bStop)
                            //    return;

                            byte[] elemento = DispatchData();
                            if (elemento != null)
                            {

                                byte[] dataToWrite;
                                if (vocoder == VoiceCommon.Vocoder.uLaw)
                                    dataToWrite = MuLawEncoder.MuLawEncode(elemento);
                                else if (vocoder == VoiceCommon.Vocoder.ALaw)
                                    dataToWrite = ALawEncoder.ALawEncode(elemento);
                                else
                                    dataToWrite = elemento;

                                //byte[] dataToWrite = MuLawEncoder.MuLawEncode(memStream.GetBuffer()); //MULAW
                                //byte[] dataToWrite = ALawEncoder.ALawEncode(memStream.GetBuffer()); //ALAW (RECOMENdADO)
                                //NORMAL

                             
                                 if (eMode == Mode.Server)
                                {
                                    for (int i = 0; i < users_count; i++)
                                    {
                                        udpClient.Send(elemento, elemento.Length, clientIPs[i].Address.ToString(), 1550);
                                    }

                                }


                                // }
                            }
                        }

                    }
                }

                cGlobalVars.AddLogChat("Sending Ended");


            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message, "VoiceChat-Send ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cGlobalVars.AddLogChat("VoiceChat-Send >> " + ex.Message);
            }
            finally
            {
                //captureBuffer.Stop();

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

        public int GetUsersCount()
        {
            lock (clientIPs)
            {
                return clientIPs.Count + 1;
            }
        }

        //BufferedWaveProvider bwp_micro;
        BufferedWaveProvider bwp_internet;

        WaveIn wi;
        WaveOut wo;

        void ResetVolume()
        {
            wo.Volume = 0;


            if (CallTrackBarValueChanged != null)
                CallTrackBarValueChanged(0);


        }


        public void ChangeVol(float Volume)
        {
            wo.Volume = Volume;
        }

        public void StartRecord()
        {
            wo = new WaveOut();
            wi = new WaveIn();
            wi.WaveFormat = new WaveFormat(11025, 8, 1);
            wi.DataAvailable += new EventHandler<WaveInEventArgs>(wi_DataAvailable);

            bwp_internet = new BufferedWaveProvider(wi.WaveFormat);
            bwp_internet.DiscardOnBufferOverflow = true;

            wo.Init(bwp_internet);
            wi.StartRecording();
            wo.Play();

            ResetVolume();
        }

        void StopRecording()
        {
            wi.StopRecording();
            wo.Stop();
        }


        void DestroyRecording()
        {
            StopRecording();
            wo.Dispose();
        }


        void wi_DataAvailable(object sender, WaveInEventArgs e)
        {
            //for (int index = 0; index < e.BytesRecorded; index += 2)
            //{
            //    short sample = (short)((e.Buffer[index + 1] << 8) |
            //                            e.Buffer[index + 0]);
            //    float sample32 = sample / 32768f;
            //    //ProcessSample(sample32);

            //}

            if (eMode == Mode.Server)
            {
                lock (_music_data)
                {
                    _music_data.Add(e.Buffer);
                }
            }


            //bwp_micro.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }



        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSendTo(ar);
            }
            catch (Exception ex)
            {
                cGlobalVars.AddLogChat("VoiceChat-OnSend () > " + ex.Message);
                // MessageBox.Show(ex.Message, "VoiceChat-OnSend ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /*
          * Send a message to the remote party.
          */
        private void SendMessage(VoiceCommon.Command cmd, EndPoint sendToEP)
        {
            try
            {
                //Create the message to send.
                VoiceCommon.Data msgToSend = new VoiceCommon.Data();

                msgToSend.strName = Nickname;   //Name of the user.
                msgToSend.cmdCommand = cmd;         //Message to send.
                msgToSend.vocoder = vocoder;        //Vocoder to be used.

                byte[] message = msgToSend.ToByte();

                //Send the message asynchronously.
                clientSocket.BeginSendTo(message, 0, message.Length, SocketFlags.None, sendToEP, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                cGlobalVars.AddLogChat("VoiceChat-SendMessage > " + ex.Message);
                //MessageBox.Show(ex.Message, "VoiceChat-SendMessage ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ChangeCodification(VoiceCommon.Vocoder codification)
        {
            if (codification == VoiceCommon.Vocoder.ALaw)
            {
                vocoder = VoiceCommon.Vocoder.ALaw;
                cGlobalVars.AddLogChat("Se ha modificado codificación a: ALaw");
                if (eMode == Mode.Server)
                {
                    lock (clientIPs)
                    {
                        for (int i = 0; i < clientIPs.Count; i++)
                            SendMessage(VoiceCommon.Command.ChangeVocoderALaw, clientIPs[i]);
                    }

                }
            }
            else if (codification == VoiceCommon.Vocoder.uLaw)
            {
                vocoder = VoiceCommon.Vocoder.uLaw;
                cGlobalVars.AddLogChat("Se ha modificado codificación a: uLaw");

                if (eMode == Mode.Server)
                {
                    lock (clientIPs)
                    {
                        for (int i = 0; i < clientIPs.Count; i++)
                            SendMessage(VoiceCommon.Command.ChangeVocoderuLaw, clientIPs[i]);
                    }

                }

            }
            else
            {
                vocoder = VoiceCommon.Vocoder.None;
                cGlobalVars.AddLogChat("Se ha modificado codificación a: None");
                if (eMode == Mode.Server)
                {
                    lock (clientIPs)
                    {
                        for (int i = 0; i < clientIPs.Count; i++)
                            SendMessage(VoiceCommon.Command.ChangeVocoderNone, clientIPs[i]);
                    }

                }

            }

        }


        // List<capture_device> _devices = new List<capture_device>();

        public static List<capture_device> GetDevices()
        {
            List<capture_device> _devices = new List<capture_device>();
            int waveInDevices = WaveIn.DeviceCount;
            for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                //LogAppend(String.Format("Device {0}: {1}, {2} channels",
                //    waveInDevice, deviceInfo.ProductName, deviceInfo.Channels));
                _devices.Add(new capture_device()
                {
                    Description = deviceInfo.ProductName,
                    Channels = deviceInfo.Channels,
                    WaveDevice = waveInDevice
                });

            }
            return _devices;
        }


        public delegate void ChangeButtonTextHandler(string text);
        public event ChangeButtonTextHandler CallChangeButtonText;

        public delegate void ModeClientConnectedHandler();
        public event ModeClientConnectedHandler CallModeClientConnected;


        public delegate void TrackBarValueChangedHandler(int value);
        public event TrackBarValueChangedHandler CallTrackBarValueChanged;



    }
}
