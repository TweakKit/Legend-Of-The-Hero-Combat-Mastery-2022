using System;
using System.Threading;
using UnityEngine;
using Runtime.Extensions;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public sealed class CharacterWeapon : MonoBehaviour, IDisposable
    {
        #region Members

        [SerializeField]
        private Transform _weaponHolderTransform;
        [SerializeField]
        private Transform _weaponFlipPivotTransform;
        [SerializeField]
        private float _rotateSpeed;
        [SerializeField]
        private bool _rotateWithOwner;
        private CharacterModel _ownerCharacterModel;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public bool PausedRotate { get; set; }

        #endregion Properties

        #region Class Methods

        public void Init(CharacterModel ownerCharacterModel)
        {
            _ownerCharacterModel = ownerCharacterModel;
            if (_rotateWithOwner)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                UpdateWeaponRotationAsync(_cancellationTokenSource.Token).Forget();
            }
        }

        public void Dispose()
            => _cancellationTokenSource?.Cancel();

        private async UniTaskVoid UpdateWeaponRotationAsync(CancellationToken cancellationToken)
        {
            while (!_ownerCharacterModel.IsDead)
            {
                if (!PausedRotate)
                {
                    var toRotation = (-_ownerCharacterModel.FaceDirection).ToQuaternion(0);
                    _weaponHolderTransform.rotation = Quaternion.RotateTowards(_weaponHolderTransform.rotation, toRotation, _rotateSpeed * Time.deltaTime);
                    var degree = Quaternion.Angle(_weaponHolderTransform.rotation, Quaternion.identity);
                    if (degree > 90 || degree < -90)
                        _weaponFlipPivotTransform.localScale = new Vector3(1, -1, 1);
                    else
                        _weaponFlipPivotTransform.localScale = new Vector3(1, 1, 1);
                }
                await UniTask.Yield(cancellationToken);
            }
        }

        #endregion Class Methods
    }
}