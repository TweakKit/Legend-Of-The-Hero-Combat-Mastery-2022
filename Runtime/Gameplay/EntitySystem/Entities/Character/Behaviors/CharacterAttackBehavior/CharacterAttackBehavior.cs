using UnityEngine;
using Runtime.Definition;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public sealed class CharacterAttackBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private const string WEAPON_HOLDER_NAME = "weapon";
        private const string WEAPON_TYPE_PREFIX_NAME = "weapon_";
        private IAttackStrategy _attackStrategy;
        private CharacterWeapon _characterWeapon;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            var entityWeaponData = model.GetEntityDistinctData<IEntityWeaponData>();
            if (entityWeaponData != null)
            {
                _attackStrategy = AttackStrategyFactory.GetAttackStrategy(entityWeaponData.WeaponModel.WeaponType);
                CreateWeaponAsync(entityWeaponData.WeaponModel).Forget();
                return true;
            }
            else return false;
        }

        private async UniTask CreateWeaponAsync(WeaponModel weaponModel)
        {
            var weaponHolderTransform = ownerTransform.FindChildTransform(WEAPON_HOLDER_NAME);
            var newEquippedWeapon = await PoolManager.Instance.Get($"{WEAPON_TYPE_PREFIX_NAME}{(int)weaponModel.WeaponType}", true);
            newEquippedWeapon.transform.SetParent(weaponHolderTransform);
            newEquippedWeapon.transform.localPosition = Vector3.zero;
            _characterWeapon = newEquippedWeapon.GetOrAddComponent<CharacterWeapon>();
            _characterWeapon.Init(ownerModel);
            await UniTask.Yield();
            _attackStrategy.Init(weaponModel, ownerModel, ownerTransform);
            ownerModel.ActionTriggeredEvent += OnActionTriggered;
            ownerModel.HardCCImpactedEvent += OnHardCCImpacted;
        }

        private void OnActionTriggered(ActionInputType actionInputType)
        {
            if (actionInputType == ActionInputType.Attack)
            {
                if (ownerModel.CheckCanAttack && _attackStrategy.CheckCanAttack())
                {
                    ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustNormalAttack);
                    RunAttackAsync().Forget();
                }
            }
            else if (actionInputType == ActionInputType.SpecialAttack)
            {
                if (_attackStrategy.CheckCanSpecialAttack())
                {
                    ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustSpecialAttack);
                    RunSpecialAttackAsync().Forget();
                }
            }
        }

        private void OnHardCCImpacted(StatusEffectType statusEffectType)
        {
            _attackStrategy.Cancel();
            ownerModel.isAttacking = false;
        }

        private async UniTaskVoid RunAttackAsync()
        {
            ownerModel.isAttacking = true;
            await _attackStrategy.OperateAttack();
            ownerModel.isAttacking = false;
        }

        private async UniTaskVoid RunSpecialAttackAsync()
        {
            ownerModel.isSpecialAttacking = true;
            await _attackStrategy.OperateSpecialAttack();
            ownerModel.isSpecialAttacking = false;
        }

        public void Disable()
        {
            _attackStrategy.Dispose();
            _characterWeapon.Dispose();
            PoolManager.Instance.Remove(_characterWeapon.gameObject);
        }

        #endregion Class Methods
    }
}