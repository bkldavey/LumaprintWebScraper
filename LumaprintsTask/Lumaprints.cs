using HtmlAgilityPack;
using LumaprintsTask.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using LumaprintsTask.Helper;
using System.Diagnostics;

namespace LumaprintsTask
{

    public partial class Lumaprints : Form
    {
        Stopwatch watch = new Stopwatch();
        Timer watchTotal = new Timer();
        int second = 0;
        public Lumaprints()
        {
            InitializeComponent();
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            watchTotal.Tick += WatchTotal_Tick;
            watchTotal.Interval = 1000;
            watchTotal.Start();
            await ProcessIt(20);
            watch.Stop();
        }

        private void WatchTotal_Tick(object sender, EventArgs e)
        {
            second++;
            lblTimeElapsed.Text = $"Time Elapsed: {second}";
        }

        public async Task ProcessIt(int pages)
        {
            #region
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            ////WebProxy proxy = new WebProxy(@"http://proxy1.smretailinc.com/SMRI.pac");

            ////////(@);
            ////proxy.Credentials = CredentialCache.DefaultCredentials;

            //WebClient client = new WebClient();
            ////client.Proxy = proxy;

            //var asd = client.DownloadString(@"https://www.etsy.com/search?q=arts%20and%20prints");
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml(asd);
            #endregion
            watch.Start();

            List<EtsyProduct> productList = new List<EtsyProduct>();

            //WebView wv = new WebView();
            //wv.NavigationCompleted += Wv_NavigationCompleted;
            //wv.Navigate(new Uri(url));

            //private async void Wv_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
            //{
            //    string wvresult = await sender.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" });
            //}

            //var doc = await Task.Factory.StartNew(() => web.Load(url));

            try
            {
                for (int x = 1; x <= pages; x++)
                {
                    string url = $"https://www.etsy.com/search?q=arts+and+crafst&_qc=arts+and+crafts&ref=pagination&page={x}";

                    string html;
                    using (var client = new HttpClient())
                    {
                        html = await client.GetStringAsync(url);
                    }

                    var htmlDocument = new HtmlAgilityPack.HtmlDocument();
                    htmlDocument.LoadHtml(html.ToString());
                    htmlDocument.OptionEmptyCollection = true;

                    #region ==== XPath ====
                    //var productName = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/a/div[2]/div/h3/text()");
                    //var products = productName.Select(node => node.InnerText).ToList();
                    //products = StringCleaner(products);

                    HtmlWeb web = new HtmlWeb();
                    var testNodes = htmlDocument.DocumentNode.SelectNodes("//*[@class=\"responsive-listing-grid wt-grid wt-grid--block justify-content-flex-start pl-xs-0\"]")
                  .Descendants("a").ToList();

                    foreach (var item in testNodes)
                    {

                        EtsyProduct cursorProduct = new EtsyProduct();
                        //    var productname = item.Descendants("h3")
                        //        .Where(node => node.GetAttributeValue("class", "")
                        //.Equals("text-gray text-truncate mb-xs-0 text-body ")).ToList();

                        //Get all the stores in nodes
                        //var shopname = item.SelectNodes("//*[@class=\"text-gray-lighter text-body-smaller display-inline-block\"]/text()");

                        cursorProduct.StoreName = HttpUtility.HtmlDecode(item.SelectSingleNode(".//p[@class=\"text-gray-lighter text-body-smaller display-inline-block\"]/text()").InnerText);


                        cursorProduct.ProductName = item.SelectSingleNode(".//h3").InnerText.Trim();

                        var ratingNode = item.SelectNodes(".//span[@class=\"v2-listing-card__rating icon-t-2 display-block\"]").ToListOrEmpty();
                        cursorProduct.Rating = ratingNode.FirstOrDefault().InnerHtml.Replace("\n", "").Trim();
                        if (cursorProduct.Rating != string.Empty)
                        {
                            cursorProduct.Rating = item.SelectSingleNode(".//input[@name=\"rating\"]").GetAttributeValue("value", "");
                            cursorProduct.Reviews = item.SelectSingleNode(".//div[2]/div/div/span/span[3]/text()").InnerText.Replace(" reviews", "");
                        }
                        else
                        {
                            cursorProduct.Rating = "No Rating";
                            cursorProduct.Reviews = "No Reviews";
                        }

                        cursorProduct.Price = item.SelectSingleNode(".//span[@class=\"currency-value\"]/text()").InnerText;

                        productList.Add(cursorProduct);
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = productList;

                        lblProductStatus.Text = $"Product Count: {productList.Count()}";

                        
                    }
                }

                lblProducts.Text = $"Number of Products: {productList.Count()}";

                var uniqueShopList = productList.Select(x => x.StoreName).Distinct().ToList();

                int successCounter = 0;
                lblStatus.Text = $"{successCounter}/{uniqueShopList.Count}";
                lblStores.Text = $"Number of Stores: {uniqueShopList.Count()}";

                List<EtsyStoreInfo> storeInfoList = new List<EtsyStoreInfo>();
                foreach (var store in uniqueShopList)
                {
                    var storeInfo = await ProcessStore(store);
                    storeInfoList.Add(storeInfo);
                    successCounter++;
                    lblStatus.Text = $"{successCounter}/{uniqueShopList.Count}";
                    UpdateDataGridView(storeInfoList);
                  
                }
                DataTable dt = new DataTableHelper().ToDataTable(storeInfoList);
                CSVHelper.CreateCSVFile(dt, @"C:\\Users\\User\\Documents\\Lumaprints\\" + "Store.csv");

                DataTable dt1 = new DataTableHelper().ToDataTable(productList);
                CSVHelper.CreateCSVFile(dt1, @"C:\\Users\\User\\Documents\\Lumaprints\\" + "Products.csv");
            }
            catch (NullReferenceException aasd)
            {

            }
            catch (Exception ex)
            {

            }

            //.Where(node => node.GetAttributeValue("class", "").Equals("c617a42c11375c7d display-inline-block listing-link  logged")).ToList();



            //var storeName = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/a/div//div/div/p[1]/text()");
            //var stores = storeName.Select(node => node.InnerText).ToList();

            //var ratingNodes = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/div/a/div//div/div/span/span//input[2]");
            //var ratings = ratingNodes.Select(node => node.InnerText).ToList();
            //var asd = doc.DocumentNode.Descendants("ul").ToList();
            //var testing = doc.DocumentNode.SelectNodes("//*[@class=\"wt-list-unstyled wt-grid__item-xs-6 wt-grid__item-md-4 wt-grid__item-xl-3 wt-order-xs-0 wt-order-sm-0 wt-order-md-0 wt-order-lg-0 wt-order-xl-0 wt-order-tv-0\"]");

            #endregion

            #region ==== LINQ ====

            //var asd = doc.DocumentNode.Descendants("ul")
            //   .Where(node => node.GetAttributeValue("class", "")
            //   .Equals("responsive-listing-grid wt-grid wt-grid--block justify-content-flex-start pl-xs-0")).ToList();


            //var products = asd[0].Descendants("li").ToList();

            //foreach (var item in products)
            //{
            //    EtsyProduct cursorProduct = new EtsyProduct();

            //    var products1 = products[0].Descendants("h3")
            //    .Where(node => node.GetAttributeValue("class", "")
            //.Equals("text-gray text-truncate mb-xs-0 text-body ")).ToList();

            //    var details = item.Descendants("div").
            //        Where(node => node.GetAttributeValue("cla" +
            //        "ss", "").Equals("v2-listing-card__shop")).Select(x => x.InnerHtml).ToList();

            //    var ShopName = item.Descendants("h3")
            //    .Where(node => node.GetAttributeValue("class", "")
            //.Equals("text-gray text-truncate mb-xs-0 text-body ")).ToList();

            //    cursorProduct.StoreName = item.Descendants("p").
            //        Where(node => node.GetAttributeValue("class", "").Equals("text-gray-lighter text-body-smaller display-inline-block")).FirstOrDefault().OuterHtml;


            //    //var czc = getShopName[0].InnerHtml.ToList();

            //    //var rating = item.Descendants("input").Select(x=> x["input"].V)
            //    //.Where(x => x["input"].v == "rating").ToList();

            //    //var ratings = item.Selec

            //    //var currency = details.Select(x => x.Descendants("p").
            //    //.Where(x => x["class"].Value == "text-gray-lighter text-body-smaller display-inline-block")).ToList();



            //    productList.Add(cursorProduct);
            //}
            #endregion

            //List<EtsyProduct> listProducts = new List<EtsyProduct>();
            //var asd = products.Zip(stores, (_productName, _storeName) =>
            //    new EtsyProduct()
            //    {
            //        ProductName = _productName,
            //        StoreName = _storeName
            //    });
            Console.WriteLine(watch.Elapsed);
            watch.Stop();
            watch.Reset();
        }
        public async Task<EtsyStoreInfo> ProcessStore(string storeName)
        {
            watch.Start();
            EtsyStoreInfo newStoreInfo = new EtsyStoreInfo();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            string url = $"https://www.etsy.com/shop/{storeName}";

            string html;
            using (var client = new HttpClient())
            {
                html = await client.GetStringAsync(url);
            }

            var htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(html.ToString());
            htmlDocument.OptionEmptyCollection = true;

            try
            {


                var testNodes = htmlDocument.DocumentNode.SelectNodes("//*[@class=\"mb-xs-5 mt-xs-3 mt-lg-4 mb-lg-4 width-full clearfix\"]").ToList();

                //newStoreInfo.Headline = testNodes[0].SelectSingleNode(".//span[@data-key=\"headline\"]").InnerText;
                //newStoreInfo.DateStarted = testNodes[0].SelectSingleNode(".//span[@class=\"etsy-since no-wrap\"]/text()").InnerText.Replace("On Etsy since ", "");
                //newStoreInfo.StoreRating = testNodes[0].SelectSingleNode(".//input[@name=\"rating\"]").GetAttributeValue("value", "");

                newStoreInfo.StoreName = storeName;
                newStoreInfo.Headline = HttpUtility.HtmlDecode(testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/div[1]/span").FirstOrDefault().InnerText);

                var storeRating = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/div[2]/a[1]/span/span/input[2]").ToListOrEmpty();
                if (storeRating.Any())
                {
                    newStoreInfo.StoreRating = storeRating.FirstOrDefault().GetAttributeValue("value","");
                }
                else
                {
                    newStoreInfo.StoreRating = string.Empty;
                }

                var dateStarted = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/p/span").FirstOrDefault().InnerText;
                if (dateStarted != string.Empty)
                {
                    newStoreInfo.DateStarted = dateStarted.Replace("On Etsy since ", "");
                }
                else
                    newStoreInfo.DateStarted = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/p/span[3]").FirstOrDefault().InnerText.Replace("On Etsy since ", "");


                var storeUpdate = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[2]/div[1]/div/div/div[2]/div/span/text()").ToListOrEmpty();
                if (storeUpdate.Any())
                    newStoreInfo.LastUpdate = storeUpdate.FirstOrDefault().InnerText;
                else
                    newStoreInfo.LastUpdate = string.Empty;

                var storeLocation = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/p/span[2]").ToListOrEmpty();
                if (storeLocation.Any())
                    newStoreInfo.StoreLocation = storeLocation.FirstOrDefault().InnerText;
                else
                    newStoreInfo.StoreLocation = string.Empty;

                var storeSales = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/div[2]/span[1]/text()").ToListOrEmpty();
                if (storeSales.Any())
                {
                    newStoreInfo.Sales = storeSales.FirstOrDefault().InnerText.Replace(" Sales", "");
                }
                else
                {
                    var storeSales2 = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[1]/div[2]/div[2]/span/a/text()").ToListOrEmpty();
                    if (storeSales2.Any())
                        newStoreInfo.Sales = storeSales2.FirstOrDefault().InnerText.Replace(" Sales", "");
                    else
                        newStoreInfo.Sales = string.Empty;
                }


                var storeOwner = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[3]/div/div/a/p/text()").ToListOrEmpty();
                if (storeOwner.Any())
                {
                    newStoreInfo.ShopOwner = storeOwner.FirstOrDefault().InnerText;
                }
                else
                {
                    newStoreInfo.ShopOwner = testNodes[0].SelectNodes("//*[@id=\"content\"]/div[1]/div[1]/div[2]/div/div/div[4]/div/div/a/p").FirstOrDefault().InnerText;
                }

            }
            catch (NullReferenceException aasd)
            {
            }
            catch (Exception ex)
            {

            }
            Console.WriteLine(watch.Elapsed);
            watch.Stop();
            watch.Reset();
            return newStoreInfo;
        }
        public void UpdateDataGridView(List<EtsyStoreInfo> obj)
        {
            dataGridView2.DataSource = null;
            dataGridView2.DataSource = obj;
            dataGridView2.Update();
            dataGridView2.Refresh();
        }
        private List<string> StringCleaner(List<string> obj)
        {
            var asd = obj.Select(x => HttpUtility.HtmlDecode(x)).ToList();
            asd = asd.Select(x => x.Replace("\n", "")).ToList();
            //asd = asd.Select(x => x.Replace("&quot;", "\"")).ToList();
            asd = asd.Select(x => Regex.Replace(x, @"\s{2,}", "")).ToList();

            //foreach (var item in obj)
            //{

            //}
            ////replaces
            //var result = obj.Select(x =>  x.Replace()
            //    x.Replace("\n", "");
            //    x.Replace("&quot;", "\"");
            //    ;
            //});
            return asd;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                var asd = await ProcessStore(textBox1.Text);
                var list = new List<EtsyStoreInfo>();
                list.Add(asd);
                dataGridView3.DataSource = list;
            }
        }
    }
}