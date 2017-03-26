﻿using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Discord.Addons.InteractiveCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuGiOhBot.Services;

namespace YuGiOhBot.Commands
{
    public class CoreCommands : InteractiveModuleBase
    {

        private YuGiOhServices _service;

        public CoreCommands(YuGiOhServices serviceParams)
        {

            _service = serviceParams;

        }

        [Command("card"), Alias("c")]
        [Summary("Returns information based on given card name")]
        public async Task CardCommand([Remainder]string cardName)
        {

            if (string.IsNullOrEmpty(cardName))
            {

                await ReplyAsync("There is no card with no name, unless it's the shadow realm. Everyone is just a soul there.");
                return;

            }

            EmbedBuilder eBuilder;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                YuGiOhCard card = await _service.GetCard(cardName);

                //i am putting the cache service here because the card name will not always be correct
                if (CacheService.YuGiOhCardCache.TryGetValue(card.Name, out eBuilder))
                {

                    await ReplyAsync("", embed: eBuilder);
                    typingState.Dispose();
                    return;

                }

                if (card.Name.Equals(string.Empty))
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    typingState.Dispose();
                    return;

                }

                var authorBuilder = new EmbedAuthorBuilder()
                {

                    Name = "YuGiOh",
                    IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
                    Url = "http://www.yugioh-card.com/en/"

                };

                var footerBuilder = new EmbedFooterBuilder()
                {

                    Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
                    IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

                };

                string lastType = card.Types.Last();
                var organizedDescription = new StringBuilder();
                organizedDescription.AppendLine($"\n**Format:** {card.Format}");

                if (!string.IsNullOrEmpty(card.CardType)) organizedDescription.AppendLine($"**Card Type:** {card.CardType}");
                if (!string.IsNullOrEmpty(card.Types.FirstOrDefault())) organizedDescription.AppendLine($"**Types:** {string.Join(" / ", card.Types)}");

                //if the card even has a level
                if (!string.IsNullOrEmpty(card.Level))
                {

                    if (card.Types.Contains("Xyz")) organizedDescription.AppendLine($"**Rank:** {card.Level}");
                    else organizedDescription.AppendLine($"**Level:** {card.Level}");

                }

                if (!string.IsNullOrEmpty(card.LeftPend))
                {

                    //for now only 1 value is needed because there are no cards with different pendulum
                    //values on both ends (for now of course, you never know)
                    organizedDescription.AppendLine($"**Pedulum Scale:** {card.LeftPend}");
                    //organizedDescription.AppendLine($"**Left Pedulum Scale:** {card.LeftPend}");
                    //organizedDescription.AppendLine($"**Right Pedulum Scale:** {card.RightPend}");

                }

                //if the card is not a spell or a trap
                if (!(card.CardType.Equals("Spell") || card.CardType.Equals("Trap")))
                {

                    organizedDescription.AppendLine($"**Attribute:** {card.Attribute}");
                    organizedDescription.AppendLine($"**Race:** {card.Race}");

                }

                eBuilder = new EmbedBuilder()
                {

                    Author = authorBuilder,
                    Color = WhatColorIsTheCard(card.Types, card.Name, card.CardType),
                    ImageUrl = card.ImageUrl,
                    //ThumbnailUrl = card.ImageUrl,
                    Title = card.Name,
                    Description = organizedDescription.ToString(),
                    Footer = footerBuilder

                };

                string description;

                if (card.Description.StartsWith("Pendulum Effect"))
                {

                    var tempArray = card.Description.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
                    description = "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Trim();

                }
                else description = card.Description;

                eBuilder.AddField(x =>
                {

                    x.Name = card.IsEffect ? "Effect" : "Description";
                    x.Value = description;
                    x.IsInline = false;

                });

