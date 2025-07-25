using CommonLibrary;
using Newtonsoft.Json.Linq;
using RobotApp.Util;
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
    public partial class PermissionForm : Form
    {
        public static bool 修改用户积分权限;
        public static bool 修改用户下注权限;
        public static bool 修改用户假人权限;
        public static bool 查看操作记录权限;
        public static bool 修改游戏赔率权限;
        public static bool 修改游戏规则权限;
        public static bool 修改信息设置权限;
        public static string PASSWORD;

        public PermissionForm()
        {
            InitializeComponent();
        }

        private void PermissionForm_Load(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (val == "")
                    return "管理密码不能为空";
                return "";
            };
            string value = "";
            if (InputBox.Show("管理员授权", "*", "请输入管理员密码(默认：123456),请及时修改！", ref value, validation) == DialogResult.OK)
            {
                if (PASSWORD != value)
                {
                    MessageBox.Show("密码错误");
                    this.Close();
                }
            }
            else 
            {
                this.Close();
            }
            chk修改用户积分权限.Checked = 修改用户积分权限;
            chk修改用户下注权限.Checked = 修改用户下注权限;
            chk修改用户假人权限.Checked = 修改用户假人权限;
            chk查看操作记录权限.Checked = 查看操作记录权限;
            chk修改游戏赔率权限.Checked = 修改游戏赔率权限;
            chk修改游戏规则权限.Checked = 修改游戏规则权限;
            chk修改信息设置权限.Checked = 修改信息设置权限;
        }

        private void btnSavePermission_Click(object sender, EventArgs e)
        {
            修改用户积分权限 = chk修改用户积分权限.Checked;
            修改用户下注权限 = chk修改用户下注权限.Checked;
            修改用户假人权限 = chk修改用户假人权限.Checked;
            查看操作记录权限 = chk查看操作记录权限.Checked;
            修改游戏赔率权限 = chk修改游戏赔率权限.Checked;
            修改游戏规则权限 = chk修改游戏规则权限.Checked;
            修改信息设置权限 = chk修改信息设置权限.Checked;
            savePermissionConfig();
            MessageBox.Show("保存成功");
        }

        private void savePermissionConfig()
        {
            string perm = "";
            perm += 修改用户积分权限 ? 1 : 0;
            perm += 修改用户下注权限 ? 1 : 0;
            perm += 修改用户假人权限 ? 1 : 0;
            perm += 查看操作记录权限 ? 1 : 0;
            perm += 修改游戏赔率权限 ? 1 : 0;
            perm += 修改游戏规则权限 ? 1 : 0;
            perm += 修改信息设置权限 ? 1 : 0;
            perm += PASSWORD;
            //string encryptData = RSAUtil.PrivateKeyEncrypt(IMConstant.PERMISSION_PRIVATE_KEY, perm);
            ReadWriteINI.ConfigFile_SetVal("激活信息", "权限码", perm);
        }

        private void btnAdminPassword_Click(object sender, EventArgs e)
        {
            InputBoxValidation validation = delegate (string val)
            {
                if (val == "")
                    return "新管理密码不能为空";
                if (val.Length < 6)
                {
                    return "新管理密码不能少于6位";
                }
                return "";
            };
            string value = "";
            if (InputBox.Show("修改管理员密码", "*", "请输入新的管理员密码！", ref value, validation) == DialogResult.OK)
            {
                PASSWORD = value;
                savePermissionConfig();
                MessageBox.Show("修改成功");
            }
        }
    }
}
