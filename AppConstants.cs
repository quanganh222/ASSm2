using System.Collections.Generic;

namespace Metroidvania.Constants
{
    public static class AppConstants
    {
        public const float BASE_DAMAGE = 10f;
        public const float CRIT_MULTIPLIER = 2.0f;
        public const float CRIT_CHANCE = 0.15f;
        public const int MAX_LEVEL = 100;
        public const float MAX_HEALTH = 9999f;
        public const int MAX_INVENTORY_SLOTS = 20;
        public const int MAX_SAVE_SLOTS = 3;
        public const float BOSS_PHASE2_HP_PERCENT = 0.5f;
        public static readonly Dictionary<string, int> RARITY_WEIGHTS = new Dictionary<string, int>
        {
            { "COMMON", 60 },
            { "RARE", 30 },
            { "EPIC", 9 },
            { "LEGENDARY", 1 }
        };
        public const string SAVE_FILE_PREFIX = "save_";
        public const string SAVE_FILE_EXT = ".json";
        public const string REPORT_FILE_NAME = "report.json";
    }
}