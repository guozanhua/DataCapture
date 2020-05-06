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
using System.Windows.Forms;

namespace DataCollection
{
    public partial class Fengyue : Form
    {
        public Fengyue()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                string module = "校園師生";

                string url = $"http://www.h528.com/post/category/{module}";

                Search(url, module, 60);

                ////写入章节
                //string htmlstr = GetHtmlStr(url);
                //GrabData(htmlstr);

            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// 公共查询方法
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="url"></param>
        private void Search(string url, string module, int count = 50)
        {
            try
            {
                string htmlstr = "";

                for (int i = 1; i <= count; i++)
                { 
                    try
                    {
                        this.textBox1.Text = this.textBox1.Text + "\r\n" + $"查询第:[{i}] 页数据";
                        this.textBox1.SelectionStart = this.textBox1.Text.Length;
                        this.textBox1.ScrollToCaret();//滚动到最后一行
                        Application.DoEvents();


                        if (i == 1)
                        {
                             htmlstr = GetHtmlStr(url);
                            GrabData(htmlstr, module);
                        }
                        else
                        {
                             htmlstr = GetHtmlStr(url + "/page/" + i);
                            GrabData(htmlstr, module);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.textBox1.Text = this.textBox1.Text + "\r\n" + $"报错:[{ex.Message}]";
                        this.textBox1.SelectionStart = this.textBox1.Text.Length;
                        this.textBox1.ScrollToCaret();//滚动到最后一行
                        Application.DoEvents();
                    }

                    


                    //System.Threading.Thread.Sleep(1000);
                }

                MessageBox.Show(module + "操作完毕，" + count + "页数据");
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
                string aaa = "{%22id%22:121%2C%22name%22:%22lijun%22%2C%22mobile%22:%2218516063170%22%2C%22roleName%22:%22%E8%B6%85%E7%BA%A7%E7%AE%A1%E7%90%86%E5%91%98%22%2C%22permission%22:%221000%2C2000%2C3000%2C4000%2C5000%2C6000%2C7000%2C8000%2C4010%22%2C%22roleId%22:1}";

                string heads = $@"Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
Accept-Encoding: gzip, deflate
Accept-Language: zh-CN,zh;q=0.9
Cookie: sc_is_visitor_unique=rx2315234.1588770685.05827513433E4F5F5A2A1E1D48145FCA.4.3.3.3.3.3.2.2.2
Host: www.h528.com
Proxy-Connection: keep-alive
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.88 Safari/537.36";


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
        private void GrabData(string htmlstr, string module)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            //string xpathstring = "//div[@id='content']/div[@class='post']/a[@rel='bookmark']";
            string xpathstring = "//a[@rel='bookmark']";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            foreach (var item in list)
            {
                var link = item.GetAttributeValue("href", "");
                var title = item.InnerText;
                string contentStr = "";

                //判断是否存在
                string selSql = $"select count(*) from article where origin_url like '%{link}%'";

                int res = MySQLHelper.GetInstance().ExecuteScalar(selSql);
                if (res > 0)
                {
                    continue;
                }
                try
                {
                    string dtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string insertChapter = $"insert into article(origin_url,name,book_type,create_time,content) values('{link}','{title}','{module}','{dtime}','')";

                    //Task.Run(() => {
                    MySQLHelper.GetInstance().ExecuteNonQuery(insertChapter);
                    //});

                    this.textBox1.Text = this.textBox1.Text + "\r\n" + $"写入:[{module}] {title},链接地址:{link},{dtime}";
                    this.textBox1.SelectionStart = this.textBox1.Text.Length;
                    this.textBox1.ScrollToCaret();//滚动到最后一行
                    Application.DoEvents();

                    //contentStr = GetHtmlStr(link);

                    //if (contentStr != "")
                    //{

                    //    GrabData(contentStr, link, title, module);

                    //}
                }
                catch (Exception ex)
                {
                    this.textBox1.Text = this.textBox1.Text + "\r\n" + $"报错:[{ex.Message}] ,link:{link},title:{title},contentStr:{contentStr}";
                    this.textBox1.SelectionStart = this.textBox1.Text.Length;
                    this.textBox1.ScrollToCaret();//滚动到最后一行
                    Application.DoEvents();
                }
            }

        }
        private void GrabData(string contentstr, string link, string title, string module)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(contentstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@class='entry']";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            string content = NoHTML(list[0].InnerHtml);
            string dtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            string insertChapter = $"insert into article(origin_url,name,book_type,create_time,content) values('{link}','{title}','{module}','{dtime}','{content}')";

            //Task.Run(() => {
            MySQLHelper.GetInstance().ExecuteNonQuery(insertChapter);
            //});

            this.textBox1.Text = this.textBox1.Text + "\r\n" + $"写入:[{module}] {title},链接地址:{link},{dtime}";
            this.textBox1.SelectionStart = this.textBox1.Text.Length;
            this.textBox1.ScrollToCaret();//滚动到最后一行
            Application.DoEvents();

           // System.Threading.Thread.Sleep(1000);

        }
        public static string NoHTML(string Htmlstring)
        {
            //删除脚本  
            Htmlstring = Regex.Replace(Htmlstring, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            //删除HTML  
            //Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", "", RegexOptions.IgnoreCase);

            Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            //Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);

            Htmlstring = Regex.Replace(Htmlstring, @"'", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"’", "", RegexOptions.IgnoreCase);

            //Htmlstring.Replace("<", "");
            //Htmlstring.Replace(">", "");
            //Htmlstring.Replace("\r\n", "");
            //Htmlstring = HttpUtility.HtmlDecode(Htmlstring).Replace("<br/>", "").Replace("<br>", "").Trim();

            return Htmlstring;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string selSql = $"select id,origin_url from article where content=''";

            //Task.Run(() => {
            var dt= MySQLHelper.GetInstance().ExecuteDataTable(selSql);

            if (dt!=null && dt.Rows.Count>0)
            {
                this.textBox1.Text = this.textBox1.Text + "\r\n" + $"查询到需要更新的条目数:[{dt.Rows.Count}]";
                this.textBox1.SelectionStart = this.textBox1.Text.Length;
                this.textBox1.ScrollToCaret();//滚动到最后一行
                Application.DoEvents();

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    this.textBox1.Text = this.textBox1.Text + "\r\n" + $"处理第:[{(i+1)}]条";
                    this.textBox1.SelectionStart = this.textBox1.Text.Length;
                    this.textBox1.ScrollToCaret();//滚动到最后一行
                    Application.DoEvents();

                    var dtrow = dt.Rows[i];
                    string link = dtrow["origin_url"].ToString();
                    string id = dtrow["id"].ToString();

                    try
                    {
                        string contentStr = GetHtmlStr(link);

                        if (contentStr != "")
                        {
                            UpdateContent(contentStr, link, id);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.textBox1.Text = this.textBox1.Text + "\r\n" + $"错误:[{ex.Message}]条";
                        this.textBox1.SelectionStart = this.textBox1.Text.Length;
                        this.textBox1.ScrollToCaret();//滚动到最后一行
                        Application.DoEvents();
                    }

                    
                }
            }
        }
        private void UpdateContent(string contentstr, string link,string id)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(contentstr);
            HtmlNode rootnode = doc.DocumentNode;    //XPath路径表达式，这里表示选取所有span节点中的font最后一个子节点，其中span节点的class属性值为num
            //根据网页的内容设置XPath路径表达式
            string xpathstring = "//div[@class='entry']";
            HtmlNodeCollection list = rootnode.SelectNodes(xpathstring);    //所有找到的节点都是一个集合

            string content = NoHTML(list[0].InnerHtml);
            string dtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


            string insertChapter = $"update article set content = '{content}' where id={id}";

            //Task.Run(() => {
            MySQLHelper.GetInstance().ExecuteNonQuery(insertChapter);
            //});

            this.textBox1.Text = this.textBox1.Text + "\r\n" + $"更新:[{id}]";
            this.textBox1.SelectionStart = this.textBox1.Text.Length;
            this.textBox1.ScrollToCaret();//滚动到最后一行
            Application.DoEvents();

            // System.Threading.Thread.Sleep(1000);

        }
    }
}
