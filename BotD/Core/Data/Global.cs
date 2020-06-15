using BotD.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Linq;
using Discord.WebSocket;
using BotD.Core.Models.Bungie;
using BotD.Core.Models.BotAccount;
using System.Threading;
using QuickType;

namespace BotD.Core.Data
{
    public enum Cargo
    {
        Guardiao,
        Patrulheiro,
        Vanguardista,
        Escolhido,
        Valete_da_Luz,
        Caveira,
        Vontade_de_Ahamkara,
        Lider

    }
    public static class Global
    {
        public static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public static DiscordSocketClient cliente;
        public static ulong Mensagem;
        public static List<ISocketMessageChannel> Guilds = new List<ISocketMessageChannel>();
        public static List<SocketGuild> socketGuilds = new List<SocketGuild>();
        public static Dictionary<long, double> pesos = new Dictionary<long, double>()
        {
            {3333172150, 1.5},//Coroa
            { 2693136600,1 },//Leviatã
            { 119944200,1 },//Pináculo
            { 3089205900 ,1 },//Devorador
            { 809170886,1.2 },//Devorador Prest
            { 417231112,1.2 },//Levitatã Prest
            { 3213556450,1.2 },//Pináculo Prest
            {2122313384,1.3 },//LW
            {548750096,1.2 },//Flagelo225,6
            { 2659723068,1.6}//Jardim
        };//106,0159
        public static Dictionary<string, double> PesoPvp { get; set; } = new Dictionary<string, double>()
        {
            {"kills",0.001 },
            {"efficiency",2 },
            {"killsDeathsRatio",2 },
            {"precisionKills",0.001 },
            {"longestKillSpree",0.1 },
            {"activitiesWon",0.0666 }//35,764
        };
        public static Dictionary<string, double> PesoRaid { get; set; } = new Dictionary<string, double>()
        {
            {"kills",0.0001 },
            {"efficiency",0.1 },
            {"longestSingleLife",0.0001 },
            {"precisionKills",0.0001 },
            {"longestKillSpree",0.01 }//21,1308
        };
        public static Dictionary<string, double> PesoAssaltos { get; set; } = new Dictionary<string, double>()
        {
            {"activitiesCleared",0.01 },
            {"fastestCompletionMs",1},//divirdir 1/minutos então aplicar peso
            {"score",0.000001},
            {"longestKillSpree",0.01}//21,177
        };
        public static Dictionary<string, double> PesoAllPve { get; set; } = new Dictionary<string, double>()
        {
            {"heroicPublicEventsCompleted",0.01 }//7,67
        };
        public static Dictionary<string, double> PesoGambit { get; set; } = new Dictionary<string, double>()
        {
            {"kills",0.0001 },
            {"efficiency",0.1 },
            {"winLossRatio",1 },
            {"precisionKills",0.001 },
            {"invasionKills",0.01 },
            {"invaderKills",0.01 },
            {"motesDeposited",0.001 }//20,2741
        };
        public static Dictionary<long, string> ActNames = new Dictionary<long, string>() {

            { 3333172150,"Coroa do Desalento"},//Coroa
            { 2693136600,"Leviatã"},//Leviatã
            { 119944200,"Pináculo Estrelar" },//Pináculo
            { 3089205900 ,"Devorador de Mundos" },//Devorador
            { 809170886,"Devorador de Mundos Prestígio" },//Devorador Prest
            { 417231112,"Leviatã Prestígio" },//Levitatã Prest
            { 3213556450,"Pináculo Estrelar Prestígio" },//Pináculo Prest
            {2122313384,"Último Desejo" },
            {548750096,"Flagelo do Passado" },
            {2659723068, "Jardim da Salvação" }
        };
        public static Dictionary<long, int> pesoTriunfos = new Dictionary<long, int>() {

            {4177910003, 10},//corrida de petra
            {2648109757,6},//Como um diamante
            {1558682416 ,6},//coroa do sossêgo
            {2422246593 ,5},//Boa sorte Hasapiko
            {2472579457,5},//Raiva incontrolada Arunak
            { 709854835,5 },//Lab Niobe
            {2422246592,5},//cordeiros ao abate Pagouri
            {851701008,3},//Só-zinho TronoSolo 
            {1290451257,10},//Sério, nunca mais trono sem morrer solo
            {866465097,30},//Quinto-Cavaleiro 50 sem morrer BF
            {807845972,3},//Somente o necessário Voodoo
            {2182090828,20},//Selo Riven
            {2053985130,20},//Selo Arsenal Negro
            {1883929036,25},//Sombra
            {2254764897,15},//MMXIX
            {1754983323,15},//Cronista
            {1313291220,15},//Vingador
            {3369119720,15},//Inquebrável
            {3798931976,10},//Dredgen
            {1693645129,10},//Quebra-Maldições
            {2757681677,10},//Destinos
            { 915055151,3 },//pontos  vitórias ano 1 D9
            { 720205407,6 },//6 pontos flawless ano 1
            {4239853304,10},// sete pecados 
            { 2422246601,4}, //-cativeiro 3 pessoas
            { 3798685639, 20 },// - matar 7 sem tomar dano
            { 2918198318,3}, //,Senhor do ferro
            { 1783003007,15 }, //Lenda do Ferro
            { 1861568610, 20 }, //Senhor da guerra
            { 4109457230, 10 },// Soma de todas as lágrimas
              { 810213052, 1 },// Assalto Perfeito(Ofensiva vex)
              { 2581768637, 3 },// Assalto Impecável(Ofensiva Vex)
              { 31448271, 8 },// Perfeição Inerente
              {3015116930, 2 },// Caça aos Pesadelos Mestre Impecável
              { 3387213, 25 },// Iluminado 
              { 2707428411, 15 },// Imortal
              { 3793754396, 20 }// Arauto
        };
        public static Dictionary<ulong, Grade> MsgGrade = new Dictionary<ulong, Grade>();
        public static Dictionary<string, string> AtvsEmojis = new Dictionary<string,string>()
        {
            { "🇦", "Crisol"},
            { "🇧", "Artimanha"},
            { "🇨", "Anoitecer"},
            { "🇩", "Evento"},
            { "🇪", "Farm"},
            { "🇫", "Coroa do Desalento"},
            { "🇬", "Flagelo do Passado"},
            { "🇭", "Último Desejo"},
            { "🇮", "Leviatã"},
            { "🇯", "Pináculo Estelar"},
            { "🇰", "Devorador de Mundos"},
            { "🇱", "Laboratório de Nióbe"},
            { "🇲", "Forja"},
            { "🇳", "Cativeiro"},
            { "🇴","Masmorra"},
            { "🇵", "Jardim Da Salvação"},
            { "🇶", "Caça Aos Pesadelos"},
            { "🇷", "Atividade da Temporada"}


        };
        public static Dictionary<string, Cargo> Emojicargo = new Dictionary<string, Cargo>()
        {
            {"🌟", Cargo.Lider},
            {"🐲" ,Cargo.Vontade_de_Ahamkara},
            {"☠️" ,Cargo.Caveira},
            {"🌐" ,Cargo.Valete_da_Luz},
            {"💮" ,Cargo.Escolhido},
            {"🏵️" ,Cargo.Vanguardista},
            {"💠" ,Cargo.Patrulheiro},
            {"🔷",Cargo.Guardiao},
           
            
        };

