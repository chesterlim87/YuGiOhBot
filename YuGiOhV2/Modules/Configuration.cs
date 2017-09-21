﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhV2.Objects;
using YuGiOhV2.Services;

namespace YuGiOhV2.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Configuration : CustomBase
    {

        private Database _db;
        private Setting _setting;

        public Configuration(Database db)
        {

            _db = db;

        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _setting = _db.Settings[Context.Guild.Id];
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Sets the prefix for the bot in this guild!")]
        public async Task SetPrefixCommand([Remainder]string prefix)
        {

            try
            {

                await _db.SetPrefix(Context.Guild.Id, prefix);
                await ReplyAsync($"Prefix has been set to `{prefix}`");

            }
            catch (Exception e)
            {

                AltConsole.Print("Error", "Prefix", "BAH GAWD SOMETHING WRONG WITH SETTING PREFIX", e);
                await ReplyAsync("There was an error in setting the prefix. Please report error with `y!feedback <message>`");

            }

        }

        [Command("prefix")]
        [RequireOwner]
        [Summary("Sets the prefix for the bot in this guild!")]
        public Task SetPrefixCommandOwner([Remainder]string prefix)
            => SetPrefixCommand(prefix);

        [Command("prefix")]
        [Summary("See the prefix the guild is using! Kinda useless tbh...")]
        public async Task PrefixCommand()
            => await ReplyAsync($"**Prefix:** {_setting.Prefix}");

        [Command("minimal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Set how much card info should be shown! (true/false)")]
        public async Task SetMinimalCommand([Remainder]string input)
        {

            if (bool.TryParse(input, out var minimal))
            {

                try
                {

                    await _db.SetMinimal(Context.Guild.Id, minimal);
                    await ReplyAsync($"Minimal has been set to `{minimal}`");

                }
                catch (Exception e)
                {

                    AltConsole.Print("Error", "Prefix", "BAH GAWD SOMETHING WRONG WITH SETTING MINIMAL", e);
                    await ReplyAsync("There was an error in setting the minimal setting. Please report error with `y!feedback <message>`");

                }

            }
            else
                await ReplyAsync("This command only accepts `true` or `false`");

        }

        [Command("minimal")]
        [RequireOwner]
        [Summary("Set how much card info should be shown! (true/false)")]
        public Task MinimalCommandOwner(string input)
            => SetMinimalCommand(input);

        [Command("minimal")]
        [Summary("Check how much card info is shown!")]
        public async Task MinimalCommand()
            => await ReplyAndDeleteAsync($"**Minimal:** {_setting.Minimal}");

    }
}
