using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models.Bungie.Activities
{
    public class Atividades
    {
        public long activityHash;
        public Dictionary<string, Stats> values;

    }

    public partial class AtvsList
    {
        public List<Atividades> activities;
    }
    public partial class Stats {
        public string statId;
        public ValuePair basic;
        public ValuePair pga;
        public ValuePair weighted;
        public long activityId;

    }


    public class ValuePair
    {

        public double value;
        public string displayValue;

    }


}
