using System;
using System.Collections.Generic;
using System.Text;

namespace BotD.Core.Models.Bungie
{
    public class Char_Component
    {
        public Int64 membershipId;
        public Int32 membershipType;
        public Int64 characterId;
        public DateTime dateLastPlayed;
        public Int64 minutesPlayedThisSession;
        public Int64 minutesPlayedTotal;
        public Int32 light;
        public Dictionary<UInt32, Int32> stats;
        public UInt32 raceHash;
        public UInt32 genderHash;
        public UInt32 classHash;
        public Int32 raceType;
        public Int32 classType;
        public Int32 genderType;
        public string emblemPath;
        public string emblemBackgroundPath;
        public UInt32 emblemHash;
        public EmblemColor emblemColor;
        public LevelProgression levelProgression;
        public int baseCharacterLevel;
        public double percentToNextLevel;
        public long titleRecordHash;
    }
    public partial class EmblemColor
    {
        public long red;
        public long green;
        public long blue;
        public long alpha;
    }
    public partial class LevelProgression {

        public UInt32 progressionHash;
        public int dailyProgress;
        public int dailyLimit;
        public int weeklyProgress;
        public int weeklyLimit;
        public int currentProgress;
        public int level;
        public int levelCap;
        public int stepIndex;
        public int progressToNextLevel;
        public int nextLevelAt;
        public int currentResetCount;
        public List<Resets> seasonResets;
    }
    public partial class Resets
    {
        public int season;
        public int resets;
    }

    }
