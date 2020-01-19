using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
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

        public class Article
        {
            public int fromid { get; set; }
            public string origin_url { get; set; }
            public string name { get; set; }
            public string book_type { get; set; }
            public string desc { get; set; }
            public string author { get; set; }
        }

        List<Article> articles = new List<Article>();
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

                //写入章节
                string htmlstr = GetHtmlStr(url);
                GrabData(htmlstr);


                ///写入书名表
                //for (int j = 1; j <= 200; j++)
                //{

                //    string htmlstr = PostHtmlStr(url, j);

                //    if (!string.IsNullOrWhiteSpace(htmlstr))
                //    {


                //        JObject jsonObj = JObject.Parse(htmlstr);

                //        var arraylist = (JArray)(((JObject)jsonObj["data"])["book"]);
                //        for (int i = 0; i < arraylist.Count; i++)
                //        {
                //            Article article = new Article();
                //            article.fromid = int.Parse(arraylist[i]["id"].ToString());
                //            article.origin_url = arraylist[i]["OriginUrl"].ToString();
                //            article.name = arraylist[i]["name"].ToString();
                //            article.book_type = arraylist[i]["bookType"].ToString();
                //            article.desc = arraylist[i]["description"].ToString(); 
                //            article.author = arraylist[i]["author"].ToString();

                //            articles.Add(article);
                //        }

                //    }
                //}
                //int countid = 4453;

                //foreach (var item in articles)
                //{
                //    if (item.fromid > 3594)
                //    {
                //        item.desc = item.desc.Replace("\\", "");

                //        string insertSql = $@"insert into article values({countid++},{item.fromid},'{item.origin_url}','{item.name}','{item.book_type}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',0,'{item.desc}','{item.author}')";

                //        MySQLHelper.GetInstance().ExecuteNonQuery(insertSql); 
                //    }
                //}


            }
            catch (WebException ex)
            {
                //连接失败
                this.textBox1.Text = this.textBox1.Text + "\r\n" + "错误：" + ex.Message;
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
                this.textBox1.ScrollToCaret();//滚动到最后一行
                Application.DoEvents();
            }
        }

        int count = 1;
        private void GrabData(string contentstr)
        {
            var _name = GrabTitleData(contentstr);
            string content = NoHTML(GrabContentData(contentstr));
            string next = GrabNextData(contentstr).Replace("&amp;", "&");
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
            if (!string.IsNullOrEmpty(next) && (next.IndexOf("last") < 0))
            {
                try
                {
                    string mainUrl = this.txtLink.Text.Substring(0, this.txtLink.Text.LastIndexOf('/'));

                    GrabData(GetHtmlStr("http://175.24.134.140:1111" + next));

                }
                catch (Exception ex)
                {
                    this.textBox1.Text = this.textBox1.Text + "\r\n" + "错误：" + ex.Message;
                    this.textBox1.Text = this.textBox1.Text + "\r\n" + "当前url：" + next;
                    this.textBox1.SelectionStart = this.textBox1.Text.Length;
                    this.textBox1.ScrollToCaret();//滚动到最后一行
                    Application.DoEvents();
                }
            }


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
            string xpathstring = "//div[@class='catalog-container']";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            return list[0].InnerHtml;
        }
        private string GrabTitleData(string htmlstr)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@class='catalog-name']";
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
                string xpathstring = "//header/a[@class='por']";
                HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合
                if (list != null)
                {
                    return list[0].GetAttributeValue("href", "");
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
        public static string PostHtmlStr(string url, int pageIndex = 1)
        {
            try
            {

                //GetToken("http://www.dianping.com");

                string heads = $@"Accept: application/json, text/plain, */*
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Connection: keep-alive
Content-Length: 62
Content-Type: application/x-www-form-urlencoded
Host: 175.24.134.140:3000
Origin: http://175.24.134.140:9092
Referer: http://175.24.134.140:9092/home?page=1&limit=200
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";

                //                string heads = $@"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
                //Accept-Encoding: gzip, deflate
                //Accept-Language: zh-CN,zh;q=0.9
                //Cache-Control: max-age=0
                //Connection: keep-alive
                //Cookie: token=686423fd8acc14fecc03466ffc94fb2c06e9236e; user={aaa}; access=1000%2C2000%2C3000%2C4000%2C5000%2C6000%2C7000%2C8000%2C4010; shrink=; bookIds=314; huyan=true; light=true; fontSize=small
                //Host: 175.24.134.140:1111
                //Upgrade-Insecure-Requests: 1
                //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";


                HttpRequestClient sc = new HttpRequestClient(true);

                string content = $"page={pageIndex}&limit=200&token=686423fd8acc14fecc03466ffc94fb2c06e9236e";

                var response = sc.httpPost(url, heads, content, Encoding.UTF8);
                return response;

            }
            catch (WebException ex)
            {
                //连接失败
                return null;
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
                string aaa = "{%22id%22:121%2C%22name%22:%22lijun%22%2C%22mobile%22:%2218516063170%22%2C%22roleName%22:%22%E8%B6%85%E7%BA%A7%E7%AE%A1%E7%90%86%E5%91%98%22%2C%22permission%22:%221000%2C2000%2C3000%2C4000%2C5000%2C6000%2C7000%2C8000%2C4010%22%2C%22roleId%22:1}";

                string heads = $@"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
                Accept-Encoding: gzip, deflate
                Accept-Language: zh-CN,zh;q=0.9
                Cache-Control: max-age=0
                Connection: keep-alive
                Cookie: token=686423fd8acc14fecc03466ffc94fb2c06e9236e; user={aaa}; access=1000%2C2000%2C3000%2C4000%2C5000%2C6000%2C7000%2C8000%2C4010; shrink=; bookIds=314; huyan=true; light=true; fontSize=small
                Host: 175.24.134.140:1111
                Upgrade-Insecure-Requests: 1
                User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.117 Safari/537.36";


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
