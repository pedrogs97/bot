using BotD.Core.Data;
using BotD.Core.Models.BotAccount;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BotD.Core.Commands
{
    internal static class TimerLOOP
    {
        private static Timer looping;

        internal static Task StartTimer()
        {
            Console.WriteLine("Timer initialized");
            looping = new Timer()
            {
                Interval = 5000,
                AutoReset = false,
                Enabled = true,
                
            };
            looping.Elapsed += Looping_Elapsed;
            return Task.CompletedTask;

                
        }

        private static void Looping_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("TICKED");
            CMD.UpdateAccounts();
            for (int i = Global.Grades.Count - 1; i >= 0; i--)
            {
                Grade g = Global.Grades[i];
                // some code
                // safePendingList.RemoveAt(i);

                if (DateTime.Now.AddHours(1).Hour > Global.Grades[i].time.Hour && DateTime.Now.Day > Global.Grades[i].time.Day)
                {
                   
                    Global.MsgGrade.Remove(Global.Grades[i].Msg);
                    Global.Grades.Remove(g);
                    g = null;
                }
            }
        }
    }
}
