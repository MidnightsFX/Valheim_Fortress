using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ValheimFortress.Challenge.Levels;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Runtime.Serialization;
using Unity.Jobs;
using UnityEngine.UIElements;
using System.Collections;
using Unity.Collections;

namespace ValheimFortress.Challenge
{
    static class Rewards
    {
        public const String NONE = CONST.NONE;
        public const String EIKYTHR = CONST.EIKYTHR;
        public const String ELDER = CONST.ELDER;
        public const String BONEMASS = CONST.BONEMASS;
        public const String MODER = CONST.MODER;
        public const String YAGLUTH = CONST.YAGLUTH;
        public const String QUEEN = CONST.QUEEN;

        public static Dictionary<String, RewardEntry> resourceRewards = new Dictionary<string, RewardEntry>
        {
            { ValheimFortress.LocalizeOrDefault("$reward_coin", "Coin"), new RewardEntry { resourcePrefab = "Coins", resouceCost = 5, requiredBoss = NONE, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_wood", "Wood"), new RewardEntry { resourcePrefab = "Wood", resouceCost = 12, requiredBoss = EIKYTHR, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_corewood", "CoreWood"), new RewardEntry { resourcePrefab = "RoundLog", resouceCost = 20, requiredBoss = BONEMASS, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_finewood", "FineWood"), new RewardEntry { resourcePrefab = "FineWood", resouceCost = 26, requiredBoss = MODER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_copper", "Copper"), new RewardEntry { resourcePrefab = "CopperOre", resouceCost = 45, requiredBoss = ELDER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_tin", "Tin"), new RewardEntry { resourcePrefab = "TinOre", resouceCost = 40, requiredBoss = ELDER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_iron", "Iron"), new RewardEntry { resourcePrefab = "IronScrap", resouceCost = 60, requiredBoss = BONEMASS, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_silver", "Silver"), new RewardEntry { resourcePrefab = "SilverOre", resouceCost = 70, requiredBoss = MODER, enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_darkmetal", "Darkmetal"), new RewardEntry { resourcePrefab = "BlackMetalScrap", resouceCost = 80, requiredBoss = YAGLUTH, enabled = true}},
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

        public static Dictionary<String, RewardEntry> GetResouceRewards()
        {
            return resourceRewards;
        }

        public static IEnumerator SpawnReward(String reward_resource, short level, GameObject shrine, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            short number_of_rewards = DetermineRewardAmount(reward_resource, level, hard_mode, boss_mode, siege_mode);

            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 spawn_position = shrine_spawnpoint.transform.position;
            float height;
            if (ZoneSystem.instance.FindFloor(spawn_position, out height))
            {
                spawn_position.y = height;
            }
            GameObject gameObject = PrefabManager.Instance.GetPrefab(resourceRewards[reward_resource].resourcePrefab);
            Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
            bool enable_pausing = false;
            short pausepoint = 0;
            short since_last_pause = 0;

            if (number_of_rewards > VFConfig.MaxRewardsPerSecond.Value)
            {
                pausepoint = (short)(number_of_rewards / VFConfig.MaxRewardsPerSecond.Value);
                enable_pausing = true;
            }
            for (int i = 0; i < number_of_rewards; i++)
            {
                if (enable_pausing)
                {
                    if (pausepoint >= since_last_pause)
                    {
                        since_last_pause++;
                    } else { yield return new WaitForSeconds(1); since_last_pause = 0; }
                }
                UnityEngine.Object.Instantiate(gameObject, spawn_position, rotation);
            }
            yield break;
        }

        public static short DetermineRewardAmount(String reward_resource, short level, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            float base_rewards_points = VFConfig.rewardsMultiplier.Value * Levels.ComputeChallengePoints(level);
            float reward_bonus = VFConfig.rewardsDifficultyScalar.Value * level;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Reward difficulty bonus {(1 + reward_bonus)}"); }
            base_rewards_points = base_rewards_points * (1 + reward_bonus);
            if (boss_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Boss Mode was enabled, reward multiplied by x1.25"); }
                base_rewards_points = base_rewards_points * 1.25f;
            }
            if (hard_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Hard Mode was enabled, reward multiplied by x1.5"); }
                base_rewards_points = base_rewards_points * 1.5f;
            }
            if (siege_mode)
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Siege Mode was enabled, reward multiplied by x1.5"); }
                base_rewards_points = base_rewards_points * 1.5f;
            }
            short number_of_rewards = (short)(base_rewards_points / resourceRewards[reward_resource].resouceCost);
            return number_of_rewards;
        }
    }
}
