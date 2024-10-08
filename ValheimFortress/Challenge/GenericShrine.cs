using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    abstract class GenericShrine : MonoBehaviour, Hoverable, Interactable
    {
        protected ZNetView zNetView;
        public IntZNetProperty spawned_creatures { get; protected set; }

        public DictionaryZNetProperty alive_creature_list { get; protected set; }

        public BoolZNetProperty hard_mode { get; protected set; }
        public BoolZNetProperty boss_mode { get; protected set; }
        public BoolZNetProperty siege_mode { get; protected set; }
        public BoolZNetProperty challenge_active { get; protected set; }

        public BoolZNetProperty start_challenge { get; protected set; }

        public IntZNetProperty selected_level { get; protected set; }
        public StringZNetProperty selected_reward { get; protected set; }
        public BoolZNetProperty end_of_challenge { get; protected set; }
        public BoolZNetProperty should_add_creature_beacons { get; protected set; }
        public IntZNetProperty currentPhase { get; protected set; }
        public BoolZNetProperty wave_definition_ready { get; protected set; }
        public BoolZNetProperty spawn_locations_ready { get; protected set; }
        public BoolZNetProperty force_next_phase { get; protected set; }

        protected static bool client_set_creature_beacons = false;
        protected static bool shrine_portal_active = false;
        protected static List<GameObject> enemies = new List<GameObject>();
        protected GameObject shrine_spawnpoint;
        protected static PhasedWaveTemplate wave_phases_definitions;
        protected static Vector3[] remote_spawn_locations = new Vector3[0];
        protected static bool phase_running = false;
        protected static Spawner spawn_controller;
        protected static int availablePhases = 0;
        protected static Rewards reward_controller = new Rewards();

        protected static CustomRPC WaveDefinitionRPC;

        public virtual void Awake()
        {
            zNetView = GetComponent<ZNetView>();

            if ((bool)zNetView)
            {
                spawned_creatures = new IntZNetProperty("spawned_creatures", zNetView, 0);
                hard_mode = new BoolZNetProperty("shrine_hard_mode", zNetView, false);
                boss_mode = new BoolZNetProperty("shrine_boss_mode", zNetView, false);
                siege_mode = new BoolZNetProperty("shrine_siege_mode", zNetView, false);
                challenge_active = new BoolZNetProperty("shrine_challenge_active", zNetView, false);
                start_challenge = new BoolZNetProperty("shrine_start_challenge", zNetView, false);
                selected_level = new IntZNetProperty("shrine_selected_level", zNetView, 0);
                selected_reward = new StringZNetProperty("shrine_selected_reward", zNetView, "coins");
                end_of_challenge = new BoolZNetProperty("end_of_challenge", zNetView, false);
                should_add_creature_beacons = new BoolZNetProperty("should_add_creature_beacons", zNetView, false);
                currentPhase = new IntZNetProperty("shrine_current_phase", zNetView, 0);
                wave_definition_ready = new BoolZNetProperty("wave_definition_ready", zNetView, false);
                spawn_locations_ready = new BoolZNetProperty("spawn_locations_ready", zNetView, false);
                force_next_phase = new BoolZNetProperty("force_next_phase", zNetView, false);

                Dictionary<String, short> default_creature_dictionary = new Dictionary<String, short>() { };
                alive_creature_list = new DictionaryZNetProperty("alive_creature_list", zNetView, default_creature_dictionary);

                if (VFConfig.EnableDebugMode.Value)
                {
                    Jotunn.Logger.LogInfo("Created Shrine Znet View Values.");
                    Jotunn.Logger.LogInfo($"spawned_creatures={spawned_creatures.Get()}");
                    Jotunn.Logger.LogInfo($"hard_mode={hard_mode.Get()}");
                    Jotunn.Logger.LogInfo($"boss_mode={boss_mode.Get()}");
                    Jotunn.Logger.LogInfo($"siege_mode={siege_mode.Get()}");
                    Jotunn.Logger.LogInfo($"challenge_active={challenge_active.Get()}");
                    Jotunn.Logger.LogInfo($"start_challenge={start_challenge.Get()}");
                    Jotunn.Logger.LogInfo($"selected_level={selected_level.Get()}");
                    Jotunn.Logger.LogInfo($"selected_reward={selected_reward.Get()}");
                    Jotunn.Logger.LogInfo($"end_of_challenge={end_of_challenge.Get()}");
                    Jotunn.Logger.LogInfo($"should_add_creature_beacons={should_add_creature_beacons.Get()}");
                    Jotunn.Logger.LogInfo($"currentPhase={currentPhase.Get()}");
                    Jotunn.Logger.LogInfo($"wave_definition_ready={wave_definition_ready.Get()}");
                    Jotunn.Logger.LogInfo($"spawn_locations_ready={spawn_locations_ready.Get()}");
                    Jotunn.Logger.LogInfo($"force_next_phase={force_next_phase.Get()}");

                    //Jotunn.Logger.LogInfo($"alive_creature_list={alive_creature_list.Get()}");
                    // Print the actual entries in the alive creature list
                    string current_creature_entries = "";
                    foreach (KeyValuePair<String, short> entry in alive_creature_list.Get())
                    {
                        current_creature_entries += $"\n{entry.Key}={entry.Value}";
                    }
                    Jotunn.Logger.LogInfo($"alive_creature_list size {alive_creature_list.Get().Count} values:{current_creature_entries}");
                }
                
                WaveDefinitionRPC = NetworkManager.Instance.AddRPC("levelsyaml_rpc", VFConfig.OnServerRecieveConfigs, OnClientReceivePhaseConfigs);
                // Don't need to sync wave data to new clients connecting. There is a chance that if we swap owners during someone connecting to a region where a shrine challenge occurs that things could go wonky
                // SynchronizationManager.Instance.AddInitialSynchronization(WaveDefinitionRPC, SendPhaseConfigs);
            }
            // Jotunn.Logger.LogInfo("Shrine Awake Finished");
        }

        protected void SetCurrentCreatureList(List<HoardConfig> phase_hoard_configs)
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updating current creature list."); }
            Dictionary<String, short> new_creature_list = new Dictionary<String, short>() { };
            foreach (HoardConfig hoard in phase_hoard_configs)
            {
                if (new_creature_list.ContainsKey(hoard.prefab))
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updating creature count for existing creature."); }
                    short existing_creature_count = 0;
                    new_creature_list.TryGetValue(hoard.prefab, out existing_creature_count);
                    new_creature_list[$"{hoard.prefab}"] = (short)(existing_creature_count + hoard.amount);
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating {hoard.prefab} {existing_creature_count} + {hoard.amount} to existing creature list."); }
                } else
                {
                    if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Adding {hoard.amount} {hoard.prefab} to existing creature list."); }
                    new_creature_list.Add(hoard.prefab, hoard.amount);
                }
            }
            alive_creature_list.Set(new_creature_list);
        }

        protected IEnumerator ReconnectUnlinkedCreatures(Vector3 shrine_location, GenericShrine shrine_ref)
        {
            Dictionary<String, short> current_creature_list = alive_creature_list.Get();
            Jotunn.Logger.LogInfo($"types of creatures to reconnect: {current_creature_list.Count}");
            // Nothing to do
            if (current_creature_list.Count == 0)
            {
                // No reconnection to do, but we should move to the next phase if there is one
                force_next_phase.Set(true);
                yield break;
            }
            string current_creature_entries = "";
            foreach (KeyValuePair<String, short> entry in current_creature_list)
            {
                current_creature_entries += $"{entry.Key}={entry.Value}\n";
            }
            Jotunn.Logger.LogInfo($"Current creatures to reconnect: \n{current_creature_entries}");

            enemies.Clear();

            List<Character> potentialTargets = new List<Character> { };
            Character.GetCharactersInRange(shrine_location, VFConfig.ShrineReconnectRange.Value, potentialTargets);
            Dictionary<String, short> number_of_this_creature_added = new Dictionary<String, short>();
            foreach(String cname in current_creature_list.Keys)
            {
                number_of_this_creature_added.Add(cname, 0);
            }
            short total_number_currently_added = 0;
            short total_number_to_add = (short)current_creature_list.Sum(x => x.Value);
            short current_pause_progress = 0;
            short current_creature_add_iteration = 0;

            foreach (Character pchar in potentialTargets)
            {
                if (current_pause_progress == VFConfig.ShrineReconnectPauseBetweenAmount.Value)
                {
                    Jotunn.Logger.LogInfo($"Pausing while reconnecting creatures {current_creature_add_iteration}/{potentialTargets.Count}");
                    yield return new WaitForSeconds(1);
                }
                current_creature_add_iteration++;
                current_pause_progress++;
                string creature_name = pchar.name.Replace("(Clone)", "");
                Jotunn.Logger.LogInfo($"checking reconnection for: {creature_name}");
                
                // we might need some of this entry
                if (current_creature_list.ContainsKey(creature_name))
                {
                    // can merge this up with safe navigation
                    // we do need this entry
                    if(current_creature_list[creature_name] > 0)
                    {
                        // we need more of this entry
                        if(number_of_this_creature_added[creature_name] < current_creature_list[creature_name])
                        {
                            Jotunn.Logger.LogInfo($"locating parent GO: {pchar.gameObject.name}");
                            // Destroy item drop for this creature
                            Destroy(pchar.gameObject.GetComponent<CharacterDrop>());
                            pchar.m_onDeath = null;
                            // Set faction to boss to avoid fighting other creatures
                            pchar.m_faction = Character.Faction.Boss;
                            // Set the AI to hunt the nearby player
                            BaseAI ai = pchar.gameObject.GetComponent<BaseAI>();
                            if (ai != null)
                            {
                                ai.SetHuntPlayer(true);
                            }
                            // Add the rewards tracker, and set the reference shrine
                            pchar.gameObject.AddComponent<CreatureTracker>();
                            pchar.gameObject.GetComponent<CreatureTracker>().SetShrine(shrine_ref);
                            pchar.gameObject.GetComponent<CreatureTracker>().setCreatureName(creature_name);
                            // Add the enemy to the locally tracked list for shrine operations
                            enemies.Add(pchar.gameObject);
                            // Increment the number of characters spawned
                            number_of_this_creature_added[creature_name]++;
                            total_number_currently_added++;

                            // exit the loop if we now have enough creatures
                            if (total_number_currently_added == total_number_to_add)
                            {
                                break;
                            }
                        }

                    }
                }
            }

            if (total_number_currently_added != total_number_to_add)
            {
                short unfound_creatures = (short)(total_number_to_add - total_number_currently_added);
                Jotunn.Logger.LogInfo($"Could not locate: {unfound_creatures} reducing remaining creatures.");
                DecrementSpawned(unfound_creatures);
            }

            yield break;
        }

        protected static IEnumerator OnClientReceivePhaseConfigs(long sender, ZPackage package)
        {
            var phaseConfigs = package.ReadString();
            // Write the phase Configs to this clients variable- after parsing them back out
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Updated Non-primary znet owner with phased config."); }
            // I really wanted to do this with JSON, but I already have YAML in here, and didn't want to pull another large dep package
            wave_phases_definitions = CONST.yamldeserializer.Deserialize<PhasedWaveTemplate>(phaseConfigs);
            availablePhases = wave_phases_definitions.hordePhases.Count;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Set wave phases: {availablePhases}."); }
            yield return null;
        }

        protected static ZPackage SendPhaseConfigs()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sending Hoard phase configs to peer clients."); }
            try
            {
                if (VFConfig.EnableDebugMode.Value) {
                    Jotunn.Logger.LogInfo($"Set wave phases: {wave_phases_definitions}.");
                }
                // I really wanted to do this with JSON, but I already have YAML in here, and didn't want to pull another large dep package
                var yaml_string = CONST.yamlserializer.Serialize(wave_phases_definitions);
                ZPackage package = new ZPackage();
                package.Write(yaml_string);
                return package;
            } catch
            {
                PhasedWaveTemplate emptyTemplate = new PhasedWaveTemplate();
                var yaml_string = CONST.yamlserializer.Serialize(emptyTemplate);
                ZPackage package = new ZPackage();
                package.Write(yaml_string);
                return package;
            }
        }

        protected static void SendUpdatedPhaseConfigs()
        {
            try
            {
                // Bail if there are no configs to send
                if (wave_phases_definitions == null) { return; }
                WaveDefinitionRPC.SendPackage(ZNet.instance.m_peers, SendPhaseConfigs());
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sent Phase configs to clients."); }
            }
            catch
            {
                Jotunn.Logger.LogError("Error while server syncing phase configs");
            }
        }

        protected bool RemainingPhases()
        {
            if (availablePhases > wave_phases_definitions.hordePhases.Count)
            {
                availablePhases = wave_phases_definitions.hordePhases.Count;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phases Available was undefined too large and was reset to: {availablePhases}."); }
            }

            if (availablePhases == 0)
            {
                availablePhases = wave_phases_definitions.hordePhases.Count;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phases Available was undefined, updating it to reflect current wavephase definition {availablePhases}."); }
            }
            bool dophases_remain = (availablePhases - currentPhase.Get()) > 0;
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Phases remaining check: available:{availablePhases} current{currentPhase.Get()} {dophases_remain}."); }
            return dophases_remain;
        }

        public void SetHardMode()
        {
            hard_mode.ForceSet(true);
        }
        public void SetBossMode()
        {
            boss_mode.ForceSet(true);
        }
        public void SetSiegeMode()
        {
            siege_mode.ForceSet(true);
        }

        public void SetReward(String reward)
        {
            selected_reward.ForceSet(reward);
        }

        public void SetLevel(Int16 level)
        {
            selected_level.ForceSet(level);
        }

        public void SetStartChallenge()
        {
            start_challenge.ForceSet(true);
        }

        public void IncrementSpawned()
        {
            spawned_creatures.Set(spawned_creatures.Get() + 1);
        }

        public void EnablePortal()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Enabling Central Shrine portal."); }
            // gets the child object which holds all of the portal fx etc, and enables it
            try
            {
                GameObject shrine_portal = this.transform.Find("portal").gameObject;
                shrine_portal.SetActive(true);
            }
            catch { }
            shrine_portal_active = true;
        }

        public void Disableportal()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Disabling Central Shrine Portal."); }
            try
            {
                GameObject shrine_portal = transform.Find("portal").gameObject;
                shrine_portal.SetActive(false);
            }
            catch { }
            shrine_portal_active = false;
        }

        public void addEnemy(GameObject enemy)
        {
            enemies.Add(enemy);
        }

        public void phaseCompleted()
        {
            phase_running = false;
        }

        public int EnemiesRemaining()
        {
            try
            {
                return spawned_creatures.Get();
            }
            catch
            {
                Jotunn.Logger.LogInfo("Znet Value not retrieved for enemies remaining.");
                return 0;
            }

        }

        public void SetWaveSpawnPoints(Vector3[] spawn_points)
        {
            remote_spawn_locations = spawn_points;
            spawn_locations_ready.Set(true);
        }

        public abstract void StartChallengeMode();

        public void CleanupOldPortals(short max_cleanup_iterations = 6)
        {
            Jotunn.Logger.LogInfo("Starting cleanup of old portals.");
            // Cleanups up old portals
            int portals_cleaned = 1;
            int limit = (max_cleanup_iterations + 1);
            try
            {
                for (var i = 0; i <= portals_cleaned; i++)
                {
                    if (i > limit) { break; }
                    GameObject old_portal = GameObject.Find("VF_portal(Clone)");
                    if (old_portal != null)
                    {
                        Jotunn.Logger.LogInfo($"Found gameobject: {old_portal.name}.");
                        Destroy(old_portal.gameObject);
                        // we keep doing iterations if we actually find something to clean
                        portals_cleaned++;
                    }
                }
            }
            catch
            {
                Jotunn.Logger.LogInfo("Cleanup of portals failed.");
            }
        }

        public void DecrementSpawned(short decrease_value = 1)
        {
            // We don't want to try to decrease this past what is expected.
            // The spawned creatures data could be lost at some point so lets avoid going negative.
            if (spawned_creatures.Get() > 0)
            {
                spawned_creatures.Set(spawned_creatures.Get() - decrease_value);
            }
        }

        public void DecrementSpecificCreatureSpawned(string creature_name, bool decrement_spawned = true)
        {
            if (!zNetView.IsOwner())
            {
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Not zview owner, not decrementing."); }
                return;
            }
            
            if (decrement_spawned)
            {
                DecrementSpawned();
            }
            var active_creature_list = alive_creature_list.Get();
            // can only modify the creature list key if it actually has it- otherwise decrementing it doesn't matter...
            if (active_creature_list != null && active_creature_list.ContainsKey(creature_name))
            {
                active_creature_list[creature_name] = (short)(active_creature_list[creature_name] - 1);
                alive_creature_list.Set(active_creature_list);
            }

        }

        public abstract void Update();

        public abstract string GetHoverText();

        public abstract string GetHoverName();

        public abstract bool Interact(Humanoid user, bool hold, bool alt);

        public virtual bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void CancelShrineRun()
        {
            challenge_active.ForceSet(false);
            boss_mode.ForceSet(false);
            hard_mode.ForceSet(false);
            siege_mode.ForceSet(false);
            Disableportal();
            wave_phases_definitions = new PhasedWaveTemplate(); // Got to clear the template
            wave_definition_ready.Set(false);
            spawn_locations_ready.Set(false);
            availablePhases = 0;
            currentPhase.ForceSet(0);
        }

        public void ReconnectCreatureList()
        {
            if (enemies.Count == 0)
            {
                spawned_creatures.ForceSet(0);
                force_next_phase.ForceSet(true);
            }
        }

        public void NotifyRemainingCreatures()
        {
            // if (should_add_creature_beacons.Get() == false) { return; }
            if (client_set_creature_beacons == true) { return; }
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }

                alive_creatures += 1;
                GameObject vfx = UnityEngine.Object.Instantiate(ValheimFortress.getNotifier(), enemy.transform.localPosition, enemy.transform.rotation);
                vfx.transform.parent = enemy.transform; // Parent to the creature?
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures.Get()) { spawned_creatures.Set(alive_creatures); }
            client_set_creature_beacons = true;
        }

        public void TeleportRemainingCreatures()
        {
            if (spawned_creatures.Get() > VFConfig.TeleportCreatureThreshold.Value)
            {
                List<Player> nearby_players = new List<Player> { };
                Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
                foreach (Player localplayer in nearby_players)
                {
                    localplayer.Message(MessageHud.MessageType.Center, $"{Localization.instance.Localize("$shrine_too_many_creautres_to_teleport")} {VFConfig.TeleportCreatureThreshold.Value} {Localization.instance.Localize("$shrine_too_many_creautres_to_teleport_2")} {spawned_creatures.Get()}.");
                }
                return;
            }
            int alive_creatures = 0;
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) { continue; }
                enemy.transform.position = shrine_spawnpoint.transform.position;
                alive_creatures += 1;
            }
            // If somehow the tracked creatures got de-synced, this has literally never happend but- why not
            if (alive_creatures != spawned_creatures.Get()) { spawned_creatures.Set(alive_creatures); }
        }

        public void SpawnMultiRewardsDirectly(Dictionary<String, short> rewards_and_costs, short level, Vector3 spawn_position, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            float total_reward_points = RewardsData.DetermineRewardPoints(level, hard_mode, boss_mode, siege_mode, DetermineMultiplayerBonus());
            float equal_split_rewards_total = total_reward_points / rewards_and_costs.Count;
            foreach (KeyValuePair<String, short> reward_entry in rewards_and_costs)
            {
                short number_of_rewards = (short)(equal_split_rewards_total / reward_entry.Value);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Spawning reward {number_of_rewards} {reward_entry.Key}"); }
                StartCoroutine(reward_controller.InitReward(reward_entry.Key, number_of_rewards, spawn_position));
            }
        }

        public void SpawnReward(String reward_resource, short level, Vector3 spawn_position, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            short number_of_rewards = RewardsData.DetermineRewardAmount(reward_resource, level, hard_mode, boss_mode, siege_mode, DetermineMultiplayerBonus());
            string reward_prefab = RewardsData.resourceRewards[reward_resource].resourcePrefab;
            StartCoroutine(reward_controller.InitReward(reward_prefab, number_of_rewards, spawn_position));
        }

        public float DetermineMultiplayerBonus()
        {
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            float player_bonus = (nearby_players.Count * VFConfig.ShrineRewardPlayerBonus.Value);
            // If we have fractional reward bonuses lets ensure that people are at least getting 100% of the rewards, then provide the multiplayer bonus afterwards.
            if (VFConfig.ShrineRewardPlayerBonus.Value < 1f) { player_bonus += 1f; }
            // Only apply to multiplayer scenarios
            if (nearby_players.Count == 1) { return 1; }
            // Only going to return bonuses here and not maluses
            if (player_bonus > 1f)
            {
                return player_bonus;
            } else
            {
                return 1f;
            }
        }
    }
}
