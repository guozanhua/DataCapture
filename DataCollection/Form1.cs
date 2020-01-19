using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace DataCollection
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.txtNumber.Text) || string.IsNullOrWhiteSpace(this.txtLink.Text))
                {
                    MessageBox.Show("请输入数据");
                    return;
                }

                //string url = "http://www.jianlaixiaoshuo.com/danhuangwudi/";
                //string url = "http://www.jianlaixiaoshuo.com/book/1.html";
                //string url = "http://www.quanshuwang.com/all/allvisit_0_0_0_0_1_0_{0}.html";
                string url = this.txtLink.Text;
                 
                string htmlstr = GetHtmlStr(url);
                 

                GrabData(htmlstr,"");


            }
            catch (WebException ex)
            {
                //连接失败

            }
        }

        int count = 1;
        private void GrabData(string contentstr)
        {
            var _name = GrabTitleData(contentstr);
            string content = NoHTML(GrabContentData(contentstr));
            string next = GrabNextData(contentstr);
            string dtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            int article_id = int.Parse(this.txtNumber.Text);

            string insertChapter = $"insert into Chapter(name,create_time,content,sort,click,article_id,page_id) values('{_name}','{dtime}','{content}',{count},0,{article_id},{count})";

            //Task.Run(() => {
            MySQLHelper.GetInstance().ExecuteNonQuery(insertChapter);
            //});

            this.textBox1.Text = this.textBox1.Text + "\r\n" + "写入" + _name + "," + dtime;
            this.textBox1.SelectionStart = this.textBox1.Text.Length;
            this.textBox1.ScrollToCaret();//滚动到最后一行
            Application.DoEvents();

            count++;
            //if (!string.IsNullOrEmpty(next) && (next.IndexOf("last") < 0))
            //{
            //    try
            //    {
            //        string mainUrl = this.txtLink.Text.Substring(0, this.txtLink.Text.LastIndexOf('/'));

            //        GrabData(GetHtmlStr(mainUrl + next));

            //    }
            //    catch (Exception ex)
            //    {
            //        this.textBox1.Text = this.textBox1.Text + "\r\n" + "错误：" + ex.Message;
            //        this.textBox1.Text = this.textBox1.Text + "\r\n" + "当前url：" + next;
            //        this.textBox1.SelectionStart = this.textBox1.Text.Length;
            //        this.textBox1.ScrollToCaret();//滚动到最后一行
            //        Application.DoEvents();
            //    }
            //}


        }

        private void GrabData(string htmlstr, string article = "剑来")
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//ol[@id='alllist']/li/a";
            //string xpathstring = "//div[@class='inner']/dl[@class='chapterlist']/dd/a";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            ////商家名称
            //var nameNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='tit']/a/h4");


            ////商家地址
            //var addNode = rootnode.SelectNodes("//div[@class='tag-addr']/span[@class='addr']");

            ////商家评分
            //var rankNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='comment']/span");

            ////分类
            //var typeNode = rootnode.SelectNodes("//div[@class='tag-addr']/a[@data-click-name='shop_tag_cate_click']/span");

            ////链接
            //var linkNode = rootnode.SelectNodes("//div[@class='txt']/div[@class='tit']/a[@data-click-name='shop_title_click']");

            int count = 1;

            foreach (var item in list)
            {
                var _name = item.InnerText;
                var _link = item.GetAttributeValue("href", "");


                string contentstr = GetHtmlStr("http://m.quanshuwang.com/" + _link);
                string content = NoHTML(GrabContentData(contentstr));
                string dtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string insertChapter = $"insert into Chapter(name,create_time,content,sort,click,article_id,page_id) values('{_name}','{dtime}','{content}',{count},0,3,{count})";

                //Task.Run(() => {
                MySQLHelper.GetInstance().ExecuteNonQuery(insertChapter);
                //});

                this.textBox1.Text = this.textBox1.Text + "\r\n" + "写入" + _name + "," + dtime;
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
                this.textBox1.ScrollToCaret();//滚动到最后一行
                Application.DoEvents();
                //for (int i = 0; i < nameNode.Count; i++)
                //{
                //    var _name = nameNode[i].InnerText;
                //    var _addr = addNode[i].InnerText;
                //    var _rank = rankNode[i].GetAttributeValue("title", "");
                //    var _type = typeNode[i].InnerText;

                //    //var _shopId = linkNode[i].GetAttributeValue("data-shopid", ""); ;
                //    //var _tel = GetPhone(_shopId);


                //}

                count++;
            }
        }


        private int GetMaxNumber()
        {
            string sql = "select max(id) from chapter";
            int number = MySQLHelper.GetInstance().ExecuteScalar(sql);
            return number++;
        }

        private string GrabContentData(string htmlstr)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@id='htmlContent']";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            return list[0].InnerHtml;
        }
        private string GrabTitleData(string htmlstr)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@id='htmlmain']/h2";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            return list[0].InnerHtml;
        }
        private string GrabNextData(string htmlstr)
        {
            try
            {

                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlstr);
                HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
                                                         //根据网页的内容设置XPath路径表达式
                string xpathstring = "//div[@id='upPage']/a";
                HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合
                if (list[list.Count - 1].InnerHtml == "下一张")
                {
                    return list[list.Count - 1].GetAttributeValue("href", "");
                }
                else
                {
                    return "";
                }

            }
            catch (Exception ex)
            {
                this.textBox1.Text = this.textBox1.Text + "\r\n" + ex.Message;
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
                this.textBox1.ScrollToCaret();//滚动到最后一行
                Application.DoEvents();
                return "";
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

                //                string heads = $@"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
                //Accept-Encoding: gzip, deflate
                //Accept-Language: zh-CN,zh;q=0.9
                //Cache-Control: max-age=0
                //Connection: keep-alive
                //Host: www.jianlaixiaoshuo.com
                //If-Modified-Since: Fri, 17 Jan 2020 12:39:42 GMT
                //If-None-Match: {0}
                //Upgrade-Insecure-Requests: 1
                //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";

                string aaa = "{\"sz\":\"16\",\"bg\":\"day-bg\",\"help\":\"1\"}";

                string heads = $@"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Connection: keep-alive
Cookie: fikker-5RMI-wKqx=uBJMat385O9HQj5f7IUjJTqREtWdMTSI; UM_distinctid=16fb878f95728d-0f3737fcac4a1e-6701b35-13c680-16fb878f95840a; CNZZDATA1262155963=2124321761-1579345171-null%7C1579345171; jieqiVisitId=article_articleviews%3D16860; iCast_Rotator_1_1=1579348132991; ReadSet={aaa}; jieqiHistoryBooks=%5B%7B%22articleid%22%3A%2216860%22%2C%22articlename%22%3A%22%u603B%u88C1%u7684%u65B0%u9C9C%u5C0F%u59BB%u5B50%22%2C%22chapterid%22%3A%226264619%22%2C%22chaptername%22%3A%22%u7B2C%u4E00%u5377%20%u7B2C1%u7AE0%20%u521D%u76F8%u9047%22%7D%5D; iCast_Rotator_1_2=1579348223684; CNZZDATA1273371515=339438370-1579342777-null%7C1579348221; iCast_Rotator_1_3=1579348231657
Host: m.quanshuwang.com
Referer: http://m.quanshuwang.com/list/16860_1.html
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

        /// 用正则表达式去掉Html中的script脚本和html标签
        /// </summary>
        /// <param name="Htmlstring"></param>
        /// <returns></returns>
        public static string NoHTML(string Htmlstring)
        {
            //删除脚本  
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            ////删除HTML  
            //Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

            //Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            //Htmlstring.Replace("<", "");
            //Htmlstring.Replace(">", "");
            //Htmlstring.Replace("\r\n", "");
            //Htmlstring = HttpUtility.HtmlDecode(Htmlstring).Replace("<br/>", "").Replace("<br>", "").Trim();

            return Htmlstring;
        }
    }
}
