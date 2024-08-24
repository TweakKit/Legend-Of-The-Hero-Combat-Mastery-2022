using UnityEngine;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Tutorial
{
    public class GameplayGuideTutorialExecutor : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private GameplayGuideTutorialManager _versionAGameplayGuideTutorialManager;
        [SerializeField]
        private GameplayGuideTutorialManager _versionBGameplayGuideTutorialManager;
        [SerializeField]
        private GameplayGuideTutorialManager _versionCGameplayGuideTutorialManager;

        #endregion Members

        #region Unity Event Callback Methods

        public void Execute()
        {
            if (TutorialNavigator.Instance.CanStartGameplayGuideTutorial)
            {
                var mechanicStyleType = GetMechanicStyleType();
                switch (mechanicStyleType)
                {
                    case NewMechanicStyleType.Default:
                        _versionAGameplayGuideTutorialManager.gameObject.SetActive(true);
                        _versionBGameplayGuideTutorialManager.gameObject.SetActive(false);
                        _versionCGameplayGuideTutorialManager.gameObject.SetActive(false);
                        TutorialNavigator.Instance.ExecuteGameplayGuideTutorial(_versionAGameplayGuideTutorialManager);
                        break;

                    case NewMechanicStyleType.NewMoveAttack:
                        _versionBGameplayGuideTutorialManager.gameObject.SetActive(true);
                        _versionAGameplayGuideTutorialManager.gameObject.SetActive(false);
                        _versionCGameplayGuideTutorialManager.gameObject.SetActive(false);
                        TutorialNavigator.Instance.ExecuteGameplayGuideTutorial(_versionBGameplayGuideTutorialManager);
                        break;

                    case NewMechanicStyleType.NewWeapon:
                        _versionCGameplayGuideTutorialManager.gameObject.SetActive(true);
                        _versionAGameplayGuideTutorialManager.gameObject.SetActive(false);
                        _versionBGameplayGuideTutorialManager.gameObject.SetActive(false);
                        TutorialNavigator.Instance.ExecuteGameplayGuideTutorial(_versionCGameplayGuideTutorialManager);
                        break;
                }
            }
        }

        public void ReplaySequence()
        {
            var mechanicStyleType = GetMechanicStyleType();
            switch (mechanicStyleType)
            {
                case NewMechanicStyleType.Default:
                    _versionAGameplayGuideTutorialManager.ReplaySequence();
                    break;

                case NewMechanicStyleType.NewMoveAttack:
                    _versionBGameplayGuideTutorialManager.ReplaySequence();
                    break;

                case NewMechanicStyleType.NewWeapon:
                    _versionCGameplayGuideTutorialManager.ReplaySequence();
                    break;
            }
        }

        public void AdventureTutorial()
        {
            var mechanicStyleType = GetMechanicStyleType();
            switch (mechanicStyleType)
            {
                case NewMechanicStyleType.Default:
                    _versionAGameplayGuideTutorialManager.AdvanceTutorial();
                    break;

                case NewMechanicStyleType.NewMoveAttack:
                    _versionBGameplayGuideTutorialManager.AdvanceTutorial();
                    break;

                case NewMechanicStyleType.NewWeapon:
                    _versionCGameplayGuideTutorialManager.AdvanceTutorial();
                    break;
            }
        }

        #endregion Unity Event Callback Methods

        #region Class Methods

        private NewMechanicStyleType GetMechanicStyleType()
        {
            var newMechanicStyleType = DataManager.RemoteConfig.GetMechanicStyleType();
            return newMechanicStyleType;
        }

        #endregion Class Methods
    }
}