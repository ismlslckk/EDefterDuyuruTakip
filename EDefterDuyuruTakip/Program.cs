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
            string kimden, kime, kadi, sifre, host = "";
            int port=0;
            bool ssl = false;

            if (args.Length > 0)
            {
                string[] parametreler = args[0].Split(',');

                kimden = parametreler[0].Split(':')[1];
                kime = parametreler[1].Split(':')[1];
                kadi = parametreler[2].Split(':')[1];
                sifre = parametreler[3].Split(':')[1];
                host = parametreler[4].Split(':')[1];
                port = Convert.ToInt32(parametreler[5].Split(':')[1]);
                ssl = Convert.ToBoolean(parametreler[6].Split(':')[1]);
            }
            else
            {
                kimden = "edefterduyurutakip@gmail.com";
                kime = "sirketdestek@adayazilim.com";
                kadi = "edefterduyurutakip@gmail.com";
                sifre = "edefterduyurutakip.2018";
                host = "smtp.gmail.com";
                port = 587;
                ssl = true;
            }
            
            List<Duyuru> sitedenOkunanTumDuyurular = new List<Duyuru>();
            try
            {
                Uri url = new Uri("http://www.edefter.gov.tr/duyurular.html");
                logYaz("satir 30\n");
                WebClient client = new WebClient();
                client.Encoding = System.Text.Encoding.UTF8;
                string html = client.DownloadString(url);
                logYaz("satir 34\n");

                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                logYaz("satir 36\n");
                document.LoadHtml(html);
                logYaz("satir 39\n");
                HtmlNodeCollection rtMainBody = document.DocumentNode.SelectNodes("//*[@id=\"rt-mainbody\"]");
                logYaz("satir 41\n");
                HtmlNode rtMainbodyDivininIcindekiDiv = rtMainBody.FirstOrDefault();
                logYaz("satir 42\n");
                HtmlNodeCollection rtMainbodyDivininIcindekiDivinIcindekiHtmlNodelari = rtMainbodyDivininIcindekiDiv.ChildNodes;
                logYaz("satir 45\n");
                HtmlNodeCollection tumDuyurular = rtMainbodyDivininIcindekiDivinIcindekiHtmlNodelari.FirstOrDefault(x => x.Name == "div").ChildNodes;
                logYaz("satir 47\n");
                List<HtmlNode> tumDuyularinListesi = tumDuyurular.Where(x => x.Name == "a" || x.Name == "p").ToList();
                logYaz("duyuru adet : " + tumDuyularinListesi.Count + "\n");
                for (int i = 0; i < tumDuyularinListesi.Count(); i = i + 2)
                {

                    DateTime tarih = DateTime.Parse(tumDuyularinListesi[i].InnerText.Replace(".", "/"));
                    string icerik = tumDuyularinListesi[i + 1].InnerHtml;
                    sitedenOkunanTumDuyurular.Add(new Duyuru() { Tarih = tarih, Icerik = icerik });
                }

                logYaz("duyurular okundu\n");
                if (sitedenOkunanTumDuyurular.Count() == 0)
                {
                    new MailInstance(kimden, kime, kadi, sifre, host, port, ssl).Gonder("Hiç bir duyuru bulunamadı ");
                    Console.Read();
                }
                else
                {
                    sitedenOkunanTumDuyurular = sitedenOkunanTumDuyurular.OrderByDescending(x => x.Tarih).ToList();
                    DateTime kaydedilenSonDuyuruTarihi = DateTime.Parse(File.ReadAllText(@"C:\AdaYazilim\ScheduledTask\sonDuyuruTarihi.txt").Replace(".", "/"));
                    DateTime sitedenOkunanSonDuyuruTarihi = sitedenOkunanTumDuyurular.FirstOrDefault().Tarih;
                    if (kaydedilenSonDuyuruTarihi != sitedenOkunanSonDuyuruTarihi)
                    {
                        //yeni duyuru tarihini .txt dosyasına kaydediyorum. 
                        File.WriteAllText(@"C:\AdaYazilim\ScheduledTask\sonDuyuruTarihi.txt", sitedenOkunanSonDuyuruTarihi.ToShortDateString());

                        //daha sonra yeni bir duyuru geldiği için mail atıyorum.
                        MailInstance mailInstance = new MailInstance(kimden, kime, kadi, sifre, host, port, ssl);
                        sitedenOkunanTumDuyurular = sitedenOkunanTumDuyurular.Where(x => x.Tarih > kaydedilenSonDuyuruTarihi).ToList();
                        mailInstance.DuyurulariIsle(sitedenOkunanTumDuyurular);

                        logYaz("mailler gönderildi\n");
                        Console.Read();
                    }
                }


            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                olusanHatalariKaydet(ex);
                new MailInstance(kimden, kime, kadi, sifre, host, port, ssl).Gonder("Sayfa bulunamadı.\r\n" + ex.Message + "\r\n" + ex.StackTrace);
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
                File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\olusanHatalar.txt", "\r\n" + DateTime.Now + " #" + ex.Message + "\n" + ex.StackTrace);
            }

            void logYaz(string ex)
            {
                File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\log.txt", "\r\n" + DateTime.Now + " #" + ex + "\n");
            }
        }

    }
}
