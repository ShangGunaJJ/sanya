using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XiaoCai.WinformUI;
using inv_tax_common;
using System.Globalization;
using System.Xml;
namespace Invtax.Winform
{
    public partial class frmFPList : Form
    {
        public delegate void ChildFromHandle(int usedpage);
        public event ChildFromHandle changedUsedPage;

        inv_tax_common.IniFileRW iniFile = new IniFileRW();
        public string CurrentFormName = "";
        string IsDataCenter = "1";
        string guid = "";
        string lastSearchStauts = "";
        string name = "";
        string loginuser = "";
        int totlePages = 0;
        int usedPages = 0;
        double zje = 0;//总金额
        double zse = 0;//总税额
        double jshjze = 0;//价税合计总额
        public System.Data.DataTable temp = null;     //临时表
        int _pageCount = 0;      //总分页数
        int _currentPage = 1;    //当前页数
        int _itemsCount = 20;    //每页的数量
        int rowNumber = 0;
        int iStart = 0;
        int iEnd = 0;
        string priority = ""; //判断是否有权限查看备注栏
        bool IsCanViewBZ = false;
        int selectrowid = 0;
        string SearchSql = "";
        UserInfo userInfo = null;
        DataTable dt = null;
        string userPrivilegeSearch = "";
        bool isSelectAll = false;
        private int currentRow = -1;
        ComboBox dgv_exception;

        int AutoRefresh = 0;
        int AutoRefreshTimeTick = 0;
        public frmFPList(UserInfo us,string formName)
        {
            InitializeComponent();
            this.dataGridView1.DataError += delegate(object sender, DataGridViewDataErrorEventArgs e) { };
            userInfo = us;
            guid = userInfo.CompanyGuid;

            //判断是否有权限查看备注字段
            if (userInfo.Priority == "1")
            {
                IsCanViewBZ = true;
                dataGridView1.Columns["bz"].Visible = true;
            }
            else
            {
                IsCanViewBZ = false;
                dataGridView1.Columns["bz"].Visible = false;
            }
            //判断公司模式显示异常字段，0 企业模式；1 会计师事务所
            if (userInfo.Mode == "1")
                dataGridView1.Columns["exception"].Visible = false;
            CurrentFormName = formName;
            if (CurrentFormName == "DataCenter")
            {
                this.Text = "发票仓库";
                IsDataCenter = "1";
                if (userInfo.Privilege == 3)
                    tsmDeleteSelecteItem.Visible = true;
                else
                    tsmDeleteSelecteItem.Visible = false;
                tmsEditSelectItem.Visible = false;
                tsmMoveToDatacenter.Visible = false;
                dataGridView1.Columns["exception"].ReadOnly = true;
            }
            else
            {
                this.Text = "工作台";
                IsDataCenter = "0";
            }

            if(!isSelectAll)
                dataGridView1.Columns["selected"].HeaderText = "全选";
            else
               dataGridView1.Columns["selected"].HeaderText = "取消";
            

            tsslzje.Text = "-";
            tsslzse.Text = "-";
            tssljshj.Text = "-";
            tscbfplx.Text = "全部";
            tscbcyzt.Text = "全部";

            //权限权限判断获取数据，1为普通用户 2为财务专员，只看自己的数据；3是管理员，看全部数据
            if (userInfo.Privilege == 3)
                userPrivilegeSearch = " flag=" + IsDataCenter + " ";
            else
                userPrivilegeSearch = " createuser ='" + userInfo.UserName + "' and flag=" + IsDataCenter + " ";
            _currentPage = 1;
            SearchSql = userPrivilegeSearch;

        }

