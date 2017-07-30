using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace inv_tax_Agent_Cls
{
    public static class webservice
    {
        //user
        public static string Login(string username, string password)
        {
            string userid = "";
            string login = "";
            string errorMsg = "";
            string guid = "";
            string name = "";
            int totlePages = 0;
            int usedPages = 0;
            string expireDate = "";
            string priority = "";
            string ExpireAlertPages = "";
            string Privilege = "";
            string sh = "";
            string Mode = "";
            string keyword = "";
            string sql1 = "select A.id,A.companyguid ,B.name,B.sh,B.Mode,B.totlepage,B.expireDate,B.priority ,B.expirealertpages,A.privilege,B.TryKeyword from inv_users as A,inv_company as B WHERE A.username ='" + username + "' and A.password ='" + password + "' and A.companyguid = B.guid";
            MySqlDataReader dr = null;
            try
            {
                dr = inv_tax_DBConn.MySqlHelper.ExecuteReader(sql1);
            }
            catch (Exception ex)
            {
                errorMsg = "服务器连接失败，原因：" + ex.Message;
                login = "false";
                goto Exit;
            }

            object obj = null;
            try
            {
                dr.Read();
                userid = dr["id"].ToString();
                guid = dr["companyguid"].ToString();
                name = dr["name"].ToString();
                totlePages = int.Parse(dr["totlepage"].ToString());
                expireDate = dr["expireDate"].ToString();
                priority = dr["priority"].ToString();
                ExpireAlertPages = dr["expirealertpages"].ToString();
                Privilege = dr["privilege"].ToString();
                sh = dr["sh"].ToString();
                Mode = dr["Mode"].ToString();
                keyword = dr["TryKeyword"].ToString();
                //查询该用户所在公司的授权使用量是否已满
                string sq2 = "select count(companyguid) as count from inv_main where companyguid ='" + guid + "' and checkstauts = '待查验' group by companyguid";
                obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(sq2);
                if (obj != null)
                {
                    usedPages = int.Parse(obj.ToString());
                }
                sq2 = "select count(companyguid) as count from inv_tax_log where companyguid ='" + guid + "' group by companyguid";
                obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(sq2);
                if (obj != null)
                {
                    usedPages += int.Parse(obj.ToString());
                }
            }
            catch (Exception ex)
            {
                errorMsg = "用户名或密码不正确，请重新输入。";
                login = "false";
                goto Exit;
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }
            login = "true";

        Exit:
            string msg ="<userinfo>" +
                        "<login>" + login + "</login>" +
                        "<userid>" + userid + "</userid>" +
                        "<username>" + username + "</username>" +
                        "<password>" + password + "</password>" +
                        "<sessionid>" + guid + "</sessionid>" +
                        "<companyname>" + name + "</companyname>" +
                        "<companyguid>" + guid + "</companyguid>" +
                        "<totlePages>" + totlePages.ToString() + "</totlePages>" +
                        "<usedPages>" + usedPages.ToString() + "</usedPages>" +
                        "<expireDate>" + expireDate + "</expireDate>" +
                        "<ExpireAlertPages>" + ExpireAlertPages + "</ExpireAlertPages>" +
                        "<priority>" + priority + "</priority>" +
                        "<Privilege>" + Privilege + "</Privilege>" +
                        "<SH>" + sh + "</SH>" +
                        "<Mode>" + Mode + "</Mode>" +
                        "<keyword>" + keyword + "</keyword>" +
                       "<errormessage>" + errorMsg + "</errormessage></userinfo>";
            return msg;
        }
        public static string LoginExLocal(string username, string password,string mode,string usedpage,string ip)
        {
            if (mode == "0")
                return Login(username, password);

            string userid = "";
            string login = "";
            string errorMsg = "";
            string guid = "";
            string name = "";
            int totlePages = 0;
            int usedPages = 0;
            string expireDate = "";
            string priority = "";
            string ExpireAlertPages = "";
            string Privilege = "";
            string sh = "";
            string Mode = "";
            string keyword = "";
            string sql1 = "select A.id,A.companyguid ,B.name,B.sh,B.Mode,B.totlepage,B.expireDate,B.priority ,B.expirealertpages,A.privilege,B.TryKeyword from inv_users as A,inv_company as B WHERE A.username ='" + username + "' and A.password ='" + password + "' and A.companyguid = B.guid";
            MySqlDataReader dr = null;
            try
            {
                dr = inv_tax_DBConn.MySqlHelper.ExecuteReader(sql1);
            }
            catch (Exception ex)
            {
                errorMsg = "服务器连接失败，原因：" + ex.Message;
                login = "false";
                goto Exit;
            }
            object obj = null;
            string stauts = "异常";
            try
            {
                dr.Read();
                userid = dr["id"].ToString();
                guid = dr["companyguid"].ToString();
                name = dr["name"].ToString();
                totlePages = int.Parse(dr["totlepage"].ToString());
                expireDate = dr["expireDate"].ToString();
                priority = dr["priority"].ToString();
                ExpireAlertPages = dr["expirealertpages"].ToString();
                Privilege = dr["privilege"].ToString();
                sh = dr["sh"].ToString();
                Mode = dr["Mode"].ToString();
                keyword = dr["TryKeyword"].ToString();
                //查询该用户最后数据使用量
                string sq2 = "select userpages from inv_login_log where companyguid ='" + guid + "' ORDER BY id DESC limit 0,1";
                obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(sq2);
                int oldusedpages = 0;
                int nowusedpages= int.Parse(usedpage);
                
                if (obj != null)
                {
                    oldusedpages = int.Parse(obj.ToString());
                }
                if (nowusedpages < oldusedpages)
                {
                    login = "false";
                    totlePages = 0;
                    errorMsg = "系统检测到恶意篡改授权使用量，贵公司所有账户已被停用，请联系软件服务商解除警报！";
                    //usedPages = 0;
                }
                else
                {
                    stauts = "正常";
                }
                usedPages = oldusedpages;
                try
                {
                    string nowtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string insertsql = "insert into inv_login_log(companyname,companyguid,username,loginTime,userpages,ip,stauts) values ('" + name + "','" + guid + "','" + username + "','" + nowtime + "','" + usedPages + "','" + ip + "','" + stauts + "')";
                    inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(insertsql);
                }
                catch { }
            }
            catch (Exception ex)
            {
                errorMsg = "用户名或密码不正确，请重新输入。";
                login = "false";
                goto Exit;
            }
            finally
            {
                if (dr != null)
                    dr.Close();
            }
            if (stauts=="正常")
                login = "true";

        Exit:
            string msg = "<userinfo>" +
                        "<login>" + login + "</login>" +
                        "<userid>" + userid + "</userid>" +
                        "<username>" + username + "</username>" +
                        "<password>" + password + "</password>" +
                        "<sessionid>" + guid + "</sessionid>" +
                        "<companyname>" + name + "</companyname>" +
                        "<companyguid>" + guid + "</companyguid>" +
                        "<totlePages>" + totlePages.ToString() + "</totlePages>" +
                        "<usedPages>" + usedPages.ToString() + "</usedPages>" +
                        "<expireDate>" + expireDate + "</expireDate>" +
                        "<ExpireAlertPages>" + ExpireAlertPages + "</ExpireAlertPages>" +
                        "<priority>" + priority + "</priority>" +
                        "<Privilege>" + Privilege + "</Privilege>" +
                        "<SH>" + sh + "</SH>" +
                        "<keyword>" + keyword + "</keyword>" +
                       "<errormessage>" + errorMsg + "</errormessage></userinfo>";
            return msg;
        }
        public static int UserUpdatePassword(string guid, string username, string oldpassword, string newpassword)
        {
            int n = 0;
            string updatePasswordSql = "update inv_users set password = '" + newpassword + "' where companyguid='" + guid + "' and username='" + username + "' and password ='" + oldpassword + "'";
            try
            {
                n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updatePasswordSql);
            }
            catch { }
            return n;
        }
        //FP
        public static string AddRecord(string fplx,string fpdm, string fphm, string kprq, string je, string sessionId,string createuser)
        {
            string flag = "false";
            //string fplx = getFPLX(fpdm);
            string errorMsg = "";
            string insertsql = "";
            string createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                if (fplx == "01")
                {
                    insertsql = "insert into inv_main (fplx,fpdm,fphm,kprq,je,jym,companyguid,checkstauts,createuser,createtime) values ('"
                                         + fplx + "','" + fpdm + "','" + fphm + "','" + kprq + "','" + je + "','','" + sessionId + "','待查验','" + createuser + "','" + createtime + "')";
                }
                else
                {
                    insertsql = "insert into inv_main (fplx,fpdm,fphm,kprq,je,jym,companyguid,checkstauts,createuser,createtime) values ('"
                                         + fplx + "','" + fpdm + "','" + fphm + "','" + kprq + "','0.00','" + je + "','" + sessionId + "','待查验','" + createuser + "','" + createtime + "')";
                }
                int nflag = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(insertsql);
                if (nflag <= 0)
                {
                    errorMsg = "增加数据失败。";
                }
                else
                {
                    flag = "true";
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            string msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;
        }
        public static string AddRecordByBarcode(string barcode, string sessionId,string createuser)
        {
            string flag = "false";
            barcode = barcode.Replace("，", ",");
            string[] tmp = barcode.Split(',');
            string fplx = tmp[1];
            string fpdm = tmp[2];
            string fphm = tmp[3];
            string kprq = tmp[5];
            string je = "";
            if (fplx == "01")
            {
                je = tmp[4];
            }
            else
            {
                je = tmp[6].Substring(tmp[6].Length - 6, 6);
            }
            string errorMsg = "";
            string insertsql = "";
            string createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                if (fplx == "01")
                {
                    insertsql = "insert into inv_main (fplx,fpdm,fphm,kprq,je,jym,companyguid,checkstauts,createuser,createtime) values ('"
                                         + fplx + "','" + fpdm + "','" + fphm + "','" + kprq + "','" + je + "','','" + sessionId + "','待查验','" + createuser + "','" + createtime + "')";
                }
                else
                {
                    insertsql = "insert into inv_main (fplx,fpdm,fphm,kprq,je,jym,companyguid,checkstauts,createuser,createtime) values ('"
                                         + fplx + "','" + fpdm + "','" + fphm + "','" + kprq + "','0.00','" + je + "','" + sessionId + "','待查验','" + createuser + "','" + createtime + "')";
                }
                int nflag = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(insertsql);
                if (nflag <= 0)
                {
                    errorMsg = "增加数据失败。";
                }
                else
                     flag = "true";
                    
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            string msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;
        }
        public static string GetRecord(string fpdm, string fphm, string sessionId)
        {
            string id = "";
            string fplx = "";
            string dq = "";
            string fpmc = "";
            string kprq = "";
            string je = "";
            string se = "";
            string jshj = "";
            string jym = "";
            string jqm = "";
            string gfmc = "";
            string gfsh = "";
            string gfdz = "";
            string gfzh = "";
            string xfmc = "";
            string xfsh = "";
            string xfdz = "";
            string xfzh = "";
            string createuser = "";
            string createtime = "";
            string checkstauts = "";
            string checktime = "";
            string cycs = "";
            string remarks = "";

            string fpmx = "";
            //string sql = "select fplx,dq,fpmc,fpdm,fphm,kprq,je,se,jshj,gfmc,gfsh,gfdz,gfzh,xfmc,xfsh,xfdz,xfzh,jym,jqm,remarks,cycs,checktime from inv_main where id='" + id + "'";
            string sql = "select * from inv_main where fpdm ='" + fpdm + "' and fphm ='" + fphm + "' and companyguid ='" + sessionId + "' order by id desc limit 0,1";

            MySqlDataReader dr = null;
            try
            {
                dr = inv_tax_DBConn.MySqlHelper.ExecuteReader(sql);
                if (dr == null) return "";
                dr.Read();
                id = dr["id"].ToString();
                fplx = dr["fplx"].ToString();
                dq = dr["dq"].ToString();
                fpmc = dr["fpmc"].ToString();
                fpdm = dr["fpdm"].ToString();
                fphm = dr["fphm"].ToString();
                kprq = dr["kprq"].ToString();
                je = dr["je"].ToString();
                se = dr["se"].ToString();
                jshj = dr["jshj"].ToString();

                gfmc = dr["gfmc"].ToString();
                gfsh = dr["gfsh"].ToString();
                gfdz = dr["gfdz"].ToString();
                gfzh = dr["gfzh"].ToString();
                xfmc = dr["xfmc"].ToString();
                xfsh = dr["xfsh"].ToString();
                xfdz = dr["xfdz"].ToString();
                xfzh = dr["xfzh"].ToString();

                jym = dr["jym"].ToString();
                jqm = dr["jqm"].ToString();
                remarks = dr["remarks"].ToString();
                checktime = dr["checktime"].ToString();
                createuser = dr["createuser"].ToString();
                createtime = dr["createtime"].ToString();
                cycs = dr["cycs"].ToString();
                checkstauts = dr["checkstauts"].ToString();

                if (cycs != null)
                {
                    if (cycs == "0")
                    {
                        cycs = "1";
                    }
                }

                string selectDetailSql = "select id,mc,gg,dw,amount,dj,je,sl,se from inv_detail where main_id= " + id;
                DataTable dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectDetailSql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string did = dt.Rows[i]["id"].ToString();
                        string mc = dt.Rows[i]["mc"].ToString();
                        string gg = dt.Rows[i]["gg"].ToString(); ;
                        string dw = dt.Rows[i]["dw"].ToString(); ;
                        string amount = dt.Rows[i]["amount"].ToString(); ;
                        string dj = dt.Rows[i]["dj"].ToString(); ;
                        string dje = dt.Rows[i]["je"].ToString(); ;
                        string sl = dt.Rows[i]["sl"].ToString();
                        if (sl != "")
                        {
                            sl = sl + "%";
                        }
                        string dse = dt.Rows[i]["se"].ToString(); ;
                        fpmx += "<fpmx>" +
                                         "<id>" + did + "</id>" +
                                         "<mc>" + mc + "</mc>" +
                                         "<gg>" + gg + "</gg>" +
                                         "<dw>" + dw + "</dw>" +
                                         "<amount>" + amount + "</amount>" +
                                         "<dj>" + dj + "</dj>" +
                                         "<je>" + dje + "</je>" +
                                         "<sl>" + sl + "</sl>" +
                                         "<se>" + dse + "</se>" +
                                     "</fpmx>";
                    }
                }
            }
            catch (Exception ex)
            {
                //return false;
            }
            finally
            {
                dr.Close();
            }
            string result = "<?xml version='1.0' encoding='utf-8'?>" +
                            "<fpxx>" +
                                "<checkstauts>" + checkstauts + "</checkstauts>" +
                                "<id>" + id + "</id>" +
                                "<fplx>" + fplx + "</fplx>" +
                                "<dq>" + dq + "</dq>" +
                                "<fpmc>" + fpmc + "</fpmc>" +
                                "<fpdm>" + fpdm + "</fpdm>" +
                                "<fphm>" + fphm + "</fphm>" +
                                "<kprq>" + kprq + "</kprq>" +
                                "<je>" + je + "</je>" +
                                "<se>" + se + "</se>" +
                                "<jshj>" + jshj + "</jshj>" +
                                "<jym>" + jym + "</jym>" +
                                "<jqm>" + jqm + "</jqm>" +
                                "<gfmc>" + gfmc + "</gfmc>" +
                                "<gfsh>" + gfsh + "</gfsh>" +
                                "<gfdz>" + gfdz + "</gfdz>" +
                                "<gfzh>" + gfzh + "</gfzh>" +
                                "<xfmc>" + xfmc + "</xfmc>" +
                                "<xfsh>" + xfsh + "</xfsh>" +
                                "<xfdz>" + xfdz + "</xfdz>" +
                                "<xfzh>" + xfzh + "</xfzh>" +
                                "<createuser>" + createuser + "</createuser>" +
                                "<createtime>" + createtime + "</createtime>" +
                                "<checktime>" + checktime + "</checktime>" +
                                "<cycs>" + cycs + "</cycs>" +
                                "<remarks>" + remarks + "</remarks>" +
                                "<fpmxs>" + fpmx +

                                "</fpmxs>" +
                            "</fpxx>";
            return result;
        }
        public static string GetRecordById(string sessionId,string id)
        {
            string fplx = "";
            string dq = "";
            string fpmc = "";
            string fpdm = "";
            string fphm = "";
            string kprq = "";
            string je = "";
            string se = "";
            string jshj = "";
            string jym = "";
            string jqm = "";
            string gfmc = "";
            string gfsh = "";
            string gfdz = "";
            string gfzh = "";
            string xfmc = "";
            string xfsh = "";
            string xfdz = "";
            string xfzh = "";
            string createuser = "";
            string createtime = "";
            string checkstauts = "";
            string checktime = "";
            string cycs = "";
            string remarks = "";

            string fpmx = "";
            //string sql = "select fplx,dq,fpmc,fpdm,fphm,kprq,je,se,jshj,gfmc,gfsh,gfdz,gfzh,xfmc,xfsh,xfdz,xfzh,jym,jqm,remarks,cycs,checktime from inv_main where id='" + id + "'";
            string sql = "select * from inv_main where id ='" + id + "' and companyguid ='" + sessionId + "'";

            MySqlDataReader dr = null;
            try
            {
                dr = inv_tax_DBConn.MySqlHelper.ExecuteReader(sql);
                if (dr == null) return "";
                dr.Read();
                id = dr["id"].ToString();
                fplx = dr["fplx"].ToString();
                dq = dr["dq"].ToString();
                fpmc = dr["fpmc"].ToString();
                fpdm = dr["fpdm"].ToString();
                fphm = dr["fphm"].ToString();
                kprq = dr["kprq"].ToString();
                je = dr["je"].ToString();
                se = dr["se"].ToString();
                jshj = dr["jshj"].ToString();

                gfmc = dr["gfmc"].ToString();
                gfsh = dr["gfsh"].ToString();
                gfdz = dr["gfdz"].ToString();
                gfzh = dr["gfzh"].ToString();
                xfmc = dr["xfmc"].ToString();
                xfsh = dr["xfsh"].ToString();
                xfdz = dr["xfdz"].ToString();
                xfzh = dr["xfzh"].ToString();

                jym = dr["jym"].ToString();
                jqm = dr["jqm"].ToString();
                remarks = dr["remarks"].ToString();
                checktime = dr["checktime"].ToString();
                createuser = dr["createuser"].ToString();
                createtime = dr["createtime"].ToString();
                cycs = dr["cycs"].ToString();
                checkstauts = dr["checkstauts"].ToString();

                if (cycs != null)
                {
                    if (cycs == "0")
                    {
                        cycs = "1";
                    }
                }

                string selectDetailSql = "select id,mc,gg,dw,amount,dj,je,sl,se from inv_detail where main_id= " + id;
                DataTable dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectDetailSql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string did = dt.Rows[i]["id"].ToString();
                        string mc = dt.Rows[i]["mc"].ToString();
                        string gg = dt.Rows[i]["gg"].ToString(); ;
                        string dw = dt.Rows[i]["dw"].ToString(); ;
                        string amount = dt.Rows[i]["amount"].ToString(); ;
                        string dj = dt.Rows[i]["dj"].ToString(); ;
                        string dje = dt.Rows[i]["je"].ToString(); ;
                        string sl = dt.Rows[i]["sl"].ToString();
                        if (sl != "")
                        {
                            sl = sl + "%";
                        }
                        string dse = dt.Rows[i]["se"].ToString(); ;
                        fpmx += "<fpmx>" +
                                         "<id>" + did + "</id>" +
                                         "<mc>" + mc + "</mc>" +
                                         "<gg>" + gg + "</gg>" +
                                         "<dw>" + dw + "</dw>" +
                                         "<amount>" + amount + "</amount>" +
                                         "<dj>" + dj + "</dj>" +
                                         "<je>" + dje + "</je>" +
                                         "<sl>" + sl + "</sl>" +
                                         "<se>" + dse + "</se>" +
                                     "</fpmx>";
                    }
                }
            }
            catch (Exception ex)
            {
                //return false;
            }
            finally
            {
                dr.Close();
            }
            string result = "<?xml version='1.0' encoding='utf-8'?>" +
                            "<fpxx>" +
                                "<checkstauts>" + checkstauts + "</checkstauts>" +
                                "<id>" + id + "</id>" +
                                "<fplx>" + fplx + "</fplx>" +
                                "<dq>" + dq + "</dq>" +
                                "<fpmc>" + fpmc + "</fpmc>" +
                                "<fpdm>" + fpdm + "</fpdm>" +
                                "<fphm>" + fphm + "</fphm>" +
                                "<kprq>" + kprq + "</kprq>" +
                                "<je>" + je + "</je>" +
                                "<se>" + se + "</se>" +
                                "<jshj>" + jshj + "</jshj>" +
                                "<jym>" + jym + "</jym>" +
                                "<jqm>" + jqm + "</jqm>" +
                                "<gfmc>" + gfmc + "</gfmc>" +
                                "<gfsh>" + gfsh + "</gfsh>" +
                                "<gfdz>" + gfdz + "</gfdz>" +
                                "<gfzh>" + gfzh + "</gfzh>" +
                                "<xfmc>" + xfmc + "</xfmc>" +
                                "<xfsh>" + xfsh + "</xfsh>" +
                                "<xfdz>" + xfdz + "</xfdz>" +
                                "<xfzh>" + xfzh + "</xfzh>" +
                                "<createuser>" + createuser + "</createuser>" +
                                "<createtime>" + createtime + "</createtime>" +
                                "<checktime>" + checktime + "</checktime>" +
                                "<cycs>" + cycs + "</cycs>" +
                                "<remarks>" + remarks + "</remarks>" +
                                "<fpmxs>" + fpmx +
                                "</fpmxs>" +
                            "</fpxx>";
            return result;
        }
        public static string GetRecordsByCreateDate(string CreateDate, string sessionId)
        {
            //string sessionId = "";
            return sessionId;
        }
        public static string DeleteRecordById(string id, string sessionId)
        {
            string errorMsg = "";
            string flag = "false";
            int n = 0;
            try
            {
                //删除主表信息
                string deleteMainSql = "delete from inv_main  where id IN (" + id + ") and companyguid ='" + sessionId + "'";
                //string deleteMainSql = "delete from inv_main  where id ='" + id + "' and companyguid ='" + sessionId + "'";
                n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(deleteMainSql);
                if (n > 0)
                {
                    //删除明细信息
                    string deleteDetailSql = "delete from inv_detail  where main_id IN (" + id + ")";
                    int m = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(deleteDetailSql);
                    errorMsg = "成功删除 " + n.ToString() + "张发票，货物明细记录" + m.ToString() + "条。";
                    flag ="true";
                }
                else
                {
                    errorMsg = "未知原因：未能删除所选择的发票记录！";
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            string msg = "<ServerInfo><flag>"+flag+"</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;

        }
        public static DataTable GetRecords(string guid, string stauts,int page,int pagesize)
        {
            DataTable dt = null;
            
            //string selectSql = "select id,fplx,checkstauts,dq,fpmc,fpdm,fphm,kprq,je,se,jshj,gfmc,gfsh,gfdz,gfzh,xfmc,xfsh,xfdz,xfzh,createuser,createtime,checktime,jym,remarks " +
            //               "from inv_main where checkstauts in(" + stauts + ") and companyguid='" + guid + "' order by id DESC";

            string selectSql = "select t1.* FROM `inv_main` AS t1" +
                                " JOIN (SELECT id FROM `inv_main`  where checkstauts in (" + stauts + ") and companyguid='" + guid + "' ORDER BY id desc LIMIT " + ((page - 1) * pagesize).ToString() + "," + pagesize.ToString()+") AS t2" +
                                " WHERE t1.id = t2.id ORDER BY t1.id desc";
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;
        }
        public static int GetRecordsCountByStauts(string guid, string stauts)
        {
            int count = 0;
            string selectSql = "select count(id) as count " +
                          "from inv_main where checkstauts in(" + stauts + ") and companyguid='" + guid + "' order by id DESC";

            try
            {
                object obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(selectSql);
                if (obj != null)
                    count = int.Parse(obj.ToString());
            }
            catch
            { }


            return count;
        }
        public static int GetSearchRecordsCountBySql(string guid, string sql)
        {
            int count = 0;
            string selectSql = "select count(id) as count " +
                           "from inv_main where (";
            selectSql += sql + ") and companyguid='" + guid + "' order by id DESC";
            try
            {
                object obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(selectSql);
                if (obj != null)
                    count = int.Parse(obj.ToString());
            }
            catch
            { }
            return count;
        }
        public static DataTable GetRecordDetailsByMainIDs(string guid,string id)
        {
            DataTable dt = null;
            string selectSql = "select b.fpdm as 发票代码 ,b.fphm as 发票号码,a.mc as 货物或应税劳务、服务名称,a.gg as 规格型号 ,a.dw as 单位 ,a.amount as 数量 ,a.dj as 单价,a.je as 金额 ,concat(a.sl,'%') as 税率 ,a.se as 税额, b.kprq as 开票日期,b.`xfmc` as 销方名称 ,b.`xfsh` as 销方税号,b.`xfdz` as 销方地址 ,b.`xfzh` as 销方账号" +
                                   " from inv_detail  as a left join inv_main  b on a.main_id  = b.id  where b.id IN (" + id + ") order by b.fpdm,b.fphm";
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;
        }
        public static DataTable GetRecordDetailsForDisplayByMainIDs(string guid, string id)
        {
            DataTable dt = null;
            string selectSql = "select mc,gg,dw,amount,dj,je,sl,se from inv_detail where main_id= " + id;
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;

        }
        public static DataTable GetSearchRecords(string guid, string sql, int page, int pagesize)
        {
            DataTable dt = null;
            //string sql2 = "select id,fplx,checkstauts,dq,fpmc,fpdm,fphm,kprq,je,se,jshj,gfmc,gfsh,gfdz,gfzh,xfmc,xfsh,xfdz,xfzh,createuser,createtime,checktime,jym,remarks " +
            //               "from inv_main where (";
            //sql2 += sql + ") and companyguid='" + guid + "' order by id DESC";

            string selectSql = "select t1.* FROM `inv_main` AS t1" +
                                " JOIN (SELECT id FROM `inv_main`  where (" + sql + ") and companyguid='" + guid + "' ORDER BY id desc LIMIT " + ((page - 1) * pagesize).ToString() + "," + pagesize.ToString() + ") AS t2" +
                                " WHERE t1.id = t2.id ORDER BY t1.id desc";
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;
        }
        private static string getFPLX(string sfpdm)
        {
            string a = sfpdm;
            string b;
            string c = "99";

            if (a.Length == 12)
            {
                b = a.Substring(7, 1);
                if (c == "99")
                {  //增加判断，判断是否为新版电子票
                    if (a.Substring(0, 1) == "0" && a.Substring(10, 2) == "11")
                    {
                        c = "10";
                    }
                    if (a.Substring(0, 1) == "0" && (a.Substring(10, 2) == "06" || a.Substring(10, 2) == "07"))
                    {  //判断是否为卷式发票  第1位为0且第11-12位为06或07
                        c = "11";
                    }
                }
                if (c == "99")
                { //如果还是99，且第8位是2，则是机动车发票
                    if (b == "2" && a.Substring(0, 1) != "0")
                    {
                        c = "03";
                    }
                    else
                    {
                        c = "10";
                    }
                }
            }
            else if (a.Length == 10)
            {
                b = a.Substring(7, 1);
                if (b == "1" || b == "5")
                {
                    c = "01";
                }
                else if (b == "6" || b == "3")
                {
                    c = "04";
                }
                else if (b == "7" || b == "2")
                {
                    c = "02";
                }
            }
            if (c == "99")
                c = "";
            return c;
        }
        public static DataTable CheckRepeatLRecords(string fpdm, string fphm, string sessionid,string loginusername,string writeLog)
        {
            string sql1 = "SELECT id,fpdm,fphm,createtime,createuser,checkstauts FROM inv_main where fpdm IN (" + fpdm + ") and fphm IN (" + fphm + ") and companyguid = '" + sessionid + "' GROUP BY fpdm,fphm";
            DataTable dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(sql1);
            if (writeLog == "1")
            {
                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        try
                        {
                            string insertLogsql = "insert into inv_repeat_log(fpdm,fphm,createtime,createuser,mainid) values ('" +
                                                   dt.Rows[i]["fpdm"] + "','" + dt.Rows[i]["fphm"] + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + loginusername + "','" + dt.Rows[i]["id"] + "')";
                            inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(insertLogsql);
                        }
                        catch { }
                    }
                }
            }
            return dt;
        }
        public static string UpdateRecord(string id,string fplx, string fpdm, string fphm, string kprq, string je)
        {
            string msg = "";

            string updateMainSql = "";

            if (fplx == "01")
            {
                updateMainSql = "update inv_main SET " +
                                             "fplx='" + fplx + "'," +
                                             "fpdm='" + fpdm + "'," +
                                             "fphm='" + fphm + "'," +
                                             "kprq='" + kprq + "'," +
                                             "je='" + je + "'," +
                                             "checkstauts='待查验'," +
                                             "locked=null,lockedTime=null," +
                                             "checktime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                             " where id = '" + id + "'";
            }
            else
            {
                updateMainSql = "update inv_main SET " +
                                             "fplx='" + fplx + "'," +
                                             "fpdm='" + fpdm + "'," +
                                             "fphm='" + fphm + "'," +
                                             "kprq='" + kprq + "'," +
                                             "jym='" + je + "'," +
                                             "checkstauts='待查验'," +
                                             "locked=null,lockedTime=null," +
                                             "checktime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                             " where id = '" + id + "'";
            }
            int m = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updateMainSql);

            if (m > 0)
            {
                string deleteDeitalSql = "delete from inv_detail where main_id =" + id;
                try
                {
                    inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(deleteDeitalSql);
                }
                catch { }
                msg = "发票编辑成功！";
            }
            else
            {
                msg = "发票数据编辑失败。";
                //return;
            }

            return msg;
        }
        public static string ReCheckFP(string idList)
        {
            string msg = "";
            string updateMainSql = "";
                   updateMainSql = "update inv_main SET " +
                                             "checkstauts='待查验'," +
                                             "locked=null,lockedTime=null," +
                                             "checktime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                                             " where id in (" + idList + ")";
            int m = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updateMainSql);

            if (m > 0)
            {
                string deleteDeitalSql = "delete from inv_detail where main_id in(" + idList+")";
                try
                {
                    inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(deleteDeitalSql);
                }
                catch { }
                msg = "已提交重新查验！";
            }
            else
            {
                msg = "重新查验失败！";
            }

            return msg;
        }
        public static string ChangFPException(string idList,string statusName,string guid)
        {
            string msg = "";
            string updateMainSql = "";
            updateMainSql = "update inv_main SET " +
                                      "Exception ='" + statusName + "'" +
                                      " where id in (" + idList + ") and companyguid ='" + guid + "'";
            int m = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updateMainSql);
            if (m > 0)
            {
                msg = m.ToString() +"张发票异常状态更新成功！";
            }
            else
            {
                msg = "发票异常状态更新成功！";
            }
            return msg;
        }
        public static string ChangFPExceptionByAuto(string guid,string companyname,string sh)
        {
            string s = "未执行！";
            //执行增值税专用发票，判断购方税号和购方名称是否相同；
            //string sql = "update inv_main set Exception='正常'" +
            //            " where (Exception IS NULL or Exception ='') and binary gfsh='"+sh+"' and gfmc ='"+companyname+"' and fplx ='01'";
            //int m = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(sql);

            return s;
        }
        public static DataTable GetRepeatLog(string sessionId, string username, string Privilege)
        {
            DataTable dt = null;
            string sql = "";
            if (Privilege == "3")
            {
                sql = "select B.dq, B.fpmc,B.xfmc,B.createtime as checktime,B.companyguid as checktime,A.* " +
                    "FROM inv_repeat_log as A," +
                    "(select id,dq,fpmc,fpdm,fphm,xfmc,createtime,companyguid from inv_main where companyguid='" + sessionId + "' group by fpdm,fphm) as B " +
                    "where A.fpdm=B.fpdm and A.fphm=B.fphm and A.mainid = B.id ORDER BY A.fpdm,A.fphm,A.createtime ";
            }
            else
            {
                sql = "select B.dq, B.fpmc,B.xfmc,B.createtime as checktime,B.companyguid as checktime,A.* " +
                    "FROM inv_repeat_log as A," +
                    "(select id,dq,fpmc,fpdm,fphm,xfmc,createtime,companyguid from inv_main where companyguid='" + sessionId + "' group by fpdm,fphm) as B " +
                    "where A.fpdm=B.fpdm and A.fphm=B.fphm and A.mainid = B.id and A.createuser='" + username + "' ORDER BY A.fpdm,A.fphm,A.createtime ";
            }
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(sql);
            }
            catch
            { }
            return dt;

        }
        public static string Test(string sessionId)
        {
            string msg = "00";
            return msg;

        }
        public static string TransferToDataCenter(string idList, string sessionId)
        {
            string errorMsg = "";
            string flag = "false";
            int n = 0;
            try
            {
                string updateMainSql = "update inv_main set flag=1 where id IN (" + idList + ") and companyguid ='" + sessionId + "'";
                n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updateMainSql);
                if (n > 0)
                {
                    errorMsg =n.ToString() + "张发票已成功移入发票仓库 ";
                    flag = "true";
                }
                else
                {
                    errorMsg = "未知原因：未能删除所选择的发票记录！";
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            string msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;
        }
        public static string UpdateTryKeyword(string keywords, string sessionId)
        {
            string msg = "";

            string updatekeywordSql = "update inv_company set Trykeyword = '" + keywords + "' where guid='" + sessionId + "'";
            try
            {
                int n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(updatekeywordSql);
                msg = n.ToString();
            }
            catch { }
            return msg;
        }
        public static string TryKeyword(string keywords, string sessionId)
        {
            string[] keyword = keywords.Split('|');
            string msg = "";
            string errorMsg = "";
            string flag = "false";
            int n = 0;
            string tmp = "";
            for (int i = 0; i < keyword.Length; i++)
            {
                tmp += " B.mc like '%" + keyword[i] + "%' or ";
            }
            if (tmp != "")
            {
                tmp = tmp.Substring(0, tmp.Length - 3);
            }
            else
            {
                msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>未设置敏感字符</errormessage></ServerInfo>";
                return msg;
            }

            try
            {
                string sql = "update inv_main As A,inv_detail as B  set A.Exception='敏感票' where " +
                    "A.companyguid ='" + sessionId + "' and A.flag=0 and A.Exception is null and A.id = B.main_id " +
                    "and ( " + tmp + ")";

                n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(sql);
                if (n > 0)
                {
                    errorMsg = "检测到：" + n.ToString() + "异常敏感发票。";
                    flag = "true";
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;
        }
        //public static string AddHistroyRecords(string values)
        //{
        //    string flag = "false";
        //    string errorMsg = "";
        //    string insertsql = "";
        //    string createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //    try
        //    {
        //        string sql = "insert into inv_gxrz (fpdm,fphm,kprq,je,se,xfmc,fpzt,rzfs,rztime,SQ,companyguid,fpmc,xfsh,gxzt,gxtime) values " + values;
        //        int nflag = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(sql);
        //        if (nflag <= 0)
        //        {
        //            errorMsg = "增加数据失败。";
        //        }
        //        else
        //        {
        //            flag = "true";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMsg = ex.Message;
        //    }
        //    string msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
        //    return msg;
        //}
        public static string AddHistroyRecords(string values)
        {
            string flag = "false";
            string errorMsg = "";
            string insertsql = "";
            string createtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                string sql = "insert into inv_main (fplx,fpdm,fphm,kprq,je,companyguid,checkstauts,createuser,createtime) values " + values;
                int nflag = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(sql);
                if (nflag <= 0)
                {
                    errorMsg = "增加数据失败。";
                }
                else
                {
                    flag = "true";
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            string msg = "<ServerInfo><flag>" + flag + "</flag><errormessage>" + errorMsg + "</errormessage></ServerInfo>";
            return msg;
        }
        public static DataTable CheckRepeatHistroyRecords(string fpdm, string fphm, string sessionid)
        {
            DataTable dt=null ;
            try
            {
                string sql1 = "SELECT id,fpdm,fphm,createtime FROM inv_gxrz where fpdm IN (" + fpdm + ") and fphm IN (" + fphm + ") and companyguid = '" + sessionid + "' GROUP BY fpdm,fphm";
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(sql1);
            }
            catch { }
            return dt;
        }
        public static string AddGXRZRecord(string id, string fplx, string fpdm, string fphm, string kprq, string je)
        {
            return "";
        }
        public static DataTable GetGXRZRecords(string guid, string sql, int page, int pagesize)
        {
            DataTable dt = null;

            string selectSql = "select t1.* FROM `inv_main` AS t1" +
                                           " JOIN (SELECT id FROM `inv_main`  where (" + sql + ") and companyguid='" + guid + "' ORDER BY id desc LIMIT " + ((page - 1) * pagesize).ToString() + "," + pagesize.ToString() + ") AS t2" +
                                           " WHERE t1.id = t2.id ORDER BY t1.id desc";
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;
        }
        /// <summary>
        /// 自动更新签收状态
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string UpdateSigninGXRZRecords(string guid)
        {
            string msg = "";
            string signinSql="update `inv_gxrz` as t1 LEFT JOIN  `inv_main` as t2 on  t1.companyguid = t2.`companyguid`  "+
                            "set t1.`Signin` =1,t1.`SigninByUser` = t2.`createuser` ,t1.`SigninTime` = t2.`createtime` "+
                            "where t1.`companyguid` = '"+guid+"' and t1.`fpdm`  = t2.`fpdm` and t1.`fphm` =t2.`fphm` and t1.`Signin` <>1";
            try
            {
                int n = inv_tax_DBConn.MySqlHelper.ExecuteNonQuery(signinSql);
                msg = n.ToString();
            }
            catch { }
            return msg;
        }
        public static int GetSearchGXRZRecordsCountBySql(string guid, string sqlvalue)
        {
            int count = 0;
            string selectSql = "";
            selectSql = "select count(id)  from `inv_gxrz` where companyguid = '"+guid+"' and ("+sqlvalue+")";
            try
            {
                object obj = inv_tax_DBConn.MySqlHelper.ExecuteScalar(selectSql);
                if (obj != null)
                    count = int.Parse(obj.ToString());
            }
            catch
            { }
            return count;
        }
        public static DataTable GetSearchGXRZRecords(string guid, string sql, int page, int pagesize)
        {
            DataTable dt = null;
            string selectSql = "select t1.* FROM `inv_gxrz` AS t1" +
                                " JOIN (SELECT id FROM `inv_gxrz`  where (" + sql + ")" +
                                " and companyguid='" + guid + "' ORDER BY id desc LIMIT " + ((page - 1) * pagesize).ToString() + "," + pagesize.ToString() + ") AS t2" +
                                " WHERE t1.id = t2.id ORDER BY t1.id DESC ";
            try
            {
                dt = inv_tax_DBConn.MySqlHelper.ExecuteDataTable(selectSql);
            }
            catch
            { }
            return dt;
        }
    }
}
