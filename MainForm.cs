using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace TeX2img {
    public partial class MainForm : Form, IOutputController {
        private OutputForm myOutputForm;
        private PreambleForm myPreambleForm;
        private string thisGenerateButtonText;

        #region コンストラクタおよび初期化処理関連のメソッド
        List<string> FirstFiles = null;

        public MainForm(List<string> files) {
            if(files.Count != 0) FirstFiles = files;

            InitializeComponent();
            thisGenerateButtonText = this.GenerateButton.Text;

            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            myPreambleForm = new PreambleForm(this);
            myOutputForm = new OutputForm(this);

            sourceTextBox.Highlighter = Sgry.Azuki.Highlighter.Highlighters.Latex;
            sourceTextBox.Resize += delegate { sourceTextBox.ViewWidth = sourceTextBox.ClientSize.Width; };
            sourceTextBox.ShowsHScrollBar = false;
            sourceTextBox.Document.WordProc.EnableWordWrap = false;
            sourceTextBox.Document.EolCode = System.Environment.NewLine;
            Sgry.Azuki.UserPref.Antialias = Sgry.Azuki.Antialias.Default;
            loadSettings();

            if(InputFromTextboxRadioButton.Checked) ActiveControl = sourceTextBox;
            else ActiveControl = inputFileNameTextBox;
        }


        protected override void OnShown(EventArgs e) {
            if(FirstFiles != null) {
                clearOutputTextBox();
                if(Properties.Settings.Default.showOutputWindowFlag) showOutputWindow(true);
                GenerateButton.Text = Properties.Resources.STOP;
                setEnabled(false);
                convertWorker.RunWorkerAsync(100);
            }
            base.OnShown(e);
        }

        #endregion

        #region 設定値の読み書き
        private void loadSettings() {
            //sourceTextBox.Text = "\\qquad あ";

            const int minLength = 50;
            if(Properties.Settings.Default.Height > minLength) this.Height = Properties.Settings.Default.Height;
            if(Properties.Settings.Default.Width > minLength) this.Width = Properties.Settings.Default.Width;
            if(Properties.Settings.Default.Left > 0) this.Left = Properties.Settings.Default.Left;
            if(Properties.Settings.Default.Top > 0) this.Top = Properties.Settings.Default.Top;
            if(Properties.Settings.Default.preambleWindowHeight > minLength) myPreambleForm.Height = Properties.Settings.Default.preambleWindowHeight;
            if(Properties.Settings.Default.preambleWindowWidth > minLength) myPreambleForm.Width = Properties.Settings.Default.preambleWindowWidth;
            if(Properties.Settings.Default.outputWindowHeight > minLength) myOutputForm.Height = Properties.Settings.Default.outputWindowHeight;
            if(Properties.Settings.Default.outputWindowWidth > minLength) myOutputForm.Width = Properties.Settings.Default.outputWindowWidth;

            if(Properties.Settings.Default.outputFile != "") {
                outputFileNameTextBox.Text = Properties.Settings.Default.outputFile;
            } else {
                outputFileNameTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\equation.eps";
            }
            string ext = Path.GetExtension(outputFileNameTextBox.Text).ToLower();
            ExtensioncomboBox.SelectedIndex = 0;
            for(int i = 0; i < ExtensioncomboBox.Items.Count; ++i) {
                if(ext == "." + ExtensioncomboBox.Items[i].ToString()) ExtensioncomboBox.SelectedIndex = i;
            }
            inputFileNameTextBox.Text = Properties.Settings.Default.inputFile;
            InputFromTextboxRadioButton.Checked = Properties.Settings.Default.inputFromTextBox;
            InputFromFileRadioButton.Checked = !InputFromTextboxRadioButton.Checked;

            Sgry.Azuki.WinForms.AzukiControl preambleTextBox = myPreambleForm.PreambleTextBox;
            preambleTextBox.Text = Properties.Settings.Default.preamble;
            // プリアンブルフォームのキャレットを最後に持っていく
            preambleTextBox.SetSelection(preambleTextBox.TextLength, preambleTextBox.TextLength);
            preambleTextBox.Focus();
            preambleTextBox.ScrollToCaret();

            ChangeSetting();
            setEnabled();
        }

        private void saveSettings() {
            if(this.WindowState != FormWindowState.Minimized) {
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Top = this.Top;
            }
            if(myPreambleForm.WindowState != FormWindowState.Minimized) {
                Properties.Settings.Default.preambleWindowHeight = myPreambleForm.Height;
                Properties.Settings.Default.preambleWindowWidth = myPreambleForm.Width;
            }
            if(myOutputForm.WindowState != FormWindowState.Minimized) {
                Properties.Settings.Default.outputWindowHeight = myOutputForm.Height;
                Properties.Settings.Default.outputWindowWidth = myOutputForm.Width;
            }


            Properties.Settings.Default.outputFile = outputFileNameTextBox.Text;
            Properties.Settings.Default.inputFile = inputFileNameTextBox.Text;
            Properties.Settings.Default.inputFromTextBox = InputFromTextboxRadioButton.Checked;
            Properties.Settings.Default.preamble = myPreambleForm.PreambleTextBox.Text;
        }
        #endregion

        #region 参照ボタンクリックのイベントハンドラ
        private void OutputBrowseButton_Click(object sender, EventArgs e) {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK) {
                outputFileNameTextBox.Text = saveFileDialog1.FileName;
            }
        }

        private void InputFileBrowseButton_Click(object sender, EventArgs e) {
            if(openFileDialog1.ShowDialog() == DialogResult.OK) {
                inputFileNameTextBox.Text = openFileDialog1.FileName;
            }
        }
        #endregion

        #region メニュークリックのイベントハンドラ関連
        private void setEnabled(object sender, EventArgs e) {
            setEnabled();
        }

        // enabled = falseだと全部disableになる。
        private void setEnabled(bool enabled = true) {
            menuStrip1.Enabled = enabled;
            groupBox1.Enabled = enabled;
            groupBox2.Enabled = enabled;
            InputFileBrowseButton.Enabled = enabled;
            InputFromFileRadioButton.Enabled = enabled;
            InputFromTextboxRadioButton.Enabled = enabled;
            OutputBrowseButton.Enabled = enabled;
            outputFileNameTextBox.Enabled = enabled;
            if(enabled) {
                sourceTextBox.BackColor = (InputFromTextboxRadioButton.Checked ? Properties.Settings.Default.editorFontColor["テキスト"].Back : System.Drawing.SystemColors.ButtonFace);
                sourceTextBox.Enabled = InputFromTextboxRadioButton.Checked;
                inputFileNameTextBox.Enabled = InputFileBrowseButton.Enabled = InputFromFileRadioButton.Checked;
                色入力ToolStripMenuItem.Enabled = InputFromTextboxRadioButton.Checked;
            } else {
                sourceTextBox.Enabled = false;
                inputFileNameTextBox.Enabled = false;
            }
        }

        private void ExitCToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void showPreambleWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            showPreambleWindow(!showPreambleWindowToolStripMenuItem.Checked);
        }

        public void showPreambleWindow(bool show) {
            showPreambleWindowToolStripMenuItem.Checked = show;
            if(show) {
                myPreambleForm.setLocation(Math.Max(0, Left - myPreambleForm.Width), Top);
                myPreambleForm.Show();
            } else {
                myPreambleForm.Hide();
            }
        }

        private void showOutputWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            showOutputWindow(!showOutputWindowToolStripMenuItem.Checked);
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e) {
            Properties.Settings.Default.preamble = myPreambleForm.PreambleTextBox.Text;
            (new SettingForm()).ShowDialog();
            ChangeSetting();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) {
            (new AboutDialog()).ShowDialog();
        }
        #endregion

        #region その他のイベントハンドラ
        protected override void OnClosing(CancelEventArgs e) {
            if(Properties.Settings.Default.SaveSettings) saveSettings();
            base.OnClosing(e);
        }
        #endregion

        #region OutputController インターフェースの実装
        public void showPathError(string exeName, string necessary) {
            MessageBox.Show(String.Format(Properties.Resources.PATHERROR, exeName, necessary), Properties.Resources.FAILED, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void showExtensionError(string file) {
            MessageBox.Show(String.Format(Properties.Resources.INVALID_EXTENSION, file),Properties.Resources.ERROR, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        delegate void appendOutputDelegate(string log);
        public void appendOutput(string log) {
            if(myOutputForm.InvokeRequired) {
                this.Invoke(new appendOutputDelegate(appendOutput), new Object[] { log });
            } else {
                myOutputForm.getOutputTextBox().AppendText(log.Replace("\n","\r\n"));
            }
        }

        public void showGenerateError() {
            MessageBox.Show(Properties.Resources.GENERATEERROR, Properties.Resources.FAILED, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void errorIgnoredWarning() {
            MessageBox.Show(Properties.Resources.IGNOREDERRORWARNING, Properties.Resources.WARNING, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        delegate void scrollOutputTextBoxToEndDelegate();
        public void scrollOutputTextBoxToEnd() {
            TextBox output = myOutputForm.getOutputTextBox();
            if(output.InvokeRequired) {
                this.Invoke(new scrollOutputTextBoxToEndDelegate(scrollOutputTextBoxToEnd));
            } else {
                output.SelectionStart = output.Text.Length;
                output.ScrollToCaret();
            }
        }

        public void showUnauthorizedError(string filePath) {
            MessageBox.Show(String.Format(Properties.Resources.AUTHORIZEDERROR, filePath), Properties.Resources.FAILED, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void showIOError(string filePath) {
            MessageBox.Show(String.Format(Properties.Resources.IOERROR, filePath), Properties.Resources.FAILED, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void showError(string msg) {
            MessageBox.Show(msg, "TeX2img");
        }

        public bool askYesorNo(string msg) {
            return (MessageBox.Show(msg, "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
        }

        public void showToolError(string tool) {
            var path = Path.Combine(Converter.ShortToolPath, tool);
            MessageBox.Show(String.Format(Properties.Resources.TOOLERROR, path, Converter.ShortToolPath), Properties.Resources.FAILED, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        #endregion

        public void showOutputWindow(bool show) {
            showOutputWindowToolStripMenuItem.Checked = show;
            if(show) {
                myOutputForm.setLocation(Left + Width, Top);
                myOutputForm.Show();
            } else {
                myOutputForm.Hide();
            }
        }

        public void clearOutputTextBox() {
            myOutputForm.getOutputTextBox().Text = "";
        }

        private void GenerateButton_Click(object sender, EventArgs arg) {
            if(!convertWorker.IsBusy) {
                var ext = CheckExtension();
                if(ext == "") return;
                outputFileNameTextBox.Text = Path.ChangeExtension(outputFileNameTextBox.Text, ext);
                clearOutputTextBox();
                if(Properties.Settings.Default.showOutputWindowFlag) showOutputWindow(true);
                GenerateButton.Text = Properties.Resources.STOP;
                setEnabled(false);

                myOutputForm.Activate();
                this.Activate();
                try { convertWorker.RunWorkerAsync(100); }
                catch(InvalidOperationException e){
                    MessageBox.Show(String.Format(Properties.Resources.DETECTERROR, e.Message));
                }
            } else {
                if(converter != null) converter.Abort();
            }
        }

        private string CheckExtension() {
            string outExt = Path.GetExtension(outputFileNameTextBox.Text);
            string reqExt = "." + ExtensioncomboBox.SelectedItem.ToString();
            if(outExt.ToLower() == reqExt) return outExt;
            using(var dialog = new ConflictExtensionDialog(outExt, reqExt)) {
                dialog.ShowDialog();
                switch(dialog.ExtensionResult) {
                    case ConflictExtensionDialog.Extension.OutputFile: return outExt;
                    case ConflictExtensionDialog.Extension.Required: return reqExt;
                    case ConflictExtensionDialog.Extension.Both: return outExt + reqExt;
                    default: return "";
                }
            }
        }

        Converter converter = null;// 実行中でなければnull
        private void convertWorker_DoWork(object sender, DoWorkEventArgs e) {
            Properties.Settings.SetCurrentLanguage();
            try {
                Directory.CreateDirectory(Path.GetTempPath());
            }
            catch(Exception) {
                MessageBox.Show(String.Format(Properties.Resources.FAIL_TMPFOLDER, Path.GetTempPath()));
                return;
            }

            if(FirstFiles != null) {
                for(int i = 0 ; i < FirstFiles.Count / 2 ; ++i) {
                    string file = FirstFiles[2 * i];
                    string workdir;
                    if (Properties.Settings.Default.workingDirectory == "file") workdir = Path.GetDirectoryName(file);
                    else if (Properties.Settings.Default.workingDirectory == "current") workdir = Directory.GetCurrentDirectory();
                    else workdir = Path.GetTempPath();
                    string tmppath;
                    try {
                        string inputextension = Path.GetExtension(file);
                        tmppath = Path.Combine(workdir,TempFilesDeleter.GetTempFileName(inputextension));
                        File.Delete(tmppath);
                        File.Copy(file, tmppath, true);
                    }
                    catch(Exception) {
                        MessageBox.Show(String.Format(Properties.Resources.FAIL_TMPFILE, workdir));
                        continue;
                    }
                    var output = Path.GetFullPath(FirstFiles[2 * i + 1]);
                    converter = new Converter(this, tmppath, output);
                    converter.Convert();
                }
                FirstFiles = null;
                converter = null;
                return;
            }

            try {
                string outputFilePath = outputFileNameTextBox.Text;

                if(InputFromFileRadioButton.Checked) {
                    string inputTeXFilePath = inputFileNameTextBox.Text;
                    if(!File.Exists(inputTeXFilePath)) {
                        MessageBox.Show(String.Format(Properties.Resources.NOTEXIST, inputTeXFilePath), Properties.Resources.ERROR, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                string extension = Path.GetExtension(outputFilePath).ToLower();
                string tmpTeXFileName;
                string tmpDir;
                if (InputFromFileRadioButton.Checked) {
                    if (Properties.Settings.Default.workingDirectory == "file") tmpDir = Path.GetDirectoryName(inputFileNameTextBox.Text);
                    else if (Properties.Settings.Default.workingDirectory == "current") tmpDir = Directory.GetCurrentDirectory();
                    else tmpDir = Path.GetTempPath();
                    tmpTeXFileName = TempFilesDeleter.GetTempFileName(Path.GetExtension(inputFileNameTextBox.Text), tmpDir);
                }else{
                    if (Properties.Settings.Default.workingDirectory == "current") tmpDir = Directory.GetCurrentDirectory();
                    else tmpDir = Path.GetTempPath();
                    tmpTeXFileName = TempFilesDeleter.GetTempFileName();
                }

                if(tmpTeXFileName == null) {
                    MessageBox.Show(String.Format(Properties.Resources.FAIL_TMPFILE, tmpDir));
                    return;
                }
                string tmpFileBaseName = Path.GetFileNameWithoutExtension(tmpTeXFileName);

                using(converter = new Converter(this, Path.Combine(tmpDir, tmpTeXFileName), outputFileNameTextBox.Text)) {
                    if(!converter.CheckFormat()) return;
                    if(!converter.CheckInputFormat()) return;

                    #region TeX ソースファイルの準備
                    // 外部ファイルから入力する場合はテンポラリディレクトリにコピー
                    if(InputFromFileRadioButton.Checked) {
                        string inputTeXFilePath = inputFileNameTextBox.Text;
                        string tmpfile = Path.Combine(tmpDir, tmpTeXFileName);
                        File.Copy(inputTeXFilePath, tmpfile, true);
                        // 読み取り専用の場合解除しておく（後でFile.Deleteに失敗するため）。
                        (new FileInfo(tmpfile)).Attributes = FileAttributes.Normal;
                        converter.AddInputPath(Path.GetDirectoryName(inputTeXFilePath));
                    }

                    // 直接入力の場合 tex ソースを出力
                    if(InputFromTextboxRadioButton.Checked) {
                        using(StreamWriter sw = new StreamWriter(Path.Combine(tmpDir, tmpTeXFileName), false, Converter.GetInputEncoding())) {
                            try {
                                TeXSource.WriteTeXSourceFile(sw, myPreambleForm.PreambleTextBox.Text, sourceTextBox.Text);
                            }
                            catch { }
                        }
                    }
                    #endregion

                    if(converter.Convert()) {
                        if(Properties.Settings.Default.setFileToClipBoard) {
                            if(converter.OutputFileNames.Count > 0) {
                                Invoke(new Action(() => {
                                    var flist = new System.Collections.Specialized.StringCollection();
                                    foreach(var f in converter.OutputFileNames) flist.Add(f);
                                    Clipboard.SetFileDropList(flist);
                                }));
                            }
                        }
                    }
                }
            }
            finally {
                converter = null;
            }
        }

        private void convertWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            setEnabled(true);
            this.GenerateButton.Text = thisGenerateButtonText;
        }

        #region 設定変更通知関連
        public void ChangeSetting() {
            ChangeSettingofEditor(sourceTextBox);
            ChangeSettingofEditor(myPreambleForm.PreambleTextBox);
        }

        public void ChangeSettingofEditor(Sgry.Azuki.WinForms.AzukiControl textBox) {
            if(textBox == null) return;
            textBox.FontInfo = new Sgry.Azuki.FontInfo(Properties.Settings.Default.editorFont);
            var x = Properties.Settings.Default.editorFontColor;
            textBox.DrawsEofMark = Properties.Settings.Default.editorDrawEOF;
            textBox.DrawsEolCode = Properties.Settings.Default.editorDrawEOL;
            textBox.DrawsFullWidthSpace = Properties.Settings.Default.editorDrawFullWidthSpace;
            textBox.DrawsSpace = Properties.Settings.Default.editorDrawSpace;
            textBox.DrawsTab = Properties.Settings.Default.editorDrawTab;
            textBox.AcceptsTab = Properties.Settings.Default.editorAcceptTab;
            textBox.TabWidth = Properties.Settings.Default.editorTabWidth;
            textBox.ColorScheme.ForeColor = Properties.Settings.Default.editorFontColor["テキスト"].Font;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Normal, Properties.Settings.Default.editorFontColor["テキスト"].Font, Properties.Settings.Default.editorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading1, Properties.Settings.Default.editorFontColor["テキスト"].Font, Properties.Settings.Default.editorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading2, Properties.Settings.Default.editorFontColor["テキスト"].Font, Properties.Settings.Default.editorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading3, Properties.Settings.Default.editorFontColor["テキスト"].Font, Properties.Settings.Default.editorFontColor["テキスト"].Back);
            textBox.ColorScheme.SelectionFore = Properties.Settings.Default.editorFontColor["選択範囲"].Font;
            textBox.ColorScheme.SelectionBack = Properties.Settings.Default.editorFontColor["選択範囲"].Back;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCommand, Properties.Settings.Default.editorFontColor["コントロールシークエンス"].Font, Properties.Settings.Default.editorFontColor["コントロールシークエンス"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexEquation, Properties.Settings.Default.editorFontColor["$"].Font, Properties.Settings.Default.editorFontColor["$"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexBracket, Properties.Settings.Default.editorFontColor["中 / 大括弧"].Font, Properties.Settings.Default.editorFontColor["中 / 大括弧"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCurlyBracket, Properties.Settings.Default.editorFontColor["中 / 大括弧"].Font, Properties.Settings.Default.editorFontColor["中 / 大括弧"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Comment, Properties.Settings.Default.editorFontColor["コメント"].Font, Properties.Settings.Default.editorFontColor["コメント"].Back);
            textBox.ColorScheme.EofColor = Properties.Settings.Default.editorFontColor["改行，EOF"].Font;
            textBox.ColorScheme.EolColor = Properties.Settings.Default.editorFontColor["改行，EOF"].Font;
            textBox.ColorScheme.MatchedBracketFore = Properties.Settings.Default.editorFontColor["対応する括弧"].Font;
            textBox.ColorScheme.MatchedBracketBack = Properties.Settings.Default.editorFontColor["対応する括弧"].Back;
            textBox.ColorScheme.WhiteSpaceColor = Properties.Settings.Default.editorFontColor["空白"].Font;
            Color backColor = new Color();
            if(textBox.Enabled) backColor = Properties.Settings.Default.editorFontColor["テキスト"].Back;
            else backColor = System.Drawing.SystemColors.ButtonFace;
            textBox.ColorScheme.BackColor = backColor;
        }

        #endregion

        #region 右クリック
        private void Undo_Click(object sender, EventArgs e) {
            sourceTextBox.Undo();
        }

        private void Cut_Click(object sender, EventArgs e) {
            sourceTextBox.Cut();
        }

        private void Copy_Click(object sender, EventArgs e) {
            sourceTextBox.Copy();
        }

        private void Paste_Click(object sender, EventArgs e) {
            sourceTextBox.Paste();
        }

        private void Delete_Click(object sender, EventArgs e) {
            sourceTextBox.Delete();
        }

        private void SelectAll_Click(object sender, EventArgs e) {
            sourceTextBox.SelectAll();
        }

        private void Redo_Click(object sender, EventArgs e) {
            sourceTextBox.Redo();
        }
        #endregion

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e) {
            var ofd = new OpenFileDialog();

            ofd.Filter = String.Format(Properties.Resources.IMPORTDIALOG_FILTER,
                String.Join(", ", Converter.imageExtensions.Select(d => "*" + d).ToArray()),
                String.Join(";", Converter.imageExtensions.Select(d => "*" + d).ToArray()));
            if(ofd.ShowDialog() == DialogResult.OK) {
                try {
                    if(MessageBox.Show(Properties.Resources.IMPORTMSG, "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                        ImportFile(ofd.FileName);
                    }
                }
                catch(FileNotFoundException) {
                    MessageBox.Show(String.Format(Properties.Resources.NOTEXIST, ofd.FileName));
                }
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e) {
            if(TeXsourceSaveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                var encoding = Converter.GetInputEncoding();
                if(encoding.CodePage == Encoding.UTF8.CodePage) encoding = new System.Text.UTF8Encoding(false);
                try {
                    using(var file = new StreamWriter(TeXsourceSaveFileDialog.FileName, false, encoding)) {
                        TeXSource.WriteTeXSourceFile(file, myPreambleForm.PreambleTextBox.Text, sourceTextBox.Text);
                    }
                }
                catch(UnauthorizedAccessException){
                    MessageBox.Show(String.Format(Properties.Resources.AUTHORIZEDERROR, TeXsourceSaveFileDialog.FileName));
                }
            }
        }

        public void ImportFile(string path) {
            string preamble, body;
            ImportFile(path, out preamble, out body);
            if(preamble != null) myPreambleForm.PreambleTextBox.Text = TeXSource.ChangeReturnCode(preamble);
            if(body != null) sourceTextBox.Text = TeXSource.ChangeReturnCode(body);
        }

        public static void ImportFile(string path, out string preamble, out string body) {
            var ext = Path.GetExtension(path).ToLower();
            bool image = Converter.imageExtensions.Contains(ext);
            if(Properties.Settings.Default.embedTeXSource && image) ImportImageFile(path, out preamble, out body);
            else ImportTeXFile(path, out preamble, out body);
        }

        public static void ImportImageFile(string path, out string preamble, out string body) {
            preamble = null; body = null;
            try {
                var text = TeXSource.ReadEmbededSource(path);
                if (text != null) {
                    using (var sr = new StringReader(text)) {
                        if (!TeXSource.ParseTeXSourceFile(sr, out preamble, out body)) {
                            MessageBox.Show(Properties.Resources.ANALYZESOURCEERROR, "TeX2img");
                        }
                    }
                } else throw new IOException();
            }
            catch(NotImplementedException) {
                MessageBox.Show(Properties.Resources.EMBED_IS_ONLY_NTFS);
            }
            catch(Exception) {
                if(File.Exists(path)) {
                    MessageBox.Show(String.Format(Properties.Resources.NO_EMBEDDED_SOURCE, path), "TeX2img");
                } else {
                    MessageBox.Show(String.Format(Properties.Resources.FAIL_OPEN, path), "TeX2img");
                }
            }
        }

        public static void ImportTeXFile(string path,out string preamble,out string body){
            preamble = null; body = null;
            var extension = Path.GetExtension(path).ToLower();
            byte[] buf;
            using(var fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                buf = new byte[fs.Length];
                fs.Read(buf, 0, (int) fs.Length);
            }

            var GuessedEncoding = KanjiEncoding.CheckBOM(buf);
            bool bom = false;
            if(GuessedEncoding != null) {
                bom = true;
                buf = KanjiEncoding.DeleteBOM(buf, GuessedEncoding);
            } else {
                var guessed = KanjiEncoding.GuessKajiEncoding(buf);
                if(guessed.Length == 0) GuessedEncoding = null;
                else GuessedEncoding = guessed[0];
            }
            Encoding encoding;
            switch(Properties.Settings.Default.encode) {
            case "euc": encoding = Encoding.GetEncoding("euc-jp"); break;
            case "jis": encoding = Encoding.GetEncoding("iso-2022-jp"); break;
            case "utf8": encoding = Encoding.UTF8; break;
            case "sjis": encoding = Encoding.GetEncoding("shift_jis"); break;
            case "_utf8":
                if(GuessedEncoding != null) encoding = GuessedEncoding;
                else encoding = Encoding.UTF8;
                break;
            case "_sjis":
            default:
                if(GuessedEncoding != null) encoding = GuessedEncoding;
                else encoding = Encoding.GetEncoding("shift_jis");
                break;
            }
            if(encoding.CodePage != GuessedEncoding.CodePage) {
                if (bom || MessageBox.Show(String.Format(Properties.Resources.CHECK_ENCODEMSG, encoding.EncodingName, GuessedEncoding.EncodingName, GuessedEncoding.EncodingName), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    encoding = GuessedEncoding;
                }
            }
            using(var sr = new StringReader(encoding.GetString(buf))) {
                if(!TeXSource.ParseTeXSourceFile(sr, out preamble,out body)) {
                   MessageBox.Show(Properties.Resources.ANALYZESOURCEERROR,"TeX2img");
                }
            }
        }

        private void sourceTextBox_DragDrop(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(files.Length == 0)return;
                if (MessageBox.Show(Properties.Resources.IMPORTMSG, "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;
                ImportFile(files[0]);
            }
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void inputFileNameTextBox_DragDrop(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);
                if(files.Length == 0) return;
                inputFileNameTextBox.Text = files[0];
            }
        }

        private void ColorInputHelperToolStripMenuItem_Click(object sender, EventArgs e) {
            if(!InputFromTextboxRadioButton.Checked) {
                MessageBox.Show(Properties.Resources.ONLY_INPUTMODE,"TeX2img");
                return;
            }
            var enUS = new System.Globalization.CultureInfo("en-US");
            Func<Color, string> GetColorString = (c) => {
                return "{" + 
                    ((double) c.R / (double) 255).ToString(enUS) + "," +
                    ((double) c.G / (double) 255).ToString(enUS) + "," +
                    ((double) c.B / (double) 255).ToString(enUS) + "}";
            };
            using(var cdg = new SupportInputColorDialog()) {
                cdg.CustomColors = (int[])Properties.Settings.Default.ColorDialogCustomColors.Clone();
                cdg.AnyColor = true;
                if(cdg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    sourceTextBox.Document.SetSelection(sourceTextBox.CaretIndex, sourceTextBox.CaretIndex);
                    int startcaret = sourceTextBox.CaretIndex;
                    var colstring = GetColorString(cdg.Color);
                    switch(cdg.CheckedControlSequence) {
                    case SupportInputColorDialog.ControlSequence.colorbox:
                        sourceTextBox.Document.Replace("\\colorbox[rgb]" + colstring + "{}");
                        sourceTextBox.SetSelection(sourceTextBox.CaretIndex-1,sourceTextBox.CaretIndex-1);
                        break;
                    case SupportInputColorDialog.ControlSequence.textcolor:
                        sourceTextBox.Document.Replace("\\textcolor[rgb]" + colstring + "{}");
                        sourceTextBox.SetSelection(sourceTextBox.CaretIndex-1,sourceTextBox.CaretIndex-1);
                        break;
                    case SupportInputColorDialog.ControlSequence.definecolor:
                        sourceTextBox.Document.Replace("\\definecolor{}{rgb}" + colstring);
                        int shift = "\\definecolor{".Length;
                        sourceTextBox.SetSelection(startcaret + shift,startcaret +shift);
                        break;
                    case SupportInputColorDialog.ControlSequence.color:
                    default:
                        sourceTextBox.Document.Replace("\\color[rgb]" + colstring);
                        break;
                    }
                }
                cdg.CustomColors.CopyTo(Properties.Settings.Default.ColorDialogCustomColors, 0);
            }
        }

        private void ExtensioncomboBox_SelectionChangeCommitted(object sender, EventArgs e) {
            string ext = ExtensioncomboBox.SelectedItem.ToString();
            if(ext != "") {
                outputFileNameTextBox.Text = Path.ChangeExtension(outputFileNameTextBox.Text, ext);
            }
        }
    }
}
