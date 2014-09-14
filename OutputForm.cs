using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TeX2img
{
    public partial class OutputForm : Form
    {
        MainForm mainForm;

        public OutputForm(MainForm _mainForm)
        {
            mainForm = _mainForm;
            InitializeComponent();
        }

        public TextBox getOutputTextBox()
        {
            return outputTextBox;
        }

        public void setLocation(int x, int y)
        {
            this.Location = new Point(x, y);
        }

        private void OutputForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ユーザの操作によりウィンドウが閉じられようとしたときはそれをキャンセルし，
            // 単にウィンドウを非表示にする
            if(e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                mainForm.showOutputWindow(false);
            }
        }

        private void logClearButton_Click(object sender, EventArgs e)
        {
            outputTextBox.Text = "";
        }
    }
}