        public static Dictionary<string, string> EmojiNomeCargo = new Dictionary<string, string>()
        {
            {"Líder da Vanguarda 🌟","🌟"},
            {"Vontade de Ahamkara 🐲","🐲"},
            {"Caveira ☠️","☠️"},
            {"Valete da Luz 🌐","🌐"},
            {"Arauto do Destino 🌀","🌀"},
            {"Resplendor da Luz ✨","✨"},
            {"Sombra do Viajante 🌓","🌓"},
            {"Escolhido 💮","💮"},
            {"Vanguardista 🏵️","🏵️" },
            {"Patrulheiro 💠","💠"},
            {"Guardião 🔷","🔷"}


        };
        public static Dictionary<uint, string> nomeTriunfos = new Dictionary<uint, string>() {

            {4177910003, "Corrida de Petra"},//corrida de petra
            { 2648109757,"Como um diamante" },//Como um diamante
            { 1558682416 ,"Coroa do Sossego" },//coroa do sossêgo
            { 2422246593 ,"Hasapiko Impecável" },//Boa sorte Hasapiko
            { 2472579457,"Arunak Impecável" },//Raiva incontrolada Arunak
            { 2422246592,"Pagouri Impecável"},//cordeiros ao abate Pagouri
            {  851701008,"Trono Solo" },//Só-zinho TronoSolo 
            {1290451257 ,"Trono Solo Sem Morrer"},//Sério, nunca mais trono sem morrer solo
            {866465097,"Quinto-Cavaleiro" },//Quinto-Cavaleiro 50 sem morrer BF
            { 709854835,"Laboratório de Niobe" },//Lab Niobe
            {807845972,"Somente o Necessário" },//Somente o necessário Voodoo
            {2182090828,"Ruína de Riven" },//Selo Riven
            {2053985130,"Ferreiro" },//Selo Arsenal Negro
            {1883929036,"Sombra" },//Sombra
            {2254764897,"MMXIX" },//MMXIX
            {1754983323,"Cronista" },//Cronista
            {1313291220,"Vingador" },//Vingador
            {3369119720,"Inquebrável" },//Inquebrável
            {3798931976,"Dredgen" },//Dredgen
            {1693645129,"Quebra-Maldições" },//Quebra-Maldições
            {2757681677,"Andarilho" },//Destinos
            { 915055151,"Venceu 10 partidas no D9" },//pontos  vitórias ano 1 D9
            { 720205407," D9 Flawless" },//6 pontos flawless ano 1
            {4239853304,"Sete pecados"},// sete pecados 
            { 2422246601,"Cativeiro 3 pessoas"}, //-cativeiro 3 pessoas
            { 3798685639, "Matou 7 sem tomar dano na Banderia" },// - matar 7 sem tomar dano
            { 2918198318,"Senhor do Ferro"}, //,Senhor do ferro
            { 1783003007,"Lenda do Ferro" }, //Lenda do Ferro
            { 1861568610, "Senhor da Guerra" },
              { 4109457230,"Soma de Todas as Lágrimas" },
              { 810213052, "Assalto Perfeito(Ofensiva vex)" },// Assalto Perfeito(Ofensiva vex)
              { 2581768637, "Assalto Impecável(Ofensiva Vex)" },// Assalto Impecável(Ofensiva Vex)
              { 31448271, "Perfeição Inerente" },// Perfeição Inerente
              {3015116930, "Caça aos Pesadelos Mestre Impecável" },// Caça aos Pesadelos Mestre Impecável
              { 3387213, "Iluminado" },// Iluminado 
              { 2707428411, "Imortal" },// Imortal
              { 3793754396, "Arauto" }// Arauto
        };
        public static List<Grade> Grades;
        public static List<Account> AccountList;
        public static string accountsPath = ".\\Accounts.json";
        

