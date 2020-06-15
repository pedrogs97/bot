
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotD.Core.Data;
using BotD.Core.Models;
using BotD.Core.Models.BotAccount;
using BotD.Core.Models.Bungie;
using BotD.Core.Models.Bungie.Activities;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using QuickType;

namespace BotD.Core.Commands
{
    public class CMD : ModuleBase<SocketCommandContext>
    {
        [Command("registrar"), Alias("reg", "registro","registra","cadastro","cadastar", "cadastra", "cad"), Summary("Registrar no Bot")]
        public async Task Registrar([Remainder]string displayName)
        {
            await Global.semaphoreSlim.WaitAsync();
            try
            {
                if (Global.GetAccount(Context.User) != null | Global.GetAccount(displayName) != null)
                {

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                    embed.WithColor(180, 0, 0);
                    embed.WithTitle($"Nós te Conhecemos, Guardião!");
                    embed.WithDescription($"{ Context.User.Username}, você **já está registrado**. \n Tem certeza que era isso que queria me dizer?");
                    await Context.Channel.SendMessageAsync("!!!", false, embed.Build());
                    return;

                }
                var user = Context.User as SocketGuildUser;



                using (var client = new HttpClient())
                {
                    string content;
                    HttpResponseMessage response = new HttpResponseMessage();
                    try
                    {
                        client.DefaultRequestHeaders.Add("X-API-Key", "6b08e62c804f489eb1f6ec8a11b9a79f");
                        response = await client.GetAsync("https://www.bungie.net/Platform/Destiny2/SearchDestinyPlayer/1/" + displayName + "/");
                        content = await response.Content.ReadAsStringAsync();
                        if (content.Contains("StatusCode: 500"))
                        {
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"Desculpe, {Context.User.Username} ", Context.User.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://tenor.com/view/destiny-gif-6188486");
                            embed3.WithTitle($"Estamos a ermo no Vácuo!");
                            embed3.WithDescription($"Desculpe, { Context.User.Username},mas Parece que os Serviços do Destiny estão **fora do ar**");
                            await Context.Channel.SendMessageAsync("!!!", false, embed3.Build());
                            return;
                        }
                        //content = JsonPrettyPrint(content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: FIRST | Erro: Algo");
                        Console.WriteLine(e);
                        return;
                    }
                    Responses resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                    Member player = resp.Response.FirstOrDefault();
                    if (player == null)
                    {
                        EmbedBuilder embed2 = new EmbedBuilder();
                        embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                        embed2.WithColor(40, 200, 150);
                        embed2.WithTitle($"**Não te Encontramos!**");
                        embed2.WithDescription($"{ Context.User.Username},Verifique se digitiou a **GamerTag** correta no comando register!");
                        await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                        return;
                    }
                    long bId = player.membershipId;
                    ulong dId = Context.User.Id;
                    Account currentAcc = Global.CreateAccount(displayName, bId, dId);
                    currentAcc.cargo = GetUserRole(user);
                    Regex rg;
                    try
                    {
                        response = await client.GetAsync($"https://bungie.net/Platform/Destiny2/1/Account/" + bId + "/Stats/");
                        content = await response.Content.ReadAsStringAsync();
                        rg = new Regex(Regex.Escape("Response"));
                        content = rg.Replace(content, "Characters", 1);
                        if (content.Contains("StatusCode: 500"))
                        {
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"Desculpe, {Context.User.Username} ", Context.User.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://assets.bigcartel.com/product_images/232119707/1549993729361_Untitled-Artwork.jpg?auto=format&fit=max&h=1000&w=1000");
                            embed3.WithTitle($"Estamos a ermo no Vácuo!");
                            embed3.WithDescription($"Desculpe, { Context.User.Username},mas Parece que os Serviços do Destiny estão **fora do ar**");
                            await Context.Channel.SendMessageAsync("!!!", false, embed3.Build());
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: Stats | Erro: Algo");
                        Console.WriteLine(e);
                        return;
                    }
                    Responses resp2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                    List<CharMerged> chars = resp2.Characters.characters;
                    foreach (CharMerged c in chars)
                    {
                        currentAcc.Characterid.Add(c.characterid);
                    }//Pontuação aqui





                    try
                    {

                        response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Profile/" + bId + "/" + "?components=" + "Profiles,Characters,Records");
                        content = await response.Content.ReadAsStringAsync();
                        rg = new Regex(Regex.Escape("Response"));
                        content = rg.Replace(content, "Profile", 1);
                        //content = JsonPrettyPrint(content);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: COmponents TRiunfos {currentAcc.DisplayName} | Erro: Algo");
                        Console.WriteLine(e);
                        return;
                    }
                    List<uint> teste = new List<uint>();
                    List<uint> fora = new List<uint>();
                    resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                    Dictionary<Int64, Char_Component> datanow = resp.Profile.characters.data;
                    Dictionary<UInt32, RecordComponent> listaTriunfo = resp.Profile.profileRecords.data.records;
                    currentAcc.PontosdeTriunfos = resp.Profile.profileRecords.data.score;

                    for (int n = 0; n < currentAcc.TriunfosConcluídos.Count; n++)
                    {

                        uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(n);
                        if (listaTriunfo.ContainsKey(key))
                        {
                            teste.Add(key);
                            if (Global.AllCompleted(listaTriunfo[key].objectives)) currentAcc.TriunfosConcluídos[key] = true;
                        }
                        else
                        {
                            fora.Add(key);
                        }
                    }


                    


                    List<Atividades> atvs = new List<Atividades>();
                    int xpChar = 0;
                    Responses respon;
                    string content2;
                    Welcome respon2;
                  
                    foreach (long characterid in currentAcc.Characterid)
                    {
                        try
                        {
                            response = await client.GetAsync("https://bungie.net/platform/destiny2/1/profile/" + currentAcc.BungieID + "/character/" + characterid + "/" + "?components=" + " records");
                            content2 = await response.Content.ReadAsStringAsync();
                            rg = new Regex(Regex.Escape("Response"));
                            content2 = rg.Replace(content2, "CharRecord", 1);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{DateTime.Now} at commands] algo deu errado ao executar o comando. texto: testechar records {currentAcc.DisplayName} | erro: algo");
                            Console.WriteLine(e);
                            return;
                        }
                        try
                        {
                            respon2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Welcome>(content2);

                        }
                        catch (Exception s)
                        {
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"desculpe");
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://assets.bigcartel.com/product_images/232119707/1549993729361_untitled-artwork.jpg?auto=format&fit=max&h=1000&w=1000");
                            embed3.WithTitle($"estamos a ermo no vácuo!");
                            embed3.WithDescription($"desculpe, mas parece que algo deu errado com  nossa conexão");
                            foreach (ISocketMessageChannel g in Global.Guilds)
                            {
                                await g.SendMessageAsync("!!!", false, embed3.Build());
                            }
                            return;
                        }
                        Dictionary<long, Record> listaTriunfoCHAR;

                        if (respon2.CharRecord.Records.Data != null)
                        {
                            //    EmbedBuilder embed3 = new EmbedBuilder();
                            //    embed3.WithAuthor($"desculpe");
                            //    embed3.WithColor(200, 40, 0);
                            //    embed3.WithImageUrl("https://i.etsystatic.com/16340822/r/il/d1e421/1590880703/il_794xN.1590880703_52qw.jpg");
                            //    embed3.WithTitle($"{currentAcc.DisplayName}, Não posso pegar seus dados");
                            //    embed3.WithDescription($"{currentAcc.DisplayName}, Suas Preferências de Privacidae não me permitem obter dados, vá no site da bungie parar habilitá-los  https://www.bungie.net/en/Profile/Settings/254/16074013?category=Privacy");
                            //    foreach (ISocketMessageChannel g in Global.Guilds)
                            //    {
                            //        await g.SendMessageAsync("", false, embed3.Build());
                            //    }
                            //    break;

                            listaTriunfoCHAR = respon2.CharRecord.Records.Data.Records;//resp.Profile.profileRecords.data.records;




                            for (int n = 0; n < currentAcc.TriunfosConcluídos.Count; n++)
                            {

                                uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(n);
                                if (listaTriunfoCHAR.ContainsKey(key))
                                {
                                    teste.Add(key);
                                    if (Global.AllCompleted(listaTriunfoCHAR[key].Objectives)) currentAcc.TriunfosConcluídos[key] = true;
                                }
                                else
                                {
                                    fora.Add(key);
                                }
                            }

                        }
                        try
                        {
                            response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Account/" + bId + "/Character/" + characterid + "/Stats/?page=0,mode=raid,count=250");
                            content2 = await response.Content.ReadAsStringAsync();
                            rg = new Regex(Regex.Escape("Response"));
                            content2 = rg.Replace(content2, "History", 1);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: RAIDS {currentAcc.DisplayName} | Erro: Algo");
                            Console.WriteLine(e);
                            return;
                        }
                        respon = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content2);



                        //PVP Score
                        HistoryChar hc;
                        if (!respon.History.TryGetValue("allPvP", out hc)) return;
                        if (hc.allTime != null)
                        {
                            for (int n = 0; n < currentAcc.Pvp.Keys.Count; n++)
                            {
                                string key = currentAcc.Pvp.Keys.ElementAt(n);
                                currentAcc.Pvp[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                //xpChar += Convert.ToInt32(currentAcc.Pvp[key] * AllAccounts.PesoPvp[key]);
                            }
                        }
                        ////Raids
                        if (!respon.History.TryGetValue("raid", out hc)) return;
                        if (hc.allTime != null)
                        {
                            for (int n = 0; n < currentAcc.Raid.Keys.Count; n++)
                            {
                                string key = currentAcc.Raid.Keys.ElementAt(n);
                                currentAcc.Raid[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                //xpChar += Convert.ToInt32(currentAcc.Raid[key] * AllAccounts.PesoRaid[key]);
                            }
                        }
                        ////Gambit
                        if (!(respon.History.TryGetValue("allPvECompetitive", out hc))) return;
                        if (hc.allTime != null)
                        {
                            for (int n = 0; n < currentAcc.Gambit.Keys.Count; n++)
                            {
                                string key = currentAcc.Gambit.Keys.ElementAt(n);
                                currentAcc.Gambit[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                //xpChar += Convert.ToInt32(currentAcc.Gambit[key] * AllAccounts.PesoGambit[key]);
                            }
                        }
                        ////PVE
                        if (!respon.History.TryGetValue("allPvE", out hc)) return;
                        if (hc.allTime != null)
                        {
                            for (int n = 0; n < currentAcc.AllPve.Keys.Count; n++)
                            {
                                string key = currentAcc.AllPve.Keys.ElementAt(n);
                                currentAcc.AllPve[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                // xpChar += Convert.ToInt32(currentAcc.AllPve[key] * AllAccounts.PesoAllPve[key]);
                            }
                        }
                        ////Strikes
                        if (!respon.History.TryGetValue("allStrikes", out hc)) return;
                        if (hc.allTime != null)
                        {
                            for (int n = 0; n < currentAcc.Assaltos.Keys.Count; n++)
                            {
                                string key = currentAcc.Assaltos.Keys.ElementAt(n);
                                if (key.Equals("fastestCompletionMs"))
                                {
                                    if (hc.allTime[key].basic.value > 0) currentAcc.Assaltos[key] = Math.Round(Math.Abs(10 - hc.allTime[key].basic.value / 60000), 2);
                                    else currentAcc.Assaltos[key] = 0;
                                }
                                else
                                {
                                    currentAcc.Assaltos[key] += hc.allTime[key].basic.value;
                                }
                                //xpChar += Convert.ToInt32(currentAcc.Assaltos[key] * AllAccounts.PesoAssaltos[key]);
                            }
                        }


                        try
                        {
                            response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Account/" + bId + "/Character/" + characterid + "/Stats/AggregateActivityStats/");
                            content = await response.Content.ReadAsStringAsync();
                            rg = new Regex(Regex.Escape("Response"));
                            content = rg.Replace(content, "Atvs", 1);
                            resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: ATVS {currentAcc.DisplayName} | Erro: Algo");
                            Console.WriteLine(e);
                            return;
                        }
                        for (int i = 0; i < resp.Atvs.activities.Count; i++)
                        {
                            Atividades act = resp.Atvs.activities.ElementAt<Atividades>(i);
                            if (act.activityHash.Equals(2693136600) | act.activityHash.Equals(2693136601) | act.activityHash.Equals(2693136602) | act.activityHash.Equals(2693136603) | act.activityHash.Equals(2693136604) | act.activityHash.Equals(2693136605))
                            {
                                currentAcc.conclusões[2693136600] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                if (act.activityHash == 2693136605)
                                {
                                    //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[2693136600] * AllAccounts.pesos[2693136600]);
                                }
                            }
                            else if (act.activityHash.Equals(417231112) | act.activityHash.Equals(1685065161) | act.activityHash.Equals(2449714930) | act.activityHash.Equals(3446541099) | act.activityHash.Equals(3879860661) | act.activityHash.Equals(757116822))
                            {
                                currentAcc.conclusões[417231112] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                if (act.activityHash == 757116822)
                                {
                                    //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[417231112] * AllAccounts.pesos[417231112]);
                                }
                            }
                            else if (Global.pesos.ContainsKey(act.activityHash))
                            {
                                currentAcc.conclusões[act.activityHash] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[act.activityHash] * AllAccounts.pesos[act.activityHash]);
                            }
                            if (act.activityHash.Equals(709854835))
                            {
                                if (act.values["activityCompletions"].basic.value > 0)
                                {
                                    currentAcc.TriunfosConcluídos[709854835] = true;
                                    xpChar += Convert.ToInt32(Global.pesoTriunfos[act.activityHash]);
                                }
                            }
                        }


                    }
                    for (int n = 0; n < currentAcc.conclusões.Keys.Count; n++)
                    {
                        long key = currentAcc.conclusões.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.conclusões[key] * Global.pesos[key]);
                    }
                    for (int n = 0; n < currentAcc.Pvp.Keys.Count; n++)
                    {
                        string key = currentAcc.Pvp.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.Pvp[key] * Global.PesoPvp[key]);
                    }
                    for (int n = 0; n < currentAcc.Raid.Keys.Count; n++)
                    {
                        string key = currentAcc.Raid.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.Raid[key] * Global.PesoRaid[key]);
                    }
                    for (int n = 0; n < currentAcc.Gambit.Keys.Count; n++)
                    {
                        string key = currentAcc.Gambit.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.Gambit[key] * Global.PesoGambit[key]);
                    }
                    for (int n = 0; n < currentAcc.AllPve.Keys.Count; n++)
                    {
                        string key = currentAcc.AllPve.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.AllPve[key] * Global.PesoAllPve[key]);
                    }
                    for (int n = 0; n < currentAcc.Assaltos.Keys.Count; n++)
                    {
                        string key = currentAcc.Assaltos.Keys.ElementAt(n);
                        xpChar += Convert.ToInt32(currentAcc.Assaltos[key] * Global.PesoAssaltos[key]);
                    }
                    for (int m = 0; m < currentAcc.TriunfosConcluídos.Keys.Count; m++)
                    {
                        uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(m);

                        if (currentAcc.TriunfosConcluídos[key] && !key.Equals(709854835))
                        {
                            currentAcc.XP += Convert.ToInt32(Global.pesoTriunfos[key]);
                        }
                    }
                    currentAcc.XP += xpChar;

