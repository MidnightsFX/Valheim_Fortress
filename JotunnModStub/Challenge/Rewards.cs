using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    static class Rewards
    {
        static private String[] requiredBosses = { "None", "Eikythr", "TheElder", "BoneMass", "Moder", "Yagluth", "TheQueen" };
        static private float rewardsMultiplier = 2;
        public class RewardEntry
        {
            public short resouce_cost;
            public String required_boss;
            public String resource_prefab;
            public bool enabled;
            public RewardEntry(String resource_prefab, Int16 resouce_cost, String required_boss, bool enabled)
            {
                if (!requiredBosses.Contains(required_boss)) { throw new ArgumentException($"Boss {required_boss} must be one of the following: {requiredBosses}"); }
                this.resouce_cost = resouce_cost;
                this.required_boss = required_boss;
                this.resource_prefab = resource_prefab;
                this.enabled = enabled;
            }
        }

        public static Dictionary<String, RewardEntry> resourceRewards = new Dictionary<string, RewardEntry>
        {
            { ValheimFortress.LocalizeOrDefault("$reward_coin", "Coin"), new RewardEntry(resource_prefab: "Coins", resouce_cost: 5, required_boss: "None", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_wood", "Wood"), new RewardEntry(resource_prefab: "Wood", resouce_cost: 8, required_boss: "Eikythr", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_corewood", "CoreWood"), new RewardEntry(resource_prefab: "RoundLog", resouce_cost: 15, required_boss: "BoneMass", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_finewood", "FineWood"), new RewardEntry(resource_prefab: "FineWood", resouce_cost: 18, required_boss: "Moder", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_copper", "Copper"), new RewardEntry(resource_prefab: "CopperOre", resouce_cost: 25, required_boss: "TheElder", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_tin", "Tin"), new RewardEntry(resource_prefab: "TinOre", resouce_cost: 20, required_boss: "TheElder", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_iron", "Iron"), new RewardEntry(resource_prefab: "IronScrap", resouce_cost: 30, required_boss: "BoneMass", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_silver", "Silver"), new RewardEntry(resource_prefab: "SilverOre", resouce_cost: 35, required_boss: "Moder", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_darkmetal", "Darkmetal"), new RewardEntry(resource_prefab: "BlackMetalScrap", resouce_cost: 40, required_boss: "Yagluth", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_tar", "Tar"), new RewardEntry(resource_prefab: "Tar", resouce_cost: 20, required_boss: "Yagluth", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_guck", "Guck"), new RewardEntry(resource_prefab: "Guck", resouce_cost: 20, required_boss: "BoneMass", enabled: false)},
            { ValheimFortress.LocalizeOrDefault("$reward_sap", "Sap"), new RewardEntry(resource_prefab: "Sap", resouce_cost: 25, required_boss: "TheQueen", enabled: true)},
            { ValheimFortress.LocalizeOrDefault("$reward_softtissue", "SoftTissue"), new RewardEntry(resource_prefab: "Softtissue", resouce_cost: 50, required_boss: "TheQueen", enabled: true)},
        };

        public static Dictionary<String, RewardEntry> GetResouceRewards()
        {
            return resourceRewards;
        }

        public static void UpdateResouceRewards(VFConfig cfg)
        {
            foreach (KeyValuePair<string, RewardEntry> entry in resourceRewards)
            {

                short attempted_resouce_value = cfg.BindServerConfig(
                    "shine of challenge - rewards",
                    $"{entry.Key}_cost",
                    entry.Value.resouce_cost,
                    $"cost out of the rewards pool for 1 {entry.Key}. Smaller values will mean more rewards (whole numbers only).",
                    advanced: true).Value;
                if(attempted_resouce_value > 150 || attempted_resouce_value < 0)
                {
                    Jotunn.Logger.LogWarning($"Resouce reward for {entry.Key} is invalid, the default {entry.Value.resouce_cost} will be used.");
                } else
                {
                    resourceRewards[entry.Key].resouce_cost = attempted_resouce_value;
                }
                String attempted_required_boss = cfg.BindServerConfig(
                    "shine of challenge - rewards",
                    $"{entry.Key}_required_boss",
                    entry.Value.required_boss,
                    $"Required boss to be defeated for this reward to be available. Must be one of: {requiredBosses}.",
                    true).Value;
                if (!requiredBosses.Contains(attempted_required_boss)) 
                {
                    Jotunn.Logger.LogWarning($"Required boss set for {entry.Key} is invalid. Must be one of: {requiredBosses}");
                } else
                {
                    resourceRewards[entry.Key].required_boss = attempted_required_boss;
                }
                resourceRewards[entry.Key].enabled = cfg.BindServerConfig(
                    "shine of challenge - rewards",
                    $"{entry.Key}_enabled",
                    entry.Value.enabled,
                    $"whether or not {entry.Key} is available as a reward (true/false).",
                    true).Value;
            }
        }

        public static void SpawnReward(String reward_resource, Int16 level, GameObject shrine)
        {
            float total_reward_points = rewardsMultiplier * (float)Levels.ComputeChallengePoints(level);
            Jotunn.Logger.LogInfo($"Points available {total_reward_points}, reward selected: {reward_resource} cost: {resourceRewards[reward_resource].resouce_cost} = {(total_reward_points/ resourceRewards[reward_resource].resouce_cost)}");
            float number_of_rewards = total_reward_points / resourceRewards[reward_resource].resouce_cost;

            GameObject shrine_spawnpoint = shrine.transform.Find("spawnpoint").gameObject;
            Vector3 spawn_position = shrine_spawnpoint.transform.position;
            float height;
            if (ZoneSystem.instance.FindFloor(spawn_position, out height))
            {
                spawn_position.y = height;
            }
            GameObject gameObject = PrefabManager.Instance.GetPrefab(resourceRewards[reward_resource].resource_prefab);
            Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
            for (int i = 0; i < number_of_rewards; i++)
            {
                UnityEngine.Object.Instantiate(gameObject, spawn_position, rotation);
            }
        }
    }
}
