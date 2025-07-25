using RobotApp.Dao;
using RobotApp.IM;
using RobotApp.Model;
using RobotApp.Util;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace RobotApp.MiniDialog
{
    [SupportedOSPlatform("windows")]
    public partial class ManualOpenForm : Form
    {
        public ManualOpenForm()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            string issue = txtIssue.Text;
            string code1 = txtOpenCode1.Text;
            string code2 = txtOpenCode2.Text;
            string code3 = txtOpenCode3.Text;
            if (issue == "")
            {
                MessageBox.Show("期号不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(code1) || string.IsNullOrEmpty(code2) || string.IsNullOrEmpty(code3))
            {
                MessageBox.Show("开奖号码不能为空！");
                return;
            }
            Result result = RobotClient.ResultList.FirstOrDefault(r => r.Issue == Convert.ToInt32(issue));
            if (result == null)
            {
                MessageBox.Show("期号不存在或无任何下注！");
                return;
            }
            if (result.Status > ResultStatus.已开奖)
            {
                MessageBox.Show("本期已正常结算，无须手动开奖！");
                return;
            }
            if (result.Status <= ResultStatus.无开盘)
            {
                MessageBox.Show("本期无开盘下注或已作废");
                return;
            }
            if (result.Status == ResultStatus.竞猜中)
            {
                BettingUtil.ProcessClosing(result);
            }
            result.Num1 = int.Parse(code1);
            result.Num2 = int.Parse(code2);
            result.Num3 = int.Parse(code3);
            result.Sum = result.Num1+result.Num2+result.Num3;
            result.Status = ResultStatus.已开奖;
            ResultDao.Save(result);
            
            BettingUtil.ProcessSettlement(result, RobotClient.UserList.ToList());
            RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_开奖, result);
            RobotClient.SendConfigMessage(Config.GetInstance().编辑框_消息_账单, result);
            MessageBox.Show(string.Format("手动开奖成功, {0}期输赢:{1}", result.Issue, result.WinOrLose));
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ManualOpenForm_Load(object sender, EventArgs e)
        {
            txtIssue.Text = Convert.ToString(RobotClient.CurrentResult.Issue);
            txtOpenCode1.Text = Convert.ToString(RobotClient.CurrentResult.Num1);
            txtOpenCode2.Text = Convert.ToString(RobotClient.CurrentResult.Num2);
            txtOpenCode3.Text = Convert.ToString(RobotClient.CurrentResult.Num3);
        }
    }
}
