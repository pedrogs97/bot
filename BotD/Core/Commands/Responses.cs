using BotD.Core.Models;
using BotD.Core.Models.Bungie;
using BotD.Core.Models.Bungie.Activities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Commands
{
    public class Responses
    {
        public RecordsChar CharRecord { get; set; }
        public Dictionary<string, HistoryChar> History { get; set; }
            public AtvsList Atvs { get; set; }
            public CharStats Characters { get; set; }
            public Profile Profile { get; set; }
            public List<Member> Response { get; set; }
            public int ErrorCode { get; set; }
            public int ThrottleSeconds { get; set; }
            public string ErrorStatus { get; set; }
            public string Message { get; set; }
            public MessageData MessageData { get; set; }
        
    }
}
