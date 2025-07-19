using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ISG.WebApi.Controllers
{

    [Authorize]
    [RoutePrefix("api/sms")]
    public class SmsController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> Post(Mesaj msj)
        {
            try
            {
                // Kullanici adi, parola ve Originator kullanilarak bir sms paketi olusturulur.
                string apiUserNameKey = ConfigurationManager.AppSettings["SMS_User"];
                string apiPasswordKey = ConfigurationManager.AppSettings["SMS_Password"];
                string apiLinkKey = ConfigurationManager.AppSettings["emailApiKey"];
                string apiOrganasitonKey = ConfigurationManager.AppSettings["SMS_Organisation"];
                SMSPaketi smspak = new SMSPaketi(apiUserNameKey, apiPasswordKey, apiOrganasitonKey, DateTime.Now);              
                String[] numaralar = msj.Numaralar;

                smspak.addSMS(msj.KisaMesaj, numaralar);
                // sonuc eger mesaj basarili ise # ile baslayan bir mesaj ID'dir. 
                // bir hata olusmussa XML dokumaninda belirtilen hata kodlarindan biri doner

                string sonuc = await smspak.gonder();
                MsjBilgisi msnj = new MsjBilgisi();

                if (sonuc == "20")
                {
                    msnj= new MsjBilgisi() { Bilgi= "Post edilen xml eksik veya hatalı.",Id= null};
                }
                if (sonuc == "21")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Kullanılan originatöre sahip değilsiniz", Id = null };
                }
                if (sonuc == "22")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Kontörünüz yetersiz", Id = null };
                }
                if (sonuc == "23")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Kullanıcı adı ya da parolanız hatalı.", Id = null };
                }
                if (sonuc == "24")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Şu anda size ait başka bir işlem aktif.", Id = null };
                }
                if (sonuc == "25")
                {
                    msnj = new MsjBilgisi() { Bilgi = "SMSC Stopped (Bu hatayı alırsanız, işlemi 1-2 dk sonra tekrar deneyin)", Id = null };
                }
                if (sonuc == "30")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Hesap Aktivasyonu sağlanmamış", Id = null };
                }
                if (sonuc.Substring(0, 1) == "$")
                {
                    string[] split = sonuc.Split('#');
                    msnj = new MsjBilgisi() { Bilgi = "SMS Başarılı " + split[1].ToString() + " Kredi kullanıldı.", Id = split[0]};
                }
                //Raporun cekilmesi

                // rapor kullanici adi, parola ve mesaj gonderme isleminde sonuc olarak donen 

                // message ID ile cekilir. XML dokumaninda belirtilen formatta doner

                //Thread.Sleep(60000);// işlemi bir dakikak bekletiyoruz.
                //string rapor = await SMSPaketi.rapor(apiUserNameKey, apiPasswordKey, Convert.ToInt32(sonuc.Substring(0, 1)));
                return Ok(await Task.Run(() => msnj));
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, await Task.Run(() => ex.Message.ToString() + " Sistem Hatası"));
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(int msj)
        {
            try
            {
                // Kullanici adi, parola ve Originator kullanilarak bir sms paketi olusturulur.
                string apiUserNameKey = ConfigurationManager.AppSettings["SMS_User"];
                string apiPasswordKey = ConfigurationManager.AppSettings["SMS_Password"];
                string apiLinkKey = ConfigurationManager.AppSettings["emailApiKey"];
                string apiOrganasitonKey = ConfigurationManager.AppSettings["SMS_Organisation"];
                Thread.Sleep(60000);// işlemi bir dakikak bekletiyoruz.
                string rapor = await SMSPaketi.rapor(apiUserNameKey, apiPasswordKey,msj);
                return Ok(await Task.Run(() => rapor));
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.NotFound, await Task.Run(() => ex.Message.ToString() + " Sistem Hatası"));
            }
        }

    }

    public class Mesaj
    {
        public string KisaMesaj { get; set; }
        public String[] Numaralar { get; set; }

    }

    public class MsjBilgisi
    {
        public string Bilgi { get; set; }
        public string Id { get; set; }

    }

    public class SMSPaketi
    {
        public SMSPaketi(String ka, String parola, String org)
        {
            start += "<smspack ka=\"" + xmlEncode(ka) + "\" pwd=\"" + xmlEncode(parola)

                    + "\" org=\"" + xmlEncode(org) + "\">";

        }

        public SMSPaketi(String ka, String parola, String org, DateTime tarih)

        {

            start += "<smspack ka=\"" + xmlEncode(ka) + "\" pwd=\"" + xmlEncode(parola) +

                    "\" org=\"" + xmlEncode(org) + "\" tarih=\"" + tarihStr(tarih) + "\">";



        }

        public void addSMS(String mesaj, String[] numaralar)

        {

            body += "<mesaj><metin>";

            body += xmlEncode(mesaj);

            body += "</metin><nums>";

            foreach (String s in numaralar)

            {

                body += xmlEncode(s) + ",";

            }

            body += "</nums></mesaj>";

        }

        public String xml()
        {

            if (body.Length == 0)

                throw new ArgumentException("SMS paketinede sms yok!");

            return start + body + end;

        }

        public async Task<String> gonder()
        {
            WebClient wc = new WebClient();
            string postData = xml();
            wc.Headers.Add("Content-Type", "text/xml; charset=UTF-8");
            // Apply ASCII Encoding to obtain the string as a byte array.
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            byte[] responseArray = await wc.UploadDataTaskAsync("https://smsgw.mutlucell.com/smsgw-ws/sndblkex", "POST", byteArray);
            String response = Encoding.UTF8.GetString(responseArray);
            return response;
        }

        public static async Task<String> rapor(String ka, String parola, long id)
        {
            String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +

                    "<smsrapor ka=\"" + ka + "\" pwd=\"" + parola + "\" id=\"" + id + "\" />";

            WebClient wc = new WebClient();

            // MessageBox.Show(xml);

            string postData = xml;

            wc.Headers.Add("Content-Type", "text/xml; charset=UTF-8");

            // Apply ASCII Encoding to obtain the string as a byte array.

            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            byte[] responseArray = await wc.UploadDataTaskAsync("https://smsgw.mutlucell.com/smsgw-ws/gtblkrprtex", "POST", byteArray);

            String response = Encoding.UTF8.GetString(responseArray);

            return response;

        }

        private static String tarihStr(DateTime d)
        {

            return xmlEncode(d.ToString("yyyy-MM-dd HH:mm"));

        }

        private static String xmlEncode(String s)

        {

            s = s.Replace("&", "&amp;");

            s = s.Replace("<", "&lt;");

            s = s.Replace(">", "&gt;");

            s = s.Replace("'", "&apos;");

            s = s.Replace("\"", "&quot;");

            return s;

        }

        private String start = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

        private String end = "</smspack>";

        private String body = "";

    }

}
