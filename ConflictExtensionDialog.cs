using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TeX2img {
    public partial class ConflictExtensionDialog : Form {
        string outputFileExtension = "";
        string requiredExtension = "";
        public enum Extension { Required, OutputFile, Both, Cancel }
        public Extension ExtensionResult { get; private set; }
        public ConflictExtensionDialog(string outputfile,string required) {
            InitializeComponent();
            outputFileExtension = Path.GetExtension(outputfile);
            requiredExtension = required;
            DialogResult = DialogResult.Cancel;
            ExtensionResult = Extension.Cancel;
            if(requiredExtension.Substring(0, 1) != ".") requiredExtension = requiredExtension + ".";
        }

        private void ConflictExtensionDialog_Load(object sender, EventArgs e) {
            Message.Text = string.Format(Properties.Resources.CONFLICT_EXTENSION_MESSAGE, outputFileExtension, requiredExtension);
            outputfileExtensionButton.Text = outputFileExtension;
            requiredExtensionButton.Text = requiredExtension;
            bothExtensionButton.Text = outputFileExtension + requiredExtension;
        }

        private void requiredExtensionButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            ExtensionResult = Extension.Required;
        }

        private void outputfileExtensionButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            ExtensionResult = Extension.OutputFile;
        }

        private void bothExtensionButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            ExtensionResult = Extension.Both;
        }

        private void Cancel_Button_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Cancel;
            ExtensionResult = Extension.Cancel;
        }
    }
}
