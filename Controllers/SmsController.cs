using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;

namespace VisitorManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsController> _logger;

        public SmsController(IConfiguration configuration, ILogger<SmsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Mesaj msj)
        {
            try
            {
                string apiUserNameKey = _configuration["SmsSettings:Username"] ?? "";
                string apiPasswordKey = _configuration["SmsSettings:Password"] ?? "";
                string apiOrganisationKey = _configuration["SmsSettings:Organisation"] ?? "";
                
                SMSPaketi smspak = new SMSPaketi(apiUserNameKey, apiPasswordKey, apiOrganisationKey, DateTime.Now);
                String[] numaralar = msj.Numaralar;

                smspak.addSMS(msj.KisaMesaj, numaralar);

                string apiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://smsgw.mutlucell.com/smsgw-ws/sndblkex";
                string sonuc = await smspak.gonder(apiUrl);
                MsjBilgisi msnj = new MsjBilgisi();

                if (sonuc == "20")
                {
                    msnj = new MsjBilgisi() { Bilgi = "Post edilen xml eksik veya hatalı.", Id = null };
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
                    msnj = new MsjBilgisi() { Bilgi = "SMS Başarılı " + split[1].ToString() + " Kredi kullanıldı.", Id = split[0] };
                }

                return Ok(msnj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS gönderim hatası");
                return BadRequest(ex.Message.ToString() + " Sistem Hatası");
            }
        }

        [HttpGet("{msj}")]
        public async Task<IActionResult> Get(int msj)
        {
            try
            {
                string apiUserNameKey = _configuration["SmsSettings:Username"] ?? "";
                string apiPasswordKey = _configuration["SmsSettings:Password"] ?? "";
                
                await Task.Delay(60000); // işlemi bir dakika bekletiyoruz.
                string reportUrl = _configuration["SmsSettings:ReportUrl"] ?? "https://smsgw.mutlucell.com/smsgw-ws/gtblkrprtex";
                string rapor = await SMSPaketi.rapor(apiUserNameKey, apiPasswordKey, msj, reportUrl);
                return Ok(rapor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS rapor alma hatası");
                return BadRequest(ex.Message.ToString() + " Sistem Hatası");
            }
        }

        [HttpPost("test")]
        public async Task<IActionResult> SendTestSms([FromBody] TestSmsRequest request)
        {
            try
            {
                string apiUserNameKey = _configuration["SmsSettings:Username"] ?? "";
                string apiPasswordKey = _configuration["SmsSettings:Password"] ?? "";
                string apiOrganisationKey = _configuration["SmsSettings:Organisation"] ?? "";
                
                SMSPaketi smspak = new SMSPaketi(apiUserNameKey, apiPasswordKey, apiOrganisationKey, DateTime.Now);
                String[] numaralar = new string[] { request.PhoneNumber };

                smspak.addSMS(request.Message, numaralar);

                string apiUrl = _configuration["SmsSettings:ApiUrl"] ?? "https://smsgw.mutlucell.com/smsgw-ws/sndblkex";
                string sonuc = await smspak.gonder(apiUrl);
                
                MsjBilgisi msnj = new MsjBilgisi();

                if (sonuc == "20")
                    msnj = new MsjBilgisi() { Bilgi = "Post edilen xml eksik veya hatalı.", Id = null };
                else if (sonuc == "21")
                    msnj = new MsjBilgisi() { Bilgi = "Kullanılan originatöre sahip değilsiniz", Id = null };
                else if (sonuc == "22")
                    msnj = new MsjBilgisi() { Bilgi = "Kontörünüz yetersiz", Id = null };
                else if (sonuc == "23")
                    msnj = new MsjBilgisi() { Bilgi = "Kullanıcı adı ya da parolanız hatalı.", Id = null };
                else if (sonuc == "24")
                    msnj = new MsjBilgisi() { Bilgi = "Şu anda size ait başka bir işlem aktif.", Id = null };
                else if (sonuc == "25")
                    msnj = new MsjBilgisi() { Bilgi = "SMSC Stopped (Bu hatayı alırsanız, işlemi 1-2 dk sonra tekrar deneyin)", Id = null };
                else if (sonuc == "30")
                    msnj = new MsjBilgisi() { Bilgi = "Hesap Aktivasyonu sağlanmamış", Id = null };
                else if (sonuc.StartsWith("$"))
                {
                    string[] split = sonuc.Split('#');
                    msnj = new MsjBilgisi() { Bilgi = "SMS Başarılı " + split[1].ToString() + " Kredi kullanıldı.", Id = split[0] };
                }
                else 
                {
                    msnj = new MsjBilgisi() { Bilgi = "Bilinmeyen yanıt: " + sonuc, Id = null };
                }

                return Ok(msnj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test SMS gönderim hatası");
                return BadRequest(ex.Message.ToString() + " Sistem Hatası");
            }
        }
    }

    public class TestSmsRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class Mesaj
    {
        public string KisaMesaj { get; set; } = string.Empty;
        public String[] Numaralar { get; set; } = Array.Empty<string>();
    }

    public class MsjBilgisi
    {
        public string Bilgi { get; set; } = string.Empty;
        public string? Id { get; set; }
    }

    public class SMSPaketi
    {
        private String start = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private String end = "</smspack>";
        private String body = "";

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
                throw new ArgumentException("SMS paketinde sms yok!");

            return start + body + end;
        }

        public async Task<String> gonder(string apiUrl = "https://smsgw.mutlucell.com/smsgw-ws/sndblkex")
        {
            using var httpClient = new HttpClient();
            string postData = xml();
            
            var content = new StringContent(postData, Encoding.UTF8, "text/xml");
            var response = await httpClient.PostAsync(apiUrl, content);
            String responseText = await response.Content.ReadAsStringAsync();
            
            return responseText;
        }

        public static async Task<String> rapor(String ka, String parola, long id, string reportUrl = "https://smsgw.mutlucell.com/smsgw-ws/gtblkrprtex")
        {
            String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                    "<smsrapor ka=\"" + ka + "\" pwd=\"" + parola + "\" id=\"" + id + "\" />";

            using var httpClient = new HttpClient();
            var content = new StringContent(xml, Encoding.UTF8, "text/xml");
            var response = await httpClient.PostAsync(reportUrl, content);
            String responseText = await response.Content.ReadAsStringAsync();

            return responseText;
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
    }
}