using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.ComponentModel;

namespace TeX2img {
    public partial class MainForm : Form, IOutputController {
        private bool saveSettingsFlag;
        public SettingForm.Settings SettingData { get; set; }
        private OutputForm myOutputForm;
        private PreambleForm myPreambleForm;

        #region コンストラクタおよび初期化処理関連のメソッド
        public MainForm(SettingForm.Settings set,List<string> files) {
            SettingData = set;
            saveSettingsFlag = true;

            InitializeComponent();
            
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            myPreambleForm = new PreambleForm(this);
            myOutputForm = new OutputForm(this);

            sourceTextBox.Highlighter = Sgry.Azuki.Highlighter.Highlighters.Latex;
            sourceTextBox.Resize += delegate { sourceTextBox.ViewWidth = sourceTextBox.ClientSize.Width; };
            loadSettings();
            setPath();

            if(InputFromTextboxRadioButton.Checked) ActiveControl = sourceTextBox;
            else ActiveControl = inputFileNameTextBox;

        }

        private void setPath() {
            if(SettingData.PlatexPath == String.Empty || SettingData.DvipdfmxPath == String.Empty || SettingData.GsPath == String.Empty) {
                if(SettingData.PlatexPath == String.Empty) SettingData.PlatexPath = Converter.which("platex");
                if(SettingData.DvipdfmxPath == String.Empty) SettingData.DvipdfmxPath = Converter.which("dvipdfmx");
                if(SettingData.GsPath == String.Empty) {
                    SettingData.GsPath = Converter.which("gswin32c.exe");
                    if(SettingData.GsPath == "") {
                        SettingData.GsPath = Converter.which("gswin64c.exe");
                        if(SettingData.GsPath == "") {
                            SettingData.GsPath = Converter.which("rungs.exe");
                            if(SettingData.GsPath == "") {
                                if(SettingData.PlatexPath != "") {
                                    SettingData.GsPath = System.IO.Path.GetDirectoryName(SettingData.PlatexPath) + "\\rungs.exe";
                                    if(!System.IO.File.Exists(SettingData.GsPath)) SettingData.GsPath = "";
                                }
                            }
                        }
                    }
                }


                if(SettingData.PlatexPath == String.Empty || SettingData.DvipdfmxPath == String.Empty || SettingData.GsPath == String.Empty) {
                    MessageBox.Show("platex / dvipdfmx / gs のパス設定に失敗しました。\n環境設定画面で手動で設定してください。");
                    (new SettingForm(this)).ShowDialog();
                } else {
                    MessageBox.Show(String.Format("TeX 関連プログラムのパスを\n {0}\n {1}\n {2}\nに設定しました。\n違っている場合は環境設定画面で手動で変更してください。", SettingData.PlatexPath, SettingData.DvipdfmxPath, SettingData.GsPath));
                }
            }

            if(SettingData.GsDevice == "" && SettingData.GsPath != "") {
                // Ghostscriptのバージョンを取得する．
                Process proc = new Process();
                proc.StartInfo.RedirectStandardError = proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.FileName = SettingData.GsPath;
                proc.StartInfo.Arguments = "-v";
                proc.Start();
                string msg = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
                proc.WaitForExit(2000);
                if(!proc.HasExited) proc.Kill();
                Regex reg = new Regex("Ghostscript ([0-9]+)\\.([0-9]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var m = reg.Match(msg);
                if(m.Success) {
                    try {
                        int major = int.Parse(m.Groups[1].Value);
                        int minor = int.Parse(m.Groups[2].Value);
                        //System.Diagnostics.Debug.WriteLine("major = " + major.ToString() + ", minor = " + minor.ToString());
                        // 9.15以上ならばeps2write，そうでないならepwsrite
                        if(major > 9 || (major == 9 && minor >= 15)) SettingData.GsDevice = "eps2write";
                        else SettingData.GsDevice = "epswrite";
                    }
                    catch(FormatException) { }
                }
            }
            if(SettingData.GsDevice == "") SettingData.GsDevice = "epswrite";
        }

        #endregion

        #region 設定値の読み書き
        private void loadSettings() {
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            this.Left = Properties.Settings.Default.Left;
            this.Top = Properties.Settings.Default.Top;
            myPreambleForm.Height = Properties.Settings.Default.preambleWindowHeight;
            myPreambleForm.Width = Properties.Settings.Default.preambleWindowWidth;
            myOutputForm.Height = Properties.Settings.Default.outputWindowHeight;
            myOutputForm.Width = Properties.Settings.Default.outputWindowWidth;

            if(Properties.Settings.Default.outputFile != "") {
                outputFileNameTextBox.Text = Properties.Settings.Default.outputFile;
            } else {
                outputFileNameTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\equation.eps";
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
            Properties.Settings.Default.Height = this.Height;
            Properties.Settings.Default.Width = this.Width;
            Properties.Settings.Default.Left = this.Left;
            Properties.Settings.Default.Top = this.Top;
            Properties.Settings.Default.preambleWindowHeight = myPreambleForm.Height;
            Properties.Settings.Default.preambleWindowWidth = myPreambleForm.Width;
            Properties.Settings.Default.outputWindowHeight = myOutputForm.Height;
            Properties.Settings.Default.outputWindowWidth = myOutputForm.Width;


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

        private void setEnabled() {
            sourceTextBox.BackColor = (InputFromTextboxRadioButton.Checked ? SettingData.EditorFontColor["テキスト"].Back : System.Drawing.SystemColors.ButtonFace);
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

        public void showExtensionError(string file) {
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
                output.SelectionStart = output.Text.Length;
                output.ScrollToCaret();
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
            if(SettingData.ShowOutputWindowFlag) showOutputWindow(true);
            this.Enabled = false;

            convertWorker.RunWorkerAsync(100);
        }

        private void convertWorker_DoWork(object sender, DoWorkEventArgs e) {
            Converter converter = new Converter(SettingData,this);

            string outputFilePath = outputFileNameTextBox.Text;
            if(!converter.CheckFormat(outputFilePath)) {
                return;
            }

            if(InputFromFileRadioButton.Checked) {
                string inputTeXFilePath = inputFileNameTextBox.Text;
                if(!File.Exists(inputTeXFilePath)) {
                    MessageBox.Show(inputTeXFilePath + "  が存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            string extension = Path.GetExtension(outputFilePath).ToLower();

            string tmpFilePath = Path.GetTempFileName();
            string tmpFileName = Path.GetFileName(tmpFilePath);
            string tmpFileBaseName = Path.GetFileNameWithoutExtension(tmpFileName);

            string tmpTeXFileName = tmpFileBaseName + ".tex";
            string tmpDir = Path.GetDirectoryName(tmpFilePath);
            File.Delete(Path.Combine(tmpDir,tmpTeXFileName));
            File.Move(Path.Combine(tmpDir,tmpFileName),Path.Combine(tmpDir,tmpTeXFileName));

            #region TeX ソースファイルの準備
            // 外部ファイルから入力する場合はテンポラリディレクトリにコピー
            if(InputFromFileRadioButton.Checked) {
                string inputTeXFilePath = inputFileNameTextBox.Text;
                File.Copy(inputTeXFilePath, Path.Combine(tmpDir,tmpTeXFileName), true);
            }

            // 直接入力の場合 tex ソースを出力
            // BOM付きUTF-8にすることで，文字コードの推定を確実にさせる．
            if(InputFromTextboxRadioButton.Checked) {
                if(SettingData.Encode == "") SettingData.Encode = "_sjis";// あり得ないはずだけど
                string enc = SettingData.Encode;
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
                        sw.WriteLine("\\begin{document}");
                        sw.Write(sourceTextBox.Text);
                        sw.WriteLine("\\end{document}");
                    }
                    finally {
                        if(sw != null) {
                            sw.Dispose();
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
            ChangeSettingofEditor(sourceTextBox);
            ChangeSettingofEditor(myPreambleForm.PreambleTextBox);
        }

        public void ChangeSettingofEditor(Sgry.Azuki.WinForms.AzukiControl textBox) {
            if(textBox == null) return;
            textBox.FontInfo = new Sgry.Azuki.FontInfo(SettingData.EditorFont);
            textBox.ColorScheme.ForeColor = SettingData.EditorFontColor["テキスト"].Font;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Normal, SettingData.EditorFontColor["テキスト"].Font, SettingData.EditorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading1, SettingData.EditorFontColor["テキスト"].Font, SettingData.EditorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading2, SettingData.EditorFontColor["テキスト"].Font, SettingData.EditorFontColor["テキスト"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Heading3, SettingData.EditorFontColor["テキスト"].Font, SettingData.EditorFontColor["テキスト"].Back);
            textBox.ColorScheme.SelectionFore = SettingData.EditorFontColor["選択範囲"].Font;
            textBox.ColorScheme.SelectionBack = SettingData.EditorFontColor["選択範囲"].Back;
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCommand, SettingData.EditorFontColor["コントロールシークエンス"].Font, SettingData.EditorFontColor["コントロールシークエンス"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexEquation, SettingData.EditorFontColor["$"].Font, SettingData.EditorFontColor["$"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexBracket, SettingData.EditorFontColor["中 / 大括弧"].Font, SettingData.EditorFontColor["中 / 大括弧"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.LatexCurlyBracket, SettingData.EditorFontColor["中 / 大括弧"].Font, SettingData.EditorFontColor["中 / 大括弧"].Back);
            textBox.ColorScheme.SetColor(Sgry.Azuki.CharClass.Comment, SettingData.EditorFontColor["コメント"].Font, SettingData.EditorFontColor["コメント"].Back);
            textBox.ColorScheme.EofColor = SettingData.EditorFontColor["改行，EOF"].Font;
            textBox.ColorScheme.EolColor = SettingData.EditorFontColor["改行，EOF"].Font;
            textBox.ColorScheme.MatchedBracketFore = SettingData.EditorFontColor["対応する括弧"].Font;
            textBox.ColorScheme.MatchedBracketBack = SettingData.EditorFontColor["対応する括弧"].Back;
            Color backColor = new Color();
            if(textBox.Enabled) backColor = SettingData.EditorFontColor["テキスト"].Back;
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
    }
}
