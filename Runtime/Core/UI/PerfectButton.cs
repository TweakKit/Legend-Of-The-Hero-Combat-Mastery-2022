using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Runtime.Audio;
using Runtime.Definition;
using Runtime.Core.Mics;

namespace Runtime.Core.UI
{
    public class PerfectButton : Button, IClickable
    {
        #region Members

        private static readonly float s_proceedOnClickDelayTime = 0.1f;
        private static readonly float s_preventSpamDelayTime = 0.15f;

        [SerializeField]
        private ButtonSourceType _buttomSourceType = ButtonSourceType.Other;
        [SerializeField]
        private ButtonType _buttonType = ButtonType.Other;
        private bool _blockInput = false;

        #endregion Members

        #region API Methods

        protected override void OnEnable()
        {
            base.OnEnable();
            _blockInput = false;
        }

        #endregion API Methods

        #region Class Methods

        public void Click()
            => Trigger();

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!_blockInput && interactable)
            {
                _blockInput = true;
                Press();
                StartCoroutine(BlockInputTemporarily());
            }
        }

        protected virtual bool Press()
        {
            if (!IsActive())
                return false;

            StartCoroutine(InvokeOnClickAction());
            return true;
        }

        private IEnumerator InvokeOnClickAction()
        {
            yield return new WaitForSecondsRealtime(s_proceedOnClickDelayTime);
            ProceedAction();
        }

        private IEnumerator BlockInputTemporarily()
        {
            yield return new WaitForSecondsRealtime(s_preventSpamDelayTime);
            _blockInput = false;
        }

        private void ProceedAction()
        {
            onClick.Invoke();
            switch (_buttonType)
            {
                case ButtonType.PlayGameSplashArt:
                case ButtonType.PlayStage:
                case ButtonType.ReplayInStage:
                case ButtonType.NextStage:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.START_GAME);
                    break;

                case ButtonType.Ok:
                case ButtonType.Yes:
                case ButtonType.Confirm:
                case ButtonType.Continue:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.UI_CONFIRM);
                    break;

                case ButtonType.Cancel:
                case ButtonType.Back:
                case ButtonType.Close:
                case ButtonType.No:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.UI_CANCEL);
                    break;

                case ButtonType.Upgrade:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.UPGRADE_SELECT);
                    break;

                case ButtonType.GachaBasicX1:
                    break;

                case ButtonType.GachaPremiumX1:
                    break;

                case ButtonType.GachaBasicX10:
                    break;

                case ButtonType.GachaPremiumX10:
                    break;

                case ButtonType.Unidentified:
                    break;

                case ButtonType.Pause:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.GAME_PAUSE);
                    break;

                default:
                    AudioController.Instance.PlaySoundEffect(AudioConstants.UI_CLICK);
                    break;
            }
        }

        #endregion Class Methods

        #region Unity Event Callback Methods

        public void Trigger()
            => ProceedAction();

        #endregion Unity Event Callback Methods
    }
}