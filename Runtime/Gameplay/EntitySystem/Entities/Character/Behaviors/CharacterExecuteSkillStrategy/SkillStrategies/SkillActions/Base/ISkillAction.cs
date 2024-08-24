using System;
using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public interface ISkillAction
    {
        #region Properties

        public SkillActionPhase SkillActionPhase { get; }

        #endregion Properties

        #region Interface Methods

        public void Init(CharacterModel creatorModel, Transform creatorTransform, SkillType skillType, SkillTargetType targetType, SkillActionPhase skillActionPhase);
        public UniTask<SkillActionData> RunOperateAsync(CancellationToken cancellationToken, SkillActionData data = null);
        public void Cancel();

        #endregion Interface Methods
    }
}