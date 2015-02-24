using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeX2img {
    public partial class InputComboDialog : Form {
        public string InputedText { get; private set; }
        TextBox InputTextBox;
        bool isCombo;

        public InputComboDialog(string title,string label,List<string> list,string defaultstr = "") {
            InitializeComponent();
            if(list == null) {
                isCombo = false;
                InputTextBox = new TextBox() {
                    Location = InputComboBox.Location,
                    Size = InputComboBox.Size,
                    TabIndex = InputComboBox.TabIndex,
                    Name = "InputTextBox",
                };
                Controls.Remove(InputComboBox);
                Controls.Add(InputTextBox);
                InputTextBox.Text = defaultstr;
            } else {
                isCombo = true;
                foreach(var l in list) {
                    InputComboBox.Items.Add(l);
                }
                InputComboBox.Text = defaultstr;
            }
            InputDialogLabel.Text = label;
            Text = title;
        }

        protected override void OnShown(EventArgs e) {
            if(isCombo) InputComboBox.Focus();
            else InputTextBox.Focus();
            base.OnShown(e);
        }

        private void OKButton_Click(object sender, EventArgs e) {
            var text = isCombo ? InputComboBox.Text : InputTextBox.Text;
            var ee = new OKButtonClickedEventArgs(text);
            OKButtonClicked(this, ee);
            if(!ee.Cancel) {
                InputedText = text;
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public class OKButtonClickedEventArgs : EventArgs{
            public bool Cancel = false;
            public string InputedText;
            public OKButtonClickedEventArgs(string text) { InputedText = text; }
        }
        public delegate void OKButtonClieckedEventHandler(object sender, OKButtonClickedEventArgs e);
        public event OKButtonClieckedEventHandler OKButtonClicked = ((s, e) => { });

    }
}