                    Global.SaveAccounts();



                    var Trying = Context.Guild.GetUser(currentAcc.DiscordID);
                    try
                    {
                        await Trying.ModifyAsync(usera => usera.Nickname = displayName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e}");
                    }

                    Global.SaveAccounts();

                    if (currentAcc.XP > 50)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Patrulheiro 💠");
                            await a.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "💠";
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }

                    }
                    if (currentAcc.XP > 150)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Vanguardista 🏵️");
                            await a.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🏵️";
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }

                    }
                    if (currentAcc.XP > 250)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Escolhido 💮");
                            await Trying.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "💮";
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"PARABÉNS {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTG2GhjBGDQPQ3ouXxEM4WhbU9OXMG3XmTePRn_mbz6JUEKr7qYRA");
                            embed3.WithTitle($"VOCÊ É UM DOS ESCOLHIDOS DO VIAJANTE");
                            embed3.WithDescription($"O guardião <@!" + Trying.Id + "> Acabou de se Tornar Escolhido.\n Eu não paro de impressionar com você...");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }


                    }
                    if (currentAcc.XP > 350)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Sombra do Viajante 🌓");
                            await Trying.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🌓";
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"Vejam! {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/9481bf02-bc14-40b7-be50-82b5bd1565a1/dcut1o6-1bb31358-8dcc-4dbf-af91-94f9d7c198a0.jpg/v1/fill/w_765,h_1044,q_70,strp/destiny_2_warlock_by_brianmoncus_dcut1o6-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTM5NyIsInBhdGgiOiJcL2ZcLzk0ODFiZjAyLWJjMTQtNDBiNy1iZTUwLTgyYjViZDE1NjVhMVwvZGN1dDFvNi0xYmIzMTM1OC04ZGNjLTRkYmYtYWY5MS05NGY5ZDdjMTk4YTAuanBnIiwid2lkdGgiOiI8PTEwMjQifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.rX2qVEP45V-ywJnm48V1d0KcshMxMrrOcH8fRo-Eb5k");
                            embed3.WithTitle($"VOCÊ É A SOMBRA DO VIAJANTE ");
                            embed3.WithDescription($"A mais nova Sombra, <@!" + Trying.Id + "> Quanto mais forte a Luz, mais forte será sua Sombra... e Nossa Luz é imbatível, guardiã(o).");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());
                            //
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }


                    }
                    if (currentAcc.XP > 450)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Resplendor da Luz ✨");
                            await Trying.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "✨";
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"HEY {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://www.gamersdecide.com/sites/default/files/authors/u145693/destiny2-sunbreaker.jpg");
                            embed3.WithTitle($"VOCÊ É O RESPLENDOR DA LUZ");
                            embed3.WithDescription($"O guardião <@!" + Trying.Id + "> Mostra sua soberania, e se conssagra como um Resplendor da Luz.\n");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }


                    }
                    if (currentAcc.XP > 550)
                    {
                        var guild = Global.socketGuilds[0];
                        var a = guild.GetUser(currentAcc.DiscordID);
                        try
                        {
                            var role = guild.Roles.First(r => r.Name == "Arauto do Destino 🌀");
                            await Trying.AddRoleAsync(role);
                            if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🌀";
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"{currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/2c9397a7-eaff-49a3-bff2-5496244eaa7f/d8crbgu-3a896568-74dd-48fb-a731-d84fc74421ee.jpg/v1/fill/w_894,h_894,q_70,strp/ascension_by_theartofsaul_d8crbgu-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTIwMCIsInBhdGgiOiJcL2ZcLzJjOTM5N2E3LWVhZmYtNDlhMy1iZmYyLTU0OTYyNDRlYWE3ZlwvZDhjcmJndS0zYTg5NjU2OC03NGRkLTQ4ZmItYTczMS1kODRmYzc0NDIxZWUuanBnIiwid2lkdGgiOiI8PTEyMDAifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.vKqwYAoAIyERd8gQMTgNAxgr8FgR5eBe_YXEjQKK4B8");//https://i.etsystatic.com/16340822/r/il/0734f0/1350635702/il_794xN.1350635702_e0cz.jpg
                            embed3.WithTitle($"Você é um Arauto do Destino");
                            embed3.WithDescription($"O Destino é inevitável <@!" + Trying.Id + "> Tal qual aquele que precede sua chegada... Tenho medo de onde pode chegar");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{e}");
                        }


                    }
                    Global.SaveAccounts();

                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithThumbnailUrl("https://pbs.twimg.com/media/Ds-2_JEWwAEpKd3.png");
                    embed.WithAuthor($"Seus dados foram registrados, {Context.User.Username}!", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    embed.WithDescription($"Você pode usar os seguintes comandos:\n\n $triunfos - Veja seus feitos triunfantes no destiny 2\n" +
                        $"$xp - Veja seus pontos no clã!\n" +
                        $"$raids - Ver suas raids feitas" +
                        $"$novagrade - Criar nova grade");
                    await Context.User.SendMessageAsync("Pronto!", false, embed.Build());


                }
            }
            finally
            {
                Global.semaphoreSlim.Release();
            }
        }


        [Command("grades"), Alias("listagrades", "grade"), Summary("Vê grades")]
        public async Task gradesnalista()
        {
            if (Global.Grades.Count < 1)
            {
                await Context.Channel.SendMessageAsync("Não há grades! Você pode criar uma com o comando $novagrade HH:MM", false,null);

            }
            for (int i =0; i < Global.Grades.Count; i++)
            {
                if (Global.Grades[i].stat == Status.aberta)
                {
                    RestUserMessage x = await Context.Channel.SendMessageAsync("\n Pressione AQUI para poder Reagir \t\n", false, Global.EmbedGrade(Global.Grades[i]).Build());
                    Global.MsgGrade.Remove(Global.Grades[i].Msg);
                    //ATENÇÂO REPENSAR

                    Global.MsgGrade.Add(x.Id, Global.Grades[i]);
                    Global.Grades[i].Msg = x.Id;
                }
            }
        }


        [Command("lista"), Alias("list", "lis","listar"), Summary("Todos os membros e seus respectivos leveis")]
        public async Task listarUsers()
        {
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;
            Account currentAcc = select.FirstOrDefault();
            if (currentAcc == null) return;
            if (currentAcc.cargo != "🐲" && currentAcc.cargo != "🌟" && currentAcc.cargo != "🌐") return;
            string players ="\n";
            foreach(Account a in Global.AccountList)
            {
                players += $"**{a.DisplayName}** ---- {a.XP} ---- {a.cargo}\n";
            }
            EmbedBuilder embed2 = new EmbedBuilder();
          
            embed2.WithColor(40, 200, 150);
            embed2.WithTitle($"São esses meus registros");
            embed2.WithDescription(players);
            await Context.User.SendMessageAsync("", false, embed2.Build());
        }

        [Command("shit"), Alias("sh", "sith", "siht"), Summary("critica ao user por sieg393")]
        public async Task Sieg(IGuildUser user)
        {
            Context.Message.MentionedUsers.First();
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;

            var selecti = from a in Global.AccountList
                          where a.DiscordID == Context.Message.MentionedUsers.First().Id
                          select a;

            Account currentAcc = select.FirstOrDefault();
            Account Infouser = selecti.FirstOrDefault();
            if (currentAcc == null) return;
            if (Infouser == null) return;
            if (currentAcc.DiscordID != 247810854580518912)
            {
                if (currentAcc.DiscordID != 406636327119880192) return;
            }
           
            string players = "\n";
            EmbedBuilder embed2 = new EmbedBuilder();
            embed2.WithImageUrl("https://i.pinimg.com/originals/4b/5a/22/4b5a22cfb3c4881849837108f95f7d70.jpg");
            embed2.WithColor(40, 200, 150);
            embed2.WithTitle($"Cale-se ,{Infouser.DisplayName}, Raideiro imundo");
            embed2.WithDescription(players);
            await Context.Channel.SendMessageAsync("", false, embed2.Build());

        }
        [Command("Delete"), Alias("del", "deletar", "remove", "rmv"), Summary("Deleta user da base de dados")]
        public async Task Del (IGuildUser user)
        {
            await Global.semaphoreSlim.WaitAsync();
            try
            {
                Context.Message.MentionedUsers.First();
                ulong dId = Context.User.Id;
                var select = from a in Global.AccountList
                             where a.DiscordID == dId
                             select a;

                var selecti = from a in Global.AccountList
                              where a.DiscordID == Context.Message.MentionedUsers.First().Id
                              select a;

                Account currentAcc = select.FirstOrDefault();
                Account Infouser = selecti.FirstOrDefault();
                if (currentAcc == null) return;
                if (selecti == null) await Context.Channel.SendMessageAsync("Não encontrei essa pessoa em meus registros", false, null);

                if (currentAcc.cargo != "🐲" && currentAcc.cargo != "🌟") return;

                Global.AccountList.Remove(Infouser);
                Global.SaveAccounts();

                await Context.Channel.SendMessageAsync($"Player excluído, até uma próxima {Infouser.DisplayName} ", false, null);

            }
            finally
            {
                Global.semaphoreSlim.Release();
            }

        }
        [Command("Deletebygt"), Alias("delgt", "deletargt", "removegt", "rmvgt"), Summary("Deleta user da base de dados por gt")]
        public async Task Del2(string user)
        {
            await Global.semaphoreSlim.WaitAsync();
            try
            {
                Context.Message.MentionedUsers.First();
                ulong dId = Context.User.Id;
                var select = from a in Global.AccountList
                             where a.DiscordID == dId
                             select a;

                var selecti = from a in Global.AccountList
                              where a.DisplayName == user
                              select a;

                Account currentAcc = select.FirstOrDefault();
                Account Infouser = selecti.FirstOrDefault();
                if (currentAcc == null) return;
                if (selecti == null) await Context.Channel.SendMessageAsync("Não encontrei essa pessoa em meus registros", false, null);

                if (currentAcc.cargo != "🐲" && currentAcc.cargo != "🌟") return;

                Global.AccountList.Remove(Infouser);
                Global.SaveAccounts();

                await Context.Channel.SendMessageAsync($"Player excluído, até uma próxima {Infouser.DisplayName} ", false, null);

            }
            finally
            {
                Global.semaphoreSlim.Release();
            }

        }

        [Command("userinfo"), Alias("ui", "useri", "usrinfo"), Summary("informação sobre user")]
        public async Task Userinfo(IGuildUser user)
        {
            Context.Message.MentionedUsers.First();
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;

            var selecti = from a in Global.AccountList
                          where a.DiscordID == Context.Message.MentionedUsers.First().Id
                          select a;

            Account currentAcc = select.FirstOrDefault();
            Account Infouser = selecti.FirstOrDefault();
            if (currentAcc == null) return;
            if (currentAcc.cargo != "🐲" && currentAcc.cargo != "🌟" && currentAcc.cargo != "🌐") return;
            string players = "\n";
            
                players += $"**{Infouser.DisplayName}** ---- {Infouser.XP} ---- {Infouser.cargo}\n";
            players += "**\nRAIDS:\n**";
            for (int j = 0; j < Infouser.conclusões.Count; j++)
            {
                players += $"\n **{Global.ActNames[Infouser.conclusões.ElementAt(j).Key]}**  = {Infouser.conclusões.ElementAt(j).Value}";

            }
            players += "**\n\nTRIUNFOS:\n**";
            for (int j = 0; j < Infouser.TriunfosConcluídos.Count; j++)
            {
                uint key = Infouser.TriunfosConcluídos.Keys.ElementAt(j);
                if (Infouser.TriunfosConcluídos[key])
                    players += $"\n **{Global.nomeTriunfos[Infouser.TriunfosConcluídos.ElementAt(j).Key]}**";

            }
            players+= $"\n E ele tem **{Infouser.PontosdeTriunfos}** pontos de triunfo";
            EmbedBuilder embed2 = new EmbedBuilder();

            embed2.WithColor(40, 200, 150);
            embed2.WithTitle($"São esses meus registros");
            embed2.WithDescription(players);
            await Context.User.SendMessageAsync("", false, embed2.Build());
        }

        [Command("xp"), Alias("Xp", "XP"), Summary("Seu XP")]
        public async Task Xp()
        {

            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;
            Account currentAcc = select.FirstOrDefault();
            if (currentAcc == null)
            {

                EmbedBuilder embed2 = new EmbedBuilder();
                embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                embed2.WithColor(40, 200, 150);
                embed2.WithTitle($"Registre seus Dados, Guardião!");
                embed2.WithDescription($"Desculpe, { Context.User.Username},mas você não consta nos meus registros. Para se cadastrar digite o código :  ```\nregistrar (Sua Gamertag Aqui!)```");
                await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithAuthor($"Caro, {Context.User.Username} ", Context.User.GetAvatarUrl());
            embed.WithColor(40, 200, 150);
            embed.WithTitle($"Seus Pontos nos meus registros são:");
            embed.WithDescription($"\n**{ currentAcc.XP}\n** \nContinue fazendo atividades e Jogando com o Clã para mais pontos! \n**Lute para sempre, Guardião!**");
            await Context.Channel.SendMessageAsync("", false, embed.Build());
            return;
        }

        [Command("triunfos"), Alias("meustriunfos","minhainfo"), Summary("Suas informações no Destiny 2")]
        public async Task D2information()
        {
            using (var client = new HttpClient())
            {
                    ulong dId = Context.User.Id;
                    var select = from a in Global.AccountList
                                 where a.DiscordID == dId
                                 select a;
                    Account currentAcc = select.FirstOrDefault();
                    if (currentAcc == null)
                    {

                        EmbedBuilder embed2 = new EmbedBuilder();
                        embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                        embed2.WithColor(40, 200, 150);
                        embed2.WithTitle($"Registre seus Dados, Guardião!");
                        embed2.WithDescription($"Desculpe, { Context.User.Username},mas você não consta nos meus registros. Para se cadastrar digite o código :  ```\nregistrar (Sua Gamertag Aqui!)```");
                        await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                        return;
                    }
                    long bId = currentAcc.BungieID;

                               
                
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor($" {currentAcc.DisplayName} ", Context.User.GetAvatarUrl());
                embed.WithColor(254, 254, 0);
                embed.WithTitle($"{currentAcc.DisplayName}, ainda não acredito que realizou esses feitos:");
                string triunfos = "";
                for (int j = 0; j < currentAcc.TriunfosConcluídos.Count; j++)
                {
                    uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(j);
                    if (currentAcc.TriunfosConcluídos[key])
                    triunfos += $"\n **{Global.nomeTriunfos[currentAcc.TriunfosConcluídos.ElementAt(j).Key]}**";

                }triunfos += $"\n E você tem **{currentAcc.PontosdeTriunfos}** pontos de triunfo";
                embed.WithDescription(triunfos);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: Busca | Erro: Algo");



                Console.WriteLine("'");

                await Context.User.SendMessageAsync("");
            
            }
             
        }

        [Command("raids"), Alias("minhasraids", "incursões"), Summary("Suas informações no Destiny 2")]
        public async Task D2Stats()
        {
            using (var client = new HttpClient())
            {
                ulong dId = Context.User.Id;
                var select = from a in Global.AccountList
                             where a.DiscordID == dId
                             select a;
                Account currentAcc = select.FirstOrDefault();
                if (currentAcc == null)
                {

                    EmbedBuilder embed2 = new EmbedBuilder();
                    embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                    embed2.WithColor(40, 200, 150);
                    embed2.WithTitle($"Registre seus Dados, Guardião!");
                    embed2.WithDescription($"Desculpe, { Context.User.Username},mas você não consta nos meus registros. Para se cadastrar digite o código :  ```\nregistrar (Sua Gamertag Aqui!)```");
                    await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                    return;
                }
                long bId = currentAcc.BungieID;
               
                
                EmbedBuilder embed = new EmbedBuilder();
                embed.WithAuthor($" {currentAcc.DisplayName} ", Context.User.GetAvatarUrl());
                embed.WithColor(200, 200, 150);
                embed.WithTitle($"{currentAcc.DisplayName}, Sua pontuação para raids Atualmente é: ");
                string raids= "";
                for (int j = 0;j < currentAcc.conclusões.Count; j++)
                {
                    raids += $"\n **{Global.ActNames[currentAcc.conclusões.ElementAt(j).Key]}**  = {currentAcc.conclusões.ElementAt(j).Value}";

                }
                embed.WithDescription(raids);
                await Context.Channel.SendMessageAsync("", false, embed.Build());
              
                return;


            }
        }
        [Command("random"), Alias("ran", "rando"), Summary("Escolhe um user aleatório")]
        public async Task Randomizar()
        {
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;
            Account currentAcc = select.FirstOrDefault();
            EmbedBuilder embed = new EmbedBuilder();
            Random random = new Random();
            int line = random.Next(0, Global.AccountList.Count);
            EmbedBuilder embed2 = new EmbedBuilder();
            embed2.WithColor(0, 200, 150);
            embed2.WithTitle($"A Luz aponta para uma direção");
            embed2.WithDescription($" o User escolhido foi:\n" + "<@!" + Global.AccountList.ElementAt(line).DiscordID + ">" );
            await Context.Channel.SendMessageAsync("", false, embed2.Build());
        }


        [Command("morri na flawless"), Alias("Morri na Flawless", "morri na corrida de petra"), Summary("Suas informações no Destiny 2")]
        public async Task Flawless()
        {
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;
            Account currentAcc = select.FirstOrDefault();
            EmbedBuilder embed = new EmbedBuilder();
            Random random = new Random();
            int line = random.Next(1, 10);
            switch (line)
            {
                case 1:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + " HAHAHAHA você olhou nos olhos na morte enquanto ela acabava contigo", false);
                    break;
                case 2:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("Você já foi melhor " + "<@!" + Context.User.Id + ">", false);
                    break;
                case 3:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + " Guardião, não desista, é a primeira de muitas!", false);
                    break;
                case 4:
                    embed = new EmbedBuilder();
                    embed.WithAuthor(Context.User.Username, Context.User.GetAvatarUrl());
                    embed.WithColor(200, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + " Meus Valetes não chegariam nem perto disso... até pediria para treiná-los, mas é melhor pedir para quem não morreu", false);
                    break;
                case 5:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(200, 0, 0);
                    await Context.Channel.SendMessageAsync(" <@!" + Context.User.Id + "> " + " Opa! Ganhar uma aposta nunca foi tão fácil", false);
                    break;
                case 6:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "HAHAHA PATÉTICO", false);
                    break;
                default:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "HAHAHAHAHA você definitivamente não jogou granadas o suficiente! ", false);
                    break;
            }
        }

        [Command("oi"), Alias("ola", "olá", "oioi"), Summary("um olá")]
        public async Task Ola()
        {
            EmbedBuilder embed = new EmbedBuilder();
            Random random = new Random();
            int line = random.Next(1,10);
            switch (line)
            {
                case 1:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "Guardião, o Zavala está te procurando", false);
                    break;
                case 2:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("Tudo certo" + "<@!" + Context.User.Id + ">", false);
                    break;
                case 3:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "Oi, Eh... Você viu meus valetes vermelhos por aí?", false);
                    break;
                case 4:
                    embed = new EmbedBuilder();
                    embed.WithAuthor(Context.User.Username, Context.User.GetAvatarUrl());
                    embed.WithColor(200, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + " Um dia maravilhoso para jogar umas granadas, não concorda, Guardião?", false);
                    break;
                case 5:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(200, 0, 0);
                    await Context.Channel.SendMessageAsync(" <@!" + Context.User.Id + "> " + "**HEEEY!!**", false);
                    break;
                case 6:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "Da ultima vez que nos vimos você acabava com eles no Crisol!!!", false);
                    break;
                default:
                    embed = new EmbedBuilder();
                    embed.WithAuthor("", Context.User.GetAvatarUrl());
                    embed.WithColor(40, 200, 150);
                    RestUserMessage msg =  await Context.Channel.SendMessageAsync("<@!" + Context.User.Id + ">" + "Olá,Guardião!", false);
                    Global.Mensagem = msg.Id;
                    break;
                    

            }


          
        }


        [Command("novagrade"), Alias("ngrd", "criargrade", "grdn","gradenova","gradenva","nvagrade"), Summary("Criar uma nova grade")]
        public async Task CriarGradeNova([Remainder] string horas)
        {
            ulong dId = Context.User.Id;
            var select = from a in Global.AccountList
                         where a.DiscordID == dId
                         select a;
            Account currentAcc = select.FirstOrDefault();
            if (currentAcc == null)
            {

                EmbedBuilder embed2 = new EmbedBuilder();
                embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                embed2.WithColor(40, 200, 150);
                embed2.WithTitle($"Registre seus Dados, Guardião!");
                embed2.WithDescription($"Desculpe, { Context.User.Username},mas você não consta nos meus registros. Para se cadastrar digite o código :  ```\nregistrar (Sua Gamertag Aqui!)```");
                await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                return;
            }
            DateTime H;
            try
            {
                H = Convert.ToDateTime(horas);
            }
            catch
            {
                EmbedBuilder embed2 = new EmbedBuilder();
                embed2.WithAuthor($"Calma ae, {Context.User.Username} ", Context.User.GetAvatarUrl());
                embed2.WithColor(40, 200, 150);
                embed2.WithTitle($"Você escreveu isso certo?");
                embed2.WithDescription($"Desculpe, { Context.User.Username},mas não consegui entender o horário que deseja que a nova grade aconteça.\nCertifique-se de ter colocado formato certo após o comando (**HH:MM**)\nex: $novagrade 12:00");
                await Context.Channel.SendMessageAsync("!!!", false, embed2.Build());
                return;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.WithColor(40, 200, 150);
            embed.WithTitle($"Chegou a Hora");
            string atvslist = "";
            ulong ide;
            IEnumerable<GuildEmote> ayy;
            string key;
            for (int n = 0; n < Global.AtvsEmojis.Count; n++)
            {
                
                key = Global.AtvsEmojis.Keys.ElementAt(n);
                //ayy = from a in Context.Guild.Emotes
                //where a.Name.Equals(key)
                //select a;
                //ide = ayy.FirstOrDefault().Id;
               
                atvslist += "\n "+ key + " === " + Global.AtvsEmojis[key];
            }                 

                embed.WithDescription($"Escolha a Atividade que Quer fazer! \nReaja com o Emoji referente a atv, como mostrado abaixo!"+atvslist);
            IUserMessage x = await Context.User.SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, embed.Build());
            Grade grd = new Grade();
            grd.time = H;
            Global.MsgGrade.Add(x.Id,grd );
            
            return;
        }

        //     using (var client = new HttpClient())
        //        {
        //            client.DefaultRequestHeaders.Add("X-API-Key", "6b08e62c804f489eb1f6ec8a11b9a79f");

        //            var response = await client.GetAsync("https://www.bungie.net/platform/Destiny/Manifest/InventoryItem/1274330687/");
        //var content = await response.Content.ReadAsStringAsync();
        //dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

        //Console.WriteLine(item.Response.data.inventoryItem.itemName); //Gjallarhorn
        //        }
        public static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            char last = ' ';
            int offset = 0;
            int indentLength = 2;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\\':
                        if (quote && last != '\\') ignore = true;
                        break;
                }

                if (quote)
                {
                    sb.Append(ch);
                    if (last == '\\' && ignore) ignore = false;
                }
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (quote || ch != ' ') sb.Append(ch);
                            break;
                    }
                }
                last = ch;
            }



            return sb.ToString().Trim();
        }

        public static string GetUserRole(SocketGuildUser u)
        {
            
                   
            List<SocketRole> rolesuser = u.Roles.ToList().OrderBy(o => o.Position).ToList();

            string role = rolesuser.ElementAt(rolesuser.Count - 1).Name;
            if (Global.EmojiNomeCargo.ContainsKey(role)) return Global.EmojiNomeCargo[role];
            
          //fazer if contains roel.name = Lider return
         
            return "🔷";

        }
        public async static void UpdateAccounts()
        {

            await Global.semaphoreSlim.WaitAsync();
            List<Account> listanova = new List<Account>();
            try
            {
                foreach (Account Acc in Global.AccountList)
                {

                    Account currentAcc = new Account(Acc.DisplayName, Acc.BungieID, Acc.DiscordID);
                    currentAcc.Characterid = Acc.Characterid;
                    currentAcc.cargo = Acc.cargo;
                    using (var client = new HttpClient())
                    {

                        string content;
                        HttpResponseMessage response = new HttpResponseMessage();

                        client.DefaultRequestHeaders.Add("X-API-Key", "6b08e62c804f489eb1f6ec8a11b9a79f");

                        Regex rg;
                        //Pontuação aqui





                        try
                        {

                            response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Profile/" + currentAcc.BungieID + "/" + "?components=" + "Profiles,Characters,Records");
                            content = await response.Content.ReadAsStringAsync();
                            rg = new Regex(Regex.Escape("Response"));
                            content = rg.Replace(content, "Profile", 1);
                            //content = JsonPrettyPrint(content);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: COmponents TRiunfos {currentAcc.DisplayName} | Erro: Algo");
                            Console.WriteLine(e);
                            return;
                        }
                        if (content.Contains("\"ErrorCode\":5"))
                        {
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"Desculpe ");
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://tenor.com/view/destiny-gif-6188486");
                            embed3.WithTitle($"Estamos a ermo no Vácuo!");
                            embed3.WithDescription($"Desculpe, mas Parece que os Serviços do Destiny estão **fora do ar**");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());
                            return;
                        }

                        List<uint> teste = new List<uint>();
                        List<uint> fora = new List<uint>();
                        Responses resp;
                        try
                        {
                            resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                        }
                        catch
                        {
                            EmbedBuilder embed3 = new EmbedBuilder();
                            embed3.WithAuthor($"Desculpe ");
                            embed3.WithColor(200, 40, 0);
                            embed3.WithImageUrl("https://tenor.com/view/destiny-gif-6188486");
                            embed3.WithTitle($"Estamos a ermo no Vácuo!");
                            embed3.WithDescription($"Desculpe, mas Parece que os Serviços do Destiny estão **fora do ar**");
                            await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());
                            return;
                        }
                        Dictionary<Int64, Char_Component> datanow = resp.Profile.characters.data;
                        Dictionary<UInt32, RecordComponent> listaTriunfo = resp.Profile.profileRecords.data.records;
                        currentAcc.PontosdeTriunfos = resp.Profile.profileRecords.data.score;

                        for (int n = 0; n < currentAcc.TriunfosConcluídos.Count; n++)
                        {

                            uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(n);
                            if (listaTriunfo.ContainsKey(key))
                            {
                                teste.Add(key);
                                if (Global.AllCompleted(listaTriunfo[key].objectives)) currentAcc.TriunfosConcluídos[key] = true;
                            }
                            else
                            {
                                fora.Add(key);
                            }
                        }





                        List<Atividades> atvs = new List<Atividades>();
                        int xpChar = 0;
                        Responses respon;
                        Welcome respon2;
                        string content2 = "";
                                        //##########################        FOREACH PARA CHAAR
                        foreach (long characterid in currentAcc.Characterid)
                        {
                            //TRIUNFOS CHAR
                            try
                            {
                                response = await client.GetAsync("https://bungie.net/platform/destiny2/1/profile/" + currentAcc.BungieID + "/character/" + characterid + "/" + "?components=" + " records");
                                content2 = await response.Content.ReadAsStringAsync();
                                rg = new Regex(Regex.Escape("Response"));
                                content2 = rg.Replace(content2, "CharRecord", 1);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{DateTime.Now} at commands] algo deu errado ao executar o comando. texto: testechar records {currentAcc.DisplayName} | erro: algo");
                                Console.WriteLine(e);
                                return;
                            }
                            try
                            {
                               respon2 = Newtonsoft.Json.JsonConvert.DeserializeObject<Welcome>(content2);
                                
                            }
                            catch (Exception s)
                            {
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"desculpe");
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://assets.bigcartel.com/product_images/232119707/1549993729361_untitled-artwork.jpg?auto=format&fit=max&h=1000&w=1000");
                                embed3.WithTitle($"estamos a ermo no vácuo!");
                                embed3.WithDescription($"desculpe, mas parece que algo deu errado com  nossa conexão");
                                foreach (ISocketMessageChannel g in Global.Guilds)
                                {
                                    await g.SendMessageAsync("!!!", false, embed3.Build());
                                }
                                return;
                            }
                          Dictionary<long, Record> listaTriunfoCHAR;

                            if (respon2.CharRecord.Records.Data != null)
                            {
                                //    EmbedBuilder embed3 = new EmbedBuilder();
                                //    embed3.WithAuthor($"desculpe");
                                //    embed3.WithColor(200, 40, 0);
                                //    embed3.WithImageUrl("https://i.etsystatic.com/16340822/r/il/d1e421/1590880703/il_794xN.1590880703_52qw.jpg");
                                //    embed3.WithTitle($"{currentAcc.DisplayName}, Não posso pegar seus dados");
                                //    embed3.WithDescription($"{currentAcc.DisplayName}, Suas Preferências de Privacidae não me permitem obter dados, vá no site da bungie parar habilitá-los  https://www.bungie.net/en/Profile/Settings/254/16074013?category=Privacy");
                                //    foreach (ISocketMessageChannel g in Global.Guilds)
                                //    {
                                //        await g.SendMessageAsync("", false, embed3.Build());
                                //    }
                                //    break;

                               listaTriunfoCHAR = respon2.CharRecord.Records.Data.Records;//resp.Profile.profileRecords.data.records;




                                for (int n = 0; n < currentAcc.TriunfosConcluídos.Count; n++)
                                {

                                    uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(n);
                                    if (listaTriunfoCHAR.ContainsKey(key))
                                    {
                                        teste.Add(key);
                                        if (Global.AllCompleted(listaTriunfoCHAR[key].Objectives)) currentAcc.TriunfosConcluídos[key] = true;
                                    }
                                    else
                                    {
                                        fora.Add(key);
                                    }
                                }

                            }



                            //Profile
                            try
                            {
                                response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Account/" + currentAcc.BungieID + "/Character/" + characterid + "/Stats/?page=0,mode=raid,count=250");
                                content2 = await response.Content.ReadAsStringAsync();
                                rg = new Regex(Regex.Escape("Response"));
                                content2 = rg.Replace(content2, "History", 1);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: RAIDS {currentAcc.DisplayName} | Erro: Algo");
                                Console.WriteLine(e);
                                return;
                            }
                            try
                            {
                                respon = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content2);

                            }
                            catch (Exception s)
                            {
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"Desculpe");
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://assets.bigcartel.com/product_images/232119707/1549993729361_Untitled-Artwork.jpg?auto=format&fit=max&h=1000&w=1000");
                                embed3.WithTitle($"Estamos a ermo no Vácuo!");
                                embed3.WithDescription($"Desculpe, mas parece que Algo deu errado com  nossa conexão");
                                foreach (ISocketMessageChannel g in Global.Guilds)
                                {
                                    await g.SendMessageAsync("!!!", false, embed3.Build());
                                }
                                return;
                            }

                            //PVP Score
                            HistoryChar hc;
                            if (!respon.History.TryGetValue("allPvP", out hc)) return;
                            if (hc.allTime != null)
                            {
                                for (int n = 0; n < currentAcc.Pvp.Keys.Count; n++)
                                {
                                    string key = currentAcc.Pvp.Keys.ElementAt(n);
                                    currentAcc.Pvp[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                    //xpChar += Convert.ToInt32(currentAcc.Pvp[key] * AllAccounts.PesoPvp[key]);
                                }
                            }
                            ////Raids
                            if (!respon.History.TryGetValue("raid", out hc)) return;
                            if (hc.allTime != null)
                            {
                                for (int n = 0; n < currentAcc.Raid.Keys.Count; n++)
                                {
                                    string key = currentAcc.Raid.Keys.ElementAt(n);
                                    currentAcc.Raid[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                    //xpChar += Convert.ToInt32(currentAcc.Raid[key] * AllAccounts.PesoRaid[key]);
                                }
                            }
                            ////Gambit
                            if (!(respon.History.TryGetValue("allPvECompetitive", out hc))) return;
                            if (hc.allTime != null)
                            {
                                for (int n = 0; n < currentAcc.Gambit.Keys.Count; n++)
                                {
                                    string key = currentAcc.Gambit.Keys.ElementAt(n);
                                    currentAcc.Gambit[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                    //xpChar += Convert.ToInt32(currentAcc.Gambit[key] * AllAccounts.PesoGambit[key]);
                                }
                            }
                            ////PVE
                            if (!respon.History.TryGetValue("allPvE", out hc)) return;
                            if (hc.allTime != null)
                            {
                                for (int n = 0; n < currentAcc.AllPve.Keys.Count; n++)
                                {
                                    string key = currentAcc.AllPve.Keys.ElementAt(n);
                                    currentAcc.AllPve[key] += Math.Round(hc.allTime[key].basic.value, 2);
                                    // xpChar += Convert.ToInt32(currentAcc.AllPve[key] * AllAccounts.PesoAllPve[key]);
                                }
                            }
                            ////Strikes
                            if (!respon.History.TryGetValue("allStrikes", out hc)) return;
                            if (hc.allTime != null)
                            {
                                for (int n = 0; n < currentAcc.Assaltos.Keys.Count; n++)
                                {
                                    string key = currentAcc.Assaltos.Keys.ElementAt(n);
                                    if (key.Equals("fastestCompletionMs"))
                                    {
                                        if (hc.allTime[key].basic.value > 0) currentAcc.Assaltos[key] = Math.Round(Math.Abs(10 - hc.allTime[key].basic.value / 60000), 2);
                                        else currentAcc.Assaltos[key] = 0;
                                    }
                                    else
                                    {
                                        currentAcc.Assaltos[key] += hc.allTime[key].basic.value;
                                    }
                                    //xpChar += Convert.ToInt32(currentAcc.Assaltos[key] * AllAccounts.PesoAssaltos[key]);
                                }
                            }


                            try
                            {
                                response = await client.GetAsync("https://bungie.net/Platform/Destiny2/1/Account/" + currentAcc.BungieID + "/Character/" + characterid + "/Stats/AggregateActivityStats/");
                                content = await response.Content.ReadAsStringAsync();
                                rg = new Regex(Regex.Escape("Response"));
                                content = rg.Replace(content, "Atvs", 1);
                                resp = Newtonsoft.Json.JsonConvert.DeserializeObject<Responses>(content);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: ATVS {currentAcc.DisplayName} | Erro: Algo");
                                Console.WriteLine(e);
                                return;
                            }
                            for (int i = 0; i < resp.Atvs.activities.Count; i++)
                            {
                                Atividades act = resp.Atvs.activities.ElementAt<Atividades>(i);
                                if (act.activityHash.Equals(2693136600) | act.activityHash.Equals(2693136601) | act.activityHash.Equals(2693136602) | act.activityHash.Equals(2693136603) | act.activityHash.Equals(2693136604) | act.activityHash.Equals(2693136605))
                                {
                                    currentAcc.conclusões[2693136600] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                    if (act.activityHash == 2693136605)
                                    {
                                        //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[2693136600] * AllAccounts.pesos[2693136600]);
                                    }
                                }
                                else if (act.activityHash.Equals(417231112) | act.activityHash.Equals(1685065161) | act.activityHash.Equals(2449714930) | act.activityHash.Equals(3446541099) | act.activityHash.Equals(3879860661) | act.activityHash.Equals(757116822))
                                {
                                    currentAcc.conclusões[417231112] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                    if (act.activityHash == 757116822)
                                    {
                                        //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[417231112] * AllAccounts.pesos[417231112]);
                                    }
                                }
                                else if (Global.pesos.ContainsKey(act.activityHash))
                                {
                                    currentAcc.conclusões[act.activityHash] += Convert.ToInt32(act.values["activityCompletions"].basic.value);
                                    //currentAcc.XP += Convert.ToInt32(currentAcc.conclusões[act.activityHash] * AllAccounts.pesos[act.activityHash]);
                                }
                                if (act.activityHash.Equals(709854835))
                                {
                                    if (act.values["activityCompletions"].basic.value > 0)
                                    {
                                        currentAcc.TriunfosConcluídos[709854835] = true;
                                        xpChar += Convert.ToInt32(Global.pesoTriunfos[act.activityHash]);
                                    }
                                }
                            }


                        }
                        for (int n = 0; n < currentAcc.conclusões.Keys.Count; n++)
                        {
                            long key = currentAcc.conclusões.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.conclusões[key] * Global.pesos[key]);
                        }
                        for (int n = 0; n < currentAcc.Pvp.Keys.Count; n++)
                        {
                            string key = currentAcc.Pvp.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.Pvp[key] * Global.PesoPvp[key]);
                        }
                        for (int n = 0; n < currentAcc.Raid.Keys.Count; n++)
                        {
                            string key = currentAcc.Raid.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.Raid[key] * Global.PesoRaid[key]);
                        }
                        for (int n = 0; n < currentAcc.Gambit.Keys.Count; n++)
                        {
                            string key = currentAcc.Gambit.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.Gambit[key] * Global.PesoGambit[key]);
                        }
                        for (int n = 0; n < currentAcc.AllPve.Keys.Count; n++)
                        {
                            string key = currentAcc.AllPve.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.AllPve[key] * Global.PesoAllPve[key]);
                        }
                        for (int n = 0; n < currentAcc.Assaltos.Keys.Count; n++)
                        {
                            string key = currentAcc.Assaltos.Keys.ElementAt(n);
                            xpChar += Convert.ToInt32(currentAcc.Assaltos[key] * Global.PesoAssaltos[key]);
                        }

                        ///Add pontos de triunfos
                        for (int m = 0; m < currentAcc.TriunfosConcluídos.Keys.Count; m++)
                        {
                            uint key = currentAcc.TriunfosConcluídos.Keys.ElementAt(m);

                            if (currentAcc.TriunfosConcluídos[key] && !key.Equals(709854835))
                            {
                                currentAcc.XP += Convert.ToInt32(Global.pesoTriunfos[key]);
                            }
                        }
                        currentAcc.XP += xpChar;

                        Global.SaveAccounts();
                        if (currentAcc.XP > 50)
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Patrulheiro 💠");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "💠";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }

                        }
                        if (currentAcc.XP > 150)
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Vanguardista 🏵️");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🏵️";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }

                        }
                        if (currentAcc.XP > 250 && Acc.XP < 250) 
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Escolhido 💮");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "💮";
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"PARABÉNS {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTG2GhjBGDQPQ3ouXxEM4WhbU9OXMG3XmTePRn_mbz6JUEKr7qYRA");
                                embed3.WithTitle($"VOCÊ É UM DOS ESCOLHIDOS DO VIAJANTE");
                                embed3.WithDescription($"O guardião <@!" + Trying.Id + "> Acabou de se Tornar Escolhido.\n Eu não paro de impressionar com você...");
                                await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());
                                
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }


                        }
          
                        if (currentAcc.XP > 350 &&  Acc.XP < 350)
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Sombra do Viajante 🌓");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🌓";
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"Vejam! {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/9481bf02-bc14-40b7-be50-82b5bd1565a1/dcut1o6-1bb31358-8dcc-4dbf-af91-94f9d7c198a0.jpg/v1/fill/w_765,h_1044,q_70,strp/destiny_2_warlock_by_brianmoncus_dcut1o6-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTM5NyIsInBhdGgiOiJcL2ZcLzk0ODFiZjAyLWJjMTQtNDBiNy1iZTUwLTgyYjViZDE1NjVhMVwvZGN1dDFvNi0xYmIzMTM1OC04ZGNjLTRkYmYtYWY5MS05NGY5ZDdjMTk4YTAuanBnIiwid2lkdGgiOiI8PTEwMjQifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.rX2qVEP45V-ywJnm48V1d0KcshMxMrrOcH8fRo-Eb5k");
                                embed3.WithTitle($"VOCÊ É A SOMBRA DO VIAJANTE ");
                                embed3.WithDescription($"A mais nova Sombra, <@!" + Trying.Id + "> Quanto mais forte a Luz, mais forte será sua Sombra... e Nossa Luz é imbatível, guardiã(o).");
                                await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());
                                //
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }


                        }
                        if (currentAcc.XP > 450 && Acc.XP < 450)
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Resplendor da Luz ✨");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "✨";
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"HEY {currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://www.gamersdecide.com/sites/default/files/authors/u145693/destiny2-sunbreaker.jpg");
                                embed3.WithTitle($"VOCÊ É O RESPLENDOR DA LUZ");
                                embed3.WithDescription($"O guardião <@!" + Trying.Id + "> Mostra sua soberania, e se conssagra como um Resplendor da Luz.\n");
                                await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }


                        }
                        if (currentAcc.XP > 550 && Acc.XP < 550)
                        {
                            var guild = Global.socketGuilds[0];
                            var Trying = guild.GetUser(currentAcc.DiscordID);
                            try
                            {
                                var role = guild.Roles.First(r => r.Name == "Arauto do Destino 🌀");
                                await Trying.AddRoleAsync(role);
                                if (!currentAcc.cargo.Equals("🐲") && !currentAcc.cargo.Equals("☠️") && !currentAcc.cargo.Equals("🌟") && !currentAcc.cargo.Equals("🌐")) currentAcc.cargo = "🌀";
                                EmbedBuilder embed3 = new EmbedBuilder();
                                embed3.WithAuthor($"{currentAcc.DisplayName} ", Trying.GetAvatarUrl());
                                embed3.WithColor(200, 40, 0);
                                embed3.WithImageUrl("https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/2c9397a7-eaff-49a3-bff2-5496244eaa7f/d8crbgu-3a896568-74dd-48fb-a731-d84fc74421ee.jpg/v1/fill/w_894,h_894,q_70,strp/ascension_by_theartofsaul_d8crbgu-pre.jpg?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7ImhlaWdodCI6Ijw9MTIwMCIsInBhdGgiOiJcL2ZcLzJjOTM5N2E3LWVhZmYtNDlhMy1iZmYyLTU0OTYyNDRlYWE3ZlwvZDhjcmJndS0zYTg5NjU2OC03NGRkLTQ4ZmItYTczMS1kODRmYzc0NDIxZWUuanBnIiwid2lkdGgiOiI8PTEyMDAifV1dLCJhdWQiOlsidXJuOnNlcnZpY2U6aW1hZ2Uub3BlcmF0aW9ucyJdfQ.vKqwYAoAIyERd8gQMTgNAxgr8FgR5eBe_YXEjQKK4B8");
                                embed3.WithTitle($"Você é um Arauto do Destino");
                                embed3.WithDescription($"O Destino é inevitável <@!" + Trying.Id + "> Tal qual aquele que precede sua chegada... Tenho medo de onde pode chegar");
                                await Global.Guilds[0].SendMessageAsync("!!!", false, embed3.Build());

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"{e}");
                            }


                        }



                    }

                    listanova.Add(currentAcc);
                }

                    Global.AccountList = listanova;
                    Global.SaveAccounts();
                }
            
            finally
            {
                Global.semaphoreSlim.Release();
            }
            
        }
    }
}