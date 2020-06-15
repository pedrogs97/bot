using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models.BotAccount
{
    public enum Status
    {
        Em_andamento,
        Cancelada,
        finalizada,
        aberta,
    };
    public class Grade
    {
        public Status stat;
        public string atv;
        public List<Account> Principais;
        public List<Account> Reserva;
        public string obs;
        public DateTime time;
        public int MaxMembers;
        public Account Organizador;
        public ulong Msg;

        public Grade()
        {
            Principais = new List<Account>();
            Reserva = new List<Account>();
        }
    }

}
