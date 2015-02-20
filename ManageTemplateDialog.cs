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

        private void NewButton_Click(object sender, EventArgs e) {
            var input = new InputDialog("TeX2img", "テンプレートの名前を入れてください");
            input.ButtonClicked += ((s, ee) => {
                if(Templates.ContainsKey(ee.InputedText)) {
                    MessageBox.Show(ee.InputedText + " は既に存在します．");
                    ee.Cancel = true;
                } else if(invalidTemplateNames.Contains(ee.InputedText)) {
                    MessageBox.Show(ee.InputedText + " はテンプレート名には用いることができません．");
                    ee.Cancel = true;
                }
            });
            if(input.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Templates.Add(input.InputedText, currentPreamble);
                TemplateListBox.Items.Add(input.InputedText);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            var temp = (string)TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show("上書きするテンプレートを選択してください．");
            } else {
                if(MessageBox.Show(temp + " に現在のプリアンブルを上書き保存します．よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    Templates[temp] = currentPreamble;
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e) {
            var temp = (string) TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show("削除するテンプレートを選択してください．");
            } else {
                if(MessageBox.Show(temp + " を削除します．よろしいですか？", "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    TemplateListBox.Items.Remove(TemplateListBox.SelectedItem);
                    Templates.Remove(temp);
                }
            }
        }

        private void ShowButton_Click(object sender, EventArgs e) {
            var temp = (string) TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show("表示するテンプレートを選択してください．");
            } else {
                MessageBox.Show(temp + " の中身は次の通りです．\n\n" + Templates[temp], "TeX2img");
            }
        }

        private void OKButton_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void Cancel_Button_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
