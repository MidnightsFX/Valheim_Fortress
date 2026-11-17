using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimFortress.Data;

namespace ValheimFortress.Challenge
{
    abstract class GenericShrine : MonoBehaviour, Hoverable, Interactable
    {
        protected ZNetView zNetView;
        public IntZNetProperty spawned_creatures { get; set; }
        public DictionaryZNetProperty alive_creature_list { get; set; }
        // Authoritative, owner-reconciled set of currently-alive challenge creatures, tracked by ZDOID so
        // counting survives creature/shrine ownership changes and shrine reload. See ReconcileSpawnedCreatures.
        public SpawnedCreatureRecordsZNetProperty spawned_creature_records { get; set; }
        public BoolZNetProperty hard_mode { get; set; }
        public BoolZNetProperty boss_mode { get; set; }
        public BoolZNetProperty siege_mode { get; set; }
        public BoolZNetProperty challenge_active { get; set; }
        public BoolZNetProperty start_challenge { get; set; }
        public IntZNetProperty selected_level { get; set; }
        public IntZNetProperty selected_level_index { get; set; }
        public StringZNetProperty selected_reward { get; set; }
        public BoolZNetProperty portal_disabled { get; set; }
        public BoolZNetProperty should_add_creature_beacons { get; set; }
        public IntZNetProperty currentPhase { get; set; }
        public BoolZNetProperty wave_definition_ready { get; set; }
        public BoolZNetProperty spawn_locations_ready { get; set; }
        public BoolZNetProperty force_next_phase { get; set; }
        public ArrayVectorZNetProperty remote_spawn_locations { get; set; }
        public ListStringZNetProperty adminLevelLimits { get; set; }
        public StringZNetProperty adminConfigData {  get; set; }
        // Per-phase spawn bookkeeping. These live in the ZDO rather than in local fields so that a client
        // which becomes the shrine owner part-way through a run (players walking in and out of range hands
        // the ZDO around via ZDOMan.ReleaseNearbyZDOS) evaluates the phase gate exactly as the previous
        // owner would have. phase_spawned_total counts creatures registered for the current phase;
        // phase_spawn_in_flight is true from the moment a phase spawn is kicked off until the spawner
        // finishes placing it.
        public IntZNetProperty phase_spawned_total { get; set; }
        public BoolZNetProperty phase_spawn_in_flight { get; set; }
        // Wall-clock (network time) seconds of the last point at which the run could still make progress.
        // Drives the stall watchdog; see CheckProgressWatchdog.
        public IntZNetProperty challenge_progress_time { get; set; }

        protected bool client_set_creature_beacons = false;
        // Whether the local 'enemies' list has been rebuilt since this client last became the shrine owner.
        // 'enemies' is local-only state belonging to whichever client drove the run before, so it needs a
        // one-time rebuild after an ownership change -- see NoteOwnershipState.
        protected bool local_enemies_synced = false;
        private bool was_owner = false;
        protected List<GameObject> enemies = new List<GameObject>();
        protected GameObject shrine_spawnpoint;
        protected GameObject shrine_portal;
        protected PhasedWaveTemplate wave_phases_definitions;
        protected Spawner spawn_controller;
        protected int availablePhases;
        protected Rewards reward_controller = new Rewards();
        // Throttle for owner-side creature reconciliation (see ReconcileIfDue). Time based so the cost does not
        // scale with frame rate.
        protected float next_reconcile_time = 0f;
        protected const float reconcile_interval_seconds = 0.33f;
        // Creatures spawned since the last flush (see RegisterSpawnedCreature / FlushSpawnedCreatures).
        private readonly List<SpawnedCreatureRecord> pending_creature_records = new List<SpawnedCreatureRecord>();

        protected CustomRPC WaveDefinitionRPC;

