using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeX2img {
    public partial class InputDialog : Form {
        public string InputedText { get; private set; }
        public InputDialog(string title,string label,string defaultstr = "") {
            InitializeComponent();
            InputDialogLabel.Text = label;
            Text = title;
            InputTextBox.Text = defaultstr;
        }

        private void OKButton_Click(object sender, EventArgs e) {
            var ee = new ButtonClickedEventArgs(InputTextBox.Text);
            ButtonClicked(this, ee);
            if(!ee.Cancel) {
                InputedText = InputTextBox.Text;
                DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e) {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public class ButtonClickedEventArgs : EventArgs{
            public bool Cancel = false;
            public string InputedText;
            public ButtonClickedEventArgs(string text) { InputedText = text; }
        }
        public delegate void ButtonClieckedEventHandler(object sender, ButtonClickedEventArgs e);
        public event ButtonClieckedEventHandler ButtonClicked = ((s, e) => { });

    }
}
