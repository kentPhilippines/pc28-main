namespace DummyApp
{
    partial class RobotConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotConfigForm));
            txt机器人ID = new System.Windows.Forms.TextBox();
            txt监控群ID = new System.Windows.Forms.TextBox();
            btn保存设置 = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // txt机器人ID
            // 
            txt机器人ID.Location = new System.Drawing.Point(139, 78);
            txt机器人ID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            txt机器人ID.Name = "txt机器人ID";
            txt机器人ID.Size = new System.Drawing.Size(152, 23);
            txt机器人ID.TabIndex = 6;
            // 
            // txt监控群ID
            // 
            txt监控群ID.Location = new System.Drawing.Point(139, 38);
            txt监控群ID.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            txt监控群ID.Name = "txt监控群ID";
            txt监控群ID.Size = new System.Drawing.Size(152, 23);
            txt监控群ID.TabIndex = 7;
            // 
            // btn保存设置
            // 
            btn保存设置.Location = new System.Drawing.Point(139, 141);
            btn保存设置.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            btn保存设置.Name = "btn保存设置";
            btn保存设置.Size = new System.Drawing.Size(88, 30);
            btn保存设置.TabIndex = 5;
            btn保存设置.Text = "保存设置";
            btn保存设置.UseVisualStyleBackColor = true;
            btn保存设置.Click += btn保存设置_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(66, 84);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(60, 17);
            label2.TabIndex = 3;
            label2.Text = "机器人ID:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(66, 43);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(60, 17);
            label1.TabIndex = 4;
            label1.Text = "监控群ID:";
            // 
            // RobotConfigForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(366, 211);
            Controls.Add(txt机器人ID);
            Controls.Add(txt监控群ID);
            Controls.Add(btn保存设置);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            Name = "RobotConfigForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "设置监控群/机器人ID";
            Load += RobotConfigForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox txt机器人ID;
        private System.Windows.Forms.TextBox txt监控群ID;
        private System.Windows.Forms.Button btn保存设置;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}