using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.DirectSound;
using System.Threading;

namespace app_VoiceChatServer
{
    class OldVoiceChat
    {

        private CaptureBufferDescription captureBufferDescription;
     
        private Notify notify;
        private WaveFormat waveFormat;
        private Capture capture;
        private int bufferSize;
        private CaptureBuffer captureBuffer;
        private SecondaryBuffer playbackBuffer;
        private BufferDescription playbackBufferDescription;
        private Device device;
     

        private void Initialize()
        {

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

        private AutoResetEvent autoResetEvent;

        private void InicializeCaptureSound()
        {
            device = new Device();
            //device.SetCooperativeLevel(this, CooperativeLevel.Normal);

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
               // MessageBox.Show(ex.Message, "VoiceChat-CreateNotifyPositions ()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
