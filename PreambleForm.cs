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
            const string newAddItemStr = "（新規）...";
            const string addItemStr = "現在のプリアンブルを保存";
            const string deleteItemStr = "削除";
            var menu = new ContextMenuStrip();
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                menu.Items.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            menu.ItemClicked += ((ss, ee) => {
                var tag = (string) ee.ClickedItem.Tag;
                try {
                    var text = Properties.Settings.Default.preambleTemplates[tag];
                    if(MessageBox.Show("以下の内容でプリアンブルを初期化します．よろしいですか？\n" + text, "TeX2mg", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                        preambleTextBox.Text = text;
                    }
                }
                catch(KeyNotFoundException) { }
                catch(ArgumentNullException) { }
            });

            var delitem = new ToolStripMenuItem(deleteItemStr);
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                delitem.DropDownItems.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            delitem.DropDownItemClicked += ((ss, ee) => {
                var tag = (string) ee.ClickedItem.Tag;
                try {
                    if(MessageBox.Show(tag + " を削除します．よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                        Properties.Settings.Default.preambleTemplates.Remove(tag);
                    }
                }
                catch(KeyNotFoundException) { }
            });
            menu.Items.Add(delitem);

            var additem = new ToolStripMenuItem(addItemStr);
            additem.DropDownItems.Add(new ToolStripMenuItem(newAddItemStr) { Tag = newAddItemStr });
            foreach(var d in Properties.Settings.Default.preambleTemplates) {
                additem.DropDownItems.Add(new ToolStripMenuItem(d.Key) { Tag = d.Key });
            }
            additem.DropDownItemClicked += ((ss, ee) => {
                var tag = (string) ee.ClickedItem.Tag;
                if(tag == newAddItemStr) {
                    var inputdlg = new InputDialog("テンプレート名", "テンプレート名を入力してください");
                    if(inputdlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK) {
                        var template = inputdlg.InputedText;
                        if(template == addItemStr || template == deleteItemStr || template == newAddItemStr) {
                            MessageBox.Show(template + " はテンプレート名に使えません．");
                        } else {
                            if(Properties.Settings.Default.preambleTemplates.ContainsKey(template)) {
                                if(MessageBox.Show(template + " はすでに存在します．上書きしますか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)return;
                            }
                            Properties.Settings.Default.preambleTemplates[template] = preambleTextBox.Text;
                        }
                    }
                } else {
                    try { Properties.Settings.Default.preambleTemplates[tag] = preambleTextBox.Text; }
                    catch(KeyNotFoundException) { }
                }
            });
            menu.Items.Add(additem);

            var btn = (Button) sender;
            menu.Show(btn, new Point(btn.Width, 0));
        }
    }
}