namespace Speakify
{
    partial class cSearchResults
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gvSearchResults = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.gvSearchResults)).BeginInit();
            this.SuspendLayout();
            // 
            // gvSearchResults
            // 
            this.gvSearchResults.AllowUserToAddRows = false;
            this.gvSearchResults.AllowUserToDeleteRows = false;
            this.gvSearchResults.BackgroundColor = System.Drawing.Color.Black;
            this.gvSearchResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gvSearchResults.Location = new System.Drawing.Point(3, 3);
            this.gvSearchResults.Name = "gvSearchResults";
            this.gvSearchResults.Size = new System.Drawing.Size(647, 184);
            this.gvSearchResults.TabIndex = 0;
            this.gvSearchResults.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gvSearchResults_CellDoubleClick);
            // 
            // cSearchResults
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Controls.Add(this.gvSearchResults);
            this.Name = "cSearchResults";
            this.Size = new System.Drawing.Size(653, 190);
            ((System.ComponentModel.ISupportInitialize)(this.gvSearchResults)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gvSearchResults;
    }
}
