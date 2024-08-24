using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class SkillActionData
    {
        #region Members

        public bool result;
        public EntityModel target;
        public Vector2 position;
        public Vector2 direction;

        #endregion Members
    }

    public abstract class SkillAction : ISkillAction
    {
        #region Members

        protected CharacterModel creatorModel;
        protected Transform creatorTransform;
        protected SkillType skillType;
        protected SkillTargetType targetType;
        protected ICharacterSkillActionPlayer characterSkillActionPlayer;
        protected SkillActionPhase skillActionPhase;

        #endregion Members

        #region Properties

        public SkillActionPhase SkillActionPhase => skillActionPhase;
        public int TargetLayerMask { get; private set; }

        #endregion Properties

        #region Class Methods

        public virtual void Init(CharacterModel creatorModel, Transform creatorTranform, SkillType skillType, SkillTargetType targetType, SkillActionPhase skillActionPhase)
        {
            this.skillActionPhase = skillActionPhase;
            this.creatorModel = creatorModel;
            this.creatorTransform = creatorTranform;
            this.skillType = skillType;
            this.targetType = targetType;
            TargetLayerMask = GetTargetLayerMask();
            characterSkillActionPlayer = creatorTransform.GetComponentInChildren<ICharacterSkillActionPlayer>(true);
#if DEBUGGING
            if (characterSkillActionPlayer == null)
            {
                Debug.LogError($"Require a character skill animation for this behavior!");
                return;
            }
#endif

            characterSkillActionPlayer?.Init(creatorModel);
        }

        public abstract UniTask<SkillActionData> RunOperateAsync(CancellationToken cancellationToken, SkillActionData skillActionData);

        public virtual void Cancel() { }

        private int GetTargetLayerMask()
        {
            if ((targetType & SkillTargetType.None) > 0)
                return 0;

            var targetLayerMask = 0;
            if ((targetType & SkillTargetType.Self) > 0)
            {
                switch (creatorModel.EntityType)
                {
                    case EntityType.Hero:
                        targetLayerMask |= Layers.HERO_LAYER_MASK;
                        break;

                    case EntityType.Zombie:
                        targetLayerMask |= Layers.ZOMBIE_LAYER_MASK;
                        break;

                    case EntityType.Object:
                        targetLayerMask |= Layers.OBJECT_LAYER_MASK;
                        break;

                    case EntityType.Trap:
                        targetLayerMask |= Layers.TRAP_LAYER_MASK;
                        break;

                    case EntityType.DamageArea:
                        targetLayerMask |= Layers.DAMAGE_AREA_LAYER_MASK;
                        break;

                    case EntityType.Boss:
                        targetLayerMask |= Layers.BOSS_LAYER_MASK;
                        break;

                    case EntityType.Decoration:
                        targetLayerMask |= Layers.OBSTACLE_LAYER_MASK;
                        break;
                }
            }

            if ((targetType & SkillTargetType.Hero) > 0)
                targetLayerMask |= Layers.HERO_LAYER_MASK;

            if ((targetType & SkillTargetType.Zombie) > 0)
                targetLayerMask |= Layers.ZOMBIE_LAYER_MASK;

            if ((targetType & SkillTargetType.Object) > 0)
                targetLayerMask |= Layers.OBJECT_LAYER_MASK;

            if ((targetType & SkillTargetType.Trap) > 0)
                targetLayerMask |= Layers.TRAP_LAYER_MASK;

            if ((targetType & SkillTargetType.Projectile) > 0)
                targetLayerMask |= Layers.PROJECTILE_LAYER_MASK;

            if ((targetType & SkillTargetType.DamageArea) > 0)
                targetLayerMask |= Layers.DAMAGE_AREA_LAYER_MASK;

            if ((targetType & SkillTargetType.Boss) > 0)
                targetLayerMask |= Layers.BOSS_LAYER_MASK;

            return targetLayerMask;
        }

        #endregion Class Methods
    }
}