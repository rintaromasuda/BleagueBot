using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

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