       static Global()
        {
            if (Storage.SaveExists(accountsPath))
            {
                AccountList = Storage.loadAccounts(accountsPath).ToList();
            }
            else
            {
                AccountList = new List<Account>();
                SaveAccounts();
            }
            Grades = new List<Grade>();
        }

        public static void SaveAccounts()
        {
            Storage.SaveAccount(AccountList, accountsPath);
        }

        public static Account GetAccount(SocketUser u)
        {
            var result = from a in AccountList
                         where a.DiscordID == u.Id
                         select a;
            Account accountselected = result.FirstOrDefault();
            return accountselected;           

        }
        
        public static Account GetAccount(string DisplayName)
        {
            var result = from a in AccountList
                         where a.DisplayName.Equals(DisplayName)
                         select a;
            Account accountselected = result.FirstOrDefault();
            return accountselected;

        }

        public static Account CreateAccount(string DName, long BID, ulong DID)
        {
            Account newAccount = new Account(DName,BID,DID);
            AccountList.Add(newAccount);
            SaveAccounts();
            return newAccount;
        }
        public static bool AllCompleted(List<Objective> objs)
        {
            bool allcomp = true;
            foreach (Objective ob in objs)
            {
                if (ob.complete == false) allcomp = false;
            }
            return allcomp;
        }
        public static bool AllCompleted(ObjectiveChar[] objs)
        {
            bool allcomp = true;
            foreach (ObjectiveChar ob in objs)
            {
                if (ob.Complete == false) allcomp = false;
            }
            return allcomp;
        }

        public static EmbedBuilder EmbedGrade(Grade grd)
        {
            EmbedBuilder emb = new EmbedBuilder();
            emb.WithTitle($"**{grd.atv} -- {grd.time}**");
            emb.WithColor(200, 200, 0 );
            string r = $"{grd.obs}\n";
            string membros="";
            string reservas="";
            List<string> memberlist= new List<string>();
            List<string> reservlist = new List<string>();
            string part="";
            for (int i=0; i< grd.MaxMembers; i++)
            {
              
                memberlist.Add("\n🔷");
            }
            for (int i = 0; i < 3; i++)
            {

                reservlist.Add("\n🔷");
            }
            for (int j=0;j < grd.Principais.Count ;j++)
            {
                if(grd.Principais[j] == grd.Organizador) part = "\n" + grd.Principais[j].cargo + " " + grd.Principais[j].DisplayName+ "🎖️";
                else  part = "\n"+ grd.Principais[j].cargo +" "+ grd.Principais[j].DisplayName;
                memberlist[j] = part;
            }
            for(int g=0; g < grd.Reserva.Count; g++)
            {
                
                part = "\n" + grd.Reserva[g].cargo + " " + grd.Reserva[g].DisplayName;
                reservlist[g] = part;

            }
            for(int i= 0; i< memberlist.Count; i++)
            {
                membros += memberlist[i];
            }
            for (int i = 0; i < reservlist.Count; i++)
            {
                reservas += reservlist[i];
            }
            string inst = "\n\n Reaja com ❌ para sair da grade ou com 👋 para entrar como reserva \n Reaja com qualquer outra opção para entar!\n 🎖️ representa o organizador, que deve ser responsável pela grade";

            emb.WithDescription(r+membros+"\n  **RESERVAS** "+reservas + inst);
            
            return emb;
        }
    }
}
