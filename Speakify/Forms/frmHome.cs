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
using Speakify.Libraries;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Models;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

//TODO: Need to move 90% of this code out of the home form, should be done in some utility class

namespace Speakify
{
    public partial class frmHome : Form
    {
        private SpotifyLocalAPI _spotifyLocal;
        SpotifyWebAPI _spotifyWeb;
        private SpeechRecognitionEngine _listen;
        private SpeechRecognitionEngine _playlistListen;
        private ResultTable _resultTable;
        private Track _currSong;
        private bool _isPlaying;
        private Paging<SimplePlaylist> _playlists;


        public frmHome()
        {
            InitializeComponent();
            SetupLocal();
            SetupWeb();
            SetupListen();
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
            InitMainListener();
            InitPlaylistListener();
        }

        private void mainListen_speechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            _listen.RecognizeAsyncStop();
            //listen for a playlist name for 5 seconds
            _playlistListen.Recognize(new TimeSpan(0, 0, 10));

            InitMainListener();
        }

        private void playlistListen_speechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SimplePlaylist playList = new SimplePlaylist();
            
            //Find playlist in list of user playlists
            //TODO: replace with LINQ
            foreach (SimplePlaylist p in _playlists.Items)
            {
                if (p.Name.Trim().Equals(e.Result.Text))
                {
                    playList = p;
                    break;
                }
            }
                
            if (!String.IsNullOrEmpty(playList.Name))
                _spotifyLocal.PlayURL(playList.Uri);
            else
                MessageBox.Show(String.Format("Could not find playlist: {0}", e.Result.Text));

            InitPlaylistListener();
            
        }

        private void InitMainListener()
        {
            _listen = new SpeechRecognitionEngine();

            //give the listener the keyword to wait for
            Choices baseChoices = GetBaseChoices();

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(baseChoices);

            Grammar g = new Grammar(gb);

            _listen.LoadGrammar(g);
            _listen.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(mainListen_speechRecognized);
            _listen.SetInputToDefaultAudioDevice();
            //RecognizeMode.Multiple will allow the listener to continue listening after completion
            _listen.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void InitPlaylistListener()
        {

            _playlistListen = new SpeechRecognitionEngine();

            //give the listener the keyword to wait for
            //TODO: Remove hardcoded username
            Choices playlistChoices = GetUserPlaylists("deruitda");

            //add each of the user's playlists to the 
            _playlists.Items.ForEach(playlist => playlistChoices.Add(new String[] { playlist.Name }));

            GrammarBuilder gb = new GrammarBuilder();
            gb.Append(playlistChoices);

            Grammar g = new Grammar(gb);

            _playlistListen.LoadGrammar(g);
            _playlistListen.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(playlistListen_speechRecognized);
            _playlistListen.SetInputToDefaultAudioDevice();
        }

        private Choices GetBaseChoices()
        {
            Choices baseChoices = new Choices();
            baseChoices.Add(new String[] { SpokenCommands.BaseCommands.Play, SpokenCommands.BaseCommands.Resume, SpokenCommands.BaseCommands.Pause, SpokenCommands.BaseCommands.Skip, SpokenCommands.BaseCommands.GoBack });

            return baseChoices;
        }

        private Choices GetUserPlaylists(string userID)
        {
            Choices playlistsChoice = new Choices();

            _playlists = _spotifyWeb.GetUserPlaylists(userID);

            foreach (SimplePlaylist playlist in _playlists.Items)
                playlistsChoice.Add(new String[] { playlist.Name.Trim() });

            return playlistsChoice;
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
                if (i <= 3)
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
