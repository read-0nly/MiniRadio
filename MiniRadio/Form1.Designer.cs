namespace MiniRadio
{
    partial class MiniRadio
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button3 = new Button();
            folderBox = new TextBox();
            ipBox = new TextBox();
            button4 = new Button();
            portBox = new TextBox();
            albumCountLabel = new Label();
            folderBrowserDialog1 = new FolderBrowserDialog();
            button1 = new Button();
            SuspendLayout();
            // 
            // button3
            // 
            button3.Location = new Point(12, 92);
            button3.Name = "button3";
            button3.Size = new Size(75, 23);
            button3.TabIndex = 4;
            button3.Text = "Start";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // folderBox
            // 
            folderBox.Location = new Point(12, 46);
            folderBox.Name = "folderBox";
            folderBox.PlaceholderText = "Folder Path";
            folderBox.Size = new Size(190, 23);
            folderBox.TabIndex = 2;
            // 
            // ipBox
            // 
            ipBox.Location = new Point(12, 17);
            ipBox.Name = "ipBox";
            ipBox.PlaceholderText = "IP";
            ipBox.Size = new Size(149, 23);
            ipBox.TabIndex = 0;
            // 
            // button4
            // 
            button4.Location = new Point(152, 92);
            button4.Name = "button4";
            button4.Size = new Size(75, 23);
            button4.TabIndex = 5;
            button4.Text = "Stop";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // portBox
            // 
            portBox.Location = new Point(167, 17);
            portBox.Name = "portBox";
            portBox.PlaceholderText = "Port";
            portBox.Size = new Size(60, 23);
            portBox.TabIndex = 1;
            // 
            // albumCountLabel
            // 
            albumCountLabel.AutoSize = true;
            albumCountLabel.Location = new Point(12, 72);
            albumCountLabel.Name = "albumCountLabel";
            albumCountLabel.Size = new Size(93, 15);
            albumCountLabel.TabIndex = 7;
            albumCountLabel.Text = "Albums Loaded:";
            // 
            // button1
            // 
            button1.Location = new Point(205, 46);
            button1.Name = "button1";
            button1.Size = new Size(22, 23);
            button1.TabIndex = 3;
            button1.Text = "📁";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // MiniRadio
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(242, 127);
            Controls.Add(button1);
            Controls.Add(albumCountLabel);
            Controls.Add(portBox);
            Controls.Add(button4);
            Controls.Add(ipBox);
            Controls.Add(folderBox);
            Controls.Add(button3);
            Name = "MiniRadio";
            Text = "MiniRadio";
            FormClosing += Form1_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button button3;
        private TextBox folderBox;
        private TextBox ipBox;
        private Button button4;
        private TextBox portBox;
        private Label albumCountLabel;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button button1;
    }
}
