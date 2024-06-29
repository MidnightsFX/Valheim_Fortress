using Jotunn.Managers;
using System;
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
            int max_stack_size = 1;
            try
            {
                reward_go = PrefabManager.Instance.GetPrefab(reward_prefab);
                max_stack_size = reward_go.GetComponent<ItemDrop>().m_itemData.m_shared.m_maxStackSize;
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
            for (int i = 0; i < number_of_rewards;)
            {
                int remaining_or_max_stack_size;
                if (number_of_rewards > max_stack_size)
                {
                    remaining_or_max_stack_size = max_stack_size;
                } else
                {
                    remaining_or_max_stack_size = number_of_rewards;
                }
                i = i + remaining_or_max_stack_size;
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
                    GameObject spawned_reward = Instantiate(reward_go, spawn_position, rotation);
                    spawned_reward.GetComponent<ItemDrop>().m_itemData.m_stack = remaining_or_max_stack_size;
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Spawning reward {remaining_or_max_stack_size} {reward_prefab}"); }
                } catch
                {
                    Jotunn.Logger.LogWarning("Failed to spawn reward.");
                }
            }
            yield break;
        }
    }
}
