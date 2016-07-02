namespace TeX2img {
    partial class ManageTemplateDialog {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageTemplateDialog));
            this.TemplateListBox = new System.Windows.Forms.ListBox();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.ShowButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.BackToDefaultButton = new System.Windows.Forms.Button();
            this.AddNewButton = new System.Windows.Forms.Button();
            this.SaveButton = new System.Windows.Forms.Button();
            this.AddTemplateContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fromCurrentPreambleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openTeXSourceDialog = new System.Windows.Forms.OpenFileDialog();
            this.backToDefaultContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.restoreDefaultTemplatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initializeTheListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AddTemplateContextMenu.SuspendLayout();
            this.backToDefaultContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // TemplateListBox
            // 
            resources.ApplyResources(this.TemplateListBox, "TemplateListBox");
            this.TemplateListBox.FormattingEnabled = true;
            this.TemplateListBox.Name = "TemplateListBox";
            // 
            // DeleteButton
            // 
            resources.ApplyResources(this.DeleteButton, "DeleteButton");
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // ShowButton
            // 
            resources.ApplyResources(this.ShowButton, "ShowButton");
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.UseVisualStyleBackColor = true;
            this.ShowButton.Click += new System.EventHandler(this.ShowButton_Click);
            // 
            // OKButton
            // 
            resources.ApplyResources(this.OKButton, "OKButton");
            this.OKButton.Name = "OKButton";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Cancel_Button
            // 
            resources.ApplyResources(this.Cancel_Button, "Cancel_Button");
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // BackToDefaultButton
            // 
            resources.ApplyResources(this.BackToDefaultButton, "BackToDefaultButton");
            this.BackToDefaultButton.Name = "BackToDefaultButton";
            this.BackToDefaultButton.UseVisualStyleBackColor = true;
            this.BackToDefaultButton.Click += new System.EventHandler(this.BackToDefaultButton_Click);
            // 
            // AddNewButton
            // 
            resources.ApplyResources(this.AddNewButton, "AddNewButton");
            this.AddNewButton.Name = "AddNewButton";
            this.AddNewButton.UseVisualStyleBackColor = true;
            this.AddNewButton.Click += new System.EventHandler(this.AddNewButton_Click);
            // 
            // SaveButton
            // 
            resources.ApplyResources(this.SaveButton, "SaveButton");
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // AddTemplateContextMenu
            // 
            resources.ApplyResources(this.AddTemplateContextMenu, "AddTemplateContextMenu");
            this.AddTemplateContextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.AddTemplateContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fromCurrentPreambleToolStripMenuItem,
            this.fromFileToolStripMenuItem});
            this.AddTemplateContextMenu.Name = "AddTemplateContextMenu";
            // 
            // fromCurrentPreambleToolStripMenuItem
            // 
            resources.ApplyResources(this.fromCurrentPreambleToolStripMenuItem, "fromCurrentPreambleToolStripMenuItem");
            this.fromCurrentPreambleToolStripMenuItem.Name = "fromCurrentPreambleToolStripMenuItem";
            // 
            // fromFileToolStripMenuItem
            // 
            resources.ApplyResources(this.fromFileToolStripMenuItem, "fromFileToolStripMenuItem");
            this.fromFileToolStripMenuItem.Name = "fromFileToolStripMenuItem";
            // 
            // openTeXSourceDialog
            // 
            resources.ApplyResources(this.openTeXSourceDialog, "openTeXSourceDialog");
            // 
            // backToDefaultContextMenu
            // 
            resources.ApplyResources(this.backToDefaultContextMenu, "backToDefaultContextMenu");
            this.backToDefaultContextMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.backToDefaultContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.restoreDefaultTemplatesToolStripMenuItem,
            this.initializeTheListToolStripMenuItem});
            this.backToDefaultContextMenu.Name = "backToDefaultContextMenu";
            // 
            // restoreDefaultTemplatesToolStripMenuItem
            // 
            resources.ApplyResources(this.restoreDefaultTemplatesToolStripMenuItem, "restoreDefaultTemplatesToolStripMenuItem");
            this.restoreDefaultTemplatesToolStripMenuItem.Name = "restoreDefaultTemplatesToolStripMenuItem";
            // 
            // initializeTheListToolStripMenuItem
            // 
            resources.ApplyResources(this.initializeTheListToolStripMenuItem, "initializeTheListToolStripMenuItem");
            this.initializeTheListToolStripMenuItem.Name = "initializeTheListToolStripMenuItem";
            // 
            // ManageTemplateDialog
            // 
            this.AcceptButton = this.OKButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.AddNewButton);
            this.Controls.Add(this.BackToDefaultButton);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ShowButton);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.TemplateListBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "ManageTemplateDialog";
            this.AddTemplateContextMenu.ResumeLayout(false);
            this.backToDefaultContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox TemplateListBox;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button ShowButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.Button BackToDefaultButton;
        private System.Windows.Forms.Button AddNewButton;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.ContextMenuStrip AddTemplateContextMenu;
        private System.Windows.Forms.ToolStripMenuItem fromCurrentPreambleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fromFileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openTeXSourceDialog;
        private System.Windows.Forms.ContextMenuStrip backToDefaultContextMenu;
        private System.Windows.Forms.ToolStripMenuItem restoreDefaultTemplatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem initializeTheListToolStripMenuItem;
    }
}