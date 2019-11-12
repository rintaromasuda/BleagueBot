using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace BleagueBot
{
    public static class GetWLGamesOfTheDay
    {
        [FunctionName("GetWLGamesOfTheDay")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dateParam = req.Query["date"];
            string lidParam = req.Query["lid"];

            var targetDate = DateTime.Parse(dateParam);
            var targetYearMonthStr = targetDate.ToString("yyyy-MM");

            HttpClient client = new HttpClient();
            string url = string.Format("https://www.wjbl.org/schedule_result/?m={0}&l_id={1}",
                                       targetYearMonthStr,
                                       lidParam);
            string html = await client.GetStringAsync(url);

            // Get the schedule table
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableRows = doc.DocumentNode.SelectNodes("//table[@class=\"schedule table04\"]//tr");

            var scheduleToReturn = new Schedule()
            {
                Date = "",
                LID = lidParam
            };

            if (tableRows != null)
            {
                foreach (var tr in tableRows)
                {
                    var cols = tr.SelectNodes("td");
                    if (cols != null)
                    {
                        var dateStr = cols[0].SelectNodes("span")[0].InnerText.Trim();
                        var timeStr = cols[0].SelectNodes("span")[1].InnerText.Trim();
                        var homeTeam = cols[1].SelectNodes("div/div/a")[0].InnerText.Trim();
                        var awayTeam = cols[1].SelectNodes("div/div/a")[1].InnerText.Trim();

                        var gameDate = System.DateTime.Parse(dateStr, new CultureInfo("ja-jp"));
                        if (gameDate == targetDate)
                        {
                            var timePartStr = timeStr.Split("ÅF")[1];

                            var game = new Game()
                            {
                                Date = gameDate.Date.ToString("yyyy/MM/dd"),
                                Time = timePartStr,
                                Home = homeTeam,
                                Away = awayTeam,
                                Point = ""
                            };
                            scheduleToReturn.Games.Add(game);
                        }
                    }
                }
            }

            // Return a response
            var json = JsonConvert.SerializeObject(scheduleToReturn, Formatting.Indented);

            return "" != null
                ? (ActionResult)new OkObjectResult(json)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    internal class Game
    {
        public string Home { get; set; }
        public string Away { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Point { get; set; }
    }

    internal class Schedule
    {
        public string Date;
        public string LID;
        public List<Game> Games = new List<Game>();
    }
}
