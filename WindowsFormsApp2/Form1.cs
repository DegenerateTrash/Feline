using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using YouTubeSearch;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Collections.Generic;
using NAudio;
using NAudio.Wave;
using System.Threading;
using WindowsFormsApp2.Properties;
using System.Text;
using System.Windows.Threading;
using System.CodeDom.Compiler;
using DiscordRPC;
using DiscordRPC.Logging;

namespace WindowsFormsApp2
{
    public partial class Feline : Form
    {
        List<Label> lbls = new List<Label>();
        bool labelc;
        string Currentdir;
        bool CurrentStat;
        private bool Pause;
        private WaveOut waveOutDevice;
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFileReader;

        private delegate void SafeCallDelegate(string text);

        private List<string> songs;
        private bool autoPlay;

        private string songName;

        private System.Windows.Forms.Timer timer1;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;


        //Discord RPC
        public DiscordRpcClient client;
        //client.Initialize();


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        public Feline()
        {
            //Application.Run(new InvokeThreadSafeForm());
            InitializeComponent();
            Initialize();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            client.Dispose();
        }

        void Initialize()
        {
            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            client = new DiscordRpcClient("754478844618211328");

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Console.WriteLine("Received Update! {0}", e.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            client.SetPresence(new RichPresence()
            {
                Details = "Just Vibing",
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Lachee's Discord IPC Library",
                    SmallImageKey = "image_small"
                }

            });
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label7_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label2_MouseHover(object sender, EventArgs e)
        {
            hov(label2);
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
            unhov(label2);
        }

        private void label3_MouseHover(object sender, EventArgs e)
        {
            hov(label3);
        }

        private void label3_MouseLeave(object sender, EventArgs e)
        {
            unhov(label3);
        }

        private void label4_MouseHover(object sender, EventArgs e)
        {
            hov(label4);
        }

        private void label4_MouseLeave(object sender, EventArgs e)
        {
            unhov(label4);
        }

        private void label5_MouseHover(object sender, EventArgs e)
        {
            hov(label5);
        }

        private void label5_MouseLeave(object sender, EventArgs e)
        {
            unhov(label5);

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //Task.Run(() => search()).Wait();
            songImg.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private async Task<int> search(string songNames)
        {
            string querystring = songNames;
            int querypages = 1;

            VideoSearch videos = new VideoSearch();
            //var items = new List<VideoSearchComponents>();
            //await Task.Factory.StartNew(async () => items = await videos.GetVideos(querystring, querypages));
            var items = await videos.GetVideos(querystring, querypages);
            foreach (var item in items)
            {
                Console.WriteLine("Thumbnail: " + item.getThumbnail());
                //var request = WebRequest.Create(string.Format("https://img.youtube.com/vi/{}/0.jpg", item.getUrl().ToString().Remove(0, 32)));
                var request = WebRequest.Create(item.getThumbnail());
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    songImg.Image = Bitmap.FromStream(stream);
                }
                return 0;
            }
            return 0;
        }

        private void label4_Click(object sender, EventArgs e)
        {


            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                Title = "Select Music Folder",
                EnsurePathExists = true
            };
            dialog.InitialDirectory = Settings.Default.lastDir;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                loadSongs(dialog.FileName);
            }
            Currentdir = dialog.FileName;
            string acc_path = dialog.FileName;
            Settings.Default.lastDir = acc_path;
            Settings.Default.Save();
            /*dialog.InitialDirectory = "C:\\Users";
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MessageBox.Show("You selected: " + dialog.FileName);
                var n = Directory.GetFiles(dialog.FileName).Length;
                TextBox[] textBoxes = new TextBox[n];
                Label[] labels = new Label[n];
                var i = 0;
                foreach (var item in Directory.GetFiles(dialog.FileName))
                {
                    
                    //MessageBox.Show(Path.GetExtension(item));
                    if ( Path.GetExtension(item) == ".mp3" || Path.GetExtension(item) == ".m4a"){
                        label8.Text = item.ToString().Remove(0,dialog.FileName.Length);
                        Task.Run(() => search()).Wait();
                    }  
                }
            }*/

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //progressBar1.ForeColor = Color.FromArgb(179, 179, 179);
            //Application.(volumeBar);
            songImg.SizeMode = PictureBoxSizeMode.StretchImage;

            autoPlay = Properties.Settings.Default.autoPlaySet;
            checkBox1.Checked = autoPlay;
            //checkBox1.Checked;
            volumeBar.Value = Settings.Default.Volume;

