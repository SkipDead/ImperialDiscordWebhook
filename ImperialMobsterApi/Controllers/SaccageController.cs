using Microsoft.AspNetCore.Mvc;
using Discord.Webhook;
using ImperialWebHook;
using Newtonsoft.Json;
using Discord;
using System.Drawing;
using AngleSharp;

namespace ImperialMobsterApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SaccageController : ControllerBase
    {

        private readonly ILogger<SaccageController> _logger;

        const string saccage_url = "https://imperial-mob.forumactif.com/f9-saccages";
        const string WEBHOOK = "https://discord.com/api/webhooks/895112443435560970/mDhvji_8eUEuw-HICgIYA6cuKnBLwtY09BMlwoivXKE16qIW0etY2k3kiquqTuH25u4q";

        public SaccageController(ILogger<SaccageController> logger)
        {
            _logger = logger;
        }

       
        [HttpGet]
        public async void Get()
        {
            List<string> saccageLinkList = new List<string>();
            List<string> saccageTitresList = new List<string>();
            List<string> saccagesNamesList = new List<string>();

            await ExtractSaccageHTML(saccageLinkList, saccageTitresList, saccagesNamesList);

            string json;
            using (StreamReader r = new StreamReader("topics.json"))
            {
                json = r.ReadToEnd();
            }
            Topic topic = JsonConvert.DeserializeObject<Topic>(json);
            var filtredlist = saccageLinkList.Where(p => topic.topics.All(p2 => p2 != p)).ToList();
            topic.topics.AddRange(filtredlist);
            topic.topics.RemoveAll(p => saccageLinkList.All(p2 => p2 != p));
            string newjson = JsonConvert.SerializeObject(topic);
            System.IO.File.WriteAllText("topics.json", newjson);

            if (filtredlist.Any())
                SendDiscordMessage(saccageLinkList, saccageTitresList, saccagesNamesList, filtredlist);

        }

        private static void SendDiscordMessage(List<string> saccageLinkList, List<string> saccageTitresList, List<string> saccagesNamesList, List<string> filtredlist)
        {
            DiscordWebhook hook = new DiscordWebhook();
            hook.Url = WEBHOOK;
            DiscordMessage message = new DiscordMessage();
            message.Embeds = new List<DiscordEmbed>();
            for (int i = 0; i < Math.Min(filtredlist.Count(), 10); i++)
            {
                DiscordEmbed embed = new DiscordEmbed();
                embed.Title = saccageTitresList[i];
                string url = "https://imperial-mob.forumactif.com" + saccageLinkList[i];
                embed.Description = "Un nouveau saccage vient d’être posté par " + saccagesNamesList[i] + ", consulte le [ici](" + url + ")";
                embed.Url = url;
                embed.Image = new EmbedMedia() { Url = "https://i.servimg.com/u/f16/18/76/94/83/valide10.png", Width = 16, Height = 26 };
                embed.Timestamp = System.DateTime.Now;
                embed.Color = Color.Red;
                message.Embeds.Add(embed);
            }
            //message
            hook.Send(message);
        }

        private static async Task ExtractSaccageHTML(List<string> saccageLinkList, List<string> saccageTitresList, List<string> saccagesNamesList)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(saccage_url);

            var listeTopics = document.QuerySelectorAll(".sujets");


            foreach (var topic in listeTopics)
            {
                string habboName = topic.QuerySelector(".sujets_auteur strong").TextContent;
                string TitreSaccage = topic.QuerySelector(".topictitle2").TextContent;
                string SaccageUrl = topic.QuerySelector(".topictitle2").GetAttribute("href");
                saccageLinkList.Add(SaccageUrl);
                saccageTitresList.Add(TitreSaccage);
                saccagesNamesList.Add(habboName);

            }
        }
    }
}