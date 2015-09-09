using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeX2img {
    public partial class ManageTemplateDialog : Form {
        public Dictionary<string, string> Templates { get; private set; }
        string currentPreamble;
        List<string> invalidTemplateNames;
        public ManageTemplateDialog(Dictionary<string,string> temp,string preamble,List<string> invalidTempNames) {
            InitializeComponent();
            Templates = new Dictionary<string,string>();
            foreach(var d in temp) {
                Templates.Add(d.Key, d.Value);
                TemplateListBox.Items.Add(d.Key);
            }
            TemplateListBox.SelectedIndex = 0;
            currentPreamble = preamble;
            invalidTemplateNames = invalidTempNames;
        }

        private void DeleteButton_Click(object sender, EventArgs e) {
            var temp = (string) TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show("削除するテンプレートを選択してください。");
            } else {
                if(MessageBox.Show(temp + " を削除します。よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    TemplateListBox.Items.Remove(TemplateListBox.SelectedItem);
                    Templates.Remove(temp);
                }
            }
        }

        private void ShowButton_Click(object sender, EventArgs e) {
            var temp = (string) TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show("表示するテンプレートを選択してください。");
            } else {
                MessageBox.Show(temp + " の中身は次の通りです。\n\n" + Templates[temp], "TeX2img");
            }
        }

        private void OKButton_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void Cancel_Button_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void BackToDefaultButton_Click(object sender, EventArgs e) {
            var temp = Properties.Settings.GetDefaultTemplate();
            string tempnames = String.Join(", ", temp.Select(d => d.Key).ToArray());
            var menu = new ContextMenuStrip();
            var setdefault = new ToolStripMenuItem("初期テンプレート内容を復元");
            setdefault.Click += ((ss, ee) => {
                if(MessageBox.Show(tempnames + "\nの中身を元に戻します。よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    foreach(var d in temp) {
                        Templates[d.Key] = d.Value;
                    }
                    TemplateListBox.Items.Clear();
                    foreach(var d in Templates) {
                        TemplateListBox.Items.Add(d.Key);
                    }
                }
            });
            menu.Items.Add(setdefault);
            var setdefaultdelothers = new ToolStripMenuItem("一覧の初期化");
            setdefaultdelothers.Click += ((ss, ee) => {
                if(MessageBox.Show(tempnames + "\nの中身を元に戻し，それ以外を削除します。よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    Templates = temp;
                    TemplateListBox.Items.Clear();
                    foreach(var d in Templates) {
                        TemplateListBox.Items.Add(d.Key);
                    }
                }
            });
            menu.Items.Add(setdefaultdelothers);

            var btn = (Button) sender;
            menu.Show(btn, new Point(btn.Width, 0));

        }

        private void AddNewButton_Click(object sender, EventArgs e) {
            Save(sender, e);
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            Save(sender, e);
        }

        private void Save(object sender, EventArgs e) {
            var menu = new ContextMenuStrip();
            var fromPreamble = new ToolStripMenuItem("現在のプリアンブルから...");
            
            fromPreamble.Click += ((ss, ee) => {
                string templatename = null;
                if(sender == AddNewButton) templatename = GetNewTemplateName();
                else templatename = GetSelectedTemplateName();
                if(templatename != null) {
                    if(!Templates.ContainsKey(templatename)) TemplateListBox.Items.Add(templatename);
                    Templates[templatename] = currentPreamble;
                }
            });
            menu.Items.Add(fromPreamble);
            var fromFile = new ToolStripMenuItem("ファイルから...");
            fromFile.Click += ((ss, ee) => {
                var ofd = new OpenFileDialog();
                ofd.Filter = "TeXソースファイル (*.tex)|*.tex|全てのファイル (*.*)|*.*";
                ofd.Title = "読み込むソースファイルを指定してください。";
                if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                string preamble = null, body = null;
                MainForm.ImportFile(ofd.FileName, out preamble, out body);
                if(preamble == null && body == null) return;
                if(preamble == null) preamble = body;

                string templatename = null;
                if(sender == AddNewButton) templatename = GetNewTemplateName();
                else templatename = GetSelectedTemplateName();
                if(templatename != null) {
                    if(!Templates.ContainsKey(templatename)) TemplateListBox.Items.Add(templatename);
                    Templates[templatename] = preamble;
                }
            });
            menu.Items.Add(fromFile);
            var btn = (Button) sender;
            menu.Show(btn,new Point(btn.Width,0));
        }

        string GetNewTemplateName() {
            var input = new InputComboDialog("TeX2img", "テンプレート名を入力してください。", null);
            input.OKButtonClicked += ((sss, eee) => {
                if(invalidTemplateNames.Contains(eee.InputedText)) {
                    MessageBox.Show("[" + eee.InputedText + "] はテンプレート名には使えません。", "TeX2img");
                    eee.Cancel = true;
                } else if(Templates.ContainsKey(eee.InputedText)) {
                    if(MessageBox.Show(eee.InputedText + " は既に存在します。上書きしてもよいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) {
                        eee.Cancel = true;
                    }
                }
            });
            if(input.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                return input.InputedText;
            } else return null;
        }

        string GetSelectedTemplateName() {
            if(TemplateListBox.SelectedItem == null) {
                MessageBox.Show("テンプレート名を選択してください。", "TeX2img");
            } else {
                if(MessageBox.Show((string) TemplateListBox.SelectedItem + " に現在のプリアンブルを上書きします。よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    return (string) TemplateListBox.SelectedItem;
                }
            }
            return null;
        }
    }
}
