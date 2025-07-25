namespace RobotApp.MiniDialog
{
    partial class ManualOpenForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManualOpenForm));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            txtIssue = new System.Windows.Forms.TextBox();
            txtOpenCode1 = new System.Windows.Forms.TextBox();
            btnConfirm = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            txtOpenCode2 = new System.Windows.Forms.TextBox();
            txtOpenCode3 = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(45, 81);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(68, 17);
            label1.TabIndex = 0;
            label1.Text = "开奖号码：";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(70, 41);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(44, 17);
            label2.TabIndex = 0;
            label2.Text = "期号：";
            // 
            // txtIssue
            // 
            txtIssue.Location = new System.Drawing.Point(121, 35);
            txtIssue.Margin = new System.Windows.Forms.Padding(4);
            txtIssue.Name = "txtIssue";
            txtIssue.Size = new System.Drawing.Size(142, 23);
            txtIssue.TabIndex = 1;
            // 
            // txtOpenCode1
            // 
            txtOpenCode1.Location = new System.Drawing.Point(121, 76);
            txtOpenCode1.Margin = new System.Windows.Forms.Padding(4);
            txtOpenCode1.MaxLength = 1;
            txtOpenCode1.Name = "txtOpenCode1";
            txtOpenCode1.Size = new System.Drawing.Size(41, 23);
            txtOpenCode1.TabIndex = 2;
            txtOpenCode1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnConfirm
            // 
            btnConfirm.Location = new System.Drawing.Point(60, 123);
            btnConfirm.Margin = new System.Windows.Forms.Padding(4);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new System.Drawing.Size(88, 30);
            btnConfirm.TabIndex = 6;
            btnConfirm.Text = "确定";
            btnConfirm.UseVisualStyleBackColor = true;
            btnConfirm.Click += btnConfirm_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(171, 123);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 30);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // txtOpenCode2
            // 
            txtOpenCode2.Location = new System.Drawing.Point(170, 76);
            txtOpenCode2.Margin = new System.Windows.Forms.Padding(4);
            txtOpenCode2.MaxLength = 1;
            txtOpenCode2.Name = "txtOpenCode2";
            txtOpenCode2.Size = new System.Drawing.Size(41, 23);
            txtOpenCode2.TabIndex = 3;
            txtOpenCode2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtOpenCode3
            // 
            txtOpenCode3.Location = new System.Drawing.Point(220, 76);
            txtOpenCode3.Margin = new System.Windows.Forms.Padding(4);
            txtOpenCode3.MaxLength = 1;
            txtOpenCode3.Name = "txtOpenCode3";
            txtOpenCode3.Size = new System.Drawing.Size(41, 23);
            txtOpenCode3.TabIndex = 4;
            txtOpenCode3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // ManualOpenForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(314, 191);
            Controls.Add(txtOpenCode3);
            Controls.Add(txtOpenCode2);
            Controls.Add(btnCancel);
            Controls.Add(btnConfirm);
            Controls.Add(txtOpenCode1);
            Controls.Add(txtIssue);
            Controls.Add(label2);
            Controls.Add(label1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ManualOpenForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "手动开奖";
            Load += ManualOpenForm_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtIssue;
        private System.Windows.Forms.TextBox txtOpenCode1;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtOpenCode2;
        private System.Windows.Forms.TextBox txtOpenCode3;
    }
}