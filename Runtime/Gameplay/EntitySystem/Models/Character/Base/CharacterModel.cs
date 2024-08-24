using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract partial class CharacterModel : EntityModel
    {
        #region Members

        public bool isDashing;
        public bool isPlayingSkill;
        public bool isAttacking;
        public bool isSpecialAttacking;
        public bool isPausedControl;
        public EntityModel currentTargetedTarget;
        protected uint level;

        // For Health
        protected float maxHp;
        protected float currentHp;
        protected float baseMultiplyHp;
        protected float baseBonusHp;
        protected float totalMultiplyHp;
        protected float totalBonusHp;

        // For Shield
        protected float maxDefense;
        protected float currentDefense;
        protected float baseMultiplyDefense;
        protected float baseBonusDefense;
        protected float totalMultiplyDefense;
        protected float totalBonusDefense;

        // Others
        protected float collideDamage;
        protected bool isMoving;
        protected Vector2 movePosition;
        protected Vector2 moveDirection;
        protected Vector2 faceDirection;
        protected bool faceRight;

        #endregion Members

        #region Properties

        public override bool IsBeingTargeted
        {
            get => isBeingTargeted;
            set
            {
                if (isBeingTargeted != value)
                {
                    isBeingTargeted = value;
                    TargetedEvent.Invoke(value);
                }
            }
        }

        public Vector2 MovePosition
        {
            get => movePosition;
            set
            {
                if (movePosition != value)
                {
                    movePosition = value;
                    MovePositionUpdatedEvent.Invoke();
                }
            }
        }

        public Vector2 MoveDirection
        {
            get => moveDirection;
        }

        public bool FaceRight
        {
            get => faceRight;
            set
            {
                if (faceRight != value)
                {
                    faceRight = value;
                    DirectionChangedEvent.Invoke();
                }
            }
        }

        public float MaxHp => ((maxHp + baseBonusHp) * baseMultiplyHp + totalBonusHp) * totalMultiplyHp;
        public float MaxDefense => ((maxDefense + baseBonusDefense) * baseMultiplyDefense + totalMultiplyDefense) * totalMultiplyDefense;
        public float CollideDamage => collideDamage;
        public bool CanCauseCollideDamage { get; set; }
        public float CurrentHp => currentHp;
        public float CurrentDefense => currentDefense;
        public bool IsMoving => isMoving;
        public Vector2 FaceDirection => faceDirection;
        public bool IsControllable => !(IsDead || IsInHardCCStatus || isDashing || isPlayingSkill || isPausedControl);
        public bool IsMoveable => !(IsDead || IsInMovementLockedStatus || isDashing || isPausedControl || isPlayingSkill);
        public bool IsDamagable => !(IsDead || isDashing);
        public bool CheckCanUseSkill => !(IsDead || IsInSkillLockedStatus || isDashing || isPlayingSkill);
        public bool CheckCanAttack => !(IsDead || IsInAttackLockedStatus || isDashing || isPlayingSkill || isAttacking);
        public bool CanDetectTarget => !isAttacking && !isSpecialAttacking;
        public bool IgnoreTarget { get; set; }
        public override uint Level => level;
        public override bool IsDead => currentHp <= 0;
        public List<SkillModel> SkillModels { get; protected set; }

        #endregion Properties

        #region Class Methods

        public CharacterModel(uint spawnedWaveIndex, uint characterUId, uint characterId, CharacterLevelModel characterLevelModel, List<SkillModel> skillModels)
            : base(spawnedWaveIndex, characterUId, characterId.ToString(), characterLevelModel.DetectedPriority)
        {
            CanCauseCollideDamage = true;
            IgnoreTarget = false;
            level = characterLevelModel.Level;
            maxHp = characterLevelModel.MaxHp;
            baseMultiplyHp = 1f;
            baseBonusHp = 0f;
            totalMultiplyHp = 1f;
            totalBonusHp = 0f;
            currentHp = maxHp;

            maxDefense = 0;
            baseMultiplyDefense = 1f;
            baseBonusDefense = 0f;
            totalMultiplyDefense = 1f;
            totalBonusDefense = 0f;
            currentDefense = 0;

            collideDamage = characterLevelModel.CollideDamage;
            faceRight = false;
            faceDirection = new Vector2(-1, 0);
            SkillModels = skillModels;
            statusEffectsStackData = new List<StatusEffectType>();
            InitStats(characterLevelModel.CharacterStatsInfo);
            InitEvents();
        }

        public void SetMoveDirection(Vector2 value, bool faceTarget = true, bool triggeredMoveChanged = true)
        {
            var direction = value;
            if (currentTargetedTarget != null && !currentTargetedTarget.IsDead && faceTarget && !IgnoreTarget)
                direction = (currentTargetedTarget.Position - Position).normalized;

            if (direction != Vector2.zero && faceDirection != direction)
            {
                faceDirection = direction;
                FaceRight = direction.x > 0;
            }

            if (moveDirection != value)
            {
                moveDirection = value;
                if (isMoving && moveDirection == Vector2.zero)
                {
                    isMoving = false;
                    if(triggeredMoveChanged)
                        MovementChangedEvent.Invoke();
                }
                else if (!isMoving && moveDirection != Vector2.zero)
                {
                    isMoving = true;
                    if (triggeredMoveChanged)
                        MovementChangedEvent.Invoke();
                }
            }
        }

        protected virtual partial void InitStats(CharacterStatsInfo statsInfo);
        protected virtual partial void InitEvents();

        #endregion Class Methods
    }

    public class CharacterLevelModel
    {
        #region Members

        protected uint level;
        protected float maxHp;
        protected float collideDamage;
        protected int detectedPriority;
        protected CharacterStatsInfo characterStatsInfo;

        #endregion Members

        #region Properties

        public uint Level => level;
        public float MaxHp => maxHp;
        public float CollideDamage => collideDamage;
        public int DetectedPriority => detectedPriority;
        public CharacterStatsInfo CharacterStatsInfo => characterStatsInfo;

        #endregion Properties

        #region Class Methods

        public CharacterLevelModel(uint level, int detectedPriority, CharacterStatsInfo characterStatsInfo)
        {
            this.level = level;
            this.detectedPriority = detectedPriority;
            this.characterStatsInfo = characterStatsInfo;
            maxHp = characterStatsInfo.GetStatTotalValue(StatType.Health);
            collideDamage = characterStatsInfo.GetStatTotalValue(StatType.CollideDamage);
        }

        #endregion Class Methods
    }
}