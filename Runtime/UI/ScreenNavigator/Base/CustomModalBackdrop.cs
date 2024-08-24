using UnityScreenNavigator.Runtime.Core.Modals;
using Runtime.UI;
using Runtime.Audio;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class CustomModalBackdrop : ModalBackdrop
    {
        #region Class Methods

        protected override void PopModal()
        {
            AudioController.Instance.PlaySoundEffect(AudioConstants.UI_CANCEL);
            ScreenNavigator.Instance.PopModal(ownerModal, true).Forget();
        }

        #endregion Class Methods
    }
}