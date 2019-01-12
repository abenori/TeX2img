namespace TeX2img {
    partial class ConflictExtensionDialog {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConflictExtensionDialog));
            this.requiredExtensionButton = new System.Windows.Forms.Button();
            this.outputfileExtensionButton = new System.Windows.Forms.Button();
            this.bothExtensionButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.Message = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // requiredExtensionButton
            // 
            this.requiredExtensionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.requiredExtensionButton.Location = new System.Drawing.Point(29, 244);
            this.requiredExtensionButton.Name = "requiredExtensionButton";
            this.requiredExtensionButton.Size = new System.Drawing.Size(137, 51);
            this.requiredExtensionButton.TabIndex = 1;
            this.requiredExtensionButton.Text = "required";
            this.requiredExtensionButton.UseVisualStyleBackColor = true;
            this.requiredExtensionButton.Click += new System.EventHandler(this.requiredExtensionButton_Click);
            // 
            // outputfileExtensionButton
            // 
            this.outputfileExtensionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.outputfileExtensionButton.Location = new System.Drawing.Point(198, 244);
            this.outputfileExtensionButton.Name = "outputfileExtensionButton";
            this.outputfileExtensionButton.Size = new System.Drawing.Size(137, 51);
            this.outputfileExtensionButton.TabIndex = 2;
            this.outputfileExtensionButton.Text = "outputfile";
            this.outputfileExtensionButton.UseVisualStyleBackColor = true;
            this.outputfileExtensionButton.Click += new System.EventHandler(this.outputfileExtensionButton_Click);
            // 
            // bothExtensionButton
            // 
            this.bothExtensionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bothExtensionButton.Location = new System.Drawing.Point(385, 244);
            this.bothExtensionButton.Name = "bothExtensionButton";
            this.bothExtensionButton.Size = new System.Drawing.Size(188, 51);
            this.bothExtensionButton.TabIndex = 3;
            this.bothExtensionButton.Text = "both";
            this.bothExtensionButton.UseVisualStyleBackColor = true;
            this.bothExtensionButton.Click += new System.EventHandler(this.bothExtensionButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(627, 244);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(133, 51);
            this.Cancel_Button.TabIndex = 4;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // Message
            // 
            this.Message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Message.Location = new System.Drawing.Point(29, 35);
            this.Message.Multiline = true;
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Size = new System.Drawing.Size(731, 190);
            this.Message.TabIndex = 5;
            this.Message.Text = "Message";
            // 
            // ConflictExtensionDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(800, 324);
            this.Controls.Add(this.Message);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.bothExtensionButton);
            this.Controls.Add(this.outputfileExtensionButton);
            this.Controls.Add(this.requiredExtensionButton);
            this.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConflictExtensionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TeX2img";
            this.Load += new System.EventHandler(this.ConflictExtensionDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button requiredExtensionButton;
        private System.Windows.Forms.Button outputfileExtensionButton;
        private System.Windows.Forms.Button bothExtensionButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.TextBox Message;
    }
}