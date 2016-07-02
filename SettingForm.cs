using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace TeX2img {
    public partial class SettingForm : Form {
        DataTable EncodeComboboxData = new DataTable();
        DataTable LanguageComboboxData = new DataTable();
        public SettingForm() {
            InitializeComponent();

            DataRow row;
            EncodeComboboxData.Columns.Add("DATA", typeof(string));
            EncodeComboboxData.Columns.Add("SHOW", typeof(string));
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "_utf8"; row["SHOW"] = String.Format(Properties.Resources.NOT_SPECIFY_ENCODE, "UTF-8");
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "_sjis"; row["SHOW"] = String.Format(Properties.Resources.NOT_SPECIFY_ENCODE, "Shift_JIS");
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "utf8"; row["SHOW"] = "UTF-8";
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "sjis"; row["SHOW"] = "Shift_JIS";
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "euc"; row["SHOW"] = "EUC-JP";
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "jis"; row["SHOW"] = "JIS";
            EncodeComboboxData.Rows.Add(row);

            EncodeComboboxData.AcceptChanges();

            encodeComboBox.DataSource = EncodeComboboxData;
            encodeComboBox.DisplayMember = "SHOW";
            encodeComboBox.ValueMember = "DATA";

            LanguageComboboxData.Columns.Add("DATA", typeof(string));
            LanguageComboboxData.Columns.Add("SHOW", typeof(string));

            var cultures =  System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            bool en = false, ja = false;
            foreach (var c in cultures) {
                if (c.Name == "en-US") en = true;
                if (c.Name == "ja-JP") ja = true;
            }

            row = LanguageComboboxData.NewRow();
            row["DATA"] = ""; row["SHOW"] = "System default";
            LanguageComboboxData.Rows.Add(row);
            if (en) {
                row = LanguageComboboxData.NewRow();
                row["DATA"] = "en-US"; row["SHOW"] = "English";
                LanguageComboboxData.Rows.Add(row);
            }
            if (ja) {
                row = LanguageComboboxData.NewRow();
                row["DATA"] = "ja-JP"; row["SHOW"] = "日本語";
                LanguageComboboxData.Rows.Add(row);
            }
            LanguageComboboxData.AcceptChanges();
            languageComboBox.DataSource = LanguageComboboxData;
            languageComboBox.DisplayMember = "SHOW";
            languageComboBox.ValueMember = "DATA";

            var FontColorListViewItemsNames = new string[] { "テキスト", "選択範囲", "コントロールシークエンス", "コメント", "$", "中 / 大括弧", "改行，EOF", "対応する括弧", "空白" };
            for(int i = 0; i < FontColorListView.Items.Count; ++i) {
                FontColorListView.Items[i].Name = FontColorListViewItemsNames[i];
            }


            for (int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                var val = Properties.Settings.Default.editorFontColor[(string)FontColorListView.Items[i].Name];
                FontColorListView.Items[i].ForeColor = val.Font;
                FontColorListView.Items[i].BackColor = val.Back;
            }
            FontColorListView.Items[0].Selected = true;
            for(int i = 1 ; i < FontColorListView.Items.Count ; ++i) {
                FontColorListView.Items[i].Selected = false;
            }
            //            FontColorListView_SelectedIndexChanged();

            platexTextBox.Text = Properties.Settings.Default.platexPath;
            dvipdfmxTextBox.Text = Properties.Settings.Default.dvipdfmxPath;
            gsTextBox.Text = Properties.Settings.Default.gsPath;
            encodeComboBox.SelectedValue = Properties.Settings.Default.encode;
            languageComboBox.SelectedValue = Properties.Settings.Default.language;
            GSUseepswriteCheckButton.Checked = (Properties.Settings.Default.gsDevice == "epswrite");
            UseLowResolutionCheckBox.Checked = Properties.Settings.Default.useLowResolution;
            GuessLaTeXCompileCheckBox.Checked = Properties.Settings.Default.guessLaTeXCompile;
            LaTeXCompileNumbernumUpDown.Value = Properties.Settings.Default.LaTeXCompileMaxNumber;

            resolutionScaleUpDown.Value = Properties.Settings.Default.resolutionScale;
            leftMarginUpDown.Value = Properties.Settings.Default.leftMargin;
            topMarginUpDown.Value = Properties.Settings.Default.topMargin;
            rightMarginUpDown.Value = Properties.Settings.Default.rightMargin;
            bottomMarginUpDown.Value = Properties.Settings.Default.bottomMargin;

            transparentPngCheckBox.Checked = Properties.Settings.Default.transparentPngFlag;
            backgroundColor = Properties.Settings.Default.backgroundColor;
            SetbackgroundColorButtonTextANDColor();
            KeepPageSizeCheckBox.Checked = Properties.Settings.Default.keepPageSize;

            useMagickCheckBox.Checked = Properties.Settings.Default.useMagickFlag;
            notOutllinedTextCheckBox.Checked = !Properties.Settings.Default.outlinedText;
            deleteDisplaySizeCheckBox.Checked = Properties.Settings.Default.deleteDisplaySize;
            MergeOutputFilesCheckBox.Checked = Properties.Settings.Default.mergeOutputFiles;
            animationDelayNumericUpDown.Value = Properties.Settings.Default.animationDelay / 100m;
            animationLoopNumericUpDown.Value = Properties.Settings.Default.animationLoop;

            showOutputWindowCheckBox.Checked = Properties.Settings.Default.showOutputWindowFlag;
            previewCheckBox.Checked = Properties.Settings.Default.previewFlag;
            deleteTmpFilesCheckBox.Checked = Properties.Settings.Default.deleteTmpFileFlag;
            ignoreErrorCheckBox.Checked = Properties.Settings.Default.ignoreErrorFlag;
            embedTeXSourCecheckBox.Checked = Properties.Settings.Default.embedTeXSource;
            setFileToClipboardCheckBox.Checked = Properties.Settings.Default.setFileToClipBoard;
            workDir_FileDirRadioButton.Checked = Properties.Settings.Default.workingDirectory == "file";
            workDir_TempDirRadioButton.Checked = Properties.Settings.Default.workingDirectory == "tmp";
            workDir_CurrentRadioButton.Checked = Properties.Settings.Default.workingDirectory == "current";

            drawEOFCheckBox.Checked = Properties.Settings.Default.editorDrawEOF;
            drawEOLCheckBox.Checked = Properties.Settings.Default.editorDrawEOL;
            drawSpaceCheckBox.Checked = Properties.Settings.Default.editorDrawSpace;
            drawFullWidthSpaceCheckBox.Checked = Properties.Settings.Default.editorDrawFullWidthSpace;
            drawTabCheckBox.Checked = Properties.Settings.Default.editorDrawTab;
            acceptTabCheckBox.Checked = Properties.Settings.Default.editorAcceptTab;
            tabWidthNumericUpDown.Value = Properties.Settings.Default.editorTabWidth;

            SettingTab.SelectedIndex = Properties.Settings.Default.settingTabIndex;

            radioButtonbp.Checked = Properties.Settings.Default.yohakuUnitBP;
            radioButtonpx.Checked = !Properties.Settings.Default.yohakuUnitBP;

            FontDataText.Text = GetFontString(Properties.Settings.Default.editorFont);
        }

        private void platexBrowseButton_Click(object sender, EventArgs e) {
            if(platexOpenFileDialog.ShowDialog() == DialogResult.OK) {
                platexTextBox.Text = platexOpenFileDialog.FileName;
            }
        }

        private void dvipdfmxBrowseButton_Click(object sender, EventArgs e) {
            if(dvipdfmxOpenFileDialog.ShowDialog() == DialogResult.OK) {
                dvipdfmxTextBox.Text = dvipdfmxOpenFileDialog.FileName;
            }
        }

        private void gsBrowseButton_Click(object sender, EventArgs e) {
            if(gsOpenFileDialog.ShowDialog() == DialogResult.OK) {
                gsTextBox.Text = gsOpenFileDialog.FileName;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Properties.Settings.Default.Reload();
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e) {
            Properties.Settings.Default.platexPath = platexTextBox.Text;
            Properties.Settings.Default.dvipdfmxPath = dvipdfmxTextBox.Text;
            Properties.Settings.Default.gsPath = gsTextBox.Text;
            Properties.Settings.Default.gsDevice = GSUseepswriteCheckButton.Checked ? "epswrite" : "eps2write";
            Properties.Settings.Default.useLowResolution = UseLowResolutionCheckBox.Checked;
            Properties.Settings.Default.encode = (string) encodeComboBox.SelectedValue;
            Properties.Settings.Default.LaTeXCompileMaxNumber = (int) LaTeXCompileNumbernumUpDown.Value;
            Properties.Settings.Default.guessLaTeXCompile = GuessLaTeXCompileCheckBox.Checked;
            
            Properties.Settings.Default.resolutionScale = (int) (resolutionScaleUpDown.Value);
            Properties.Settings.Default.leftMargin = leftMarginUpDown.Value;
            Properties.Settings.Default.topMargin = topMarginUpDown.Value;
            Properties.Settings.Default.rightMargin = rightMarginUpDown.Value;
            Properties.Settings.Default.bottomMargin = bottomMarginUpDown.Value;

            Properties.Settings.Default.backgroundColor = backgroundColor;
            Properties.Settings.Default.transparentPngFlag = transparentPngCheckBox.Checked;
            Properties.Settings.Default.keepPageSize = KeepPageSizeCheckBox.Checked;

            Properties.Settings.Default.useMagickFlag = useMagickCheckBox.Checked;
            Properties.Settings.Default.outlinedText = !notOutllinedTextCheckBox.Checked;
            Properties.Settings.Default.deleteDisplaySize = deleteDisplaySizeCheckBox.Checked;
            Properties.Settings.Default.mergeOutputFiles = MergeOutputFilesCheckBox.Checked;
            Properties.Settings.Default.animationDelay = (uint)(animationDelayNumericUpDown.Value * 100m);
            Properties.Settings.Default.animationLoop = (uint)animationLoopNumericUpDown.Value;

            Properties.Settings.Default.showOutputWindowFlag = showOutputWindowCheckBox.Checked;
            Properties.Settings.Default.previewFlag = previewCheckBox.Checked;
            Properties.Settings.Default.deleteTmpFileFlag = deleteTmpFilesCheckBox.Checked;
            Properties.Settings.Default.ignoreErrorFlag = ignoreErrorCheckBox.Checked;
            Properties.Settings.Default.embedTeXSource = embedTeXSourCecheckBox.Checked;
            Properties.Settings.Default.setFileToClipBoard = setFileToClipboardCheckBox.Checked;
            if (workDir_FileDirRadioButton.Checked) Properties.Settings.Default.workingDirectory = "file";
            else if (workDir_CurrentRadioButton.Checked) Properties.Settings.Default.workingDirectory = "current";
            else Properties.Settings.Default.workingDirectory = "tmp";

            Properties.Settings.Default.settingTabIndex = SettingTab.SelectedIndex;

            Properties.Settings.Default.yohakuUnitBP = radioButtonbp.Checked;

            Properties.Settings.Default.editorDrawEOF = drawEOFCheckBox.Checked;
            Properties.Settings.Default.editorDrawEOL = drawEOLCheckBox.Checked;
            Properties.Settings.Default.editorDrawSpace = drawSpaceCheckBox.Checked;
            Properties.Settings.Default.editorDrawFullWidthSpace = drawFullWidthSpaceCheckBox.Checked;
            Properties.Settings.Default.editorDrawTab = drawTabCheckBox.Checked;
            Properties.Settings.Default.editorAcceptTab = acceptTabCheckBox.Checked;
            Properties.Settings.Default.editorTabWidth = (int) tabWidthNumericUpDown.Value;

            if (Properties.Settings.Default.language != (string)languageComboBox.SelectedValue) {
                Properties.Settings.Default.language = (string)languageComboBox.SelectedValue;
                if(Properties.Settings.Default.language == "ja-JP" ||
                    (Properties.Settings.Default.language == "" && Properties.Settings.SystemDefaultCaltureInfo.Name == "ja-JP")) {
                    MessageBox.Show("言語の変更は TeX2img の再起動後に反映されます。");
                }else {
                    MessageBox.Show("For the change of the language, close TeX2img and execute again.");
                }
            }


            Properties.Settings.Default.Save();

            this.Close();
        }

        private void openTmpFolderButton_Click(object sender, EventArgs e) {
            try { Process.Start(Path.GetTempPath()); }
            catch(Exception) {
                MessageBox.Show(String.Format(Properties.Resources.CANNOT_OPEN_WORKDIR, Path.GetTempPath()));
            }
        }

        private void ChangeFontButton_Click(object sender, EventArgs e) {
            FontDialog fd = new FontDialog();
            fd.Font = Properties.Settings.Default.editorFont;
            fd.ShowEffects = false;
            if(fd.ShowDialog() != DialogResult.Cancel) {
                Properties.Settings.Default.editorFont = fd.Font;
                FontDataText.Text = GetFontString(Properties.Settings.Default.editorFont);
            }
        }

        string GetFontString(Font f) {
            int fontsize = (int) f.Size;
            if(f.Size - fontsize > 0.5) ++fontsize;
            return f.Name + " " + fontsize + " pt" + (f.Bold ? " " + Properties.Resources.BOLD : "") + (f.Italic ? " " + Properties.Resources.ITALIC : "");
        }

        private void FontColorListView_SelectedIndexChanged(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Name;
            FontColorGroup.Text = FontColorListView.SelectedItems[0].Text;
            FontColorButton.BackColor = Properties.Settings.Default.editorFontColor[item].Font;
            BackColorButton.BackColor = Properties.Settings.Default.editorFontColor[item].Back;
            if(item == "改行，EOF" || item == "空白") BackColorButton.Enabled = false;
            else BackColorButton.Enabled = true;
        }

        private void FontColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Name;
            using(ColorDialog cd = new ColorDialog()) {
                cd.CustomColors = (int[]) Properties.Settings.Default.ColorDialogCustomColors.Clone();
                cd.Color = Properties.Settings.Default.editorFontColor[item].Font;
                if(cd.ShowDialog() == DialogResult.OK) {
                    Properties.Settings.Default.editorFontColor[item].Font = cd.Color;
                    FontColorButton.BackColor = cd.Color;
                    FontColorListView.SelectedItems[0].ForeColor = cd.Color;
                    cd.CustomColors.CopyTo(Properties.Settings.Default.ColorDialogCustomColors, 0);
                }
            }
        }

        private void BackColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Name;
            using(ColorDialog cd = new ColorDialog()) {
                cd.Color = Properties.Settings.Default.editorFontColor[item].Back;
                cd.CustomColors = (int[]) Properties.Settings.Default.ColorDialogCustomColors.Clone();
                if(cd.ShowDialog() == DialogResult.OK) {
                    Properties.Settings.Default.editorFontColor[item].Back = cd.Color;
                    BackColorButton.BackColor = cd.Color;
                    FontColorListView.SelectedItems[0].BackColor = cd.Color;
                    if(item == "テキスト") {
                        for(int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                            if(FontColorListView.Items[i].Name == "改行，EOF" || FontColorListView.Items[i].Name == "空白") {
                                FontColorListView.Items[i].BackColor = cd.Color;
                            }
                        }
                    }
                    cd.CustomColors.CopyTo(Properties.Settings.Default.ColorDialogCustomColors, 0);
                }
            }
        }

        private void GuessPathButton_Click(object sender, EventArgs e) {
            string platex = Properties.Settings.Default.GuessPlatexPath();
            platexTextBox.Text = platex;
            var dvipdfmx = Properties.Settings.Default.GuessDvipdfmxPath();
            dvipdfmxTextBox.Text = dvipdfmx;
            string gs = Properties.Settings.Default.GuessGsPath(platex);
            gsTextBox.Text = gs;
            string gsdevice = Properties.Settings.Default.GuessGsdevice(gs);
            if(gsdevice != "") GSUseepswriteCheckButton.Checked = (gsdevice == "epswrite");
            var errs = new List<string>();
            if(platex == "") errs.Add("latex");
            if(dvipdfmx == "") errs.Add("DVI driver");
            if(gs == "") errs.Add("Ghostscript");
            var err = String.Join(", ", errs.ToArray());
            if (err != "") MessageBox.Show(String.Format(Properties.Resources.FAIL_GUESS, err), "TeX2img");
        }

        private void AdvancedGuess_Click(object sender, EventArgs e) {
            var menu = new ContextMenuStrip();
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                menu.Items.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            menu.ItemClicked += ((ss, ee) => {
                var tag = (string) ee.ClickedItem.Tag;
                var preamble = Properties.Settings.Default.preambleTemplates[tag];
                string platex = Properties.Settings.Default.GuessPlatexPath(preamble);
                platexTextBox.Text = platex;
                dvipdfmxTextBox.Text = Properties.Settings.Default.GuessDvipdfmxPath(preamble);
                string gs = Properties.Settings.Default.GuessGsPath(platex);
                gsTextBox.Text = gs;
                string gsdevice = Properties.Settings.Default.GuessGsdevice(gs);
                if(gsdevice != "") GSUseepswriteCheckButton.Checked = (gsdevice == "epswrite");
            });
            var btn = (Button) sender;
            menu.Show(btn, new Point(btn.Width, 0));
        }

        private void backgroundColorButton_Click(object sender, EventArgs e) {
            using (var cdg = new ColorDialog()) {
                cdg.CustomColors = (int[])Properties.Settings.Default.ColorDialogCustomColors.Clone();
                cdg.AnyColor = true;
                cdg.Color = backgroundColor;
                if (cdg.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    backgroundColor = cdg.Color;
                    SetbackgroundColorButtonTextANDColor();
                }
            }
        }

        Color backgroundColor;
        private void SetbackgroundColorButtonTextANDColor() {
            //backgroundColorButton.Enabled = !transparentPngCheckBox.Checked;
            if (transparentPngCheckBox.Checked) {
                backgroundColorButton.Text = Properties.Resources.TRANSPARENT;
                backgroundColorButton.BackColor = Color.White;
            } else {
                backgroundColorButton.Text = "";
                backgroundColorButton.BackColor = backgroundColor;
            }
        }

        private void transparentPngCheckBox_CheckedChanged(object sender, EventArgs e) {
            SetbackgroundColorButtonTextANDColor();
        }
    }
}
