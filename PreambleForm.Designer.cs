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
            Sgry.Azuki.FontInfo fontInfo1 = new Sgry.Azuki.FontInfo();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreambleForm));
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
            this.preambleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.preambleTextBox.BackColor = System.Drawing.Color.White;
            this.preambleTextBox.ContextMenuStrip = this.preambleTextBoxMenu;
            this.preambleTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.preambleTextBox.DrawingOption = ((Sgry.Azuki.DrawingOption)(((((Sgry.Azuki.DrawingOption.DrawsFullWidthSpace | Sgry.Azuki.DrawingOption.DrawsTab) 
            | Sgry.Azuki.DrawingOption.DrawsEol) 
            | Sgry.Azuki.DrawingOption.DrawsEof) 
            | Sgry.Azuki.DrawingOption.HighlightsMatchedBracket)));
            this.preambleTextBox.DrawsEofMark = true;
            this.preambleTextBox.FirstVisibleLine = 0;
            this.preambleTextBox.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F);
            fontInfo1.Name = "ＭＳ ゴシック";
            fontInfo1.Size = 12;
            fontInfo1.Style = System.Drawing.FontStyle.Regular;
            this.preambleTextBox.FontInfo = fontInfo1;
            this.preambleTextBox.ForeColor = System.Drawing.Color.Black;
            this.preambleTextBox.HighlightsCurrentLine = false;
            this.preambleTextBox.Location = new System.Drawing.Point(14, 13);
            this.preambleTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.preambleTextBox.Name = "preambleTextBox";
            this.preambleTextBox.ScrollPos = new System.Drawing.Point(0, 0);
            this.preambleTextBox.ScrollsBeyondLastLine = false;
            this.preambleTextBox.ShowsDirtBar = false;
            this.preambleTextBox.ShowsHScrollBar = false;
            this.preambleTextBox.ShowsLineNumber = false;
            this.preambleTextBox.Size = new System.Drawing.Size(592, 456);
            this.preambleTextBox.TabIndex = 4;
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
            this.preambleTextBoxMenu.Size = new System.Drawing.Size(177, 212);
            // 
            // Undo
            // 
            this.Undo.Name = "Undo";
            this.Undo.Size = new System.Drawing.Size(176, 28);
            this.Undo.Text = "元に戻す(&U)";
            this.Undo.Click += new System.EventHandler(this.Undo_Click);
            // 
            // Redo
            // 
            this.Redo.Name = "Redo";
            this.Redo.Size = new System.Drawing.Size(176, 28);
            this.Redo.Text = "やり直し(&Y)";
            this.Redo.Click += new System.EventHandler(this.Redo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(173, 6);
            // 
            // Cut
            // 
            this.Cut.Name = "Cut";
            this.Cut.Size = new System.Drawing.Size(176, 28);
            this.Cut.Text = "切り取り(&T)";
            this.Cut.Click += new System.EventHandler(this.Cut_Click);
            // 
            // Copy
            // 
            this.Copy.Name = "Copy";
            this.Copy.Size = new System.Drawing.Size(176, 28);
            this.Copy.Text = "コピー(&C)";
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
            // 
            // Paste
            // 
            this.Paste.Name = "Paste";
            this.Paste.Size = new System.Drawing.Size(176, 28);
            this.Paste.Text = "貼り付け(&P)";
            this.Paste.Click += new System.EventHandler(this.Paste_Click);
            // 
            // Delete
            // 
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(176, 28);
            this.Delete.Text = "削除(&D)";
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // SelectAll
            // 
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Size = new System.Drawing.Size(176, 28);
            this.SelectAll.Text = "全て選択(&A)";
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // TemplateButon
            // 
            this.TemplateButon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.TemplateButon.Location = new System.Drawing.Point(428, 476);
            this.TemplateButon.Name = "TemplateButon";
            this.TemplateButon.Size = new System.Drawing.Size(180, 40);
            this.TemplateButon.TabIndex = 5;
            this.TemplateButon.Text = "テンプレート ▼";
            this.TemplateButon.UseVisualStyleBackColor = true;
            this.TemplateButon.Click += new System.EventHandler(this.TemplateButon_Click);
            // 
            // PreambleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 528);
            this.Controls.Add(this.TemplateButon);
            this.Controls.Add(this.preambleTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "PreambleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "プリアンブルの設定";
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