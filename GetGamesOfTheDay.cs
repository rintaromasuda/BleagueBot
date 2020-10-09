using HtmlAgilityPack;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace BleagueBot.Function
{
    public static class GetGamesOfTheDay
    {
        [FunctionName("GetGamesOfTheDay")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // Get parameteres
            string dateParam = req.Query["date"];
            string tabParam = req.Query["tab"];
            string yearParam = req.Query["year"];
            string eventParam = req.Query["event"];

            DateTime targetDate = new DateTime();
            bool isValidDateParm = false;
            double startOfTheDay = 0;
            double startOfTheNextDay = 0;
            if (!string.IsNullOrEmpty(dateParam))
            {
                if (DateTime.TryParse(dateParam, out targetDate))
                {
                    targetDate = new DateTime(targetDate.Year, targetDate.Month, targetDate.Day, 0, 0, 0, 0); // Just in case cleaning up the time part
                    isValidDateParm = true;
                    startOfTheDay = ConvertToUnixTime(targetDate);
                    startOfTheNextDay = ConvertToUnixTime(targetDate.AddDays(1));
                }
            }

            // Param check
            if (!isValidDateParm || (tabParam.Equals("0")) || (yearParam.Equals("0")) || (eventParam.Equals("0")))
            {
                return new BadRequestObjectResult(string.Format("Invalid param(s): date:{0}, year:{1}, tag:{2}, event:{3}",
                                                                dateParam,
                                                                yearParam,
                                                                tabParam,
                                                                eventParam));
            }

            // Access bleague.jp
            HttpClient client = new HttpClient();
            string url = string.Format("https://www.bleague.jp/schedule/?tab={0}&year={1}&event={2}",
                                       tabParam,
                                       yearParam,
                                       eventParam);
            string html = await client.GetStringAsync(url);

            // Parse the HTML and create a JSON object
            var scheduleToReturn = new Schedule()
            {
                Date = dateParam,
                Tab = Convert.ToInt16(tabParam),
                Year = Convert.ToInt16(yearParam),
                Event = Convert.ToInt16(eventParam)
            };

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var rootNode =  doc.DocumentNode.SelectNodes("//section[@class=\"round\"]/dl[" + tabParam + "]");

            var dateListItems = rootNode.Nodes();
            DateTime gameDate = DateTime.MinValue;

            //var gameCount = gameListItems.Count;
            foreach(var dateItem in dateListItems)
            {
                if(dateItem.Name == "dt")
                {
                    var gameDateStr = dateItem.InnerText;
                    var cultureInfo = new CultureInfo("ja-JP");
                    gameDate = DateTime.Parse(gameDateStr.Split()[0], cultureInfo);
                }

                if (dateItem.Name == "dd")
                {
                    if (gameDate.Date == targetDate.Date)
                    {
                        var gameItems = dateItem.Descendants("li")
                                            .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "round__list--item"));
                        foreach(var gameItem in gameItems)
                        {
                            var arena = gameItem.Descendants("div")
                                .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "arena"))
                                .First();
                            var timeTag = arena.SelectNodes(".//span[3]");
                            var time = timeTag.Nodes().First().InnerText;

                            time = time.Split()[0]; // Don't need the TIPOFF part

                            var home = gameItem.Descendants("span")
                                           .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "team_name home"))
                                           .First()
                                           .InnerText;

                            var away = gameItem.Descendants("span")
                                           .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "team_name"))
                                           .First()
                                           .InnerText;

                            string point = string.Empty;

                            var game = new Game()
                            {
                                Date = dateParam,
                                Time = time,
                                Home = home,
                                Away = away,
                                Point = point
                            };
                            scheduleToReturn.Games.Add(game);
                        }
                    }
                }
            }

            // Return a response
            var json = JsonConvert.SerializeObject(scheduleToReturn, Formatting.Indented);
            
            return dateParam != null
                ? (ActionResult)new OkObjectResult(json)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
 
        internal static double ConvertToUnixTime(DateTime date)
        {
            var diff = date - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return diff.TotalSeconds;
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
        public int Tab;
        public int Year;
        public int Event;
        public List<Game> Games = new List<Game>();
    }
}
