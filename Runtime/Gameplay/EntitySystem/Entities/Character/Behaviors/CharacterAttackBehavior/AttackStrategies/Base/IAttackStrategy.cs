using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IAttackStrategy : IDisposable
    {
        #region Interface Methods

        void Init(WeaponModel weaponModel, CharacterModel creatorModel, Transform creatorTransform);
        bool CheckCanAttack();
        bool CheckCanSpecialAttack();
        UniTask OperateAttack();
        UniTask OperateSpecialAttack();
        void Cancel();

        #endregion Interface Methods
    }
}