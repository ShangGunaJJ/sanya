using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GdPicture9;
using System.Xml;
using System.Globalization;
using System.IO;
namespace Invtax.Winform
{
    public partial class frmScan : XiaoCai.WinformUI.Forms.CustomForm
    {
        //定义委托
        public delegate void ChildFromHandle(int usedpage);
        public event ChildFromHandle changedUsedPage;  

        private GdPictureImaging m_GdPictureImaging = new GdPictureImaging();
        private GdPicturePDF m_GdPicturePDF = new GdPicturePDF();
        private int m_ImageID;
        private DocumentType m_DocSourceType;
        private int _currentItem = -1;
        public inv_tax_common.UserInfo userInfo = null;
        inv_tax_common.IniFileRW iniFile = new inv_tax_common.IniFileRW();
        string ScannerModel = "";
        string ScannerhDrv = "";
        string IsShowTwainUI = "";
        string DPI = "";
        string ImageFormat = "";
        string ColorMode = "";
        string Compress = "";

        string scanFPlx = "";
        string Scanmode = "";

        int fileNumber = 1;
        int currentItem = -1;
        public frmScan()
        {
            InitializeComponent();
        }

        public frmScan(inv_tax_common.UserInfo usInfo,string fplx,string mode)
        {
            InitializeComponent();
            userInfo = usInfo;
            scanFPlx = fplx;
            Scanmode = mode;

            try
            {
                Icon softIcon = new Icon("SHHK.ico");
                this.Icon = softIcon;
               // this.Image = Properties.Resources.HK;
            }
            catch { }
        }

        private void frmScan_Load(object sender, EventArgs e)
        {
            GdPicture9.LicenseManager oLicenceManager = new GdPicture9.LicenseManager();
            oLicenceManager.RegisterKEY("211858726909419701115114542223623");
            if (scanFPlx == "01")
            {
                dataGridView1.Columns["dgvkjje"].Visible = true;
                dataGridView1.Columns["dgvjym"].Visible = false;
            }
            else
            {
                dataGridView1.Columns["dgvkjje"].Visible = false;
                dataGridView1.Columns["dgvjym"].Visible = true;
            }

            if (Scanmode == "scan")
            {
                tsBtnScannerSetting.Visible = true;
                tsBtnScanImage.Visible = true;
                tsBtnImportFile.Visible = false;
            }
            else
            {
                tsBtnScannerSetting.Visible = false;
                tsBtnScanImage.Visible = false;
                tsBtnImportFile.Visible = true;
            }
        }

        public void getScannerConfig()
        {
            ScannerModel = iniFile.ReadValue("ScannerSetting", "ScannerModel");
            ScannerhDrv = iniFile.ReadValue("ScannerSetting", "ScannerhDrv");
            IsShowTwainUI = iniFile.ReadValue("ScannerSetting", "IsShowTwainUI");
            DPI = iniFile.ReadValue("ScannerSetting", "DPI");
            ImageFormat = iniFile.ReadValue("ScannerSetting", "ImageFormat");
            Compress = iniFile.ReadValue("ScannerSetting", "Compress");
            ColorMode = iniFile.ReadValue("ScannerSetting", "ColorMode");
        }

        private void tsBtnImportFile_Click(object sender, EventArgs e)
        {
            CloseDocument();
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer;
            OpenFileDialog openFiles = new OpenFileDialog();
            openFiles.Multiselect = true;
            openFiles.Filter = "电子发票 PDF|*.pdf;*.jpg;*.bmp";
            openFiles.ShowDialog();
            if (openFiles.FileNames.Length <= 0) return;
            string[] filenames =openFiles.FileNames;
            for (int i = 0; i < filenames.Length; i++)
            {
                string result="";
                string sFilePath = filenames[i];
                string exf = sFilePath.Substring(sFilePath.LastIndexOf('.'),sFilePath.Length-sFilePath.LastIndexOf('.'));
                try
                {
                    if (exf.ToLower() == ".pdf")
                        result = QRcodeReader(sFilePath);
                    else
                        result = AvisionSDK.QRCodeRecognitionFromFile(sFilePath);
                }
                catch { }
                AddNewRecordItem(result, sFilePath);
            }
        }