                if (!(string.IsNullOrEmpty(card.Atk) || string.IsNullOrEmpty(card.Def)))
                {

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Attack";
                        x.Value = card.Atk;
                        x.IsInline = true;

                    });

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Defense";
                        x.Value = card.Def;
                        x.IsInline = true;

                    });

                }

                if (!string.IsNullOrEmpty(card.Archetype))
                {

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Archetype(s)";
                        x.Value = card.Archetype;
                        x.IsInline = false;

                    });

                }

                if (card.Prices.data != null)
                {

                    var organizedPrices = new StringBuilder();

                    //debug usage
                    //card.Prices.data.ForEach(d => Console.WriteLine(d.price_data.data.prices.average));
                    if (card.Prices.data == null)
                    {

                        organizedPrices.Append("**No prices to show.**");

                    }
                    else if (card.Prices.data.Count > 8)
                    {

                        List<Datum> prices = card.Prices.data;

                        organizedPrices.AppendLine("**Showing the first 7 prices due to too many available.**");

                        for (int counter = 0; counter < 8; counter++)
                        {

                            Datum data = prices[counter];

                            organizedPrices.AppendLine($"**Name:** {data.name}");
                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
                            //this is what redundancy looks like people, lmfao
                            organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
                            organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
                            organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

                        }

                    }
                    else if (card.Prices.data.Count < 8)
                    {

                        foreach (Datum data in card.Prices.data)
                        {

                            organizedPrices.AppendLine($"**Name:** {data.name}");
                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
                            //this is what redundancy looks like people, lmfao

                            if (data.price_data.data == null)
                            {

                                organizedPrices.AppendLine($"\t\tError: No prices to display for this card variant.");

                            }
                            else
                            {


                                organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
                                organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
                                organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

                            }

                        }

                    }

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Prices";
                        x.Value = organizedPrices.ToString();
                        x.IsInline = false;

                    });

                }

            }

            await ReplyAsync("", embed: eBuilder);
            CacheService.YuGiOhCardCache.TryAdd(cardName, eBuilder);

        }

        [Command("lcard"), Alias("lc")]
        [Summary("Word position does not matter and will pull the first available result in the search")]
        public async Task LazyCardCommand([Remainder]string cardName)
        {

            if (string.IsNullOrEmpty(cardName))
            {

                await ReplyAsync("There are no cards with a lack of name.");
                return;

            }

            if (CacheService.YuGiOhCardCache.TryGetValue(cardName, out EmbedBuilder eBuilder))
            {

                await ReplyAsync("", embed: eBuilder);
                return;

            }

            using (var typingState = Context.Channel.EnterTypingState())
            {

                YuGiOhCard card = await _service.LazyGetCard(cardName);

                if (card.Name.Equals(string.Empty))
                {

                    await ReplyAsync($"No card by the name {cardName} was found!");
                    typingState.Dispose();
                    return;

                }

                var authorBuilder = new EmbedAuthorBuilder()
                {

                    Name = "YuGiOh",
                    IconUrl = "http://card-masters.com/cardmasters/wp-content/uploads/2013/08/yugioh-product-icon-lg.png",
                    Url = "http://www.yugioh-card.com/en/"

                };

                var footerBuilder = new EmbedFooterBuilder()
                {

                    Text = "It's time to d-d-d-d-duel | Database made by chinhodado",
                    IconUrl = "http://i722.photobucket.com/albums/ww227/omar_alami/icon_gold_classic_zps66eae1c7.png"

                };

                string lastType = card.Types.Last();
                var organizedDescription = new StringBuilder();
                organizedDescription.AppendLine($"\n**Format:** {card.Format}");

                if (!string.IsNullOrEmpty(card.CardType)) organizedDescription.AppendLine($"**Card Type:** {card.CardType}");
                if (!string.IsNullOrEmpty(card.Types.FirstOrDefault())) organizedDescription.AppendLine($"**Types:** {string.Join(" / ", card.Types)}");

                //if the card even has a level
                if (!string.IsNullOrEmpty(card.Level))
                {

                    if (card.Types.Contains("Xyz")) organizedDescription.AppendLine($"**Rank:** {card.Level}");
                    else organizedDescription.AppendLine($"**Level:** {card.Level}");

                }

                if (!string.IsNullOrEmpty(card.LeftPend))
                {

                    //for now only 1 value is needed because there are no cards with different pendulum
                    //values on both ends (for now of course, you never know)
                    organizedDescription.AppendLine($"**Pedulum Scale:** {card.LeftPend}");
                    //organizedDescription.AppendLine($"**Left Pedulum Scale:** {card.LeftPend}");
                    //organizedDescription.AppendLine($"**Right Pedulum Scale:** {card.RightPend}");

                }

                //if the card is not a spell or a trap
                if (!(card.CardType.Equals("Spell") || card.CardType.Equals("Trap")))
                {

                    organizedDescription.AppendLine($"**Attribute:** {card.Attribute}");
                    organizedDescription.AppendLine($"**Race:** {card.Race}");

                }

                eBuilder = new EmbedBuilder()
                {

                    Author = authorBuilder,
                    Color = WhatColorIsTheCard(card.Types, card.Name, card.CardType),
                    ImageUrl = card.ImageUrl,
                    //ThumbnailUrl = card.ImageUrl,
                    Title = card.Name,
                    Description = organizedDescription.ToString(),
                    Footer = footerBuilder

                };

                string description;

                if (card.Description.StartsWith("Pendulum Effect"))
                {

                    var tempArray = card.Description.Split(new string[] { "Monster Effect" }, StringSplitOptions.None);
                    description = "__Pendulum Effect__\n" + tempArray[0].Replace("Pendulum Effect", "").Trim() + "\n__Monster Effect__\n" + tempArray[1].Replace("Monster Effect", "").Trim();

                }
                else description = card.Description;

                eBuilder.AddField(x =>
                {

                    x.Name = card.IsEffect ? "Effect" : "Description";
                    x.Value = description;
                    x.IsInline = false;

                });

                if (!(string.IsNullOrEmpty(card.Atk) || string.IsNullOrEmpty(card.Def)))
                {

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Attack";
                        x.Value = card.Atk;
                        x.IsInline = true;

                    });

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Defense";
                        x.Value = card.Def;
                        x.IsInline = true;

                    });

                }

                if (!string.IsNullOrEmpty(card.Archetype))
                {

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Archetype(s)";
                        x.Value = card.Archetype;
                        x.IsInline = false;

                    });

                }

                if (card.Prices.data != null)
                {

                    var organizedPrices = new StringBuilder();

                    //debug usage
                    //card.Prices.data.ForEach(d => Console.WriteLine(d.price_data.data.prices.average));
                    if (card.Prices.data == null)
                    {

                        organizedPrices.Append("**No prices to show.**");

                    }
                    else if (card.Prices.data.Count > 8)
                    {

                        List<Datum> prices = card.Prices.data;

                        organizedPrices.AppendLine("**Showing the first 7 prices due to too many available.**");

                        for (int counter = 0; counter < 8; counter++)
                        {

                            Datum data = prices[counter];

                            organizedPrices.AppendLine($"**Name:** {data.name}");
                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
                            //this is what redundancy looks like people, lmfao
                            organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
                            organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
                            organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

                        }

                    }
                    else if (card.Prices.data.Count < 8)
                    {

                        foreach (Datum data in card.Prices.data)
                        {

                            organizedPrices.AppendLine($"**Name:** {data.name}");
                            organizedPrices.AppendLine($"\t\tRarity: {data.rarity}");
                            //this is what redundancy looks like people, lmfao

                            if (data.price_data.data == null)
                            {

                                organizedPrices.AppendLine($"\t\tError: No prices to display for this card variant.");

                            }
                            else
                            {


                                organizedPrices.AppendLine($"\t\tHigh: ${data.price_data.data.prices.high.ToString("0.00")}");
                                organizedPrices.AppendLine($"\t\tLow: ${data.price_data.data.prices.low.ToString("0.00")}");
                                organizedPrices.AppendLine($"\t\tAverage: ${data.price_data.data.prices.average.ToString("0.00")}");

                            }

                        }

                    }

                    eBuilder.AddField(x =>
                    {

                        x.Name = "Prices";
                        x.Value = organizedPrices.ToString();
                        x.IsInline = false;

                    });

                }

            }

            await ReplyAsync("", embed: eBuilder);
            CacheService.YuGiOhCardCache.TryAdd(cardName, eBuilder);

        }

        [Command("search", RunMode = RunMode.Async), Alias("s")]
        [Summary("Searches for cards based on name given")]
        public async Task CardSearchCommand([Remainder]string search)
        {

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = await _service.SearchCards(search, false);

                if (searchResults.Count == 0)
                {

                    await ReplyAsync($"Nothing was found with the search of {search}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");
                typingState.Dispose();
            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }

            await CardCommand(searchResults[searchNumber - 1]);

        }

        [Command("lsearch", RunMode = RunMode.Async), Alias("ls")]
        [Summary("A lazy search of all cards that CONTAIN the words entered, it may be not be in any particular order")]
        public async Task LazySearchCommand([Remainder]string search)
        {

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState = Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = await _service.LazySearchCards(search);

                if (searchResults.Count == 0)
                {

                    await ReplyAsync($"Nothing was found with the search of {search}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }

            await CardCommand(searchResults[searchNumber - 1]);

        }

        [Command("archetype", RunMode = RunMode.Async), Alias("a", "arch")]
        [Summary("Attempt to search all cards associated with searched archetype")]
        public async Task ArchetypeSearchCommand([Remainder]string archetype)
        {

            StringBuilder organizedResults;
            List<string> searchResults;

            using (var typingState =  Context.Channel.EnterTypingState())
            {

                //<card names>
                searchResults = await _service.SearchCards(archetype, true);

                if (searchResults.Count == 0)
                {

                    await ReplyAsync($"Nothing was found with the search of {archetype}!");
                    typingState.Dispose();
                    return;

                }
                else if (searchResults.Count > 50)
                {

                    await ReplyAsync($"Too many results were returned, please refine your search!");
                    typingState.Dispose();
                    return;

                }

                var str = $"```There are {searchResults.Count} results based on your search!\n\n";
                organizedResults = new StringBuilder(str);
                var counter = 1;

                foreach (string card in searchResults)
                {

                    organizedResults.AppendLine($"{counter}. {card}");
                    counter++;

                }

                organizedResults.Append("\nHit a number to look at that card. Expires in a minute!```");

            }

            await ReplyAsync(organizedResults.ToString());

            IUserMessage response = await WaitForMessage(Context.User, Context.Channel, TimeSpan.FromSeconds(60));

            if (!int.TryParse(response.Content, out int searchNumber)) return;
            else if (searchNumber > searchResults.Count)
            {

                await ReplyAsync($"{Context.User.Mention} Not a valid search!");
                return;

            }

            await CardCommand(searchResults[searchNumber - 1]);

        }

        private Color WhatColorIsTheCard(List<string> cardTypes, string cardName, string cardType)
        {

            if (cardName.Equals("Slifer the Sky Dragon")) return new Color(255, 0, 0);
            else if (cardName.Equals("The Winged Dragon of Ra")) return new Color(255, 215, 0);
            else if (cardName.Equals("Obelisk the Tormentor")) return new Color(50, 50, 153);
            else if (cardTypes.Contains("Pendulum")) return new Color(175, 219, 205);
            else if (cardType.Equals("Spell")) return new Color(29, 158, 116);
            else if (cardType.Equals("Trap")) return new Color(188, 90, 132);
            else if (cardTypes.Contains("XYZ")) return new Color(0, 0, 0);
            else if (cardTypes.Contains("Token")) return new Color(192, 192, 192);
            else if (cardTypes.Contains("Synchro")) return new Color(204, 204, 204);
            else if (cardTypes.Contains("Fusion")) return new Color(160, 134, 183);
            else if (cardTypes.Contains("Ritual")) return new Color(157, 181, 204);
            else if (cardTypes.Contains("Effect")) return new Color(174, 121, 66);
            else return new Color(216, 171, 12);

        }

    }
}
