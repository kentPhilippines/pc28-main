using CommonLibrary;
using Newtonsoft.Json.Linq;
using RobotApp.IM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotApp.MiniDialog
{
    [SupportedOSPlatform("windows")]
    public partial class UpdatePasswordForm : Form
    {
        public UpdatePasswordForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string oldPassord = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            if (string.IsNullOrEmpty(oldPassord))
            {
                MessageBox.Show("旧密码不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("新密码不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("确认密码不能为空！");
                return;
            }
            if (newPassword == oldPassord)
            {
                MessageBox.Show("新旧密码不能相同！");
                return;
            }
            if (confirmPassword != newPassword)
            {
                MessageBox.Show("确认密码与新密码不一致！");
                return;
            }
            JObject jo = HttpHelper.Instance.HttpPut<JObject>(IMConstant.ROBOT_SERVER + $"/system/user/profile/updatePwd?oldPassword={oldPassord}&newPassword={newPassword}");
            if ((int)jo["code"] == 200)
            {
                MessageBox.Show((string)jo["msg"]);
                this.Close();
            }
            else
            {
                MessageBox.Show((string)jo["msg"]);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