        private void AddNewRecordItem(string strBarcode, string sFilePath)
        {
            int n = dataGridView1.Rows.Count;
            if (strBarcode == "")
            {
                dataGridView1.Rows.Add();
                dataGridView1.Rows[n].Cells["sid"].Value = dataGridView1.Rows.Count.ToString();
                dataGridView1.Rows[n].Cells["filepath"].Value = sFilePath;
                dataGridView1.Refresh();
                return;
            }
            strBarcode = strBarcode.Replace("，", ",");
            string[] tmp = strBarcode.Split(',');
            for (int i = 0; i < n; i++)
            {
                if (dataGridView1.Rows[i].Cells["dgvfpdm"].Value != null && dataGridView1.Rows[i].Cells["dgvfphm"].Value != null)
                {
                    if (dataGridView1.Rows[i].Cells["dgvfpdm"].Value.ToString() == tmp[2] && dataGridView1.Rows[i].Cells["dgvfphm"].Value.ToString() == tmp[3])
                    {
                        MessageBox.Show("发票代码：" + tmp[2] + ",发票号码：" + tmp[3] + "已采集成功.");
                        return;
                    }
                }
            }
            string fplx = tmp[1];
            dataGridView1.Rows.Add();
            dataGridView1.Rows[n].Cells["sid"].Value = dataGridView1.Rows.Count.ToString();
            dataGridView1.Rows[n].Cells["dgvfplx"].Value = fplx.ToString();
            dataGridView1.Rows[n].Cells["dgvdq"].Value = inv_tax_common.Common.getFPDQ(tmp[2]);
            dataGridView1.Rows[n].Cells["dgvfplxName"].Value = inv_tax_common.Common.getFPLXName(fplx);
            dataGridView1.Rows[n].Cells["dgvfpdm"].Value = tmp[2];
            dataGridView1.Rows[n].Cells["dgvfphm"].Value = tmp[3];
            dataGridView1.Rows[n].Cells["dgvkprq"].Value = tmp[5];
            if (fplx == "01")
            {
                dataGridView1.Rows[n].Cells["dgvkjje"].Value = tmp[4];
            }
            else
            {
                dataGridView1.Rows[n].Cells["dgvjym"].Value = tmp[6].Substring(tmp[6].Length - 6, 6);
            }
            dataGridView1.Rows[n].Cells["filepath"].Value = sFilePath;
            dataGridView1.Refresh();
        }
        private void tsTxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateRecordItem(tsTxtBarcode.Text, currentItem);
            }

        }
        private void tsTxtBarcode_Enter(object sender, EventArgs e)
        {
            if (currentItem > -1)
            {
                gdViewer1.ZoomArea(0, 0, 600, 600);
            }
        }

        private void UpdateRecordItem(string strBarcode, int n)
        {
            if (strBarcode == "" && n >-1)
            {
                return;
            }
            strBarcode = strBarcode.Replace("，", ",");
            string[] tmp = strBarcode.Split(',');
            if (tmp.Length <7)
            {
                MessageBox.Show("当前二维码内容不是有效的发票信息，请重新采集！", "无效采集", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tsTxtBarcode.Text = "";
                return;
            }
            for (int i = 0; i < n; i++)
            {
                if (dataGridView1.Rows[i].Cells["dgvfpdm"].Value != null && dataGridView1.Rows[i].Cells["dgvfphm"].Value != null)
                {
                    if (dataGridView1.Rows[i].Cells["dgvfpdm"].Value.ToString() == tmp[2] && dataGridView1.Rows[i].Cells["dgvfphm"].Value.ToString() == tmp[3])
                    {
                        MessageBox.Show("发票代码：" + tmp[2] + ",发票号码：" + tmp[3] + "已采集成功.");
                        tsTxtBarcode.Text = "";
                        return;
                    }
                }
            }
            string fplx = tmp[1];
            //dataGridView1.Rows[n].Cells["sid"].Value = dataGridView1.Rows.Count.ToString();
            dataGridView1.Rows[n].Cells["dgvfplx"].Value = fplx.ToString();
            dataGridView1.Rows[n].Cells["dgvdq"].Value = inv_tax_common.Common.getFPDQ(tmp[2]);
            dataGridView1.Rows[n].Cells["dgvfplxName"].Value = inv_tax_common.Common.getFPLXName(fplx);
            dataGridView1.Rows[n].Cells["dgvfpdm"].Value = tmp[2];
            dataGridView1.Rows[n].Cells["dgvfphm"].Value = tmp[3];
            dataGridView1.Rows[n].Cells["dgvkprq"].Value = tmp[5];
            if (fplx == "01")
            {
                dataGridView1.Rows[n].Cells["dgvkjje"].Value = tmp[4];
            }
            else
            {
                dataGridView1.Rows[n].Cells["dgvjym"].Value = tmp[6].Substring(tmp[6].Length - 6, 6);
            }
            tsTxtBarcode.Text = "";
            tsTxtBarcode.Focus();
            dataGridView1.Refresh();
        }

        private void renderPdfPage(int pageNo)
        {
            if (m_ImageID != 0)
            {
                m_GdPictureImaging.ReleaseGdPictureImage(m_ImageID);
                m_ImageID = 0;
            }
            m_GdPicturePDF.SelectPage(pageNo);
            m_ImageID = m_GdPicturePDF.RenderPageToGdPictureImageEx(float.Parse("200"), true);
        }
        private void CloseCurrentImage()
        {
            if (m_DocSourceType == DocumentType.DocumentTypePDF)
            {
                m_GdPicturePDF.CloseDocument();
            }
            else
            {
                m_GdPictureImaging.ReleaseGdPictureImage(m_ImageID);
                gdViewer1.CloseDocument();
                m_ImageID = 0;
            }
            m_DocSourceType = DocumentType.DocumentTypeUnknown;
        }

        private void CloseDocument()
        {
            gdViewer1.CloseDocument();
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeWidthViewer;
            gdViewer1.DocumentAlignment = ViewerDocumentAlignment.DocumentAlignmentTopCenter;
        }

        private void tsBtnViewer_zoom_in_Click(object sender, EventArgs e)
        {
            if (gdViewer1.ZoomIN() != GdPictureStatus.OK)
                MessageBox.Show("Cannot Zoom IN");
        }

        private void tsBtnViewer_zoom_out_Click(object sender, EventArgs e)
        {
            if (gdViewer1.ZoomOUT() != GdPictureStatus.OK)
                MessageBox.Show("Cannot Zoom out");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tpBtmViewer_zoom_page_Click(object sender, EventArgs e)
        {
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer; 
        }

        private void tsBtnViewer_zoom_width_Click(object sender, EventArgs e)
        {
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeWidthViewer; 
        }

        private void tsBtnViewer_zoom_pan_Click(object sender, EventArgs e)
        {
            gdViewer1.MouseMode = ViewerMouseMode.MouseModePan; 
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            currentItem = e.RowIndex;
            if (e.ColumnIndex == 0 && e.RowIndex>-1)
            {
                
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[e.RowIndex].Cells["selected"];
                checkCell.Value = !Convert.ToBoolean(checkCell.Value);
                return;
            }
            if (_currentItem != e.RowIndex && e.RowIndex>-1)
            {
                _currentItem = e.RowIndex;
                CloseDocument();
                gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer;
                string filePath = dataGridView1.Rows[e.RowIndex].Cells["filepath"].Value.ToString();
                gdViewer1.DisplayFromFile(filePath);
            }
        }

        private void tsBtnScannerSetting_Click(object sender, EventArgs e)
        {
            frmScannerSetting fss = new frmScannerSetting();
            fss.ShowDialog();
            if (fss.flag)
            {
                getScannerConfig();
            }
        }

        private void tsBtnDelete_Click(object sender, EventArgs e)
        {
            CloseDocument();
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer;

            int n = dataGridView1.Rows.Count;
            if (n == 0) return;
            for (int i = n - 1; i < dataGridView1.Rows.Count; i--)
            {
                if (i < 0) break;
                DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[i].Cells["selected"];
                if (dataGridView1.Rows.Count > 0 && Convert.ToBoolean(checkCell.Value))
                {
                    if (Scanmode == "scan")
                    {
                        string filePath = dataGridView1.Rows[i].Cells["filepath"].Value.ToString();
                        inv_tax_common.Common.DeleteFile(filePath);
                    }
                    dataGridView1.Rows.RemoveAt(i);
                    dataGridView1.Refresh();
                }
            }
        }

        private void tsBtnSubmit_Click(object sender, EventArgs e)
        {
            CloseDocument();
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer;

            int n = dataGridView1.Rows.Count;
            if (n == 0) return;
            if ((userInfo.TotlePages - userInfo.UsedPages) < n)
            {
                MessageBox.Show("发票查验授权剩余数量不够，请联系软件服务商增加授权数量！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string strfpdmList = "";
            string strfphmList = "";
            for (int i = 0; i < n; i++)
            {
                string fplx = "";
                if (dataGridView1.Rows[i].Cells["dgvfplx"].Value != null)
                    fplx = dataGridView1.Rows[i].Cells["dgvfplx"].Value.ToString();
                string fpdm = dataGridView1.Rows[i].Cells["dgvfpdm"].Value.ToString();
                string fphm = dataGridView1.Rows[i].Cells["dgvfphm"].Value.ToString();
                string kprq = dataGridView1.Rows[i].Cells["dgvkprq"].Value.ToString();
                if (kprq.IndexOf('-') < 0)
                    kprq = DateTime.ParseExact(kprq, "yyyyMMdd", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                string kjje = "";
                if (dataGridView1.Rows[i].Cells["dgvkjje"].Value != null)
                    kjje = dataGridView1.Rows[i].Cells["dgvkjje"].Value.ToString();
                string jym = "";
                if (dataGridView1.Rows[i].Cells["dgvjym"].Value != null)
                    jym = dataGridView1.Rows[i].Cells["dgvjym"].Value.ToString();

                if (fpdm == "" || fphm == "" || kprq == "")
                {
                    MessageBox.Show("第" + n.ToString() + "行的发票四要素信息不全，请补充完整！");
                    return;
                }

                if (fplx == "")
                {
                    fplx = inv_tax_common.Common.getFPLX(fpdm);
                    if (fplx == "")
                    {
                        MessageBox.Show("发票代码:" + fpdm + "为无效发票，请重新输入！");
                        return;
                    }
                    else
                        dataGridView1.Rows[i].Cells["dgvfplx"].Value = fplx;
                }
                
                strfpdmList = strfpdmList + "'" + fpdm + "',";
                strfphmList = strfphmList + "'" + fphm + "',";
            }
            strfpdmList = strfpdmList.Substring(0, strfpdmList.Length - 1);
            strfphmList = strfphmList.Substring(0, strfphmList.Length - 1);
            //查验当前发票是否已经在台账中存在
            DataTable dt = inv_tax_common.Webservice.CheckRepeatLRecords(strfpdmList, strfphmList, userInfo.CompanyGuid, userInfo.UserName,"1");
            if (dt != null && dt.Rows.Count > 0)
            {
                string msg = "以下发票已查验,是否自动删除重复提交的发票？" + "\n";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    msg = msg + "发票代码：" + dt.Rows[i]["fpdm"] + ",发票号码：" + dt.Rows[i]["fphm"] + ",上次查验：" + dt.Rows[i]["createtime"] + "\n";
                    
                    for (int k = 0; k < dataGridView1.Rows.Count;k++ )
                    {
                        if (dataGridView1.Rows[k].Cells["dgvfpdm"].Value.ToString() == dt.Rows[i]["fpdm"].ToString() && dataGridView1.Rows[k].Cells["dgvfphm"].Value.ToString() == dt.Rows[i]["fphm"].ToString())
                        {
                            DataGridViewCheckBoxCell checkCell = (DataGridViewCheckBoxCell)dataGridView1.Rows[k].Cells["selected"];
                            checkCell.Value = true;
                            dataGridView1.Rows[k].Selected = true;
                            dataGridView1.Rows[k].DefaultCellStyle.BackColor = Color.FromArgb(255, 255, 192);
                            dataGridView1.Rows[k].DefaultCellStyle.ForeColor = Color.Red;
                            break;
                        }
                    }
                }
                if (MessageBox.Show(msg, "提示",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    tsBtnDelete_Click(sender, e);
                }
                else
                {
                    return;
                }
            }
            int iupload = 0;
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("请添加需要查验的发票信息.");
                return;
            }
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                string fplx = dataGridView1.Rows[i].Cells["dgvfplx"].Value.ToString();
                string fpdm = dataGridView1.Rows[i].Cells["dgvfpdm"].Value.ToString();
                string fphm = dataGridView1.Rows[i].Cells["dgvfphm"].Value.ToString();
                string kprq = dataGridView1.Rows[i].Cells["dgvkprq"].Value.ToString();
                if (kprq.IndexOf('-')<0)
                    kprq = DateTime.ParseExact(kprq, "yyyyMMdd", CultureInfo.CurrentCulture).ToString("yyyy-MM-dd");
                string kjje = "";
                if (dataGridView1.Rows[i].Cells["dgvkjje"].Value != null)
                    kjje = dataGridView1.Rows[i].Cells["dgvkjje"].Value.ToString();
                string jym = "";
                if(dataGridView1.Rows[i].Cells["dgvjym"].Value!=null)
                    jym= dataGridView1.Rows[i].Cells["dgvjym"].Value.ToString();
                string createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string filePath = dataGridView1.Rows[i].Cells["filepath"].Value.ToString();
                try
                {
                    string result = "";
                    if (fplx == "01")
                    {
                       result = inv_tax_common.Webservice.AddRecord(fplx, fpdm, fphm, kprq, kjje, userInfo.CompanyGuid,userInfo.UserName);
                    }
                    else
                    {
                        result = inv_tax_common.Webservice.AddRecord(fplx, fpdm, fphm, kprq, jym, userInfo.CompanyGuid,userInfo.UserName);
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(result.ToString());
                    XmlElement node = xmlDoc.DocumentElement;

                    bool f = bool.Parse(node.ChildNodes.Item(0).InnerText);
                    if (f)
                    {
                        userInfo.UsedPages++;
                        iupload += 1;
                        string filename = fpdm + "_" + fphm;
                        inv_tax_common.Common.UploadImage(filePath, userInfo.CompanyName, filename);
                        if (Scanmode == "scan")
                        {
                            inv_tax_common.Common.DeleteFile(filePath);
                        }
                    }
                }
                catch
                { }
            }
            changedUsedPage(userInfo.UsedPages);
            //inv_tax_common.Common.OpenInvtaxService();
            MessageBox.Show("已成功提交" + iupload.ToString() + "张发票.");
            if (iupload == dataGridView1.Rows.Count)
                dataGridView1.Rows.Clear();
           
        }
        
        private void tsBtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsBtnPrint_Click(object sender, EventArgs e)
        {
            gdViewer1.PrintSetDocumentName("发票打印");
            gdViewer1.PrintDialog(this);
        }

        private void tssBtnScan_ButtonClick(object sender, EventArgs e)
        {

        }

        private unsafe void tsBtnScanImage_Click(object sender, EventArgs e)
        {
            string tempFolder = System.IO.Path.GetTempPath() + "InvtaxScan";
            try
            {
                Directory.Delete(tempFolder,true);
            }
            catch { }

            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            AvisionSDK.ONEPAGECALLBACK fnCallback = new AvisionSDK.ONEPAGECALLBACK(AvisionSDK.OnePageCallbackDemo);

            getScannerConfig();
            int hDrv = 0;
            int iRet, iIndex=-1;
            //int nCount;
            //char* szModel;
            int isssScanMode = int.Parse(ColorMode);
            ScanObj.LoadScanList();
            for (int i = 0; i < ScanObj.ScanDataList.Count; i++)
            {
                if (ScanObj.ScanDataList[i].ScanName == ScannerModel)
                {
                    iIndex = i;
                    hDrv = ScanObj.ScanDataList[i].ScanHandle;
                    break;
                }
            }
            iRet = AvisionSDK.Get_Avision_Driver(iIndex,&hDrv);
            if (iRet == 0)
            {
                MessageBox.Show("找不到扫描仪。");
                return;
            }

            iRet = AvisionSDK.Load_Driver(hDrv);
            if (iRet == 0)
            {
                MessageBox.Show("加载扫描仪失败。");
                return;
            }

            //2015.05.05 判断是否有纸
            int iStatus = AvisionSDK.Get_ADFStatus(hDrv);
            if (iStatus == 0)
            {
                MessageBox.Show("请放好纸张再点击扫描!");
                return;
            }

            //设置扫描颜色(正反面).
            int iScanMode = int.Parse(ColorMode);
            AvisionSDK.Set_Scan_Mode(hDrv, iScanMode);

            //设置扫描dpi
            AvisionSDK.Set_Resolution(hDrv, 300, iScanMode);

            //设置每次进纸张数
            AvisionSDK.Set_Scanner_ScanNum(Convert.ToInt32(-1));


            //是否显示Twain配置窗口
            if (IsShowTwainUI=="1")
                AvisionSDK.Show_Twain_UI(hDrv, Handle.ToInt32());

            //显示twain配置界面, 扫描操作函数见 AvisionSDK.OnePageCallbackDemo
            iRet = AvisionSDK.Start_Up_Scanner(hDrv, Handle.ToInt32(), 0, fnCallback);
            
            if (iRet == 0)
                MessageBox.Show("扫描图像失败。");
            else
            {
                ImportScanFile();
                //MessageBox.Show("扫描成功。请到D盘查看生成的图像。");
            }
        }

        private void ImportScanFile()
        {
            CloseDocument();
            gdViewer1.ZoomMode = ViewerZoomMode.ZoomModeFitToViewer;
            string tempFolder = System.IO.Path.GetTempPath() + "InvtaxScan";
            string[] tmpfilenames = Directory.GetFiles(tempFolder,"*.jpg");
            if (tmpfilenames.Length <= 0) return;

            string[] filenames = tmpfilenames;
            string scanFolder = System.IO.Directory.GetCurrentDirectory() + "\\scan";
            if (!Directory.Exists(scanFolder))
                Directory.CreateDirectory(scanFolder);

            for (int n = 0; n < tmpfilenames.Length; n++)
            {
                try
                {
                    string destFileName = scanFolder + "\\" + fileNumber.ToString("0000") + ".jpg";
                    File.Copy(tmpfilenames[n], destFileName, true);
                    fileNumber++;
                    filenames[n] = destFileName;
                }
                catch { }
                
            }
            for (int i = 0; i < filenames.Length; i++)
            {
                string sFilePath = filenames[i];
                //string result = QRcodeReader(sFilePath);
                string result = "";
                try
                {
                    result = AvisionSDK.QRCodeRecognitionFromFile(sFilePath);
                }
                catch { }
                //string result = "";
                AddNewRecordItem(result, sFilePath);
                
            }
        }

        private string QRcodeReader(string sFilePath)
        {
            string result = "";
            if (m_GdPictureImaging.GetDocumentFormatFromFile(sFilePath) == DocumentFormat.DocumentFormatPDF)
            {
                if (m_GdPicturePDF.LoadFromFile(sFilePath, false) == GdPictureStatus.OK)
                {
                    m_DocSourceType = DocumentType.DocumentTypePDF;
                    renderPdfPage(1);
                }
            }
            else
            {
                m_DocSourceType = DocumentType.DocumentTypeBitmap;
                m_ImageID = m_GdPictureImaging.CreateGdPictureImageFromFile(sFilePath);
            }

            BarcodeQRReaderScanMode scanMode = default(BarcodeQRReaderScanMode);
            scanMode = BarcodeQRReaderScanMode.BestQuality;
            m_GdPictureImaging.BarcodeQRReaderDoScan(m_ImageID, scanMode, 1);
            int bcfound = m_GdPictureImaging.BarcodeQRReaderGetBarcodeCount();

            if (bcfound > 0)
            {
                result = m_GdPictureImaging.BarcodeQRReaderGetBarcodeValue(1);
                
            }
            else
            {
                result = "";
            }

            return result;
        }

        

        
      
    }
}
