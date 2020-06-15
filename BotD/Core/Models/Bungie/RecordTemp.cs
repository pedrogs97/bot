using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace QuickType
{
    public partial class Welcome
    {
        [JsonProperty("CharRecord")]
        public CharRecord CharRecord { get; set; }

        [JsonProperty("ErrorCode")]
        public long ErrorCode { get; set; }

        [JsonProperty("ThrottleSeconds")]
        public long ThrottleSeconds { get; set; }

        [JsonProperty("ErrorStatus")]
        public string ErrorStatus { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("MessageData")]
        public MessageData MessageData { get; set; }
    }

    public partial class CharRecord
    {
        [JsonProperty("records")]
        public Records Records { get; set; }
    }

    public partial class Records
    {
        [JsonProperty("data")]
        public Data Data { get; set; }

        [JsonProperty("privacy")]
        public long Privacy { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("featuredRecordHashes")]
        public long[] FeaturedRecordHashes { get; set; }

        [JsonProperty("records")]
        public Dictionary<long, Record> Records { get; set; }
    }

    public partial class Record
    {
        [JsonProperty("state")]
        public long State { get; set; }

        [JsonProperty("objectives", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectiveChar[] Objectives { get; set; }

        [JsonProperty("intervalsRedeemedCount")]
        public long IntervalsRedeemedCount { get; set; }

        [JsonProperty("intervalObjectives", NullValueHandling = NullValueHandling.Ignore)]
        public ObjectiveChar[] IntervalObjectives { get; set; }
    }

    public partial class ObjectiveChar
    {
        [JsonProperty("objectiveHash")]
        public long ObjectiveHash { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("completionValue")]
        public long CompletionValue { get; set; }

        [JsonProperty("complete")]
        public bool Complete { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }
    }

    public partial class MessageData
    {
    }

    public partial class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, QuickType.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
