namespace TeX2img
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GenerateEPSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.表示VToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showPreambleWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showOutputWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.色入力ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.オプションOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ヘルプHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sourceTextBox = new Sgry.Azuki.WinForms.AzukiControl();
            this.sourceTextBoxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.Redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.InputFileBrowseButton = new System.Windows.Forms.Button();
            this.inputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.InputFromFileRadioButton = new System.Windows.Forms.RadioButton();
            this.InputFromTextboxRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.OutputBrowseButton = new System.Windows.Forms.Button();
            this.outputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.convertWorker = new System.ComponentModel.BackgroundWorker();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.sourceTextBoxMenu.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.表示VToolStripMenuItem,
            this.SettingToolStripMenuItem,
            this.ヘルプHToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1115, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileToolStripMenuItem
            // 
            this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GenerateEPSToolStripMenuItem,
            this.ImportToolStripMenuItem,
            this.ExportToolStripMenuItem,
            this.ExitToolStripMenuItem});
            this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
            this.FileToolStripMenuItem.Size = new System.Drawing.Size(99, 27);
            this.FileToolStripMenuItem.Text = "ファイル(&F)";
            // 
            // GenerateEPSToolStripMenuItem
            // 
            this.GenerateEPSToolStripMenuItem.Name = "GenerateEPSToolStripMenuItem";
            this.GenerateEPSToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.GenerateEPSToolStripMenuItem.Size = new System.Drawing.Size(296, 28);
            this.GenerateEPSToolStripMenuItem.Text = "画像ファイル生成(&T)";
            this.GenerateEPSToolStripMenuItem.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // ImportToolStripMenuItem
            // 
            this.ImportToolStripMenuItem.Name = "ImportToolStripMenuItem";
            this.ImportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.ImportToolStripMenuItem.Size = new System.Drawing.Size(296, 28);
            this.ImportToolStripMenuItem.Text = "インポート(&O)";
            this.ImportToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // ExportToolStripMenuItem
            // 
            this.ExportToolStripMenuItem.Name = "ExportToolStripMenuItem";
            this.ExportToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.ExportToolStripMenuItem.Size = new System.Drawing.Size(296, 28);
            this.ExportToolStripMenuItem.Text = "エクスポート(&S)";
            this.ExportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(296, 28);
            this.ExitToolStripMenuItem.Text = "終了(&X)";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitCToolStripMenuItem_Click);
            // 
            // 表示VToolStripMenuItem
            // 
            this.表示VToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPreambleWindowToolStripMenuItem,
            this.showOutputWindowToolStripMenuItem,
            this.色入力ToolStripMenuItem});
            this.表示VToolStripMenuItem.Name = "表示VToolStripMenuItem";
            this.表示VToolStripMenuItem.Size = new System.Drawing.Size(86, 27);
            this.表示VToolStripMenuItem.Text = "表示(&V)";
            // 
            // showPreambleWindowToolStripMenuItem
            // 
            this.showPreambleWindowToolStripMenuItem.Name = "showPreambleWindowToolStripMenuItem";
            this.showPreambleWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.P)));
            this.showPreambleWindowToolStripMenuItem.Size = new System.Drawing.Size(406, 28);
            this.showPreambleWindowToolStripMenuItem.Text = "プリアンブル設定ウィンドウ(&P)";
            this.showPreambleWindowToolStripMenuItem.Click += new System.EventHandler(this.showPreambleWindowToolStripMenuItem_Click);
            // 
            // showOutputWindowToolStripMenuItem
            // 
            this.showOutputWindowToolStripMenuItem.Name = "showOutputWindowToolStripMenuItem";
            this.showOutputWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.showOutputWindowToolStripMenuItem.Size = new System.Drawing.Size(406, 28);
            this.showOutputWindowToolStripMenuItem.Text = "出力ウィンドウ(&O)";
            this.showOutputWindowToolStripMenuItem.Click += new System.EventHandler(this.showOutputWindowToolStripMenuItem_Click);
            // 
            // 色入力ToolStripMenuItem
            // 
            this.色入力ToolStripMenuItem.Name = "色入力ToolStripMenuItem";
            this.色入力ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.色入力ToolStripMenuItem.Size = new System.Drawing.Size(406, 28);
            this.色入力ToolStripMenuItem.Text = "色入力補助";
            this.色入力ToolStripMenuItem.Click += new System.EventHandler(this.ColorInputHelperToolStripMenuItem_Click);
            // 
            // SettingToolStripMenuItem
            // 
            this.SettingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.オプションOToolStripMenuItem});
            this.SettingToolStripMenuItem.Name = "SettingToolStripMenuItem";
            this.SettingToolStripMenuItem.Size = new System.Drawing.Size(92, 27);
            this.SettingToolStripMenuItem.Text = "ツール(&T)";
            // 
            // オプションOToolStripMenuItem
            // 
            this.オプションOToolStripMenuItem.Name = "オプションOToolStripMenuItem";
            this.オプションOToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.オプションOToolStripMenuItem.Size = new System.Drawing.Size(259, 28);
            this.オプションOToolStripMenuItem.Text = "オプション...(&O)";
            this.オプションOToolStripMenuItem.Click += new System.EventHandler(this.SettingToolStripMenuItem_Click);
            // 
            // ヘルプHToolStripMenuItem
            // 
            this.ヘルプHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem});
            this.ヘルプHToolStripMenuItem.Name = "ヘルプHToolStripMenuItem";
            this.ヘルプHToolStripMenuItem.Size = new System.Drawing.Size(95, 27);
            this.ヘルプHToolStripMenuItem.Text = "ヘルプ(&H)";
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(229, 28);
            this.AboutToolStripMenuItem.Text = "バージョン情報...(&A)";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.sourceTextBox);
            this.groupBox1.Controls.Add(this.InputFileBrowseButton);
            this.groupBox1.Controls.Add(this.inputFileNameTextBox);
            this.groupBox1.Controls.Add(this.InputFromFileRadioButton);
            this.groupBox1.Controls.Add(this.InputFromTextboxRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(20, 40);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Size = new System.Drawing.Size(1075, 432);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "入力設定";
            // 
            // sourceTextBox
            // 
            this.sourceTextBox.AcceptsTab = false;
            this.sourceTextBox.AllowDrop = true;
            this.sourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.sourceTextBox.ContextMenuStrip = this.sourceTextBoxMenu;
            this.sourceTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.sourceTextBox.DrawingOption = ((Sgry.Azuki.DrawingOption)(((((Sgry.Azuki.DrawingOption.DrawsFullWidthSpace | Sgry.Azuki.DrawingOption.DrawsTab) 
            | Sgry.Azuki.DrawingOption.DrawsEol) 
            | Sgry.Azuki.DrawingOption.DrawsEof) 
            | Sgry.Azuki.DrawingOption.HighlightsMatchedBracket)));
            this.sourceTextBox.DrawsEofMark = true;
            this.sourceTextBox.FirstVisibleLine = 0;
            this.sourceTextBox.Font = new System.Drawing.Font("ＭＳ ゴシック", 12F);
            fontInfo1.Name = "ＭＳ ゴシック";
            fontInfo1.Size = 12;
            fontInfo1.Style = System.Drawing.FontStyle.Regular;
            this.sourceTextBox.FontInfo = fontInfo1;
            this.sourceTextBox.ForeColor = System.Drawing.Color.Black;
            this.sourceTextBox.HighlightsCurrentLine = false;
            this.sourceTextBox.Location = new System.Drawing.Point(26, 57);
            this.sourceTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.sourceTextBox.Name = "sourceTextBox";
            this.sourceTextBox.ScrollPos = new System.Drawing.Point(0, 0);
            this.sourceTextBox.ScrollsBeyondLastLine = false;
            this.sourceTextBox.ShowsDirtBar = false;
            this.sourceTextBox.ShowsHScrollBar = false;
            this.sourceTextBox.ShowsLineNumber = false;
            this.sourceTextBox.Size = new System.Drawing.Size(1047, 293);
            this.sourceTextBox.TabIndex = 3;
            this.sourceTextBox.TabWidth = 4;
            this.sourceTextBox.ViewType = Sgry.Azuki.ViewType.WrappedProportional;
            this.sourceTextBox.ViewWidth = 4097;
            this.sourceTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.sourceTextBox_DragDrop);
            this.sourceTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // sourceTextBoxMenu
            // 
            this.sourceTextBoxMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.sourceTextBoxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Undo,
            this.Redo,
            this.toolStripSeparator1,
            this.Cut,
            this.Copy,
            this.Paste,
            this.Delete,
            this.toolStripSeparator2,
            this.SelectAll});
            this.sourceTextBoxMenu.Name = "sourceTextBoxMenu";
            this.sourceTextBoxMenu.Size = new System.Drawing.Size(177, 212);
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
            // InputFileBrowseButton
            // 
            this.InputFileBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.InputFileBrowseButton.Enabled = false;
            this.InputFileBrowseButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.InputFileBrowseButton.Location = new System.Drawing.Point(945, 392);
            this.InputFileBrowseButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.InputFileBrowseButton.Name = "InputFileBrowseButton";
            this.InputFileBrowseButton.Size = new System.Drawing.Size(112, 28);
            this.InputFileBrowseButton.TabIndex = 6;
            this.InputFileBrowseButton.Text = "参照...";
            this.InputFileBrowseButton.UseVisualStyleBackColor = true;
            this.InputFileBrowseButton.Click += new System.EventHandler(this.InputFileBrowseButton_Click);
            // 
            // inputFileNameTextBox
            // 
            this.inputFileNameTextBox.AllowDrop = true;
            this.inputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputFileNameTextBox.Enabled = false;
            this.inputFileNameTextBox.Location = new System.Drawing.Point(10, 392);
            this.inputFileNameTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.inputFileNameTextBox.Name = "inputFileNameTextBox";
            this.inputFileNameTextBox.Size = new System.Drawing.Size(919, 25);
            this.inputFileNameTextBox.TabIndex = 5;
            this.inputFileNameTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.inputFileNameTextBox_DragDrop);
            this.inputFileNameTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // InputFromFileRadioButton
            // 
            this.InputFromFileRadioButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.InputFromFileRadioButton.AutoSize = true;
            this.InputFromFileRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.InputFromFileRadioButton.Location = new System.Drawing.Point(10, 361);
            this.InputFromFileRadioButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.InputFromFileRadioButton.Name = "InputFromFileRadioButton";
            this.InputFromFileRadioButton.Size = new System.Drawing.Size(244, 22);
            this.InputFromFileRadioButton.TabIndex = 4;
            this.InputFromFileRadioButton.Text = "TeX ソースファイルを読み込む";
            this.InputFromFileRadioButton.UseVisualStyleBackColor = true;
            this.InputFromFileRadioButton.Click += new System.EventHandler(this.setEnabled);
            // 
            // InputFromTextboxRadioButton
            // 
            this.InputFromTextboxRadioButton.AutoSize = true;
            this.InputFromTextboxRadioButton.Checked = global::TeX2img.Properties.Settings.Default.inputFromTextBox;
            this.InputFromTextboxRadioButton.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::TeX2img.Properties.Settings.Default, "inputFromTextBox", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.InputFromTextboxRadioButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.InputFromTextboxRadioButton.Location = new System.Drawing.Point(10, 27);
            this.InputFromTextboxRadioButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.InputFromTextboxRadioButton.Name = "InputFromTextboxRadioButton";
            this.InputFromTextboxRadioButton.Size = new System.Drawing.Size(553, 22);
            this.InputFromTextboxRadioButton.TabIndex = 1;
            this.InputFromTextboxRadioButton.TabStop = true;
            this.InputFromTextboxRadioButton.Text = "TeX コードを直接入力（ \\begin{document} ～ \\end{document} の内部 ）";
            this.InputFromTextboxRadioButton.UseVisualStyleBackColor = true;
            this.InputFromTextboxRadioButton.CheckedChanged += new System.EventHandler(this.setEnabled);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.OutputBrowseButton);
            this.groupBox2.Controls.Add(this.outputFileNameTextBox);
            this.groupBox2.Location = new System.Drawing.Point(20, 482);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox2.Size = new System.Drawing.Size(1075, 69);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "出力先ファイル（拡張子 .eps / .png / .jpg / .pdf / .svg / .emf /.bmp ）";
            // 
            // OutputBrowseButton
            // 
            this.OutputBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputBrowseButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OutputBrowseButton.Location = new System.Drawing.Point(945, 27);
            this.OutputBrowseButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.OutputBrowseButton.Name = "OutputBrowseButton";
            this.OutputBrowseButton.Size = new System.Drawing.Size(112, 28);
            this.OutputBrowseButton.TabIndex = 1;
            this.OutputBrowseButton.Text = "参照...";
            this.OutputBrowseButton.UseVisualStyleBackColor = true;
            this.OutputBrowseButton.Click += new System.EventHandler(this.OutputBrowseButton_Click);
            // 
            // outputFileNameTextBox
            // 
            this.outputFileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileNameTextBox.Location = new System.Drawing.Point(10, 27);
            this.outputFileNameTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.outputFileNameTextBox.Name = "outputFileNameTextBox";
            this.outputFileNameTextBox.Size = new System.Drawing.Size(919, 25);
            this.outputFileNameTextBox.TabIndex = 0;
            // 
            // GenerateButton
            // 
            this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GenerateButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GenerateButton.Location = new System.Drawing.Point(881, 564);
            this.GenerateButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.Size = new System.Drawing.Size(212, 45);
            this.GenerateButton.TabIndex = 3;
            this.GenerateButton.Text = "画像ファイル生成";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "eps";
            this.saveFileDialog1.FileName = "equation.eps";
            this.saveFileDialog1.Filter = "EPSファイル (*.eps)|*.eps|JPEGファイル (*.jpg)|*.jpg|PNGファイル (*.png)|*.png|PDFファイル (*.pdf" +
    ")|*.pdf|EMFファイル (*.emf)|*.emf|SVGファイル (*.svg)|*.svg|BMPファイル (*.bmp)|*.bmp|すべてのファ" +
    "イル (*.*)|*.*";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "tex";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "TeXソースファイル (*.tex)|*.tex|すべてのファイル (*.*)|*.*";
            // 
            // convertWorker
            // 
            this.convertWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.convertWorker_DoWork);
            this.convertWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.convertWorker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1115, 622);
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TeX2img";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.sourceTextBoxMenu.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SettingToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button GenerateButton;
		private System.Windows.Forms.TextBox outputFileNameTextBox;
        private System.Windows.Forms.RadioButton InputFromFileRadioButton;
        private System.Windows.Forms.RadioButton InputFromTextboxRadioButton;
        private System.Windows.Forms.Button OutputBrowseButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TextBox inputFileNameTextBox;
        private System.Windows.Forms.Button InputFileBrowseButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem GenerateEPSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 表示VToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showPreambleWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showOutputWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ヘルプHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem オプションOToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker convertWorker;
        private Sgry.Azuki.WinForms.AzukiControl sourceTextBox;
        private System.Windows.Forms.ContextMenuStrip sourceTextBoxMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem SelectAll;
        private System.Windows.Forms.ToolStripMenuItem Delete;
        private System.Windows.Forms.ToolStripMenuItem Undo;
        private System.Windows.Forms.ToolStripMenuItem Cut;
        private System.Windows.Forms.ToolStripMenuItem Copy;
        private System.Windows.Forms.ToolStripMenuItem Paste;
        private System.Windows.Forms.ToolStripMenuItem Redo;
        private System.Windows.Forms.ToolStripMenuItem ImportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 色入力ToolStripMenuItem;
    }
}

