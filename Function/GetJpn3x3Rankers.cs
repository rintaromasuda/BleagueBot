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
using System.IO;

namespace BleagueBot
{
    public static class GetJpn3x3Rankers
    {
        [FunctionName("GetJpn3x3Rankers")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string rankType = req.Query["type"];

            string tableName = string.Empty;
            if (rankType.Equals("m") || rankType.Equals("M"))
            {
                tableName = "men";
            }
            else if(rankType.Equals("w") || rankType.Equals("W"))
            {
                tableName = "women";
            }
            else
            {
                // This is a required param
                tableName = string.Empty;
                return new BadRequestObjectResult("No param!!!");

            }

            uint limit = Convert.ToUInt32(req.Query["limit"]);
            limit = limit == 0 ? uint.MaxValue : limit;

            var result = new Rank();
            result.RankType = rankType;

            // Access FIBA 3x3 ranking
            HttpClient client = new HttpClient();
            string url = @"https://fiba3x3.com/en/rankings/individual.html";
            string html = await client.GetStringAsync(url);

            // Parse the page
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var playerRows = doc.DocumentNode.SelectNodes(string.Format("/html/body/div[@class='ranking']/table[@data-table-name='{0}']/tbody/tr", tableName));

            uint playerCount = 0;
            foreach (var row in playerRows)
            {
                var nationality = row.Descendants("td")
                                        .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "rankings-individual__nationality"))
                                        .First()
                                        .InnerText;

                if (nationality.Trim().Equals("Japan"))
                {
                    playerCount++;
                    if (playerCount > limit)
                    {
                        break;
                    }

                    var player = new Player();

                    var rank = row.Descendants("td")
                                  .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "rankings-individual__rank"))
                                  .First()
                                  .InnerText;

                    var playerCol = row.Descendants("td")
                                       .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "rankings-individual__player"))
                                       .First();
                    var playerName = playerCol.InnerText;
                    var playerUrl = playerCol.Descendants("a").First().Attributes["href"].Value;

                    var pts = row.Descendants("td")
                                    .Where(x => x.Attributes.Contains("class") && (x.Attributes["class"].Value == "rankings-individual__points"))
                                    .First()
                                    .InnerText;

                    player.Id = playerUrl.Split('/').Last<string>();
                    player.Rank = Convert.ToUInt16(rank);
                    player.Name = playerName;
                    player.Url = playerUrl;
                    player.Points = pts;

                    result.Players.Add(player);

                    log.LogInformation(string.Format("{0}à  {1}Åi{2}ptsÅj", rank, playerName, playerUrl, pts));
                }
            }

            // Return a response
            var json = JsonConvert.SerializeObject(result, Formatting.Indented);

            return json != null
                ? (ActionResult)new OkObjectResult(json)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    internal class Player
    {
        public string Id;
        public uint Rank;
        public string Name;
        public string Url;
        public string Points;
    }

    internal class Rank
    {
        public string RankType;
        public List<Player> Players = new List<Player>();
    }
}
