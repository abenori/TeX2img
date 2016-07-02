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
                MessageBox.Show(Properties.Resources.NOTSELECTED_DELETETEMPLATE);
            } else {
                if (MessageBox.Show(String.Format(Properties.Resources.DELETEMSG, temp), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    TemplateListBox.Items.Remove(TemplateListBox.SelectedItem);
                    Templates.Remove(temp);
                }
            }
        }

        private void ShowButton_Click(object sender, EventArgs e) {
            var temp = (string) TemplateListBox.SelectedItem;
            if(temp == null || !Templates.ContainsKey(temp)) {
                MessageBox.Show(Properties.Resources.NOTSELECTED_SHOWTEMPLATE);
            } else {
                MessageBox.Show(String.Format(Properties.Resources.SHOWTEMPLATEMSG, temp, Templates[temp]), "TeX2img");
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
            backToDefaultContextMenu.Items[0].Click += ((ss, ee) => {
                if (MessageBox.Show(String.Format(Properties.Resources.RESTORETEMPLATEMSG, tempnames), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    foreach (var d in temp) {
                        Templates[d.Key] = d.Value;
                    }
                    TemplateListBox.Items.Clear();
                    foreach (var d in Templates) {
                        TemplateListBox.Items.Add(d.Key);
                    }
                }
            });
            backToDefaultContextMenu.Items[1].Click += ((ss, ee) => {
                if(MessageBox.Show(String.Format(Properties.Resources.RESTORETEMPLATE_COMPLETEMSG, tempnames), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    Templates = temp;
                    TemplateListBox.Items.Clear();
                    foreach(var d in Templates) {
                        TemplateListBox.Items.Add(d.Key);
                    }
                }
            });

            var btn = (Button) sender;
            backToDefaultContextMenu.Show(btn, new Point(btn.Width, 0));

        }

        private void AddNewButton_Click(object sender, EventArgs e) {
            Save(sender, e);
        }

        private void SaveButton_Click(object sender, EventArgs e) {
            Save(sender, e);
        }

        private void Save(object sender, EventArgs e) {
            AddTemplateContextMenu.Items[0].Click += ((ss, ee) => {
                string templatename = null;
                if(sender == AddNewButton) templatename = GetNewTemplateName();
                else templatename = GetSelectedTemplateName();
                if(templatename != null) {
                    if(!Templates.ContainsKey(templatename)) TemplateListBox.Items.Add(templatename);
                    Templates[templatename] = currentPreamble;
                }
            });
            AddTemplateContextMenu.Items[1].Click += ((ss, ee) => {
                if(openTeXSourceDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
                string preamble = null, body = null;
                MainForm.ImportFile(openTeXSourceDialog.FileName, out preamble, out body);
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
            var btn = (Button) sender;
            AddTemplateContextMenu.Show(btn,new Point(btn.Width,0));
        }

        string GetNewTemplateName() {
            var input = new InputComboDialog("TeX2img", Properties.Resources.INPUTE_TEMPLATE_NAME, null);
            input.OKButtonClicked += ((sss, eee) => {
                if (invalidTemplateNames.Contains(eee.InputedText)) {
                    MessageBox.Show(String.Format(Properties.Resources.INPUTE_TEMPLATE_NAME, eee.InputedText), "TeX2img");
                    eee.Cancel = true;
                } else if (Templates.ContainsKey(eee.InputedText)) {
                    if (MessageBox.Show(String.Format(Properties.Resources.OVERWRITEMSG, eee.InputedText), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) {
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
                MessageBox.Show(Properties.Resources.NOTSELECTED_TEMPLATE, "TeX2img");
            } else {
                
                if(MessageBox.Show(String.Format(Properties.Resources.OVERWRITETEMPLATEMSG, (string)TemplateListBox.SelectedItem), "TeX2img", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    return (string) TemplateListBox.SelectedItem;
                }
            }
            return null;
        }

    }
}
