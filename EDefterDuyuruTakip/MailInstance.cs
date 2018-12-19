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
        SmtpClient smtp = new SmtpClient();
        public MailInstance(string kimden,string kime,string kadi,string sifre,string host,int port,bool ssl)
        {
            ePosta.From = new MailAddress(kimden);
            ePosta.To.Add(kime);
            ePosta.Subject = "E-DEFTER DUYURULARI HAKKINDA";
            smtp.Credentials = new System.Net.NetworkCredential(kadi, sifre);
            smtp.Port = port;
            smtp.Host = host;
            smtp.EnableSsl = ssl; 
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
            string olusanTumHatalar = File.ReadAllText("olusanHatalar.txt");
            olusanTumHatalar += "\n #" + ex.Message + "\n" + ex.StackTrace;
            File.WriteAllText("olusanHatalar.txt", olusanTumHatalar);
        }
        private void mailGondermeBasarili_Event(object sender, AsyncCompletedEventArgs args)
        {
            System.Environment.Exit(-1);
        }
    }
    
}

