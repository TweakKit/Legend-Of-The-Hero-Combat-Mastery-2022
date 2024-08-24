using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Extensions;
using UnityEngine;
using System.Threading;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior display text want to say.<br/>
    /// </summary>
    public class CharacterSpeakBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private static string s_vfxHolderName = "vfx_holder";
        private static string s_vfxTopPositionName = "top_position";

        private CancellationTokenSource _cancellationTokenSource;
        private Transform _topPosition;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            var vfxHolder = ownerTransform.FindChildTransform(s_vfxHolderName);
            if (vfxHolder == null)
            {
                Debug.LogError("VFX holder's name is not mapped!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel.ReactionChangedEvent += OnReactionChanged;
            var vfxHolder = transform.FindChildTransform(s_vfxHolderName);
            _topPosition = vfxHolder.Find(s_vfxTopPositionName);
            _cancellationTokenSource = new CancellationTokenSource();
            return true;
        }

        private void OnReactionChanged(CharacterReactionType characterReactionType)
        {
            string content = string.Empty;
            switch (characterReactionType)
            {
                case CharacterReactionType.JustBuffSpeed:
                    content = SpeakText.VROOM;
                    break;
                case CharacterReactionType.JustBuffBigSpeed:
                    content = SpeakText.BIG_VROOM;
                    break;
                case CharacterReactionType.JustDodge:
                    content = SpeakText.DODGE;
                    break;
                default:
                    break;
            }

            if(!string.IsNullOrEmpty(content))
                DisplaySpeakContent(content).Forget();
        }
        private async UniTaskVoid DisplaySpeakContent(string content)
        {
            var text = await PoolManager.Instance.Get(content, _cancellationTokenSource.Token);
            text.transform.position = _topPosition.position;
        }

        public void Disable() => _cancellationTokenSource.Cancel();

        #endregion Class Methods
    }
}