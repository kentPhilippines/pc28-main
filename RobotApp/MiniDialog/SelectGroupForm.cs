using ImLibrary.IM;
using ImLibrary.Model;
using RobotApp.IM;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace RobotApp.MiniDialog
{  
    [SupportedOSPlatform("windows")]
    internal partial class SelectGroupForm : Form
    {
        public string SelectedValue { get; private set; }

        public SelectGroupForm()
        {
            InitializeComponent();
        }

        private void btnGroupConfirm_Click(object sender, EventArgs e)
        {
            if (cmbGroups.SelectedValue == null)
            {
                MessageBox.Show("请选择一个群");
                return;
            }
            string groupType = RobotClient.Robot.GroupMap[(string)cmbGroups.SelectedValue].GroupType;
            if (groupType != "2")
            {
                MessageBox.Show("请选择一个工作群！");
                return;
            }
            RobotClient.Robot.DefauleGroup = RobotClient.Robot.GroupMap[(string)cmbGroups.SelectedValue];

            UserConfig userConfig = new UserConfig();
            userConfig.configName = "绑定信息";
            userConfig.configKey = "game.bindInfo";
            userConfig.configValue = RobotClient.Robot.ToBindJsonInfo();
            CentralApi.SaveConfig(userConfig);
            this.DialogResult = DialogResult.OK;
        }

        private void btnLoginGetGroups_Click(object sender, EventArgs e)
        {
            if (txtUserName.Text == "")
            {
                MessageBox.Show("用户名不能为空");
                return;
            }
            if (txtPassword.Text == "")
            {
                MessageBox.Show("密码不能为空");
                return;
            }
            try { 
                bool suc = RobotClient.Robot.CreateToken(txtUserName.Text, txtPassword.Text);
                if (!suc)
                {
                    MessageBox.Show("登录失败！");
                    return;
                }
                suc = RobotClient.Robot.TryLogin();
                if (!suc)
                {
                    MessageBox.Show("登录聊天服务器失败！");
                    return;
                }
                suc = RobotClient.Robot.TryGetGroups();
                if (!suc)
                {
                    MessageBox.Show("获取群列表失败！");
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
            cmbGroups.Items.Clear();
            cmbGroups.DataSource = RobotClient.Robot.GroupMap.Values.ToList();
        }
    }
}
