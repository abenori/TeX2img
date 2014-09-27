using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace TeX2img {
    public partial class MainForm : Form, IOutputController {
        #region プロパティの設定
        public string PlatexPath {get; set;}
        public string DvipdfmxPath {get; set;}
        public string GsPath {get; set;}
        public int ResolutionScale {get; set;}
        public bool UseMagickFlag {get; set;}
        public bool TransparentPngFlag {get; set;}
        public bool ShowOutputWindowFlag {get; set;}
        public bool PreviewFlag {get; set;}
        public bool DeleteTmpFileFlag {get; set;}
        public bool IgnoreErrorFlag {get; set;}
        public decimal TopMargin  { get; set; }
        public decimal BottomMargin { get; set; }
        public decimal LeftMargin { get; set; }
        public decimal RightMargin { get; set; }
        public int SettingTabIndex { get; set; }
        public bool YohakuUnitBP { get; set; }
        private bool saveSettingsFlag;
        public Font EditorFont {
            get { return sourceTextBox.Font; }
            set { sourceTextBox.Font = value; }
        }

        public class FontColor { public Color Font, Back; public FontColor() { } }
        
        public FontColor EditorNormalFontColor { get; set; }
        public FontColor EditorSelectedFontColor { get; set; }
        public FontColor EditorCommandFontColor { get; set; }
        public FontColor EditorEquationFontColor { get; set; }
        public FontColor EditorBracketFontColor { get; set; }
        public FontColor EditorCommentFontColor { get; set; }
        public FontColor EditorEOFFontColor { get; set; }

        // 文字コードを表す utf8,sjis,jis,euc
        // _utf8, _sjisは文字コードを推定に任せ，それぞれ入力されたソースをUTF-8/Shift_JISで扱う
        public string Encode {get; set;}

        #endregion

        private OutputForm myOutputForm;
        private PreambleForm myPreambleForm;

        #region コンストラクタおよび初期化処理関連のメソッド
        public MainForm() {
            TopMargin = 0;
            BottomMargin = 0;
            LeftMargin = 0;
            RightMargin = 0;
            ShowOutputWindowFlag = true;
            PreviewFlag = true;
            DeleteTmpFileFlag = true;
            IgnoreErrorFlag = false;
            SettingTabIndex = 0;
            saveSettingsFlag = true;
            YohakuUnitBP = false;

            EditorNormalFontColor = new FontColor();
            EditorSelectedFontColor = new FontColor();
            EditorCommandFontColor = new FontColor();
            EditorEquationFontColor = new FontColor();
            EditorBracketFontColor = new FontColor();
            EditorCommentFontColor = new FontColor();
            EditorEOFFontColor = new FontColor();

            InitializeComponent();
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            myPreambleForm = new PreambleForm(this);
            myOutputForm = new OutputForm(this);
            loadSettings();
            loadCommandLine();
            setPath();
            sourceTextBox.Highlighter = Sgry.Azuki.Highlighter.Highlighters.Latex;

            if(InputFromTextboxRadioButton.Checked) ActiveControl = sourceTextBox;
            else ActiveControl = inputFileNameTextBox;
        }

        private void loadCommandLine() {
            string[] cmds = Environment.GetCommandLineArgs();
            bool exit = false;
            for(int i = 0 ; i < cmds.Length ; ++i) {
                switch(cmds[i]) {
                case "/platex":
                    ++i;
                    if(i == cmds.Length) break;
                    PlatexPath = cmds[i];
                    break;
                case "/dvipdfmx":
                    ++i;
                    if(i == cmds.Length) break;
                    DvipdfmxPath = cmds[i];
                    break;
                case "/gs":
                    ++i;
                    if(i == cmds.Length) break;
                    GsPath = cmds[i];
                    break;
                case "/exit":
                    exit = true;
                    break;
                case "/nosavesetting":
                    saveSettingsFlag = false;
                    break;
                default:
                    break;
                }
            }
            if(exit) {
                if(saveSettingsFlag)saveSettings();
                Environment.Exit(0);
            }
        }


        private void setPath() {
            if(PlatexPath == String.Empty || DvipdfmxPath == String.Empty || GsPath == String.Empty) {
                if(PlatexPath == String.Empty) PlatexPath = Converter.which("platex");
                if(DvipdfmxPath == String.Empty) DvipdfmxPath = Converter.which("dvipdfmx");
                if(GsPath == String.Empty) GsPath = Converter.guessgsPath();

                if(PlatexPath == String.Empty || DvipdfmxPath == String.Empty || GsPath == String.Empty) {
                    MessageBox.Show("platex / dvipdfmx / gs のパス設定に失敗しました。\n環境設定画面で手動で設定してください。");
                    (new SettingForm(this)).ShowDialog();
                } else {
                    MessageBox.Show(String.Format("TeX 関連プログラムのパスを\n {0}\n {1}\n {2}\nに設定しました。\n違っている場合は環境設定画面で手動で変更してください。", PlatexPath, DvipdfmxPath, GsPath));
                }
            }
        }

        #endregion

        #region 設定値の読み書き
        private void loadSettings() {
            PlatexPath = Properties.Settings.Default.platexPath;
            DvipdfmxPath = Properties.Settings.Default.dvipdfmxPath;
            GsPath = Properties.Settings.Default.gsPath;
            Encode = Properties.Settings.Default.encode;

            TransparentPngFlag = Properties.Settings.Default.transparentPngFlag;
            ResolutionScale = Properties.Settings.Default.resolutionScale;
            TopMargin = Properties.Settings.Default.topMargin;
            LeftMargin = Properties.Settings.Default.leftMargin;
            RightMargin = Properties.Settings.Default.rightMargin;
            BottomMargin = Properties.Settings.Default.bottomMargin;
            YohakuUnitBP = Properties.Settings.Default.yohakuUnitBP;

            UseMagickFlag = Properties.Settings.Default.useMagickFlag;

            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;
            myPreambleForm.Height = Properties.Settings.Default.preambleWindowHeight;
            myPreambleForm.Width = Properties.Settings.Default.preambleWindowWidth;
            myOutputForm.Height = Properties.Settings.Default.outputWindowHeight;
            myOutputForm.Width = Properties.Settings.Default.outputWindowWidth;

            ShowOutputWindowFlag = Properties.Settings.Default.showOutputWindowFlag;
            PreviewFlag = Properties.Settings.Default.previewFlag;
            DeleteTmpFileFlag = Properties.Settings.Default.deleteTmpFileFlag;
            IgnoreErrorFlag = Properties.Settings.Default.ignoreErrorFlag;

            SettingTabIndex = Properties.Settings.Default.settingTabIndex;

            if(Properties.Settings.Default.outputFile != "") {
                outputFileNameTextBox.Text = Properties.Settings.Default.outputFile;
            } else {
                outputFileNameTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\equation.eps";
            }
            inputFileNameTextBox.Text = Properties.Settings.Default.inputFile;
            InputFromTextboxRadioButton.Checked = Properties.Settings.Default.inputFromTextBox;
            InputFromFileRadioButton.Checked = !InputFromTextboxRadioButton.Checked;

            EditorFont = Properties.Settings.Default.editorFont;
            EditorNormalFontColor.Font = Properties.Settings.Default.editorNormalColorFont;
            EditorNormalFontColor.Back = Properties.Settings.Default.editorNormalColorBack;
            EditorSelectedFontColor.Font = Properties.Settings.Default.editorSelectedColorFont;
            EditorSelectedFontColor.Back = Properties.Settings.Default.editorSelectedColorBack;
            EditorCommandFontColor.Font = Properties.Settings.Default.editorCommandColorFont;
            EditorCommandFontColor.Back = Properties.Settings.Default.editorCommandColorBack;
            EditorEquationFontColor.Font = Properties.Settings.Default.editorEquationColorFont;
            EditorEquationFontColor.Back = Properties.Settings.Default.editorEquationColorBack;
            EditorBracketFontColor.Font = Properties.Settings.Default.editorBracketColorFont;
            EditorBracketFontColor.Back = Properties.Settings.Default.editorBracketColorBack;
            EditorCommentFontColor.Font = Properties.Settings.Default.editorCommentColorFont;
            EditorCommentFontColor.Back = Properties.Settings.Default.editorCommentColorBack;
            EditorEOFFontColor.Font = Properties.Settings.Default.editorEOFColorFont;
            EditorEOFFontColor.Back = Properties.Settings.Default.editorNormalColorBack;

            setEnabled();

            Sgry.Azuki.WinForms.AzukiControl preambleTextBox = myPreambleForm.PreambleTextBox;
            preambleTextBox.Text = Properties.Settings.Default.preamble;
            // プリアンブルフォームのキャレットを最後に持っていく
            //            preambleTextBox.SelectionStart = preambleTextBox.Text.Length;
            preambleTextBox.Focus();
            preambleTextBox.ScrollToCaret();

            ChangeSetting();
        }

        private void saveSettings() {
            Properties.Settings.Default.platexPath = PlatexPath;
            Properties.Settings.Default.dvipdfmxPath = DvipdfmxPath;
            Properties.Settings.Default.gsPath = GsPath;
            Properties.Settings.Default.encode = Encode;

            Properties.Settings.Default.resolutionScale = ResolutionScale;
            Properties.Settings.Default.transparentPngFlag = TransparentPngFlag;
            Properties.Settings.Default.topMargin = TopMargin;
            Properties.Settings.Default.leftMargin = LeftMargin;
            Properties.Settings.Default.rightMargin = RightMargin;
            Properties.Settings.Default.bottomMargin = BottomMargin;
            Properties.Settings.Default.yohakuUnitBP = YohakuUnitBP;

            Properties.Settings.Default.useMagickFlag = UseMagickFlag;

            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.preambleWindowHeight = myPreambleForm.Height;
            Properties.Settings.Default.preambleWindowWidth = myPreambleForm.Width;
            Properties.Settings.Default.outputWindowHeight = myOutputForm.Height;
            Properties.Settings.Default.outputWindowWidth = myOutputForm.Width;

            Properties.Settings.Default.showOutputWindowFlag = ShowOutputWindowFlag;
            Properties.Settings.Default.previewFlag = PreviewFlag;
            Properties.Settings.Default.deleteTmpFileFlag = DeleteTmpFileFlag;
            Properties.Settings.Default.ignoreErrorFlag = IgnoreErrorFlag;

            Properties.Settings.Default.outputFile = outputFileNameTextBox.Text;
            Properties.Settings.Default.inputFile = inputFileNameTextBox.Text;
            Properties.Settings.Default.inputFromTextBox = InputFromTextboxRadioButton.Checked;
            Properties.Settings.Default.preamble = myPreambleForm.PreambleTextBox.Text;

            Properties.Settings.Default.editorFont = EditorFont;

            Properties.Settings.Default.editorNormalColorFont = EditorNormalFontColor.Font;
            Properties.Settings.Default.editorNormalColorBack = EditorNormalFontColor.Back;
            Properties.Settings.Default.editorSelectedColorFont = EditorSelectedFontColor.Font;
            Properties.Settings.Default.editorSelectedColorBack = EditorSelectedFontColor.Back;
            Properties.Settings.Default.editorCommandColorFont = EditorCommandFontColor.Font;
            Properties.Settings.Default.editorCommandColorBack = EditorCommandFontColor.Back;
            Properties.Settings.Default.editorEquationColorFont = EditorEquationFontColor.Font;
            Properties.Settings.Default.editorEquationColorBack = EditorEquationFontColor.Back;
            Properties.Settings.Default.editorBracketColorFont = EditorBracketFontColor.Font;
            Properties.Settings.Default.editorBracketColorBack = EditorBracketFontColor.Back;
            Properties.Settings.Default.editorCommentColorFont = EditorCommentFontColor.Font;
            Properties.Settings.Default.editorCommentColorBack = EditorCommentFontColor.Back;
            Properties.Settings.Default.editorEOFColorFont = EditorEOFFontColor.Font;
            Properties.Settings.Default.editorNormalColorBack = EditorEOFFontColor.Back;

            Properties.Settings.Default.settingTabIndex = SettingTabIndex;

            Properties.Settings.Default.Save();
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

        private void setEnabled() {
            sourceTextBox.BackColor = (InputFromTextboxRadioButton.Checked ? System.Drawing.SystemColors.Window : System.Drawing.SystemColors.ButtonFace);
            sourceTextBox.Enabled = InputFromTextboxRadioButton.Checked;
            inputFileNameTextBox.Enabled = InputFileBrowseButton.Enabled = InputFromFileRadioButton.Checked;
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
                myPreambleForm.setLocation(Left - myPreambleForm.Width, Top);
                myPreambleForm.Show();
            } else {
                myPreambleForm.Hide();
            }
        }

        private void showOutputWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            showOutputWindow(!showOutputWindowToolStripMenuItem.Checked);
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e) {
            (new SettingForm(this)).ShowDialog();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e) {
            (new AboutDialog()).ShowDialog();
        }
        #endregion

        #region その他のイベントハンドラ
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(saveSettingsFlag)saveSettings();
        }

        private void sourceTextBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.Control && e.KeyCode == Keys.A) {
                ((TextBox) sender).SelectAll();
            }
        }
        #endregion

        #region OutputController インターフェースの実装
        public void showPathError(string exeName, string necessary) {
            MessageBox.Show(exeName + " を起動することができませんでした。\n" + necessary + "がインストールされているか，\n" + exeName + " のパスの設定が正しいかどうか，\n確認してください。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void showExtensionError() {
            MessageBox.Show("出力ファイルの拡張子は eps/png/jpg/pdf のいずれかにしてください。", "ファイル形式エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        delegate void appendOutputDelegate(string log);
        public void appendOutput(string log) {
            if(myOutputForm.InvokeRequired) {
                this.Invoke(new appendOutputDelegate(appendOutput), new Object[] { log });
            } else {
                myOutputForm.getOutputTextBox().Text += log;
            }
        }

        public void showGenerateError() {
            MessageBox.Show("画像生成に失敗しました。\nソースコードにエラーがないか確認してください。", "画像生成失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void showPstoeditError() {
            MessageBox.Show(@".\pstoedit\pstoedit.exe を起動することができませんでした。" + "\n付属の pstoedit フォルダを消さないでください。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public void showImageMagickError() {
            MessageBox.Show("ImageMagick がインストールされていないため，画像変換ができませんでした。\nImageMagick を別途インストールしてください。\nインストールされている場合は，パスが通っているかどうか確認してください。", "失敗", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        delegate void scrollOutputTextBoxToEndDelegate();
        public void scrollOutputTextBoxToEnd() {
            TextBox output = myOutputForm.getOutputTextBox();
            if(output.InvokeRequired) {
                this.Invoke(new scrollOutputTextBoxToEndDelegate(scrollOutputTextBoxToEnd));
            } else {
                //output.SelectionLength = 0;
                //output.Text += "";
                //output.Select(output.Text.Length, 0);
                output.SelectionStart = output.Text.Length;
                output.ScrollToCaret();
                //output.Focus();
                //output.Refresh();
            }
        }

        public void showUnauthorizedError(string filePath) {
            MessageBox.Show(filePath + "\nに上書き保存できませんでした。\n一度このファイルを削除してから再試行してください。", "画像生成失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void showIOError(string filePath) {
            MessageBox.Show(filePath + "\nが他のアプリケーション開かれているため生成できませんでした。\nこのファイルを開いているアプリケーションを閉じて再試行してください。", "画像生成失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            clearOutputTextBox();
            if(ShowOutputWindowFlag) showOutputWindow(true);
            this.Enabled = false;

            convertWorker.RunWorkerAsync(100);
        }

        private void convertWorker_DoWork(object sender, DoWorkEventArgs e) {
            Converter converter = new Converter(PlatexPath, DvipdfmxPath, GsPath,Encode,
                   ResolutionScale, LeftMargin, RightMargin, TopMargin, BottomMargin,YohakuUnitBP,
                         UseMagickFlag, TransparentPngFlag, ShowOutputWindowFlag, PreviewFlag, DeleteTmpFileFlag, IgnoreErrorFlag,
                            this);

            string outputFilePath = outputFileNameTextBox.Text;
            if(!converter.CheckFormat(outputFilePath)) {
                return;
            }

            string extension = Path.GetExtension(outputFilePath).ToLower();

            string tmpFilePath = Path.GetTempFileName();
            string tmpFileName = Path.GetFileName(tmpFilePath);
            string tmpFileBaseName = Path.GetFileNameWithoutExtension(tmpFileName);

            string tmpTeXFileName = tmpFileBaseName + ".tex";
            string tmpDir = Path.GetDirectoryName(tmpFilePath);
            Directory.SetCurrentDirectory(tmpDir);
            File.Delete(tmpTeXFileName);
            File.Move(tmpFileName, tmpTeXFileName);

            #region TeX ソースファイルの準備
            // 外部ファイルから入力する場合はテンポラリディレクトリにコピー
            if(InputFromFileRadioButton.Checked) {
                string inputTeXFilePath = inputFileNameTextBox.Text;
                if(!File.Exists(inputTeXFilePath)) {
                    MessageBox.Show(inputTeXFilePath + "  が存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                File.Copy(inputTeXFilePath, tmpTeXFileName, true);
            }

            // 直接入力の場合 tex ソースを出力
            // BOM付きUTF-8にすることで，文字コードの推定を確実にさせる．
            if(InputFromTextboxRadioButton.Checked) {
                string enc = Encode;
                if(enc.Substring(0, 1) == "_") enc = enc.Remove(0, 1);
                Encoding encoding;
                switch(enc) {
                case "sjis": encoding = Encoding.GetEncoding("shift_jis");break;
                case "euc": encoding = Encoding.GetEncoding("euc-jp");break;
                case "jis": encoding = Encoding.GetEncoding("iso-2022-jp"); break;
                default: encoding = Encoding.UTF8; break;
                }
                using(StreamWriter sw = new StreamWriter(Path.Combine(tmpDir, tmpTeXFileName), false, encoding)) {
                    try {
                        sw.Write(myPreambleForm.PreambleTextBox.Text);
                        sw.Write("\\begin{document}");
                        sw.Write(sourceTextBox.Text);
                        sw.Write("\\end{document}");
                    }
                    finally {
                        if(sw != null) {
                            sw.Close();
                        }
                    }
                }
            }
            #endregion

            converter.Convert(Path.Combine(tmpDir, tmpTeXFileName), outputFileNameTextBox.Text);
        }

        private void convertWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            this.Enabled = true;
        }

        #region 設定変更通知関連
        public void ChangeSetting() {
            myPreambleForm.PreambleTextBox.Font = sourceTextBox.Font;
            ChangeColorSchemeOfEditor(sourceTextBox);
            ChangeColorSchemeOfEditor(myPreambleForm.PreambleTextBox);
        }

        public void ChangeColorSchemeOfEditor(Sgry.Azuki.WinForms.AzukiControl textBox) {
            if(textBox == null) return;
            textBox.ColorScheme.ForeColor = EditorNormalFontColor.Font;
            textBox.ColorScheme.BackColor = EditorNormalFontColor.Back;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Normal,EditorNormalFontColor.Font,EditorNormalFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading1, EditorNormalFontColor.Font, EditorNormalFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading2, EditorNormalFontColor.Font, EditorNormalFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading3, EditorNormalFontColor.Font, EditorNormalFontColor.Back);
            textBox.ColorScheme.SelectionFore = EditorSelectedFontColor.Font;
            textBox.ColorScheme.SelectionBack = EditorSelectedFontColor.Back;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCommand, EditorCommandFontColor.Font, EditorCommandFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexEquation, EditorEquationFontColor.Font, EditorEquationFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexBracket, EditorBracketFontColor.Font, EditorBracketFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCurlyBracket, EditorBracketFontColor.Font, EditorBracketFontColor.Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Comment, EditorCommentFontColor.Font, EditorCommentFontColor.Back);
            textBox.ColorScheme.EofColor = EditorEOFFontColor.Font;
            textBox.ColorScheme.EolColor = EditorEOFFontColor.Font;
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
    }
}
