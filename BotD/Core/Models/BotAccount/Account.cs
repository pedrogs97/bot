using BotD.Core.Data;
using BotD.Core.Models.Bungie.Activities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models
{
    
    public enum Flag
    {
        escrevendo,
        nao_escrevendo,
        deletando_grade,
        escrevendo_descrição,
    };
    public class Account
    {
        public ulong lastmsg;
        public string DisplayName { get; set; }
        public long BungieID { get; set; }
        public ulong DiscordID { get; set; }
        public int PontosdeTriunfos { get; set; }
        public Flag EstadodeAcao { get; set; }
        public string cargo;
        public List<long> Characterid = new List<long>();
        public int XP { get; set; }
        public Dictionary<long, int> conclusões { get; set; } = new Dictionary<long, int>(){
            {3333172150, 0},//Coroa
            { 2693136600,0 },//Leviatã
            { 119944200,0 },//Pináculo
            { 3089205900 ,0 },//Devorador
            { 809170886,0 },//Devorador Prest
            { 417231112,0 },//Levitatã Prest
            { 3213556450,0 },//Pináculo Prest
            {2122313384,0 },//LW
            {548750096,0 },//Flagelo
             { 2659723068,0}//Jardim
         
          
        };
        public Dictionary<string, double> Pvp { get; set; } = new Dictionary<string, double>()
        {
            {"kills",0 },
            {"efficiency",0 },
            {"killsDeathsRatio",0 },
            {"precisionKills",0 },
            {"longestKillSpree",0 },
            {"activitiesWon",0}
        };
        public Dictionary<string, double> Raid{ get; set; } = new Dictionary<string, double>()
        {
            {"kills",0 },
            {"efficiency",0 },
            {"longestSingleLife",0 },
            {"precisionKills",0 },
            {"longestKillSpree",0 }
        };
        public Dictionary<string, double> Assaltos { get; set; } = new Dictionary<string, double>()
        {
            {"activitiesCleared",0},
            {"fastestCompletionMs",0},
            {"score",0},
            {"longestKillSpree",0}
        };
        public Dictionary<string, double> AllPve { get; set; } = new Dictionary<string, double>()
        {
            {"heroicPublicEventsCompleted",0 }
        };
        public Dictionary<string, double> Gambit { get; set; } = new Dictionary<string, double>()
        {
            {"kills",0 },
            {"efficiency",0 },
            {"winLossRatio",0 },
            {"precisionKills",0 },
            {"invasionKills",0 },
            {"invaderKills",0 },
            {"motesDeposited",0 }
        };
        public Dictionary<uint, bool> TriunfosConcluídos { get; set; } = new Dictionary<uint, bool>(){
            {4177910003,false},//corrida de petra
            { 2648109757,false },//Como um diamante
            { 1558682416 ,false },//coroa do sossêgo2
            { 2422246593 ,false},//Boa sorte Hasapiko
            { 2472579457,false },//Raiva incontrolada Arunak
            { 2422246592,false },//cordeiros ao abate Pagouri
            {  851701008,false},//Só-zinho TronoSolo 
            {1290451257 ,false },//Sério, nunca mais trono sem morrer solo
            {866465097,false },//Quinto-Cavaleiro 50 sem morrer BF
            {807845972,false },//Somente o necessário Voodoo
            {2182090828,false },//Selo Riven
            {2053985130,false },//Selo Arsenal Negro
            {1883929036,false },//Sombra
            {2254764897,false },//MMXIX
            {1754983323,false },//Cronista
            {1313291220,false },//Vingador
            {3369119720,false },//Inquebrável
            {3798931976,false },//Dredgen
            {1693645129,false },//Quebra-Maldições
            {2757681677,false },//Destinos
            { 709854835,false },//Lab Niobe
            { 915055151,false},//pontos  vitórias ano 1 D9
            { 720205407,false },//6 pontos flawless ano 1
            {4239853304,false},// sete pecados 
            { 2422246601,false}, //-cativeiro 3 pessoas
            { 3798685639, false },// - matar 7 sem tomar dano
            { 2918198318,false}, //,Senhor do ferro
            { 1783003007,false }, //Lenda do Ferro
             { 1861568610, false }, //Senhor da guerra
            { 4109457230,false  },//soma de todas as lágrimas
            { 810213052, false },// Assalto Perfeito(Ofensiva vex)
              { 2581768637, false  },// Assalto Impecável(Ofensiva Vex)
              { 31448271, false  },// Perfeição Inerente
              {3015116930, false },// Caça aos Pesadelos Mestre Impecável
              { 3387213, false },// Iluminado 
              { 2707428411, false },// Imortal
              { 3793754396, false }// Arauto

        };

        public Account(string DName, long BID, ulong DID)
        {
            BungieID = BID;
            DiscordID = DID;
            DisplayName = DName;
            EstadodeAcao = Flag.nao_escrevendo;
        }

    }
}
