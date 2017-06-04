using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Speakify
{
    public partial class frmHome : Form
    {
        private SpotifyLocalAPI _spotifyLocal;
        SpotifyWebAPI _spotifyWeb;
        private SpeechRecognizer _listen;
        private ResultTable _resultTable;
        private Track _currSong;
        private bool _isPlaying;


        public frmHome()
        {
            InitializeComponent();
            SetupLocal();
            SetupWeb();
            //listen();
        }

        private void SetupLocal()
        {
            _spotifyLocal = new SpotifyLocalAPI();

            if (!SpotifyLocalAPI.IsSpotifyRunning())
            {
                try
                {
                    SpotifyLocalAPI.RunSpotify();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.Message);
                }
            }

            if (!SpotifyLocalAPI.IsSpotifyWebHelperRunning())
            {
                try
                {
                    SpotifyLocalAPI.RunSpotifyWebHelper();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.InnerException.Message);
                }
            }

            if (!_spotifyLocal.Connect())
            {
                MessageBox.Show("Failed to connect to spotify");
                return;
            }

            _spotifyLocal.OnPlayStateChange += play_state_changed;

            StatusResponse temp = _spotifyLocal.GetStatus();
            btnPlay.Text = temp.Playing ? "Pause" : "Play";
            _currSong = temp.Track;


        }

        private void play_state_changed(object sender, PlayStateEventArgs args)
        {
            _isPlaying = args.Playing;
            btnPlay.Text = args.Playing ? "Pause" : "Play";
        }

        private async void SetupWeb()
        {
            //Test API Calls
            WebAPIFactory factory = new WebAPIFactory("http://localhost",
                8000,
                "5cd4c69282df4b23979d20b69126a198",
                Scope.Streaming,
                TimeSpan.FromSeconds(20));

            try
            {
                _spotifyWeb = await factory.GetWebApi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ex.Message");
            }
        }

        private void SetupListen()
        {
            _listen = new SpeechRecognizer();

            Choices colors = new Choices();
            colors.Add(new String[] { "red", "green", "blue" });

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(colors);

            Grammar g = new Grammar(gb);

            _listen.LoadGrammar(g);

            _listen.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sr_speechRecognized);

        }

        private void sr_speechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            MessageBox.Show(e.Result.Text);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            _spotifyLocal.Skip();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            _spotifyLocal.Previous();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (!_isPlaying)
            {
                _spotifyLocal.Play();
                _isPlaying = true;
                this.btnPlay.Text = "Pause";
            }
            else
            {
                _spotifyLocal.Pause();
                _isPlaying = false;
                this.btnPlay.Text = "Play";
            }


        }

        private void cbSearch_Click(object sender, EventArgs e)
        {
            cbSearch.Text = String.Empty;
            cbSearch.ForeColor = Color.Black;
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            string search = cbSearch.Text;

            SearchItem results = _spotifyWeb.SearchItems(search, SearchType.All);
            //Very Temporary
            cbSearch.Items.Add(String.Format("\r\nSongs like '{0}'", search));
            cbSearch.Items.Add("--------------------------------------------------");

            for (int i = 0; i < results.Tracks.Items.Count; i++)
            {
                //do the top 3 results
                if(i <= 3)
                    cbSearch.Items.Add(results.Tracks.Items[i].Name);
            }

            cbSearch.Items.Add(String.Format("\r\nArtists like '{0}'", search));
            cbSearch.Items.Add("--------------------------------------------------");

            for (int i = 0; i < results.Artists.Items.Count; i++)
            {
                //do the top 3 results
                if (i <= 3)
                    cbSearch.Items.Add(results.Artists.Items[i].Name);
            }

            cbSearch.Items.Add(String.Format("\r\nAlbums like '{0}'", search));
            cbSearch.Items.Add("--------------------------------------------------");

            for (int i = 0; i < results.Albums.Items.Count; i++)
            {
                //do the top 3 results
                if (i <= 3)
                    cbSearch.Items.Add(results.Albums.Items[i].Name);
            }

            this.Size = new Size(1020, 130);


            _resultTable = new ResultTable(results); 
            this.cSearchResults1.UpdateGrid(_resultTable);
        }

        public void ChangeSong(string uri)
        {
            _spotifyLocal.PlayURL(uri);
        }

    }
}
