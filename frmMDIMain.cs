using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using inv_tax_common;

namespace Invtax.Winform
{
    public partial class frmMDIMain : RibbonForm
    {
        int totlePages = 0;
        int usedPages = 0;
        UserInfo userInfo = null;
        frmFPList fpList = null;
        frmFPcollect fpc = null;
        frmScan fScan = null;
        frmFPGXQR gxqr = null;
        //frmWorkStation fws = null;

        public frmMDIMain()
        {
            InitializeComponent();
        }
        public frmMDIMain(UserInfo us)
        {
            InitializeComponent();
            userInfo = us;
            if (userInfo.Logined)
            {
                RefreshLicencesInfo();
            }
            this.Text = inv_tax_common.Common.softName;
        }
        private void RefreshLicencesInfo()
        {
            this.tsslCompanyName.Text = userInfo.CompanyName;
            this.tssLabSH.Text = userInfo.SH;
            totlePages = userInfo.TotlePages;
            usedPages = userInfo.UsedPages;

            tsslUser.Text = userInfo.UserName + "  ";
            tsslTotlePages.Text = totlePages.ToString() + "  ";
            tsslUsedPages.Text = (totlePages - usedPages).ToString() + "  ";

            if (totlePages - usedPages < userInfo.ExpireAlertPages)
                tsslUsedPages.ForeColor = Color.Red;
        }

