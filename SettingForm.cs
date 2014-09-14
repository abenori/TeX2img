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
        public SettingForm(MainForm _mainForm) {
            mainForm = _mainForm;
            InitializeComponent();

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

    }
}
