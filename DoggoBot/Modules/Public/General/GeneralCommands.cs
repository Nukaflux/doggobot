﻿using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Newtonsoft.Json.Linq;

using Tweetinvi;

using DoggoBot.Core.Models.Module;
using DoggoBot.Core.Common.Colors;
using DoggoBot.Core.Configuration.Bot;

namespace DoggoBot.Modules.Public.General
{
    [Name("General")]
    [Summary("Contains random and general commands for users")]
    public class GeneralCommands : DoggoModuleBase
    {
        private readonly DiscordSocketClient borkClient;
        private readonly BotConfiguration borkConfig;

        private readonly Colors ourColors = new Colors();

        public GeneralCommands(DiscordSocketClient client, BotConfiguration config)
        {
            borkClient = client;
            borkConfig = config;
        }

        [Command("stats")]
        [Summary("Display statistics and information about DoggoBot")]
        [Remarks("stats")]
        public async Task StatsAsync()
        {
            String commits;

            HttpClient ourHttp = new HttpClient();
            ourHttp.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            using (var get = await ourHttp.GetAsync("https://api.github.com/repos/DigitalDoggoProductions/doggobot/commits"))
                if (!get.IsSuccessStatusCode)
                    commits = "Error Receiving Latest Changes :x:";
                else
                {
                    dynamic response = JArray.Parse(await get.Content.ReadAsStringAsync()); 
                    commits =
                        $"[{((string)response[0].sha).Substring(0, 7)}]({response[0].html_url}) - {response[0].commit.message}\n" +
                        $"[{((string)response[1].sha).Substring(0, 7)}]({response[1].html_url}) - {response[1].commit.message}\n" +
                        $"[{((string)response[2].sha).Substring(0, 7)}]({response[2].html_url}) - {response[2].commit.message}\n" +
                        $"[{((string)response[3].sha).Substring(0, 7)}]({response[3].html_url}) - {response[3].commit.message}";

                    get.Dispose();
                }

            ourHttp.Dispose();

            await ReplyEmbed(new EmbedBuilder()
                .WithThumbnailUrl(borkClient.CurrentUser.GetAvatarUrl())
                .WithFooter(x => { x.Text = "Created by: Doggo~ ❤#3899"; })
                .WithColor(ourColors.EmbedMint)
                .AddField(x => { x.Name = "Latest Changes"; x.Value = commits; x.IsInline = true; })
                .AddField(x => { x.Name = "Important Links"; x.Value = "• [Github Page](https://github.com/DigitalDoggoProductions/doggobot)\n• [DoggoBot Support Server](https://discord.gg/DwDzPBr)\n• [Twitter Updates](https://twitter.com/tweetdoggobot)\n• [Digital Doggo Productions](https://github.com/DigitalDoggoProductions)"; x.IsInline = true; })
                .AddField(x => { x.Name = "Bot Info"; x.Value = $"• **Version:** {Assembly.GetEntryAssembly().GetName().Version}\n• **Total Guilds:** {borkClient.Guilds.Count}\n• **Total Channels:** {borkClient.Guilds.Sum(c => c.Channels.Count)}\n• **Total Users:** {borkClient.Guilds.Sum(u => u.Users.Where(b => !b.IsBot).Count())}"; x.IsInline = true; })
                .AddField(x => { x.Name = "Process Info"; x.Value = $"• **Library:** {DiscordConfig.Version}\n• **Runtime:** {RuntimeInformation.FrameworkDescription}\n• **Uptime:** {(DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss")}\n• **Heap Size:** {Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString()}Mb"; x.IsInline = true; }).Build());
        }

        [Command("invite")]
        [Summary("Receive the invite link for DoggoBot")]
        [Remarks("invite")]
        public async Task InviteAsync()
            => await ReplyEmbed(new EmbedBuilder().WithColor(ourColors.EmbedBlue).WithDescription("**Invite me to your server!**\n\n[Invite Link](https://discordapp.com/oauth2/authorize?client_id=" + (await borkClient.GetApplicationInfoAsync()).Id + "&scope=bot&permissions=" + borkConfig.Load().BotPermissions + ")").Build());

        [Command("ping")]
        [Alias("bork")]
        [Summary("Show the bots connection latency")]
        [Remarks("ping")]
        public async Task PingAsync()
            => await ReplyAsync($"**Bork! - {borkClient.Latency}ms**");

        [Command("tweet")]
        [Alias("yeet")]
        [Summary("Send a tweet to the DoggoBot twitter saying 'yeet'")]
        [Remarks("tweet")]
        public async Task SendTweetAsync()
        {
            borkConfig.AddTweetToCount();
            var Tweet = await TweetAsync.PublishTweet($"{borkConfig.Load().TweetCount}. Yeet (by: {Context.Message.Author})");

            await ReplyEmbed(new EmbedBuilder()
                .WithColor(ourColors.RandomColor())
                .WithDescription($"You just made me tweet 'Yeet'\n\n[View your Tweet]({Tweet.Url})")
                .Build());
        }
    }
}
