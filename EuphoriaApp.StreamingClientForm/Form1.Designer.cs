using LibVLCSharp.WinForms;

namespace EuphoriaApp.StreamingClientForm
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
            videoView1 = new VideoView();
            btnStartListening = new Button();
            btnStatus = new Button();
            timer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)videoView1).BeginInit();
            SuspendLayout();
            // 
            // videoView1
            // 
            videoView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videoView1.BackColor = Color.Black;
            videoView1.Location = new Point(0, 0);
            videoView1.MediaPlayer = null;
            videoView1.Name = "videoView1";
            videoView1.Size = new Size(1920, 1080);
            videoView1.TabIndex = 1;
            videoView1.Text = "videoView1";
            // 
            // btnStartListening
            // 
            btnStartListening.Location = new Point(12, 1026);
            btnStartListening.Name = "btnStartListening";
            btnStartListening.Size = new Size(180, 23);
            btnStartListening.TabIndex = 0;
            btnStartListening.Text = "Start Listening";
            btnStartListening.UseVisualStyleBackColor = true;
            btnStartListening.Click += btnStartListening_Click;
            // 
            // btnStatus
            // 
            btnStatus.Location = new Point(198, 1026);
            btnStatus.Name = "btnStatus";
            btnStatus.Size = new Size(1724, 23);
            btnStatus.TabIndex = 2;
            btnStatus.Text = "Status";
            btnStatus.UseVisualStyleBackColor = true;
            // 
            // timer
            // 
            timer.Enabled = true;
            timer.Interval = 2000;
            timer.Tick += Timer_Tick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1934, 1061);
            Controls.Add(btnStatus);
            Controls.Add(btnStartListening);
            Controls.Add(videoView1);
            MaximumSize = new Size(1950, 1100);
            MinimumSize = new Size(1950, 1100);
            Name = "Form1";
            Text = "Form1";
            FormClosed += Form1_FormClosed;
            ((System.ComponentModel.ISupportInitialize)videoView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnStartListening;
        private VideoView videoView1;
        private Button btnStatus;
        private System.Windows.Forms.Timer timer;
    }
}
