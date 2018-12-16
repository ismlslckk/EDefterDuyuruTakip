using EDefterDuyuruTakip.Modeller;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EDefterDuyuruTakip
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Duyuru> dosyayaYazdirilacakDuyurular = new List<Duyuru>();
            try
            {
                Uri url = new Uri("http://www.edefter.gov.tr/duyurular.html");
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string html = client.DownloadString(url);


                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(html);

                HtmlNodeCollection rtMainBody = document.DocumentNode.SelectNodes("//*[@id=\"rt-mainbody\"]");

                HtmlNode rtMainbodyDivininIcindekiDiv = rtMainBody.FirstOrDefault();
                HtmlNodeCollection rtMainbodyDivininIcindekiDivinIcindekiHtmlNodelari = rtMainbodyDivininIcindekiDiv.ChildNodes;
                HtmlNodeCollection tumDuyurular = rtMainbodyDivininIcindekiDivinIcindekiHtmlNodelari.FirstOrDefault(x => x.Name == "div").ChildNodes;
                List<HtmlNode> tumDuyularinListesi = tumDuyurular.Where(x => x.Name == "a" || x.Name == "p").ToList();
                for (int i = 0; i < tumDuyularinListesi.Count(); i=i+2)
                {
                    string tarih = tumDuyularinListesi[i].InnerText;
                    string icerik = tumDuyularinListesi[i + 1].InnerHtml;
                    dosyayaYazdirilacakDuyurular.Add(new Duyuru() { Tarih = tarih, Icerik = icerik });
                }
                 
            } 
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            { 
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
            { 
            }
            finally
            { 
            }

        }
    }
}
