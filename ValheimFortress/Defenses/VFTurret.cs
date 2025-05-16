using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimFortress.Challenge;

namespace ValheimFortress.Defenses
{
	public class VFTurret : MonoBehaviour, Hoverable, IPieceMarker
	{
		private static float m_turnRate = 80f;
		private static float m_horizontalAngle = 85f;
		private static float m_hitNoise = 10f;
		private static float m_shootWhenAimDiff = 0.99f; // 1 is perfect accuracy, we want to shoot when we are very close to dead center, leaving room for errors
		private static float m_predictionModifier = 1f;
		private static float m_updateTargetIntervalNear = 2f;
		private static float m_updateTargetIntervalFar = 8f;
        private static float m_aimDiffToTarget = -1f; // This needs to start out as a greater than zero value otherwise the turret will always immediately fire when it locks its first target
        public static float m_markerHideTime = 0.5f;

        public float m_viewDistance = VFConfig.BallistaRange.Value;
        public float m_attackCooldown = VFConfig.BallistaCooldownTime.Value;
        public float m_ammo_accuracy = VFConfig.BallistaAmmoAccuracyPenalty.Value; // Ammo will be perfectly accurate minus this percent, right now 95% accuracy
        private static int max_ticks_between_target_cache_update = VFConfig.BallistaTargetUpdateCacheInterval.Value;

		private static LayerMask lmsk;

        // These are all set later in time
        private GameObject m_Projectile;
		public ItemDrop.ItemData m_Ammo;
		private GameObject m_shootEffect;
		private GameObject m_reloadEffect;
		private GameObject m_newTargetEffect;
		private GameObject m_lostTargetEffect;
		private ZNetView m_nview;
		private Character m_target = null;
		private GameObject turretBodyArmed;
		private GameObject turretBodyUnarmed;
		private GameObject turretBodyArmedBolt;
		private GameObject turretBody;
		private GameObject turretNeck;
		private GameObject eye;
		private CircleProjector areaMarker;
		Quaternion m_baseBodyRotation;
		Quaternion m_baseNeckRotation;

		// These must be instanced
		private List<Character> nearby_targets = new List<Character>();
		private int update_target_cache_interval = 0;
        private int current_cache_check_tick = 0;
        private bool m_haveTarget = false;
		private float m_updateTargetTimer = 0f;
		private float m_scan = 0f;
        private float m_noTargetScanRate = 12f;

        private ZDOIDZNetProperty target { get; set; }


