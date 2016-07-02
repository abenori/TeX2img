namespace TeX2img
{
    partial class PreambleForm
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreambleForm));
            Sgry.Azuki.FontInfo fontInfo1 = new Sgry.Azuki.FontInfo();
            this.preambleTextBox = new Sgry.Azuki.WinForms.AzukiControl();
            this.preambleTextBoxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.Redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.TemplateButon = new System.Windows.Forms.Button();
            this.preambleTextBoxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // preambleTextBox
            // 
            resources.ApplyResources(this.preambleTextBox, "preambleTextBox");
            this.preambleTextBox.BackColor = System.Drawing.Color.White;
            this.preambleTextBox.ContextMenuStrip = this.preambleTextBoxMenu;
            this.preambleTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.preambleTextBox.DrawingOption = ((Sgry.Azuki.DrawingOption)(((((Sgry.Azuki.DrawingOption.DrawsFullWidthSpace | Sgry.Azuki.DrawingOption.DrawsTab) 
            | Sgry.Azuki.DrawingOption.DrawsEol) 
            | Sgry.Azuki.DrawingOption.DrawsEof) 
            | Sgry.Azuki.DrawingOption.HighlightsMatchedBracket)));
            this.preambleTextBox.DrawsEofMark = true;
            this.preambleTextBox.FirstVisibleLine = 0;
            fontInfo1.Name = "ＭＳ ゴシック";
            fontInfo1.Size = 12;
            fontInfo1.Style = System.Drawing.FontStyle.Regular;
            this.preambleTextBox.FontInfo = fontInfo1;
            this.preambleTextBox.ForeColor = System.Drawing.Color.Black;
            this.preambleTextBox.HighlightsCurrentLine = false;
            this.preambleTextBox.Name = "preambleTextBox";
            this.preambleTextBox.ScrollPos = new System.Drawing.Point(0, 0);
            this.preambleTextBox.ScrollsBeyondLastLine = false;
            this.preambleTextBox.ShowsDirtBar = false;
            this.preambleTextBox.ShowsHScrollBar = false;
            this.preambleTextBox.ShowsLineNumber = false;
            this.preambleTextBox.TabWidth = 4;
            this.preambleTextBox.ViewType = Sgry.Azuki.ViewType.WrappedProportional;
            this.preambleTextBox.ViewWidth = 4097;
            // 
            // preambleTextBoxMenu
            // 
            this.preambleTextBoxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Undo,
            this.Redo,
            this.toolStripSeparator1,
            this.Cut,
            this.Copy,
            this.Paste,
            this.Delete,
            this.toolStripSeparator2,
            this.SelectAll});
            this.preambleTextBoxMenu.Name = "sourceTextBoxMenu";
            resources.ApplyResources(this.preambleTextBoxMenu, "preambleTextBoxMenu");
            // 
            // Undo
            // 
            this.Undo.Name = "Undo";
            resources.ApplyResources(this.Undo, "Undo");
            this.Undo.Click += new System.EventHandler(this.Undo_Click);
            // 
            // Redo
            // 
            this.Redo.Name = "Redo";
            resources.ApplyResources(this.Redo, "Redo");
            this.Redo.Click += new System.EventHandler(this.Redo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // Cut
            // 
            this.Cut.Name = "Cut";
            resources.ApplyResources(this.Cut, "Cut");
            this.Cut.Click += new System.EventHandler(this.Cut_Click);
            // 
            // Copy
            // 
            this.Copy.Name = "Copy";
            resources.ApplyResources(this.Copy, "Copy");
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
            // 
            // Paste
            // 
            this.Paste.Name = "Paste";
            resources.ApplyResources(this.Paste, "Paste");
            this.Paste.Click += new System.EventHandler(this.Paste_Click);
            // 
            // Delete
            // 
            this.Delete.Name = "Delete";
            resources.ApplyResources(this.Delete, "Delete");
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // SelectAll
            // 
            this.SelectAll.Name = "SelectAll";
            resources.ApplyResources(this.SelectAll, "SelectAll");
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // TemplateButon
            // 
            resources.ApplyResources(this.TemplateButon, "TemplateButon");
            this.TemplateButon.Name = "TemplateButon";
            this.TemplateButon.UseVisualStyleBackColor = true;
            this.TemplateButon.Click += new System.EventHandler(this.TemplateButon_Click);
            // 
            // PreambleForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TemplateButon);
            this.Controls.Add(this.preambleTextBox);
            this.Name = "PreambleForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PreambleForm_FormClosing);
            this.preambleTextBoxMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sgry.Azuki.WinForms.AzukiControl preambleTextBox;
        private System.Windows.Forms.ContextMenuStrip preambleTextBoxMenu;
        private System.Windows.Forms.ToolStripMenuItem Undo;
        private System.Windows.Forms.ToolStripMenuItem Redo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem Cut;
        private System.Windows.Forms.ToolStripMenuItem Copy;
        private System.Windows.Forms.ToolStripMenuItem Paste;
        private System.Windows.Forms.ToolStripMenuItem Delete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem SelectAll;
        private System.Windows.Forms.Button TemplateButon;
    }
}