using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ValheimFortress;

namespace ValheimFortress.Defenses
{
	public class VFTurret : MonoBehaviour, Hoverable, IPieceMarker
	{
		private static float m_turnRate = 80f;
		private static float m_horizontalAngle = 85f;
		private static float m_viewDistance = 25f;
		private static float m_noTargetScanRate = 12f;
		private static float m_attackCooldown = 2f;
		private static float m_hitNoise = 10f;
		private static float m_shootWhenAimDiff = 0.99f; // 1 is perfect accuracy, we want to shoot when we are very close to dead center, leaving room for errors
		private static float m_ammo_accuracy = 0.05f; // Ammo will be perfectly accurate minus this percent, right now 95% accuracy
		private static float m_predictionModifier = 1f;
		private static float m_updateTargetIntervalNear = 2f;
		private static float m_updateTargetIntervalFar = 8f;
		// private CircleProjector areaMarker;
		public static float m_markerHideTime = 0.5f;
		// These are all set later in time
		private GameObject m_Projectile;
		private ItemDrop.ItemData m_Ammo;
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
		private static bool m_haveTarget = false;
		private static float m_aimDiffToTarget = -1f; // This needs to start out as a greater than zero value otherwise the turret will always immediately fire when it locks its first target
		private static float m_updateTargetTimer = 0f;
		private static float m_scan = 0f;

		protected void Awake()
		{
			// Jotunn.Logger.LogInfo("Setting ZNetView");
			m_nview = GetComponent<ZNetView>();
			if ((bool)m_nview)
			{
				//Jotunn.Logger.LogInfo("Setting RPC_SetTarget");
				m_nview.Register<ZDOID>("RPC_SetTarget", RPC_SetTarget);
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
				turretBodyArmedBolt = bodyRotation.transform.Find("Bolt Black Metal").gameObject;

				//Jotunn.Logger.LogInfo("Setting TurretEye");
				eye = bodyRotation.transform.Find("Eye").gameObject;

				m_Projectile = PrefabManager.Instance.GetPrefab("TurretBolt");
				m_Ammo = m_Projectile.GetComponent<ItemDrop>().m_itemData;
				//Jotunn.Logger.LogInfo($"Set projectile to {m_Projectile.name}");
				//Jotunn.Logger.LogInfo($"Set projectile to {m_Ammo}");
				areaMarker = transform.Find("AreaMarker").gameObject.GetComponent<CircleProjector>();

				//Jotunn.Logger.LogInfo("Setting Effect Prefabs");
				m_shootEffect = PrefabManager.Instance.GetPrefab("fx_turret_fire");
				m_reloadEffect = PrefabManager.Instance.GetPrefab("fx_turret_reload");
				m_newTargetEffect = PrefabManager.Instance.GetPrefab("fx_turret_newtarget");
				m_lostTargetEffect = PrefabManager.Instance.GetPrefab("fx_turret_notarget");
			}
		}

		private void FixedUpdate()
		{
			//Jotunn.Logger.LogInfo("Starting turret fixed update");
			float fixedDeltaTime = Time.fixedDeltaTime;
			UpdateMarker(fixedDeltaTime);
			if (m_nview.IsValid())
			{
				UpdateTurretRotation();
				if (m_nview.IsOwner() && !IsCoolingDown())
				{
                    if (!turretBodyArmed.activeSelf)
                    {
						UnityEngine.Object.Instantiate(m_reloadEffect, turretBodyArmed.transform.position, turretBodyArmed.transform.rotation);
						turretBodyArmed.SetActive(true);
						turretBodyArmedBolt.SetActive(true);
						turretBodyUnarmed.SetActive(false);
					}

					UpdateTarget(fixedDeltaTime);
					ShootProjectile(fixedDeltaTime);
				}
			}
		}

		private void UpdateTurretRotation()
		{
			// Jotunn.Logger.LogInfo("Updating Turret rotation");
			float fixedDeltaTime = Time.fixedDeltaTime;
			bool has_target = (bool)m_target;
			Vector3 forward;
			if (has_target)
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
			m_aimDiffToTarget = (has_target ? Math.Abs(Quaternion.Dot(quaternion2, quaternion)) : -1f);
		}

