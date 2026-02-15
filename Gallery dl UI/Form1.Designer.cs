namespace Gallery_dl_UI
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            toolStrip1 = new ToolStrip();
            btnStartdownload = new Button();
            textBox1 = new TextBox();
            Sclip = new WK.Libraries.SharpClipboardNS.SharpClipboard(components);
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(815, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnStartdownload
            // 
            btnStartdownload.Location = new Point(615, 419);
            btnStartdownload.Name = "btnStartdownload";
            btnStartdownload.Size = new Size(161, 42);
            btnStartdownload.TabIndex = 1;
            btnStartdownload.Text = "Start";
            btnStartdownload.UseVisualStyleBackColor = true;
            btnStartdownload.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 222);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(287, 154);
            textBox1.TabIndex = 2;
            // 
            // Sclip
            // 
            Sclip.MonitorClipboard = true;
            Sclip.ObservableFormats.All = true;
            Sclip.ObservableFormats.Files = false;
            Sclip.ObservableFormats.Images = false;
            Sclip.ObservableFormats.Others = false;
            Sclip.ObservableFormats.Texts = true;
            Sclip.ObserveLastEntry = false;
            Sclip.Tag = null;
            Sclip.ClipboardChanged += sharpClipboard1_ClipboardChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(815, 483);
            Controls.Add(textBox1);
            Controls.Add(btnStartdownload);
            Controls.Add(toolStrip1);
            Name = "Form1";
            Text = "Gallery dl UI";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private Button btnStartdownload;
        private TextBox textBox1;
        private WK.Libraries.SharpClipboardNS.SharpClipboard Sclip;
    }
}
