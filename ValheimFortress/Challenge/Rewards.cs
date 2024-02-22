using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace ValheimFortress.Challenge
{
    class Rewards : MonoBehaviour
    {
        public IEnumerator InitReward(String reward_prefab, short number_of_rewards, Vector3 spawn_position)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Reward spawning of {number_of_rewards} {reward_prefab} at {spawn_position}"); }
            GameObject reward_go;
            try
            {
                reward_go = PrefabManager.Instance.GetPrefab(reward_prefab);
            } catch
            {
                Jotunn.Logger.LogWarning($"There was an error finding the reward prefab ({reward_prefab}), ensure it matches an existing prefab");
                reward_go = PrefabManager.Instance.GetPrefab("Coins");
                Jotunn.Logger.LogWarning($"Reward was set to 'Coins' as a fallback. Reward amount will likely be considerably less.");
            }
            
            Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
            bool enable_pausing = false;
            short pausepoint = 0;
            short since_last_pause = 0;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Starting reward spawn."); }
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
                    }
                    else 
                    { 
                        yield return new WaitForSeconds(1); since_last_pause = 0;
                    }
                }
                try
                {
                    Instantiate(reward_go, spawn_position, rotation);
                } catch
                {
                    Jotunn.Logger.LogWarning("Failed to spawn reward.");
                }
            }
            yield break;
        }
    }
}
