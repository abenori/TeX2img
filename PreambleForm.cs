using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TeX2img {
    public partial class PreambleForm : Form {
        private MainForm mainForm;
        public PreambleForm(MainForm _mainForm) {
            mainForm = _mainForm;
            InitializeComponent();
            if(Properties.Settings.Default.editorFont != null) preambleTextBox.Font = Properties.Settings.Default.editorFont;
            preambleTextBox.Highlighter = Sgry.Azuki.Highlighter.Highlighters.Latex;
            preambleTextBox.Resize += ((s, e) => { preambleTextBox.ViewWidth = preambleTextBox.ClientSize.Width; });
            ActiveControl = preambleTextBox;
        }

        public Sgry.Azuki.WinForms.AzukiControl PreambleTextBox {
            get { return preambleTextBox; }
        }

        public void setLocation(int x, int y) {
            this.Location = new Point(x, y);
        }

        private void PreambleForm_FormClosing(object sender, FormClosingEventArgs e) {
            // ユーザの操作によりウィンドウが閉じられようとしたときはそれをキャンセルし，
            // 単にウィンドウを非表示にする
            if(e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                mainForm.showPreambleWindow(false);
            }
        }

        private void backToDefaultButton_Click(object sender, EventArgs e) {
            if(DialogResult.Yes == MessageBox.Show("現在のプリアンブル設定を破棄して，初期設定に戻してよろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
                string option = "fleqn";
                option += System.IO.Path.GetFileName(Converter.setProcStartInfo(Properties.Settings.Default.platexPath)) == "uplatex.exe" ? ",uplatex" : "";
                preambleTextBox.Text = "\\documentclass[" + option + "]{jsarticle}\r\n\\usepackage{amsmath,amssymb}\r\n\\pagestyle{empty}\r\n";
            }
        }

        private void Undo_Click(object sender, EventArgs e) {
            preambleTextBox.Undo();
        }

        private void Redo_Click(object sender, EventArgs e) {
            preambleTextBox.Redo();
        }

        private void Cut_Click(object sender, EventArgs e) {
            preambleTextBox.Cut();
        }

        private void Copy_Click(object sender, EventArgs e) {
            preambleTextBox.Copy();
        }

        private void Paste_Click(object sender, EventArgs e) {
            preambleTextBox.Paste();
        }

        private void Delete_Click(object sender, EventArgs e) {
            preambleTextBox.Delete();
        }

        private void SelectAll_Click(object sender, EventArgs e) {
            preambleTextBox.SelectAll();
        }
    }
}