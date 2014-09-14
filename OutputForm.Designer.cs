namespace TeX2img
{
    partial class OutputForm
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
            if(disposing && (components != null))
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputForm));
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.logClearButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.Font = new System.Drawing.Font("ＭＳ ゴシック", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.outputTextBox.Location = new System.Drawing.Point(10, 9);
            this.outputTextBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(737, 484);
            this.outputTextBox.TabIndex = 0;
            // 
            // logClearButton
            // 
            this.logClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.logClearButton.Location = new System.Drawing.Point(528, 504);
            this.logClearButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.logClearButton.Name = "logClearButton";
            this.logClearButton.Size = new System.Drawing.Size(220, 50);
            this.logClearButton.TabIndex = 1;
            this.logClearButton.Text = "消去";
            this.logClearButton.UseVisualStyleBackColor = true;
            this.logClearButton.Click += new System.EventHandler(this.logClearButton_Click);
            // 
            // OutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 562);
            this.Controls.Add(this.logClearButton);
            this.Controls.Add(this.outputTextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "OutputForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "出力";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OutputForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button logClearButton;
    }
}