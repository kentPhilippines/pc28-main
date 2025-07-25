using ImLibrary.IM;
using System.Runtime.Versioning;
using System.Windows.Forms;
using System;
namespace DummyApp;

[SupportedOSPlatform("windows")]
public partial class LoginForm : Form
{
    public LoginForm()
    {
        InitializeComponent();
    }

    private void btnLogin_Click(object sender, EventArgs e)
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

        try
        {
            CentralApi.Login(txtUserName.Text, txtPassword.Text);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void LoginForm_Load(object sender, EventArgs e)
    {
#if DEBUG
        //txtUserName.Text = "lexun";
        //txtPassword.Text = "123456";
#endif
        if (txtUserName.Text != "" && txtPassword.Text != "") btnLogin.PerformClick();
    }
}