            timer1 = new System.Windows.Forms.Timer();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = 500; // in miliseconds
            timer1.Start();

            client.Invoke();
        }

        private void hov(Label lab)
        {
            Color Xcol = new Color();
            Xcol = Color.FromArgb(150, 150, 150);
            lab.ForeColor = Xcol;
        }
        private void unhov(Label lab)
        {
            Color Xcol = new Color();
            Xcol = Color.FromArgb(240, 240, 240);
            lab.ForeColor = Xcol;
        }

        private void loadSongs(string songsdir)
        {
            songs = new List<string>();
            foreach (var l in lbls)
            {
                this.Controls.Remove(l);
            }
            List<String> comp = new List<String>();
            comp.Add(".WAV");
            comp.Add(".AIFF");
            comp.Add(".MP3");
            comp.Add(".G.711");
            comp.Add(".ADPCM");
            comp.Add(".WMA");
            comp.Add(".AAC");
            comp.Add(".MP4");
            comp.Add(".M4A");
            foreach (var item in Directory.GetFiles(songsdir))
            {
                //MessageBox.Show(new FileInfo(string.Format(@"{0}", item.ToString().Replace("'", ""))).Extension.ToString().ToUpper()).ToString();
                //string sss;
                //sss = string.Format(@"{0}\",songsdir);
                if (comp.Contains(new FileInfo(string.Format(@"{0}",item.ToString().Replace("'",""))).Extension.ToString().ToUpper())){
                    songs.Add(item.ToString().Remove(0, songsdir.Length + 1));
                } 
                
                
            }
            
            
            for (int i = 0; i < songs.Count; i++) 
            {
                
                Label label = new Label();
                lbls.Add(label);
                label.Location = new System.Drawing.Point(0, 0 + i * 25);
                label.Name = i.ToString();
                label.Text = songs[i].ToString();
                label.Size = new System.Drawing.Size(510, 25);
                label.ForeColor = Color.FromArgb(240, 240, 240);
                label.MouseHover += (se, ev) => { hov(label); };
                label.MouseLeave += (se, ev) => { unhov(label); };
                label.Click += (se, ev) => {
                    bool temp = autoPlay;
                    labelc = true;
                    autoPlay = false;
                    string text = "";
                    string name = "";
                    label.Invoke(new System.Action(() => { text = label.Text; })); // Potential issue
                    label.Invoke(new System.Action(() => {name = label.Name; })); // Potential issue
                        
                    //Task.Run(() => playSong(text, name)).Wait();
                    playSong(text, name);
                    Thread.Sleep(250);
                    Task.Run(() => search(text));
                    changePly();
                    chngSong(text);
                    Thread.Sleep(50);
                    autoPlay = temp; 
                };
                panel1.Controls.Add(label);

            }
        }

