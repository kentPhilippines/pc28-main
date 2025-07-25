namespace RobotApp.MiniDialog
{
    partial class ChatRecordForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatRecordForm));
            panel1 = new System.Windows.Forms.Panel();
            txt发送内容 = new System.Windows.Forms.TextBox();
            panel3 = new System.Windows.Forms.Panel();
            btn发送 = new System.Windows.Forms.Button();
            btn清空 = new System.Windows.Forms.Button();
            txt聊天记录 = new System.Windows.Forms.TextBox();
            panel1.SuspendLayout();
            panel3.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(txt发送内容);
            panel1.Controls.Add(panel3);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(6, 625);
            panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(12, 13, 12, 13);
            panel1.Size = new System.Drawing.Size(422, 167);
            panel1.TabIndex = 1;
            // 
            // txt发送内容
            // 
            txt发送内容.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txt发送内容.Dock = System.Windows.Forms.DockStyle.Fill;
            txt发送内容.Location = new System.Drawing.Point(12, 13);
            txt发送内容.Margin = new System.Windows.Forms.Padding(12, 13, 12, 13);
            txt发送内容.Multiline = true;
            txt发送内容.Name = "txt发送内容";
            txt发送内容.Size = new System.Drawing.Size(341, 141);
            txt发送内容.TabIndex = 0;
            // 
            // panel3
            // 
            panel3.Controls.Add(btn发送);
            panel3.Controls.Add(btn清空);
            panel3.Dock = System.Windows.Forms.DockStyle.Right;
            panel3.Location = new System.Drawing.Point(353, 13);
            panel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(57, 141);
            panel3.TabIndex = 2;
            // 
            // btn发送
            // 
            btn发送.Location = new System.Drawing.Point(5, 95);
            btn发送.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            btn发送.Name = "btn发送";
            btn发送.Size = new System.Drawing.Size(46, 30);
            btn发送.TabIndex = 0;
            btn发送.Text = "发送";
            btn发送.UseVisualStyleBackColor = true;
            btn发送.Click += btn发送_Click;
            // 
            // btn清空
            // 
            btn清空.Location = new System.Drawing.Point(5, 25);
            btn清空.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            btn清空.Name = "btn清空";
            btn清空.Size = new System.Drawing.Size(46, 30);
            btn清空.TabIndex = 0;
            btn清空.Text = "清空";
            btn清空.UseVisualStyleBackColor = true;
            btn清空.Click += btn清空_Click;
            // 
            // txt聊天记录
            // 
            txt聊天记录.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txt聊天记录.Dock = System.Windows.Forms.DockStyle.Fill;
            txt聊天记录.Location = new System.Drawing.Point(6, 7);
            txt聊天记录.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            txt聊天记录.Multiline = true;
            txt聊天记录.Name = "txt聊天记录";
            txt聊天记录.ReadOnly = true;
            txt聊天记录.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txt聊天记录.Size = new System.Drawing.Size(422, 618);
            txt聊天记录.TabIndex = 2;
            // 
            // ChatRecordForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(434, 799);
            Controls.Add(txt聊天记录);
            Controls.Add(panel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChatRecordForm";
            Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "机器人记录";
            FormClosed += ChatRecordForm_FormClosed;
            Load += ChatRecordForm_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel3.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txt发送内容;
        private System.Windows.Forms.Button btn发送;
        private System.Windows.Forms.Button btn清空;
        private System.Windows.Forms.TextBox txt聊天记录;
    }
}