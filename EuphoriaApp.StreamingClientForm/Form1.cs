using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace EuphoriaApp.StreamingClientForm
{
    public partial class Form1 : Form
    {
        private readonly BoyerMoore boyerMooreAlg = new BoyerMoore();
        private readonly byte[] startOfFile = Encoding.UTF8.GetBytes("<|SOF|>");
        private readonly byte[] endOfFile = Encoding.UTF8.GetBytes("<|EOF|>");
        private readonly IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, 51333);
        private readonly TcpClient tcpClient = new TcpClient();

        private List<ImageFile> imageArray = new List<ImageFile>();
        private ImageFile tempImage = null;
        private List<byte> tempDataHolder = new List<byte>();
        private LibVLC _libVLC;
        private MediaPlayer _mp;

        public Form1()
        {
            InitializeComponent();
            Core.Initialize();
            this.KeyPreview = true;
            _libVLC = new LibVLC();
            _mp = new MediaPlayer(_libVLC);
            videoView1.MediaPlayer = _mp;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (imageArray.Any())
            {
                using (MemoryStream ms = new MemoryStream(imageArray.Last().Bytes.ToArray()))
                {
                    using (var media = new Media(_libVLC, new StreamMediaInput(ms)))
                    {
                        var videoPath = string.Concat(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "\\video.mp4");
                        File.WriteAllBytes(videoPath, ms.ToArray());
                        var isSuccessful = _mp.Play(new Media(_libVLC, new Uri(videoPath)));
                        btnStatus.Text = $"Status: {isSuccessful}, Playing: {_mp.IsPlaying}";
                    }
                }
            }
        }

        private void btnStartListening_Click(object sender, EventArgs e)
        {
            Task.Run(StartListening);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _mp.Stop();
            _mp.Dispose();
            _libVLC.Dispose();

            tcpClient.Close();
            tcpClient.Dispose();
        }

        private async Task StartListening()
        {
            tcpClient.Connect(remoteEP);
            using (NetworkStream stream = tcpClient.GetStream())
            {
                while (true)
                {
                    byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
                    var bytesReceived = tcpClient.Client.Receive(buffer);
                    if (bytesReceived > 0)
                    {
                        tempDataHolder.AddRange(buffer);
                        PrepareBufferData();
                    }
                }
            }
        }

        private void PrepareBufferData()
        {
            var startOfFileIndexes = boyerMooreAlg.SearchAll(tempDataHolder.ToArray(), startOfFile);
            var endOfFileIndexes = boyerMooreAlg.SearchAll(tempDataHolder.ToArray(), endOfFile);
            if (endOfFileIndexes.Any() && startOfFileIndexes.Any())
            {
                List<int> ignoreList = new List<int>();
                if (endOfFileIndexes.Min() < startOfFileIndexes.Min() && tempImage != null)
                {
                    // if buffer starts with <|EOF|>, it means file start is in before buffer
                    tempImage.IsFinished = true;
                    tempImage.Bytes.AddRange(tempDataHolder.Take(endOfFileIndexes.Min()));
                    imageArray.Add(tempImage);
                    ignoreList.Add(endOfFileIndexes.Min());
                }

                if (endOfFileIndexes.Max() < startOfFileIndexes.Max())
                {
                    // if buffer ends with <|SOF|>, it means file end is in after buffer
                    var startIndex = startOfFileIndexes.Max() + startOfFile.Length;
                    tempImage = new ImageFile
                    {
                        IsStarted = true,
                        IsFinished = false,
                        Bytes = tempDataHolder.Skip(startIndex).Take(tempDataHolder.Count - startIndex).ToList()
                    };
                    ignoreList.Add(startOfFileIndexes.Max());
                }

                foreach (var start in startOfFileIndexes.Where(f => !ignoreList.Contains(f)))
                {
                    var end = endOfFileIndexes.Where(f => !ignoreList.Contains(f) && f > start).Min();
                    var startIndex = start + startOfFile.Length;
                    var newImage = new ImageFile
                    {
                        IsStarted = true,
                        IsFinished = true,
                        Bytes = tempDataHolder.Skip(startIndex).Take(end - startIndex).ToList()
                    };
                    imageArray.Add(newImage);
                }
            }
            else if (startOfFileIndexes.Any())
            {
                // if buffer only has <|SOF|>
                var startIndex = startOfFileIndexes.FirstOrDefault() + startOfFile.Length;
                tempImage = new ImageFile
                {
                    IsStarted = true,
                    IsFinished = false,
                    Bytes = tempDataHolder.Skip(startIndex).Take(tempDataHolder.Count - startIndex).ToList()
                };
            }
            else if (endOfFileIndexes.Any())
            {
                // if buffer only has <|EOF|> 
                if (tempImage != null)
                {
                    tempImage.IsFinished = true;
                    tempImage.Bytes.AddRange(tempDataHolder.Take(endOfFileIndexes.FirstOrDefault()));
                    imageArray.Add(tempImage);
                }
            }
            else
            {
                if (tempImage != null)
                {
                    tempImage.Bytes.AddRange(tempDataHolder);
                }
            }
        }
    }
}

public class ImageFile
{
    public bool IsStarted { get; set; }

    public bool IsFinished { get; set; }

    public List<byte> Bytes { get; set; } = new List<byte>();
}