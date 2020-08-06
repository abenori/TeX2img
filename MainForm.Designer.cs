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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Sgry.Azuki.FontInfo fontInfo1 = new Sgry.Azuki.FontInfo();
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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ExtensioncomboBox = new System.Windows.Forms.ComboBox();
            this.OutputBrowseButton = new System.Windows.Forms.Button();
            this.outputFileNameTextBox = new System.Windows.Forms.TextBox();
            this.GenerateButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.convertWorker = new System.ComponentModel.BackgroundWorker();
            this.TeXsourceSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.InputFromTextboxRadioButton = new System.Windows.Forms.RadioButton();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.sourceTextBoxMenu.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.表示VToolStripMenuItem,
            this.SettingToolStripMenuItem,
            this.ヘルプHToolStripMenuItem});
            this.menuStrip1.Name = "menuStrip1";
            // 
            // FileToolStripMenuItem
            // 
            resources.ApplyResources(this.FileToolStripMenuItem, "FileToolStripMenuItem");
            this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GenerateEPSToolStripMenuItem,
            this.ImportToolStripMenuItem,
            this.ExportToolStripMenuItem,
            this.ExitToolStripMenuItem});
            this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
            // 
            // GenerateEPSToolStripMenuItem
            // 
            resources.ApplyResources(this.GenerateEPSToolStripMenuItem, "GenerateEPSToolStripMenuItem");
            this.GenerateEPSToolStripMenuItem.Name = "GenerateEPSToolStripMenuItem";
            this.GenerateEPSToolStripMenuItem.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // ImportToolStripMenuItem
            // 
            resources.ApplyResources(this.ImportToolStripMenuItem, "ImportToolStripMenuItem");
            this.ImportToolStripMenuItem.Name = "ImportToolStripMenuItem";
            this.ImportToolStripMenuItem.Click += new System.EventHandler(this.ImportToolStripMenuItem_Click);
            // 
            // ExportToolStripMenuItem
            // 
            resources.ApplyResources(this.ExportToolStripMenuItem, "ExportToolStripMenuItem");
            this.ExportToolStripMenuItem.Name = "ExportToolStripMenuItem";
            this.ExportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // ExitToolStripMenuItem
            // 
            resources.ApplyResources(this.ExitToolStripMenuItem, "ExitToolStripMenuItem");
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitCToolStripMenuItem_Click);
            // 
            // 表示VToolStripMenuItem
            // 
            resources.ApplyResources(this.表示VToolStripMenuItem, "表示VToolStripMenuItem");
            this.表示VToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showPreambleWindowToolStripMenuItem,
            this.showOutputWindowToolStripMenuItem,
            this.色入力ToolStripMenuItem});
            this.表示VToolStripMenuItem.Name = "表示VToolStripMenuItem";
            // 
            // showPreambleWindowToolStripMenuItem
            // 
            resources.ApplyResources(this.showPreambleWindowToolStripMenuItem, "showPreambleWindowToolStripMenuItem");
            this.showPreambleWindowToolStripMenuItem.Name = "showPreambleWindowToolStripMenuItem";
            this.showPreambleWindowToolStripMenuItem.Click += new System.EventHandler(this.showPreambleWindowToolStripMenuItem_Click);
            // 
            // showOutputWindowToolStripMenuItem
            // 
            resources.ApplyResources(this.showOutputWindowToolStripMenuItem, "showOutputWindowToolStripMenuItem");
            this.showOutputWindowToolStripMenuItem.Name = "showOutputWindowToolStripMenuItem";
            this.showOutputWindowToolStripMenuItem.Click += new System.EventHandler(this.showOutputWindowToolStripMenuItem_Click);
            // 
            // 色入力ToolStripMenuItem
            // 
            resources.ApplyResources(this.色入力ToolStripMenuItem, "色入力ToolStripMenuItem");
            this.色入力ToolStripMenuItem.Name = "色入力ToolStripMenuItem";
            this.色入力ToolStripMenuItem.Click += new System.EventHandler(this.ColorInputHelperToolStripMenuItem_Click);
            // 
            // SettingToolStripMenuItem
            // 
            resources.ApplyResources(this.SettingToolStripMenuItem, "SettingToolStripMenuItem");
            this.SettingToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.オプションOToolStripMenuItem});
            this.SettingToolStripMenuItem.Name = "SettingToolStripMenuItem";
            // 
            // オプションOToolStripMenuItem
            // 
            resources.ApplyResources(this.オプションOToolStripMenuItem, "オプションOToolStripMenuItem");
            this.オプションOToolStripMenuItem.Name = "オプションOToolStripMenuItem";
            this.オプションOToolStripMenuItem.Click += new System.EventHandler(this.SettingToolStripMenuItem_Click);
            // 
            // ヘルプHToolStripMenuItem
            // 
            resources.ApplyResources(this.ヘルプHToolStripMenuItem, "ヘルプHToolStripMenuItem");
            this.ヘルプHToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem});
            this.ヘルプHToolStripMenuItem.Name = "ヘルプHToolStripMenuItem";
            // 
            // AboutToolStripMenuItem
            // 
            resources.ApplyResources(this.AboutToolStripMenuItem, "AboutToolStripMenuItem");
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Controls.Add(this.sourceTextBox);
            this.groupBox1.Controls.Add(this.InputFileBrowseButton);
            this.groupBox1.Controls.Add(this.inputFileNameTextBox);
            this.groupBox1.Controls.Add(this.InputFromTextboxRadioButton);
            this.groupBox1.Controls.Add(this.InputFromFileRadioButton);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // sourceTextBox
            // 
            this.sourceTextBox.AcceptsTab = false;
            resources.ApplyResources(this.sourceTextBox, "sourceTextBox");
            this.sourceTextBox.AllowDrop = true;
            this.sourceTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.sourceTextBox.ContextMenuStrip = this.sourceTextBoxMenu;
            this.sourceTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.sourceTextBox.DrawingOption = ((Sgry.Azuki.DrawingOption)(((((Sgry.Azuki.DrawingOption.DrawsFullWidthSpace | Sgry.Azuki.DrawingOption.DrawsTab) 
            | Sgry.Azuki.DrawingOption.DrawsEol) 
            | Sgry.Azuki.DrawingOption.DrawsEof) 
            | Sgry.Azuki.DrawingOption.HighlightsMatchedBracket)));
            this.sourceTextBox.DrawsEofMark = true;
            this.sourceTextBox.FirstVisibleLine = 0;
            fontInfo1.Name = "ＭＳ ゴシック";
            fontInfo1.Size = 12;
            fontInfo1.Style = System.Drawing.FontStyle.Regular;
            this.sourceTextBox.FontInfo = fontInfo1;
            this.sourceTextBox.ForeColor = System.Drawing.Color.Black;
            this.sourceTextBox.HighlightsCurrentLine = false;
            this.sourceTextBox.Name = "sourceTextBox";
            this.sourceTextBox.ScrollPos = new System.Drawing.Point(0, 0);
            this.sourceTextBox.ScrollsBeyondLastLine = false;
            this.sourceTextBox.ShowsDirtBar = false;
            this.sourceTextBox.ShowsHScrollBar = false;
            this.sourceTextBox.ShowsLineNumber = false;
            this.sourceTextBox.TabWidth = 4;
            this.sourceTextBox.ViewType = Sgry.Azuki.ViewType.WrappedProportional;
            this.sourceTextBox.ViewWidth = 4097;
            this.sourceTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.sourceTextBox_DragDrop);
            this.sourceTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // sourceTextBoxMenu
            // 
            resources.ApplyResources(this.sourceTextBoxMenu, "sourceTextBoxMenu");
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
            // 
            // Undo
            // 
            resources.ApplyResources(this.Undo, "Undo");
            this.Undo.Name = "Undo";
            this.Undo.Click += new System.EventHandler(this.Undo_Click);
            // 
            // Redo
            // 
            resources.ApplyResources(this.Redo, "Redo");
            this.Redo.Name = "Redo";
            this.Redo.Click += new System.EventHandler(this.Redo_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // Cut
            // 
            resources.ApplyResources(this.Cut, "Cut");
            this.Cut.Name = "Cut";
            this.Cut.Click += new System.EventHandler(this.Cut_Click);
            // 
            // Copy
            // 
            resources.ApplyResources(this.Copy, "Copy");
            this.Copy.Name = "Copy";
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
            // 
            // Paste
            // 
            resources.ApplyResources(this.Paste, "Paste");
            this.Paste.Name = "Paste";
            this.Paste.Click += new System.EventHandler(this.Paste_Click);
            // 
            // Delete
            // 
            resources.ApplyResources(this.Delete, "Delete");
            this.Delete.Name = "Delete";
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // toolStripSeparator2
            // 
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // SelectAll
            // 
            resources.ApplyResources(this.SelectAll, "SelectAll");
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // InputFileBrowseButton
            // 
            resources.ApplyResources(this.InputFileBrowseButton, "InputFileBrowseButton");
            this.InputFileBrowseButton.Name = "InputFileBrowseButton";
            this.InputFileBrowseButton.UseVisualStyleBackColor = true;
            this.InputFileBrowseButton.Click += new System.EventHandler(this.InputFileBrowseButton_Click);
            // 
            // inputFileNameTextBox
            // 
            resources.ApplyResources(this.inputFileNameTextBox, "inputFileNameTextBox");
            this.inputFileNameTextBox.AllowDrop = true;
            this.inputFileNameTextBox.Name = "inputFileNameTextBox";
            this.inputFileNameTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.inputFileNameTextBox_DragDrop);
            this.inputFileNameTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // InputFromFileRadioButton
            // 
            resources.ApplyResources(this.InputFromFileRadioButton, "InputFromFileRadioButton");
            this.InputFromFileRadioButton.Name = "InputFromFileRadioButton";
            this.InputFromFileRadioButton.UseVisualStyleBackColor = true;
            this.InputFromFileRadioButton.Click += new System.EventHandler(this.setEnabled);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.ExtensioncomboBox);
            this.groupBox2.Controls.Add(this.OutputBrowseButton);
            this.groupBox2.Controls.Add(this.outputFileNameTextBox);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // ExtensioncomboBox
            // 
            resources.ApplyResources(this.ExtensioncomboBox, "ExtensioncomboBox");
            this.ExtensioncomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ExtensioncomboBox.FormattingEnabled = true;
            this.ExtensioncomboBox.Items.AddRange(new object[] {
            resources.GetString("ExtensioncomboBox.Items"),
            resources.GetString("ExtensioncomboBox.Items1"),
            resources.GetString("ExtensioncomboBox.Items2"),
            resources.GetString("ExtensioncomboBox.Items3"),
            resources.GetString("ExtensioncomboBox.Items4"),
            resources.GetString("ExtensioncomboBox.Items5"),
            resources.GetString("ExtensioncomboBox.Items6"),
            resources.GetString("ExtensioncomboBox.Items7")});
            this.ExtensioncomboBox.Name = "ExtensioncomboBox";
            this.ExtensioncomboBox.SelectionChangeCommitted += new System.EventHandler(this.ExtensioncomboBox_SelectionChangeCommitted);
            // 
            // OutputBrowseButton
            // 
            resources.ApplyResources(this.OutputBrowseButton, "OutputBrowseButton");
            this.OutputBrowseButton.Name = "OutputBrowseButton";
            this.OutputBrowseButton.UseVisualStyleBackColor = true;
            this.OutputBrowseButton.Click += new System.EventHandler(this.OutputBrowseButton_Click);
            // 
            // outputFileNameTextBox
            // 
            resources.ApplyResources(this.outputFileNameTextBox, "outputFileNameTextBox");
            this.outputFileNameTextBox.Name = "outputFileNameTextBox";
            this.outputFileNameTextBox.TextChanged += new System.EventHandler(this.outputFileNameTextBox_TextChanged);
            // 
            // GenerateButton
            // 
            resources.ApplyResources(this.GenerateButton, "GenerateButton");
            this.GenerateButton.Name = "GenerateButton";
            this.GenerateButton.UseVisualStyleBackColor = true;
            this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "eps";
            this.saveFileDialog1.FileName = "equation.eps";
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "tex";
            this.openFileDialog1.FileName = "openFileDialog1";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // convertWorker
            // 
            this.convertWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.convertWorker_DoWork);
            this.convertWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.convertWorker_RunWorkerCompleted);
            // 
            // TeXsourceSaveFileDialog
            // 
            resources.ApplyResources(this.TeXsourceSaveFileDialog, "TeXsourceSaveFileDialog");
            // 
            // InputFromTextboxRadioButton
            // 
            resources.ApplyResources(this.InputFromTextboxRadioButton, "InputFromTextboxRadioButton");
            this.InputFromTextboxRadioButton.Checked = global::TeX2img.Properties.Settings.Default.inputFromTextBox;
            this.InputFromTextboxRadioButton.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::TeX2img.Properties.Settings.Default, "inputFromTextBox", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.InputFromTextboxRadioButton.Name = "InputFromTextboxRadioButton";
            this.InputFromTextboxRadioButton.TabStop = true;
            this.InputFromTextboxRadioButton.UseVisualStyleBackColor = true;
            this.InputFromTextboxRadioButton.CheckedChanged += new System.EventHandler(this.setEnabled);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GenerateButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
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
        private System.Windows.Forms.SaveFileDialog TeXsourceSaveFileDialog;
        private System.Windows.Forms.ComboBox ExtensioncomboBox;
    }
}