        protected void Awake()
		{
			// Jotunn.Logger.LogInfo("Setting ZNetView");
			m_nview = GetComponent<ZNetView>();
			if ((bool)m_nview)
			{
                //Jotunn.Logger.LogInfo("Setting RPC_SetTarget");
                // m_nview.Register<ZDOID>("RPC_SetTarget", RPC_SetTarget);
                target = new ZDOIDZNetProperty("VFTurret_Target", m_nview, ZDOID.None);
            }
			//Jotunn.Logger.LogInfo("Setting variable update timer");
			m_updateTargetTimer = UnityEngine.Random.Range(0f, m_updateTargetIntervalNear);

            if ((bool)m_nview)
            {
				GameObject rootNew = transform.Find("New").gameObject;
				GameObject turretBase = rootNew.transform.Find("Base").gameObject;
				GameObject neckRotation = rootNew.transform.Find("NeckRotation").gameObject;
				GameObject neck = neckRotation.transform.GetChild(0).gameObject; // Only one object ever under here
				//Jotunn.Logger.LogInfo("Setting TurretNeck");
				turretNeck = neckRotation;
				m_baseNeckRotation = turretNeck.transform.localRotation;

				//Jotunn.Logger.LogInfo("Setting TurretBody");
				GameObject bodyRotation = rootNew.transform.Find("BodyRotation").gameObject;
				turretBody = bodyRotation;
				m_baseBodyRotation = turretBody.transform.localRotation;

				//Jotunn.Logger.LogInfo("Setting Turret BodyArmed");
				turretBodyArmed = bodyRotation.transform.Find("Body").gameObject;
				turretBodyUnarmed = bodyRotation.transform.Find("Body_Unarmed").gameObject;
				turretBodyArmedBolt = bodyRotation.transform.Find("Bolt_Black_Metal").gameObject;

				//Jotunn.Logger.LogInfo("Setting TurretEye");
				eye = bodyRotation.transform.Find("Eye").gameObject;

				m_Projectile = PrefabManager.Instance.GetPrefab("TurretBolt");
				m_Ammo = m_Projectile.GetComponent<ItemDrop>().m_itemData;
				m_Ammo.m_shared.m_damages.m_pierce = VFConfig.BallistaDamage.Value;
                //Jotunn.Logger.LogInfo($"Set projectile to {m_Projectile.name}");
                //Jotunn.Logger.LogInfo($"Set projectile to {m_Ammo}");
                areaMarker = transform.Find("AreaMarker").gameObject.GetComponent<CircleProjector>();

				//Jotunn.Logger.LogInfo("Setting Effect Prefabs");
				m_shootEffect = PrefabManager.Instance.GetPrefab("fx_turret_fire");
				m_reloadEffect = PrefabManager.Instance.GetPrefab("fx_turret_reload");
				m_newTargetEffect = PrefabManager.Instance.GetPrefab("fx_turret_newtarget");
				m_lostTargetEffect = PrefabManager.Instance.GetPrefab("fx_turret_notarget");

                // Randomize which tick this turret uses for its update
                m_noTargetScanRate = (float)UnityEngine.Random.Range(8, 16);
                update_target_cache_interval = UnityEngine.Random.Range(1, VFConfig.BallistaTargetUpdateCacheInterval.Value);

				// Invert bit mask to check collisions
				// lmsk |= (1 << 0); // ignore default
                // lmsk |= (1 << 1); // ignore transparentFX
                // lmsk |= (1 << 2); // ignore raycast ignore
                // lmsk |= (1 << 9); // ignore characters
                lmsk = LayerMask.GetMask("Default", "TransparentFX", "character");
                lmsk = ~lmsk; // Invert default bitshift to avoid colliding with masked layers, but still collide with everything else
            }
		}

		private void FixedUpdate()
		{
			//Jotunn.Logger.LogInfo("Starting turret fixed update");
			float fixedDeltaTime = Time.fixedDeltaTime;
			UpdateMarker();
			if (m_nview.IsValid())
			{
                ConnectTargetIfSetAndInRange();
                UpdateTurretRotation(fixedDeltaTime);
                if (m_nview.IsOwner() && !IsCoolingDown())
                {
                    TurretRearmAnimate();

                    UpdateTarget(fixedDeltaTime);
                    ShootProjectile(fixedDeltaTime);
                }
			}
		}


        private void TurretRearmAnimate()
		{
            if (!turretBodyArmed.activeSelf)
            {
                Instantiate(m_reloadEffect, turretBodyArmed.transform.position, turretBodyArmed.transform.rotation);
                turretBodyArmed.SetActive(true);
                turretBodyArmedBolt.SetActive(true);
                turretBodyUnarmed.SetActive(false);
            }
        }

