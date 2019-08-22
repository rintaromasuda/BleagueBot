using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Drawing;

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
            string json = data?.json;
            //json = "[{\"word\":\"社長交代\",\"count\":6},{\"word\":\"本気\",\"count\":1},{\"word\":\"島田体制\",\"count\":3},{\"word\":\"クラブ\",\"count\":4},{\"word\":\"ジェッツ\",\"count\":23},{\"word\":\"島田慎\",\"count\":1},{\"word\":\"キャラブレ\",\"count\":1},{\"word\":\"米盛\",\"count\":4},{\"word\":\"Bリーグ\",\"count\":2},{\"word\":\"就任\",\"count\":4},{\"word\":\"商科大学マッチデー\",\"count\":1},{\"word\":\"経営\",\"count\":5},{\"word\":\"chibajets\",\"count\":2},{\"word\":\"🏀社長交代🏀\",\"count\":1},{\"word\":\"船橋アリーナ\",\"count\":1},{\"word\":\"継承\",\"count\":8},{\"word\":\"次代へ\",\"count\":3},{\"word\":\"米盛勇哉氏\",\"count\":3},{\"word\":\"コルセアーズ戦\",\"count\":1},{\"word\":\"バスケットボール\",\"count\":1},{\"word\":\"ジェットコースター\",\"count\":1},{\"word\":\"横浜ビー\",\"count\":1},{\"word\":\"富樫勇樹\",\"count\":1},{\"word\":\"コーヒーカップ\",\"count\":1},{\"word\":\"サービス創造学部\",\"count\":1},{\"word\":\"ビックリ\",\"count\":1},{\"word\":\"焼き鳥\",\"count\":1},{\"word\":\"社長\",\"count\":4},{\"word\":\"まし\",\"count\":1},{\"word\":\"ナスハイ\",\"count\":1},{\"word\":\"挑戦\",\"count\":3},{\"word\":\"応援\",\"count\":1},{\"word\":\"ヤバ\",\"count\":1},{\"word\":\"島田慎二氏\",\"count\":1},{\"word\":\"たい\",\"count\":1},{\"word\":\"転職\",\"count\":1},{\"word\":\"代表取締役会長\",\"count\":1},{\"word\":\"入社\",\"count\":1},{\"word\":\"思い\",\"count\":1},{\"word\":\"犬サポ\",\"count\":1},{\"word\":\"抱負\",\"count\":1},{\"word\":\"球団社長\",\"count\":1},{\"word\":\"連続黒字\",\"count\":1},{\"word\":\"まげ\",\"count\":1},{\"word\":\"経常利益\",\"count\":1},{\"word\":\"Chu_erina_avex\",\"count\":1},{\"word\":\"突破\",\"count\":1},{\"word\":\"いき\",\"count\":1},{\"word\":\"ニュース\",\"count\":1},{\"word\":\"開催\",\"count\":1},{\"word\":\"試み\",\"count\":1},{\"word\":\"バスケットボールキング\",\"count\":1},{\"word\":\"ユニフォーム\",\"count\":1},{\"word\":\"成長\",\"count\":2},{\"word\":\"背番号\",\"count\":1},{\"word\":\"🔥\",\"count\":1},{\"word\":\"😂\",\"count\":1},{\"word\":\"先陣\",\"count\":1},{\"word\":\"代表取締役社長\",\"count\":1},{\"word\":\"ヨダレ\",\"count\":1},{\"word\":\"米盛勇哉\",\"count\":1},{\"word\":\"バスケット\",\"count\":2},{\"word\":\"取締役会\",\"count\":1},{\"word\":\"Yahoo!ニュース\",\"count\":2},{\"word\":\"カウント\",\"count\":2},{\"word\":\"こと\",\"count\":1},{\"word\":\"島田慎二\",\"count\":1},{\"word\":\"バスケットカウント\",\"count\":1},{\"word\":\"キモオタ\",\"count\":1},{\"word\":\"playerapp\",\"count\":1},{\"word\":\"EXPスポーツニュース\",\"count\":1},{\"word\":\"批判\",\"count\":1},{\"word\":\"間違い\",\"count\":1},{\"word\":\"代表取締役 社長\",\"count\":1},{\"word\":\"動き\",\"count\":1},{\"word\":\"Bリーグ運営\",\"count\":1},{\"word\":\"異動\",\"count\":1},{\"word\":\"注目👀\",\"count\":1},{\"word\":\"ウイリアムソン\",\"count\":1},{\"word\":\"追加選任\",\"count\":1},{\"word\":\"今後\",\"count\":1},{\"word\":\"ドンチッチ\",\"count\":1},{\"word\":\"報告\",\"count\":1},{\"word\":\"💦\",\"count\":1},{\"word\":\"イオン\",\"count\":1},{\"word\":\"持ち主\",\"count\":1},{\"word\":\"キャリア\",\"count\":1},{\"word\":\"ルカ\",\"count\":1},{\"word\":\"プレー\",\"count\":1}]";
            json = "[{\"word\":\"アルゼンチン戦\",\"count\":1},{\"word\":\"辻直人\",\"count\":1},{\"word\":\"日本代表\",\"count\":1},{\"word\":\"男子バスケ日本一丸\",\"count\":1},{\"word\":\"こと\",\"count\":1},{\"word\":\"試合\",\"count\":2},{\"word\":\"負け\",\"count\":1},{\"word\":\"ボックススコア\",\"count\":1},{\"word\":\"か4Q\",\"count\":1},{\"word\":\"大事\",\"count\":1},{\"word\":\"日本🇯🇵 vs アルゼンチン🇦🇷\",\"count\":1},{\"word\":\"AkatsukiFive\",\"count\":6},{\"word\":\"試合運び\",\"count\":1},{\"word\":\"いただき\",\"count\":1},{\"word\":\"会場\",\"count\":1},{\"word\":\"くらい\",\"count\":1},{\"word\":\"勉強\",\"count\":1},{\"word\":\"比江島慎\",\"count\":1},{\"word\":\"チーム\",\"count\":1},{\"word\":\"観戦\",\"count\":1},{\"word\":\"録画\",\"count\":1},{\"word\":\"リード\",\"count\":1},{\"word\":\"結果\",\"count\":1},{\"word\":\"時間帯\",\"count\":1},{\"word\":\"日本一丸\",\"count\":1},{\"word\":\"期待\",\"count\":2},{\"word\":\"勢い\",\"count\":1},{\"word\":\"今後\",\"count\":1},{\"word\":\"スコアラー\",\"count\":1},{\"word\":\"勝負強さ\",\"count\":1},{\"word\":\"アルゼンチン\",\"count\":2},{\"word\":\"🏀日本vsアルゼンチン🏀\",\"count\":1},{\"word\":\"勝負\",\"count\":1},{\"word\":\"世界ランク上位\",\"count\":1},{\"word\":\"さいたまスーパーアリーナ\",\"count\":1},{\"word\":\"くれ\",\"count\":1},{\"word\":\"希望\",\"count\":1},{\"word\":\"ラスト以外\",\"count\":1},{\"word\":\"高揚感\",\"count\":1},{\"word\":\"ドイツ\",\"count\":1},{\"word\":\"本戦期待\",\"count\":1},{\"word\":\"国際親善試合\",\"count\":1},{\"word\":\"ボコ\",\"count\":1},{\"word\":\"BABADUNK\",\"count\":1},{\"word\":\"3Q\",\"count\":1},{\"word\":\"akatsukifive\",\"count\":1},{\"word\":\"🏀\",\"count\":1},{\"word\":\"川崎ブレイブサンダース感\",\"count\":1},{\"word\":\"流れ\",\"count\":1},{\"word\":\"Bリーグ\",\"count\":1}]";

            // Parse JSON
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<WordItem>>(json);
            var words = new List<string>();
            var counts = new List<int>();
            items.ForEach((item) => {
                words.Add(item.Word);
                counts.Add(item.Count);
            });

            // Create a WordCloud image
            var wc = new WordCloud.WordCloud(1600, 900);
            var wcImage = wc.Draw(words, counts);

            // Return the image as a response
            ImageConverter converter = new ImageConverter();
            var byteArray = (byte[])converter.ConvertTo(wcImage, typeof(byte[]));

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
