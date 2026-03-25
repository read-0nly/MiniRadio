
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MiniRadio
{
    public partial class MiniRadio : Form
    {

        public MiniRadio()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool failed = false;
            string[] ipstr = ipBox.Text.Replace(" ", "").Replace("\t", "").Split(":")[0].Split(".");
            if (ipstr.Length != 4)
            {
                failed = true;
                ipBox.BackColor = Color.Salmon;
            }
            byte[] ipBytes = null;
            try
            {
                ipBytes = new byte[] {
                (byte)Int32.Parse(ipstr[0]),
                (byte)Int32.Parse(ipstr[1]),
                (byte)Int32.Parse(ipstr[2]),
                (byte)Int32.Parse(ipstr[3])
                };

            }
            catch { failed = true; ipBox.BackColor = Color.Salmon; }
            if (ipBytes != null)
            {
                ipBox.BackColor = Color.PaleGreen;
            }

            int port = 0;
            try
            {
                port = Int32.Parse(portBox.Text);
                portBox.BackColor = Color.PaleGreen;
            }
            catch { failed = true; portBox.BackColor = Color.Salmon; }

            if (!Directory.Exists(folderBox.Text)) { failed = true; folderBox.BackColor = Color.Salmon; }
            else { folderBox.BackColor = Color.MintCream; }

            if (failed) { return; }

            MusicServer.folder = folderBox.Text;
            MusicServer.bindIPBytes = ipBytes;
            MusicServer.bindPort = port;

            MusicServer.Run();
            Thread.Sleep(1000);
            albumCountLabel.Text += MusicServer.playheads.Keys.Count().ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MusicServer.Kill();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MusicServer.Kill();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                folderBox.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }


    public class MP3Parser
    {
        List<List<List<int>>> BitrateTable = new List<List<List<int>>>();
        List<List<int>> SampleTable = new List<List<int>>();
        public byte[] fileBytes;
        public int fileByteIdx = 0;
       
        
        public MP3Parser(byte[] fb)
        {
            for (int i = 0; i < 2; i++)
            {
                BitrateTable.Add(new List<List<int>>());
                for (int j = 0; j < 3; j++)
                {
                    BitrateTable[i].Add(new List<int>());
                }

            }
            BitrateTable[0][0] = new List<int>(new int[]{0, 32, 64, 96, 128, 160, 192, 224, 256, 288, 320, 352, 384, 416, 448,0 });
            BitrateTable[0][1] = new List<int>(new int[] {0, 32, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 384,0 });
            BitrateTable[0][2] = new List<int>(new int[] {0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320,0 });
            BitrateTable[1][0] = new List<int>(new int[] {0, 32, 48, 56, 64, 80, 96, 112, 128, 144, 160, 176, 192, 224, 256,0 });
            BitrateTable[1][1] = new List<int>(new int[] {0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160,0 });
            BitrateTable[1][2] = new List<int>(new int[] {0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160,0 });
            SampleTable.Add(new List<int>(new int[] { 44100, 48000, 32000 }));
            SampleTable.Add(new List<int>(new int[] { 22050, 24000, 16000 }));
            fileBytes = fb;
        } //Generates the value tables and loads the file bytes for reading
       
        
        public byte[] NextFrame()
        {
            List<byte> result = new List<byte>();
            if (fileBytes == null) { return result.ToArray(); }
            while (fileByteIdx < fileBytes.Length - 3)
            {
                //if ($bytes[$i] - eq 0xFF - and($bytes[$i + 1] - band 0xF0) - eq 0xF0){
                bool increment = true;
                if (fileBytes[fileByteIdx] == 0xFF &&
                    (fileBytes[fileByteIdx+1] & 0xF0) == 0xF0)
                {
                    FrameInfo frame = new FrameInfo();
                    frame.version = GetVersion(fileBytes[fileByteIdx + 1]);
                    frame.layer = GetLayer(fileBytes[fileByteIdx + 1]);
                    frame.bitRate = GetBitrate(fileBytes[fileByteIdx + 2],frame.version, frame.layer);
                    frame.sampleRate = GetSample(fileBytes[fileByteIdx + 2], frame.version, frame.layer);
                    frame.padding = GetPadding(fileBytes[fileByteIdx + 2]);
                    frame.size = frame.GetFrameSize();
                    if(frame.size>0 && frame.size+fileByteIdx< fileBytes.Length - 12)
                    {
                        if (fileBytes[frame.size + fileByteIdx] == 0xFF &&
                            (fileBytes[frame.size + fileByteIdx+1] & 0xF0) == 0xF0)
                        {
                            for(int i = 0; i < frame.size; i++)
                            {
                                result.Add(fileBytes[fileByteIdx + i]);
                            }
                            fileByteIdx += frame.size; //FUCKING GOT YOU YOU BASTARD
                            break;
                        }

                    }
                }
                fileByteIdx++;
            }
            return result.ToArray();
        } //Get the next frame of the file


        #region BitFuckery //Functions to extract frame metadata
        int GetVersion(byte b)
        {
            if ((b & 8) == 8)
            {
                return 1;
            }
            return 2;
        } //MPEG version
        int GetLayer(byte b)
        {
            int layerflag = (b & 6) >> 1;
            return 4 - layerflag;
        } //MPEG layer
        int GetBitrate(byte b, int version, int layer)
        {
            if (version == 0 || layer == 0) { return 0; }
            int bitflag = b >> 4;
            if (layer > 3) { return 0; }
            return BitrateTable[version - 1][layer - 1][bitflag];
        } //Bitrate for this frame (VBR should work)
        int GetSample(byte b, int version, int layer)
        {
            if (version == 0 || layer == 0) { return 0; }
            int sample = (b & 0x0c) >> 2;
            if (sample > 2) { return 0; }
            return SampleTable[version - 1][sample];
        } //Sample rate for this frame 
        int GetPadding(byte b)
        {
            return (b & 0x02) >> 1;
        } //Include a padding bit to the frame bounds?
        #endregion


        public class FrameInfo
        {
            public int version = 0;
            public int layer = 0;
            public int bitRate = 0;
            public int sampleRate = 0;
            public int padding = 0;
            public int size = 0;

            public int GetFrameSize()
            {
                int frameLength = 0;
                if (bitRate == 0 || sampleRate == 0) { return 0; }
                if (layer == 1)
                {
                    frameLength = (int)((12 * bitRate * 1000.0f / sampleRate) + padding) * 4;
                }
                else
                {
                    frameLength = (int)(((144 * (bitRate * 1000.0f)) / sampleRate) + padding);
                }
                return frameLength;
            }
        } //Frame metadata, mostly to calc frame bounds

    }
    public class MusicServer
    {
        public static Dictionary<string, Dictionary<string, string>> albums = new Dictionary<string, Dictionary<string, string>>();
        public static string folder = "";
        public static bool keepRunning = false;
        public static byte[] bindIPBytes = new byte[4];
        public static int bindPort = 8001;
        public static Socket listeningSocket = null;
        public static Dictionary<string, Playhead> playheads = new Dictionary<string, Playhead>();

        public static void Run()
        {
            Playhead.ScanMusic();
            keepRunning = true;
            Thread StaticCaller = new(new ThreadStart(Playhead.ProcessIncoming));
            StaticCaller.Start();
            Thread StaticCaller2 = new(new ThreadStart(Playhead.ProcessPlayheadHandshakes));
            StaticCaller2.Start();
            Thread StaticCaller3 = new(new ThreadStart(Playhead.ProcessPlayheadQueues));
            StaticCaller3.Start();

        } //Starts the thread workers
        public static void Kill()
        {
            keepRunning = false;
            if(listeningSocket!=null)
                listeningSocket.Close();
        } //Kills the server


        public class Connection
        {
            public Socket socket;
            public string request;
        } //Represents an client connection and request
        public class Playhead
        {
            public Dictionary<string, string> album = null;
            public int currentSongIdx = 0;
            public byte[] currentSong = null;
            MP3Parser currentSongParser = null;
            public int bytesSent = 0;
            public Queue<Connection> newSockets = new Queue<Connection>();
            public Queue<Connection> needsUpdate = new Queue<Connection>();
            public Queue<Connection> beenUpdated = new Queue<Connection>();
            public Queue<Connection> abandoned = new Queue<Connection>();

            public Playhead(Dictionary<string, string> album_input)
            {
                bindIPBytes[0] = 127;
                bindIPBytes[1] = 0;
                bindIPBytes[2] = 0;
                bindIPBytes[3] = 1;
                album = album_input;
                currentSong = File.ReadAllBytes(album[album.Keys.ToArray<string>()[currentSongIdx]]);
                currentSongParser = new MP3Parser(currentSong);
            }


            public static void ScanMusic()
            {
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(folder));
                foreach (string d in dirs)
                {
                    string[] foldersplit = d.Split("\\");
                    string folkey = Uri.EscapeDataString(foldersplit[foldersplit.Length - 1]);
                    List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(d, "*.mp3", SearchOption.TopDirectoryOnly));
                    if (!albums.ContainsKey(folkey))
                    {
                        albums.Add(folkey, new Dictionary<string, string>());
                    }
                    Console.WriteLine("Folder : " + folkey);
                    foreach (string f in files)
                    {
                        string[] filesplit = f.Split("\\");
                        string filekey = Uri.EscapeDataString(filesplit[filesplit.Length - 1]);
                        if (!albums[folkey].ContainsKey(filekey))
                        {
                            albums[folkey].Add(filekey, f); //
                        }
                        Console.WriteLine("  File : " + filekey);

                    }
                    Playhead playhead = new Playhead(albums[folkey]);
                    lock (playheads)
                    {
                        playheads.Add(folkey, playhead);
                    }
                }
            }//Scans for music and creates the necessary playheads


            public static void ProcessPlayheadHandshakes()
            {
                while (keepRunning)
                {
                    foreach (Playhead p in playheads.Values)
                    {
                        p.Handshakes();
                    }
                }
            }//Thread worker that handles handshakes for all playheads (sends HTTP header)
            public static void ProcessPlayheadQueues()
            {
                while (keepRunning)
                {
                    DateTime centuryBegin = DateTime.Now;
                    foreach (Playhead p in playheads.Values)
                    {
                        p.ProcessQueue();
                    }
                    DateTime currentDate = DateTime.Now;

                    long elapsedTicks = currentDate.Ticks - centuryBegin.Ticks;
                    TimeSpan elapsedSpan = new TimeSpan(elapsedTicks);
                    if (240 - (int)elapsedSpan.TotalMilliseconds > 0) { Thread.Sleep(240 - (int)elapsedSpan.TotalMilliseconds); }
                }

            }//Thread worker that kicks off queue processing for all handshaken connections
            public static void ProcessIncoming()
            {
                if (listeningSocket == null)
                {
                    // Create and connect a dual-stack socket
                    listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    //listeningSocket.Accept();
                    listeningSocket.Bind(new IPEndPoint(new IPAddress(MusicServer.bindIPBytes), bindPort));
                    listeningSocket.Listen();

                }
                while (keepRunning)
                {
                    try
                    {
                        Socket socket = listeningSocket.Accept();
                        Connection incoming = new Connection() { socket = socket };

                        byte[] responseBytes = new byte[256];
                        char[] responseChars = new char[256];

                        int bytesReceived = socket.Receive(responseBytes);
                        if (bytesReceived != 0)
                        {
                            int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                            Console.WriteLine(String.Join("", responseChars));
                            incoming.request = String.Join("", responseChars);
                            string path = incoming.request.Split("\n")[0].Split(" ")[1];
                            string[] pathSplit = path.Split("/");
                            lock (playheads)
                            {
                                if (playheads.ContainsKey(pathSplit[1].Replace("/", "")))
                                {
                                    lock (playheads[pathSplit[1].Replace("/", "")].newSockets)
                                    {
                                        playheads[pathSplit[1].Replace("/", "")].newSockets.Enqueue(incoming);
                                    }
                                }
                                else
                                {
                                    socket.Close();
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }

            }//Thread worker that adds incoming connections to the requested playhead
           

            public void Handshakes()//Process incoming connections and add them to the music queue for the right album
            {
                while (newSockets.Count() > 0)
                {
                    Connection connection = newSockets.Dequeue();
                    Socket socket = connection.socket;


                    List<byte> requestBytesAudio = new List<byte>(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nServer: Your mom\r\n" +
                                       "Content-Type: audio/mpeg\r\nConnection: keep-alive\r\nKeep-Alive: timeout=200, max=200\r\n"));
                    requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("Transfer-Encoding: chunked\r\n"));
                    requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("\r\n"));
                    int bytesSent1 = 0;
                    byte[] requestBytesAudioArr = requestBytesAudio.ToArray();
                    while (bytesSent1 < requestBytesAudioArr.Length)
                    {
                        bytesSent1 += socket.Send(requestBytesAudioArr, bytesSent1, requestBytesAudioArr.Length - bytesSent1, SocketFlags.None);
                    }
                    lock (connection)
                    {
                        lock (needsUpdate)
                        {
                            needsUpdate.Enqueue(connection);
                        }
                    }
                }
            }
            public void ProcessQueue()
            {
                Socket socket = null;
                lock (needsUpdate)
                {
                    if (currentSongParser != null)
                    {
                        if (needsUpdate.Count() > 0)
                        {
                            if (currentSongParser.fileByteIdx < currentSongParser.fileBytes.Length - 32)
                            {
                                List<byte> nextFrameList = new List<byte>();
                                for(int i = 0; i < 10; i++)
                                {
                                    nextFrameList.AddRange(currentSongParser.NextFrame());
                                }
                                    
                                byte[] nextFrame = nextFrameList.ToArray();
                                int len = nextFrame.Length;
                                byte[] lentag = Encoding.ASCII.GetBytes(len.ToString("X4") + "\r\n");
                                while (needsUpdate.Count() > 0 && len>0)
                                {

                                    Connection connection = needsUpdate.Dequeue();
                                    socket = connection.socket;
                                    try
                                    {
                                        string path = connection.request.Split("\n")[0].Split(" ")[1];
                                        string[] pathSplit = path.Split("/");

                                        socket.Send(lentag, 0, lentag.Length, SocketFlags.None);
                                        int bytesSent1 = 0;
                                        while (bytesSent1 < nextFrame.Length)
                                        {
                                            bytesSent1 += socket.Send(nextFrame, bytesSent1, nextFrame.Length - bytesSent1, SocketFlags.None);
                                        }
                                        socket.Send(Encoding.ASCII.GetBytes("\r\n"), 0, 2, SocketFlags.None);

                                        lock (connection)
                                        {
                                            lock (beenUpdated)
                                            {
                                                beenUpdated.Enqueue(connection);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        lock (abandoned)
                                        {
                                            abandoned.Enqueue(connection);
                                        }
                                    }
                                }
                                bytesSent += len;
                            }
                            else
                            {
                                currentSongIdx++;
                                if(currentSongIdx>= album.Keys.Count())
                                {
                                        currentSongIdx = 0;
                                }
                                currentSong = File.ReadAllBytes(album[album.Keys.ToArray<string>()[currentSongIdx]]);
                                currentSongParser = new MP3Parser(currentSong);
                                bytesSent = 0;
                            }
                        }
                    }
                    else
                    {
                        if (album.Keys.Count() > 0)
                        {
                            currentSongIdx = 0;
                            currentSong = File.ReadAllBytes(album[album.Keys.ToArray<string>()[currentSongIdx]]);
                            currentSongParser = new MP3Parser(currentSong);
                        }
                    }

                    lock (beenUpdated)
                    {
                            lock (abandoned)
                            {

                                while (beenUpdated.Count() > 0)
                                {

                                    Connection connection = beenUpdated.Dequeue();
                                    if (connection.socket.Connected)
                                    {
                                        needsUpdate.Enqueue(connection);
                                    }
                                    else
                                    {
                                        abandoned.Enqueue(connection);
                                    }
                                }
                            }
                        }
                    lock (abandoned)
                    {
                        while (abandoned.Count() > 0)
                        {
                            Connection connection = abandoned.Dequeue();
                            connection.socket.Close();
                        }
                    }
                }
            } // Send next frame to all listening connections on this playhead
        } //Represents a single running album. Each album with listeners gets processed independently of eachother
        
    }


}





/*
        public void Start()
        {
            List<string> dirs = new List<string>(Directory.EnumerateDirectories("c:\\rustserver\\oxide\\media"));
            foreach (string d in dirs)
            {
                string[] foldersplit = d.Split("\\");
                string folkey = Uri.EscapeDataString(foldersplit[foldersplit.Length - 1]);
                List<string> files = new List<string>(System.IO.Directory.EnumerateFiles(d, "*.mp3", SearchOption.TopDirectoryOnly));
                if (!albums.ContainsKey(folkey))
                {
                    albums.Add(folkey, new Dictionary<string, string>());
                }
                Console.WriteLine("Folder : " + folkey);
                foreach (string f in files)
                {
                    string[] filesplit = f.Split("\\");
                    string filekey = Uri.EscapeDataString(filesplit[filesplit.Length - 1]);
                    if (!albums[folkey].ContainsKey(filekey))
                    {
                        albums[folkey].Add(filekey, f); //
                    }
                    Console.WriteLine("  File : " + filekey);

                }
            }
            keepRunning = true;
            if (StaticCaller == null)
            {
                StaticCaller = new(new ThreadStart(MusicServer.RunSocketServer));
                StaticCaller.Start();
            }
        }
        public void Stop()
        {
            keepRunning = false;
            if (StaticCaller != null)
            {
                StaticCaller = null;
            }
        }

        public static void ProcessSocket(Socket socket)
        {
            byte[] responseBytes = new byte[256];
            char[] responseChars = new char[256];
            int bytesReceived = socket.Receive(responseBytes);
            if (bytesReceived != 0)
            {
                int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                Console.WriteLine(String.Join("", responseChars));
                string internalmsg = String.Join("", responseChars);
                try
                {
                    string path = internalmsg.Split("\n")[0].Split(" ")[1];
                    string[] pathSplit = path.Split("/");
                    switch (pathSplit.Length)
                    {
                        case 3:
                            if (albums.ContainsKey(pathSplit[1].Replace("/", "")))
                            {
                                if (albums[pathSplit[1].Replace("/", "")].ContainsKey(pathSplit[2].Replace("/", "")))
                                {
                                    msg = internalmsg;
                                    List<byte> requestBytesAudio = new List<byte>(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nServer: Your mom\r\n" +
                                        "Connection: keep-alive\r\nKeep-Alive: timeout=200, max=200\r\nContent-Type: audio/mpeg\r\n"));
                                    byte[] audiobytes = File.ReadAllBytes(albums[pathSplit[1].Replace("/", "")][pathSplit[2].Replace("/", "")]);
                                    string audiob64 = Convert.ToBase64String(audiobytes);
                                    byte[] audiob64bytes = Encoding.ASCII.GetBytes(audiob64);
                                    int length = audiob64bytes.Length;
                                    requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("Transfer-Encoding: chunked\r\n"));
                                    requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("\r\n"));
                                    int bytesSent1 = 0;
                                    byte[] requestBytesAudioArr = requestBytesAudio.ToArray();
                                    while (bytesSent1 < requestBytesAudioArr.Length)
                                    {
                                        bytesSent1 += socket.Send(requestBytesAudioArr, bytesSent1, requestBytesAudioArr.Length - bytesSent1, SocketFlags.None);
                                    }
                                    bytesSent1 = 0;
                                    int loops = 0;
                                    while (bytesSent1 < audiobytes.Length && loops < 3 && socket.Connected)
                                    {

                                        int len = (audiobytes.Length - bytesSent1 > packetsize ? packetsize : audiobytes.Length - bytesSent1);
                                        byte[] lentag = Encoding.ASCII.GetBytes(len.ToString("X4") + "\r\n");
                                        socket.Send(lentag, 0, lentag.Length, SocketFlags.None);
                                        bytesSent1 += socket.Send(audiobytes, bytesSent1, len, SocketFlags.None);
                                        socket.Send(Encoding.ASCII.GetBytes("\r\n"), 0, 2, SocketFlags.None);
                                        if (bytesSent1 == audiobytes.Length) { bytesSent1 = 0; loops++; }

                                    }
                                }
                                else
                                {
                                    socket.Close();
                                }
                            }
                            else
                            {
                                socket.Close();
                            }
                            break;
                        case 2:
                            if (albums.ContainsKey(pathSplit[1].Replace("/", "")))
                            {
                                int songidx = 0;
                                Dictionary<string, string> album = albums[pathSplit[1].Replace("/", "")];
                                msg = internalmsg;
                                List<byte> requestBytesAudio = new List<byte>(Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nServer: Your mom\r\n" +
                                    "Connection: keep-alive\r\nKeep-Alive: timeout=200, max=200\r\nContent-Type: audio/mpeg\r\n"));
                                requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("Transfer-Encoding: chunked\r\n"));
                                requestBytesAudio.AddRange(Encoding.ASCII.GetBytes("\r\n"));
                                byte[] requestBytesAudioArr = requestBytesAudio.ToArray();
                                int bytesSent2 = 0;
                                while (bytesSent2 < requestBytesAudioArr.Length)
                                {
                                    bytesSent2 += socket.Send(requestBytesAudioArr, bytesSent2, requestBytesAudioArr.Length - bytesSent2, SocketFlags.None);
                                }
                                while (songidx < album.Keys.Count())
                                {
                                    byte[] audiobytes = File.ReadAllBytes(album[album.Keys.ToArray()[songidx]]);
                                    string audiob64 = Convert.ToBase64String(audiobytes);
                                    byte[] audiob64bytes = Encoding.ASCII.GetBytes(audiob64);
                                    int length = audiob64bytes.Length;
                                    bytesSent2 = 0;
                                    while (bytesSent2 < audiobytes.Length && socket.Connected)
                                    {

                                        int len = (audiobytes.Length - bytesSent2 > packetsize ? packetsize : audiobytes.Length - bytesSent2);
                                        byte[] lentag = Encoding.ASCII.GetBytes(len.ToString("X4") + "\r\n");
                                        socket.Send(lentag, 0, lentag.Length, SocketFlags.None);
                                        bytesSent2 += socket.Send(audiobytes, bytesSent2, len, SocketFlags.None);
                                        socket.Send(Encoding.ASCII.GetBytes("\r\n"), 0, 2, SocketFlags.None);

                                    }
                                    songidx++;
                                }
                                byte[] tagclose = Encoding.ASCII.GetBytes("0\r\n\r\n");
                                socket.Send(tagclose, 0, tagclose.Length, SocketFlags.None);
                            }
                            break;
                        case 1:
                            string data = "Hello there!";
                            byte[] buffer = Encoding.UTF8.GetBytes(data);
                            break;
                    }
                    msg = internalmsg;
                }
                catch (Exception e)
                {
                    msg = "Exception!!";
                    msg += "\n" + e.Message;
                    socket.Close();
                }



            }

        }
        public static void RunSocketServer()
        {
            Uri uri = new Uri("http://localhost:8001");
            
            byte[] localhostbytes = new byte[4];
            localhostbytes[0] = 127;
            localhostbytes[1] = 0;
            localhostbytes[2] = 0;
            localhostbytes[3] = 1;
            // Create and connect a dual-stack socket
            Socket listeningSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //listeningSocket.Accept();
            listeningSocket.Bind(new IPEndPoint(new IPAddress(localhostbytes), 8001));
            listeningSocket.Listen();


            while (keepRunning)
            {
                Socket socket = listeningSocket.Accept();
                ProcessSocket(socket);
            }
            listeningSocket.Close();
            return;
        }
        public static void RunServer()
        {

            using var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8001/");//

            listener.Start();

            Console.WriteLine("Listening on port 8001...");

            try
            {
                while (keepRunning)
                {
                    HttpListenerContext ctx = listener.GetContext();
                    using HttpListenerResponse resp = ctx.Response;

                    resp.StatusCode = (int)HttpStatusCode.OK;
                    //resp.StatusDescription = "Status OK";


                    foreach (string s in ctx.Request.Url.Segments)
                    {
                        Console.WriteLine("Segment: " + s);
                    }
                    
                    Stream ros = resp.OutputStream;
                    switch (ctx.Request.Url.Segments.Length)
                    {
                        case 3:
                            if (albums.ContainsKey(ctx.Request.Url.Segments[1].Replace("/", "")))
                            {
                                if (albums[ctx.Request.Url.Segments[1].Replace("/", "")].ContainsKey(ctx.Request.Url.Segments[2].Replace("/", "")))
                                {
                                    resp.ContentType = "audio/mpeg";
                                    resp.AddHeader("Accept-Ranges", "none");
                                    resp.AddHeader("cache-control", "no-cache, no-store");

                                    byte[] audiobytes = File.ReadAllBytes(albums[ctx.Request.Url.Segments[1].Replace("/", "")][ctx.Request.Url.Segments[2].Replace("/", "")]);
                                    resp.ContentLength64 = Convert.ToBase64String(audiobytes).Length;

                                    using (FileStream fileStream = File.OpenRead(albums[ctx.Request.Url.Segments[1].Replace("/", "")][ctx.Request.Url.Segments[2].Replace("/", "")]))
                                    {
                                        byte[] buffer22 = new byte[8192];
                                        int bytesRead;
                                        while ((bytesRead = fileStream.Read(buffer22, 0, buffer22.Length)) > 0)
                                        {
                                            resp.OutputStream.Write(buffer22, 0, bytesRead);
                                            resp.OutputStream.Flush();
                                        }
                                    }

                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                }
                                else
                                {
                                    resp.StatusCode = 404;
                                }
                            }
                            else
                            {
                                resp.StatusCode = 404;
                            }
                            break;
                        case 2:
                            resp.ContentType = "text/plain; charset=utf-8";
                            string data2 = "Hello there! This is a playlist";
                            byte[] buffer2 = Encoding.UTF8.GetBytes(data2);
                            resp.ContentLength64 = buffer2.Length;
                            ros.Write(buffer2, 0, buffer2.Length);
                            break;
                        case 1:
                            resp.ContentType = "text/plain; charset=utf-8";
                            string data = "Hello there!";
                            byte[] buffer = Encoding.UTF8.GetBytes(data);
                            resp.ContentLength64 = buffer.Length;
                            ros.Write(buffer, 0, buffer.Length);
                            break;
                    }
                    //ros.Dispose();
                    resp.Close();
                }
                listener.Close();
            }
            catch (ThreadAbortException ex)
            {
                Console.WriteLine("ThreadAbortException!!!!");
                listener.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception!!!!");
                listener.Close();

            }
        }
        public static void RunTCPServer()
        {
            var hostName = "localhost";
            IPHostEntry localhost = Dns.GetHostEntry(hostName);
            byte[] localhostbytes = new byte[4];
            localhostbytes[0] = 127;
            localhostbytes[1] = 0;
            localhostbytes[2] = 0;
            localhostbytes[3] = 1;
            // This is the IP address of the local machine

            IPAddress localIpAddress = new IPAddress(localhostbytes);
            var ipEndPoint = new IPEndPoint(IPAddress.Any, 8001);
            TcpListener listener = new(ipEndPoint);

            try
            {
                listener.Start();

                TcpClient handler = listener.AcceptTcpClient();
                NetworkStream stream = handler.GetStream();

                var message = $"📅 {DateTime.Now} 🕛";
                var dateTimeBytes = Encoding.UTF8.GetBytes(message);
                stream.Write(dateTimeBytes);

                Console.WriteLine($"Sent message: \"{message}\"");
                // Sample output:
                //     Sent message: "📅 8/22/2022 9:07:17 AM 🕛"
            }
            finally
            {
                listener.Stop();
            }
        }
        */