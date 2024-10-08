using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimFortress.Challenge
{
    internal static class RewardsData
    {
        public const String NONE = CONST.NONE;
        public const String EIKYTHR = CONST.EIKYTHR;
        public const String ELDER = CONST.ELDER;
        public const String BONEMASS = CONST.BONEMASS;
        public const String MODER = CONST.MODER;
        public const String YAGLUTH = CONST.YAGLUTH;
        public const String QUEEN = CONST.QUEEN;
        public const string FADER = CONST.FADER;

        public static Dictionary<String, RewardEntry> resourceRewards = new Dictionary<string, RewardEntry>
        {
            { ValheimFortress.LocalizeOrDefault("$reward_coin", "Coin"), new RewardEntry { resourcePrefab = "Coins", resouceCost = 5, requiredBoss = NONE, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_wood", "Wood"), new RewardEntry { resourcePrefab = "Wood", resouceCost = 12, requiredBoss = EIKYTHR, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_corewood", "CoreWood"), new RewardEntry { resourcePrefab = "RoundLog", resouceCost = 20, requiredBoss = BONEMASS, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_finewood", "FineWood"), new RewardEntry { resourcePrefab = "FineWood", resouceCost = 26, requiredBoss = MODER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_ashwood", "Ashwood"), new RewardEntry { resourcePrefab = "blackwood", resouceCost = 50, requiredBoss = FADER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_copper", "Copper"), new RewardEntry { resourcePrefab = "CopperOre", resouceCost = 45, requiredBoss = ELDER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_tin", "Tin"), new RewardEntry { resourcePrefab = "TinOre", resouceCost = 40, requiredBoss = ELDER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_iron", "Iron"), new RewardEntry { resourcePrefab = "IronScrap", resouceCost = 60, requiredBoss = BONEMASS, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_silver", "Silver"), new RewardEntry { resourcePrefab = "SilverOre", resouceCost = 70, requiredBoss = MODER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_blackmetal", "Darkmetal"), new RewardEntry { resourcePrefab = "BlackMetalScrap", resouceCost = 80, requiredBoss = YAGLUTH, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_flametal", "Flametal"), new RewardEntry { resourcePrefab = "FlametalNew", resouceCost = 160, requiredBoss = FADER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_tar", "Tar"), new RewardEntry { resourcePrefab = "Tar", resouceCost = 60, requiredBoss = YAGLUTH, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_guck", "Guck"), new RewardEntry { resourcePrefab = "Guck", resouceCost = 40, requiredBoss = BONEMASS, enabled = false}},
            { ValheimFortress.LocalizeOrDefault("$reward_sap", "Sap"), new RewardEntry { resourcePrefab = "Sap", resouceCost = 100, requiredBoss = QUEEN, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_softtissue", "SoftTissue"), new RewardEntry { resourcePrefab = "Softtissue", resouceCost = 140, requiredBoss = QUEEN, enabled = true}},
        };

        public static string YamlRewardsDefinition()
        {
            var rewardsCollection = new RewardEntryCollection();
            rewardsCollection.Rewards = resourceRewards;
            var yaml = CONST.yamlserializer.Serialize(rewardsCollection);
            return yaml;
        }

        public static void UpdateRewardsEntries(RewardEntryCollection rewards)
        {
            resourceRewards.Clear();
            foreach (KeyValuePair<string, RewardEntry> entry in rewards.Rewards)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updated Reward {entry.Key} cost:{entry.Value.resouceCost} requirement: {entry.Value.requiredBoss} prefab: {entry.Value.resourcePrefab} enabled: {entry.Value.enabled}"); }
                resourceRewards.Add(ValheimFortress.LocalizeOrDefault($"$reward_{entry.Key.ToString().ToLower()}", entry.Key), entry.Value);
            }
        }

        public static short DetermineRewardAmount(String reward_resource, short level, bool hard_mode, bool boss_mode, bool siege_mode, float multiplayer_bonus = 1f)
        {
            float base_rewards_points = DetermineRewardPoints(level, hard_mode, boss_mode, siege_mode, multiplayer_bonus);
            short number_of_rewards = (short)(base_rewards_points / RewardsData.resourceRewards[reward_resource].resouceCost);
            return number_of_rewards;
        }

        public static float DetermineRewardPoints(short level, bool hard_mode, bool boss_mode, bool siege_mode, float multiplayer_bonus)
        {
            float total_reward_points = VFConfig.rewardsMultiplier.Value * Levels.ComputeChallengePoints(level);
            float reward_bonus = VFConfig.rewardsDifficultyScalar.Value * level;
            total_reward_points = total_reward_points * (1 + reward_bonus);
            if (boss_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Boss Mode was enabled, reward multiplied by x1.25"); }
                total_reward_points = total_reward_points * 1.25f;
            }
            if (hard_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Hard Mode was enabled, reward multiplied by x1.5"); }
                total_reward_points = total_reward_points * 1.5f;
            }
            if (siege_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Siege Mode was enabled, reward multiplied by x1.5"); }
                total_reward_points = total_reward_points * 1.5f;
            }
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Modifying reward based on multiplayer bonus of {multiplayer_bonus}"); }
            total_reward_points *= multiplayer_bonus;
            return total_reward_points;
        }
    }
}
