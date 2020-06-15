using System;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Text;
using BotD.Core.Data;
using BotD.Core.Models.BotAccount;
using Discord.Rest;
using BotD.Core.Models;
using System.Collections.Generic;
using System.Linq;
using BotD.Core.Commands;

namespace Bot
{
    class Program
    {
        private DiscordSocketClient client;
        private CommandService cmd;

        static void Main(string[] args)
        {
            
            new Program().MainAsync().GetAwaiter().GetResult();
        }
        private async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Debug
            }
            );
            cmd = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Critical
            });

            client.MessageReceived += client_MessageReceived;
            
            await cmd.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);

            client.Ready += client_Ready;
            
            client.Log += client_Log;
            client.ReactionAdded += OnReactionadd;
          
          
            
            string Token = "";
            //using (var Stream = new FileStream((Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Replace(@"bin\Debug\netcoreapp2.0", @"Data\Token.txt"), FileMode.Open, FileAccess.Read))
           // using (var ReadToken = new StreamReader(Stream))
            //{
             //   Token = ReadToken.ReadToEnd();
           //}
            await client.LoginAsync(TokenType.Bot, "NjAyODY3ODQwODczMjY3MjAy.XTXGlQ.ah4mvcEdfx4yZQUS6iPiuwB39RE");
            await client.StartAsync();
            Global.cliente = client;
            await Task.Delay(-1);
        }

        private async Task OnReactionadd(Cacheable<IUserMessage, ulong> cache, ISocketMessageChannel ch, SocketReaction reac)
        {
            Account acc = Global.AccountList.Find(a => a.DiscordID == reac.UserId);
            if (!(acc == null))
            {
                if (Global.MsgGrade.ContainsKey(reac.MessageId))
                {
                    var key = reac.MessageId;
                    Grade grde = Global.MsgGrade[key];

                    if (grde.obs == null)
                    {
                        if (!reac.Emote.Name.Equals("❌"))
                        {
                            grde.atv = Global.AtvsEmojis[reac.Emote.Name];
                            Global.MsgGrade.Remove(key);
                            EmbedBuilder embed = new EmbedBuilder();
                            embed.WithColor(40, 200, 150);
                            embed.WithTitle("Algo a declarar?");
                            embed.WithDescription("Adicione Observações em sua Grade!\n Me envie uma mensagem com as observações que deseja adicionar a grade, se não quiser nenhuma envie **0**\n Lembre-se de mencionar armas, Poder e outros atributos que considera importante para o sucesso da missão!");
                            RestUserMessage x = await ch.SendMessageAsync("", false, embed.Build());
                            Global.MsgGrade.Add(x.Id, grde);
                            acc.EstadodeAcao = Flag.escrevendo;
                            acc.lastmsg = x.Id;
                            grde.Msg = x.Id;

                        }
                        else
                        {
                            grde.stat = Status.Cancelada;
                            EmbedBuilder embed = new EmbedBuilder();
                            embed.WithColor(250, 0, 0);
                            embed.WithTitle("Grade Cancelada");
                            
                            RestUserMessage x = await ch.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else if (grde.MaxMembers != 0)

                    {
                        Grade g = Global.MsgGrade[reac.MessageId];
                        if (reac.Emote.Name.Equals("❌"))
                        {
                            if (g.Principais.Find( a => a.DiscordID == acc.DiscordID) != null)
                            {
                                if (acc.DiscordID == g.Organizador.DiscordID)
                                {
                                    g.Organizador.EstadodeAcao = Flag.deletando_grade;
                                    acc.EstadodeAcao = Flag.deletando_grade;
                                    g.stat = Status.Cancelada;
                                    EmbedBuilder embed = new EmbedBuilder();
                                    embed.WithColor(250, 0, 0);
                                    embed.WithTitle("Está desistindo??");
                                    embed.WithDescription($"\n Por que essa grade foi cancelada?: {g.atv} - {g.time}");

                                    var u = await ch.GetUserAsync(reac.UserId);

                                    await UserExtensions.SendMessageAsync(u, "", false, embed.Build());

                                }
                                else
                                {
                                    g.Principais.RemoveAll(A => A.DiscordID == acc.DiscordID);
                                    RestUserMessage x = await reac.Channel.SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, Global.EmbedGrade(g).Build());
                                    Global.MsgGrade.Remove(g.Organizador.lastmsg);
                                    Global.MsgGrade.Add(x.Id, g);
                                    g.Organizador.lastmsg = x.Id;
                                    g.Msg = x.Id;
                                }
                            }
                            if (g.Reserva.Find(a => a.DiscordID == acc.DiscordID) != null)
                            {
                                g.Reserva.RemoveAll(A => A.DiscordID == acc.DiscordID);
                                RestUserMessage x = await reac.Channel.SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, Global.EmbedGrade(g).Build());
                                Global.MsgGrade.Remove(g.Organizador.lastmsg);
                                Global.MsgGrade.Add(x.Id, g);
                                g.Organizador.lastmsg = x.Id;
                                g.Msg = x.Id;
                            }
                        }
                        else if (reac.Emote.Name.Equals("👋"))
                        {
                            if (g.Reserva.Find(a => a.DiscordID == acc.DiscordID) == null && g.Principais.Find(a => a.DiscordID == acc.DiscordID) == null)
                            {
                                g.Reserva.Add(acc);
                                RestUserMessage x = await reac.Channel.SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, Global.EmbedGrade(g).Build());
                                Global.MsgGrade.Remove(g.Organizador.lastmsg);
                                Global.MsgGrade.Add(x.Id, g);
                                g.Organizador.lastmsg = x.Id;
                                g.Msg = x.Id;
                            }
                        }
                        else if (g.Principais.Count <= g.MaxMembers)
                        {
                            if (g.Principais.Find(a => a.DiscordID == acc.DiscordID) == null && g.Reserva.Find(a => a.DiscordID == acc.DiscordID) == null)
                            {
                                
                                g.Principais.Add(acc);
                                RestUserMessage x = await reac.Channel.SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, Global.EmbedGrade(g).Build());
                                Global.MsgGrade.Remove(g.Organizador.lastmsg);
                                Global.MsgGrade.Add(x.Id, g);
                                g.Organizador.lastmsg = x.Id;
                                g.Msg = x.Id;
                            }
                        }
                    }
                   
                }
            }
        }

        private async Task client_Log(LogMessage Message)
         {
            Console.WriteLine($"{DateTime.Now} at {Message.Source}] {Message.Message}");
        }

        private async  Task client_Ready()
        {
            await client.SetGameAsync("Guardiões", null, ActivityType.Listening);
            TimerLOOP.StartTimer();
        }

        private async Task client_MessageReceived(SocketMessage arg)

        {
            var message = arg as SocketUserMessage;
            if (message == null) return;
            var context = new SocketCommandContext(client, message);
            if (context.User.IsBot) return;
            if (context.Message == null || context.Message.Content == "") return;
            if (Global.AccountList != null)
            {
                Account acc = Global.AccountList.Find(user => user.DiscordID == context.User.Id);
                if (context.Guild != null)
                {
                    if (Global.Guilds.Find(a => a.Id == context.Channel.Id) == null)
                    {
                        Global.Guilds.Add(context.Channel);
                        Global.socketGuilds.Add(context.Guild);
                    }
                }
                if (acc != null && context.Guild == null)
                {
                    var CanalGrades = from a in Global.Guilds
                                      where a.Name.Contains("grades")
                                      select a;
                    
                        if (acc.EstadodeAcao == Flag.deletando_grade)
                    {

                        var gradesadeletar = from a in Global.Grades
                                             where a.Organizador.DiscordID == acc.DiscordID
                                             where  a.stat == Status.Cancelada
                                             select a;

                        foreach (Grade g in gradesadeletar)
                        {
                            for (int i = 0; i < CanalGrades.Count(); i++)
                            {
                                EmbedBuilder embeda = new EmbedBuilder();
                                Global.Grades.Add(g);
                                embeda.WithColor(250, 0, 0);
                                embeda.WithTitle($"GRADE {g.atv} - {g.time} CANCELADA");
                                embeda.WithDescription($"Por {g.Organizador.DisplayName}. Com o motivo de: **{message.Content}** ");

                                await CanalGrades.ToList()[i].SendMessageAsync("", false, embeda.Build());
                                Global.MsgGrade.Remove(g.Msg);
                                Global.Grades.Remove(g);
                            }
                        }gradesadeletar = null;
                    }
                        
                    if (acc.EstadodeAcao == Flag.escrevendo && acc.lastmsg != 0)
                    {
                        if (Global.MsgGrade.ContainsKey(acc.lastmsg))
                        {
                            Grade g = Global.MsgGrade[acc.lastmsg];
                            if (g.obs == null)
                            {
                                if (message.Content == "0") Global.MsgGrade[acc.lastmsg].obs = "Seja Paciente e dê o seu melhor!";
                                else Global.MsgGrade[acc.lastmsg].obs = message.Content;


                                EmbedBuilder embed = new EmbedBuilder();
                                embed.WithColor(40, 200, 150);
                                embed.WithTitle("Quantos guardiões você precisa??");
                                embed.WithDescription("\nMe envie uma mensagem com o número de pessoas que precisa adicionar a sua grade,(e.g 6), o Máximo de 12 pessoas por grade!");
                                RestUserMessage x = await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else if (g.MaxMembers == 0)
                            {
                                int members;
                                if (Int32.TryParse(message.Content, out members))
                                {
                                    if (members > 12)
                                    {
                                        EmbedBuilder embed = new EmbedBuilder();
                                        embed.WithColor(200, 200, 200);
                                        embed.WithTitle("Opa!");
                                        embed.WithDescription("Digite algo válido para o número de pessoas da sua grade (ex: 6)");
                                        RestUserMessage x = await context.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                    else
                                    {

                                        g.MaxMembers = members;
                                        g.Principais.Add(acc);
                                        g.Organizador = acc;
                                        g.stat = Status.aberta;
                                        acc.EstadodeAcao = Flag.nao_escrevendo;
                                        RestUserMessage x;
                                        for (int i = 0; i < CanalGrades.Count(); i++)
                                        {
                                            x = await CanalGrades.ToList()[i].SendMessageAsync("\n Pressione AQUI para poder Reagir \n", false, Global.EmbedGrade(g).Build());
                                            Global.MsgGrade.Remove(acc.lastmsg);
                                            Global.MsgGrade.Add(x.Id, g);
                                            acc.lastmsg = x.Id;
                                            g.Msg = x.Id;
                                        }
                                        EmbedBuilder embed = new EmbedBuilder();
                                        Global.Grades.Add(g);
                                        embed.WithColor(200, 200, 200);
                                        embed.WithTitle("PRONTO");
                                        embed.WithDescription("GRADE CRIADA. Você é o organizador, portanto, qualquer problema na raid é sua responsabilidade contactar qualquer adm! Seja gigante, Guardião! Acabe com eles!");
                                        await context.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                }
                                else
                                {
                                    EmbedBuilder embed = new EmbedBuilder();
                                    embed.WithColor(200, 200, 200);
                                    embed.WithTitle("Opa!");
                                    embed.WithDescription("Digite algo válido para o número de pessoas da sua grade (ex: 6)");
                                    RestUserMessage x = await context.Channel.SendMessageAsync("", false, embed.Build());
                                  

                                }
                            }
                        }
                    }
                }
            }
            int ArgPos = 0;
            if (!(message.HasStringPrefix("$", ref ArgPos) || message.HasMentionPrefix(client.CurrentUser, ref ArgPos))) return;

            var Result = await cmd.ExecuteAsync(context, ArgPos, null);
            
            if (!Result.IsSuccess)
            {
                Console.WriteLine($"{DateTime.Now} at Commands] Algo deu errado ao executar o comando. Texto: {context.Message.Content} | Erro: {Result.ErrorReason}");
                
            }


        }

        
    }
}
