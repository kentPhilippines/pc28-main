namespace RobotApp.MiniDialog
{
    partial class SelectGroupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectGroupForm));
            cmbGroups = new System.Windows.Forms.ComboBox();
            btnGroupConfirm = new System.Windows.Forms.Button();
            txtPassword = new System.Windows.Forms.TextBox();
            txtUserName = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            lblUserName = new System.Windows.Forms.Label();
            btnLoginGetGroups = new System.Windows.Forms.Button();
            lblGroupList = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // cmbGroups
            // 
            cmbGroups.DisplayMember = "GroupNameContainsType";
            cmbGroups.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbGroups.FormattingEnabled = true;
            cmbGroups.Location = new System.Drawing.Point(107, 158);
            cmbGroups.Margin = new System.Windows.Forms.Padding(4);
            cmbGroups.Name = "cmbGroups";
            cmbGroups.Size = new System.Drawing.Size(175, 25);
            cmbGroups.TabIndex = 0;
            cmbGroups.ValueMember = "GroupId";
            // 
            // btnGroupConfirm
            // 
            btnGroupConfirm.Location = new System.Drawing.Point(122, 202);
            btnGroupConfirm.Margin = new System.Windows.Forms.Padding(4);
            btnGroupConfirm.Name = "btnGroupConfirm";
            btnGroupConfirm.Size = new System.Drawing.Size(88, 30);
            btnGroupConfirm.TabIndex = 1;
            btnGroupConfirm.Text = "绑定帐号";
            btnGroupConfirm.UseVisualStyleBackColor = true;
            btnGroupConfirm.Click += btnGroupConfirm_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(122, 76);
            txtPassword.Margin = new System.Windows.Forms.Padding(4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new System.Drawing.Size(160, 23);
            txtPassword.TabIndex = 6;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUserName
            // 
            txtUserName.Location = new System.Drawing.Point(122, 28);
            txtUserName.Margin = new System.Windows.Forms.Padding(4);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new System.Drawing.Size(160, 23);
            txtUserName.TabIndex = 5;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(50, 80);
            lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(51, 17);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "密    码:";
            // 
            // lblUserName
            // 
            lblUserName.AutoSize = true;
            lblUserName.Location = new System.Drawing.Point(50, 32);
            lblUserName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new System.Drawing.Size(47, 17);
            lblUserName.TabIndex = 4;
            lblUserName.Text = "用户名:";
            // 
            // btnLoginGetGroups
            // 
            btnLoginGetGroups.Location = new System.Drawing.Point(122, 117);
            btnLoginGetGroups.Margin = new System.Windows.Forms.Padding(4);
            btnLoginGetGroups.Name = "btnLoginGetGroups";
            btnLoginGetGroups.Size = new System.Drawing.Size(121, 30);
            btnLoginGetGroups.TabIndex = 1;
            btnLoginGetGroups.Text = "登录获取群列表↓↓↓";
            btnLoginGetGroups.UseVisualStyleBackColor = true;
            btnLoginGetGroups.Click += btnLoginGetGroups_Click;
            // 
            // lblGroupList
            // 
            lblGroupList.AutoSize = true;
            lblGroupList.Location = new System.Drawing.Point(41, 161);
            lblGroupList.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblGroupList.Name = "lblGroupList";
            lblGroupList.Size = new System.Drawing.Size(56, 17);
            lblGroupList.TabIndex = 3;
            lblGroupList.Text = "群列表：";
            // 
            // SelectGroupForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(331, 261);
            Controls.Add(txtPassword);
            Controls.Add(txtUserName);
            Controls.Add(lblGroupList);
            Controls.Add(lblPassword);
            Controls.Add(lblUserName);
            Controls.Add(btnLoginGetGroups);
            Controls.Add(btnGroupConfirm);
            Controls.Add(cmbGroups);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SelectGroupForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "首次使用，请输入要绑定的信息";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbGroups;
        private System.Windows.Forms.Button btnGroupConfirm;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Button btnLoginGetGroups;
        private System.Windows.Forms.Label lblGroupList;
    }
}