namespace DummyApp
{
    partial class LoginForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
            grbLogin = new System.Windows.Forms.GroupBox();
            btnCancel = new System.Windows.Forms.Button();
            btnLogin = new System.Windows.Forms.Button();
            txtPassword = new System.Windows.Forms.TextBox();
            txtUserName = new System.Windows.Forms.TextBox();
            lblPassword = new System.Windows.Forms.Label();
            lblUserName = new System.Windows.Forms.Label();
            staLogin = new System.Windows.Forms.StatusStrip();
            stalLoginState = new System.Windows.Forms.ToolStripStatusLabel();
            grbLogin.SuspendLayout();
            staLogin.SuspendLayout();
            SuspendLayout();
            // 
            // grbLogin
            // 
            grbLogin.Controls.Add(btnCancel);
            grbLogin.Controls.Add(btnLogin);
            grbLogin.Controls.Add(txtPassword);
            grbLogin.Controls.Add(txtUserName);
            grbLogin.Controls.Add(lblPassword);
            grbLogin.Controls.Add(lblUserName);
            grbLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            grbLogin.Location = new System.Drawing.Point(0, 0);
            grbLogin.Margin = new System.Windows.Forms.Padding(4);
            grbLogin.Name = "grbLogin";
            grbLogin.Padding = new System.Windows.Forms.Padding(4);
            grbLogin.Size = new System.Drawing.Size(384, 241);
            grbLogin.TabIndex = 0;
            grbLogin.TabStop = false;
            grbLogin.Text = "登录";
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(94, 153);
            btnCancel.Margin = new System.Windows.Forms.Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(88, 30);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "取    消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnLogin
            // 
            btnLogin.Location = new System.Drawing.Point(198, 153);
            btnLogin.Margin = new System.Windows.Forms.Padding(4);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(88, 30);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "登    录";
            btnLogin.UseVisualStyleBackColor = true;
            btnLogin.Click += btnLogin_Click;
            // 
            // txtPassword
            // 
            txtPassword.Location = new System.Drawing.Point(135, 98);
            txtPassword.Margin = new System.Windows.Forms.Padding(4);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new System.Drawing.Size(160, 23);
            txtPassword.TabIndex = 2;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // txtUserName
            // 
            txtUserName.Location = new System.Drawing.Point(135, 50);
            txtUserName.Margin = new System.Windows.Forms.Padding(4);
            txtUserName.Name = "txtUserName";
            txtUserName.Size = new System.Drawing.Size(160, 23);
            txtUserName.TabIndex = 1;
            // 
            // lblPassword
            // 
            lblPassword.AutoSize = true;
            lblPassword.Location = new System.Drawing.Point(81, 102);
            lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new System.Drawing.Size(47, 17);
            lblPassword.TabIndex = 0;
            lblPassword.Text = "密   码:";
            // 
            // lblUserName
            // 
            lblUserName.AutoSize = true;
            lblUserName.Location = new System.Drawing.Point(81, 54);
            lblUserName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lblUserName.Name = "lblUserName";
            lblUserName.Size = new System.Drawing.Size(47, 17);
            lblUserName.TabIndex = 0;
            lblUserName.Text = "用户名:";
            // 
            // staLogin
            // 
            staLogin.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { stalLoginState });
            staLogin.Location = new System.Drawing.Point(0, 219);
            staLogin.Name = "staLogin";
            staLogin.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            staLogin.Size = new System.Drawing.Size(384, 22);
            staLogin.TabIndex = 1;
            // 
            // stalLoginState
            // 
            stalLoginState.Name = "stalLoginState";
            stalLoginState.Size = new System.Drawing.Size(0, 17);
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(384, 241);
            Controls.Add(staLogin);
            Controls.Add(grbLogin);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "智能托";
            Load += LoginForm_Load;
            grbLogin.ResumeLayout(false);
            grbLogin.PerformLayout();
            staLogin.ResumeLayout(false);
            staLogin.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox grbLogin;
        private System.Windows.Forms.StatusStrip staLogin;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.ToolStripStatusLabel stalLoginState;
        private System.Windows.Forms.Button btnCancel;
    }
}