        private void playSong(string songName, string labelName)
        {
            if (waveOutDevice != null) { waveOutDevice.Stop(); }
            Pause = false;
            CurrentStat = true;


            //label8.Text = "⏸";
            AudioFileReader audioFileReader = new AudioFileReader(string.Format(@"{0}\{1}", Currentdir, songName));
            waveOutDevice = new WaveOut();

            client.SetPresence(new RichPresence()
            {
                Details = string.Format("Vibing to {0}",(songName.Replace("'","")).Replace(new FileInfo(string.Format("{0}", songName.ToString().Replace("'", ""))).Extension.ToString(),"")),
                Assets = new Assets()
                {
                    LargeImageKey = "image_large",
                    LargeImageText = "Lachee's Discord IPC Library",
                    SmallImageKey = "image_small"
                }

            });

            //currentSong.BeginInvoke(new System.Action(() => { currentSong.Text = songName; }));
            //IAsyncResult res = currentSong.BeginInvoke(new System.Action(() => { currentSong.Text = "cock"; }));
            /*if (currentSong.InvokeRequired)
            {
                currentSong.BeginInvoke(new System.Action(() => currentSong.Text = songName));
            }*/

            //currentSong.Invoke((MethodInvoker)(() => { currentSong.Text = "cock"; }));
            //this.Invoke((MethodInvoker)delegate () { currentSong.Text = songName; });#
            //currentSong.Text = songName;
            /*if (volumeBar.InvokeRequired)
            { // TODO fix freezing when changing volume
                volumeBar.BeginInvoke(new System.Action(() => volumeBar.Scroll += (s, a) => waveOutDevice.Volume = volumeBar.Value / 100f)); 
                volumeBar.BeginInvoke(new System.Action(() => waveOutDevice.Volume = volumeBar.Value / 100f));
            }*/
            //volumeBar.Scroll += (s, a) => waveOutDevice.Volume = volumeBar.Value / 100f;
            //waveOutDevice.Volume = volumeBar.Value / 100f;
            waveOutDevice.Volume = Settings.Default.Volume / 100f;
            waveOutDevice.Init(audioFileReader);
            waveOutDevice.Play();
            int temp;
            temp = Convert.ToInt32(labelName);
            waveOutDevice.PlaybackStopped += (s, a) => { //This occurs multiple times for some reason IDK why
                //MessageBox.Show((CurrentStat && !Pause && autoPlay).ToString());
                if (CurrentStat && Pause == false) 
                {
                    labelName = temp.ToString();
                    //MessageBox.Show(autoPlay.ToString());
                    if (autoPlay && labelc == false)
                    {
                        /*MessageBox.Show("Autoplaying");
                        //MessageBox.Show(index.ToString(), songs.Count.ToString());7*/
                        //MessageBox.Show(songName);
                        var index = Convert.ToInt32(labelName) + 1;
                        if (index > songs.Count)
                        {
                            index = 0;
                        }
                        songName = songs[index];
                        //playSong(songName, index.ToString());
                        songAutoPlay(songName, index.ToString());
                        CurrentStat = false;
                        return;
                    }
                    //timer1.Stop();
                    try
                    {
                        if (!autoPlay)
                        {
                            audioFileReader.Dispose();
                            audioFileReader = null;
                            waveOutDevice.Dispose();
                            waveOutDevice = null;
                            Thread.Sleep(100);
                        }
                        
                        
                    }
                    catch (Exception e) { }


                }
                else if(CurrentStat && Pause == true && autoPlay != true)
                {
                    waveOutDevice.Dispose();
                    audioFileReader.Dispose();
                    timer1.Stop();
                }
                else if(autoPlay == true)
                {
                    CurrentStat = true;
                }
                if(labelc == true && CurrentStat == true)
                {
                    labelc = false;
                }
                
            };

            //MessageBox.Show(string.Format(@"Now Playing : {0}\{1}", Currentdir, songName));
            //System.Media.SoundPlayer player = new System.Media.SoundPlayer(string.Format(@"{0}\{1}", Currentdir, songName));
        }

        private void volumeBar_Enter(object sender, EventArgs e)
        {
            label1.Focus();
        }

        private void label8_MouseHover(object sender, EventArgs e)
        {
            hov(label8);
        }

        private void label8_MouseLeave(object sender, EventArgs e)
        {
            unhov(label8);
        }

        private void label8_Click(object sender, EventArgs e)
        {
            if (CurrentStat) // If it is playing
            {
                CurrentStat = false;
                Pause = true;
                waveOutDevice.Stop();
                Pause = false;
                label8.Text = "▶";
            }
            else
            {
                CurrentStat = true;
                label8.Text = "⏸";
                waveOutDevice.Play();
            }
        }

        /*private void OnPlaybackStopped(object sender, StoppedEventArgs args)
        {
            outputDevice.Dispose();
            outputDevice = null;
            audioFileReader.Dispose();
            audioFileReader = null;
            
        }*/

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            autoPlay = checkBox1.Checked;
            Properties.Settings.Default.autoPlaySet = autoPlay;
            Properties.Settings.Default.Save();
            if (!Properties.Settings.Default.autoPlaySet) {
                NotifyIcon notifyIcon = new NotifyIcon()
                {
                    Visible = true,
                    Icon = SystemIcons.Application,
                    BalloonTipTitle = this.Text,
                };
                if (autoPlay) { notifyIcon.BalloonTipText = "Autoplay been enabled."; }
                else if (!autoPlay) { notifyIcon.BalloonTipText = "Autoplay been disabled."; }
                notifyIcon.ShowBalloonTip(3000);
            }
            
        }
        private void songAutoPlay(string songName, string index)
        {
            playSong(songName, index);
            Thread.Sleep(250);
            Task.Run(() => search(songName));
            changePly();
            chngSong(songName);
        }

        private void volumeBar_Scroll(object sender, EventArgs e)
        {

            Settings.Default.Volume = volumeBar.Value;
            Settings.Default.Save();
            if(waveOutDevice != null)
            {
                waveOutDevice.Volume = Settings.Default.Volume / 100f;
            }
            
        }
        private void changePly()
        {
            label8.Text = "⏸";
        }
        private void chngSong(string songName)
        {
            currentSong.Text = songName;
        }
    }
}


