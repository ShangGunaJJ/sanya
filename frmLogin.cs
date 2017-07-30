using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XiaoCai.WinformUI;
using XiaoCai.WinformUI.Forms;
using System.IO;
using System.Xml;
using inv_tax_common;

namespace Invtax.Winform
{
    public partial class frmLogin : XiaoCai.WinformUI.Forms.DialogFormW
    {
        string username = "";
        string password = "";
        UserInfo ui = null;
        inv_tax_common.IniFileRW iniFile = new inv_tax_common.IniFileRW();
        string strLogin = "0";//判断是否记录用户名和密码
        public frmLogin()
        {
            InitializeComponent();
            this.Text = inv_tax_common.Common.softName;
            //this.prgTitle.Text = inv_tax_common.Common.softName;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           //string dbFileString = iniFile.ReadValue("SystemConfig", "DatabaseFile");
           //if (!File.Exists(dbFileString))
           //{
           //    openDBSetting();
           //}
           strLogin = iniFile.ReadValue("UserInfo", "AutoLogin");
           if (strLogin == "1")
           {
               username = iniFile.ReadValue("UserInfo", "UserName");
               password = iniFile.ReadValue("UserInfo", "Password");
               txtusername.Text =username =inv_tax_common.EncrptStr.Decode(username);
               txtpassword.Text =password = inv_tax_common.EncrptStr.Decode(password);
               
               cbAutoLogin.Checked = true;
           }
        }

        private void buttonW2_Click(object sender, EventArgs e)
        {
            this.Close();
        }     

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblMsg.Text = "";
            lblMsg.ForeColor = Color.Red;
            if (txtusername.Text == "" || txtpassword.Text.Trim() == "")
            {
                lblMsg.Text = "请输入用户名和密码";
                return;
            }
            string encodePassword = inv_tax_common.EncrptStr.Encode(txtpassword.Text.Trim());
            //string usedpages = inv_tax_common.Webservice.GetUsedPages().ToString();
            //ui = Webservice.Login(txtusername.Text.Trim(), encodePassword,"1",usedpages);
            ui = Webservice.Login(txtusername.Text.Trim(), encodePassword);

            if (ui.Logined)
            {
                if (ui.UsedPages + ui.ExpireAlertPages >= ui.TotlePages)
                {
                    MessageBox.Show("系统授权发票查验量仅剩余:" + (ui.TotlePages - ui.UsedPages).ToString() + "张,请尽快联系软件服务商！");
                }
                //查询该用户所在公司的过期时间
                int day = DateTime.Compare(DateTime.Now, DateTime.Parse(ui.ExpireDate));
                if (day >= 0)
                {
                    MessageBox.Show(ui.CompanyName + " 有效授权时间期为：" + ui.ExpireDate + ", 使用已过期，请联系软件服务商！");
                    return;
                }

                //判断是否为初始化密码，强制要求更改密码
                if (txtpassword.Text.Trim() == "000000")
                {
                    MessageBox.Show("您当前使用的是系统初始化密码，请更改密码！");
                    frmUserUpdatePassword fuup = new frmUserUpdatePassword(ui);
                    fuup.ShowDialog();
                }
                if (cbAutoLogin.Checked)
                {
                    if (username != txtusername.Text.Trim() || password != txtpassword.Text.Trim())
                    {
                        iniFile.Write("UserInfo", "AutoLogin", "1");
                        iniFile.Write("UserInfo", "UserName", inv_tax_common.EncrptStr.Encode(txtusername.Text.Trim()));
                        iniFile.Write("UserInfo", "Password", inv_tax_common.EncrptStr.Encode(txtpassword.Text.Trim()));
                    }
                }
                else
                {
                    if (strLogin!="0")
                        iniFile.Write("UserInfo", "AutoLogin", "0");
                }
                //inv_tax_common.Common.OpenInvtaxService();
                frmMDIMain fmf = new frmMDIMain(ui);
                this.Hide();
                fmf.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show(ui.Message,"系统登录失败",MessageBoxButtons.OK,MessageBoxIcon.Information);
                return;
            }
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openDBSetting();

            //frmServerSetting fss = new frmServerSetting();
            //fss.ShowDialog();
            //if (fss.urlFlag)
            //    this.Close();
        }

        private void openDBSetting()
        {
            frmFPDBSetting fdb = new frmFPDBSetting();
            fdb.ShowDialog();
            if (fdb.urlFlag)
                this.Close();
        }
        private void txtpassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnLogin_Click(sender, e);
            }
        }
    }
}
