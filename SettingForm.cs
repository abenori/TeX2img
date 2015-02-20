using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace TeX2img {
    public partial class SettingForm : Form {
        DataTable EncodeComboboxData = new DataTable();
        public SettingForm() {
            InitializeComponent();

            DataRow row;
            EncodeComboboxData.Columns.Add("DATA", typeof(string));
            EncodeComboboxData.Columns.Add("SHOW", typeof(string));
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "_utf8"; row["SHOW"] = "指定しない（入力 UTF-8 ）";
            EncodeComboboxData.Rows.Add(row);
            row = EncodeComboboxData.NewRow();
            row["DATA"] = "_sjis"; row["SHOW"] = "指定しない（入力 Shift_JIS ）";
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

            for(int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                var val = Properties.Settings.Default.editorFontColor[FontColorListView.Items[i].Text];
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
            GSUseepswriteCheckButton.Checked = (Properties.Settings.Default.gsDevice == "epswrite");
            UseLowResolutionCheckBox.Checked = Properties.Settings.Default.useLowResolution;
            GuessLaTeXCompileCheckBox.Checked = Properties.Settings.Default.guessLaTeXCompile;
            LaTeXCompileNumbernumUpDown.Value = Properties.Settings.Default.LaTeXCompileMaxNumber;

            resolutionScaleUpDown.Value = Properties.Settings.Default.resolutionScale;
            leftMarginUpDown.Value = Properties.Settings.Default.leftMargin;
            topMarginUpDown.Value = Properties.Settings.Default.topMargin;
            rightMarginUpDown.Value = Properties.Settings.Default.rightMargin;
            bottomMarginUpDown.Value = Properties.Settings.Default.bottomMargin;

            useMagickCheckBox.Checked = Properties.Settings.Default.useMagickFlag;
            transparentPngCheckBox.Checked = Properties.Settings.Default.transparentPngFlag;

            showOutputWindowCheckBox.Checked = Properties.Settings.Default.showOutputWindowFlag;
            previewCheckBox.Checked = Properties.Settings.Default.previewFlag;
            deleteTmpFilesCheckBox.Checked = Properties.Settings.Default.deleteTmpFileFlag;
            ignoreErrorCheckBox.Checked = Properties.Settings.Default.ignoreErrorFlag;

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

            Properties.Settings.Default.useMagickFlag = useMagickCheckBox.Checked;
            Properties.Settings.Default.transparentPngFlag = transparentPngCheckBox.Checked;

            Properties.Settings.Default.showOutputWindowFlag = showOutputWindowCheckBox.Checked;
            Properties.Settings.Default.previewFlag = previewCheckBox.Checked;
            Properties.Settings.Default.deleteTmpFileFlag = deleteTmpFilesCheckBox.Checked;
            Properties.Settings.Default.ignoreErrorFlag = ignoreErrorCheckBox.Checked;

            Properties.Settings.Default.settingTabIndex = SettingTab.SelectedIndex;

            Properties.Settings.Default.yohakuUnitBP = radioButtonbp.Checked;

            Properties.Settings.Default.Save();

            this.Close();
        }

        private void openTmpFolderButton_Click(object sender, EventArgs e) {
            Process.Start(Path.GetTempPath());
        }

        private void imageMagickLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://imagemagick.org/script/binary-releases.php#windows");
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

            return f.Name + " " + fontsize + " pt" + (f.Bold ? " 太字" : "") + (f.Italic ? " 斜体" : "");
        }

        private void FontColorListView_SelectedIndexChanged(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            FontColorGroup.Text = item;
            FontColorButton.BackColor = Properties.Settings.Default.editorFontColor[item].Font;
            BackColorButton.BackColor = Properties.Settings.Default.editorFontColor[item].Back;
            if(item == "改行，EOF") BackColorButton.Enabled = false;
            else BackColorButton.Enabled = true;
        }

        private void FontColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = Properties.Settings.Default.editorFontColor[item].Font;
            if(cd.ShowDialog() == DialogResult.OK) {
                Properties.Settings.Default.editorFontColor[item].Font = cd.Color;
                FontColorButton.BackColor = cd.Color;
                FontColorListView.SelectedItems[0].ForeColor = cd.Color;
            }
        }

        private void BackColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = Properties.Settings.Default.editorFontColor[item].Back;
            if(cd.ShowDialog() == DialogResult.OK) {
                Properties.Settings.Default.editorFontColor[item].Back = cd.Color;
                BackColorButton.BackColor = cd.Color;
                FontColorListView.SelectedItems[0].BackColor = cd.Color;
                if(item == "テキスト") {
                    for(int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                        if(FontColorListView.Items[i].Text == "改行，EOF") {
                            FontColorListView.Items[i].BackColor = cd.Color;
                        }
                    }
                }
            }
        }

        private void GuessPathButton_Click(object sender, EventArgs e) {
            string platex = Properties.Settings.Default.GuessPlatexPath();
            platexTextBox.Text = platex;
            dvipdfmxTextBox.Text = Properties.Settings.Default.GuessDvipdfmxPath();
            string gs = Properties.Settings.Default.GuessGsPath(platex);
            gsTextBox.Text = gs;
            string gsdevice = Properties.Settings.Default.GuessGsdevice(gs);
            if(gsdevice != "") GSUseepswriteCheckButton.Checked = (gsdevice == "epswrite");
        }

    }
}
