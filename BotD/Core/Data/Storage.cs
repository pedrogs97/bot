using BotD.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.IO;

namespace BotD.Core.Data
{
    public class Storage
    {
        public static void SaveAccount(IEnumerable<Account> acc, string path)
        {
            try
            {
                string json = JsonConvert.SerializeObject(acc, Formatting.Indented);
                File.WriteAllText(path, json);
            }catch(Exception e)
            {
                Console.WriteLine("Erro ao gravar");
                return;
            }
        }
        public static IEnumerable<Account> loadAccounts(string path)
        {
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            List<Account> contas = JsonConvert.DeserializeObject<List<Account>>(json);
            Console.WriteLine("Loading");
            return (IEnumerable<Account>)contas;
        }
        public static bool SaveExists(string filePath)
        {
            return File.Exists(filePath);
        }

    }
}
