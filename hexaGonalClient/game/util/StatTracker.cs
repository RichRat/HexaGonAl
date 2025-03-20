using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace hexaGonalClient.game.util
{
    using StatType = Dictionary<Difficulty, Dictionary<int, int>>;

    public class StatTracker
    {
        StatType stats;

        public Difficulty Diff { get; set; } = 0;
        public int GameLength { get; set; } = 0;

        public StatTracker()
        {
            string serializedStats = Properties.Settings.Default.stats;
            if (string.IsNullOrEmpty(serializedStats))
                stats = [];
            else
                stats = JsonSerializer.Deserialize<StatType>(serializedStats);
        }

        public StatTracker UpdateStats(Player p)
        {
            if (p.IsBot || Diff == Difficulty.HotSeat || GameLength == 0)
                return this;

            update(GameLength);
            // update all wins against bot ignoring game len
            update(0);
            return this;
        }

        private void update(int len)
        {
            if (!stats.ContainsKey(Diff))
                stats.Add(Diff, []);

            var sub = stats[Diff];
            if (!sub.ContainsKey(len))
                sub.Add(len, 0);

            sub[len]++;
        }

        public int getCurrentStat()
        {
            if (stats.ContainsKey(Diff) && stats[Diff].ContainsKey(GameLength))
                return stats[Diff][GameLength];
            
            return 0;
        }

        public int getGlobalStat()
        {
            if (stats.ContainsKey(Diff) && stats[Diff].ContainsKey(0))
                return stats[Diff][0];

            return 0;
        }

        public void SaveStats()
        {
            Properties.Settings.Default.stats = JsonSerializer.Serialize(stats);
            Properties.Settings.Default.Save();
        }
    }
}
