using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using inv_tax_common;
using System.Xml;
namespace Invtax.Winform
{
    public partial class frmFPDetailNew : XiaoCai.WinformUI.Forms.DialogFormW
    {
        string fpid = "";
        public string fpdm = "";
        public string fphm = "";
        public string kprq = "";
        public string kjje = "";
        public string jym = "";
        public string jqm = "";
        public string k2 = "";
        public string k3 = "";
        public string fplx = "";
        public string dq = ""; //发票所属地区
        string fpmc = "";     //发票所属地区
        string se = "";
        string je = "";
        string jshj = "";
        //购买方信息
        string gfmc = "";
        string gfsh = "";
        string gfdz = "";
        string gfzh = "";
        //销售方信息
        string xfmc = "";
        string xfsh = "";
        string xfdz = "";
        string xfzh = "";

        string checktime = "";
        string bz = "";
        string cycs = "";

        UserInfo tmpuserInfo = null;
        public frmFPDetailNew()
        {
            InitializeComponent();
            this.Text = inv_tax_common.Common.softName;
        }

        private void frmFPDetailNew_Load(object sender, EventArgs e)
        {

        }
        public bool getFPDetail(string mainId, UserInfo userInfo)
        {
            tmpuserInfo = userInfo;
            bool IsCanViewBZ = false;
            if (userInfo.Priority == "1")
            {
                IsCanViewBZ = true;
            }
            string result = inv_tax_common.Webservice.GetRecordById(userInfo.CompanyGuid, mainId);
            try
            {
                if (result == "") return false;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(result.ToString());
                XmlElement node = xmlDoc.DocumentElement;
                fpid = node["id"].InnerText;
                fpmc = node["fpmc"].InnerText;
                dq = node["dq"].InnerText;
                fpdm = node["fpdm"].InnerText;
                fphm = node["fphm"].InnerText;
                kprq = node["kprq"].InnerText; 
                je = node["je"].InnerText; 
                se = node["se"].InnerText; 
                jshj = node["jshj"].InnerText; 

                gfmc = node["gfmc"].InnerText;
                gfsh = node["gfsh"].InnerText;
                gfdz = node["gfdz"].InnerText; 
                gfzh = node["gfzh"].InnerText;
                xfmc = node["xfmc"].InnerText;
                xfsh = node["xfsh"].InnerText;
                xfdz = node["xfdz"].InnerText;
                xfzh = node["xfzh"].InnerText;

                jym = node["jym"].InnerText;
                jqm = node["jqm"].InnerText; 
                if (IsCanViewBZ)
                {
                    bz = node["remarks"].InnerText;
                }
                checktime = node["checktime"].InnerText;
                cycs = node["cycs"].InnerText;
                if (cycs != null)
                {
                    if (cycs == "0")
                    {
                        cycs = "1";
                    }
                }
                lblTT.Text = dq + fpmc;
                lblfpdm.Text = fpdm;
                lblfphm.Text = fphm;
                if (kprq.IndexOf('-') > 0)
                {
                    DateTime da = DateTime.ParseExact(kprq, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
                    lblkprq.Text = da.ToString("yyyy年MM月dd日");
                }
                else
                {
                    DateTime da = DateTime.ParseExact(kprq, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                    lblkprq.Text = da.ToString("yyyy年MM月dd日"); //开票日期
                }
                //购买方信息
                lblgmfmc.Text = gfmc;
                lblgmfsh.Text = gfsh;
                lblgmfdz.Text = gfdz;
                lblgmfzh.Text = gfzh;

                //销售方信息
                lblxsfmc.Text = xfmc;
                lblxsfsh.Text = xfsh;
                lblxsfdz.Text = xfdz;
                lblxsfzh.Text = xfzh;

                lbljym.Text = jym;
                lbljqm.Text = jqm;
                //判断是否有授权查看备注
                if (IsCanViewBZ)
                {
                    txtbz.Text = bz;
                    txtbz.Enabled = false;
                }
                lblCheckTime.Text = checktime;
                lblcycs.Text = "第" + cycs + "次";

                lblkjje.Text = "¥" + je;
                lblse.Text = "¥" + se.ToString();
                lbljshj.Text = "¥" + jshj.ToString();
                decimal d = decimal.Parse(jshj.ToString());
                lblDX.Text = CmycurD(d);

                DataTable dt = inv_tax_common.Webservice.GetRecordDetailsForDisplayByMainIDs(userInfo.CompanyGuid, fpid);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add();
                        dataGridView1.Rows[i].Cells["id"].Value = (i + 1).ToString();
                        dataGridView1.Rows[i].Cells["mc"].Value = dt.Rows[i]["mc"].ToString();
                        dataGridView1.Rows[i].Cells["gg"].Value = dt.Rows[i]["gg"];
                        dataGridView1.Rows[i].Cells["dw"].Value = dt.Rows[i]["dw"];
                        dataGridView1.Rows[i].Cells["amount"].Value = dt.Rows[i]["amount"];
                        dataGridView1.Rows[i].Cells["dj"].Value = dt.Rows[i]["dj"];
                        dataGridView1.Rows[i].Cells["dje"].Value = dt.Rows[i]["je"];
                        dataGridView1.Rows[i].Cells["sl"].Value = dt.Rows[i]["sl"].ToString() + "%";
                        dataGridView1.Rows[i].Cells["dse"].Value = dt.Rows[i]["se"];
                    }
                }


                dt.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {


            }
            return true;
        }
        public bool getFPDetail(DataGridViewRow dgvr, DataTable dt, UserInfo userInfo)
        {
            tmpuserInfo = userInfo;
            bool IsCanViewBZ = false;
            if (userInfo.Priority == "1")
            {
                IsCanViewBZ = true;
            }
            try
            {
                fpid = dgvr.Cells["id"].Value.ToString();
                fpmc = dgvr.Cells["fplx"].Value.ToString();
                dq = dgvr.Cells["dq"].Value.ToString();
                //fpmc = dgvr.Cells["fpmc"].Value.ToString();
                fpdm = dgvr.Cells["fpdm"].Value.ToString();
                fphm = dgvr.Cells["fphm"].Value.ToString();
                kprq = dgvr.Cells["kprq"].Value.ToString();
                je = dgvr.Cells["je"].Value.ToString();
                se = dgvr.Cells["se"].Value.ToString();
                jshj = dgvr.Cells["jshj"].Value.ToString();

                gfmc = dgvr.Cells["gfmc"].Value.ToString();
                gfsh = dgvr.Cells["gfsh"].Value.ToString();
                gfdz = dgvr.Cells["gfdz"].Value.ToString();
                gfzh = dgvr.Cells["gfzh"].Value.ToString();
                xfmc = dgvr.Cells["xfmc"].Value.ToString();
                xfsh = dgvr.Cells["xfsh"].Value.ToString();
                xfdz = dgvr.Cells["xfdz"].Value.ToString();
                xfzh = dgvr.Cells["xfzh"].Value.ToString();

                jym = dgvr.Cells["jym"].Value.ToString();
                jqm = dgvr.Cells["jqm"].Value.ToString();
                if (IsCanViewBZ)
                {
                    bz = dgvr.Cells["bz"].Value.ToString();
                }
                checktime = dgvr.Cells["checktime"].Value.ToString();
                cycs = dgvr.Cells["cycs"].Value.ToString();
                if (cycs != null)
                {
                    if (cycs == "0")
                    {
                        cycs = "1";
                    }
                }
                lblTT.Text = dq + fpmc;

                lblfpdm.Text = fpdm;
                lblfphm.Text = fphm;
                if (kprq.IndexOf('-') > 0)
                {
                    DateTime da = DateTime.ParseExact(kprq, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
                    lblkprq.Text = da.ToString("yyyy年MM月dd日");
                }
                else
                {
                    DateTime da = DateTime.ParseExact(kprq, "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
                    lblkprq.Text = da.ToString("yyyy年MM月dd日"); //开票日期
                }

                //购买方信息
                lblgmfmc.Text = gfmc;
                lblgmfsh.Text = gfsh;
                lblgmfdz.Text = gfdz;
                lblgmfzh.Text = gfzh;

                //销售方信息
                lblxsfmc.Text = xfmc;
                lblxsfsh.Text = xfsh;
                lblxsfdz.Text = xfdz;
                lblxsfzh.Text = xfzh;

                lbljym.Text = jym;
                lbljqm.Text = jqm;
                //判断是否有授权查看备注
                if (IsCanViewBZ)
                {
                    txtbz.Text = bz;
                    txtbz.Enabled = false ;
                }
                lblCheckTime.Text = checktime;
                lblcycs.Text = "第" + cycs + "次";

                lblkjje.Text = "¥" + je;
                lblse.Text = "¥" + se.ToString();
                lbljshj.Text = "¥" + jshj.ToString();
                decimal d = decimal.Parse(jshj.ToString());
                lblDX.Text = CmycurD(d);

                //DataTable dt = inv_tax_common.Webservice.GetRecordDetailsForDisplayByMainIDs("", id);

                if (dt != null && dt.Rows.Count > 0)
                {
                    dataGridView1.Rows.Clear();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dataGridView1.Rows.Add();
                        dataGridView1.Rows[i].Cells["id"].Value = (i+1).ToString();
                        dataGridView1.Rows[i].Cells["mc"].Value = dt.Rows[i]["mc"].ToString();
                        dataGridView1.Rows[i].Cells["gg"].Value = dt.Rows[i]["gg"];
                        dataGridView1.Rows[i].Cells["dw"].Value = dt.Rows[i]["dw"];
                        dataGridView1.Rows[i].Cells["amount"].Value = dt.Rows[i]["amount"];
                        dataGridView1.Rows[i].Cells["dj"].Value = dt.Rows[i]["dj"];
                        dataGridView1.Rows[i].Cells["dje"].Value = dt.Rows[i]["je"];
                        dataGridView1.Rows[i].Cells["sl"].Value = dt.Rows[i]["sl"].ToString() + "%";
                        dataGridView1.Rows[i].Cells["dse"].Value = dt.Rows[i]["se"];
                    }
                }


                dt.Dispose();

            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {


            }
            return true;
        }
        public string CmycurD(decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾圆角分"; //数字位所对应的汉字
            string str3 = "";    //从原num值中取出的值
            string str4 = "";    //数字的字符串形式
            string str5 = "";  //人民币大写金额形式
            int i;    //循环变量
            int j;    //num的值乘以100的字符串长度
            string ch1 = "";    //数字的汉语读法
            string ch2 = "";    //数字位的汉字读法
            int nzero = 0;  //用来计算连续的零值是几个
            int temp;            //从原num值中取出的值

            num = Math.Round(Math.Abs(num), 2);    //将num取绝对值并四舍五入取2位小数
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式
            j = str4.Length;      //找出最高位
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分

            //循环取出每一位需要转换的值
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值
                temp = Convert.ToInt32(str3);      //转换为数字
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整”
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零圆整";
            }
            return str5;
        }
        private void button2_Click(object sender, EventArgs e)
        {

        }
        private void btnOutput_Click(object sender, EventArgs e)
        {
            string filename = fpdm + "-" + fphm;

           _ToExcel2(dataGridView1, filename);
        }
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, this.dataGridView1.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
            this.dataGridView1.RowHeadersDefaultCellStyle.Font,
            rectangle, this.dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right);  
        }
        public void _ToExcel2(DataGridView myDGV, string filename)
        {
            string path = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "xls";
            saveDialog.Filter = "Excel97-2003 (*.xls)|*.xls|All Files (*.*)|*.*";
            saveDialog.FileName = filename;
            saveDialog.ShowDialog();
            path = saveDialog.FileName;
            if (path.IndexOf(":") < 0) return; //判断是否点击取消
            try
            {
                Thread.Sleep(1000);
                StreamWriter sw = new StreamWriter(path, false, Encoding.GetEncoding("gb2312"));
                StringBuilder sb = new StringBuilder();
                //写入标题
                for (int k = 0; k < myDGV.Columns.Count; k++)
                {
                    if (myDGV.Columns[k].Visible)//导出可见的标题
                    {
                        //"\t"就等于键盘上的Tab,加个"\t"的意思是: 填充完后进入下一个单元格.
                        sb.Append(myDGV.Columns[k].HeaderText.ToString().Trim() + "\t");
                    }
                }
                sb.Append(Environment.NewLine);//换行
                //写入每行数值
                for (int i = 0; i < myDGV.Rows.Count; i++)
                {
                    System.Windows.Forms.Application.DoEvents();
                    for (int j = 0; j < myDGV.Columns.Count; j++)
                    {
                        if (myDGV.Columns[j].Visible)//导出可见的单元格
                        {
                            //注意单元格有一定的字节数量限制,如果超出,就会出现两个单元格的内容是一模一样的.
                            //具体限制是多少字节,没有作深入研究.
                            if (myDGV.Rows[i].Cells[j].Value != null)
                                sb.Append(myDGV.Rows[i].Cells[j].Value.ToString().Trim() + "\t");
                            else
                                sb.Append("" + "\t");
                        }
                    }
                    sb.Append(Environment.NewLine); //换行
                }
                sw.Write(sb.ToString());
                sw.Flush();
                sw.Close();
                MessageBox.Show(path + "，导出成功", "系统提示", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public DataTable GetDataGridViewToDataTable(DataGridView dgv, bool selected)
        {
            DataTable dt = new DataTable();
            // 列强制转换
            for (int count = 0; count < dgv.Columns.Count; count++)
            {
                if (dgv.Columns[count].Visible == true)
                {
                    DataColumn dc = new DataColumn(dgv.Columns[count].HeaderText.ToString());
                    dt.Columns.Add(dc);
                }
            }
            // 循环行
            for (int count = 0; count < dgv.Rows.Count; count++)
            {
                if (selected)
                {
                    if (dgv.Rows[count].Selected == selected)
                    {
                        DataRow dr = dt.NewRow();
                        int isGetValueIndex = 0;
                        for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                        {
                            if (dgv.Columns[countsub].Visible == true)
                            {
                                dr[isGetValueIndex] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                                isGetValueIndex++;
                            }
                        }
                        dt.Rows.Add(dr);
                    }

                }
                else
                {
                    DataRow dr = dt.NewRow();
                    int isGetValueIndex = 0;
                    for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
                    {
                        if (dgv.Columns[countsub].Visible == true)
                        {
                            dr[isGetValueIndex] = Convert.ToString(dgv.Rows[count].Cells[countsub].Value);
                            isGetValueIndex++;
                        }
                    }
                    dt.Rows.Add(dr);
                }

            }
            return dt;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void prgTitle_CloseClick(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrinter_Click(object sender, EventArgs e)
        {
            try
            {
                string djhm = fpid.ToString();
                if (djhm != null)
                {
                    djhm = djhm + ";";
                    if (djhm.Contains(";"))
                    {
                        Frm_PrintFP _Frm_PrintFP = new Frm_PrintFP(tmpuserInfo);
                        _Frm_PrintFP.djhm = djhm.ToString().Substring(0, djhm.ToString().Length - 1);
                        _Frm_PrintFP.mbName = "发票信息";
                        if (_Frm_PrintFP.ShowDialog(this) == DialogResult.OK)
                        {
                        }
                    }
                }
            }
            catch (Exception exception1)
            {

            }
        }
    }
}