		private void UpdateTarget(float dt)
		{
			//Jotunn.Logger.LogInfo("Updating Target");
			m_updateTargetTimer -= dt;
			if (m_updateTargetTimer <= 0f)
			{
				m_updateTargetTimer = (Character.IsCharacterInRange(base.transform.position, 40f) ? m_updateTargetIntervalNear : m_updateTargetIntervalFar);
				// Character character = BaseAI.FindClosestCreature(base.transform, eye.transform.position, 0f, m_viewDistance, m_horizontalAngle, false, false);
				// Id much prefer to get characters in range, but we need a complete list of all valid targets, instead of just avoiding the things we don't want to hit
				Character selectedTarget = selectTarget();

				if (selectedTarget != m_target)
				{
					if ((bool)selectedTarget)
					{
						UnityEngine.Object.Instantiate(m_newTargetEffect, base.transform.position, base.transform.rotation);
					}
					else
					{
						UnityEngine.Object.Instantiate(m_lostTargetEffect, base.transform.position, base.transform.rotation);
					}
					m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", selectedTarget ? selectedTarget.GetZDOID() : ZDOID.None);
				}
			}
			if (m_haveTarget && (!m_target || m_target.IsDead()))
			{
				// Jotunn.Logger.LogInfo("Target is dead, clearing target.");
				m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetTarget", ZDOID.None);
				UnityEngine.Object.Instantiate(m_lostTargetEffect, base.transform.position, base.transform.rotation);
			}
		}

		private bool IsValidTarget(Character ptarget)
		{
			// Dead, tames and other players are not valid targets
			if (ptarget.IsDead() || ptarget.IsTamed() || ptarget.IsPlayer()) { return false; }
			// Faction 0 (player faction) and the local player is not a target either
			if ((int)ptarget.GetFaction() == 0 || ptarget is Player) { return false; }
			//Jotunn.Logger.LogInfo("Found valid target");
			return true;
		}

		private Character selectTarget()
		{
			//Jotunn.Logger.LogInfo("selecting target");
			List<Character> potentialTargets = Character.GetAllCharacters();
			Character selectedTarget = null;
			foreach (Character ptarget in potentialTargets)
			{
				if (!IsValidTarget(ptarget))
				{
					continue;
				}
				BaseAI ptargetAI = ptarget.GetBaseAI();
				if (ptargetAI.IsSleeping()) { continue; }
				float distance_to_ptarget = Vector3.Distance(base.transform.position, ptarget.transform.position);
				if (distance_to_ptarget < m_viewDistance)
				{
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
			// This is really noisy
			if (VFConfig.EnableTurretDebugMode.Value) { Jotunn.Logger.LogInfo($"Turret target status:{!(bool)m_target} aimdiff:{m_aimDiffToTarget} > {m_shootWhenAimDiff} ({!(m_aimDiffToTarget > m_shootWhenAimDiff)}) cooldown:{IsCoolingDown()}"); }
			UnityEngine.Object.Instantiate(m_shootEffect, turretBodyArmed.transform.position, eye.transform.rotation);
			m_nview.GetZDO().Set("lastAttack", (float)ZNet.instance.GetTimeSeconds());
			{
				Vector3 forward = eye.transform.forward;
				Vector3 axis = Vector3.Cross(forward, Vector3.up);
				float projectileAccuracy = m_ammo_accuracy;
				Quaternion quaternion = Quaternion.AngleAxis(UnityEngine.Random.Range(0f - projectileAccuracy, projectileAccuracy), Vector3.up);
				forward = Quaternion.AngleAxis(UnityEngine.Random.Range(0f - projectileAccuracy, projectileAccuracy), axis) * forward;
				forward = quaternion * forward;
				GameObject projectile = UnityEngine.Object.Instantiate(m_Ammo.m_shared.m_attack.m_attackProjectile, eye.transform.position, eye.transform.rotation);
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
					component.Setup(null, forward * (m_Ammo.m_shared.m_attack.m_projectileVel * 2), m_hitNoise, hitData, null, m_Ammo);
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

		private void RPC_SetTarget(long sender, ZDOID character)
		{
			GameObject gameObject = ZNetScene.instance.FindInstance(character);
			if ((bool)gameObject)
			{
				Character component = gameObject.GetComponent<Character>();
				if ((object)component != null)
				{
					m_target = component;
					m_haveTarget = true;
					return;
				}
			}
			m_target = null;
			m_haveTarget = false;
			m_scan = 0f;
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

		private void UpdateMarker(float dt)
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