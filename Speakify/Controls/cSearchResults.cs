using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Speakify
{
    public partial class cSearchResults : UserControl
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ResultTable _results;
        public cSearchResults()
        {
            InitializeComponent();
        }

        public void UpdateGrid(ResultTable results)
        {
            _results = results;
            gvSearchResults.DataSource = _results;
            gvSearchResults.Refresh();
        }

        private void gvSearchResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataRow row = _results.Rows[e.RowIndex];
            (this.Parent as frmHome).ChangeSong(row["URI"].ToString());
        }
    }
}
