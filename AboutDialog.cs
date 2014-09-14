using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TeX2img
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            ProductNameLabel.Text = Application.ProductName;
            VersionLabel.Text = "Version : " + Application.ProductVersion;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}