        public virtual void Awake()
        {
            if (this.gameObject.TryGetComponent<ZNetView>(out zNetView) == false)
            {
                this.gameObject.AddComponent<ZNetView>();
                zNetView = this.gameObject.GetComponent<ZNetView>();
                zNetView.m_persistent = true;
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("ZnetView was not found, and was added manually."); }
            }
            if ((bool)zNetView)
            {
                spawned_creatures = new IntZNetProperty("spawned_creatures", zNetView, 0);
                hard_mode = new BoolZNetProperty("shrine_hard_mode", zNetView, false);
                boss_mode = new BoolZNetProperty("shrine_boss_mode", zNetView, false);
                siege_mode = new BoolZNetProperty("shrine_siege_mode", zNetView, false);
                challenge_active = new BoolZNetProperty("shrine_challenge_active", zNetView, false);
                start_challenge = new BoolZNetProperty("shrine_start_challenge", zNetView, false);
                selected_level = new IntZNetProperty("shrine_selected_level", zNetView, 0);
                selected_level_index = new IntZNetProperty("selected_level_index", zNetView, 0);
                selected_reward = new StringZNetProperty("shrine_selected_reward", zNetView, "coins");
                portal_disabled = new BoolZNetProperty("end_of_challenge", zNetView, false);
                should_add_creature_beacons = new BoolZNetProperty("should_add_creature_beacons", zNetView, false);
                currentPhase = new IntZNetProperty("shrine_current_phase", zNetView, 0);
                wave_definition_ready = new BoolZNetProperty("wave_definition_ready", zNetView, false);
                spawn_locations_ready = new BoolZNetProperty("spawn_locations_ready", zNetView, false);
                force_next_phase = new BoolZNetProperty("force_next_phase", zNetView, false);
                phase_spawned_total = new IntZNetProperty("phase_spawned_total", zNetView, 0);
                phase_spawn_in_flight = new BoolZNetProperty("phase_spawn_in_flight", zNetView, false);
                challenge_progress_time = new IntZNetProperty("challenge_progress_time", zNetView, 0);
                remote_spawn_locations = new ArrayVectorZNetProperty("remote_spawn_locations", zNetView, new Vector3[0]);
                adminLevelLimits = new ListStringZNetProperty("adminLevelLimits", zNetView, new List<string>() { });
                adminConfigData = new StringZNetProperty("adminConfigData", zNetView, "filter:levelname,levelname2");

                Dictionary<String, short> default_creature_dictionary = new Dictionary<String, short>() { };
                alive_creature_list = new DictionaryZNetProperty("alive_creature_list", zNetView, default_creature_dictionary);
                spawned_creature_records = new SpawnedCreatureRecordsZNetProperty("spawned_creature_records", zNetView, new List<SpawnedCreatureRecord>());

                if (VFConfig.EnableDebugMode.Value)
                {
                    // Only log znet values if the znet is present and valid
                    if (zNetView.IsValid())
                    {
                        Jotunn.Logger.LogInfo("Created Shrine Znet View Values.");
                        Jotunn.Logger.LogInfo($"spawned_creatures={spawned_creatures.Get()}");
                        Jotunn.Logger.LogInfo($"hard_mode={hard_mode.Get()}");
                        Jotunn.Logger.LogInfo($"boss_mode={boss_mode.Get()}");
                        Jotunn.Logger.LogInfo($"siege_mode={siege_mode.Get()}");
                        Jotunn.Logger.LogInfo($"challenge_active={challenge_active.Get()}");
                        Jotunn.Logger.LogInfo($"start_challenge={start_challenge.Get()}");
                        Jotunn.Logger.LogInfo($"selected_level={selected_level.Get()}");
                        Jotunn.Logger.LogInfo($"selected_level_index={selected_level_index.Get()}");
                        Jotunn.Logger.LogInfo($"selected_reward={selected_reward.Get()}");
                        Jotunn.Logger.LogInfo($"end_of_challenge={portal_disabled.Get()}");
                        Jotunn.Logger.LogInfo($"should_add_creature_beacons={should_add_creature_beacons.Get()}");
                        Jotunn.Logger.LogInfo($"currentPhase={currentPhase.Get()}");
                        Jotunn.Logger.LogInfo($"wave_definition_ready={wave_definition_ready.Get()}");
                        Jotunn.Logger.LogInfo($"spawn_locations_ready={spawn_locations_ready.Get()}");
                        Jotunn.Logger.LogInfo($"force_next_phase={force_next_phase.Get()}");
                        Jotunn.Logger.LogInfo($"remote_spawn_locations={remote_spawn_locations}");
                        Jotunn.Logger.LogInfo($"adminLevelLimits={adminLevelLimits.Get()}");

                        //Jotunn.Logger.LogInfo($"alive_creature_list={alive_creature_list.Get()}");
                        // Print the actual entries in the alive creature list
                        string current_creature_entries = "";
                        foreach (KeyValuePair<String, short> entry in alive_creature_list.Get())
                        {
                            current_creature_entries += $"\n{entry.Key}={entry.Value}";
                        }
                        Jotunn.Logger.LogInfo($"alive_creature_list size {alive_creature_list.Get().Count} values:{current_creature_entries}");
                    }
                }
                
                WaveDefinitionRPC = NetworkManager.Instance.AddRPC("VF_levelsyaml_rpc", VFConfig.OnServerRecieveConfigs, OnClientReceivePhaseConfigs);
                // Don't need to sync wave data to new clients connecting. There is a chance that if we swap owners during someone connecting to a region where a shrine challenge occurs that things could go wonky
                // SynchronizationManager.Instance.AddInitialSynchronization(WaveDefinitionRPC, SendPhaseConfigs);
            }
            shrine_portal = gameObject.transform.Find("portal").gameObject;
            shrine_spawnpoint = gameObject.transform.FindDeepChild("spawnpoint").gameObject;
            // Jotunn.Logger.LogInfo("Shrine Awake Finished");
            availablePhases = 0;
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

