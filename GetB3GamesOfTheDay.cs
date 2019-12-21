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
    public static class GetB3GamesOfTheDay
    {
        [FunctionName("GetB3GamesOfTheDay")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dateParam = req.Query["date"];

            var targetDate = DateTime.Parse(dateParam);
            var targetYearStr = targetDate.ToString("yyyy");

            HttpClient client = new HttpClient();
            string url = "https://www.b3league.jp/schedule/";
            string html = await client.GetStringAsync(url);

            // Get the schedule table
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var tableRows = doc.DocumentNode.SelectNodes("//tr[@class=\"siro\" or @class=\"kuro\"]");

            var scheduleToReturn = new Schedule()
            {
                Date = ""
            };

            if (tableRows != null)
            {
                foreach (var tr in tableRows)
                {
                    var cols = tr.SelectNodes("td");
                    if (cols != null && cols.Count >= 5)
                    {
                        var dateStr = cols[0].InnerText.Trim();
                        var timeStr = cols[5].InnerText.Trim();
                        var homeTeam = cols[4].InnerText.Trim();
                        var awayTeam = cols[6].InnerText.Trim();

                        var seachDateStr = targetDate.ToString("MM/dd");
                        if (dateStr.Contains(seachDateStr))
                        {
                            if (timeStr.IndexOf("TIP") > 0)
                            {
                                timeStr = timeStr.Substring(0, timeStr.IndexOf("TIP"));
                            }

                            var game = new Game()
                            {
                                Date = targetDate.Date.ToString("yyyy/MM/dd"),
                                Time = timeStr,
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
            public List<Game> Games = new List<Game>();
        }
    }
}
