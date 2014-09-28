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
        // 設定データ
        public class Settings {
            public string PlatexPath { get; set; }
            public string DvipdfmxPath { get; set; }
            public string GsPath { get; set; }
            public int ResolutionScale { get; set; }
            public bool UseMagickFlag { get; set; }
            public bool TransparentPngFlag { get; set; }
            public bool ShowOutputWindowFlag { get; set; }
            public bool PreviewFlag { get; set; }
            public bool DeleteTmpFileFlag { get; set; }
            public bool IgnoreErrorFlag { get; set; }
            public decimal TopMargin { get; set; }
            public decimal BottomMargin { get; set; }
            public decimal LeftMargin { get; set; }
            public decimal RightMargin { get; set; }
            public int SettingTabIndex { get; set; }
            public bool YohakuUnitBP { get; set; }
            public Font EditorFont { get; set; }

            public class FontColor : ICloneable{
                public Color Font, Back;
                public FontColor() { }
                public FontColor(Color f, Color b) { Font = f; Back = b; }
                public Object Clone() { return MemberwiseClone(); }
            }

            public class FontColorCollection : Dictionary<string, FontColor> {
                public new FontColor this[string key] {
                    get {
                        if(key == "改行，EOF") base["改行，EOF"].Back = base["テキスト"].Back;
                        return base[key];
                    }
                    set {
                        base[key] = value;
                        if(key == "改行，EOF")base["改行，EOF"].Back = base["テキスト"].Back;
                    }
                }

            }

            public FontColorCollection EditorFontColor { get; set; }

            // 文字コードを表す utf8,sjis,jis,euc
            // _utf8, _sjisは文字コードを推定に任せ，それぞれ入力されたソースをUTF-8/Shift_JISで扱う
            public string Encode { get; set; }

            public Settings() {
                TopMargin = 0;
                BottomMargin = 0;
                LeftMargin = 0;
                RightMargin = 0;
                ShowOutputWindowFlag = true;
                PreviewFlag = true;
                DeleteTmpFileFlag = true;
                IgnoreErrorFlag = false;
                SettingTabIndex = 0;
                YohakuUnitBP = false;
                Encode = "_sjis";

                EditorFontColor = new FontColorCollection();
            }

            protected Settings(Settings s) {
                PlatexPath = (string)s.PlatexPath.Clone();
                DvipdfmxPath = (string)s.DvipdfmxPath.Clone();
                GsPath = (string)s.GsPath.Clone();
                ResolutionScale = s.ResolutionScale;
                UseMagickFlag = s.UseMagickFlag;
                TransparentPngFlag = s.TransparentPngFlag;
                ShowOutputWindowFlag = s.ShowOutputWindowFlag;
                PreviewFlag = s.PreviewFlag;
                DeleteTmpFileFlag = s.DeleteTmpFileFlag;
                IgnoreErrorFlag = s.IgnoreErrorFlag;
                TopMargin = s.TopMargin;
                BottomMargin = s.BottomMargin;
                LeftMargin = s.LeftMargin;
                RightMargin = s.RightMargin;
                SettingTabIndex = s.SettingTabIndex;
                YohakuUnitBP = s.YohakuUnitBP;
                EditorFont = (Font)s.EditorFont.Clone();
                EditorFontColor = new FontColorCollection() ;
                foreach(var item in s.EditorFontColor) {
                    EditorFontColor[item.Key] = (FontColor)item.Value.Clone();
                }
                Encode = s.Encode;
            }

            public Settings DeepCopy() { return new Settings(this); }
            
            public void LoadSetting() {
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

                ShowOutputWindowFlag = Properties.Settings.Default.showOutputWindowFlag;
                PreviewFlag = Properties.Settings.Default.previewFlag;
                DeleteTmpFileFlag = Properties.Settings.Default.deleteTmpFileFlag;
                IgnoreErrorFlag = Properties.Settings.Default.ignoreErrorFlag;

                SettingTabIndex = Properties.Settings.Default.settingTabIndex;

                EditorFontColor["テキスト"] = new FontColor(Properties.Settings.Default.editorNormalColorFont, Properties.Settings.Default.editorNormalColorBack);
                EditorFontColor["選択範囲"] = new FontColor(Properties.Settings.Default.editorSelectedColorFont,Properties.Settings.Default.editorSelectedColorBack);
                EditorFontColor["コントロールシークエンス"] = new FontColor(Properties.Settings.Default.editorCommandColorFont,Properties.Settings.Default.editorCommandColorBack);
                EditorFontColor["$"] = new FontColor(Properties.Settings.Default.editorEquationColorFont,Properties.Settings.Default.editorEquationColorBack);
                EditorFontColor["中 / 大括弧"] = new FontColor(Properties.Settings.Default.editorBracketColorFont,Properties.Settings.Default.editorBracketColorBack);
                EditorFontColor["コメント"] = new FontColor(Properties.Settings.Default.editorCommentColorFont,Properties.Settings.Default.editorCommentColorBack);
                EditorFontColor["改行，EOF"] = new FontColor(Properties.Settings.Default.editorEOFColorFont,Properties.Settings.Default.editorNormalColorBack);
                EditorFontColor["対応する括弧"] = new FontColor(Properties.Settings.Default.editorMatchedBracketColorFont,Properties.Settings.Default.editorMatchedBracketColorBack);
                EditorFont = Properties.Settings.Default.editorFont;
            }

            public void SaveSettings() {
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
                Properties.Settings.Default.showOutputWindowFlag = ShowOutputWindowFlag;
                Properties.Settings.Default.previewFlag = PreviewFlag;
                Properties.Settings.Default.deleteTmpFileFlag = DeleteTmpFileFlag;
                Properties.Settings.Default.ignoreErrorFlag = IgnoreErrorFlag;

                Properties.Settings.Default.editorFont = EditorFont;
                Properties.Settings.Default.settingTabIndex = SettingTabIndex;
                Properties.Settings.Default.editorNormalColorFont = EditorFontColor["テキスト"].Font;
                Properties.Settings.Default.editorNormalColorBack = EditorFontColor["テキスト"].Back;
                Properties.Settings.Default.editorSelectedColorFont = EditorFontColor["選択範囲"].Font;
                Properties.Settings.Default.editorSelectedColorBack = EditorFontColor["選択範囲"].Back;
                Properties.Settings.Default.editorCommandColorFont = EditorFontColor["コントロールシークエンス"].Font;
                Properties.Settings.Default.editorCommandColorBack = EditorFontColor["コントロールシークエンス"].Back;
                Properties.Settings.Default.editorEquationColorFont = EditorFontColor["$"].Font;
                Properties.Settings.Default.editorEquationColorBack = EditorFontColor["$"].Back;
                Properties.Settings.Default.editorBracketColorFont = EditorFontColor["中 / 大括弧"].Font;
                Properties.Settings.Default.editorBracketColorBack = EditorFontColor["中 / 大括弧"].Back;
                Properties.Settings.Default.editorCommentColorFont = EditorFontColor["コメント"].Font;
                Properties.Settings.Default.editorCommentColorBack = EditorFontColor["コメント"].Back;
                Properties.Settings.Default.editorEOFColorFont = EditorFontColor["改行，EOF"].Font;
                Properties.Settings.Default.editorMatchedBracketColorFont = EditorFontColor["対応する括弧"].Font;
                Properties.Settings.Default.editorMatchedBracketColorBack = EditorFontColor["対応する括弧"].Back;
            }

        }

        Settings SettingData;

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

            SettingData = mainForm.SettingData.DeepCopy();

            for(int i = 0 ; i < FontColorListView.Items.Count ; ++i) {
                Settings.FontColor val = SettingData.EditorFontColor[FontColorListView.Items[i].Text];
                FontColorListView.Items[i].ForeColor = val.Font;
                FontColorListView.Items[i].BackColor = val.Back;
            }
            FontColorListView.Items[0].Selected = true;
            for(int i = 1 ; i < FontColorListView.Items.Count ; ++i) {
                FontColorListView.Items[i].Selected = false;
            }
