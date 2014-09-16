namespace TeX2img
{
    partial class SettingForm
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            this.SettingTab = new System.Windows.Forms.TabControl();
            this.PathSettingTab = new System.Windows.Forms.TabPage();
            this.encodeComboBox = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.ignoreErrorCheckBox = new System.Windows.Forms.CheckBox();
            this.gsBrowseButton = new System.Windows.Forms.Button();
            this.gsTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.dvipdfmxBrowseButton = new System.Windows.Forms.Button();
            this.dvipdfmxTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.platexBrowseButton = new System.Windows.Forms.Button();
            this.platexTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.OutputImgSettingTab = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.imageMagickLinkLabel = new System.Windows.Forms.LinkLabel();
            this.useMagickCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.transparentPngCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.radioButtonbp = new System.Windows.Forms.RadioButton();
            this.radioButtonpx = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.bottomMarginUpDown = new System.Windows.Forms.NumericUpDown();
            this.rightMarginUpDown = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.leftMarginUpDown = new System.Windows.Forms.NumericUpDown();
            this.topMarginUpDown = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.resolutionScaleUpDown = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.AfterCompilingTab = new System.Windows.Forms.TabPage();
            this.showOutputWindowCheckBox = new System.Windows.Forms.CheckBox();
            this.previewCheckBox = new System.Windows.Forms.CheckBox();
            this.openTmpFolderButton = new System.Windows.Forms.Button();
            this.deleteTmpFilesCheckBox = new System.Windows.Forms.CheckBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.platexOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.dvipdfmxOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.gsOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SettingTab.SuspendLayout();
            this.PathSettingTab.SuspendLayout();
            this.OutputImgSettingTab.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomMarginUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightMarginUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftMarginUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.topMarginUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionScaleUpDown)).BeginInit();
            this.AfterCompilingTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingTab
            // 
            this.SettingTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SettingTab.Controls.Add(this.PathSettingTab);
            this.SettingTab.Controls.Add(this.OutputImgSettingTab);
            this.SettingTab.Controls.Add(this.AfterCompilingTab);
            this.SettingTab.Location = new System.Drawing.Point(22, 20);
            this.SettingTab.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.SettingTab.Name = "SettingTab";
            this.SettingTab.SelectedIndex = 0;
            this.SettingTab.Size = new System.Drawing.Size(683, 500);
            this.SettingTab.TabIndex = 1;
            // 
            // PathSettingTab
            // 
            this.PathSettingTab.Controls.Add(this.encodeComboBox);
            this.PathSettingTab.Controls.Add(this.label10);
            this.PathSettingTab.Controls.Add(this.ignoreErrorCheckBox);
            this.PathSettingTab.Controls.Add(this.gsBrowseButton);
            this.PathSettingTab.Controls.Add(this.gsTextBox);
            this.PathSettingTab.Controls.Add(this.label3);
            this.PathSettingTab.Controls.Add(this.dvipdfmxBrowseButton);
            this.PathSettingTab.Controls.Add(this.dvipdfmxTextBox);
            this.PathSettingTab.Controls.Add(this.label2);
            this.PathSettingTab.Controls.Add(this.platexBrowseButton);
            this.PathSettingTab.Controls.Add(this.platexTextBox);
            this.PathSettingTab.Controls.Add(this.label1);
            this.PathSettingTab.Location = new System.Drawing.Point(4, 28);
            this.PathSettingTab.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.PathSettingTab.Name = "PathSettingTab";
            this.PathSettingTab.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.PathSettingTab.Size = new System.Drawing.Size(675, 468);
            this.PathSettingTab.TabIndex = 0;
            this.PathSettingTab.Text = "パスの設定";
            this.PathSettingTab.UseVisualStyleBackColor = true;
            // 
            // encodeComboBox
            // 
            this.encodeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.encodeComboBox.FormattingEnabled = true;
            this.encodeComboBox.Location = new System.Drawing.Point(120, 181);
            this.encodeComboBox.Name = "encodeComboBox";
            this.encodeComboBox.Size = new System.Drawing.Size(419, 26);
            this.encodeComboBox.TabIndex = 20;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 184);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(84, 18);
            this.label10.TabIndex = 19;
            this.label10.Text = "文字コード";
            // 
            // ignoreErrorCheckBox
            // 
            this.ignoreErrorCheckBox.AutoSize = true;
            this.ignoreErrorCheckBox.Location = new System.Drawing.Point(17, 134);
            this.ignoreErrorCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.ignoreErrorCheckBox.Name = "ignoreErrorCheckBox";
            this.ignoreErrorCheckBox.Size = new System.Drawing.Size(405, 22);
            this.ignoreErrorCheckBox.TabIndex = 18;
            this.ignoreErrorCheckBox.Text = "少々のコンパイルエラーは無視して画像化を強行する";
            this.ignoreErrorCheckBox.UseVisualStyleBackColor = true;
            // 
            // gsBrowseButton
            // 
            this.gsBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gsBrowseButton.Location = new System.Drawing.Point(553, 96);
            this.gsBrowseButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.gsBrowseButton.Name = "gsBrowseButton";
            this.gsBrowseButton.Size = new System.Drawing.Size(98, 28);
            this.gsBrowseButton.TabIndex = 15;
            this.gsBrowseButton.Text = "参照...";
            this.gsBrowseButton.UseVisualStyleBackColor = true;
            this.gsBrowseButton.Click += new System.EventHandler(this.gsBrowseButton_Click);
            // 
            // gsTextBox
            // 
            this.gsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gsTextBox.Location = new System.Drawing.Point(120, 96);
            this.gsTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.gsTextBox.Name = "gsTextBox";
            this.gsTextBox.Size = new System.Drawing.Size(419, 25);
            this.gsTextBox.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 100);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 18);
            this.label3.TabIndex = 17;
            this.label3.Text = "gswin32c ：";
            // 
            // dvipdfmxBrowseButton
            // 
            this.dvipdfmxBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dvipdfmxBrowseButton.Location = new System.Drawing.Point(553, 58);
            this.dvipdfmxBrowseButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.dvipdfmxBrowseButton.Name = "dvipdfmxBrowseButton";
            this.dvipdfmxBrowseButton.Size = new System.Drawing.Size(98, 28);
            this.dvipdfmxBrowseButton.TabIndex = 13;
            this.dvipdfmxBrowseButton.Text = "参照...";
            this.dvipdfmxBrowseButton.UseVisualStyleBackColor = true;
            this.dvipdfmxBrowseButton.Click += new System.EventHandler(this.dvipdfmxBrowseButton_Click);
            // 
            // dvipdfmxTextBox
            // 
            this.dvipdfmxTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dvipdfmxTextBox.Location = new System.Drawing.Point(120, 58);
            this.dvipdfmxTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.dvipdfmxTextBox.Name = "dvipdfmxTextBox";
            this.dvipdfmxTextBox.Size = new System.Drawing.Size(419, 25);
            this.dvipdfmxTextBox.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 63);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 18);
            this.label2.TabIndex = 16;
            this.label2.Text = "dvipdfmx ：";
            // 
            // platexBrowseButton
            // 
            this.platexBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.platexBrowseButton.Location = new System.Drawing.Point(553, 21);
            this.platexBrowseButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.platexBrowseButton.Name = "platexBrowseButton";
            this.platexBrowseButton.Size = new System.Drawing.Size(98, 28);
            this.platexBrowseButton.TabIndex = 11;
            this.platexBrowseButton.Text = "参照...";
            this.platexBrowseButton.UseVisualStyleBackColor = true;
            this.platexBrowseButton.Click += new System.EventHandler(this.platexBrowseButton_Click);
            // 
            // platexTextBox
            // 
            this.platexTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.platexTextBox.Location = new System.Drawing.Point(120, 21);
            this.platexTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.platexTextBox.Name = "platexTextBox";
            this.platexTextBox.Size = new System.Drawing.Size(419, 25);
            this.platexTextBox.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 18);
            this.label1.TabIndex = 10;
            this.label1.Text = "platex ：";
            // 
            // OutputImgSettingTab
            // 
            this.OutputImgSettingTab.Controls.Add(this.groupBox4);
            this.OutputImgSettingTab.Controls.Add(this.groupBox2);
            this.OutputImgSettingTab.Controls.Add(this.groupBox1);
            this.OutputImgSettingTab.Location = new System.Drawing.Point(4, 28);
            this.OutputImgSettingTab.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.OutputImgSettingTab.Name = "OutputImgSettingTab";
            this.OutputImgSettingTab.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.OutputImgSettingTab.Size = new System.Drawing.Size(675, 468);
            this.OutputImgSettingTab.TabIndex = 1;
            this.OutputImgSettingTab.Text = "出力画像設定";
            this.OutputImgSettingTab.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.imageMagickLinkLabel);
            this.groupBox4.Controls.Add(this.useMagickCheckBox);
            this.groupBox4.Location = new System.Drawing.Point(25, 282);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox4.Size = new System.Drawing.Size(612, 87);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "PNG / JPEG 共通設定";
            // 
            // imageMagickLinkLabel
            // 
            this.imageMagickLinkLabel.AutoSize = true;
            this.imageMagickLinkLabel.Location = new System.Drawing.Point(43, 56);
            this.imageMagickLinkLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.imageMagickLinkLabel.Name = "imageMagickLinkLabel";
            this.imageMagickLinkLabel.Size = new System.Drawing.Size(262, 18);
            this.imageMagickLinkLabel.TabIndex = 1;
            this.imageMagickLinkLabel.TabStop = true;
            this.imageMagickLinkLabel.Text = "ImageMagick のダウンロードページへ";
            this.imageMagickLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.imageMagickLinkLabel_LinkClicked);
            // 
            // useMagickCheckBox
            // 
            this.useMagickCheckBox.AutoSize = true;
            this.useMagickCheckBox.Location = new System.Drawing.Point(15, 27);
            this.useMagickCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.useMagickCheckBox.Name = "useMagickCheckBox";
            this.useMagickCheckBox.Size = new System.Drawing.Size(514, 22);
            this.useMagickCheckBox.TabIndex = 0;
            this.useMagickCheckBox.Text = "ImageMagick（別途インストール）を使用してアンチエイリアス処理する";
            this.useMagickCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.transparentPngCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(25, 380);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox2.Size = new System.Drawing.Size(612, 70);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PNG 固有の設定";
            // 
            // transparentPngCheckBox
            // 
            this.transparentPngCheckBox.AutoSize = true;
            this.transparentPngCheckBox.Checked = true;
            this.transparentPngCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.transparentPngCheckBox.Location = new System.Drawing.Point(15, 30);
            this.transparentPngCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.transparentPngCheckBox.Name = "transparentPngCheckBox";
            this.transparentPngCheckBox.Size = new System.Drawing.Size(179, 22);
            this.transparentPngCheckBox.TabIndex = 0;
            this.transparentPngCheckBox.Text = "背景色を透過させる";
            this.transparentPngCheckBox.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.bottomMarginUpDown);
            this.groupBox1.Controls.Add(this.rightMarginUpDown);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.leftMarginUpDown);
            this.groupBox1.Controls.Add(this.topMarginUpDown);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.resolutionScaleUpDown);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Location = new System.Drawing.Point(25, 17);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Size = new System.Drawing.Size(612, 255);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "共通設定";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.radioButtonbp);
            this.groupBox3.Controls.Add(this.radioButtonpx);
            this.groupBox3.Location = new System.Drawing.Point(13, 190);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(591, 58);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "余白の単位（JPEG / PNG）";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(181, 28);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(223, 18);
            this.label9.TabIndex = 2;
            this.label9.Text = "（PDF / EPS は　bp　で固定）";
            // 
            // radioButtonbp
            // 
            this.radioButtonbp.AutoSize = true;
            this.radioButtonbp.Location = new System.Drawing.Point(91, 26);
            this.radioButtonbp.Name = "radioButtonbp";
            this.radioButtonbp.Size = new System.Drawing.Size(51, 22);
            this.radioButtonbp.TabIndex = 1;
            this.radioButtonbp.TabStop = true;
            this.radioButtonbp.Text = "bp";
            this.radioButtonbp.UseVisualStyleBackColor = true;
            // 
            // radioButtonpx
            // 
            this.radioButtonpx.AutoSize = true;
            this.radioButtonpx.Location = new System.Drawing.Point(22, 26);
            this.radioButtonpx.Name = "radioButtonpx";
            this.radioButtonpx.Size = new System.Drawing.Size(50, 22);
            this.radioButtonpx.TabIndex = 0;
            this.radioButtonpx.TabStop = true;
            this.radioButtonpx.Text = "px";
            this.radioButtonpx.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(422, 128);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 18);
            this.label4.TabIndex = 9;
            this.label4.Text = "右：";
            // 
            // bottomMarginUpDown
            // 
            this.bottomMarginUpDown.Location = new System.Drawing.Point(308, 158);
            this.bottomMarginUpDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.bottomMarginUpDown.Name = "bottomMarginUpDown";
            this.bottomMarginUpDown.Size = new System.Drawing.Size(93, 25);
            this.bottomMarginUpDown.TabIndex = 6;
            // 
            // rightMarginUpDown
            // 
            this.rightMarginUpDown.Location = new System.Drawing.Point(470, 124);
            this.rightMarginUpDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.rightMarginUpDown.Name = "rightMarginUpDown";
            this.rightMarginUpDown.Size = new System.Drawing.Size(93, 25);
            this.rightMarginUpDown.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(260, 160);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 18);
            this.label5.TabIndex = 6;
            this.label5.Text = "下：";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(260, 93);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 18);
            this.label6.TabIndex = 5;
            this.label6.Text = "上：";
            // 
            // leftMarginUpDown
            // 
            this.leftMarginUpDown.Location = new System.Drawing.Point(147, 123);
            this.leftMarginUpDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.leftMarginUpDown.Name = "leftMarginUpDown";
            this.leftMarginUpDown.Size = new System.Drawing.Size(93, 25);
            this.leftMarginUpDown.TabIndex = 4;
            // 
            // topMarginUpDown
            // 
            this.topMarginUpDown.Location = new System.Drawing.Point(308, 90);
            this.topMarginUpDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.topMarginUpDown.Name = "topMarginUpDown";
            this.topMarginUpDown.Size = new System.Drawing.Size(93, 25);
            this.topMarginUpDown.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 126);
            this.label7.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 18);
            this.label7.TabIndex = 2;
            this.label7.Text = "余白    　　左：";
            // 
            // resolutionScaleUpDown
            // 
            this.resolutionScaleUpDown.Location = new System.Drawing.Point(391, 34);
            this.resolutionScaleUpDown.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.resolutionScaleUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.resolutionScaleUpDown.Name = "resolutionScaleUpDown";
            this.resolutionScaleUpDown.Size = new System.Drawing.Size(110, 25);
            this.resolutionScaleUpDown.TabIndex = 1;
            this.resolutionScaleUpDown.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 38);
            this.label8.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(348, 18);
            this.label8.TabIndex = 0;
            this.label8.Text = "解像度レベル（1～100 PNG / JPEG 用設定）：";
            // 
            // AfterCompilingTab
            // 
            this.AfterCompilingTab.Controls.Add(this.showOutputWindowCheckBox);
            this.AfterCompilingTab.Controls.Add(this.previewCheckBox);
            this.AfterCompilingTab.Controls.Add(this.openTmpFolderButton);
            this.AfterCompilingTab.Controls.Add(this.deleteTmpFilesCheckBox);
            this.AfterCompilingTab.Location = new System.Drawing.Point(4, 28);
            this.AfterCompilingTab.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.AfterCompilingTab.Name = "AfterCompilingTab";
            this.AfterCompilingTab.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.AfterCompilingTab.Size = new System.Drawing.Size(675, 468);
            this.AfterCompilingTab.TabIndex = 2;
            this.AfterCompilingTab.Text = "コンパイル後処理";
            this.AfterCompilingTab.UseVisualStyleBackColor = true;
            // 
            // showOutputWindowCheckBox
            // 
            this.showOutputWindowCheckBox.AutoSize = true;
            this.showOutputWindowCheckBox.Checked = true;
            this.showOutputWindowCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showOutputWindowCheckBox.Location = new System.Drawing.Point(25, 28);
            this.showOutputWindowCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.showOutputWindowCheckBox.Name = "showOutputWindowCheckBox";
            this.showOutputWindowCheckBox.Size = new System.Drawing.Size(336, 22);
            this.showOutputWindowCheckBox.TabIndex = 0;
            this.showOutputWindowCheckBox.Text = "コンパイル後出力ウィンドウを自動的に表示";
            this.showOutputWindowCheckBox.UseVisualStyleBackColor = true;
            // 
            // previewCheckBox
            // 
            this.previewCheckBox.AutoSize = true;
            this.previewCheckBox.Checked = true;
            this.previewCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.previewCheckBox.Location = new System.Drawing.Point(25, 62);
            this.previewCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.previewCheckBox.Name = "previewCheckBox";
            this.previewCheckBox.Size = new System.Drawing.Size(248, 22);
            this.previewCheckBox.TabIndex = 1;
            this.previewCheckBox.Text = "コンパイル後生成ファイルを開く";
            this.previewCheckBox.UseVisualStyleBackColor = true;
            // 
            // openTmpFolderButton
            // 
            this.openTmpFolderButton.Location = new System.Drawing.Point(25, 128);
            this.openTmpFolderButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.openTmpFolderButton.Name = "openTmpFolderButton";
            this.openTmpFolderButton.Size = new System.Drawing.Size(260, 30);
            this.openTmpFolderButton.TabIndex = 3;
            this.openTmpFolderButton.Text = "作業フォルダを開く...";
            this.openTmpFolderButton.UseVisualStyleBackColor = true;
            this.openTmpFolderButton.Click += new System.EventHandler(this.openTmpFolderButton_Click);
            // 
            // deleteTmpFilesCheckBox
            // 
            this.deleteTmpFilesCheckBox.AutoSize = true;
            this.deleteTmpFilesCheckBox.Checked = true;
            this.deleteTmpFilesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.deleteTmpFilesCheckBox.Location = new System.Drawing.Point(25, 94);
            this.deleteTmpFilesCheckBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.deleteTmpFilesCheckBox.Name = "deleteTmpFilesCheckBox";
            this.deleteTmpFilesCheckBox.Size = new System.Drawing.Size(340, 22);
            this.deleteTmpFilesCheckBox.TabIndex = 2;
            this.deleteTmpFilesCheckBox.Text = "コンパイル後作業ファイルを削除する（推奨）";
            this.deleteTmpFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(445, 529);
            this.OKButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(125, 34);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(580, 529);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(125, 34);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // platexOpenFileDialog
            // 
            this.platexOpenFileDialog.FileName = "platex.exe";
            this.platexOpenFileDialog.Filter = "exe ファイル (*.exe)|*.exe|bat ファイル (*.bat)|*.bat|すべてのファイル (*.*)|*.*";
            // 
            // dvipdfmxOpenFileDialog
            // 
            this.dvipdfmxOpenFileDialog.FileName = "dvipdfmx.exe";
            this.dvipdfmxOpenFileDialog.Filter = "exe ファイル (*.exe)|*.exe|bat ファイル (*.bat)|*.bat|すべてのファイル (*.*)|*.*";
            // 
            // gsOpenFileDialog
            // 
            this.gsOpenFileDialog.FileName = "gswin32c.exe";
            this.gsOpenFileDialog.Filter = "exe ファイル (*.exe)|*.exe|bat ファイル (*.bat)|*.bat|すべてのファイル (*.*)|*.*";
            // 
            // SettingForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(725, 582);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.SettingTab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "SettingForm";
            this.Text = "オプション";
            this.SettingTab.ResumeLayout(false);
            this.PathSettingTab.ResumeLayout(false);
            this.PathSettingTab.PerformLayout();
            this.OutputImgSettingTab.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bottomMarginUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rightMarginUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.leftMarginUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.topMarginUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resolutionScaleUpDown)).EndInit();
            this.AfterCompilingTab.ResumeLayout(false);
            this.AfterCompilingTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl SettingTab;
        private System.Windows.Forms.TabPage PathSettingTab;
        private System.Windows.Forms.TabPage OutputImgSettingTab;
        private System.Windows.Forms.TabPage AfterCompilingTab;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button gsBrowseButton;
        private System.Windows.Forms.TextBox gsTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button dvipdfmxBrowseButton;
        private System.Windows.Forms.TextBox dvipdfmxTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button platexBrowseButton;
        private System.Windows.Forms.TextBox platexTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox transparentPngCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown bottomMarginUpDown;
        private System.Windows.Forms.NumericUpDown rightMarginUpDown;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown leftMarginUpDown;
        private System.Windows.Forms.NumericUpDown topMarginUpDown;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown resolutionScaleUpDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.OpenFileDialog platexOpenFileDialog;
        private System.Windows.Forms.OpenFileDialog dvipdfmxOpenFileDialog;
        private System.Windows.Forms.OpenFileDialog gsOpenFileDialog;
        private System.Windows.Forms.CheckBox deleteTmpFilesCheckBox;
        private System.Windows.Forms.Button openTmpFolderButton;
        private System.Windows.Forms.CheckBox showOutputWindowCheckBox;
        private System.Windows.Forms.CheckBox previewCheckBox;
        private System.Windows.Forms.CheckBox ignoreErrorCheckBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.LinkLabel imageMagickLinkLabel;
        private System.Windows.Forms.CheckBox useMagickCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButtonpx;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.RadioButton radioButtonbp;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox encodeComboBox;
    }
}