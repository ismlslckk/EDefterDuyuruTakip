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
            string[] parametreler = args[0].Split(',');
            
            string kimden = parametreler[0].Split(':')[1];
            string kime = parametreler[1].Split(':')[1];
            string kadi = parametreler[2].Split(':')[1];
            string sifre = parametreler[3].Split(':')[1];
            string host = parametreler[4].Split(':')[1];
            int port = Convert.ToInt32(parametreler[5].Split(':')[1]);
            bool ssl = Convert.ToBoolean(parametreler[6].Split(':')[1]); 
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
                    string tarih = tumDuyularinListesi[i].InnerText;
                    string icerik = tumDuyularinListesi[i + 1].InnerHtml; 
                    sitedenOkunanTumDuyurular.Add(new Duyuru() { Tarih = tarih, Icerik = icerik });
                }

                if(sitedenOkunanTumDuyurular.Count()==0)
                {
                    new MailInstance(kimden,kime,kadi,sifre,host,port,ssl).Gonder("Hiç bir duyuru bulunamadı ");
                    Console.Read();
                }
                else
                {
                    sitedenOkunanTumDuyurular = sitedenOkunanTumDuyurular.OrderByDescending(x => x.Tarih).ToList();
                    string kaydedilenSonDuyuruTarihi = File.ReadAllText("sonDuyuruTarihi.txt");
                    string sitedenOkunanSonDuyuruTarihi = sitedenOkunanTumDuyurular.FirstOrDefault().Tarih;
                    if (kaydedilenSonDuyuruTarihi != sitedenOkunanSonDuyuruTarihi)
                    {
                        //yeni duyuru tarihini .txt dosyasına kaydediyorum. 
                        File.WriteAllText("sonDuyuruTarihi.txt", sitedenOkunanSonDuyuruTarihi);

                        //daha sonra yeni bir duyuru geldiği için mail atıyorum.
                        MailInstance mailInstance = new MailInstance(kimden, kime, kadi, sifre, host, port, ssl);
                        mailInstance.Gonder(sitedenOkunanTumDuyurular.FirstOrDefault());
                        Console.Read();
                    }
                }
              

            } 
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                olusanHatalariKaydet(ex);
                new MailInstance(kimden, kime, kadi, sifre, host, port, ssl).Gonder("Sayfa bulunamadı.\r\n"+ex.Message+"\r\n"+ex.StackTrace);
                Console.Read();
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.InternalServerError)
            {
                olusanHatalariKaydet(ex);
                new MailInstance(kimden, kime, kadi, sifre, host, port, ssl).Gonder("Sunucuya Erişilemiyor.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
                Console.Read();
            }
            catch (Exception ex)
            {
                olusanHatalariKaydet(ex);
                new MailInstance(kimden, kime, kadi, sifre, host, port, ssl).Gonder(ex.Message + "\r\n" + ex.StackTrace);
                Console.Read();
            }
            void olusanHatalariKaydet(Exception ex)
            {
                string olusanTumHatalar = File.ReadAllText("olusanHatalar.txt");
                olusanTumHatalar += "\r\n #" + ex.Message + "\n" + ex.StackTrace;
                File.WriteAllText("olusanHatalar.txt", olusanTumHatalar+"\n");
            }
        }
      
    }
}