//            FontColorListView_SelectedIndexChanged();

            platexTextBox.Text = SettingData.PlatexPath;
            dvipdfmxTextBox.Text = SettingData.DvipdfmxPath;
            gsTextBox.Text = SettingData.GsPath;
            encodeComboBox.SelectedValue = SettingData.Encode;

            resolutionScaleUpDown.Value = SettingData.ResolutionScale;
            leftMarginUpDown.Value = SettingData.LeftMargin;
            topMarginUpDown.Value = SettingData.TopMargin;
            rightMarginUpDown.Value = SettingData.RightMargin;
            bottomMarginUpDown.Value = SettingData.BottomMargin;

            useMagickCheckBox.Checked = SettingData.UseMagickFlag;
            transparentPngCheckBox.Checked = SettingData.TransparentPngFlag;

            showOutputWindowCheckBox.Checked = SettingData.ShowOutputWindowFlag;
            previewCheckBox.Checked = SettingData.PreviewFlag;
            deleteTmpFilesCheckBox.Checked = SettingData.DeleteTmpFileFlag;
            ignoreErrorCheckBox.Checked = SettingData.IgnoreErrorFlag;

            SettingTab.SelectedIndex = SettingData.SettingTabIndex;

            radioButtonbp.Checked = SettingData.YohakuUnitBP;
            radioButtonpx.Checked = !SettingData.YohakuUnitBP;

            FontDataText.Text = SettingData.EditorFont.Name + " " + SettingData.EditorFont.Size + " pt" +
                (SettingData.EditorFont.Bold ? " 太字" : "") +
                (SettingData.EditorFont.Italic ? " 斜体" : "");
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

            SettingData.PlatexPath = platexTextBox.Text;
            SettingData.DvipdfmxPath = dvipdfmxTextBox.Text;
            SettingData.GsPath = gsTextBox.Text;
            SettingData.Encode = (string)encodeComboBox.SelectedValue;

            SettingData.ResolutionScale = (int) (resolutionScaleUpDown.Value);
            SettingData.LeftMargin = leftMarginUpDown.Value;
            SettingData.TopMargin = topMarginUpDown.Value;
            SettingData.RightMargin = rightMarginUpDown.Value;
            SettingData.BottomMargin = bottomMarginUpDown.Value;

            SettingData.UseMagickFlag = useMagickCheckBox.Checked;
            SettingData.TransparentPngFlag = transparentPngCheckBox.Checked;

            SettingData.ShowOutputWindowFlag = showOutputWindowCheckBox.Checked;
            SettingData.PreviewFlag = previewCheckBox.Checked;
            SettingData.DeleteTmpFileFlag = deleteTmpFilesCheckBox.Checked;
            SettingData.IgnoreErrorFlag = ignoreErrorCheckBox.Checked;

            SettingData.SettingTabIndex = SettingTab.SelectedIndex;

            SettingData.YohakuUnitBP = radioButtonbp.Checked;

            mainForm.SettingData = SettingData.DeepCopy();
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
            fd.Font = SettingData.EditorFont;
            fd.ShowEffects = false;
            if(fd.ShowDialog() != DialogResult.Cancel) {
                SettingData.EditorFont = fd.Font;
                FontDataText.Text = SettingData.EditorFont.Name + " " + SettingData.EditorFont.Size + " pt" +
                    (SettingData.EditorFont.Bold ? " 太字" : "") +
                    (SettingData.EditorFont.Italic ? " 斜体" : "");
            }
        }

        private void FontColorListView_SelectedIndexChanged(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            FontColorGroup.Text = item;
            FontColorButton.BackColor = SettingData.EditorFontColor[item].Font;
            BackColorButton.BackColor = SettingData.EditorFontColor[item].Back;
            if(item == "改行，EOF") BackColorButton.Enabled = false;
            else BackColorButton.Enabled = true;
        }

        private void FontColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = SettingData.EditorFontColor[item].Font;
            if(cd.ShowDialog() == DialogResult.OK) {
                SettingData.EditorFontColor[item].Font = cd.Color;
                FontColorButton.BackColor = cd.Color;
                FontColorListView.SelectedItems[0].ForeColor = cd.Color;
            }
        }

        private void BackColorButton_Click(object sender, EventArgs e) {
            if(FontColorListView.SelectedIndices.Count == 0) return;
            string item = FontColorListView.SelectedItems[0].Text;
            ColorDialog cd = new ColorDialog();
            cd.Color = SettingData.EditorFontColor[item].Back;
            if(cd.ShowDialog() == DialogResult.OK) {
                SettingData.EditorFontColor[item].Back = cd.Color;
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

    }
}
