using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
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
            preambleTextBox.Document.WordProc.EnableWordWrap = false;
            preambleTextBox.Document.EolCode = "\n";
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
            const string addItemStr = "現在のプリアンブルを保存...";
            const string manageItemStr = "テンプレートの管理...";
            var menu = new ContextMenuStrip();
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                menu.Items.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem(addItemStr) { Tag = addItemStr });
            menu.Items.Add(new ToolStripMenuItem(manageItemStr) { Tag = manageItemStr });
            menu.ItemClicked += ((ss, ee) => {
                menu.Close();
                var tag = (string) ee.ClickedItem.Tag;
                if(tag == manageItemStr) {
                    var managedlg = new ManageTemplateDialog(Properties.Settings.Default.preambleTemplates, preambleTextBox.Text, new List<string> { manageItemStr, addItemStr ,""});
                    if(managedlg.ShowDialog(mainForm) == System.Windows.Forms.DialogResult.OK) {
                        Properties.Settings.Default.preambleTemplates = managedlg.Templates;
                    }
                }else if(tag == addItemStr){
                    var input = new InputComboDialog(
                        "テンプレートの追加","テンプレート名を入力してください",
                        Properties.Settings.Default.preambleTemplates.Select(d => d.Key).ToList());
                    input.OKButtonClicked += ((sss, eee) => {
                        if(eee.InputedText == manageItemStr || eee.InputedText == addItemStr || eee.InputedText == "") {
                            MessageBox.Show("[" + eee.InputedText + "] はテンプレート名には使えません．", "TeX2img");
                            eee.Cancel = true;
                        } else if(Properties.Settings.Default.preambleTemplates.ContainsKey(eee.InputedText)) {
                            if(MessageBox.Show(eee.InputedText + " に上書きしますか？", "TeX2img", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes) {
                                eee.Cancel = true;
                            }
                        }
                    });
                    if(input.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                        Properties.Settings.Default.preambleTemplates[input.InputedText] = preambleTextBox.Text;
                    }
                } else {
                    try {
                        var text = Properties.Settings.Default.preambleTemplates[tag];
                        if(text != null) {
                            if(MessageBox.Show("現在のプリアンブルを以下の内容に変更します．よろしいですか？\n" + text, "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                                preambleTextBox.Text = text;
                            }
                        }
                    }
                    catch(KeyNotFoundException) { }
                    catch(ArgumentNullException) { }
                }
            });

            var btn = (Button) sender;
            menu.Show(btn, new Point(0, btn.Height));
        }
    }
}