using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace TeX2img {
    public partial class MainForm : Form, IOutputController {
        #region プロパティの設定
        private int resolutionScale;
        private bool transparentPngFlag;
        private decimal topmargin = 0;
        private decimal bottommargin = 0;
        private decimal leftmargin = 0;
        private decimal rightmargin = 0;
        private string platexPath = "";
        private string dvipdfmxPath = "";
        private string gsPath = "";
        private bool useMagickFlag = false;
        private bool showOutputWindowFlag = true;
        private bool previewFlag = true;
        private bool deleteTmpFileFlag = true;
        private bool ignoreErrorFlag = false;
        private int settingTabIndex = 0;
        private bool saveSettings_ = true;
        private bool yohakuunitbp_ = false;

        public string PlatexPath {
            get { return platexPath; }
            set { platexPath = value; }
        }

        public string DvipdfmxPath {
            get { return dvipdfmxPath; }
            set { dvipdfmxPath = value; }
        }

        public string GsPath {
            get { return gsPath; }
            set { gsPath = value; }
        }

        public int ResolutionScale {
            get { return resolutionScale; }
            set { resolutionScale = value; }
        }

        public bool UseMagickFlag {
            get { return useMagickFlag; }
            set { useMagickFlag = value; }
        }

        public bool TransparentPngFlag {
            get { return transparentPngFlag; }
            set { transparentPngFlag = value; }
        }

        public bool ShowOutputWindowFlag {
            get { return showOutputWindowFlag; }
            set { showOutputWindowFlag = value; }
        }

        public bool PreviewFlag {
            get { return previewFlag; }
            set { previewFlag = value; }
        }

        public bool DeleteTmpFileFlag {
            get { return deleteTmpFileFlag; }
            set { deleteTmpFileFlag = value; }
        }

        public bool IgnoreErrorFlag {
            get { return ignoreErrorFlag; }
            set { ignoreErrorFlag = value; }
        }

        public decimal TopMargin {
            get { return topmargin; }
            set { topmargin = value; }
        }

        public decimal BottomMargin {
            get { return bottommargin; }
            set { bottommargin = value; }
        }

        public decimal LeftMargin {
            get { return leftmargin; }
            set { leftmargin = value; }
        }

        public decimal RightMargin {
            get { return rightmargin; }
            set { rightmargin = value; }
        }

        public int SettingTabIndex {
            get { return settingTabIndex; }
            set { settingTabIndex = value; }
        }

        public bool YohakuUnitBP {
            get { return yohakuunitbp_; }
            set { yohakuunitbp_ = value;  }
        }
        #endregion

        private OutputForm myOutputForm;
        private PreambleForm myPreambleForm;

        #region コンストラクタおよび初期化処理関連のメソッド
        public MainForm() {
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
                    platexPath = cmds[i];
                    break;
                case "/dvipdfmx":
                    ++i;
                    if(i == cmds.Length) break;
                    dvipdfmxPath = cmds[i];
                    break;
                case "/gs":
                    ++i;
                    if(i == cmds.Length) break;
                    gsPath = cmds[i];
                    break;
                case "/exit":
                    exit = true;
                    break;
                case "/nosavesetting":
                    saveSettings_ = false;
                    break;
                default:
                    break;
                }
            }
            if(exit) {
                if(saveSettings_)saveSettings();
                Environment.Exit(0);
            }
        }


        private void setPath() {
            if(platexPath == String.Empty || dvipdfmxPath == String.Empty || gsPath == String.Empty) {
                if(platexPath == String.Empty) platexPath = Converter.which("platex");
                if(dvipdfmxPath == String.Empty) dvipdfmxPath = Converter.which("dvipdfmx");
                if(gsPath == String.Empty) gsPath = Converter.guessgsPath();

                if(platexPath == String.Empty || dvipdfmxPath == String.Empty || gsPath == String.Empty) {
                    MessageBox.Show("platex / dvipdfmx / gs のパス設定に失敗しました。\n環境設定画面で手動で設定してください。");
                    (new SettingForm(this)).ShowDialog();
                } else {
                    MessageBox.Show(String.Format("TeX 関連プログラムのパスを\n {0}\n {1}\n {2}\nに設定しました。\n違っている場合は環境設定画面で手動で変更してください。", platexPath, dvipdfmxPath, gsPath));
                }
            }
        }

        #endregion

        #region 設定値の読み書き
        private void loadSettings() {
            platexPath = Properties.Settings.Default.platexPath;
            dvipdfmxPath = Properties.Settings.Default.dvipdfmxPath;
            gsPath = Properties.Settings.Default.gsPath;

            transparentPngFlag = Properties.Settings.Default.transparentPngFlag;
            resolutionScale = Properties.Settings.Default.resolutionScale;
            topmargin = Properties.Settings.Default.topMargin;
            leftmargin = Properties.Settings.Default.leftMargin;
            rightmargin = Properties.Settings.Default.rightMargin;
            bottommargin = Properties.Settings.Default.bottomMargin;
            yohakuunitbp_ = Properties.Settings.Default.yohakuUnitBP;

            useMagickFlag = Properties.Settings.Default.useMagickFlag;

            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;
            myPreambleForm.Height = Properties.Settings.Default.preambleWindowHeight;
            myPreambleForm.Width = Properties.Settings.Default.preambleWindowWidth;
            myOutputForm.Height = Properties.Settings.Default.outputWindowHeight;
            myOutputForm.Width = Properties.Settings.Default.outputWindowWidth;

            showOutputWindowFlag = Properties.Settings.Default.showOutputWindowFlag;
            previewFlag = Properties.Settings.Default.previewFlag;
            deleteTmpFileFlag = Properties.Settings.Default.deleteTmpFileFlag;
            ignoreErrorFlag = Properties.Settings.Default.ignoreErrorFlag;

            settingTabIndex = Properties.Settings.Default.settingTabIndex;

            if(Properties.Settings.Default.outputFile != "") {
                outputFileNameTextBox.Text = Properties.Settings.Default.outputFile;
            } else {
                outputFileNameTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\equation.eps";
            }
            inputFileNameTextBox.Text = Properties.Settings.Default.inputFile;
            InputFromTextboxRadioButton.Checked = Properties.Settings.Default.inputFromTextBox;
            InputFromFileRadioButton.Checked = !InputFromTextboxRadioButton.Checked;
            setEnabled();

            Sgry.Azuki.WinForms.AzukiControl preambleTextBox = myPreambleForm.PreambleTextBox;
            preambleTextBox.Text = Properties.Settings.Default.preamble;
            // プリアンブルフォームのキャレットを最後に持っていく
            //            preambleTextBox.SelectionStart = preambleTextBox.Text.Length;
            preambleTextBox.Focus();
            preambleTextBox.ScrollToCaret();
        }

        private void saveSettings() {
            Properties.Settings.Default.platexPath = platexPath;
            Properties.Settings.Default.dvipdfmxPath = dvipdfmxPath;
            Properties.Settings.Default.gsPath = gsPath;

            Properties.Settings.Default.resolutionScale = resolutionScale;
            Properties.Settings.Default.transparentPngFlag = transparentPngFlag;
            Properties.Settings.Default.topMargin = topmargin;
            Properties.Settings.Default.leftMargin = leftmargin;
            Properties.Settings.Default.rightMargin = rightmargin;
            Properties.Settings.Default.bottomMargin = bottommargin;
            Properties.Settings.Default.yohakuUnitBP = yohakuunitbp_;

            Properties.Settings.Default.useMagickFlag = useMagickFlag;

            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.preambleWindowHeight = myPreambleForm.Height;
            Properties.Settings.Default.preambleWindowWidth = myPreambleForm.Width;
            Properties.Settings.Default.outputWindowHeight = myOutputForm.Height;
            Properties.Settings.Default.outputWindowWidth = myOutputForm.Width;

            Properties.Settings.Default.showOutputWindowFlag = showOutputWindowFlag;
            Properties.Settings.Default.previewFlag = previewFlag;
            Properties.Settings.Default.deleteTmpFileFlag = deleteTmpFileFlag;
            Properties.Settings.Default.ignoreErrorFlag = ignoreErrorFlag;

            Properties.Settings.Default.outputFile = outputFileNameTextBox.Text;
            Properties.Settings.Default.inputFile = inputFileNameTextBox.Text;
            Properties.Settings.Default.inputFromTextBox = InputFromTextboxRadioButton.Checked;
            Properties.Settings.Default.preamble = myPreambleForm.PreambleTextBox.Text;

            Properties.Settings.Default.settingTabIndex = settingTabIndex;

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
            if(saveSettings_)saveSettings();
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
            if(showOutputWindowFlag) showOutputWindow(true);
            this.Enabled = false;

            convertWorker.RunWorkerAsync(100);
        }

        private void convertWorker_DoWork(object sender, DoWorkEventArgs e) {
            Converter converter = new Converter(platexPath, dvipdfmxPath, gsPath,
                   resolutionScale, leftmargin, rightmargin, topmargin, bottommargin,yohakuunitbp_,
                         useMagickFlag, transparentPngFlag, showOutputWindowFlag, previewFlag, deleteTmpFileFlag, ignoreErrorFlag,
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
            if(InputFromTextboxRadioButton.Checked) {
                using(StreamWriter sw = new StreamWriter(Path.Combine(tmpDir, tmpTeXFileName), false, Encoding.GetEncoding("shift_jis"))) {
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
    }
}
