using ScreenRecorderLib;
using System.Net;
using System.Net.Sockets;
using System.Text;

internal class Program
{
    #region Fields
    private static Recorder _rec;
    private static byte[] videoByteArray;
    private static MemoryStream videoStream = new MemoryStream();
    private static NetworkStream nwStream;
    private static int VIDEO_LENGTH = 2000;
    private static byte[] startOfFile = Encoding.UTF8.GetBytes("<|SOF|>");
    private static byte[] endOfFile = Encoding.UTF8.GetBytes("<|EOF|>");
    private static readonly IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, 51333);
    #endregion
    private static void Main(string[] args)
    {
        using (TcpClient tcpClient = new TcpClient())
        {
            tcpClient.Connect(remoteEP);
            nwStream = tcpClient.GetStream();
            while (true)
            {
                RecordScreen();
            }
        }
    }

    static void RecordScreen()
    {
        var inputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.InputDevices);
        var outputDevices = Recorder.GetSystemAudioDevices(AudioDeviceSource.OutputDevices);
        var selectedInputDevice = inputDevices.FirstOrDefault();
        var selectedOutputDevice = outputDevices.FirstOrDefault();
        var mainMonitor = new DisplayRecordingSource(DisplayRecordingSource.MainMonitor);
        _rec = Recorder.CreateRecorder(
            new RecorderOptions
            {
                SourceOptions = new SourceOptions
                {
                    RecordingSources = new List<RecordingSourceBase>() { mainMonitor },
                },
                VideoEncoderOptions = new VideoEncoderOptions
                {
                    IsFragmentedMp4Enabled = true,
                    Framerate = 60,
                    Bitrate = 8000 * 1000,
                    IsFixedFramerate = true,
                    Encoder = new H264VideoEncoder
                    {
                        BitrateMode = H264BitrateControlMode.CBR,
                        EncoderProfile = H264Profile.Main,
                    },
                },
                AudioOptions = new AudioOptions
                {
                    IsAudioEnabled = true,
                    IsInputDeviceEnabled = false,
                    InputVolume = 1,
                    OutputVolume = 1,
                    AudioOutputDevice = selectedOutputDevice.DeviceName,
                    AudioInputDevice = selectedInputDevice.DeviceName,
                    Bitrate = AudioBitrate.bitrate_128kbps,
                    Channels = AudioChannels.Stereo,
                },
                OutputOptions = new OutputOptions
                {
                    IsVideoCaptureEnabled = true,
                    RecorderMode = RecorderMode.Video,
                    OutputFrameSize = new ScreenSize(1920, 1080),
                    Stretch = StretchMode.Uniform,
                }
            });
        _rec.OnRecordingComplete += Rec_OnRecordingComplete;
        _rec.Record(videoStream);
        Task.Delay(VIDEO_LENGTH).Wait();
        _rec.Stop();
    }

    static void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
    {
        videoByteArray = videoStream.ToArray();
        if (videoByteArray != null && videoByteArray.Length > 0)
        {
            var fileBytes = new List<byte>();
            fileBytes.AddRange(startOfFile);
            fileBytes.AddRange(videoByteArray);
            fileBytes.AddRange(endOfFile);
            nwStream.Write(fileBytes.ToArray(), 0, fileBytes.Count);
        }
    }
}