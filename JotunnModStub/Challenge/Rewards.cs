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

namespace ValheimFortress.Challenge
{
    static class Rewards
    {
        static private String[] requiredBosses = { "None", "Eikythr", "TheElder", "BoneMass", "Moder", "Yagluth", "TheQueen" };
        static private float rewardsMultiplier = 1.5f;

        [DataContract]
        public class RewardEntry
        {
            public short resouceCost { get; set; }
            public String requiredBoss { get; set; }
            public String resourcePrefab { get; set; }
            public bool enabled { get; set; }
            // required for serialization, since this used to have a custom init
            public RewardEntry()
            {
            }
        }

        public class RewardEntryCollection
        {
            public Dictionary<string, RewardEntry> Rewards { get; set; }
        }

        public static Dictionary<String, RewardEntry> resourceRewards = new Dictionary<string, RewardEntry>
        {
            { ValheimFortress.LocalizeOrDefault("$reward_coin", "Coin"), new RewardEntry { resourcePrefab = "Coins", resouceCost = 5, requiredBoss = "None", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_wood", "Wood"), new RewardEntry { resourcePrefab = "Wood", resouceCost = 8, requiredBoss = "Eikythr", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_corewood", "CoreWood"), new RewardEntry { resourcePrefab = "RoundLog", resouceCost = 15, requiredBoss = "BoneMass", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_finewood", "FineWood"), new RewardEntry { resourcePrefab = "FineWood", resouceCost = 18, requiredBoss = "Moder", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_copper", "Copper"), new RewardEntry { resourcePrefab = "CopperOre", resouceCost = 25, requiredBoss = "TheElder", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_tin", "Tin"), new RewardEntry { resourcePrefab = "TinOre", resouceCost = 20, requiredBoss = "TheElder", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_iron", "Iron"), new RewardEntry { resourcePrefab = "IronScrap", resouceCost = 30, requiredBoss = "BoneMass", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_silver", "Silver"), new RewardEntry { resourcePrefab = "SilverOre", resouceCost = 35, requiredBoss = "Moder", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_darkmetal", "Darkmetal"), new RewardEntry { resourcePrefab = "BlackMetalScrap", resouceCost = 40, requiredBoss = "Yagluth", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_tar", "Tar"), new RewardEntry { resourcePrefab = "Tar", resouceCost = 20, requiredBoss = "Yagluth", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_guck", "Guck"), new RewardEntry { resourcePrefab = "Guck", resouceCost = 20, requiredBoss = "BoneMass", enabled = false}},
            { ValheimFortress.LocalizeOrDefault("$reward_sap", "Sap"), new RewardEntry { resourcePrefab = "Sap", resouceCost = 25, requiredBoss = "TheQueen", enabled = true}},
            { ValheimFortress.LocalizeOrDefault("$reward_softtissue", "SoftTissue"), new RewardEntry { resourcePrefab = "Softtissue", resouceCost = 50, requiredBoss = "TheQueen", enabled = true}},
        };

        public static string YamlRewardsDefinition()
        {
            var rewardsCollection = new RewardEntryCollection();
            rewardsCollection.Rewards = resourceRewards;
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(rewardsCollection);
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

        public static void SpawnReward(String reward_resource, Int16 level, GameObject shrine, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            float total_reward_points = rewardsMultiplier * (float)Levels.ComputeChallengePoints(level);
            // We give a bonus for higher levels, so the rewards for higher level fights will outscale the cost
            // eg: higher level fights are always worth more, in a linear fashion
            float reward_bonus = 0.02f * level;
            Jotunn.Logger.LogInfo($"Reward difficulty bonus {(1 + reward_bonus)}");
            total_reward_points = total_reward_points * (1 + reward_bonus);
            if (boss_mode) {
                Jotunn.Logger.LogInfo("Boss Mode was enabled, reward multiplied by x1.25");
                total_reward_points = total_reward_points * 1.25f;
            }
            if (hard_mode) {
                Jotunn.Logger.LogInfo("Hard Mode was enabled, reward multiplied by x1.5");
                total_reward_points = total_reward_points * 1.5f;
            }
            if (siege_mode) {
                Jotunn.Logger.LogInfo("Siege Mode was enabled, reward multiplied by x1.5");
                total_reward_points = total_reward_points * 1.5f; 
            }
            Jotunn.Logger.LogInfo($"Points available {total_reward_points}, reward selected: {reward_resource} cost: {resourceRewards[reward_resource].resouceCost} = {(total_reward_points/ resourceRewards[reward_resource].resouceCost)}");
            float number_of_rewards = total_reward_points / resourceRewards[reward_resource].resouceCost;

            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 spawn_position = shrine_spawnpoint.transform.position;
            float height;
            if (ZoneSystem.instance.FindFloor(spawn_position, out height))
            {
                spawn_position.y = height;
            }
            GameObject gameObject = PrefabManager.Instance.GetPrefab(resourceRewards[reward_resource].resourcePrefab);
            Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
            for (int i = 0; i < number_of_rewards; i++)
            {
                UnityEngine.Object.Instantiate(gameObject, spawn_position, rotation);
            }
        }
    }
}
