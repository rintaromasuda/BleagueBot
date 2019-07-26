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
            var scheduleToReturn = new Schedule()
            {
                Date = dateParam,
                Tag = Convert.ToInt16(tagParam),
                Year = Convert.ToInt16(yearParam),
                Event = Convert.ToInt16(eventParam)
            };

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var gameListItems = doc.DocumentNode.SelectNodes("//*[@id=\"round_list\"]/dd/ul/li");
            var gameCount = gameListItems.Count;
            foreach(var gameItem in gameListItems)
            {
                if (gameItem.Attributes.Contains("data-schedule-key") && gameItem.Attributes.Contains("data-game-date"))
                {
                    var gameUnixTime = Convert.ToDouble(gameItem.Attributes["data-game-date"].Value);
                    if (startOfTheDay <= gameUnixTime && gameUnixTime < startOfTheNextDay)
                    {
                        var date = gameItem.Descendants("span")
                                        .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "date"))
                                        .First()
                                        .InnerText;

                        var time = gameItem.Descendants("span")
                                        .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "time"))
                                        .First()
                                        .InnerText;
                        time = time.Split()[0]; // Don't need the TIPOFF part

                        var home = gameItem.Descendants("span")
                                       .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "team_name home"))
                                       .First()
                                       .InnerText;

                        var away = gameItem.Descendants("span")
                                       .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "team_name"))
                                       .First()
                                       .InnerText;                                       

                        var game = new Game()
                        {
                            Date = date,
                            Time = time,
                            Home = home,
                            Away = away
                        };
                        scheduleToReturn.Games.Add(game);
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
    }

    internal class Schedule
    {
        public string Date;
        public int Tag;
        public int Year;
        public int Event;
        public List<Game> Games = new List<Game>();
    }
}
