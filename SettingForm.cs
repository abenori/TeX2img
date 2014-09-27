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
        MainForm mainForm;
        private Font EditorFont;
        Dictionary<string, MainForm.FontColor> EditorFontColor = new Dictionary<string,MainForm.FontColor>();

        DataTable EncodeComboboxData = new DataTable();
        public SettingForm(MainForm _mainForm) {
            mainForm = _mainForm;
            InitializeComponent();

            DataRow row;
            EncodeComboboxData.Columns.Add("DATA",typeof(string));
            EncodeComboboxData.Columns.Add("SHOW",typeof(string));
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

            encodeComboBox.SelectedValue = mainForm.Encode;

            EditorFontColor["テキスト"] = mainForm.EditorNormalFontColor;
            EditorFontColor["選択範囲"] = mainForm.EditorSelectedFontColor;
            EditorFontColor["コントロールシークエンス"] = mainForm.EditorCommandFontColor;
            EditorFontColor["$"] = mainForm.EditorEquationFontColor;
            EditorFontColor["中 / 大括弧"] = mainForm.EditorBracketFontColor;
            EditorFontColor["コメント"] = mainForm.EditorCommentFontColor;
            EditorFontColor["改行，EOF"] = mainForm.EditorEOFFontColor;

            for(int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                MainForm.FontColor val = EditorFontColor[FontColorListView.Items[i].Text];
                FontColorListView.Items[i].ForeColor = val.Font;
                FontColorListView.Items[i].BackColor = val.Back;
            }
            FontColorListView.Items[0].Selected = true;
            for(int i = 1 ; i < FontColorListView.Items.Count ; ++i) {
                FontColorListView.Items[i].Selected = false;
            }
//            FontColorListView_SelectedIndexChanged();

            platexTextBox.Text = mainForm.PlatexPath;
            dvipdfmxTextBox.Text = mainForm.DvipdfmxPath;
            gsTextBox.Text = mainForm.GsPath;

            resolutionScaleUpDown.Value = mainForm.ResolutionScale;
            leftMarginUpDown.Value = mainForm.LeftMargin;
            topMarginUpDown.Value = mainForm.TopMargin;
            rightMarginUpDown.Value = mainForm.RightMargin;
            bottomMarginUpDown.Value = mainForm.BottomMargin;

            useMagickCheckBox.Checked = mainForm.UseMagickFlag;
            transparentPngCheckBox.Checked = mainForm.TransparentPngFlag;

            showOutputWindowCheckBox.Checked = mainForm.ShowOutputWindowFlag;
            previewCheckBox.Checked = mainForm.PreviewFlag;
            deleteTmpFilesCheckBox.Checked = mainForm.DeleteTmpFileFlag;
            ignoreErrorCheckBox.Checked = mainForm.IgnoreErrorFlag;

            SettingTab.SelectedIndex = mainForm.SettingTabIndex;

            radioButtonbp.Checked = mainForm.YohakuUnitBP;
            radioButtonpx.Checked = !mainForm.YohakuUnitBP;

            EditorFont = mainForm.EditorFont;
            FontDataText.Text = EditorFont.Name + " " + EditorFont.Size + " pt" +
                (EditorFont.Bold ? " 太字" : "") +
                (EditorFont.Italic ? " 斜体" : "");
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
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e) {
            string dummy;
            if(!File.Exists(Converter.setProcStartInfo(platexTextBox.Text, out dummy))) {
                showNotFoundError(Converter.setProcStartInfo(platexTextBox.Text, out dummy));
                return;
            }
            if(!File.Exists(Converter.setProcStartInfo(dvipdfmxTextBox.Text, out dummy))) {
                showNotFoundError(Converter.setProcStartInfo(dvipdfmxTextBox.Text, out dummy));
                return;
            }
            if(!File.Exists(Converter.setProcStartInfo(gsTextBox.Text, out dummy))) {
                showNotFoundError(Converter.setProcStartInfo(gsTextBox.Text, out dummy));
                return;
            }

            mainForm.PlatexPath = platexTextBox.Text;
            mainForm.DvipdfmxPath = dvipdfmxTextBox.Text;
            mainForm.GsPath = gsTextBox.Text;
            mainForm.Encode = (string)encodeComboBox.SelectedValue;

            mainForm.ResolutionScale = (int) (resolutionScaleUpDown.Value);
            mainForm.LeftMargin = leftMarginUpDown.Value;
            mainForm.TopMargin = topMarginUpDown.Value;
            mainForm.RightMargin = rightMarginUpDown.Value;
            mainForm.BottomMargin = bottomMarginUpDown.Value;

            mainForm.UseMagickFlag = useMagickCheckBox.Checked;
            mainForm.TransparentPngFlag = transparentPngCheckBox.Checked;

            mainForm.ShowOutputWindowFlag = showOutputWindowCheckBox.Checked;
            mainForm.PreviewFlag = previewCheckBox.Checked;
            mainForm.DeleteTmpFileFlag = deleteTmpFilesCheckBox.Checked;
            mainForm.IgnoreErrorFlag = ignoreErrorCheckBox.Checked;

            mainForm.SettingTabIndex = SettingTab.SelectedIndex;

            mainForm.YohakuUnitBP = radioButtonbp.Checked;

            mainForm.EditorFont = EditorFont;

            mainForm.EditorNormalFontColor = EditorFontColor["テキスト"];
            mainForm.EditorSelectedFontColor = EditorFontColor["選択範囲"];
            mainForm.EditorCommandFontColor = EditorFontColor["コントロールシークエンス"];
            mainForm.EditorEquationFontColor = EditorFontColor["$"];
            mainForm.EditorBracketFontColor = EditorFontColor["中 / 大括弧"];
            mainForm.EditorCommentFontColor = EditorFontColor["コメント"];
            mainForm.EditorEOFFontColor = EditorFontColor["改行，EOF"];

            mainForm.ChangeSetting();
            this.Close();
        }

        private void showNotFoundError(string filePath) {
            MessageBox.Show(filePath + " が存在しません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void openTmpFolderButton_Click(object sender, EventArgs e) {
            Process.Start(Path.GetTempPath());
        }

        private void imageMagickLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("http://imagemagick.org/script/binary-releases.php#windows");
        }

        private void ChangeFontButton_Click(object sender, EventArgs e) {
            FontDialog fd = new FontDialog();
            fd.Font = EditorFont;
            fd.ShowEffects = false;
            if(fd.ShowDialog() != DialogResult.Cancel) {
                EditorFont = fd.Font;
                FontDataText.Text = EditorFont.Name + " " + EditorFont.Size + " pt" + 
                    (EditorFont.Bold ? " 太字" : "") +
                    (EditorFont.Italic ? " 斜体" : "");
            }
        }

        private void FontColorListView_SelectedIndexChanged(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            FontColorGroup.Text = item;
            FontColorButton.BackColor = EditorFontColor[item].Font;
            BackColorButton.BackColor = EditorFontColor[item].Back;
            if(item == "改行，EOF") BackColorButton.Enabled = false;
            else BackColorButton.Enabled = true;
        }

        private void FontColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = EditorFontColor[item].Font;
            if(cd.ShowDialog() == DialogResult.OK) {
                EditorFontColor[item].Font = cd.Color;
                FontColorButton.BackColor = cd.Color;
                FontColorListView.SelectedItems[0].ForeColor = cd.Color;
            }
        }

        private void BackColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = EditorFontColor[item].Back;
            if(cd.ShowDialog() == DialogResult.OK) {
                EditorFontColor[item].Back = cd.Color;
                BackColorButton.BackColor = cd.Color;
                FontColorListView.SelectedItems[0].BackColor = cd.Color;
            }
        }

    }
}
