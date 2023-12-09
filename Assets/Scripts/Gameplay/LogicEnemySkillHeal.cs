using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
	public class LogicEnemySkillHeal : EnemyController
	{
		[Space]
		[Header("Parameters")]
		[SerializeField]
		private int healthRestoreAmount;

		[SerializeField]
		private float skillRange;

		[SerializeField]
		private float criticalHealthPercentage;

		[SerializeField]
		private float coolDownTimeMillisecond;

		[SerializeField]
		private float minSpeed = 0.05f;

		[SerializeField]
		private float attackAnimationDuration = 1f;

		private List<EnemyModel> inRangeEnemies = new List<EnemyModel>();

		private float coolDownTime;

		private float coolDownTimeTracking;

		private bool skillReady;

		private bool isCastingSkill;

		[SerializeField]
		private bool isActiveSkillOnAppear;

		private WaitForSeconds waitForAnimation;

		public override void Initialize()
		{
			base.Initialize();
		}

		public override void OnAppear()
		{
			base.OnAppear();
			coolDownTime = coolDownTimeMillisecond / 1000f;
			if (isActiveSkillOnAppear)
			{
				coolDownTimeTracking = 0f;
			}
			else
			{
				coolDownTimeTracking = coolDownTime;
			}
			skillReady = true;
			isCastingSkill = false;
			SingletonMonoBehaviour<SpawnFX>.Instance.InitFX(SpawnFX.EFFECT_HEAL_0);
		}

		private void Start()
		{
			waitForAnimation = new WaitForSeconds(attackAnimationDuration);
		}

		public override void Update()
		{
			base.Update();
			if (!skillReady || isCastingSkill || !IsEnemyAlive() || SingletonMonoBehaviour<GameData>.Instance.IsGameOver)
			{
				return;
			}
			if (IsCooldownSkillDone())
			{
				GetInRangeEnemies();
				if (inRangeEnemies.Count > 0 && ShouldCastSkill())
				{
					StartCoroutine(CastSkillHeal());
				}
			}
			coolDownTimeTracking = Mathf.MoveTowards(coolDownTimeTracking, 0f, Time.deltaTime);
		}

		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(base.transform.position, skillRange / GameData.PIXEL_PER_UNIT);
		}

		private bool IsCooldownSkillDone()
		{
			return coolDownTimeTracking == 0f;
		}

		private void GetInRangeEnemies()
		{
			inRangeEnemies.Clear();
			SingletonMonoBehaviour<GameData>.Instance.GetInRangeEnemies(base.EnemyModel.transform.position, skillRange / GameData.PIXEL_PER_UNIT, inRangeEnemies);
		}

		private bool ShouldCastSkill()
		{
			bool result = false;
			foreach (EnemyModel inRangeEnemy in inRangeEnemies)
			{
				if ((float)inRangeEnemy.EnemyHealthController.CurrentHealth <= criticalHealthPercentage / 100f * (float)inRangeEnemy.EnemyHealthController.OriginHealth)
				{
					result = true;
				}
			}
			return result;
		}

		private IEnumerator CastSkillHeal()
		{
			isCastingSkill = true;
			if (!IsCurrentSpeedGreaterThanMinSpeed())
			{
				yield return null;
			}
			if (!IsEnemyAlive())
			{
				yield return null;
			}
			base.EnemyModel.SetSpecialStateDuration(attackAnimationDuration);
			base.EnemyModel.SetSpecialStateAnimationName(EnemyAnimationController.animSpecialAttack);
			base.EnemyModel.GetFSMController().GetCurrentState().OnInput(StateInputType.SpecialState, EnemyAnimationController.animSpecialAttack);
			base.EnemyModel.EnemyAnimationController.ToSpecialAttackState();
			GetInRangeEnemies();
			foreach (EnemyModel inRangeEnemy in inRangeEnemies)
			{
				if ((float)inRangeEnemy.EnemyHealthController.CurrentHealth <= criticalHealthPercentage / 100f * (float)inRangeEnemy.EnemyHealthController.OriginHealth)
				{
					inRangeEnemy.EnemyHealthController.AddHealth(healthRestoreAmount);
					EffectController effect = SingletonMonoBehaviour<SpawnFX>.Instance.GetEffect(SpawnFX.EFFECT_HEAL_0);
					effect.Init(0.5f, inRangeEnemy.transform);
				}
			}
			UnityEngine.Debug.Log("heal " + healthRestoreAmount);
			yield return waitForAnimation;
			coolDownTimeTracking = coolDownTime;
			isCastingSkill = false;
		}

		public void OnDisable()
		{
			isCastingSkill = false;
		}
	}
}
