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
using System.Security.Claims;

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
            // https://www.bleague.jp/schedule/?tab=2&year=2022&mon=10&day=07&event=2&club=
            HttpClient client = new HttpClient();
            string url = string.Format("https://www.bleague.jp/schedule/?tab={0}&year={1}&event={2}&mon={3}&day={4}",
                                       tabParam,
                                       yearParam,
                                       eventParam,
                                       targetDate.Month,
                                       targetDate.Day);
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
            HtmlNodeCollection rootNode = doc.DocumentNode.SelectNodes("//div[@class=\"swiper-wrapper\"]");
            var dateListItems = rootNode.Nodes();
 
            bool isCurrentFound = false;
            string currentMonthStr = String.Empty;
            string currentDayStr = String.Empty;
            foreach (var dateItem in dateListItems)
            {
                if (!isCurrentFound)
                {
                    if (dateItem.Name == "div" && dateItem.Attributes["class"].Value.Contains("is-current"))
                    {
                        isCurrentFound = true;

                        currentMonthStr = dateItem.SelectNodes("a/span")[0].InnerText.Trim();
                        currentDayStr = dateItem.SelectNodes("a/span")[1].InnerText.Trim();

                    }
                }
            }

            bool IsCurrentSameDay = false;
            if (isCurrentFound)
            {
                int currentMonth = Convert.ToInt32(currentMonthStr.Substring(0, currentMonthStr.Length - 1));
                int currentDay = Convert.ToInt32(currentDayStr);

                IsCurrentSameDay = ((currentMonth == targetDate.Month) && (currentDay == targetDate.Day));
            }

            if (IsCurrentSameDay)
            {
                HtmlNode rootGameNode = doc.DocumentNode.SelectNodes("//ul[@class=\"round-list\"]")[0];
                var gameItems = rootGameNode.SelectNodes("li[@class=\"list-item\"]");
                foreach (var gameItem in gameItems)
                {
                    var time = gameItem.SelectNodes(".//div[@class=\"info-arena\"]/span")[2].InnerText.Trim();
                    var home = gameItem.SelectNodes(".//span[@class=\"team home\"]/span[@class=\"team-name\"]")[0].InnerText.Trim();
                    var away = gameItem.SelectNodes(".//span[@class=\"team away\"]/span[@class=\"team-name\"]")[0].InnerText.Trim();

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