        private void frmMDIMain_Load(object sender, EventArgs e)
        {
            //判断操作权限，1为普通用户，只可以操作工作台；2为操作员，只可查看本人上传数据，无删除权限；3为管理员，可查看公司所有的发票，有所有权限
            switch (userInfo.Privilege)
            {
                case 2:
                    rbBtnDeleteData.Enabled = false;
                    break;
                case 3:
                    break;
                default:
                    ribbon1.Tabs[0].Visible = true;
                    ribbon1.Tabs[1].Visible = false;
                    break;
            }
            if (userInfo.Privilege != 3)
                ribbonPanel11.Visible = false; //没有管理员权限不显示敏感字设置
            if (userInfo.Mode == "1")
            {
                btnChangException.Visible = false;
                ribbonSeparator1.Visible = false;
            }

            ribbon1_ActiveTabChanged(sender, e);

        }
        private void rbBtnSaveToDataCenter_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                if (fpList.CurrentFormName == "Workstation")
                    fpList.MoveToDataCenter();
            }
        }

        private bool checkOpenChildForm(string formname)
        {
            bool f = false;
            foreach (Form childrenForm in this.MdiChildren)
            {
                //检测是不是当前子窗体名称
                if (childrenForm.Name == formname)
                {
                    //是的话就是把他显示   
                    childrenForm.Visible = true;
                    //并激活该窗体  
                    childrenForm.Activate();
                    childrenForm.WindowState = FormWindowState.Maximized;
                    return !f;
                }
            }
            return f;
        }

        private bool checkCurrentDisplayChildForm(string formname)
        {
            bool f = false;
            foreach (Form childrenForm in this.MdiChildren)
            {
                //检测是不是当前子窗体名称
                if (childrenForm.Name == formname)
                {
                    f= !f;
                    break;
                }
            }
            return f;
        }

        private void rbOutputSelectedFPXX_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.OutputSelectedFPXX();
            }
        }

        private void rbOutputSearchFPXX_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.OutputSearchFPXX();
            }
        }

        private void rbBarcodeCollect_Click(object sender, EventArgs e)
        {
            if (checkHavePage())
            {
                if (!checkOpenChildForm("frmFPcollect"))
                {
                    fpc = new frmFPcollect(userInfo);
                    fpc.changedUsedPage += new frmFPcollect.ChildFromHandle(ChildFormChangLicencesInfo);
                    fpc.WindowState = FormWindowState.Normal;
                    fpc.ShowDialog();
                }
            }
        }

        private void rbViewRepeatLog_Click(object sender, EventArgs e)
        {
            frmRepeatLog frl = new frmRepeatLog(userInfo);
            frl.ShowDialog();
        }

        private void ribbonButton22_Click(object sender, EventArgs e)
        {
            rbOutputSelectedFPXX_Click(sender, e);
        }

        private void ribbonButton24_Click(object sender, EventArgs e)
        {
            rbOutputSearchFPXX_Click(sender, e);
        }

        private void rbOutputSelectedToGxExcelFile_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.OutputSelectedToGxExcel();
            }

        }

        private void rbBtnImportExcel_Click(object sender, EventArgs e)
        {
            if (checkHavePage())
            {
                frmFPImportExcel frmImportexcel = new frmFPImportExcel(userInfo);
                frmImportexcel.Show();
                if (userInfo.UsedPages != frmImportexcel.userInfo.UsedPages)
                {
                    userInfo.UsedPages = frmImportexcel.userInfo.UsedPages;
                    RefreshLicencesInfo();

                    if (checkCurrentDisplayChildForm("frmFPList"))
                    {
                        fpList.RefreshAll();
                    }
                }
            }
        }

        private bool checkHavePage()
        {
            bool flag = true;
            if (userInfo.UsedPages >= userInfo.TotlePages)
            {
                MessageBox.Show("发票查验授权数据已用完！请联系软件服务商增加授权数量！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                flag = false;
            }
            return flag;

        }

        private void rbBtnScan_Click(object sender, EventArgs e)
        {
            if (checkHavePage())
            {
                if (!checkOpenChildForm("frmScan"))
                {
                    fScan = new frmScan(userInfo, "01", "scan");
                    fScan.changedUsedPage += new frmScan.ChildFromHandle(ChildFormChangLicencesInfo);
                    fScan.MdiParent = this;
                    fScan.WindowState = FormWindowState.Maximized;
                    fScan.Show();
                    //rbBtnRefresh_Click(sender, e);
                    //if (userInfo.UsedPages != fScan.userInfo.UsedPages)
                    //{
                    //    userInfo.UsedPages = fScan.userInfo.UsedPages;
                    //    RefreshLicencesInfo();
                    //    rbBtnRefresh_Click(sender, e);
                    //}
                }
                else
                {
                    MessageBox.Show("请先关闭当前正在进行的采集窗口！");
                }
            }
        }

        //改变值的事件  
        private void ChildFormChangLicencesInfo(int usedPages)
        {
            //此处是给原有窗体中控件赋值  
            userInfo.UsedPages = usedPages;

            RefreshLicencesInfo();

            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.RefreshAll();
            }
        }

        private void rbBtnImportPDFFile_Click(object sender, EventArgs e)
        {
            if (checkHavePage())
            {
                if (!checkOpenChildForm("frmScan"))
                {
                    fScan = new frmScan(userInfo, "", "");
                    fScan.changedUsedPage += new frmScan.ChildFromHandle(ChildFormChangLicencesInfo);
                    fScan.MdiParent = this;
                    fScan.WindowState = FormWindowState.Maximized;
                    fScan.Show();
                    
                    //if (userInfo.UsedPages != fScan.userInfo.UsedPages)
                    //{
                    //    userInfo.UsedPages = fScan.userInfo.UsedPages;
                    //    RefreshLicencesInfo();
                    //    rbBtnRefresh_Click(sender, e);
                    //}
                }
                else
                {
                    MessageBox.Show("请先关闭当前正在进行的采集窗口！");
                }
            }
        }

        private void ribbonButton20_Click(object sender, EventArgs e)
        {
            if (checkHavePage())
            {
                if (!checkOpenChildForm("frmScan"))
                {
                    fScan = new frmScan(userInfo, "", "scan");
                    fScan.changedUsedPage += new frmScan.ChildFromHandle(ChildFormChangLicencesInfo);
                    fScan.MdiParent = this;
                    fScan.WindowState = FormWindowState.Maximized;
                    fScan.Show();
                    //rbBtnRefresh_Click(sender, e);
                    //if (userInfo.UsedPages != fScan.userInfo.UsedPages)
                    //{
                    //    userInfo.UsedPages = fScan.userInfo.UsedPages;
                    //    RefreshLicencesInfo();
                    //    rbBtnRefresh_Click(sender, e);
                    //}
                }
                else
                {
                    MessageBox.Show("请先关闭当前正在进行的采集窗口！");
                }
            }
        }

        private void rbBtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rbBtnEditItem_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                if (fpList.CurrentFormName == "Workstation")
                    fpList.UpdateSelectedItem();
            }
        }
        private void rbBtnDeleteSelectedItems_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                //if (fpList.CurrentFormName == "Workstation")
                    fpList.deleteSelectedItems();
            }
        }

        private void rbAllData_Click(object sender, EventArgs e)
        {
            if (!checkOpenChildForm("frmFPList"))
            {
                fpList = new frmFPList(userInfo, "DataCenter");
                fpList.changedUsedPage += new frmFPList.ChildFromHandle(ChildFormChangLicencesInfo);
                fpList.MdiParent = this;
                fpList.WindowState = FormWindowState.Maximized;
                fpList.Show();
                
            }
        }

        private void rbBtnReCheck_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.reCheckFP();
            }
        }

        private void rbBtnDeleteData_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.deleteSelectedItems();
            }
        }

        private void rbBtnSearch_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.CallSearchForm();
            }
        }

        private void rbBtnRefresh_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.RefreshAll();
            }
        }

        private void rbBtnOpenWorkstation_Click(object sender, EventArgs e)
        {
            if (!checkOpenChildForm("frmFPList"))
            {
                fpList = new frmFPList(userInfo, "Workstation");
                fpList.changedUsedPage += new frmFPList.ChildFromHandle(ChildFormChangLicencesInfo);
                fpList.MdiParent = this;
                fpList.WindowState = FormWindowState.Maximized;
                fpList.Show();

            }
            else
            {
                if (fpList.CurrentFormName == "Workstation")
                    return;
                else
                {
                    MessageBox.Show("请先关闭发票仓库窗体！");
                    return;
                }
            }
        }

        private void rbBtnExitWorkStation_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                if (fpList.CurrentFormName == "Workstation")
                {
                    fpList.Close();
                }
            }
        }

        private void rbBtnExitDataCenter_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                if (fpList.CurrentFormName == "DataCenter")
                {
                    fpList.Close();
                }
            }
        }

        

        public void CallChangFPException(string statusname)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                if (fpList.CurrentFormName == "Workstation")
                    fpList.ChangFPException(statusname);
            }
        }

        private void ribbonButton23_Click(object sender, EventArgs e)
        {
            CallChangFPException("正常");
        }
        private void ribbonButton24_Click_1(object sender, EventArgs e)
        {
            CallChangFPException("错票");
        }

        private void ribbonButton25_Click(object sender, EventArgs e)
        {
            CallChangFPException("假票");
        }

        private void ribbonButton26_Click(object sender, EventArgs e)
        {
            CallChangFPException("敏感票");
        }



        private void ribbon1_ActiveTabChanged(object sender, EventArgs e)
        {
            if (ribbon1.ActiveTab.Text == "工作台")
            {
                rbBtnExitDataCenter_Click(sender, e);//关闭发票仓库
                rbBtnOpenWorkstation_Click(sender, e); //打开工作台
            }
            if (ribbon1.ActiveTab.Text == "发票仓库")
            {
                rbBtnExitWorkStation_Click(sender, e);//关闭工作台；
                rbAllData_Click(sender, e); //打开发票仓库
            }
            if (ribbon1.ActiveTab.Text == "发票勾选")
            {
                if (checkCurrentDisplayChildForm("frmFPList"))
                {
                    fpList.Close();
                }
            }
            if (ribbon1.ActiveTab.Text == "系统设置")
            {
                if (checkCurrentDisplayChildForm("frmFPList"))
                {
                    fpList.Close();
                }
            }
        }

        private void rbBtnChangPassword_Click(object sender, EventArgs e)
        {
            frmUserUpdatePassword fuup = new frmUserUpdatePassword(userInfo);
            fuup.ShowDialog();
        }

        private void rbBtnTryKeyword_Click(object sender, EventArgs e)
        {
            frmTryKeyword ftk = new frmTryKeyword(userInfo.KeyWord,userInfo.CompanyGuid);
            ftk.ShowDialog();
            if (ftk.isUpdate)
                userInfo.KeyWord = ftk.TryKeyword;
        }

        private void rbtnPLGX_Click(object sender, EventArgs e)
        {
            if (!checkOpenChildForm("frmFPPLGX"))
            {
                frmFPPLGX plgx = new frmFPPLGX(userInfo);
                plgx.changedUsedPage += new frmFPPLGX.ChildFromHandle(ChildFormChangLicencesInfo);
                plgx.MdiParent = this;
                plgx.WindowState = FormWindowState.Maximized;
                plgx.Show();
            }
        }

        private void rBtnGXQR_Click(object sender, EventArgs e)
        {
             
             if (!checkOpenChildForm("frmFPGXQR"))
             {
                 gxqr = new frmFPGXQR();
                 gxqr.MdiParent = this;
                 gxqr.WindowState = FormWindowState.Maximized;
                 gxqr.Show();
             }
             else
                 gxqr.Close();
        }

        private void rbtnHistroyData_Click(object sender, EventArgs e)
        {
            if (!checkOpenChildForm("frmFPRZHistroyData"))
            {
                frmFPRZHistroyData gxqr = new frmFPRZHistroyData(userInfo);
                gxqr.changedUsedPage += new frmFPRZHistroyData.ChildFromHandle(ChildFormChangLicencesInfo);
                gxqr.MdiParent = this;
                gxqr.WindowState = FormWindowState.Maximized;
                gxqr.Show();
            }
        }
        private void rbtnSPSetting_Click(object sender, EventArgs e)
        {
            frmSKPSetting skp = new frmSKPSetting();
            skp.ShowDialog();
        }

        private void ribbonButton10_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.Print_FPXX();
            }
        }

        private void ribbonButton11_Click(object sender, EventArgs e)
        {
            if (checkCurrentDisplayChildForm("frmFPList"))
            {
                fpList.Print_FPList();
            }
        }

        private void frmMDIMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill(); 
        }

        private void frmMDIMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("您确认退出本系统吗？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void frmMDIMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {

                // this .StartPosition = FormStartPosition.CenterParent ;
                this.Height = Screen.PrimaryScreen.Bounds.Height / 2;
                this.Width = Screen.PrimaryScreen.Bounds.Width / 2;
                this.Location = new Point(Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);
            }
        }
    }
}
