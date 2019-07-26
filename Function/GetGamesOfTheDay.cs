using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BleagueBot.Function
{
    public static class GetGamesOfTheDay
    {
        [FunctionName("GetGamesOfTheDay")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get parameteres
            string dateParam = req.Query["date"];
            string tagParam = req.Query["tag"];
            string yearParam = req.Query["year"];
            string eventParam = req.Query["event"];

            DateTime targetDate = new DateTime();
            //bool isValidDateParm = false;
            double startOfTheDay = 0;
            double startOfTheNextDay = 0;
            if (!string.IsNullOrEmpty(dateParam))
            {
                if (DateTime.TryParse(dateParam, out targetDate))
                {
                    targetDate = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 0, 0, 0, 0); // Just in case cleaning up the time part
                    //isValidDateParm = true;
                    startOfTheDay = ConvertToUnixTime(targetDate);
                    startOfTheNextDay = ConvertToUnixTime(targetDate.AddDays(1));
                }
            }

            // Access bleague.jp
            HttpClient client = new HttpClient();
            string url = "https://www.bleague.jp/schedule/?tab=1&year=2019&event=2";
            string html = await client.GetStringAsync(url);

            // Parse the HTML and create a JSON object
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
         
            //var postTitles = doc.DocumentNode
            //                    .Descendants("td")
            //                    .Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value.Contains("title"))
            //                    .Select(x => x.InnerText);

            // Return a response
            return dateParam != null
                ? (ActionResult)new OkObjectResult($"{startOfTheDay}-{startOfTheNextDay}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
 
        internal static double ConvertToUnixTime(DateTime date)
        {
            var diff = date - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return diff.TotalSeconds;
        }
    }
}
