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

namespace LumaprintsTask
{
    public class EtsyProduct
    {
        public string ProductName { get; set; }
        public string StoreName { get; set; }
        public string  Rating { get; set; }
        public string Reviews { get; set; }
        public string  Price { get; set; }
    }
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            gogo();
        }
        public async void gogo()
        {
            await ProcessIt();

        }
        public async Task ProcessIt()
        {
            #region
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            WebProxy proxy = new WebProxy(@"http://proxy1.smretailinc.com/SMRI.pac");

            ////(@);
            proxy.Credentials = CredentialCache.DefaultCredentials;

            WebClient client = new WebClient();
            client.Proxy = proxy;

            //var asd = client.DownloadString(@"https://www.etsy.com/search?q=arts%20and%20prints");
            //HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            //doc.LoadHtml(asd);
            #endregion

            HtmlWeb web = new HtmlWeb();

            List<EtsyProduct> productList = new List<EtsyProduct>();

            var doc = await Task.Factory.StartNew(() => web.Load(@"https://www.etsy.com/search?q=arts%20and%20prints"));
            //var productName = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/a/div[2]/div/h3/text()");
            //var products = productName.Select(node => node.InnerText).ToList();
            //products = StringCleaner(products);

            //var storeName = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/a/div//div/div/p[1]/text()");
            //var stores = storeName.Select(node => node.InnerText).ToList();

            //var ratingNodes = doc.DocumentNode.SelectNodes("//*[@id=\"content\"]/div/div//div/div/div//div//div//div/div/ul/li//div/div/a/div//div/div/span/span//input[2]");
            //var ratings = ratingNodes.Select(node => node.InnerText).ToList();

            //var asd = doc.DocumentNode.Descendants("ul").ToList();
            //var testing = doc.DocumentNode.SelectNodes("//*[@class=\"wt-list-unstyled wt-grid__item-xs-6 wt-grid__item-md-4 wt-grid__item-xl-3 wt-order-xs-0 wt-order-sm-0 wt-order-md-0 wt-order-lg-0 wt-order-xl-0 wt-order-tv-0\"]");

            var asd = doc.DocumentNode.Descendants("ul")
                .Where(node => node.GetAttributeValue("class", "")
                .Equals("responsive-listing-grid wt-grid wt-grid--block justify-content-flex-start pl-xs-0")).ToList();


            var products = asd[0].Descendants("li").ToList();

            foreach(var item in products)
            {
                EtsyProduct cursorProduct = new EtsyProduct();
                
                var products1 = products[0].Descendants("h3")
                .Where(node => node.GetAttributeValue("class", "")
            .Equals("text-gray text-truncate mb-xs-0 text-body ")).ToList();

                var details = item.Descendants("div").
                    Where(node => node.GetAttributeValue("class", "").Equals("v2-listing-card__shop")).ToList();

                var ShopName = item.Descendants("h3")
                .Where(node => node.GetAttributeValue("class", "")
            .Equals("text-gray text-truncate mb-xs-0 text-body ")).ToList();

                var currency = details.Select(x => x.InnerText)
               .ToList();


                var rating = item.Descendants("input")
                .Where(x => x["input"].v == "rating").ToList();

                //var currency = details.Select(x => x.Descendants("p").
                //.Where(x => x["class"].Value == "text-gray-lighter text-body-smaller display-inline-block")).ToList();



                productList.Add(cursorProduct);
            }
            

            //List<EtsyProduct> listProducts = new List<EtsyProduct>();
            //var asd = products.Zip(stores, (_productName, _storeName) =>
            //    new EtsyProduct()
            //    {
            //        ProductName = _productName,
            //        StoreName = _storeName
            //    });
        }
        private List<string> StringCleaner(List<string> obj)
        {
            var asd = obj.Select(x => HttpUtility.HtmlDecode(x)).ToList();
            asd = asd.Select(x => x.Replace("\n","")).ToList();
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

    }
}
