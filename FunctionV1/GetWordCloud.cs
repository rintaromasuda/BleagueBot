﻿using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Drawing;
using System.Text;
using System;

namespace BleagueBotV1
{
    public static class GetWordCloud
    {
        [FunctionName("GetWordCloud")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            string base64 = data?.base64;
            base64 = "W3sid29yZCI6IkxvcyBSb2pvcyBhZnJvbnRhcsOhbiBlc3RlIHPDoWJhZG/CoGVuIFZpbGxhIE1hcmlhIEPDs3Jkb2JhwqBsYSBzw6lwdGltYSB5IMO6bHRpbWEgcGFyYWRhIGRlIGxhIGZhc2UgY2xhc2lmaWNhY2nDs24gZGUgTGEgTGlnYSAzeDMgcXVlIG9yZ2FuaXphIGxhIEFEQyIsImNvdW50IjoxfSx7IndvcmQiOiJCYXNrZXRiYWxsIENoYXJpdHkiLCJjb3VudCI6Mn0seyJ3b3JkIjoi57WQ5p6cIiwiY291bnQiOjF9LHsid29yZCI6IuqSqtCU6pKqIiwiY291bnQiOjF9LHsid29yZCI6Iuaxn+aIuOW3neWkp+WtpiIsImNvdW50IjoxfSx7IndvcmQiOiIzeDMgRXZlbnQgQmFza2V0YmFsbCBKZXJzZXkgYnkgSGVucnkgQ2hvdyIsImNvdW50IjoxfSx7IndvcmQiOiLluLDlroXjg7wiLCJjb3VudCI6MX0seyJ3b3JkIjoi44Kz44Op44Oz6Zm95LuLIOmBuOaJiyIsImNvdW50IjoxfSx7IndvcmQiOiLjg5DjgrnjgrHnlYwiLCJjb3VudCI6MX0seyJ3b3JkIjoi562R5rOi5aSn5a2mIiwiY291bnQiOjF9LHsid29yZCI6ImVuIGJ1c2NhIGRlIHVuIGx1Z2FyIGVuIGVsIEZpbmFsIEZvdXIiLCJjb3VudCI6MX0seyJ3b3JkIjoiZXZlbnQgRnVsbCBDb3VydCBTcG9uc29yIiwiY291bnQiOjF9LHsid29yZCI6IuWwj+advuaYjOW8mCDpgbjmiYsiLCJjb3VudCI6MX0seyJ3b3JkIjoiTkJBIiwiY291bnQiOjF9LHsid29yZCI6IuaXpeacrOS9k+iCsuWkp+WtpiIsImNvdW50IjoxfSx7IndvcmQiOiJiYXNxdWV0IiwiY291bnQiOjF9LHsid29yZCI6IjN4M2V4ZSIsImNvdW50IjoxfSx7IndvcmQiOiLml4vpoqgiLCJjb3VudCI6MX0seyJ3b3JkIjoiM3gzcHJlbWllcmV4ZSIsImNvdW50IjoyfSx7IndvcmQiOiLlv43ogIXpm4blm6MiLCJjb3VudCI6MX0seyJ3b3JkIjoidnPjgr/jgqQiLCJjb3VudCI6Mn0seyJ3b3JkIjoi5Yi244OQ44K544KxIiwiY291bnQiOjJ9LHsid29yZCI6IuaDheWgsSIsImNvdW50IjoxfSx7IndvcmQiOiJ2cyDjg57jg6zjg7zjgrfjgqLjgIAyMS04IHdpbiIsImNvdW50IjoxfSx7IndvcmQiOiIzeDMiLCJjb3VudCI6Mn0seyJ3b3JkIjoidnPjgrnjg6rjg6njg7PjgqsiLCJjb3VudCI6Mn0seyJ3b3JkIjoi5aSn5a2m55SfIiwiY291bnQiOjF9LHsid29yZCI6IsyB4LmRIiwiY291bnQiOjF9LHsid29yZCI6IuaXpeacrFUyMyIsImNvdW50IjoxfSx7IndvcmQiOiLjg43jgqrjg5Xjgqfjg4vjg4Pjgq/jgrkiLCJjb3VudCI6MX0seyJ3b3JkIjoidnMg5Lit5Zu944CAMjEtOSB3aW4iLCJjb3VudCI6MX0seyJ3b3JkIjoiM3gzIFUxOOOCouOCuOOCouOCq+ODg+ODlyIsImNvdW50IjoyfSx7IndvcmQiOiJFWEUgUFJFTUlFUiDjg7wg44Ki44Oh44OW44OtIiwiY291bnQiOjF9LHsid29yZCI6InRva3lvZGltZSIsImNvdW50IjoxfSx7IndvcmQiOiJkYW5pZWxiYWlsZXkg6YG45omLIiwiY291bnQiOjF9LHsid29yZCI6IuaXpeacrOWvvuaxuiIsImNvdW50IjoxfSx7IndvcmQiOiJUQkIzeDMiLCJjb3VudCI6MX0seyJ3b3JkIjoi5bCP5qCXIiwiY291bnQiOjF9LHsid29yZCI6IummmemHjCIsImNvdW50IjoyfSx7IndvcmQiOiJTSkFfWW9yayBSZWdpb24iLCJjb3VudCI6Mn0seyJ3b3JkIjoiM3gzYmFza2V0YmFsbCIsImNvdW50IjoxfSx7IndvcmQiOiLmtJfmv68iLCJjb3VudCI6MX0seyJ3b3JkIjoi44OI44O844Kv44K344On44O8IiwiY291bnQiOjF9LHsid29yZCI6IuS4ieiwtyIsImNvdW50IjoxfSx7IndvcmQiOiLkuInnlLDmnb7ogZbpq5jmoKEiLCJjb3VudCI6MX0seyJ3b3JkIjoiQnJhbmNoIEJvYXJkIERpcmVjdG9yIGF0IiwiY291bnQiOjJ9LHsid29yZCI6IuaZgueCuSIsImNvdW50IjoxfSx7IndvcmQiOiLpgbjmiYsiLCJjb3VudCI6MX0seyJ3b3JkIjoiQ29uY2VwY2lvbmRlbFVydWd1YXkiLCJjb3VudCI6MX0seyJ3b3JkIjoi5a6J5rGf5rKZIiwiY291bnQiOjJ9LHsid29yZCI6IuS4reWbveWLoiIsImNvdW50IjoxfSx7IndvcmQiOiIzeDMgV29tZW5zIHNlcmllcyBNb250cmVhbCIsImNvdW50IjoxfSx7IndvcmQiOiJMaWdhQXJnZW50aW5hIiwiY291bnQiOjF9LHsid29yZCI6IuWyuOacrCIsImNvdW50IjoyfSx7IndvcmQiOiLjg63jgrnjgr/jg7wiLCJjb3VudCI6MX0seyJ3b3JkIjoi44Oq44O844OJIiwiY291bnQiOjF9LHsid29yZCI6IlJvY2Ftb3JhIiwiY291bnQiOjF9LHsid29yZCI6IummrOeTnOOCueODhuODleOCoeODi+ODvCIsImNvdW50IjoxfSx7IndvcmQiOiIzeDNXUyIsImNvdW50IjoyfSx7IndvcmQiOiJ2cyDkuK3lm71VMjPjgIAxNi0yMSBsb3NlIiwiY291bnQiOjF9LHsid29yZCI6IjN4MyBCYXNrZXRiYWxsIEplcnNleSBieSBIZW5yeSBDaG93IiwiY291bnQiOjF9LHsid29yZCI6IuaxuuWLnSIsImNvdW50IjoxfSx7IndvcmQiOiLmm7TmlrAiLCJjb3VudCI6MX0seyJ3b3JkIjoiM3gzVTE4IOOCouOCuOOCouOCq+ODg+ODlyIsImNvdW50IjoxfSx7IndvcmQiOiJNcnMuIFN1c2FuYSBDaGVuIG9uIGJlaGFsZiBvZiBTdGV2ZW4gQ2hvbmcgQ0EgUHJvZmVzc2lvbmFsIENvcnAiLCJjb3VudCI6MX0seyJ3b3JkIjoi44GL44KCIiwiY291bnQiOjF9LHsid29yZCI6IuOBvuOBlyIsImNvdW50IjoxfSx7IndvcmQiOiJwcmVzZW50ZWQiLCJjb3VudCI6Mn0seyJ3b3JkIjoi44Go44GT44KN5Luj6KGo5oimIiwiY291bnQiOjF9LHsid29yZCI6IsyA0LQiLCJjb3VudCI6MX0seyJ3b3JkIjoi5biC5bedIiwiY291bnQiOjF9LHsid29yZCI6IuiEs+WGhSIsImNvdW50IjoxfSx7IndvcmQiOiIzeDNleGVwcmVtaWVyIiwiY291bnQiOjF9LHsid29yZCI6IuaVtOeQhiIsImNvdW50IjoxfSx7IndvcmQiOiLmqKrlnLAiLCJjb3VudCI6MX0seyJ3b3JkIjoi44OQ44K544Kx44OD44OI44Kz44O844OIIiwiY291bnQiOjF9LHsid29yZCI6IjV4NSB5IiwiY291bnQiOjF9LHsid29yZCI6IuOCouODg+ODlyIsImNvdW50IjoxfSx7IndvcmQiOiJ2cyDml6XmnKwiLCJjb3VudCI6MX0seyJ3b3JkIjoiVml0YW1pbiBCIiwiY291bnQiOjF9LHsid29yZCI6IlN0LiBKb2huIEFtYnVsYW5jZSIsImNvdW50IjoyfSx7IndvcmQiOiIzeDMgVTE4IEFzaWFDdXAg5Yid5pelIiwiY291bnQiOjF9LHsid29yZCI6IuOCteOCpOODs+S8miIsImNvdW50IjoxfSx7IndvcmQiOiJDYWxlbmRhcmlvIHkgZGVtw6FzIiwiY291bnQiOjF9LHsid29yZCI6IumtheWKmyIsImNvdW50IjoxfSx7IndvcmQiOiJWaXRhbWluIEMiLCJjb3VudCI6MX0seyJ3b3JkIjoiYW5udWFsIiwiY291bnQiOjJ9LHsid29yZCI6IuaXpeacrOS7o+ihqCIsImNvdW50IjoxfSx7IndvcmQiOiLkuInlpb3ljZfnqYIiLCJjb3VudCI6MX0seyJ3b3JkIjoi5bKp55Sw6YGL5YuV5YWs5ZyS5YaFIiwiY291bnQiOjF9LHsid29yZCI6IkJhbG9uY2VzdG9lblQgR3JhY2lhcyBwb3IgZGlmdW5kaXIgbGEgaW5mb3JtYWNpw7NuIGRlbCBDYW1wZW9uYXRvIE5hY2lvbmFsIiwiY291bnQiOjF9LHsid29yZCI6IuaLoeaVoyIsImNvdW50IjoxfSx7IndvcmQiOiJWaXRhbWluIEQiLCJjb3VudCI6MX0seyJ3b3JkIjoiZGltZTN4MyIsImNvdW50IjoxfSx7IndvcmQiOiIzb24zIiwiY291bnQiOjJ9LHsid29yZCI6IuOBi+OBqiIsImNvdW50IjoxfSx7IndvcmQiOiJKRE5KMjAxOSBNYXNjdWxpbm8geSBGZW1lbmlubyIsImNvdW50IjoxfSx7IndvcmQiOiLlhoXph47mmbrpppnoi7EiLCJjb3VudCI6MX0seyJ3b3JkIjoi44OX44Os44KkIiwiY291bnQiOjF9LHsid29yZCI6IlNKQTN4MyDwn4+AIiwiY291bnQiOjF9LHsid29yZCI6IuS7pemZjSIsImNvdW50IjoxfSx7IndvcmQiOiLlh7rloLTjg4Hjg7zjg6AiLCJjb3VudCI6MX0seyJ3b3JkIjoi6YCy5Ye6IiwiY291bnQiOjF9LHsid29yZCI6IlRoYW5reW91IiwiY291bnQiOjF9LHsid29yZCI6IuOBi+OBkSIsImNvdW50IjoxfSx7IndvcmQiOiLnr6DltI7mvqoiLCJjb3VudCI6MX0seyJ3b3JkIjoi44OB44Kn44OD44KvIiwiY291bnQiOjF9LHsid29yZCI6IuippuWQiCIsImNvdW50IjoyfSx7IndvcmQiOiLvvbfvvpjvva8iLCJjb3VudCI6MX0seyJ3b3JkIjoiTXIuIFdlaSBmcm9tIENISU5BIENJVFkiLCJjb3VudCI6MX0seyJ3b3JkIjoiM3gzZXhlIOODl+ODrOODn+OCoiIsImNvdW50IjoxfSx7IndvcmQiOiJVMjPml6XmnKwgdnMgVEgiLCJjb3VudCI6MX0seyJ3b3JkIjoi44K444O844K444KqIiwiY291bnQiOjF9LHsid29yZCI6IkFrYXRzdWtpRml2ZSIsImNvdW50IjoxfSx7IndvcmQiOiLjg5njgqTjg7MiLCJjb3VudCI6MX0seyJ3b3JkIjoi5a6f5pa9IiwiY291bnQiOjF9LHsid29yZCI6InNpbW9uZXhlIiwiY291bnQiOjF9LHsid29yZCI6IkLjg6rjg7zjgrAiLCJjb3VudCI6MX0seyJ3b3JkIjoi55m75aC0IiwiY291bnQiOjF9LHsid29yZCI6ImxhIHNlbWFuYSBxdWUgdmllbmUgZXN0YXJlbW9zIGhhY2llbmRvIGxsZWdhciBndcOtYSBpbmZvcm1hdGl2YSBjb24gdG9kYSBsYSBpbmZvcm1hY2nDs24gZGVsIHRvcm5lbyIsImNvdW50IjoxfSx7IndvcmQiOiLoqbPntLAiLCJjb3VudCI6MX0seyJ3b3JkIjoiQ2xhc2lmaWNhdG9yaW9VMTfwn4+AIiwiY291bnQiOjF9LHsid29yZCI6IvCfmIXwn5Km5rS76LqNIiwiY291bnQiOjF9LHsid29yZCI6ImRlIGlndWFsIG1hbmVyYSIsImNvdW50IjoxfSx7IndvcmQiOiLml6XmnKwiLCJjb3VudCI6MX0seyJ3b3JkIjoiVVRTVU5PTUlZQSIsImNvdW50IjoxfSx7IndvcmQiOiJJdmV0dGVCb3JnZXMiLCJjb3VudCI6MX0seyJ3b3JkIjoi5rqW5rG6IFUyM+S4reWbvSB2cyBVMjPml6XmnKwiLCJjb3VudCI6MX0seyJ3b3JkIjoibG9zIiwiY291bnQiOjF9LHsid29yZCI6IuOBqOOBkyIsImNvdW50IjoxfSx7IndvcmQiOiJ2cyDkuK3lm73jgIAxNC0xNSBsb3NlIiwiY291bnQiOjF9LHsid29yZCI6IuaXpeacrOaZgumWkyIsImNvdW50IjoxfSx7IndvcmQiOiLltovmtKXlj4siLCJjb3VudCI6MX0seyJ3b3JkIjoidnMg44Oi44Oz44K044Or44CAMjAtMTUgd2luIiwiY291bnQiOjF9LHsid29yZCI6InZzIOOCv+OCpOOAgDIyLTcgd2luIiwiY291bnQiOjF9LHsid29yZCI6InZzIOODi+ODpeODvOOCuOODvOODqeODs+ODieOAgDIxLTEyIHdpbiIsImNvdW50IjoxfSx7IndvcmQiOiJCUkVYRVhFIiwiY291bnQiOjF9LHsid29yZCI6IuOCueOCpOOCuSIsImNvdW50IjoxfSx7IndvcmQiOiLjg4vjg7Pjgrjjg6PjgqjjgqLjg7zjgroiLCJjb3VudCI6MX0seyJ3b3JkIjoi44OJ44Kl44K144Oz44Od44Od44OT44OD44OBIiwiY291bnQiOjF9LHsid29yZCI6Im5pbmphYWlycyIsImNvdW50IjoxfSx7IndvcmQiOiLliqDol6TpgaXoj5wiLCJjb3VudCI6MX0seyJ3b3JkIjoiR3J1cG9zIiwiY291bnQiOjF9LHsid29yZCI6InZzIE5aIiwiY291bnQiOjF9LHsid29yZCI6ImR1c2FucG9wb3ZpYyIsImNvdW50IjoxfSx7IndvcmQiOiJyYXllcjE4IiwiY291bnQiOjF9LHsid29yZCI6Iua6luaxuiDkuK3lm70iLCJjb3VudCI6MX0seyJ3b3JkIjoiM3gzRVhFUFJFTUlFUiIsImNvdW50IjoxfSx7IndvcmQiOiJQaXBhR3V0aWVycmV6IHkgUm9zY28gZW4gZWwgZXZlbnRvIiwiY291bnQiOjF9LHsid29yZCI6IuWFqOmDqOS7leS6i+S4rSIsImNvdW50IjoxfSx7IndvcmQiOiLpiLTmnKjkvpEiLCJjb3VudCI6MX0seyJ3b3JkIjoiRWwgZG9taW5nbyBlc3RhcmVtb3MgZW4gQm91bG9nbmUgcGFyYSB1biBudWV2byIsImNvdW50IjoxfSx7IndvcmQiOiLvuI7jg6njgqTjg5bphY3kv6EiLCJjb3VudCI6MX0seyJ3b3JkIjoidnMg44Ki44Oh44Oq44Kr44CA5LiN5oim5YudIiwiY291bnQiOjF9LHsid29yZCI6IuWls+WtkCIsImNvdW50IjoxfSx7IndvcmQiOiJjb21wZXRpciB5IGRpc2ZydXRhciBqdW50byBhbCBiw6FzcXVldCIsImNvdW50IjoxfSx7IndvcmQiOiLmuIXmsLTpmobkuq4iLCJjb3VudCI6MX0seyJ3b3JkIjoiM1gzIPCfj4AiLCJjb3VudCI6MX0seyJ3b3JkIjoi5rG65Yud5oimIiwiY291bnQiOjF9LHsid29yZCI6IkVzdGFyw6FuIiwiY291bnQiOjF9LHsid29yZCI6IkxvcyBlc3BlcmFtb3MiLCJjb3VudCI6MX0seyJ3b3JkIjoidmVzdWJpbzI3MTEiLCJjb3VudCI6MX0seyJ3b3JkIjoiU2FuSXNpZHJvR29iIiwiY291bnQiOjF9LHsid29yZCI6ImJhbmNvcHJvdmluY2lhIiwiY291bnQiOjF9LHsid29yZCI6IuevoOW0jiIsImNvdW50IjoxfSx7IndvcmQiOiLkuInlpb0iLCJjb3VudCI6MX0seyJ3b3JkIjoi5bGx5pysIiwiY291bnQiOjF9XQ==";

            // Base64 to String
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));

            // Parse JSON
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordItem>>(json);
            var words = new List<string>();
            var counts = new List<int>();
            items.ForEach((item) => {
                words.Add(item.Word);
                counts.Add(item.Count);
            });

            // Create a WordCloud image
            var wc = new WordCloud.WordCloud(900, 600);
            var wcImage = wc.Draw(words, counts);

            // Change background color
            Bitmap wcBmp = new Bitmap(wcImage.Width, wcImage.Height);
            using (Graphics g = Graphics.FromImage(wcBmp))
            {
                g.Clear(Color.White);
                g.DrawImage(wcImage, new Point { X = 0, Y = 0 });
            }

            // Return the image as a response
            ImageConverter converter = new ImageConverter();
            var byteArray = (byte[])converter.ConvertTo(wcBmp, typeof(byte[]));

            HttpResponseMessage res = new HttpResponseMessage();
            res.Content = new ByteArrayContent(byteArray);
            res.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            res.StatusCode = HttpStatusCode.OK;

            return json == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : res;
        }
    }

    public class WordItem
    {
        public string Word { get; set; }
        public int Count { get; set; }
    }
}