		private void UpdateTurretRotation(float fixedDeltaTime)
		{
            // if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Rotation Towards target? {(bool)m_target}"); }
            Vector3 forward;
			if ((bool)m_target)
			{
				float num = Vector2.Distance(m_target.transform.position, eye.transform.position) / (m_Ammo.m_shared.m_attack.m_projectileVel * 2);
				Vector3 vector = m_target.GetVelocity() * num * m_predictionModifier;
				forward = m_target.transform.position + vector - turretBody.transform.position;
				ref float y = ref forward.y;
				float num2 = y;
				CapsuleCollider targetCollider = m_target.GetComponentInChildren<CapsuleCollider>();
				y = num2 + (((object)targetCollider != null) ? (targetCollider.height / 2f) : 1f);
			}
			else
			{
				m_scan += fixedDeltaTime;
				if (m_scan > m_noTargetScanRate * 2f)
				{
					m_scan = 0f;
				}
				forward = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y + (float)((m_scan - m_noTargetScanRate > 0f) ? 1 : (-1)) * (m_horizontalAngle/2), 0f) * Vector3.forward;
			}
			forward.Normalize();
			Quaternion quaternion = Quaternion.LookRotation(forward, Vector3.up);
			Vector3 eulerAngles = quaternion.eulerAngles;
			float y2 = base.transform.rotation.eulerAngles.y;
			eulerAngles.y -= y2;
			if (m_horizontalAngle >= 0f)
			{
				float num3 = eulerAngles.y;
				if (num3 > 180f)
				{
					num3 -= 360f;
				}
				else if (num3 < -180f)
				{
					num3 += 360f;
				}
				if (num3 > m_horizontalAngle)
				{
					eulerAngles = new Vector3(eulerAngles.x, m_horizontalAngle + y2, eulerAngles.z);
					quaternion.eulerAngles = eulerAngles;
				}
				else if (num3 < 0f - m_horizontalAngle)
				{
					eulerAngles = new Vector3(eulerAngles.x, 0f - m_horizontalAngle + y2, eulerAngles.z);
					quaternion.eulerAngles = eulerAngles;
				}
			}
			Quaternion quaternion2 = Quaternion.RotateTowards(turretBody.transform.rotation, quaternion, m_turnRate * fixedDeltaTime);
			turretBody.transform.rotation = m_baseBodyRotation * quaternion2;
			//Jotunn.Logger.LogInfo($"Turret Rotation {turretBody.transform.rotation}.");
			turretNeck.transform.rotation = m_baseNeckRotation * Quaternion.Euler(0f, turretBody.transform.rotation.eulerAngles.y, turretBody.transform.rotation.eulerAngles.z);
			//Jotunn.Logger.LogInfo($"has_target:{has_target} {Quaternion.Dot(quaternion2, quaternion)} or 2f");
			m_aimDiffToTarget = (m_haveTarget ? Math.Abs(Quaternion.Dot(quaternion2, quaternion)) : -1f);
		}

