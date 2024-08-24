using System;
using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public interface ISkillStrategy : IDisposable
    {
        #region Interface Methods

        public void Init(SkillModel skillModel, CharacterModel creatorModel, Transform creatorTransform);
        public UniTask<SkillTriggerResult> CheckCanCast(CancellationToken cancellationToken);
        public UniTask Execute(SkillActionData actionData, CancellationToken cancellationToken);
        public SkillActionPhase Cancel();

        #endregion Interface Methods
    }

    public struct SkillTriggerResult
    {
        #region Members

        public readonly bool Result;
        public readonly SkillActionData Data;

        #endregion Members

        #region Struct Methods

        public SkillTriggerResult(bool result, SkillActionData data = null)
        {
            Result = result;
            Data = data;
        }

        #endregion Struct Methods
    }
}