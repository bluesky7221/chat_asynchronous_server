namespace chat_asynchronous_server
{
    partial class ChatServerForm
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
            this.ChatBox = new System.Windows.Forms.TextBox();
            this.ServerStatus = new System.Windows.Forms.Label();
            this.ServerOnOffBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChatBox
            // 
            this.ChatBox.Location = new System.Drawing.Point(42, 12);
            this.ChatBox.Multiline = true;
            this.ChatBox.Name = "ChatBox";
            this.ChatBox.Size = new System.Drawing.Size(700, 690);
            this.ChatBox.TabIndex = 0;
            // 
            // ServerStatus
            // 
            this.ServerStatus.AutoSize = true;
            this.ServerStatus.Font = new System.Drawing.Font("맑은 고딕", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ServerStatus.Location = new System.Drawing.Point(42, 708);
            this.ServerStatus.Name = "ServerStatus";
            this.ServerStatus.Size = new System.Drawing.Size(103, 28);
            this.ServerStatus.TabIndex = 1;
            this.ServerStatus.Text = "Server Off";
            // 
            // ServerOnOffBtn
            // 
            this.ServerOnOffBtn.Location = new System.Drawing.Point(411, 708);
            this.ServerOnOffBtn.Name = "ServerOnOffBtn";
            this.ServerOnOffBtn.Size = new System.Drawing.Size(331, 41);
            this.ServerOnOffBtn.TabIndex = 2;
            this.ServerOnOffBtn.Text = "Server On";
            this.ServerOnOffBtn.UseVisualStyleBackColor = true;
            this.ServerOnOffBtn.Click += new System.EventHandler(this.ServerOnOffBtn_Click);
            // 
            // ChatServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 761);
            this.Controls.Add(this.ServerOnOffBtn);
            this.Controls.Add(this.ServerStatus);
            this.Controls.Add(this.ChatBox);
            this.Name = "ChatServerForm";
            this.Text = "Chat Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox ChatBox;
        private Label ServerStatus;
        private Button ServerOnOffBtn;
    }
}