        private void frmFPList_Load(object sender, EventArgs e)
        {
            try
            {
                if (CurrentFormName != "DataCenter")
                {
                    timer2.Interval = 100;
                    timer2.Start();

                    AutoRefresh = int.Parse(iniFile.ReadValue("SystemConfig", "AutoRefresh"));
                    AutoRefreshTimeTick = int.Parse(iniFile.ReadValue("SystemConfig", "AutoRefreshTimeTick"));
                    if (AutoRefresh == 0)
                    {
                        timer1.Interval = AutoRefreshTimeTick;
                        timer1.Start();
                    }
                    else
                    {
                        timer1.Stop();
                    }
                }
                else
                {
                    timer1.Stop();
                }
                //if (temp == null) { tsbtnSearch_Click(null, null); }
            }
            catch { }
            
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            Application.DoEvents();
            RefreshAll();
            timer2.Stop();
        }
        public void RefreshAll()
        {
            ////权限权限判断获取数据，2为普通用户，只看自己的数据；3是管理员，看全部数据
            //if(userInfo.Privilege ==2)
            //    userPrivilegeSearch = " createuser ='" + userInfo.UserName + "' and flag=" + IsDataCenter + " ";
            //if (userInfo.Privilege == 3)
            //    userPrivilegeSearch = " flag=" + IsDataCenter + " ";
            //_currentPage = 1;
            //SearchSql = userPrivilegeSearch;
            if (CurrentFormName != "DataCenter")
            {
                string keyword = "";
               // keyword = iniFile.ReadValue("SystemConfig", "TryKeyword");
                keyword = userInfo.KeyWord;
                if (keyword != "")
                {
                    string tmp = inv_tax_common.Webservice.TryKeyword(keyword, userInfo.CompanyGuid);
                }
            }
            GetSearchData(SearchSql,true);
            int days = System.DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); 
        }
        private void GetSearchData(string sql,bool needRefreshPager)
        {
            //lastSearchStauts = "查询结果";
            tssljshj.Text = "-";
            tsslzje.Text = "-";
            tsslzse.Text = "-";
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            if (needRefreshPager)
            {
                rowNumber = inv_tax_common.Webservice.GetSearchRecordsCountBySql(guid, sql);
                if (rowNumber > 0)
                {
                    _pageCount = rowNumber / _itemsCount;
                    if (rowNumber % _itemsCount != 0)
                    {
                        _pageCount++;
                    }
                    // pager1 = new Pager();

                    labelW1.Text = "共"+rowNumber.ToString()+"条记录";
                    labelW2.Text = "共计：" + _pageCount + "页";

                    pager1.CurrentPage = _currentPage;
                    pager1.PageSize = _itemsCount;
                    pager1.IsShowGroup = false;
                    pager1.GroupSize = _pageCount;
                    pager1.RecordCount = rowNumber;
                    pager1.Refresh();

                    
                }
            }
            if (rowNumber > 0)
            {
                temp = inv_tax_common.Webservice.GetSearchRecords(guid, sql, _currentPage, _itemsCount);
                LoadContents(temp);
            }
            
        }
        private void getFPListByStauts(string stauts)
        {

            lastSearchStauts = stauts;
            tssljshj.Text = "-";
            tsslzje.Text = "-";
            tsslzse.Text = "-";
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();

            rowNumber = inv_tax_common.Webservice.GetRecordsCountByStauts(guid, stauts);
            if (rowNumber > 0)
            {
                temp = inv_tax_common.Webservice.GetRecords(guid, stauts, _currentPage, _itemsCount);
                LoadContents(temp);
            }
            _pageCount = rowNumber / _itemsCount;
            if (rowNumber % _itemsCount != 0)
            {
                _pageCount++;
            }
        }
        private void LoadContents(DataTable dt)
        {
            zje = 0;//总金额
            zse = 0;//总税额
            jshjze = 0;
           
            dataGridView1.Rows.Clear();
            if (dt == null || dt.Rows.Count == 0) return;
            for (int n = 0; n < dt.Rows.Count; n++)
            {
                int i = n;
                dataGridView1.Rows.Add();
                dataGridView1.Rows[n].Cells["yhid"].Value = ((_currentPage - 1) * _itemsCount + n + 1).ToString();
                dataGridView1.Rows[n].Cells["id"].Value = dt.Rows[i]["id"].ToString();

                string stauts = dt.Rows[i]["checkstauts"].ToString();
                dataGridView1.Rows[n].Cells["stauts"].Value = stauts;
                if (stauts != "一致" && stauts!="待查验")
                    dataGridView1.Rows[n].Cells["stauts"].Style.ForeColor = Color.Red;

                string exception = dt.Rows[i]["Exception"].ToString();
                dataGridView1.Rows[n].Cells["exception"].Value = exception;
                string dq = dt.Rows[i]["dq"].ToString();
                if (dq == "")
                {
                    dq = inv_tax_common.Common.getFPDQ(dt.Rows[i]["fpdm"].ToString());
                }
                dataGridView1.Rows[n].Cells["dq"].Value = dq;
                
                string fplx = dt.Rows[i]["fplx"].ToString();
                dataGridView1.Rows[n].Cells["fplx"].Value = inv_tax_common.Common.getFPLXName(fplx);
                dataGridView1.Rows[n].Cells["fpdm"].Value = dt.Rows[i]["fpdm"];
                dataGridView1.Rows[n].Cells["fphm"].Value = dt.Rows[i]["fphm"];
                string kprq = dt.Rows[i]["kprq"].ToString();
                if (kprq.IndexOf('-') > 0)
                {
                    kprq = kprq.Substring(0, 10);
                }
                else
                {
                    kprq = kprq.Substring(0, 8);
                }

                dataGridView1.Rows[n].Cells["kprq"].Value = kprq;
                string je = dt.Rows[i]["je"].ToString();
                if (je == "0")
                    je = "";
                dataGridView1.Rows[n].Cells["je"].Value = je ;
                
                dataGridView1.Rows[n].Cells["se"].Value = dt.Rows[i]["se"];
                dataGridView1.Rows[n].Cells["jshj"].Value = dt.Rows[i]["jshj"];
                try
                {
                    if (dt.Rows[i]["je"].ToString() != "")
                    {
                        zje += double.Parse(dt.Rows[i]["je"].ToString());
                        tsslzje.Text = zje.ToString();
                    }
                    if (dt.Rows[i]["se"].ToString() != "")
                    {
                        zse += double.Parse(dt.Rows[i]["se"].ToString());
                        tsslzse.Text = zse.ToString();
                    }
                    if (dt.Rows[i]["jshj"].ToString() != "")
                    {
                        jshjze += double.Parse(dt.Rows[i]["jshj"].ToString());
                        tssljshj.Text = jshjze.ToString();
                    }
                }
                catch
                { }

                string gfmc = dt.Rows[i]["gfmc"].ToString();
                string gfsh = dt.Rows[i]["gfsh"].ToString();
                
                dataGridView1.Rows[n].Cells["gfmc"].Value = gfmc;
                dataGridView1.Rows[n].Cells["gfsh"].Value = gfsh;
                bool isErrorFP = false;

                
                DataGridViewComboBoxCell comboCell = (DataGridViewComboBoxCell)dataGridView1.Rows[n].Cells["exception"];
                if (CurrentFormName == "DataCenter")
                {
                    comboCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                    comboCell.ReadOnly = true;
                }

                if (dt.Rows[i]["checkstauts"].ToString() != "待查验" && dt.Rows[i]["checkstauts"].ToString() != "次日再查" && dt.Rows[i]["checkstauts"].ToString() != "查无此票")
                {
                    if (exception == "")
                    {

                        if (userInfo.SH != gfsh)
                        {
                            exception = "错票";
                        }


                        if (userInfo.CompanyName != gfmc)
                        {
                            exception = "错票";
                        }
                    }

                    if (exception == "" || exception == "正常")
                        comboCell.Value = "正常";
                    else
                    {
                        comboCell.Value = exception;
                        comboCell.Style.ForeColor = Color.Red;
                        //dataGridView1.Rows[n].Cells["exception"].Style.ForeColor = Color.Red;
                    }



                    if (userInfo.SH != gfsh)
                    {
                        dataGridView1.Rows[n].Cells["gfsh"].Style.ForeColor = Color.Red;
                        dataGridView1.Rows[n].Cells["gfsh"].ReadOnly = true;
                    }

                    if (userInfo.CompanyName != gfmc)
                    {
                        dataGridView1.Rows[n].Cells["gfmc"].Style.ForeColor = Color.Red;
                        dataGridView1.Rows[n].Cells["gfmc"].ReadOnly = true;
                    }
                }
                dataGridView1.Rows[n].Cells["gfdz"].Value = dt.Rows[i]["gfdz"];
                dataGridView1.Rows[n].Cells["gfzh"].Value = dt.Rows[i]["gfzh"];
                dataGridView1.Rows[n].Cells["xfmc"].Value = dt.Rows[i]["xfmc"];
                dataGridView1.Rows[n].Cells["xfsh"].Value = dt.Rows[i]["xfsh"];
                dataGridView1.Rows[n].Cells["xfdz"].Value = dt.Rows[i]["xfdz"];
                dataGridView1.Rows[n].Cells["xfzh"].Value = dt.Rows[i]["xfzh"];
                dataGridView1.Rows[n].Cells["createuser"].Value = dt.Rows[i]["createuser"];
                dataGridView1.Rows[n].Cells["createtime"].Value = dt.Rows[i]["createtime"];
                dataGridView1.Rows[n].Cells["checktime"].Value = dt.Rows[i]["checktime"];
                dataGridView1.Rows[n].Cells["jym"].Value = dt.Rows[i]["jym"];
                dataGridView1.Rows[n].Cells["jqm"].Value = dt.Rows[i]["jqm"];
                dataGridView1.Rows[n].Cells["cycs"].Value = dt.Rows[i]["cycs"];
                dataGridView1.Rows[n].Cells["bz"].Value = dt.Rows[i]["remarks"];
                
            }
            AutoSizeColumn();
            dataGridView1.Refresh();
        }
        /// <summary> 让表格列宽自动适应
        /// 
        /// </summary>
        private void AutoSizeColumn()
        {
            //冻结某列 从左开始 0，1，2
            dataGridView1.Columns[1].Frozen = true;
            dataGridView1.Columns[2].Frozen = true;
            dataGridView1.Columns[3].Frozen = true;
            dataGridView1.Columns[4].Frozen = true;
            dataGridView1.Columns[5].Frozen = true;
            dataGridView1.Columns[6].Frozen = true;
            dataGridView1.Columns[7].Frozen = true;
            dataGridView1.Columns[8].Frozen = true;
            dataGridView1.Columns[9].Frozen = true;
            dataGridView1.Columns[10].Frozen = true;
            dataGridView1.Columns[11].Frozen = true;
        }
        private void pager1_PageChanged()
        {
            if (pager1.CurrentPage != _currentPage)
            {
                _currentPage = pager1.CurrentPage;
                pager1.Enabled = false;
                GetSearchData(SearchSql,false);
                //getFPListByStauts(lastSearchStauts);
                pager1.Enabled = true;
                selectrowid = 0;
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex != 4)
            {
                if (e.RowIndex != 0)
                {
                    if (selectrowid == e.RowIndex) return;
                }
                selectrowid = e.RowIndex;
               
                try
                {
                    dataGridView2.Rows.Clear();
                    string indexid = dataGridView1.Rows[e.RowIndex].Cells["id"].Value.ToString();
                    if (dataGridView1.Rows[e.RowIndex].Cells["stauts"].Value.ToString() != "待查验" && dataGridView1.Rows[e.RowIndex].Cells["stauts"].Value.ToString() != "次日再查")
                    {
                        this.dataGridView1.Enabled = false;

                        dt = inv_tax_common.Webservice.GetRecordDetailsForDisplayByMainIDs(guid, indexid);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //dataGridView2.DataSource = dt;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dataGridView2.Rows.Add();
                                dataGridView2.Rows[i].Cells["sid"].Value = (i + 1).ToString();
                                dataGridView2.Rows[i].Cells["mc"].Value = dt.Rows[i]["mc"].ToString();
                                dataGridView2.Rows[i].Cells["gg"].Value = dt.Rows[i]["gg"];
                                dataGridView2.Rows[i].Cells["dw"].Value = dt.Rows[i]["dw"];
                                dataGridView2.Rows[i].Cells["amount"].Value = dt.Rows[i]["amount"];
                                dataGridView2.Rows[i].Cells["dj"].Value = dt.Rows[i]["dj"];
                                dataGridView2.Rows[i].Cells["dje"].Value = dt.Rows[i]["je"];
                                dataGridView2.Rows[i].Cells["sl"].Value = dt.Rows[i]["sl"].ToString() + "%";
                                dataGridView2.Rows[i].Cells["dse"].Value = dt.Rows[i]["se"];
                            }
                        }
                        this.dataGridView1.Enabled = true;
                        this.dataGridView1.Focus();
                    }
                }
                catch { }
                
            }
            else
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells["selected"];
                    checkCell.Value = !Convert.ToBoolean(checkCell.Value);
                    dataGridView1.Rows[e.RowIndex].Selected = Convert.ToBoolean(checkCell.Value);
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                        dataGridView1.Rows[i].Selected = Convert.ToBoolean(checkCell.Value);
                    }
                }
            } 
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string indexid = dataGridView1.Rows[e.RowIndex].Cells["id"].Value.ToString();
                DataGridViewRow dgvr = dataGridView1.Rows[e.RowIndex];
                if (dt != null && dt.Rows.Count>0)
                {
                    frmFPDetailNew fpd = new frmFPDetailNew();
                    fpd.getFPDetail(dgvr, dt, userInfo);
                    fpd.ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("没有明细内容！！");
                }
            //if (e.RowIndex >= 0)
            //{
            //    string id = dataGridView1.Rows[e.RowIndex].Cells["id"].Value.ToString();

            //    frmFPDetailNew fpd = new frmFPDetailNew();
            //    if (dt != null && dt.Rows.Count>0)
            //    {
            //        if (fpd.getFPDetail(id, userInfo))
            //        {
            //            fpd.ShowDialog();
                        
            //        }
            //    }
            //    else
            //    {
            //        MessageBox.Show("没有明细内容！！");
            //    }
            }
        }
        public void OutputSearchFPXX()
        {
            if (lastSearchStauts == "查询结果")
            {
                if (_pageCount < _itemsCount)
                {
                    _currentPage = 1;
                    _itemsCount = _pageCount;
                    GetSearchData(SearchSql,true);
                    //getFPListByStauts(lastSearchStauts);
                }
                //if (int.Parse(cbPagesum.Text) < rowNumber)
                //    cbPagesum.Text = rowNumber.ToString();
                string path = "";
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel97-2003(*.xls)|*.xls";
                saveDialog.FileName = "";
                saveDialog.ShowDialog();
                path = saveDialog.FileName;
                if (path.IndexOf(":") < 0) return; //判断是否点击取消
                bool saveExcelflag = false;

                string selectid = "";

                if (dataGridView1.Rows.Count > 0)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                    }
                    if (selectid != "")
                    {
                        selectid = selectid.Substring(0, selectid.Length - 1);
                    }
                    DataTable dtYS = GetDataGridViewToDataTable(dataGridView1, false);
                    DataTable dtMX = inv_tax_common.Webservice.GetRecordDetailsByMainIDs(guid, selectid);
                    saveExcelflag = inv_tax_common.ExcelCls._DataTableExportExcel(dtYS, dtMX, path);
                    if (saveExcelflag)
                    {
                        MessageBox.Show(path + "，发票信息导出成功", "系统提示", MessageBoxButtons.OK);
                    }
                    dtYS.Dispose();
                    dtMX.Dispose();
                }
            }
        }
        public void OutputSelectedFPXX()
        {
            bool saveExcelflag = false;
            string selectid = "";
            dataGridView1.Refresh();
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value))
                    {
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                    }      
                }
                if (selectid != "")
                    selectid = selectid.Substring(0, selectid.Length - 1);
                else
                {
                    MessageBox.Show("请选择需要导出发票明细的记录，可同时选择多条记录。");
                    return;
                }
                string path = "";
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.DefaultExt = "xls";
                saveDialog.Filter = "Excel97-2003(*.xls)|*.xls";
                saveDialog.FileName = "";
                saveDialog.ShowDialog();
                path = saveDialog.FileName;
                if (path.IndexOf(":") < 0) return; //判断是否点击取消

                DataTable dt = inv_tax_common.Webservice.GetRecordDetailsByMainIDs(guid, selectid);

                saveExcelflag = inv_tax_common.ExcelCls._DataTableExportExcel(GetDataGridViewToDataTable(dataGridView1, true), dt, path);
                if (saveExcelflag)
                {
                    MessageBox.Show(path + "，发票信息导出成功", "系统提示", MessageBoxButtons.OK);
                }
            }
        }
        public void OutputSelectedToGxExcel()
        {
            if (dataGridView1.Rows.Count == 0)
                return;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "excel 2003|*.xls";
            saveFileDialog1.ShowDialog();
            string file = saveFileDialog1.FileName;

            if (file == "") return;

            DataTable dt = new DataTable();
            dt.Columns.Add("是否勾选");
            dt.Columns.Add("发票代码");
            dt.Columns.Add("发票号码");
            dt.Columns.Add("开票日期");
            dt.Columns.Add("销方名称");
            dt.Columns.Add("销方税号");
            dt.Columns.Add("金额");
            dt.Columns.Add("税额");
            int rowsCount = 0;
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataRow row = dt.NewRow();
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                string xfmc = dataGridView1.Rows[i].Cells["xfmc"].Value.ToString();
                string xfsh = dataGridView1.Rows[i].Cells["xfsh"].Value.ToString();
                string je = dataGridView1.Rows[i].Cells["je"].Value.ToString();
                string se = dataGridView1.Rows[i].Cells["se"].Value.ToString();
                if (Convert.ToBoolean(checkCell.Value))
                    row[0] = "是";
                else
                    row[0] = "否";
                row[1] = dataGridView1.Rows[i].Cells["fpdm"].Value.ToString();
                row[2] = dataGridView1.Rows[i].Cells["fphm"].Value.ToString();
                string str = dataGridView1.Rows[i].Cells["kprq"].Value.ToString();
                if (str.IndexOf('-')<=0)
                    str = DateTime.ParseExact(str, "yyyyMMdd", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                row[3] = str;
                row[4] = xfmc;
                row[5] = xfsh;
                row[6] = je;
                row[7] = se;
                dt.Rows.Add(row);
                rowsCount++;
            }

            if (rowsCount > 2000)
            {
                MessageBox.Show("批量勾选数量超过2000行，请拆分多次进行勾选。");
                return;
            }
            bool uploaded = inv_tax_common.ExcelCls._DataTableExportExcel(dt, "", file);

            if (uploaded)
            {
                MessageBox.Show(file + "批量勾选Excel文件已成功导出，请登录增值税发票选择确认平台上传！");
            }
            else
            {
                MessageBox.Show("导出文件失败。。");
            }
        }
        private DataTable GetDataGridViewToDataTable(DataGridView dgv, bool selected)
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
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[count].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value) == selected)
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
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                dataGridView1.CurrentCell = null;
                isSelectAll = !isSelectAll;
                int count = dataGridView1.Rows.Count;
                for (int i = 0; i < count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    checkCell.Value = isSelectAll;
                    //checkCell.Value = !Convert.ToBoolean(checkCell.Value);
                    dataGridView1.Rows[i].Selected = Convert.ToBoolean(checkCell.Value);
                }

                if (!isSelectAll)
                    dataGridView1.Columns["selected"].HeaderText = "全选";
                else
                    dataGridView1.Columns["selected"].HeaderText = "取消";
            }
        }
        private void dataGridView1_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            //   如果是序号、金额、税额、价额合计列，则按浮点数处理
            if (e.Column.Name == "yhid" || e.Column.Name == "je" || e.Column.Name == "se" || e.Column.Name == "jshj")
            {
                if (e.ToString() == "" && e.CellValue2.ToString() == "")
                    e.SortResult = 0;
                else if (e.CellValue1.ToString() == "")// ‘如果第一个值为空，则返回第一个值小于第二个值。
                    e.SortResult = -1;
                else if (e.CellValue2.ToString() == "") //  如果第二个值为空，则返回第一个值大于第二个值.
                    e.SortResult = 1;
                else
                 e.SortResult = (Convert.ToDouble(e.CellValue1) - Convert.ToDouble(e.CellValue2) > 0) ? 1 : (Convert.ToDouble(e.CellValue1) - Convert.ToDouble(e.CellValue2) < 0) ? -1 : 0;
            }
            //否则，按字符串比较
            else
            {
                if (e.ToString() == "" && e.CellValue2.ToString() == "") // '取得前后两个值，如果都为空，就返回相等的结果
                    e.SortResult = 0;
                else if (e.CellValue1.ToString() == "")// ‘如果第一个值为空，则返回第一个值小于第二个值。
                    e.SortResult = -1;
                else if (e.CellValue2.ToString() == "") //  如果第二个值为空，则返回第一个值大于第二个值.
                    e.SortResult = 1;
                else
                    e.SortResult = System.String.Compare(Convert.ToString(e.CellValue1), Convert.ToString(e.CellValue2));//‘都不为空时，按正常排序操作
            }

            // 如果发现两行相同，则按序号排序  
            if (e.SortResult == 0 && e.Column.Name != "yhid")
            {
                e.SortResult = Convert.ToInt32(dataGridView1.Rows[e.RowIndex1].Cells["yhid"].Value.ToString()) -
                        Convert.ToInt32(dataGridView1.Rows[e.RowIndex2].Cells["yhid"].Value.ToString());
            }
            e.Handled = true;    // 最后示意本次事件已正常处理。不能省掉，不然没效果
        }
        private void tsbtnSearchMore_Click(object sender, EventArgs e)
        {
            CallSearchForm();
        }
        public void CallSearchForm()
        {
            frmSearchCondition fsc = new frmSearchCondition(userInfo.CompanyGuid);
            fsc.ShowDialog();
            if (fsc.SQL != "")
            {
                SearchSql = fsc.SQL + " and " + userPrivilegeSearch;
                GetSearchData(SearchSql,true);
            }
        }
        private void tscbcyzt_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void tsBtnRefresh_Click(object sender, EventArgs e)
        {
            GetSearchData(SearchSql,false);
        }
        private void tsbtnSearch_Click(object sender, EventArgs e)
        {
            string sqltmp = "";
            string tmp = " and ";
            int n = 0;

            if (tscbfplx.Text != "全部")
            {
                n++;
                string fplx = "";
                switch (tscbfplx.Text)
                {
                    case "增值税专用发票":
                        fplx = "01";
                        break;
                    case "增值税普通发票":
                        fplx = "04";
                        break;
                    case "增值税电子普通发票":
                        fplx = "10";
                        break;
                    case "增值税普通发票(卷票)":
                        fplx = "11";
                        break;
                }
                sqltmp += " fplx = '" + fplx + "' ";
            }
            if (tscbcyzt.Text != "全部")
            {
                if (n > 0)
                    sqltmp += tmp;
                sqltmp += " checkstauts = '" + tscbcyzt.Text + "' ";
                n++;
            }

            if ( tstbxfmc.Text != "")
            {
                if (n > 0)
                    sqltmp += tmp;
                sqltmp += " xfmc LIKE '%" + tstbxfmc.Text + "%' ";
                n++;
            }
            if (tstbFPDM.Text != "")
            {
                if (n > 0)
                    sqltmp += tmp;
                sqltmp += " fphm LIKE '%" + tstbFPDM.Text + "%' ";
                n++;
            }
            if (tstbkrrq_q.Text != "")
            {
                try
                {
                    DateTime krrq_q = DateTime.Parse(tstbkrrq_q.Text);
                    DateTime krrq_z = DateTime.Parse(tstbkrrq_z.Text);

                    if (tstbkrrq_q.Text == tstbkrrq_z.Text)
                    {
                        if (n > 0)
                            sqltmp += tmp;
                        sqltmp += " str_to_date(kprq,'%Y%m%d') ='" + tstbkrrq_q.Text + "' ";
                        n++;

                    }
                    else if (krrq_q < krrq_z)
                    {
                        if (n > 0)
                            sqltmp += tmp;
                        sqltmp += " datetime(kprq) >=datetime('" + tstbkrrq_q.Text + "') and datetime(kprq) <=datetime('" + tstbkrrq_z.Text + "') ";
                        n++;
                    }
                    else
                    {
                        MessageBox.Show("开票日期起始日期不能大于结束日期，请重新输入日期！");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("请输入有效的开票时间，格式如：2017-03-11");
                    return;
                }
            }

            if (n > 0)
            {
                SearchSql = sqltmp + " and " + userPrivilegeSearch;
                //sql2 += sqltmp + ") and companyguid='" + companyguid + "' order by id DESC";
            }
            else
            {
                SearchSql = userPrivilegeSearch;
            }
            _currentPage = 1;
            GetSearchData(SearchSql, true);
           
        }
        private void cbPageSize_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _itemsCount = int.Parse(cbPageSize.Text);
                //_pageCount = rowNumber / _itemsCount;
                //if (rowNumber % _itemsCount != 0)
                //{
                //    _pageCount++;
                //}
                _currentPage = 1;
                GetSearchData(SearchSql,true);
                
            }
            catch
            { }
        }
        public void reCheckFP()
        {
            string selectid = "";
            int selectedrows = 0;
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value))
                    {
                        selectedrows += 1;
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                    }
                }
                if (selectid != "")
                    selectid = selectid.Substring(0, selectid.Length - 1);

                if (selectedrows <=0)
                {
                    MessageBox.Show("请选择需要重新查验的发票记录，可同时选择多条记录。");
                    return;
                }

                if ((userInfo.TotlePages - userInfo.UsedPages) < selectedrows)
                {
                    MessageBox.Show("发票查验授权剩余数量不够，请联系软件服务商增加授权数量！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (MessageBox.Show("请再次确认是否需要重新查验：" + selectedrows.ToString() + "张发票？", "提示", MessageBoxButtons.YesNoCancel) == System.Windows.Forms.DialogResult.Yes)
                {
                    string result = inv_tax_common.Webservice.ReCheckFP(selectid, userInfo.CompanyGuid);
                    //inv_tax_common.Common.OpenInvtaxService();
                    MessageBox.Show(result);
                    userInfo.UsedPages = userInfo.UsedPages + selectedrows;
                    changedUsedPage(userInfo.UsedPages);
                    GetSearchData(SearchSql, false);
                }
            }
        }
        public void UpdateSelectedItem()
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                if (Convert.ToBoolean(checkCell.Value))
                {
                    string id  = dataGridView1.Rows[i].Cells["id"].Value.ToString();
                    string fplx = dataGridView1.Rows[i].Cells["fplx"].Value.ToString();
                    string fpdm = dataGridView1.Rows[i].Cells["fpdm"].Value.ToString();
                    string fphm = dataGridView1.Rows[i].Cells["fphm"].Value.ToString();
                    string kprq = dataGridView1.Rows[i].Cells["kprq"].Value.ToString();
                    string je = dataGridView1.Rows[i].Cells["je"].Value.ToString();
                    string jym = dataGridView1.Rows[i].Cells["jym"].Value.ToString();
                    frmFPcollect fc = new frmFPcollect("edit", id, fpdm, fphm, kprq, je, jym, userInfo);
                    fc.ShowDialog();
                    if (fc.userInfo.UsedPages > userInfo.UsedPages)
                    {
                        userInfo.UsedPages = fc.userInfo.UsedPages;
                        changedUsedPage(userInfo.UsedPages);
                    }
                    GetSearchData(SearchSql,false);
                    break;
                }
            }
        }
        public void deleteSelectedItems()
        {
            string selectid = "";
            int selectedrows = 0;
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value))
                    {
                        selectedrows += 1;
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                    }
                }
                if (selectid != "")
                    selectid = selectid.Substring(0, selectid.Length - 1);
                else
                {
                    MessageBox.Show("请选择需要删除的发票记录，可同时选择多条记录。");
                    return;
                }
                if (MessageBox.Show("请再次确认是否需要删除：" + selectedrows.ToString() + "张发票？", "提示", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    string result = inv_tax_common.Webservice.DeleteRecordById(selectid, userInfo.CompanyGuid);
                    try
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(result);
                        XmlElement node = xmlDoc.DocumentElement;
                        MessageBox.Show(node.ChildNodes.Item(1).InnerText);

                        if (node.ChildNodes.Item(0).InnerText == "true")
                        {
                            GetSearchData(SearchSql, false);
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }
        }
        public void MoveToDataCenter()
        {
            string selectid = "";
            int selectItems = 0;
            dataGridView1.Refresh();
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value))
                    {
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                        selectItems++;
                    }
                }
                if (selectid != "")
                    selectid = selectid.Substring(0, selectid.Length - 1);
                else
                {
                    MessageBox.Show("请勾选需要转入发票仓库的发票记录，可同时勾选多条记录。");
                    return;
                }

                if (MessageBox.Show("已选择：" + selectItems.ToString() + "张发票,请确认是否移入发票仓库？\n提示：移入发票仓库后不得移出！", "移入发票仓库", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                string result = inv_tax_common.Webservice.TransferToDataCenter(selectid, userInfo.CompanyGuid);
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(result);
                    XmlElement node = xmlDoc.DocumentElement;
                    MessageBox.Show(node.ChildNodes.Item(1).InnerText);
                    if (node.ChildNodes.Item(0).InnerText == "true")
                    {
                        RefreshAll();
                    }
                }
                catch (Exception ex)
                { }

            }
        }
        public void ChangFPExceptionByCurrentItem(string stautsName, int currentItem)
        {
            string exception = dataGridView1.Rows[currentItem].Cells["exception"].Value.ToString();
            if (exception != stautsName)
            {
                string selectid = dataGridView1.Rows[currentItem].Cells["id"].Value.ToString();

                if (MessageBox.Show("请确认是否将当前发票的状态标识更新为:" + stautsName, "更新状态标识", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    dgv_exception.Text = exception;
                    return;
                }

                string result = inv_tax_common.Webservice.ChangFPException(selectid, stautsName, userInfo.CompanyGuid);

                dataGridView1.Rows[currentItem].Cells["exception"].Value = stautsName;
            }
           

        }
        public void ChangFPException(string stautsName)
        {
            string selectid = "";
            int selectItems = 0;
            dataGridView1.Refresh();
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                    if (Convert.ToBoolean(checkCell.Value) && dataGridView1.Rows[i].Cells["exception"].ToString() != stautsName)
                    {
                        selectid += dataGridView1.Rows[i].Cells["id"].Value.ToString() + ",";
                        selectItems++;
                    }
                }
                if (selectid != "")
                    selectid = selectid.Substring(0, selectid.Length - 1);
                else
                {
                    MessageBox.Show("请勾选需要更新异常状态的发票，可同时勾选多条发票。");
                    return;
                }

                if (MessageBox.Show("请确认是否将" + selectItems.ToString() + "张发票的异常状态更新为:"+stautsName,"更新异常状态", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                string result = inv_tax_common.Webservice.ChangFPException(selectid,stautsName, userInfo.CompanyGuid);
                MessageBox.Show(result);

                GetSearchData(SearchSql, false);
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right && e.ColumnIndex > -1 && e.RowIndex > -1 && e.ColumnIndex !=7)
            {
                this.contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
            }

            //if (CurrentFormName != "DataCenter")
            //{
            //    if (e.Button == System.Windows.Forms.MouseButtons.Right && e.ColumnIndex > -1 && e.RowIndex > -1 && e.ColumnIndex == 7)
            //    {
            //        currentRow = e.RowIndex;
            //        this.contextMenuStrip2.Show(MousePosition.X, MousePosition.Y);
            //    }
            //}
        }

        private void tmsOutputSelectedItem_Click(object sender, EventArgs e)
        {
            OutputSelectedFPXX();
        }

        private void tsmRecheck_Click(object sender, EventArgs e)
        {
            reCheckFP();
        }

        private void tsmMoveToDatacenter_Click(object sender, EventArgs e)
        {
            MoveToDataCenter();
        }

        private void tmsEditSelectItem_Click(object sender, EventArgs e)
        {
            UpdateSelectedItem();
        }

        private void tsmDeleteSelecteItem_Click(object sender, EventArgs e)
        {
            deleteSelectedItems();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1_CellContentClick(sender, e);
        }

        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            currentRow = e.RowIndex;
            if (CurrentFormName != "DataCenter" && e.ColumnIndex == 7)
            {
                return;
            }
            if (e.RowIndex >= 0 && e.ColumnIndex != 4)
            {
                
                if (e.RowIndex != 0)
                {
                    if (selectrowid == e.RowIndex) return;
                }
                selectrowid = e.RowIndex;

                try
                {
                    dataGridView2.Rows.Clear();
                    string indexid = dataGridView1.Rows[e.RowIndex].Cells["id"].Value.ToString();
                    if (dataGridView1.Rows[e.RowIndex].Cells["stauts"].Value.ToString() != "待查验" && dataGridView1.Rows[e.RowIndex].Cells["stauts"].Value.ToString() != "次日再查")
                    {
                        //this.dataGridView1.Enabled = false;

                        dt = inv_tax_common.Webservice.GetRecordDetailsForDisplayByMainIDs(guid, indexid);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //dataGridView2.DataSource = dt;
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                dataGridView2.Rows.Add();
                                dataGridView2.Rows[i].Cells["sid"].Value = (i + 1).ToString();
                                string mc = dt.Rows[i]["mc"].ToString();
                                dataGridView2.Rows[i].Cells["mc"].Value = mc;
                                if (isKeyword(mc))
                                    dataGridView2.Rows[i].Cells["mc"].Style.ForeColor = Color.Red;
                                dataGridView2.Rows[i].Cells["gg"].Value = dt.Rows[i]["gg"];
                                dataGridView2.Rows[i].Cells["dw"].Value = dt.Rows[i]["dw"];
                                dataGridView2.Rows[i].Cells["amount"].Value = dt.Rows[i]["amount"];
                                dataGridView2.Rows[i].Cells["dj"].Value = dt.Rows[i]["dj"];
                                dataGridView2.Rows[i].Cells["dje"].Value = dt.Rows[i]["je"];
                                dataGridView2.Rows[i].Cells["sl"].Value = dt.Rows[i]["sl"].ToString() + "%";
                                dataGridView2.Rows[i].Cells["dse"].Value = dt.Rows[i]["se"];
                            }
                        }
                        //this.dataGridView1.Enabled = true;
                        this.dataGridView1.Focus();
                    }
                }
                catch { }

            }
            else
            {
                if (e.RowIndex >= 0 && e.ColumnIndex != 7)
                {
                    DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells["selected"];
                    checkCell.Value = !Convert.ToBoolean(checkCell.Value);
                    
                    dataGridView1.Rows[e.RowIndex].Selected = Convert.ToBoolean(checkCell.Value);
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                        dataGridView1.Rows[i].Selected = Convert.ToBoolean(checkCell.Value);
                    }
                    dataGridView1.EndEdit();
                }
            } 
        }

        public bool isKeyword(string mc)
        {
            bool flag = false;

            if (userInfo.KeyWord != "")
            {
                string[] keywords =userInfo.KeyWord.Split('|');
                if (keywords.Length > 0)
                {
                    for (int i = 0; i < keywords.Length; i++)
                    {
                        string key = keywords[i].Trim();
                        if (key != "")
                        {
                            if (mc.IndexOf(key) > -1)
                            {
                                flag = true;
                                break;
                            }
                        }

                    }
                }
            }

            return flag;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ChangFPExceptionByCurrentItem("正常", currentRow);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            ChangFPExceptionByCurrentItem("错票", currentRow);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            ChangFPExceptionByCurrentItem("假票", currentRow);
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            ChangFPExceptionByCurrentItem("敏感票", currentRow);
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (CurrentFormName != "DataCenter")
            {
                if (dataGridView1.CurrentCell.ColumnIndex == 7 && dataGridView1.CurrentCell.RowIndex != -1)
                {
                    dgv_exception = e.Control as ComboBox;
                    dgv_exception.SelectedIndexChanged -= new EventHandler(dgv_exception_SelectedIndexChanged);

                    dgv_exception.SelectedIndexChanged += new EventHandler(dgv_exception_SelectedIndexChanged);

                    currentRow = dataGridView1.CurrentCell.RowIndex;
                }
            }
        }
        private void dgv_exception_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangFPExceptionByCurrentItem(dgv_exception.Text, currentRow);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                bool needRefresh = false;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    if (dataGridView1.Rows[i].Cells["stauts"].Value.ToString() == "待查验")
                    {
                        needRefresh = true;
                        break;
                    }
                }
                if (needRefresh)
                {
                    GetSearchData(SearchSql, false);
                }



            }

        }

        private void pager1_Load(object sender, EventArgs e)
        {

        }


        private void dataGridView2_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.ToString() == "" && e.CellValue2.ToString() == "") // '取得前后两个值，如果都为空，就返回相等的结果
                e.SortResult = 0;
            else if (e.CellValue1.ToString() == "")// ‘如果第一个值为空，则返回第一个值小于第二个值。
                e.SortResult = -1;
            else if (e.CellValue2.ToString() == "") //  如果第二个值为空，则返回第一个值大于第二个值.
                e.SortResult = 1;
            else
                e.SortResult = String.Compare(e.CellValue1.ToString(), e.CellValue2.ToString());//‘都不为空时，按正常排序操作
            e.Handled = true;   
        }

        private void tstbBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string strBarcode = tstbBarcode.Text;
                if (strBarcode.IndexOf("，") > -1)
                {
                    MessageBox.Show("请将当前输入法切换成英文半角状态！", "无效采集", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (strBarcode != "")
                {
                    strBarcode = strBarcode.Replace("，", ",");
                    string[] tmp = strBarcode.Split(',');
                    //if (tmp.Length < 6)
                    //{
                    //    MessageBox.Show("当前二维码内容不是有效的发票信息，请重新采集！", "无效采集", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    //    return;
                    //}
                    if (tmp[2] != "" && (tmp[2].Length == 10 || tmp[2].Length == 12) && tmp[3] != "" && tmp[3].Length == 8)
                    {
                        string sqltmp = "fpdm = '" + tmp[2] + "' and fphm = '" + tmp[3] + "'";
                            SearchSql = sqltmp + " and " + userPrivilegeSearch;
                        _currentPage = 1;
                        GetSearchData(SearchSql, true);
                        tstbBarcode.Text = "";
                    }
                    else
                    {
                        MessageBox.Show("发票代码：" + tmp[2] + ",发票号码：" + tmp[3] + "不符合发票规范.");
                        tstbBarcode.Text = "";
                        return;

                    }
                }

            }
        }
        #region 报表报告模块
        public void setCullRow(string djhm)
        {
            try
            {
                int num2 = this.dataGridView1.RowCount - 1;
                for (int i = 0; i <= num2; i++)
                {
                    this.dataGridView1.Rows[i].Selected = false;
                    if (this.dataGridView1.Rows[i].Cells["djhm"].Value.ToString().Trim() == djhm)
                    {
                        this.dataGridView1.Rows[i].Selected = true;
                        this.dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    }
                    else
                    {
                        this.dataGridView1.Rows[i].Selected = false;
                    }
                }
            }
            catch
            {

            }
        }
        //打印发票信息
        public void Print_FPXX()
        {
            try
            {
                string djhm = this.dataGridView1.CurrentRow.Cells["id"].Value.ToString();
                if (djhm != null)
                {
                    djhm = djhm + ";";
                    if (djhm.Contains(";"))
                    {
                        Frm_PrintFP _Frm_PrintFP = new Frm_PrintFP(userInfo);
                        _Frm_PrintFP.djhm = djhm.ToString().Substring(0, djhm.ToString().Length - 1);
                        _Frm_PrintFP.mbName = "发票信息";
                        if (_Frm_PrintFP.ShowDialog(this) == DialogResult.OK)
                        {
                            //GetSearchData(SearchSql, false);
                            ////设定当前行
                            //this.setCullRow(djhm);
                        }
                    }
                }
            }
            catch (Exception exception1)
            {

            }

        }
        //打印发票清单 
        public void Print_FPList()
        {
            try
            {
                if (temp != null)
                {
                    Frm_PrintFPList _Frm_PrintFPlist = new Frm_PrintFPList(userInfo, temp);
                    _Frm_PrintFPlist.mbName = "发票清单";
                    _Frm_PrintFPlist.Show();
                }
            }
            catch (Exception exception1)
            {
            }
        }

        #endregion

    }

}