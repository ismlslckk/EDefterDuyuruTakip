using EDefterDuyuruTakip.Modeller;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
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
            List<Duyuru> sitedenOkunanTumDuyurular = new List<Duyuru>();
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
                    DateTime tarih = DateTime.Parse(tumDuyularinListesi[i].InnerText);
                    string icerik = tumDuyularinListesi[i + 1].InnerHtml; 
                    sitedenOkunanTumDuyurular.Add(new Duyuru() { Tarih = tarih, Icerik = icerik });
                }

                if(sitedenOkunanTumDuyurular.Count()==0)
                {
                    new MailInstance().Gonder("Hiç bir duyuru bulunamadı ");
                    Console.Read();
                }
                else
                {
                    sitedenOkunanTumDuyurular = sitedenOkunanTumDuyurular.OrderByDescending(x => x.Tarih).ToList();
                    DateTime kaydedilenSonDuyuruTarihi = DateTime.Parse(File.ReadAllText("sonDuyuruTarihi.txt"));
                    DateTime sitedenOkunanSonDuyuruTarihi = sitedenOkunanTumDuyurular.FirstOrDefault().Tarih;
                    if (kaydedilenSonDuyuruTarihi != sitedenOkunanSonDuyuruTarihi)
                    {
                        //yeni duyuru tarihini .txt dosyasına kaydediyorum. 
                        File.WriteAllText("sonDuyuruTarihi.txt", sitedenOkunanSonDuyuruTarihi.ToShortDateString());

                        //daha sonra yeni bir duyuru geldiği için mail atıyorum.
                        MailInstance mailInstance = new MailInstance();
                        mailInstance.Gonder(sitedenOkunanTumDuyurular.FirstOrDefault());
                        Console.Read();
                    }
                }
              

            } 
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                new MailInstance().Gonder("Sayfa bulunamadı.\r\n"+ex.Message+"\r\n"+ex.StackTrace);
                Console.Read();
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
            {
                new MailInstance().Gonder("Sunucuya Erişilemiyor.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                Console.Read();
            }
            catch (Exception ex)
            {
                new MailInstance().Gonder(ex.Message + "\r\n" + ex.StackTrace);
                Console.Read();
            }
             
        }
    }
}