        // The spawn-time decision for whether a freshly spawned challenge creature should keep its normal loot
        // drops. Physical shrines defer to the per-creature drop configuration; an unknown creature keeps its
        // drops (matching prior behavior). The API runner overrides this with the caller's global toggle plus
        // any per-creature overrides. The spawner persists the result to the creature's VFDrops ZDO flag,
        // which is the authoritative runtime gate (read on death and on reload).
        public virtual bool ShouldDropLoot(string creature)
        {
            return !Monsters.SpawnableCreatures.TryGetValue(creature, out CreatureValues values) || values == null || values.dropsEnabled;
        }

        // The between-wave pause message this shrine wants to show, or null to use the built-in random phrase
        // pool. Physical shrines use the built-in pool; the API runner overrides this to return a caller-supplied
        // phrase (ordered or random) when one was provided for the run.
        public virtual string SelectPhasePauseMessage() { return null; }

        // True when this run shows no announcements at all. Physical shrines always announce; the API runner
        // returns the caller's SuppressAnnouncements setting.
        public virtual bool SuppressAnnouncements() { return false; }

        // Re-derives local shrine state after a reload (or after the local 'enemies' list is lost) from the
        // persisted, owner-authoritative ZDOID records, instead of name/radius matching nearby creatures.
        // First reconcile to prune anything that died while we were unloaded, then rebuild the local
        // 'enemies' list (used for phase gating and notify/teleport) from survivors that are currently
        // instantiated locally. No replacement creatures are spawned (removing the old double-spawn risk);
        // survivors not yet loaded locally stay counted via spawned_creatures. shrine_location/shrine_ref are
        // retained for call-site compatibility but are no longer needed.
        protected IEnumerator ReconnectUnlinkedCreatures(Vector3 shrine_location, GenericShrine shrine_ref)
        {
            ReconcileSpawnedCreatures();
            var records = spawned_creature_records.Get();
            Jotunn.Logger.LogDebug($"creatures to reconnect: {records.Count}");
            if (records.Count == 0)
            {
                // Nothing alive to reconnect to; advance to the next phase if there is one. Only when this
                // phase actually placed creatures -- an empty record list before the first spawn lands just
                // means the spawner has not gotten to it yet, and forcing there would skip a phase.
                if (phase_spawned_total.Get() > 0) { force_next_phase.Set(true); }
                yield break;
            }

            enemies.Clear();
            foreach (var record in records)
            {
                GameObject instance = ZNetScene.instance.FindInstance(record.Id);
                if (instance == null) { continue; } // alive but not loaded locally yet; still counted

                Character pchar = instance.GetComponent<Character>();
                if (pchar != null)
                {
                    // Re-apply the challenge creature setup (idempotent) in case this is a fresh instance.
                    pchar.m_faction = Character.Faction.Boss;
                    BaseAI ai = instance.GetComponent<BaseAI>();
                    if (ai != null) { ai.SetHuntPlayer(true); }
                    // Re-apply drop suppression from the creature's own authoritative VFDrops flag, which was
                    // set at spawn from the shrine's drop decision (per-creature config, or the API runner's
                    // global toggle + per-creature overrides). Reading it back here is keyed off the creature
                    // ZDO, so it stays correct regardless of prefab/creature-name differences.
                    if (pchar.m_nview != null && pchar.m_nview.GetZDO() != null
                        && pchar.m_nview.GetZDO().GetBool("VFDrops", true) == false)
                    {
                        Destroy(instance.GetComponent<CharacterDrop>());
                        pchar.m_onDeath = null;
                    }
                }
                enemies.Add(instance);
            }

            if (enemies.Count == 0 && spawned_creatures.Get() <= 0 && phase_spawn_in_flight.Get() == false
                && phase_spawned_total.Get() > 0)
            {
                Jotunn.Logger.LogInfo("No live creatures remain after reconnection, force starting next phase.");
                force_next_phase.ForceSet(true);
            }
            yield break;
        }

