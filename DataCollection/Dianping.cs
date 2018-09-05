using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class Dianping : Form
    {
        public Dianping()
        {
            InitializeComponent();
        }

        #region Public Method

        public static string dzCookie = "";

        public static CookieContainer cc = new CookieContainer();//维持cookie或Session

        //string sql = "insert into BusniessInfo2(BusinessName,Rank,Address,Category,CreateTime,Zone,Module,Tel) values(@BusinessName,@Rank,@Address,@Category,@CreateTime,@Zone,@Module,@Tel)";
        string sql = "insert into BusniessInfo(BusinessName,Rank,Address,Category,CreateTime,Zone,Module) values(@BusinessName,@Rank,@Address,@Category,@CreateTime,@Zone,@Module)";

        /// <summary>
        /// 获取token
        /// </summary>
        public static bool GetToken(string url)
        {

            //formData用于保存提交的信息
            string formData = "";

            if (formData.Length > 0)
                formData = formData.Substring(0, formData.Length - 1); //去除最后一个 '&'

            //把提交的信息转码（post提交必须转码）
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] data = encoding.GetBytes(formData);

            //开始创建请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";    //提交方式：post
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; SV1; .NET CLR 2.0.1124)";
            request.AllowAutoRedirect = true;
            request.KeepAlive = true;

            Stream newStream = request.GetRequestStream();
            newStream.Write(data, 0, data.Length);//将请求的信息写入request
            newStream.Close();
            request.CookieContainer = cc;

            //向服务器发送请求
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //获得Cookie 保存到Appliction中
            string cookieHeader = request.CookieContainer.GetCookieHeader(new Uri("http://www.dianping.com/shenzhen/ch10/o3"));

            dzCookie = cookieHeader;


            return true;
        }


        /// <summary>
        /// 公共查询方法
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="url"></param>
        private void Search(string zone, string url, string module, int count = 50)
        {
            try
            {
                for (int i = 1; i <= count; i++)
                {
                    string htmlstr = GetHtmlStr(url + "p" + i);
                    GrabData(htmlstr, zone, module);

                    //System.Threading.Thread.Sleep(1000);
                }

                MessageBox.Show(zone + "操作完毕，" + count + "页数据");
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

                //GetToken("http://www.dianping.com");

                string heads = @"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Cache-Control: max-age=0
Cookie: showNav=#nav-tab|0|2; navCtgScroll=100; _lxsdk_cuid=162dcf84af9c8-034273fca8ff5d-b34356b-1fa400-162dcf84af9c8; _lxsdk=162dcf84af9c8-034273fca8ff5d-b34356b-1fa400-162dcf84af9c8; _hc.v=''8a8af4af-d518-45d4-bce6-e312f757ab5a.1524125682'';cy=7; cye=shenzhen; s_ViewType=10; aburl=1; _lx_utm=utm_source%3DBaidu%26utm_medium%3Dorganic; _lxsdk_s=165455e68c1-fd6-46c-d56%7C%7C1408
Host: www.dianping.com
Proxy-Connection: keep-alive
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";


                HttpRequestClient sc = new HttpRequestClient(true);
                var response = sc.httpPost(url, heads, "", Encoding.UTF8); 
                return response;

            }
            catch (WebException ex)
            {
                //连接失败
                return null;
            }
        }

        public static string GetDetailStr(string shopid)
        {
            try
            { 
                string url = string.Format("http://m.dianping.com/shop/{0}", shopid);

                string heads = @"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Cache-Control: max-age=0
Connection: keep-alive
Cookie: _lxsdk_cuid=162dcf84af9c8-034273fca8ff5d-b34356b-1fa400-162dcf84af9c8; _lxsdk=162dcf84af9c8-034273fca8ff5d-b34356b-1fa400-162dcf84af9c8; _hc.v=''8a8af4af-d518-45d4-bce6-e312f757ab5a.1524125682\""; cye=shenzhen; s_ViewType=10; aburl=1; _lx_utm=utm_source%3DBaidu%26utm_medium%3Dorganic; cy=7; cityid=7; msource=default; default_ab=shop%3AA%3A1; _lxsdk_s=165565c39fa-54b-b47-3c9%7C%7C73
Host: m.dianping.com
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1";


                HttpRequestClient sc = new HttpRequestClient(true);
                var response = sc.httpGet(url, heads);

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
        private void GrabData(string htmlstr, string zone = "福田区", string module = "食模块")
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@id='shop-all-list']/ul/li";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            //foreach (var item in list)
            //{
            //商家名称
            var nameNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='tit']/a/h4");


            //商家地址
            var addNode = rootnode.SelectNodes("//div[@class='tag-addr']/span[@class='addr']");

            //商家评分
            var rankNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='comment']/span");

            //分类
            var typeNode = rootnode.SelectNodes("//div[@class='tag-addr']/a[@data-click-name='shop_tag_cate_click']/span");

            //链接
            var linkNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='tit']/a[@data-click-name='shop_title_click']");



            for (int i = 0; i < nameNode.Count; i++)
            {
                var _name = nameNode[i].InnerText;
                var _addr = addNode[i].InnerText;
                var _rank = rankNode[i].GetAttributeValue("title", "");
                var _type = typeNode[i].InnerText;

                //var _shopId = linkNode[i].GetAttributeValue("data-shopid", ""); ;
                //var _tel = GetPhone(_shopId);

                string insertSql = string.Format(sql, _name, _rank, _addr, _type, DateTime.Now, zone);

                SqlParameter[] param = new SqlParameter[] {
                    new  SqlParameter("@BusinessName",_name),
                    new  SqlParameter("@Rank",_rank),
                    new  SqlParameter("@Address",_addr),
                    new  SqlParameter("@Category",_type),
                    new  SqlParameter("@CreateTime", DateTime.Now),
                    new  SqlParameter("@Zone",zone),
                    new  SqlParameter("@Module",module),
                    //new  SqlParameter("@Tel",_tel),
                };

                SqlDbHelper.ExecuteNonQuery(insertSql, param);
            }
        }

        /// <summary>
        /// 获取详情页电话号码
        /// </summary>
        /// <param name="linkDetail"></param>
        /// <returns></returns>
        private string GetPhone(string shopId)
        {
            List<string> list = new List<string>();

            string _html = GetDetailStr(shopId);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(_html);
            HtmlNode rootnode = doc.DocumentNode;

            try
            { 
                var _nodeList = rootnode.SelectNodes("//div[@class='J_phone']/div[@class='details-mode info-address']/div/div/a");
                if (_nodeList==null)
                {
                    _nodeList = rootnode.SelectNodes("//div[@class='aboutPhoneNum']/a");
                }

                //var _nodeList = rootnode.SelectNodes("//div[@class='aboutPhoneNum']/a[@class='item']");
                for (int i = 0; i < _nodeList.Count; i++)
                {
                    list.Add(_nodeList[i].InnerText.Trim());
                } 
            }
            catch (Exception ex)
            {
                 
            }
            return string.Join(",", list); 
        }

        #endregion

        #region 食模块

        /// <summary>
        /// 福田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            var zone = this.button1.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r29o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 南山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            var zone = this.button3.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r31o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 罗湖区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            var zone = this.button2.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r30o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 盐田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            var zone = this.button6.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r32o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 龙华区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            var zone = this.button7.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r12033o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 龙岗区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            var zone = this.button5.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r34o3";
            Search(zone, url, "食模块");
        }

        /// <summary>
        /// 宝安区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            var zone = this.button4.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r33o3";
            Search(zone, url, "食模块");
        }
        /// <summary>
        /// 坪山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            var zone = this.button8.Text;
            string url = "http://www.dianping.com/shenzhen/ch10/r12035o3";
            Search(zone, url, "食模块");
        }

        #endregion

        #region 乐模块

        /// <summary>
        ///  福田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button16_Click(object sender, EventArgs e)
        {
            var zone = this.button16.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r29o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 南山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            var zone = this.button14.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r31o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 罗湖区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_Click(object sender, EventArgs e)
        {
            var zone = this.button15.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r30o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 盐田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            var zone = this.button11.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r32o3";
            Search(zone, url, "乐模块", 1);
        }

        /// <summary>
        /// 龙华区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            var zone = this.button10.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r12033o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 龙岗区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            var zone = this.button12.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r34o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 宝安区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            var zone = this.button13.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r33o3";
            Search(zone, url, "乐模块", 30);
        }

        /// <summary>
        /// 坪山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            var zone = this.button9.Text;
            string url = "http://www.dianping.com/shenzhen/ch30/r12035o3";
            Search(zone, url, "乐模块", 1);
        }



        #endregion

        #region 丽人

        /// <summary>
        /// 福田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button24_Click(object sender, EventArgs e)
        {
            var zone = this.button24.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r29o3";
            Search(zone, url, "丽人", 50);
        }

        /// <summary>
        /// 南山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button22_Click(object sender, EventArgs e)
        {
            var zone = this.button22.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r31o3";
            Search(zone, url, "丽人", 50);
        }

        /// <summary>
        /// 罗湖区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button23_Click(object sender, EventArgs e)
        {
            var zone = this.button23.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r30o3";
            Search(zone, url, "丽人", 50);
        }

        /// <summary>
        /// 盐田区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button19_Click(object sender, EventArgs e)
        {
            var zone = this.button19.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r32o3";
            Search(zone, url, "丽人", 1);
        }

        /// <summary>
        /// 龙华区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button18_Click(object sender, EventArgs e)
        {
            var zone = this.button18.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r12033o3";
            Search(zone, url, "丽人", 20);
        }

        /// <summary>
        /// 龙岗区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button20_Click(object sender, EventArgs e)
        {
            var zone = this.button20.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r34o3";
            Search(zone, url, "丽人", 20);
        }

        /// <summary>
        /// 宝安区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button21_Click(object sender, EventArgs e)
        {
            var zone = this.button21.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r33o3";
            Search(zone, url, "丽人", 20);
        }

        /// <summary>
        /// 坪山区
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            var zone = this.button17.Text;
            string url = "http://www.dianping.com/shenzhen/ch50/r12035o3";
            Search(zone, url, "丽人", 1);
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            //GetDetailStr("97181823");
        }
    }
}