		private void UpdateTarget(float dt)
		{
            // No need to update our target if we already have a target
            // if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Already has target? {m_haveTarget}"); }
			// This gets cleared out when the target dies, since the object reference will break
            if ((bool)m_target) { return; }
			//Jotunn.Logger.LogInfo("Updating Target");
			m_updateTargetTimer -= dt;
			if (m_updateTargetTimer <= 0f)
			{
				bool character_in_range = IsCharacterInRangeAndNotPlayer(base.transform.position, VFConfig.BallistaRange.Value, Character.GetAllCharacters());
                m_updateTargetTimer = (character_in_range ? m_updateTargetIntervalNear : m_updateTargetIntervalFar);
				// Character character = BaseAI.FindClosestCreature(base.transform, eye.transform.position, 0f, m_viewDistance, m_horizontalAngle, false, false);
				// Id much prefer to get characters in range, but we need a complete list of all valid targets, instead of just avoiding the things we don't want to hit
				Character selectedTarget = selectTarget(character_in_range);

				if (selectedTarget != null && selectedTarget != m_target)
				{
					if ((bool)selectedTarget)
					{
						Instantiate(m_newTargetEffect, base.transform.position, base.transform.rotation);
					}
					else
					{
						Instantiate(m_lostTargetEffect, base.transform.position, base.transform.rotation);
					}
                    if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"set target {selectedTarget}"); }
                    // m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", selectedTarget ? selectedTarget.GetZDOID() : ZDOID.None);
                    target.Set(selectedTarget ? selectedTarget.GetZDOID() : ZDOID.None);
                    m_target = selectedTarget;
                    m_haveTarget = true;
                }
			}
			if (m_haveTarget && (!m_target || m_target.IsDead()))
			{
				// Jotunn.Logger.LogInfo("Target is dead, clearing target.");
				// m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", ZDOID.None);
				target.Set(ZDOID.None);
                m_haveTarget = false;
                m_scan = 0f;
                Instantiate(m_lostTargetEffect, base.transform.position, base.transform.rotation);
			}
		}

		// This is just IsCharacterInRange with an added avoidance for characters, to prevent turrets from going into high alert mode when the player and/or friends are nearby but no enemies are
        public static bool IsCharacterInRangeAndNotPlayer(Vector3 point, float range, List<Character> character_list)
        {
            foreach (Character s_character in character_list)
            {
                if (Vector3.Distance(s_character.transform.position, point) < range)
                {
					if (s_character.IsPlayer()) { continue; }
                    if (s_character.GetFaction() == 0 || s_character is Player) { continue; }
                    return true;
                }
            }
            return false;
        }

        private bool IsValidTarget(Character ptarget)
		{
			// This is a stale character and no longer exists if its null
			if (ptarget == null) { return false; }
			// Dead, tames and other players are not valid targets
			if (ptarget.IsDead() || ptarget.IsTamed() || ptarget.IsPlayer()) { return false; }
			// Ballista does not automatically target passives
            if (!VFConfig.BallistaTargetsPassives.Value && (int)ptarget.GetFaction() == 1) { return false; }
            // Faction 0 (player faction) and the local player is not a target either
            if (ptarget.GetFaction() == 0 || ptarget is Player) { return false; }
			//Jotunn.Logger.LogInfo("Found valid target");
			return true;
		}

		private Character selectTarget(bool character_in_range)
		{
			// if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Selecting Target"); }
			//Jotunn.Logger.LogInfo("selecting target");
			// List<Character> potentialTargets = Character.GetAllCharacters();
			//List<Character> potentialTargets = new List<Character>();

			//if (character_in_range)
			//{
			//	if (update_target_cache_interval == current_cache_check_tick)
			//	{
			//                 Character.GetCharactersInRange(base.transform.position, m_viewDistance, nearby_targets);
			//             } else
			//	{
			//		current_cache_check_tick++;
			//             }
			//         }


			// nothing is in range, nothing to do.
			if (!character_in_range) { return null; }

            // rebuild the list of targets to check once we've emptied out our current target list OR we've hit the cache update
            if (nearby_targets.Count == 0 || update_target_cache_interval == current_cache_check_tick)
			{
                if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Updating Turret target cache."); }
                Character.GetCharactersInRange(base.transform.position, m_viewDistance, nearby_targets);
				// Order the list of targets by whoever is closest
				nearby_targets.OrderBy(o => Vector3.Distance(base.transform.position, o.gameObject.transform.position));

            }
            List<Character> targets_to_check = new List<Character>(nearby_targets);
            // Reset the cache tick, or increment it
            if (max_ticks_between_target_cache_update <= current_cache_check_tick) { current_cache_check_tick = 0; } else { current_cache_check_tick++; }

            Character selectedTarget = null;
            if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"checking targets {targets_to_check.Count}"); }
            foreach (Character ptarget in targets_to_check)
			{
				if (!IsValidTarget(ptarget))
				{
                    // if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Removing invalid target"); }
                    // Remove invalid entries so we don't need to recheck them in the future
                    nearby_targets.Remove(ptarget);
					continue;
				}
				BaseAI ptargetAI = ptarget.GetBaseAI();
				if (ptargetAI.IsSleeping()) { continue; }
				float distance_to_ptarget = Vector3.Distance(base.transform.position, ptarget.transform.position);
				if (distance_to_ptarget < m_viewDistance)
				{
					// if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Checking for target visual."); }
					// This raycast is only triggered if the target is within range.
					//  This verifies if the target is VISUALLY targetable aka no more psychic ballista
					RaycastHit rayhit;
					Vector3 direction = (ptarget.transform.position - eye.transform.position).normalized;
					bool did_raycast_hit = Physics.Raycast(eye.transform.position, direction, out rayhit, m_viewDistance, lmsk);
					float raycast_distance_to_hit = Vector3.Distance(eye.transform.position, ptarget.transform.position);
					bool rayshot_hit_distance = rayhit.distance > (raycast_distance_to_hit - 2);
					if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"TargetCheck: distance to target: {raycast_distance_to_hit}, raycast distance test: {rayhit.distance}, can hit distance: {rayshot_hit_distance}"); }
					// rayshot did not travel far enough to potentially hit the target, it collided with something before hitting the target
					if (!rayshot_hit_distance) { continue; }

					selectedTarget = ptarget;
					break;
				}
			}
			if(selectedTarget != null && VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Selected target: {selectedTarget}");  }

            return selectedTarget;
		}

		public void ShootProjectile(float dt)
		{
            // We only fire a shot if we are ready to do so, aka has target, can aim at it, and is ready to fire
            // Jotunn.Logger.LogInfo($"m_aimDiffToTarget {m_aimDiffToTarget} > m_shootWhenAimDiff {m_shootWhenAimDiff} ({!(m_aimDiffToTarget > m_shootWhenAimDiff)})");
            if (!m_target || !(m_aimDiffToTarget > m_shootWhenAimDiff) || IsCoolingDown())
            {
                return;
			}
			// Disabling this means lots of distruction, but also a lot more shooting.
			if (VFConfig.BallistaEnableShotSafetyCheck.Value)
			{
                RaycastHit rayhit;
                bool did_raycast_hit = Physics.Raycast(eye.transform.position, eye.transform.forward, out rayhit, m_viewDistance, lmsk);
				Vector3 raycast_start_pos = eye.transform.position;
				raycast_start_pos.z += 0.5f; // offset the starting point a little bit forward
                float raycast_distance_to_hit = Vector3.Distance(raycast_start_pos, m_target.transform.position);
                bool rayshot_hit_distance = rayhit.distance > (raycast_distance_to_hit - 2);
                if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($" distance to target: {raycast_distance_to_hit}, raycast distance test: {rayhit.distance}, can hit distance: {rayshot_hit_distance} hit bool: {did_raycast_hit}"); }
                if (!rayshot_hit_distance) { return; }
				// if (!did_raycast_hit) { return; }
            }

            // This is really noisy
            if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Turret target status:{!(bool)m_target} aimdiff:{m_aimDiffToTarget} > {m_shootWhenAimDiff} ({!(m_aimDiffToTarget > m_shootWhenAimDiff)}) cooldown:{IsCoolingDown()}"); }
			Instantiate(m_shootEffect, turretBodyArmed.transform.position, eye.transform.rotation);
			m_nview.GetZDO().Set("lastAttack", (float)ZNet.instance.GetTimeSeconds());
			{
				Vector3 forward = eye.transform.forward;
				Vector3 axis = Vector3.Cross(forward, Vector3.up);

				Quaternion quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0f - m_ammo_accuracy, m_ammo_accuracy), Vector3.up);
				forward = Quaternion.AngleAxis(UnityEngine.Random.Range(0f - m_ammo_accuracy, m_ammo_accuracy), axis) * forward;
				forward = quaternion * forward;
				GameObject projectile = Instantiate(m_Ammo.m_shared.m_attack.m_attackProjectile, eye.transform.position, eye.transform.rotation);
				HitData hitData = new HitData();
				hitData.m_pushForce = m_Ammo.m_shared.m_attackForce;
				hitData.m_backstabBonus = m_Ammo.m_shared.m_backstabBonus;
				hitData.m_staggerMultiplier = m_Ammo.m_shared.m_attack.m_staggerMultiplier;
				hitData.m_damage.Add(m_Ammo.GetDamage());
				hitData.m_blockable = m_Ammo.m_shared.m_blockable;
				hitData.m_dodgeable = m_Ammo.m_shared.m_dodgeable;
				hitData.m_skill = m_Ammo.m_shared.m_skillType;
				IProjectile component = projectile.GetComponent<IProjectile>();
				if (component != null)
				{
					component.Setup(null, forward * (m_Ammo.m_shared.m_attack.m_projectileVel * 3), m_hitNoise, hitData, null, m_Ammo);
				}
			}
			turretBodyArmed.SetActive(false);
			turretBodyArmedBolt.SetActive(false);
			turretBodyUnarmed.SetActive(true);
		}

		public bool IsCoolingDown()
		{
			// Jotunn.Logger.LogInfo("Checking cooldown");
			if (!m_nview.IsValid())
			{
				return false;
			}
			return (double)(m_nview.GetZDO().GetFloat("lastAttack") + m_attackCooldown) > ZNet.instance.GetTimeSeconds();
		}

		public string GetHoverText()
		{
			if (!m_nview.IsValid())
			{
				return "";
			}
			return Localization.instance.Localize("$piece_vfturret");
		}

		public string GetHoverName()
		{
			return Localization.instance.Localize("$piece_vfturret");
		}

        private void ConnectTargetIfSetAndInRange()
        {
			if (target.Get() != ZDOID.None && m_haveTarget == false)
			{
                // Jotunn.Logger.LogInfo("Recieving target from ZDO");
                GameObject gameObject = ZNetScene.instance.FindInstance(target.Get());
                // Jotunn.Logger.LogInfo($"Found target in the scene? {(bool)gameObject}");
                if ((bool)gameObject) {
					bool target_within_range = Vector3.Distance(eye.transform.position, gameObject.transform.position) > m_viewDistance;
                    // Jotunn.Logger.LogInfo($"ZDO target is within range? ({target_within_range})");
                    // We don't take the group target if its outside of our shooting range
                    if (target_within_range)
					{
                        Character component = gameObject.GetComponent<Character>();
                        // Jotunn.Logger.LogInfo($"Target is a valid character? {(object)component != null}");
                        if ((object)component != null)
                        {
                            // Jotunn.Logger.LogInfo("Setting Target From ZDO");
                            m_target = component;
                            m_haveTarget = true;
                            return;
                        }
                    }
                } else
				{
                    // Jotunn.Logger.LogInfo("Clear target due to being unable to find the target in the scene");
                    target.ForceSet(ZDOID.None);
                    m_target = null;
                    m_haveTarget = false;
                    m_scan = 0f;
                }
            } 
			//else {
			//	// No target is available to target
			//	// Its possible that we get here and have a target but ZDOID is none
			//	// In that case we want to clear out our target, as its likely dead.
			//	if (target.Get() == ZDOID.None)
			//	{
   //                 Jotunn.Logger.LogInfo("Clear target due to empty ZDO");
   //                 m_target = null;
   //                 m_haveTarget = false;
   //                 m_scan = 0f;
   //             }
   //         }
            
        }

        private void OnDestroyed()
		{
			GetComponent<WearNTear>().m_onDestroyed();
		}

		public void ShowHoverMarker()
		{
			ShowBuildMarker();
		}

		public void ShowBuildMarker()
		{
			if (!(bool)areaMarker)
			{
				areaMarker.m_radius = m_viewDistance;
				areaMarker.gameObject.SetActive(false);
			}
			if ((bool)areaMarker)
			{
				areaMarker.gameObject.SetActive(true);
				CancelInvoke("HideMarker");
				Invoke("HideMarker", m_markerHideTime);
			}
		}

		private void UpdateMarker()
		{
			if ((bool)areaMarker && areaMarker.isActiveAndEnabled)
			{
				areaMarker.m_start = base.transform.rotation.eulerAngles.y - m_horizontalAngle;
				areaMarker.m_turns = m_horizontalAngle * 2f / 360f;
			}
		}

		private void HideMarker()
		{
			if ((bool)areaMarker)
			{
				areaMarker.gameObject.SetActive(false);
			}
		}
	}
}