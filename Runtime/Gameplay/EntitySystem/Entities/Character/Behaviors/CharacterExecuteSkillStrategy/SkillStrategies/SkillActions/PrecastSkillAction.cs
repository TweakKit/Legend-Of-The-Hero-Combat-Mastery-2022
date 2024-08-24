using System.Threading;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class PrecastSkillAction : SkillAction
    {

        #region Class Methods

        public override async UniTask<SkillActionData> RunOperateAsync(CancellationToken cancellationToken, SkillActionData skillActionData)
        {
            var hasFinishedAnimation = false;
            var characterPlayedSkillAction = new CharacterPlayedSkillAction
            (
                skillType: skillType,
                skillActionPhase: SkillActionPhase,
                eventTriggeredCallbackAction: null,
                endActionCallbackAction: () => hasFinishedAnimation = true
            );
            characterSkillActionPlayer.Play(characterPlayedSkillAction);
            await UniTask.WaitUntil(() => hasFinishedAnimation, cancellationToken: cancellationToken);
            return skillActionData;
        }

        #endregion Class Methods
    }
}