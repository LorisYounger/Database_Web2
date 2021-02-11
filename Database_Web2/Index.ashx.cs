using LinePutScript.SQLHelper;
using System;
using System.Configuration;
using System.Web;
using LinePutScript;

namespace Database_Web2
{
    /// <summary>
    /// 数据库连接接口,对应ver1的index.asp
    /// </summary>
    public class Index : IHttpHandler
    {
        HttpContext Context;

        /// <summary>
        /// 是否为新版本连接
        /// </summary>
        bool ver2 = false;
        private int getkey() => DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year;//秘钥,请自行更改

        public void ProcessRequest(HttpContext context)
        {
            Context = context;
            Context.Response.ContentType = "text/plain";

            int.TryParse(Context.Request.QueryString["Key"], out int key);

            if (key != getkey())
            {
                Context.Response.Write("Error KEY");
                Context.Response.End();
                return;
            }

            if (Context.Request.QueryString["Ver"] != null)
            {//新版本优化
                ver2 = true;
            }

            switch (context.Request.QueryString["Action"])
            {
                case "ReadAll":
                    ReadALL();
                    break;
                case "ReadAllbyBlock"://旧版本名称
                case "ReadBlock"://新版本名称
                    string block = CheckQuery("Block");
                    if (block != null)
                        ReadBlock(block);
                    break;
                case "Read":
                    block = CheckQuery("Block");
                    string nid = CheckQuery("Nid");
                    if (block != null && nid != null)
                        Read(block, nid);
                    break;
                case "Delete":
                case "DeleteID"://旧版本名称合并至delete
                    if (int.TryParse(Context.Request.QueryString["ID"], out int id))
                    {
                        Delete(id);
                        break;
                    }
                    block = CheckQuery("Block");
                    nid = CheckQuery("Nid");
                    if (block != null && nid != null)
                        Delete(block, nid);
                    break;
                case "Input"://旧版本名称
                case "Insert"://新版本名称
                    block = CheckQuery("Block");
                    nid = CheckQuery("Nid");
                    string info = CheckQuery("Info");
                    if (info == null)
                        info = "";
                    if (block != null && nid != null)
                        Insert(block, nid, info);
                    break;
                case "Modify"://旧版本名称
                case "Update"://新版本名称
                    block = CheckQuery("Block");
                    nid = CheckQuery("Nid");
                    info = CheckQuery("Info");
                    if (info == null)
                        info = "";
                    if (block != null && nid != null)
                        Update(block, nid, info);
                    break;
                case "Append":
                    block = CheckQuery("Block");
                    nid = CheckQuery("Nid");
                    info = CheckQuery("Info");
                    if (info == null)
                        info = "";
                    if (block != null && nid != null)
                        Append(block, nid, info);
                    break;
                case "MaxID":
                    MaxID();
                    break;
                default:
                    Context.Response.Write("No Action");
                    break;
            }
            Context.Response.End();
        }

        private string CheckQuery(string query)
        {
            string que = Context.Request.QueryString[query];
            if (que == null || que == "")
            {
                return null;
            }
            else
                return que;
        }



        /// <summary>
        /// 读取全部数据
        /// </summary>
        private void ReadALL()
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            LpsDocument lps = Mata.ExecuteQuery("SELECT * FROM `mata`");
            if (ver2)
            {
                Context.Response.Write(lps.ToString());
                return;
            }
            foreach (Line line in lps)
            {
                line.text = line.Last().info;
                line.Remove(line.Last());
                Context.Response.Write(line.ToString() + "\r\n");
            }
        }
        /// <summary>
        /// 读取某一个Block的全部数据
        /// </summary>
        private void ReadBlock(string block)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            LpsDocument lps = Mata.ExecuteQuery("SELECT * FROM `mata` WHERE `Block`=@block", new MySQLHelper.Parameter("block", block));
            if (ver2)
            {
                Context.Response.Write(lps.ToString());
                return;
            }
            foreach (Line line in lps)
            {
                line.text = line.Last().info;
                line.Remove(line.Last());
                Context.Response.Write(line.ToString() + "\r\n");
            }
        }

        /// <summary>
        /// 读取指定的信息
        /// </summary>
        /// 注:在Ver2中可以有多个相同的指定
        private void Read(string block, string nid)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            LpsDocument lps = Mata.ExecuteQuery("SELECT * FROM `mata` WHERE `Block`=@block AND `Nid`=@nid", new MySQLHelper.Parameter("block", block), new MySQLHelper.Parameter("nid", nid));
            if (ver2)
            {
                Context.Response.Write(lps.ToString());
                return;
            }
            Context.Response.Write(lps.Last().Last().Info);
        }
        /// <summary>
        /// 删除指定的信息
        /// </summary>
        private void Delete(string block, string nid)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Mata.ExecuteNonQuery("DELETE FROM `mata` WHERE `Block`=@block AND `Nid`=@nid", new MySQLHelper.Parameter("block", block), new MySQLHelper.Parameter("nid", nid));
            if (ver2)
                Context.Response.Write("TRUE");
            else
                Context.Response.Write("Delete Success");
        }
        /// <summary>
        /// 删除指定的信息
        /// </summary>
        private void Delete(int id)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Mata.ExecuteNonQuery("DELETE FROM `mata` WHERE `ID`=@id", new MySQLHelper.Parameter("id", id));
            if (ver2)
                Context.Response.Write("TRUE");
            else
                Context.Response.Write("Delete Success");
        }
        /// <summary>
        /// 更新数据库
        /// </summary>
        private void Update(string block, string nid, string info)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Mata.ExecuteNonQuery($"UPDATE `mata` SET `Info`=@info WHERE `Block`=@block AND `Nid`=@nid", new MySQLHelper.Parameter("info", info), new MySQLHelper.Parameter("block", block), new MySQLHelper.Parameter("nid", nid));
            if (ver2)
                Context.Response.Write("TRUE");
            else
                Context.Response.Write("Modify Success");

        }
        /// <summary>
        /// 在信息后添加内容
        /// </summary>
        private void Append(string block, string nid, string info)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Mata.ExecuteNonQuery($"UPDATE `mata` SET `Info`=CONCAT(`Info`,@info) WHERE `Block`=@block AND `Nid`=@nid", new MySQLHelper.Parameter("info", info), new MySQLHelper.Parameter("block", block), new MySQLHelper.Parameter("nid", nid));
            if (ver2)
                Context.Response.Write("TRUE");
            else
                Context.Response.Write("Append Success");
        }
        /// <summary>
        /// 添加新的数据
        /// </summary>
        private void Insert(string block, string nid, string info)
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Mata.ExecuteNonQuery($"INSERT INTO `mata` VALUES (NULL,@block,@nid,@info)", new MySQLHelper.Parameter("info", info), new MySQLHelper.Parameter("block", block), new MySQLHelper.Parameter("nid", nid));
            if (ver2)
                Context.Response.Write(Mata.ExecuteQuery("select LAST_INSERT_ID()").First().info);
            else
                Context.Response.Write("Input Success");
        }
        /// <summary>
        /// 获取当前最大ID
        /// </summary>
        private void MaxID()
        {
            MySQLHelper Mata = new MySQLHelper(ConfigurationManager.ConnectionStrings["connStr"].ConnectionString);
            Context.Response.Write(Mata.ExecuteQuery("SELECT MAX(ID) FROM `mata`").First().info);
        }
        public bool IsReusable => false;
    }
}