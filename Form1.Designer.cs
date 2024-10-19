using System.Windows.Forms;

namespace Coloring_Book_Frame
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private TextBox txtFolderPath;
        private Button btnSelectFolder;
        private Button btnCreatePDF;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txtFolderPath = new System.Windows.Forms.TextBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.btnCreatePDF = new System.Windows.Forms.Button();
            this.txtUrlList = new System.Windows.Forms.TextBox();
            this.txtTextList = new System.Windows.Forms.TextBox();
            this.checkBoxTitle = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtFolderPath
            // 
            this.txtFolderPath.Location = new System.Drawing.Point(12, 12);
            this.txtFolderPath.Name = "txtFolderPath";
            this.txtFolderPath.Size = new System.Drawing.Size(300, 20);
            this.txtFolderPath.TabIndex = 0;
            this.txtFolderPath.Text = "C:\\Users\\redfo\\Desktop\\Coloring Book Assets\\Big Eyes, Tiny Paws A World of Adorab" +
    "le Kittens\\Test";
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Location = new System.Drawing.Point(318, 10);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFolder.TabIndex = 1;
            this.btnSelectFolder.Text = "Избор папка";
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // btnCreatePDF
            // 
            this.btnCreatePDF.Location = new System.Drawing.Point(12, 38);
            this.btnCreatePDF.Name = "btnCreatePDF";
            this.btnCreatePDF.Size = new System.Drawing.Size(381, 23);
            this.btnCreatePDF.TabIndex = 2;
            this.btnCreatePDF.Text = "Създай PDF";
            this.btnCreatePDF.UseVisualStyleBackColor = true;
            this.btnCreatePDF.Click += new System.EventHandler(this.btnCreatePDF_Click);
            // 
            // txtUrlList
            // 
            this.txtUrlList.Location = new System.Drawing.Point(12, 67);
            this.txtUrlList.Multiline = true;
            this.txtUrlList.Name = "txtUrlList";
            this.txtUrlList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUrlList.Size = new System.Drawing.Size(1317, 150);
            this.txtUrlList.TabIndex = 3;
            this.txtUrlList.Text = resources.GetString("txtUrlList.Text");
            // 
            // txtTextList
            // 
            this.txtTextList.Location = new System.Drawing.Point(12, 237);
            this.txtTextList.Multiline = true;
            this.txtTextList.Name = "txtTextList";
            this.txtTextList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTextList.Size = new System.Drawing.Size(1850, 179);
            this.txtTextList.TabIndex = 4;
            this.txtTextList.Text = resources.GetString("txtTextList.Text");
            // 
            // checkBoxTitle
            // 
            this.checkBoxTitle.AutoSize = true;
            this.checkBoxTitle.Checked = true;
            this.checkBoxTitle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTitle.Location = new System.Drawing.Point(410, 13);
            this.checkBoxTitle.Name = "checkBoxTitle";
            this.checkBoxTitle.Size = new System.Drawing.Size(149, 17);
            this.checkBoxTitle.TabIndex = 5;
            this.checkBoxTitle.Text = "Да Включва ЗАГЛАВИЯ";
            this.checkBoxTitle.UseVisualStyleBackColor = true;
            this.checkBoxTitle.CheckedChanged += new System.EventHandler(this.checkBoxTitle_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(1884, 571);
            this.Controls.Add(this.checkBoxTitle);
            this.Controls.Add(this.txtTextList);
            this.Controls.Add(this.txtUrlList);
            this.Controls.Add(this.btnCreatePDF);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtFolderPath);
            this.Name = "Form1";
            this.Text = "High Res PDF Creator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private TextBox txtUrlList;
        private TextBox txtTextList;
        private CheckBox checkBoxTitle;

        #endregion

        //private WebBrowser webBrowser1;
        //private Button buttonRunJS;
        //private Button buttonGetResults;
        //private TextBox textBoxResults;
    }
}

