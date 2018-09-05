using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DataCollection
{
    public partial class ZhaoBiao : Form
    {

        public class ZBModel
        {
            public string Title { get; set; }
            public string PublishDate { get; set; }
            public string Link { get; set; }
            public string LinkText { get; set; }
        }

        public List<ZBModel> ZbList = new List<ZBModel>();

        public ZhaoBiao()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 公共查询方法
        /// </summary>
        /// <param name="site"></param>
        /// <param name="url"></param>
        private void Search(string site, string url, int count = 50)
        {
            try
            {
                var _text = "开始查询第{0}页，总计{1}页";

                for (int i = 1; i <= count; i++)
                {
                    this.lblInfo.Text = string.Format(_text, i, count);
                    Application.DoEvents();

                    string htmlstr = GetHtmlStr(url + "&page=" + i);
                    GrabData(htmlstr, site);

                    //System.Threading.Thread.Sleep(1000);
                }

                // MessageBox.Show(site + "操作完毕，" + count + "页数据");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 获取请求返回的html字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetHtmlStr(string url)
        {
            try
            {
                string heads = @"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Cache-Control: max-age=0
Connection: keep-alive
Cookie: UM_distinctid=165a2838a2c1b-0a85a6219240a8-9393265-1fa400-165a2838a2d4; CNZZDATA1273963295=1563960200-1536024787-%7C1536044894
Host: www.rail-transit.com
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36";


                HttpRequestClient sc = new HttpRequestClient(true);
                //var response = sc.httpPost(url, heads, "", Encoding.UTF8);
                var response = sc.httpGet(url, heads, "");
                return response;

            }
            catch (WebException ex)
            {
                //连接失败
                return null;
            }
        }

        /// <summary>
        /// 查询节点数据，保存数据
        /// </summary>
        /// <param name="htmlstr"></param>
        /// <param name="zone"></param>
        /// <param name="module"></param>
        private void GrabData(string htmlstr, string site = "中国轨道交通网")
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@class='catlist']/ul/li";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            //foreach (var item in list)
            //{
            //日期 名称
            var dateNode = rootnode.SelectNodes("//div[@class='catlist']/ul/li/span");
            var titleNode = rootnode.SelectNodes("//div[@class='catlist']/ul/li/a");

            for (int i = 0; i < titleNode.Count; i++)
            {
                var _name = titleNode[i].InnerText;
                var _link = titleNode[i].GetAttributeValue("href", "");
                var _date = dateNode[i].InnerText;

                ZbList.Add(new ZBModel
                {
                    Link = _link,
                    PublishDate = _date,
                    Title = _name,
                    LinkText = "打开链接",
                });

                // string insertSql = string.Format(sql, _name, _rank, _addr, _type, DateTime.Now, zone);

                //SqlParameter[] param = new SqlParameter[] {
                //    new  SqlParameter("@BusinessName",_name),
                //    new  SqlParameter("@Rank",_rank),
                //    new  SqlParameter("@Address",_addr),
                //    new  SqlParameter("@Category",_type),
                //    new  SqlParameter("@CreateTime", DateTime.Now),
                //    new  SqlParameter("@Zone",zone),
                //    new  SqlParameter("@Module",module), 
                //};

                //SqlDbHelper.ExecuteNonQuery(insertSql, param);


            }


        }

        private void button1_Click(object sender, EventArgs e)
        {

            int count = 50;

            try
            {
                count = int.Parse(ConfigurationManager.AppSettings["count"]);
            }
            catch (Exception)
            { 
            }


            ZbList = new List<ZBModel>();
            this.Enabled = false;
            Application.DoEvents();
            var zone = this.button1.Text;
            string url = "http://www.rail-transit.com/zhaobiao/search.php?moduleid=29&catid=0&areaid=0&fromdate=&todate=&kw=";
            Search("中国轨道交通网", url, count);
            this.dataGridView1.DataSource = ZbList;

            this.Enabled = true;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                var url = dataGridView1.CurrentRow.Cells["Link"].Value.ToString();

                //MessageBox.Show(text1);

                //调用系统默认的浏览器 
                System.Diagnostics.Process.Start(url);
            }


        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
            e.Row.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter; 
        }
    }
}
