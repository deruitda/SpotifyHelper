using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Local.Models;
using SpotifyAPI.Web.Models;

namespace Speakify
{
    public class ResultTable : DataTable
    {

        public ResultTable()
        {
        }

        public ResultTable(SearchItem results)
        {
            this.Columns.Add("Track");
            this.Columns.Add("Artist");
            this.Columns.Add("Album");
            this.Columns.Add("Duration");
            this.Columns.Add("URI");

            foreach (FullTrack track in results.Tracks.Items)
            {
                DataRow row = this.NewRow();
                row["Track"] = track.Name;
                row["Artist"] = track.Artists.FirstOrDefault().Name;
                row["Album"] = track.Album.Name;
                row["Duration"] = track.DurationMs;
                row["URI"] = track.Uri;

                this.Rows.Add(row);
            }
        }
    }
}
