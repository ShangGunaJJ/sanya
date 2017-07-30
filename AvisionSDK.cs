using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Invtax.Winform
{
    class AvisionSDK
    {
        [DllImport("AVBarcode.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string QRCodeRecognitionFromFile([MarshalAs(UnmanagedType.LPWStr)] string filepath);

        [DllImport("AVBarcode.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public static extern string QRCodeRecognition(byte[] buff, uint BuffLen, int ImgDPI);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate void ONEPAGECALLBACK(int hBitmap, char** pBarCode, int* nXPos, int* nYPos, int* nBarcodeCount, int nScanIndex);
        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Driver_Count
         *描    述 : 获取扫描仪驱动数目
         *输    入 : 无
         *输    出 : pi_Drv_count 扫描仪驱动数目
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Driver_Count(int* pi_Drv_count);
        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Avision_Driver
        *描    述 : 获取扫描仪驱动句柄
        *输    入 : i_Item 扫描仪序号
        *输    出 : ph_Drv 扫描仪驱动HANDLE
        *返 回 值 : 返回TRUE  成功
        *           返回FALSE 失败
        */
        public unsafe static extern int Get_Avision_Driver(int i_Item, int* ph_Drv);
        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Model_Name
         *描    述 : 获取扫描仪名称
         *输    入 : h_Drv          扫描仪驱动HANDLE
         *输    出 : str_Model_Name 扫描仪名称
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Model_Name(int h_Drv, char** str_Model_Name);
        [DllImport("AviDriver.dll")]
        /*函数名称 : Load_Driver
         *描    述 : 加载扫描仪驱动
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Load_Driver(int h_Drv);
        [DllImport("AviDriver.dll")]
        /*函数名称 : Is_Loaded_Driver
         *描    述 : 判断是否加载扫描仪驱动
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  已经加载
         *           返回FALSE 未加载
         */
        public unsafe static extern int Is_Loaded_Driver(int h_Drv);
        [DllImport("AviDriver.dll")]
        /* 函数名称 : With_ADF
         * 描    述 : 判断扫描仪是否带有自动进稿器
         * 输    入 : h_Drv 扫描仪驱动HANDLE
         * 输    出 : 无
         * 返 回 值 : 返回TRUE  带有自动进稿器
         *           返回FALSE 不带有自动进稿器
         * */
        public unsafe static extern int With_ADF(int h_Drv);

        [DllImport("AviDriver.dll")]
        /*函数名称 : With_Flatbed
         *描    述 : 判断扫描仪是否带有平板扫描 
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  带有自动进稿器
         *          返回FALSE 不带有自动进稿器
         */
        public unsafe static extern int With_Flatbed(int h_Drv);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Is_Duplex
         *描    述 : 判断扫描仪是否带有双面扫描功能
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  带有双面扫描
         *           返回FALSE 不带有双面扫描
         */
        public unsafe static extern int Is_Duplex(int h_Drv);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_ADFSize_X
         *描    述 : 获取自动进稿器宽度
         *输    入 : h_Drv        扫描仪驱动HANDLE
         *输    出 : pi_ADFSize_X 自动进稿器宽度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_ADFSize_X(int h_Drv, int* pi_ADFSize_X);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_ADFSize_Y
         *描    述 : 获取自动进稿器高度
         *输    入 : h_Drv        扫描仪驱动HANDLE
         *输    出 : pi_ADFSize_Y 自动进稿器高度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_ADFSize_Y(int h_Drv, int* pi_ADFSize_Y);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Flatbed_Size_X
         *描    述 : 获取平板扫描仪的宽度
         *输    入 : h_Drv             扫描仪驱动HANDLE
         *输    出 : pi_Flatbed_Size_X 平板扫描仪的宽度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Flatbed_Size_X(int h_Drv, int* pi_Flatbed_Size_X);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Flatbed_Size_Y
         *描    述 : 获取平板扫描仪的高度
         *输    入 : h_Drv             扫描仪驱动HANDLE
         *输    出 : pi_Flatbed_Size_Y 平板扫描仪的高度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Flatbed_Size_Y(int h_Drv, int* pi_Flatbed_Size_Y);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Scan_Source
         *描    述 : 获取纸张来源 0-自动
         *                        1-自动进稿器
         *                        2-平板
         *输    入 : h_Drv         扫描仪驱动HANDLE
         *输    出 : pi_Scan_Source纸张来源
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Scan_Source(int h_Drv, int* pi_Scan_Source);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Scan_Mode
         *描    述 : 获取扫描模式0x01黑白正面 
         *                       0x02 
         *                       0x04灰阶正面 
         *                       0x08 
         *                       0x10彩色正面 
         *                       0x20 
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *输    出 : pi_Scan_Mode扫描模式
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Scan_Mode(int h_Drv, int* pi_Scan_Mode);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Resolution
         *描    述 : 根据扫描模式获取扫描分辨率
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           i_Scan_Mode 扫描模式
         *输    出 : pi_Res      分辨率
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Resolution(int h_Drv, int i_Scan_Mode, int* pi_Res);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Brightness
         *描    述 : 根据扫描模式获取亮度
         *输    入 : h_Drv         扫描仪驱动HANDLE
         *           i_Scan_Mode     扫描模式
         *输    出 : pi_Brightness 亮度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Brightness(int h_Drv, int i_Scan_Mode, int* pi_Brightness);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Contrast
         *描    述 : 根据扫描模式获取对比度
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           i_Scan_Mode 扫描模式
         *输    出 : pi_Contrast 对比度
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Contrast(int h_Drv, int i_Scan_Mode, int* pi_Contrast);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Frame
         *描    述 : 获取扫描框偏移量
         *输    入 : h_Drv     扫描仪驱动HANDLE
         *输    出 : pl_left   左侧偏移量
         *           pl_top    上侧偏移量
         *           pl_right  右侧偏移量
         *           pl_bottom 下侧偏移量
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Frame(int h_Drv,
                              int* pl_left,
                              int* pl_top,
                              int* pl_right,
                              int* pl_bottom
                             );
        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Scan_Source
         *描    述 : 设置纸张来源 0-自动
         *                        1-自动进稿器
         *                        2-平板
         *输    入 : h_Drv        扫描仪驱动HANDLE
         *           i_Source     纸张来源
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Scan_Source(int h_Drv, int i_Source);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Scan_Mode
         *描    述 : 设置扫描模式0x01黑白正面 
         *                       0x02 
         *                       0x04灰阶正面 
         *                       0x08 
         *                       0x10彩色正面 
         *                       0x20 
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Scan_Mode(int h_Drv, int i_Scan_Mode);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Resolution
         *描    述 : 根据扫描模式设置扫描分辨率
         *输    入 : h_Drv        扫描仪驱动HANDLE
         *           i_Scan_Mode  扫描模式
         *           i_Resolution 分辨率
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Resolution(int h_Drv, int i_Resolution, int i_Scan_Mode);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Brightness
         *描    述 : 根据扫描模式设置亮度
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           i_Scan_Mode 扫描模式
         *           i_Brightness亮度
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Brightness(int h_Drv, int i_Brightness, int i_Scan_Mode);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Contrast
         *描    述 : 根据扫描模式设置对比度
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           i_Scan_Mode 扫描模式
         *           i_Contrast  对比度
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Contrast(int h_Drv, int i_Contrast, int i_Scan_Mode);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Frame
         *描    述 : 设置扫描框偏移量
         *输    入 : h_Drv    扫描仪驱动HANDLE
         *           i_left   左侧偏移量
         *           i_top    上侧偏移量
         *           i_right  右侧偏移量
         *           i_bottom 下侧偏移量
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Frame(int h_Drv, int i_Left, int i_Top, int i_Right, int i_Bottom);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Profile_Count
         *描    述 : 获取用户自定义配置的数量
         *输    入 : h_Drv           扫描仪驱动HANDLE
         *输    出 : i_Profile_Count 用户自定义配置的数量
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Profile_Count(int h_Drv, int* i_Profile_Count);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Profile_Title
         *描    述 : 获取用户自定义配置的标题
         *输    入 : h_Drv             扫描仪驱动HANDLE
         *           nItem             扫描仪序号
         *输    出 : str_Profile_Title 用户自定义配置的标题
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Profile_Title(int h_Drv, int i_Item, char** pstr_Profile_Title);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Profile
         *描    述 : 选择用户自定义的配置设置为当前配置
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           str_Profile 用户配置的标题
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Profile(int h_Drv, char* str_Profile);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Save_Profile
         *描    述 : 保存用户的配置，并设置成当前配置
         *输    入 : h_Drv       扫描仪驱动HANDLE
         *           str_Profile 用户配置的标题
         *输    出 : 无
         *返 回 值 : 返回TRUE    成功
         *           返回FALSE   失败
         */
        public unsafe static extern int Save_Profile(int h_Drv, char* str_Profile);
        [DllImport("AviDriver.dll")]
        public unsafe static extern int Save_ProfileEx(int h_Drv, char* str_Profile, int bCheckExist);		//oscar 2010.03.03

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Current_Profile_Index
         *描    述 : 获取当前用户配置的序号
         *输    入 : h_Drv            扫描仪驱动HANDLE
         *输    出 : i_C_Profile_Idx  用户配置的序号
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Get_Current_Profile_Index(int h_Drv, int* i_C_Profile_Idx);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Write_Data
         *描    述 : 向配置文件中写入数据
         *输    入 : h_Drv     扫描仪驱动HANDLE
         *           str_Title 配置文件项标题 
         *           i_Data    写入的数据
         *输    出 : 无
         *返 回 值 : 无
         */
        public unsafe static extern int Write_Data(int h_Drv, char* str_Title, int i_Data);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Auto_Cropping
         *描    述 : 设置是否按原稿尺寸扫描
         *输    入 : h_Drv      扫描仪驱动HANDLE
         *           b_Cropping 1-按照原稿尺寸扫描
         *                      0-不按照原稿尺寸扫描
         *输    出 : 无
         *返 回 值 : 返回TRUE  成功
         *           返回FALSE 失败
         */
        public unsafe static extern int Set_Auto_Cropping(int h_Drv, int b_Cropping);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Start_Up_Scanner
        *描    述 : 启动扫描仪
        *输    入 : h_Drv 扫描仪驱动HANDLE
        *           h_Wnd windows有效句柄       
        *           Scan_Mode Scan_Mode_TWAIN-TWAIN模式扫描
        *                     Scan_Mode_USER -用户模式扫描
        *输    出 : 无
        *返 回 值 : 返回TRUE  成功
        *           返回FALSE 失败
        */
        public unsafe static extern int Start_Up_Scanner(int h_Drv, int h_Wnd, int Scan_Mode, ONEPAGECALLBACK fnOnePageCallBack);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Scanner_FeederLoad
         *描    述 : 启动扫描仪
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回TRUE  有纸张
         *           返回FALSE 没有败
         */
        public unsafe static extern int Get_Scanner_FeederLoad(int h_Drv, int h_Wnd);	//oscar 2009.12.17

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Driver_Type					//Get Driver Type			2009.12.18	by oscar
         *描    述 : 驱动类型
         *输    入 : h_Drv 扫描仪驱动HANDLE
         *输    出 : 无
         *返 回 值 : 返回 Scan_Driver_Type_Twain->Twain Dri
         *           返回 Scan_Driver_Type_KOFAX->Kofax Dri
         */
        public unsafe static extern int Get_Driver_Type(int h_Drv);

        [DllImport("AviDriver.dll")]
        /*函数名称 : Set_Open_BarcodeFunction
         *描    述 : 开通条码识别功能
         *输    入 : bOpen       开通
         *输    出 : 无
         *返 回 值 : 返回TRUE    成功
         *           返回FALSE   失败
         */
        public unsafe static extern int Set_Open_BarcodeFunction(int bOpen);				//oscar 2010.03.18

        [DllImport("AviDriver.dll")]
        public unsafe static extern int Get_Scanned_Count(int h_Drv);			//oscar 2010.04.08

        [DllImport("AviDriver.dll")]
        /*函数名称 : Get_Scanned_Count
        *描    述 : 获取扫描仪序列号
        *输    入 : h_Drv 扫描仪驱动HANDLE
        *输    出 : pSerNumber扫描仪序列号信息
        *输    出 : 无
        *返 回 值 : 返回TRUE  有纸张
        *           返回FALSE 没有败
        */
        public unsafe static extern int Get_Scanned_ServialNum(int h_Drv, char* pSerNumber);			/*oscar 2010.05.11*/


        [DllImport("AviDriver.dll")]
        /*函数名称: Show_Twain_UI
        *描    述 : 启动扫描仪
        *输    入 : h_Drv 扫描仪驱动HANDLE
        *           h_Wnd windows有效句柄       
        *输    出 : 无
        *返 回 值 : 返回TRUE  成功
        *           返回FALSE 失败
        */
        public unsafe static extern int Show_Twain_UI(int h_Drv, int h_Wnd);			/*oscar 2010.05.11*/
        //BOOL WINAPI EXPORT Show_Twain_UI( HANDLE h_Drv, HWND h_Wnd )

        /// <summary>
        /// 指定本次扫描页数
        /// </summary>
        /// <param name="nScanNum">值为-1代表不限定页数(</param>
        /// <returns></returns>
        [DllImport("AviDriver.dll")]
        public unsafe static extern int Set_Scanner_ScanNum(int nScanNum);			/*oscar 2010.05.11*/
        //BOOL WINAPI EXPORT Set_Scanner_ScanNum( INT nScanNum )  

        /// <summary>
        /// 获取扫描仪状态(卡纸\ADF盖开\等),见scanner.ini里面的错误列表。
        /// </summary>
        /// <param name="h_Drv"></param>
        /// <returns></returns>
        [DllImport("AviDriver.dll")]
        public unsafe static extern int Get_ScannerStatus(int h_Drv);
        //function Get_ScannerStatus(h_Drv: THANDLE): DWORD; stdcall;	external DllName; //



        /// <summary>
        /// 获取ADF扫描仪是否有纸,
        /// </summary>
        /// <param name="h_Drv"></param>
        /// <returns> 0.没纸, 1.有纸 </returns>
        [DllImport("AviDriverEx.dll")]
        public unsafe static extern int Get_ADFStatus(int h_Drv);



        /* 以下部分为展示如何写ONEPAGECALLBACK回调函数的代码.
         * 如果不需要可以将下面代码删除.
         */
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(int handle);
        [DllImport("kernel32.dll")]
        public static extern int GlobalUnlock(int handle);

        //public unsafe static void OnePageCallbackDemo(int hBitmap, ref IntPtr pBarCode, ref int nXPos, ref int nYPos, ref int nBarcodeCount, int nScanIndex)
        public unsafe static void OnePageCallbackDemo(int hBitmap, char** pBarCode, int* nXPos, int* nYPos, int* nBarcodeCount, int nScanIndex)
        {
            string strIndex = nScanIndex.ToString();
            string tempFolder = System.IO.Path.GetTempPath() + "InvtaxScan\\";
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);
            string tempFileName = nScanIndex.ToString("0000");
            string bmpfile = tempFolder + tempFileName + ".bmp";
            FileStream fl = new FileStream(bmpfile, FileMode.Create);
            IntPtr bmp = GlobalLock(hBitmap);
            int nWidth = Marshal.ReadInt32(bmp, 4);
            int nHeight = Marshal.ReadInt32(bmp, 8);
            short nBPP = Marshal.ReadInt16(bmp, 14);
            int nColor = (nBPP > 8) ? 0 : (1 << nBPP);
            int nImageSize = (nWidth * nBPP + 31) / 32 * 4 * nHeight;
            byte[] pHeader = new byte[14];
            pHeader[0] = (byte)'B';
            pHeader[1] = (byte)'M';
            int nTemp = 14 + 40 + nColor * 4 + nImageSize;
            pHeader[2] = (byte)nTemp;
            pHeader[3] = (byte)(nTemp >> 8);
            pHeader[4] = (byte)(nTemp >> 16);
            pHeader[5] = (byte)(nTemp >> 24);
            pHeader[6] = 0;
            pHeader[7] = 0;
            pHeader[8] = 0;
            pHeader[9] = 0;
            nTemp = 14 + 40 + nColor * 4;
            pHeader[10] = (byte)nTemp;
            pHeader[11] = (byte)(nTemp >> 8);
            pHeader[12] = (byte)(nTemp >> 16);
            pHeader[13] = (byte)(nTemp >> 24);
            int nCount = 14 + 40 + nColor * 4 + nImageSize;
            byte[] a = new byte[nCount];
            for (int i = 0; i < 14; i++)
                a[i] = pHeader[i];
            for (int i = 0; i < nCount - 14; i++)
                a[i + 14] = Marshal.ReadByte(bmp, i);// bData[i]; //pData[i];
            fl.Write(a, 0, nCount);
            GlobalUnlock(hBitmap);
            //  bmp = null;
            fl.Flush();
            fl.Close();
            pHeader = null;
            a = null;
            fl = null;

            //2015.05.23 bmp转换成jpg
            if (File.Exists(bmpfile))
            {
                string AJpgFile = tempFolder + tempFileName + ".jpg";
                //string AJpgFile = "D:\\" + DateTime.Now.ToString("YYYY-MM-DD_hh-mm-ss") + ".jpg";
                //100为压缩质量
                ScanObj.BmpFileToJpgFile(bmpfile, AJpgFile, 80);
            }
            try
            {
                File.Delete(bmpfile);
            }
            catch { }
        }
    }
}
