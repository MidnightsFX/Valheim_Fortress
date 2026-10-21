using Jotunn.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimFortress.Challenge
{
    // API-driven challenge runner. This is hosted on a lightweight, registered, networked prefab
    // (VF_api_challenge_runner) that the public API instantiates at runtime. It reuses the full
    // GenericShrine machinery (phase progression, ZDOID creature tracking/reconciliation, reward
    // spawning, multiplayer state sync) but, unlike the physical shrines, takes its wave template,
    // spawn points, and reward location from the API caller instead of a shrine config. It owns no
    // portal/spawnpoint children and self-destroys once the run completes.
    internal class ExternalShrine : GenericShrine
    {
        // ZDO keys and the stop RPC read by APIReceiver's handle methods (GetChallengeState, StopChallenge).
        internal const string StartKey = "shrine_start_challenge";
        internal const string ActiveKey = "shrine_challenge_active";
        internal const string PhaseKey = "shrine_current_phase";
        internal const string AliveKey = "spawned_creatures";
        internal const string TotalPhasesKey = "api_total_phases";
        internal const string CompletedKey = "api_completed";
        internal const string StoppedKey = "api_stopped";
        internal const string StopRequestedKey = "api_stop_requested";
        internal const string StopRpc = "VF_ApiStopChallenge";

        // How long a finished runner stays, so its rewards finish spawning and handle holders can read the outcome.
        private const int EndedRunnerSeconds = 30;

        // Reward data is held on the ZDO so a new owner can finish the run if the owning region changes
        // mid-challenge (full shrine parity). Keys are item prefab names.
        private Vector3ZNetProperty api_reward_location;
        private DictionaryZNetProperty api_scaled_rewards;
        private DictionaryZNetProperty api_fixed_rewards;

        // Whether challenge creatures keep their normal loot drops. Held on the ZDO so the spawning/reconnect
        // logic honors it regardless of which instance owns the run. Defaults to false (no drops), matching
        // the physical shrines. Per-creature overrides (creature name -> 1/0) take precedence over the global
        // toggle; creatures without an override fall back to it.
        private BoolZNetProperty api_creature_drops_enabled;
        private DictionaryZNetProperty api_creature_drop_overrides;

        // Player-facing messages are transient and only used by the owner that started the run.
        private string api_wave_start_msg;
        private string api_wave_end_msg;

        // Caller-supplied between-wave phrases (each a $localization key or literal). Held on the ZDO so the
        // phrases, the ordered/random flag, and the sequential cursor survive an ownership change mid-run
        // (same durability rationale as the reward/mode state above). Empty list -> built-in phrase pool.
        private ListStringZNetProperty api_between_wave_phrases;
        private BoolZNetProperty api_ordered_phrases;
        private IntZNetProperty api_phase_message_index;

        // Whether to mark the spawn points on the minimap for the duration of the run. Transient: the overlay
        // is client-local and only drawn/cleared by the owner that drives the run (see DrawSpawnLocationOverlay).
        private bool api_draw_map_overlay;

        // Schema 4 run state, all ZDO-backed so any owner (and any handle holder) sees it: whether to open spawn
        // portals, the run's phase count, how it ended, a pending stop request, and when it ended (ZNet seconds).
        private BoolZNetProperty api_spawn_portals;
        private IntZNetProperty api_total_phases;
        private BoolZNetProperty api_completed;
        private BoolZNetProperty api_stopped;
        private BoolZNetProperty api_stop_requested;
        private IntZNetProperty api_ended_at;

        public override void Awake()
        {
            // The runner prefab carries its own ZNetView (added by PrefabManager.CreateEmptyPrefab),
            // so unlike WildShrine/the location shrines we can wire everything up immediately.
            if (this.gameObject.TryGetComponent<ZNetView>(out zNetView) == false) { return; }
            if ((bool)zNetView == false) { return; }

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

            alive_creature_list = new DictionaryZNetProperty("alive_creature_list", zNetView, new Dictionary<String, short>() { });
            spawned_creature_records = new SpawnedCreatureRecordsZNetProperty("spawned_creature_records", zNetView, new List<SpawnedCreatureRecord>());

            api_reward_location = new Vector3ZNetProperty("api_reward_location", zNetView, Vector3.zero);
            api_scaled_rewards = new DictionaryZNetProperty("api_scaled_rewards", zNetView, new Dictionary<String, short>() { });
            api_fixed_rewards = new DictionaryZNetProperty("api_fixed_rewards", zNetView, new Dictionary<String, short>() { });
            api_creature_drops_enabled = new BoolZNetProperty("api_creature_drops_enabled", zNetView, false);
            api_creature_drop_overrides = new DictionaryZNetProperty("api_creature_drop_overrides", zNetView, new Dictionary<String, short>() { });
            api_between_wave_phrases = new ListStringZNetProperty("api_between_wave_phrases", zNetView, new List<string>() { });
            api_ordered_phrases = new BoolZNetProperty("api_ordered_phrases", zNetView, false);
            api_phase_message_index = new IntZNetProperty("api_phase_message_index", zNetView, 0);
            api_spawn_portals = new BoolZNetProperty("api_spawn_portals", zNetView, false);
            api_total_phases = new IntZNetProperty(TotalPhasesKey, zNetView, 0);
            api_completed = new BoolZNetProperty(CompletedKey, zNetView, false);
            api_stopped = new BoolZNetProperty(StoppedKey, zNetView, false);
            api_stop_requested = new BoolZNetProperty(StopRequestedKey, zNetView, false);
            api_ended_at = new IntZNetProperty("api_ended_at", zNetView, 0);
            zNetView.Register(StopRpc, RPC_StopChallenge);

            // Reuse the same wave-config RPC the other shrines use; Jotunn returns the existing RPC for a
            // repeated name so registering it again here is safe.
            WaveDefinitionRPC = NetworkManager.Instance.AddRPC("VF_levelsyaml_rpc", VFConfig.OnServerRecieveConfigs, OnClientReceivePhaseConfigs);

            spawn_controller = this.gameObject.GetComponent<Spawner>();
            if (spawn_controller == null) { spawn_controller = this.gameObject.AddComponent<Spawner>(); }
            availablePhases = 0;
        }

        // Entry point used by APIReceiver immediately after the runner prefab is instantiated. Stores
        // all run state (already-generated wave template, caller spawn points + reward location, reward
        // tables, difficulty/modes) and flags the run to begin. The owning instance's Update loop picks
        // it up and starts the challenge.
        public void BeginApiChallenge(PhasedWaveTemplate waves, Vector3[] spawnPoints, Vector3 rewardLocation,
            Dictionary<String, short> scaledRewards, Dictionary<String, short> fixedRewards,
            short difficulty, bool hard, bool boss, bool siege, bool enableCreatureDrops,
            Dictionary<String, short> creatureDropOverrides, string startMessage, string endMessage,
            bool drawMapOverlay, List<string> betweenWavePhrases, bool orderedPhrases, bool spawnPortals)
        {
            wave_phases_definitions = waves;
            availablePhases = waves.hordePhases.Count;
            api_total_phases.ForceSet(availablePhases);
            api_spawn_portals.ForceSet(spawnPortals);

            SetWaveSpawnPoints(spawnPoints);
            wave_definition_ready.ForceSet(true);

            api_reward_location.ForceSet(rewardLocation);
            api_scaled_rewards.ForceSet(scaledRewards ?? new Dictionary<String, short>() { });
            api_fixed_rewards.ForceSet(fixedRewards ?? new Dictionary<String, short>() { });

            selected_level_index.ForceSet(difficulty);
            selected_level.ForceSet(difficulty);
            hard_mode.ForceSet(hard);
            boss_mode.ForceSet(boss);
            siege_mode.ForceSet(siege);
            api_creature_drops_enabled.ForceSet(enableCreatureDrops);
            api_creature_drop_overrides.ForceSet(creatureDropOverrides ?? new Dictionary<String, short>() { });

            api_wave_start_msg = startMessage;
            api_wave_end_msg = endMessage;
            api_draw_map_overlay = drawMapOverlay;
            api_between_wave_phrases.ForceSet(betweenWavePhrases ?? new List<string>() { });
            api_ordered_phrases.ForceSet(orderedPhrases);
            api_phase_message_index.ForceSet(0);

            start_challenge.ForceSet(true);
        }

        public override void StartChallengeMode()
        {
            currentPhase.Set(0);
            challenge_active.Set(true);
            BeginPhaseSpawn();
            // The runner has no portals of its own. When the caller opts in, open the shrines' spawn portals at the
            // spawn points (they close themselves when the run ends) and/or mark the points on the minimap (cleared
            // in FinishChallenge). The map marks are local/cosmetic and a no-op without a minimap.
            if (api_spawn_portals.Get())
            {
                RemoteLocationPortals.DrawMapOverlayAndPortals(remote_spawn_locations.Get(), this, api_draw_map_overlay);
            }
            else if (api_draw_map_overlay)
            {
                RemoteLocationPortals.DrawSpawnLocationOverlay(remote_spawn_locations.Get(), this.gameObject.transform.position);
            }
            spawn_controller.TrySpawningPhase(5f, false, wave_phases_definitions.hordePhases[currentPhase.Get()], gameObject, remote_spawn_locations.Get());
            SetCurrentCreatureList(wave_phases_definitions.hordePhases[currentPhase.Get()]);
            start_challenge.Set(false);
            currentPhase.Set(currentPhase.Get() + 1);
            AnnounceToNearbyPlayers(api_wave_start_msg);
            Jotunn.Logger.LogInfo($"API challenge started. Level: {selected_level.Get()} Phases: {availablePhases}");
        }

        public override void Update()
        {
            // No-op until the prefab's ZNetView is live (set up in Awake on instantiate).
            if (zNetView == null || zNetView.IsValid() != true) { return; }

            if (spawn_controller == null) { spawn_controller = this.gameObject.GetComponent<Spawner>(); }

            // Everything below is owner-authoritative, identical to the physical shrines.
            NoteOwnershipState(zNetView.IsOwner());
            if (!zNetView.IsOwner()) { return; }

            // A finished or stopped run waits a moment, then whoever owns it then removes it.
            if (api_ended_at.Get() > 0)
            {
                if (ZNet.instance != null && (int)ZNet.instance.GetTimeSeconds() - api_ended_at.Get() >= EndedRunnerSeconds)
                {
                    ZNetScene.instance.Destroy(gameObject);
                }
                return;
            }
            if (api_stop_requested.Get())
            {
                StopChallenge();
                return;
            }

            // Kick off the challenge once BeginApiChallenge has staged the data.
            if (start_challenge.Get() == true)
            {
                if (wave_definition_ready.Get() == true && spawn_locations_ready.Get() == true)
                {
                    SendUpdatedPhaseConfigs();
                    StartChallengeMode();
                }
                return;
            }

            if (challenge_active.Get() == true)
            {
                // Authoritatively reconcile the alive count from tracked ZDOIDs (throttled). This is what
                // advances phases as creatures die, independent of creature/shrine ZDO ownership.
                ReconcileIfDue();
                CheckProgressWatchdog();

                // Lost the wave definition (e.g. ownership transfer before the RPC arrived). We can't
                // reconstruct an API-supplied wave, so finish gracefully and still grant rewards.
                if (wave_phases_definitions == null)
                {
                    Jotunn.Logger.LogWarning("API challenge runner lost its wave definition; finishing the run.");
                    FinishChallenge();
                    return;
                }

                if (wave_phases_definitions.hordePhases != null && wave_phases_definitions.hordePhases.Count > 0)
                {
                    // We need to have spawned creatures, and none of them remaining, before advancing.
                    if (ShouldAdvancePhase())
                    {
                        if (RemainingPhases())
                        {
                            should_add_creature_beacons.Set(false);
                            force_next_phase.Set(false);
                            int current_phase = currentPhase.Get();
                            BeginPhaseSpawn();
                            spawn_controller.TrySpawningPhase(10f, true, wave_phases_definitions.hordePhases[current_phase], gameObject, remote_spawn_locations.Get());
                            SetCurrentCreatureList(wave_phases_definitions.hordePhases[current_phase]);
                            int max_wave_phase = wave_phases_definitions.hordePhases.Count;
                            int expected_next_phase = currentPhase.Get() + 1;
                            currentPhase.Set(max_wave_phase <= expected_next_phase ? max_wave_phase : expected_next_phase);
                        }
                        else
                        {
                            FinishChallenge();
                        }
                    }
                }
            }
        }

        // Grants the configured rewards at the caller's reward location, tears down all challenge state,
        // and schedules the runner object for removal.
        private void FinishChallenge()
        {
            Jotunn.Logger.LogInfo("API challenge complete! Spawning rewards.");
            AnnounceToNearbyPlayers(string.IsNullOrEmpty(api_wave_end_msg) ? "$shrine_challenge_complete" : api_wave_end_msg);
            api_completed.Set(true);

            Vector3 reward_pos = api_reward_location.Get();
            Dictionary<String, short> scaled = api_scaled_rewards.Get();
            if (scaled != null && scaled.Count > 0)
            {
                SpawnMultiRewardsDirectly(scaled, (short)selected_level_index.Get(), reward_pos, hard_mode.Get(), boss_mode.Get(), siege_mode.Get());
            }
            Dictionary<String, short> fixed_rewards = api_fixed_rewards.Get();
            if (fixed_rewards != null && fixed_rewards.Count > 0)
            {
                SpawnFixedRewardsDirectly(fixed_rewards, reward_pos);
            }

            EndRun();
        }

        // A stop from the API (StopChallenge): the run ends without rewards. Owner only.
        private void StopChallenge()
        {
            if (api_ended_at.Get() > 0) { return; }
            Jotunn.Logger.LogInfo("API challenge stopped by its caller.");
            api_stop_requested.Set(false);
            api_stopped.Set(true);
            EndRun();
        }

        private void RPC_StopChallenge(long sender)
        {
            if (zNetView == null || !zNetView.IsValid()) { return; }
            if (zNetView.IsOwner()) { StopChallenge(); }
            else { api_stop_requested.ForceSet(true); }
        }

        // Tears down all challenge state; the runner object is removed a little later (see Update).
        private void EndRun()
        {
            challenge_active.Set(false);
            start_challenge.Set(false);
            RemoteLocationPortals.ClearMapOverlay();
            DestroyAllSpawnedCreatures();
            boss_mode.Set(false);
            hard_mode.Set(false);
            siege_mode.Set(false);
            force_next_phase.Set(false);
            wave_phases_definitions = new PhasedWaveTemplate() { hordePhases = new List<List<HoardConfig>> { } };
            SendUpdatedPhaseConfigs();
            currentPhase.Set(0);
            wave_definition_ready.Set(false);
            spawn_locations_ready.Set(false);
            phase_spawned_total.Set(0);
            phase_spawn_in_flight.Set(false);
            challenge_progress_time.Set(0);

            // Give the reward/cleanup coroutines a moment to finish before removing the runner object. Kept on the
            // ZDO so a new owner still removes it if ownership moves in the meantime.
            api_ended_at.Set(ZNet.instance != null ? Math.Max(1, (int)ZNet.instance.GetTimeSeconds()) : 1);
        }

        private void AnnounceToNearbyPlayers(string message)
        {
            if (string.IsNullOrEmpty(message)) { return; }
            List<Player> nearby_players = new List<Player> { };
            Player.GetPlayersInRange(this.transform.position, VFConfig.ShrineAnnouncementRange.Value, nearby_players);
            foreach (Player localplayer in nearby_players)
            {
                localplayer.Message(MessageHud.MessageType.Center, Localization.instance.Localize(message));
            }
        }

        // Unlike the physical shrines, loot drops are an API-supplied per-run setting: a global toggle
        // (default false) with optional per-creature overrides. A creature listed in the override map uses
        // its mapped value; everything else falls back to the global toggle.
        public override bool ShouldDropLoot(string creature)
        {
            Dictionary<String, short> overrides = api_creature_drop_overrides.Get();
            if (overrides != null && overrides.TryGetValue(creature, out short value))
            {
                return value != 0;
            }
            return api_creature_drops_enabled.Get();
        }

        // Between-wave phrase for this run. Returns null (use the built-in pool) when the caller supplied none.
        // Ordered mode walks the list in order and wraps; random mode picks any entry. The cursor is ZDO-backed
        // so ordering stays correct if ownership transfers mid-run.
        public override string SelectPhasePauseMessage()
        {
            List<string> phrases = api_between_wave_phrases.Get();
            if (phrases == null || phrases.Count == 0) { return null; }
            if (api_ordered_phrases.Get())
            {
                int idx = api_phase_message_index.Get() % phrases.Count;
                api_phase_message_index.Set(api_phase_message_index.Get() + 1);
                return phrases[idx];
            }
            return phrases[UnityEngine.Random.Range(0, phrases.Count)];
        }

        // The runner has no world-interaction surface; it is spawned and driven entirely by the API.
        public override string GetHoverName() { return ""; }

        public override string GetHoverText() { return ""; }

        public override bool Interact(Humanoid user, bool hold, bool alt) { return false; }
    }
}
