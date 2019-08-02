using EDefterDuyuruTakip.Modeller;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EDefterDuyuruTakip
{
    public class MailInstance
    {
        MailMessage ePosta = new MailMessage();
        List<Duyuru> duyurular = new List<Duyuru>();
        SmtpClient smtp = new SmtpClient();
        public MailInstance(string kimden, string kime, string kadi, string sifre, string host, int port, bool ssl)
        {
            ePosta.From = new MailAddress(kimden);
            ePosta.To.Add(kime);
            ePosta.Subject = "E-DEFTER DUYURULARI HAKKINDA";
            smtp.Credentials = new System.Net.NetworkCredential(kadi, sifre);
            smtp.Port = port;
            smtp.Host = host;
            smtp.EnableSsl = ssl;
        }

        public void DuyurulariIsle(List<Duyuru> duyurular)
        {
            this.duyurular = duyurular;
            Duyuru duyuru = duyurular.First();
            duyuru.Gonderildi = true;
            Gonder(duyuru);
        }

        public void Gonder(Duyuru duyuru)
        {
            ePosta.Body = duyuru.Tarih.ToShortDateString() + "\r\n" + duyuru.Icerik;
            object userState = ePosta;
            try
            {
                smtp.SendAsync(ePosta, (object)ePosta);
                smtp.SendCompleted += new SendCompletedEventHandler(mailGondermeBasarili_Event);
            }
            catch (Exception ex)
            {
                olusanHatalariKaydet(ex);
            }

        }

        public void Gonder(string mesaj)
        {
            ePosta.Body = mesaj;
            object userState = ePosta;
            try
            {
                smtp.SendAsync(ePosta, (object)ePosta);
                smtp.SendCompleted += new SendCompletedEventHandler(mailGondermeBasarili_Event);
            }
            catch (Exception ex)
            {
                olusanHatalariKaydet(ex);
            }
        }

        public void olusanHatalariKaydet(Exception ex)
        {
            File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\olusanHatalar.txt", "\n" + DateTime.Now + " #" + ex.Message + "\n" + ex.StackTrace);
        }

        private void mailGondermeBasarili_Event(object sender, AsyncCompletedEventArgs args)
        {
            if (args.Error != null && args.Error.ToString() != "")
            {
                string icerik = "\r\n" + DateTime.Now + " #mail gönderimi başarısız:" + args.Error.ToString() + "\n";
                File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\log.txt", icerik);
                File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\olusanHatalar.txt", icerik);
            }
            else
                File.AppendAllText(@"C:\AdaYazilim\ScheduledTask\log.txt", "\r\n" + DateTime.Now + " #mail gönderimi başarılı:" + args.Cancelled + "|" + args.Error + "\n");

            if (this.duyurular.Count(x => x.Gonderildi == false) > 0)
            {
                Duyuru duyuru = this.duyurular.First(x => x.Gonderildi == false);
                duyuru.Gonderildi = true;
                Gonder(duyuru);
            }
            else
            {
                System.Environment.Exit(-1);
            }
        }
    }

}

