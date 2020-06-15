using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models.Bungie
{
    public class RecordsChar
    {
        DataCharRec records;
    }
    public class DataCharRec
    {       public DataRecordChar data;
            public long privacy;
    }
    public class DataRecordChar
    {
            public List<long> featuredRecordsHashes;
            public Dictionary<long, RecordComponent> records;
    }
   
    
}
