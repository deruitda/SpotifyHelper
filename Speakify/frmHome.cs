using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Recognition;
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
        private bool _isPlaying;


        public frmHome()
        {
            SetupLocal();
            SetupWeb();
            InitializeComponent();
            //listen();
        }

        private void SetupLocal()
        {
            _spotifyLocal = new SpotifyLocalAPI();

            //can't set to false immediately, what if spotify is on and they are playing a song already?
            _isPlaying = false;

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
                btnPlay.Text = "Pause";
            }
            else
            {
                _spotifyLocal.Pause();
                _isPlaying = false;
                btnPlay.Text = "Play";
            }


        }

        private void txtSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = String.Empty;
            txtSearch.ForeColor = Color.Black;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text;

            SearchItem results = _spotifyWeb.SearchItems(search, SearchType.All);

            FullTrack track = results.Tracks.Items.FirstOrDefault();

            ErrorResponse testError = _spotifyWeb.ResumePlayback(uris: new List<string> {track.Uri});
        }

    }
}
