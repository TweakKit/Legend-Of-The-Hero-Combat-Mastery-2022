using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnDamageBoxSkillAction : SkillAction
    {
        #region Members

        private string _damageBoxPrefabName;
        private float _damageBoxHeight;
        private float _damageBoxWidth;
        private float _damageBonus;
        private DamageFactor[] _damageFactors;
        private StatusEffectModel[] _damageStatusEffectModels;

        #endregion Members

        #region Class Methods

        public void Init(
            CharacterModel creatorModel, 
            Transform creatorTranform, 
            SkillType skillType, 
            SkillTargetType targetType, 
            SkillActionPhase skillActionPhase,
            string damageBoxPrefabName,
            float damageBoxWidth,
            float damageBoxHeight,
            float damageBonus,
            DamageFactor[] damageFactors,
            StatusEffectModel[] damageStatusEffectModels
        )
        {
            base.Init(creatorModel, creatorTranform, skillType, targetType, skillActionPhase);

            _damageBoxPrefabName = damageBoxPrefabName;
            _damageBoxWidth = damageBoxWidth;
            _damageBoxHeight = damageBoxHeight;
            _damageBonus = damageBonus;
            _damageFactors = damageFactors;
            _damageStatusEffectModels = damageStatusEffectModels;
        }

        public async override UniTask<SkillActionData> RunOperateAsync(CancellationToken cancellationToken, SkillActionData skillActionData)
        {
            var damageBoxGameObject = await PoolManager.Instance.Get(_damageBoxPrefabName, cancellationToken);
            var damageBox = damageBoxGameObject.GetComponent<SpriteAnimatorDamageBox>();
            damageBoxGameObject.transform.position = creatorModel.Position;
            damageBoxGameObject.transform.localScale = new Vector2(_damageBoxWidth / 2, _damageBoxHeight / 2);
            damageBox.Init(creatorModel, DamageSource.FromSkill, true, _damageBonus, _damageFactors, _damageStatusEffectModels);
            return skillActionData;
        }

        #endregion Class Methods
    }
}