        protected IEnumerator OnClientReceivePhaseConfigs(long sender, ZPackage package)
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

        protected ZPackage SendPhaseConfigs()
        {
            if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo("Sending Hoard phase configs to peer clients."); }
            try
            {
                // This is needed for the sender to also have this set, clients will set it on recieve
                availablePhases = wave_phases_definitions.hordePhases.Count;
                if (VFConfig.EnableDebugMode.Value) {
                    Jotunn.Logger.LogInfo($"Set wave phases: {availablePhases}.");
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

        protected void SendUpdatedPhaseConfigs()
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
            Jotunn.Logger.LogInfo($"Checking for remaining phases.");
            if (availablePhases > wave_phases_definitions.hordePhases.Count)
            {
                availablePhases = wave_phases_definitions.hordePhases.Count;
                Jotunn.Logger.LogInfo($"Phases Available was undefined too large and was reset to: {availablePhases}.");
            }

            if (availablePhases == 0)
            {
                availablePhases = wave_phases_definitions.hordePhases.Count;
                Jotunn.Logger.LogInfo($"Phases Available was undefined, updating it to reflect current wavephase definition {availablePhases}.");
            }
            bool dophases_remain = (availablePhases - currentPhase.Get()) > 0;
            Jotunn.Logger.LogInfo($"Phases remaining check: available:{availablePhases} current{currentPhase.Get()} {dophases_remain}.");
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

        public void SetLevel(int level, int level_index)
        {
            selected_level.ForceSet(level);
            selected_level_index.ForceSet(level_index);
        }

        public void SetStartChallenge()
        {
            Jotunn.Logger.LogInfo($"Challenge started at {this.GetInstanceID()}");
            start_challenge.ForceSet(true);
            spawned_creatures.ForceSet(0);
        }

        // Registers a freshly spawned challenge creature with the shrine owner. Called from the spawn
        // coroutine (which runs on the shrine owner). Records are buffered and written in one go by
        // FlushSpawnedCreatures: the record list is stored as a single byte array, so writing it per creature
        // re-encoded and re-sent the whole list for every spawn (quadratic in the phase size).
        public void RegisterSpawnedCreature(ZDOID creature_id, string prefab_name)
        {
            if (creature_id == ZDOID.None) { return; }
            pending_creature_records.Add(new SpawnedCreatureRecord(creature_id, prefab_name));
        }

        // Writes buffered creature records to the shrine ZDO. The spawner calls this after each spawn segment
        // and at the end of each horde; reconciliation and cleanup flush first so they always see every creature.
        // spawned_creatures is always kept equal to the record count so it can never drift.
        public void FlushSpawnedCreatures()
        {
            if (pending_creature_records.Count == 0) { return; }
            if (zNetView == null || !zNetView.IsValid())
            {
                pending_creature_records.Clear();
                return;
            }
            var records = spawned_creature_records.Get();
            records.AddRange(pending_creature_records);
            spawned_creature_records.Set(records);
            spawned_creatures.Set(records.Count);
            // Records the fact that this phase actually placed creatures, independent of the local 'enemies'
            // list, so the phase gate survives an owner change.
            phase_spawned_total.Set(phase_spawned_total.Get() + pending_creature_records.Count);
            OnCreaturesRegistered(pending_creature_records);
            pending_creature_records.Clear();
            TouchProgressWatchdog();
        }

        // Called after newly spawned creatures are written to the ZDO. The API runner keeps whole-run totals for its
        // progress state; the physical shrines need nothing.
        protected virtual void OnCreaturesRegistered(List<SpawnedCreatureRecord> added) { }

        // Called after each reconcile with the creatures still alive (the API runner tracks its bosses and
        // CompleteWhenDead creatures from it).
        protected virtual void OnReconciled(List<SpawnedCreatureRecord> survivors) { }

        // Throttled entry point for owner-side reconciliation; call once per Update from the owner region.
        public void ReconcileIfDue()
        {
            if (Time.time < next_reconcile_time) { return; }
            next_reconcile_time = Time.time + reconcile_interval_seconds;
            ReconcileSpawnedCreatures();
        }

        // Feed this the result of the per-Update IsOwner check. On a false->true transition (players moving
        // in and out of range hands the shrine ZDO around) the local run state belongs to a different client,
        // so flag it for a one-time rebuild instead of rebuilding it every frame.
        protected void NoteOwnershipState(bool is_owner)
        {
            if (is_owner == false) { was_owner = false; return; }
            if (was_owner == false)
            {
                was_owner = true;
                local_enemies_synced = false;
            }
        }

        // Call immediately before spawn_controller.TrySpawningPhase. Resets the per-phase spawn counter and
        // flags a spawn as in flight, both in the ZDO, so an owner change mid-phase does not lose the fact
        // that a phase is currently being placed.
        protected void BeginPhaseSpawn()
        {
            phase_spawned_total.Set(0);
            phase_spawn_in_flight.Set(true);
            TouchProgressWatchdog();
        }

        // Ownership-independent phase gate: is the current phase finished, so the run may advance (or pay
        // out)? Every term is ZDO-backed, so whichever client owns the shrine reaches the same answer. This
        // replaces the old 'enemies.Count > 0 && phase_running == false' test, which read local-only fields:
        // a client that took ownership part-way through a run had an empty 'enemies' list, so once the last
        // creature died neither the recovery branch (needs spawned_creatures > 0) nor this gate (needed
        // enemies.Count > 0) could fire and the challenge hung forever without granting rewards.
        protected bool ShouldAdvancePhase()
        {
            if (force_next_phase.Get()) { return true; }
            return phase_spawned_total.Get() > 0
                && spawned_creatures.Get() <= 0
                && phase_spawn_in_flight.Get() == false;
        }

        protected void TouchProgressWatchdog()
        {
            if (ZNet.instance == null) { return; }
            int now = (int)ZNet.instance.GetTimeSeconds();
            // This runs every Update while a challenge is live, and ZDO.Set marks the object dirty for
            // network sync, so only write when the whole-second value actually moves.
            if (challenge_progress_time.Get() == now) { return; }
            challenge_progress_time.Set(now);
        }

        // Safety net for a run that can no longer make progress on its own -- an owner that vanished
        // mid-spawn, a spawn coroutine lost with its client, creature ZDOs removed out from under us. If
        // ShrineStallTimeout passes with no sign of progress the run is forced forward, so it always reaches
        // its payout instead of leaving the shrine permanently active and unusable.
        //
        // Progress is anything that moves the run along: creatures alive (players are fighting), a phase
        // spawn being kicked off, or a creature registering. Deliberately NOT phase_spawn_in_flight -- that
        // flag is cleared by the spawn coroutine, so an owner that disappears mid-spawn leaves it stuck true,
        // which is precisely the stall this watchdog has to break. The longest legitimate gap between
        // progress signals is the between-phase regroup (10s) plus a spawn segment pause (up to 5s), well
        // inside the 30s minimum timeout.
        // Owner-only; call once per Update from the owner region of a challenge that is active.
        protected void CheckProgressWatchdog()
        {
            if (ZNet.instance == null) { return; }
            if (spawned_creatures.Get() > 0)
            {
                TouchProgressWatchdog();
                return;
            }

            int now = (int)ZNet.instance.GetTimeSeconds();
            int last_progress = challenge_progress_time.Get();
            // First observation (or a clock that moved backwards) just seeds the timestamp.
            if (last_progress <= 0 || last_progress > now)
            {
                TouchProgressWatchdog();
                return;
            }
            if ((now - last_progress) < VFConfig.ShrineStallTimeout.Value) { return; }

            Jotunn.Logger.LogWarning($"Challenge made no progress for {now - last_progress}s; forcing it forward so rewards are not lost.");
            phase_spawn_in_flight.Set(false);
            // Nothing was ever placed for this phase (the spawn was lost with its client), so step back and
            // retry the same phase rather than skipping it.
            if (phase_spawned_total.Get() == 0 && currentPhase.Get() > 0)
            {
                currentPhase.Set(currentPhase.Get() - 1);
            }
            // force_next_phase short-circuits ShouldAdvancePhase, so the run resumes on the next Update.
            force_next_phase.Set(true);
            TouchProgressWatchdog();
        }

        // Owner-authoritative alive-count reconciliation. Walks the tracked ZDOIDs and asks ZDOMan whether
        // each ZDO still exists; any that no longer resolve have been destroyed network-wide (combat death,
        // remote removal) and are pruned. This is independent of which client owns each creature and of
        // whether a per-creature tracker component exists, which is what makes it robust to ZDO ownership
        // changes. Must only be called by the shrine owner.
        public void ReconcileSpawnedCreatures()
        {
            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner()) { return; }
            FlushSpawnedCreatures();
            var records = spawned_creature_records.Get();
            if (records.Count == 0)
            {
                if (spawned_creatures.Get() != 0) { spawned_creatures.Set(0); }
                return;
            }

            var survivors = new List<SpawnedCreatureRecord>(records.Count);
            var alive_by_prefab = new Dictionary<string, short>();
            foreach (var record in records)
            {
                ZDO zdo = ZDOMan.instance.GetZDO(record.Id);
                // A null ZDO means the creature has been destroyed everywhere on the network.
                if (zdo == null) { continue; }
                survivors.Add(record);
                if (alive_by_prefab.ContainsKey(record.Name)) { alive_by_prefab[record.Name] += 1; }
                else { alive_by_prefab[record.Name] = 1; }
            }

            if (survivors.Count != records.Count)
            {
                spawned_creature_records.Set(survivors);
                alive_creature_list.Set(alive_by_prefab);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Reconciled challenge creatures: {records.Count} -> {survivors.Count} alive."); }
            }
            if (spawned_creatures.Get() != survivors.Count) { spawned_creatures.Set(survivors.Count); }
            OnReconciled(survivors);
        }

        // Authoritative cleanup used when a run ends or is cancelled. Destroys every still-tracked creature
        // regardless of which client owns it: claim ownership of the local instance (or the bare ZDO) and
        // route through ZNetScene/ZDOMan so the removal propagates. Replaces the per-creature, owner-gated
        // CreatureTracker self-destruct. Must only be called by the shrine owner.
        public void DestroyAllSpawnedCreatures()
        {
            if (zNetView == null || !zNetView.IsValid() || !zNetView.IsOwner()) { return; }
            FlushSpawnedCreatures();
            foreach (var record in spawned_creature_records.Get())
            {
                GameObject instance = ZNetScene.instance.FindInstance(record.Id);
                if (instance != null)
                {
                    ZNetView creature_nview = instance.GetComponent<ZNetView>();
                    if (creature_nview != null && creature_nview.IsValid() && !creature_nview.IsOwner())
                    {
                        creature_nview.ClaimOwnership();
                    }
                    ZNetScene.instance.Destroy(instance);
                }
                else
                {
                    ZDO zdo = ZDOMan.instance.GetZDO(record.Id);
                    if (zdo != null)
                    {
                        // Take ownership before requesting destruction; ZDOMan.DestroyZDO no-ops for non-owners.
                        zdo.SetOwner(ZDOMan.GetSessionID());
                        ZDOMan.instance.DestroyZDO(zdo);
                    }
                }
            }
            spawned_creature_records.Set(new List<SpawnedCreatureRecord>());
            spawned_creatures.Set(0);
            alive_creature_list.Set(new Dictionary<string, short>());
            enemies.Clear();
        }

        public bool IsChallengeActive() {
            // Jotunn.Logger.LogInfo($"Checking if challenge is active: {challenge_active.Get()} | phase_spawn_in_flight {phase_spawn_in_flight.Get()}");
            return challenge_active.Get();
        }

        public bool ChallengeNoLongerSpawnable()
        {
            return challenge_active.Get() == false && wave_definition_ready.Get() == false;
        }

        public Boolean CentralPortalActiveStatus()
        {
            if (shrine_portal == null) {
                shrine_portal = gameObject.transform.Find("portal").gameObject;
            }
            return shrine_portal.activeSelf;
        }

        public void EnablePortal()
        {
            if (shrine_portal == null)
            {
                shrine_portal = gameObject.transform.Find("portal").gameObject;
            }
            shrine_portal.SetActive(true);
        }

        public void Disableportal()
        {
            if (shrine_portal == null)
            {
                shrine_portal = gameObject.transform.Find("portal").gameObject;
            }
            shrine_portal.SetActive(false);
        }

        public void addEnemy(GameObject enemy)
        {
            enemies.Add(enemy);
        }

        public void phaseCompleted()
        {
            phase_spawn_in_flight.Set(false);
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
            remote_spawn_locations.Set(spawn_points);
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
            // Authoritively remove any still-living challenge creatures (ForceSet above guarantees we own
            // the shrine ZDO at this point). Replaces the old per-creature, owner-gated CreatureTracker cleanup.
            DestroyAllSpawnedCreatures();
            Disableportal();
            RemoteLocationPortals.ClearMapOverlay();
            wave_phases_definitions = new PhasedWaveTemplate(); // Got to clear the template
            wave_definition_ready.Set(false);
            spawn_locations_ready.Set(false);
            phase_spawned_total.Set(0);
            phase_spawn_in_flight.Set(false);
            challenge_progress_time.Set(0);
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

        // Rewards are worthless if they land somewhere the players will never look, so never trust a
        // spawnpoint that failed to resolve -- fall back to the shrine itself.
        protected Vector3 ResolveRewardPosition(Vector3 spawn_position)
        {
            if (spawn_position == Vector3.zero)
            {
                Jotunn.Logger.LogWarning("Reward spawn position was unset; falling back to the shrine position.");
                return this.transform.position;
            }
            return spawn_position;
        }

        public void SpawnMultiRewardsDirectly(Dictionary<String, short> rewards_and_costs, short level, Vector3 spawn_position, bool hard_mode, bool boss_mode, bool siege_mode)
        {
            spawn_position = ResolveRewardPosition(spawn_position);
            float total_reward_points = RewardsData.DetermineRewardPoints(level, hard_mode, boss_mode, siege_mode, DetermineMultiplayerBonus());
            float equal_split_rewards_total = total_reward_points / rewards_and_costs.Count;
            foreach (KeyValuePair<String, short> reward_entry in rewards_and_costs)
            {
                short number_of_rewards = (short)(equal_split_rewards_total / reward_entry.Value);
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Spawning reward {number_of_rewards} {reward_entry.Key}"); }
                StartCoroutine(reward_controller.InitReward(reward_entry.Key, number_of_rewards, spawn_position));
            }
        }
        public void SpawnReward(Vector3 spawn_position)
        {
            spawn_position = ResolveRewardPosition(spawn_position);
            string reward_resource = selected_reward.Get();
            short number_of_rewards = RewardsData.DetermineRewardAmount(reward_resource, (short)selected_level_index.Get(), hard_mode.Get(), boss_mode.Get(), siege_mode.Get(), DetermineMultiplayerBonus());
            string reward_prefab = RewardsData.resourceRewards[reward_resource].resourcePrefab;
            StartCoroutine(reward_controller.InitReward(reward_prefab, number_of_rewards, spawn_position));
        }

        // Spawns rewards at exactly the supplied amounts, with no level/mode scaling. Used by the API
        // challenge runner for caller-defined "fixed" rewards. Keys are item prefab names, values are
        // the exact counts to spawn.
        public void SpawnFixedRewardsDirectly(Dictionary<String, short> rewards_and_amounts, Vector3 spawn_position)
        {
            if (rewards_and_amounts == null) { return; }
            spawn_position = ResolveRewardPosition(spawn_position);
            foreach (KeyValuePair<String, short> reward_entry in rewards_and_amounts)
            {
                if (reward_entry.Value <= 0) { continue; }
                if (VFConfig.EnableDebugMode.Value) { Jotunn.Logger.LogInfo($"Spawning fixed reward {reward_entry.Value} {reward_entry.Key}"); }
                StartCoroutine(reward_controller.InitReward(reward_entry.Key, reward_entry.Value, spawn_position));
            }
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

        public float GetHoverOffset() {
            // Hoverable gained this member in the Sept 2026 game update. Every vanilla implementer
            // returns a serialized m_hoverOffset; zero keeps the shrine's hover text where it has
            // always been. Throwing here would take out Player.UpdateHover the moment anyone looked
            // at a shrine.
            return 0f;
        }
    }
}
