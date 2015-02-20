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
            preambleTextBox.ShowsHScrollBar = false;
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

        private void TemplateButon_Click(object sender, EventArgs e) {
            const string manageItemStr = "テンプレートの管理...";
            var menu = new ContextMenuStrip();
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                menu.Items.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            menu.Items.Add(new ToolStripMenuItem(manageItemStr) { Tag = manageItemStr });
            menu.AutoClose = true;
            menu.ItemClicked += ((ss, ee) => {
                menu.Close();
                var tag = (string) ee.ClickedItem.Tag;
                if(tag == manageItemStr) {
                    var managedlg = new ManageTemplateDialog(Properties.Settings.Default.preambleTemplates,preambleTextBox.Text,new List<string>{manageItemStr});
                    if(managedlg.ShowDialog(mainForm) == System.Windows.Forms.DialogResult.OK) {
                        Properties.Settings.Default.preambleTemplates = managedlg.Templates;
                    }
                } else {
                    try {
                        var text = Properties.Settings.Default.preambleTemplates[tag];
                        if(MessageBox.Show("以下の内容でプリアンブルを初期化します．よろしいですか？\n" + text, "TeX2mg", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                            preambleTextBox.Text = text;
                        }
                    }
                    catch(KeyNotFoundException) { }
                    catch(ArgumentNullException) { }
                }
            });


            var btn = (Button) sender;
            menu.Show(btn, new Point(btn.Width, 0));
        }
    }
}