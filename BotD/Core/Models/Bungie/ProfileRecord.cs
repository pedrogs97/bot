using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models.Bungie
{
    public class ProfileRecord
    {
        public DataRecord data;
        public Int32 privacy;
    }
    public class DataRecord
    {
        public Int32 score;
        public Dictionary<UInt32, RecordComponent> records;
    }
    public class RecordComponent
    {
        public int state;
        public List<Objective> objectives;
        public long intervalsRedeemedCount;
       
    }
    public class Objective{
        public long objectiveHash;
        public long progress;
        public long completionValue;
        public bool complete;
        public bool visible;
    }
    
}
