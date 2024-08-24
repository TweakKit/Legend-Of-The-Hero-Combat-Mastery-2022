using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Runtime.Utilities;
using Runtime.Manager;
using Runtime.Definition;
using Runtime.Tracking;
using Runtime.Extensions;
using Runtime.Core.Mics;
using Runtime.Manager.Data;
using Runtime.Server.CallbackData;
using Runtime.Server;
using Runtime.UI;
using Sirenix.OdinInspector;

namespace Runtime.Tutorial
{
    public abstract class TutorialManager : MonoBehaviour
    {
        #region Members

        [SerializeField]
        protected bool useUnscaledTime;
        [SerializeField]
        protected bool markTutorialCompletedBySequenceIndex;
        [ShowIf(nameof(markTutorialCompletedBySequenceIndex), true)]
        [SerializeField]
        protected int markTutorialCompletedSequenceIndex = -1;
        [SerializeField]
        protected TutorialSequenceData[] tutorialSequencesData;
        [SerializeField]
        protected TutorialRuntimeTargetSetUpData[] tutorialRuntimeTargetsSetUpData;
        protected int currentSequenceStep;
        protected bool hasStartedTutorial;
        protected bool hasCompletedAllSequences;
        protected bool isEnoughRequirementToMarkTutorialCompleted;
        protected TutorialSequenceData currentPlayingSequenceData;
        protected Dictionary<TutorialBlockData, List<TutorialBlockIndicator>> tutorialBlockIndicatorsDictionary;
        protected HashSet<TutorialBlockData> currentPlayingTutorialBlocksData;
        protected HashSet<TutorialBlockData> removalTutorialBlocksData;
        protected Canvas tutorialContainerCanvas;
        protected TutorialIndexData savedCurrentTutorialIndexData;

        #endregion Members

        #region Properties

        public virtual TutorialType TutorialType { get; protected set; }
        public TutorialTimer TutorialTimer { get; private set; }
        public Camera Camera { get; private set; }
        public bool HasCompletedAllSequences => hasCompletedAllSequences;

        #endregion Properties

        #region API Methods

        protected virtual void Awake() { }

        protected virtual void Update()
        {
            if (hasStartedTutorial)
            {
                if (currentPlayingSequenceData != null)
                {
                    CheckSequenceEventTrigger(currentPlayingSequenceData, false);
                    UpdateSequence(currentPlayingSequenceData);

                    foreach (var tutorialBlock in currentPlayingTutorialBlocksData)
                    {
                        tutorialBlock.UpdateTargetScreenRect();
                        CheckBlockEventTrigger(tutorialBlock, false);
                        UpdateBlockEvent(tutorialBlock);
                    }
                }
            }
        }

        protected virtual void LateUpdate()
        {
            if (hasStartedTutorial)
            {
                if (removalTutorialBlocksData.Count != 0)
                {
                    foreach (var tutorialBlock in removalTutorialBlocksData)
                    {
                        if (currentPlayingTutorialBlocksData.Contains(tutorialBlock))
                            currentPlayingTutorialBlocksData.Remove(tutorialBlock);
                    }
                    removalTutorialBlocksData.Clear();
                }
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (tutorialSequencesData != null)
                for (int i = 0; i < tutorialSequencesData.Length; i++)
                    tutorialSequencesData[i].ValidateData(i + 1);
        }
#endif 

        private void OnApplicationQuit()
            => LeaveApplication();

        #endregion API Methods

        #region Unity Event Callback Methods

        public virtual void SaveSequence()
        {
            string tutorialSequenceId = currentPlayingSequenceData.sequenceName;
            SendTutorialTracking(tutorialSequenceId, currentSequenceStep);
            DataManager.Server.SetTutorialCompletedStep(TutorialType, currentSequenceStep);
            var completeTutorialStepRequestData = new CompleteTutorialStepRequestData(TutorialType, currentSequenceStep);
            NetworkServer.CompleteStepTutorial(completeTutorialStepRequestData, null);

            var markTutCompletedAsSequenceSaved = currentPlayingSequenceData.markTutCompletedAsSequenceSaved;
            if (isEnoughRequirementToMarkTutorialCompleted && markTutCompletedAsSequenceSaved)
            {
                DataManager.Server.SetTutorialCompletedStep(TutorialType, Constants.MARK_TUTORIAL_COMPLETED_VALUE);
                var markTutorialCompletedStepRequestData = new CompleteTutorialStepRequestData(TutorialType, Constants.MARK_TUTORIAL_COMPLETED_VALUE);
                NetworkServer.CompleteStepTutorial(markTutorialCompletedStepRequestData, null);
            }
        }

        public virtual void SaveNextSequence()
        {
            var nextSequence = GetSequence(currentSequenceStep);
            if (nextSequence != null)
            {
                string nextTutorialSequenceId = nextSequence.sequenceName;
                SendTutorialTracking(nextTutorialSequenceId, currentSequenceStep);
                DataManager.Server.SetTutorialCompletedStep(TutorialType, currentSequenceStep);
                var completeEquipEquipmentTutorialStepRequestData = new CompleteTutorialStepRequestData(TutorialType, currentSequenceStep);
                NetworkServer.CompleteStepTutorial(completeEquipEquipmentTutorialStepRequestData, null);
            }
        }

        public virtual void SaveEndTutorialSequences()
        {
            if (isEnoughRequirementToMarkTutorialCompleted)
            {
                string endTutorialSequencesId = currentPlayingSequenceData.sequenceName;
                SendTutorialTracking(endTutorialSequencesId, currentSequenceStep);
                MarkTutorialCompleted();
            }
        }

        public virtual void MarkTutorialCompleted()
        {
            hasCompletedAllSequences = true;
            DataManager.Server.SetTutorialCompletedStep(TutorialType, Constants.MARK_TUTORIAL_COMPLETED_VALUE);
            var completeEquipEquipmentTutorialStepRequestData = new CompleteTutorialStepRequestData(TutorialType, Constants.MARK_TUTORIAL_COMPLETED_VALUE);
            NetworkServer.CompleteStepTutorial(completeEquipEquipmentTutorialStepRequestData, null);
            ResetTutorialContainer();
        }

        public void ReplaySequence()
        {
            currentPlayingSequenceData = null;
            InitOverralData();
            InitializeTutorialSequences();
            PlaySequence(savedCurrentTutorialIndexData.tutorialSequenceIndex);
        }

        public void PopTopMostModal()
            => ScreenNavigator.Instance.PopTopmostModal();

        public void PopAllModals()
            => ScreenNavigator.Instance.PopAllModals();

        #endregion Unity Event Callback Methods

        #region Class Methods

        public virtual void ResetTutorial()
        {
            StopTutorial(false);
            currentPlayingSequenceData = null;
            hasStartedTutorial = false;
        }

        public virtual void SkipSequence()
        {
            string tutorialSequenceId = currentPlayingSequenceData.sequenceName;
            SendTutorialTracking("Skip_" + tutorialSequenceId + "", currentSequenceStep);
            StopSequence();
        }

        public void PlaySequence(int sequenceStep, bool autoStart = true)
        {
            var sequence = GetSequence(sequenceStep - 1);
            if (sequence != null)
            {
                if (CanPlaySequence(sequence))
                    StartSequence(sequence, autoStart);
            }
            else LogWarning("No sequence for current sequence steps " + sequenceStep);
        }

        public void PlaySequence(string sequenceName, bool autoStart = true)
        {
            var sequence = GetCurrentSequence(sequenceName);
            if (sequence != null)
            {
                if (CanPlaySequence(sequence))
                    StartSequence(sequence, autoStart);
            }
            else LogWarning("No tutorial sequence found for current sequence steps " + sequenceName);
        }

        public void PlaySequence(bool autoStart = true)
            => PlaySequence(currentSequenceStep, autoStart);

        public void StopSequence()
        {
            if (currentPlayingSequenceData == null)
            {
                currentPlayingTutorialBlocksData.Clear();
                return;
            }

            if (currentPlayingSequenceData.state == PlayState.Start || currentPlayingSequenceData.state == PlayState.Executing)
            {
                ClearTutorialSequenceTriggerData(currentPlayingSequenceData, false);
                StopSquence(true);
            }
            else LogWarning("Current sequence:" + currentPlayingSequenceData.sequenceName + " is ending, please wait until the sequence is complete.");
        }

        public void PlayTutorial(int step, bool reset = true)
        {
            if (currentPlayingSequenceData == null)
                return;

            var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, step - 1);
            if (tutorialBlockData != null)
            {
                if (reset)
                    InitializedTutorialBlock(tutorialBlockData);

                if (CanPlayTutorial(tutorialBlockData))
                    StartTutorial(tutorialBlockData);
            }
            else LogWarning("No Tutorial Block for current tutorial steps: " + step + " in sequence: " + currentPlayingSequenceData.sequenceName);
        }

        public void PlayTutorial(string name, bool reset = true)
        {
            if (currentPlayingSequenceData == null)
            {
                LogWarning("No sequence playing at the moment");
                return;
            }

            var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, name);
            if (tutorialBlockData != null)
            {
                if (reset)
                    InitializedTutorialBlock(tutorialBlockData);

                if (CanPlayTutorial(tutorialBlockData))
                    StartTutorial(tutorialBlockData);
            }
            else LogWarning("No Tutorial Block for current tutorial steps: " + currentPlayingSequenceData.CurrentStep + " in sequence: " + currentPlayingSequenceData.sequenceName);
        }

        public void PlayTutorial()
        {
            if (currentPlayingSequenceData == null)
                return;

            PlayTutorial(currentPlayingSequenceData.CurrentStep);
        }

        public void PlayTutorial(int sequenceIndex, int blockIndex)
        {
            if (currentPlayingSequenceData == null)
            {
                PlaySequence(sequenceIndex, false);
                PlayTutorial(blockIndex);
            }
            else if (currentPlayingSequenceData.sequenceIndex == sequenceIndex)
            {
                PlayTutorial(blockIndex);
            }
            else
            {
                LogWarning("Sequence with index: " + sequenceIndex + " cannot be played due to current playing sequence: " + currentPlayingSequenceData.sequenceName);
            }
        }

        public void SendTracking(string eventName)
        {
#if TRACKING
            FirebaseManager.Instance.TrackEvent(eventName);
#endif
        }

        public void StopTutorial(bool autoAdvance = true)
        {
            if (currentPlayingSequenceData == null)
            {
                LogWarning("No sequence playing at the moment");
                return;
            }

            StopTutorial(currentPlayingSequenceData.CurrentStep, autoAdvance);
        }

        public void StopTutorial(int step, bool autoAdvance = true)
        {
            if (currentPlayingSequenceData == null)
            {
                LogWarning("No sequence playing at the moment");
                return;
            }

            var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, step - 1);
            if (tutorialBlockData != null)
            {
                if (currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
                    StopTutorialBlock(tutorialBlockData, autoAdvance);
                else
                    LogWarning("There is no running tutorial block at the moment");
            }
            else LogWarning("Could not find tutorial block with step " + currentPlayingSequenceData.CurrentStep + " when stopping");
        }

        public void StopTutorial(int sequenceIndex, int blockIndex, bool autoAdvance = true)
        {
            if (currentPlayingSequenceData != null && currentPlayingSequenceData.sequenceIndex == sequenceIndex)
                StopTutorial(blockIndex, autoAdvance);
            else
                LogWarning("Sequence with index: " + sequenceIndex + " is not playing at the moment");
        }

        public void StopTutorial(string name, bool autoAdvance = true)
        {
            if (currentPlayingSequenceData == null)
            {
                LogWarning("No sequence playing at the moment");
                return;
            }

            var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, name);
            if (tutorialBlockData != null)
            {
                if (currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
                    StopTutorialBlock(tutorialBlockData, autoAdvance);
                else
                    LogWarning("There is no running tutorial block at the moment");
            }
            else LogWarning("Could not find tutorial block with step " + currentPlayingSequenceData.CurrentStep + " when stopping");
        }

        public void AdvanceTutorial()
        {
            if (currentPlayingSequenceData != null)
            {
                bool isSequenceEnd = CheckSequenceEnd(currentPlayingSequenceData.CurrentStep);
                var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, currentPlayingSequenceData.CurrentStep - 1);
                if (tutorialBlockData != null && currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
                {
                    StopTutorialBlock(tutorialBlockData, false);
                    if (!isSequenceEnd && currentPlayingSequenceData != null)
                        PlayTutorial();
                }
                else
                {
                    if (!isSequenceEnd && tutorialBlockData != null)
                        PlayTutorial();
                    else
                        LogWarning("Can not advance tutorial as no tutorial block for current tutorial steps");
                }
            }
            else PlaySequence();
        }

        public void StepbackTutorial()
        {
            if (currentPlayingSequenceData != null)
            {
                bool stageStart = CheckSequenceStart(currentPlayingSequenceData.CurrentStep);
                var tutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, currentPlayingSequenceData.CurrentStep - 1);
                var currentStep = currentPlayingSequenceData.CurrentStep;
                if (tutorialBlockData != null && currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
                {
                    StopTutorialBlock(tutorialBlockData, false, true);
                    if (!stageStart && currentPlayingSequenceData != null)
                    {
                        var previousTutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, currentStep - 2);
                        if (previousTutorialBlockData != null)
                            PlayTutorial(previousTutorialBlockData.blockIndex);
                    }
                }
                else
                {
                    if (!stageStart && tutorialBlockData != null)
                    {
                        var previousTutorialBlockData = GetCurrentTutorialBlockData(currentPlayingSequenceData, currentStep - 2);
                        if (previousTutorialBlockData != null)
                            PlayTutorial(previousTutorialBlockData.blockIndex);
                    }
                    else LogWarning("Can not step tutorial as the tutorial is the first tutorial block");
                }
            }
            else PlaySequence();
        }

        public bool CheckSequenceStart(int index)
        {
            if (currentPlayingSequenceData != null)
            {
                if (index == 1)
                    return true;
            }
            return false;
        }

        public void SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType tutorialRuntimeStepType, params TutorialRuntimeTarget[] tutorialRuntimeTargets)
        {
            var runtimeTutorialIndexData =  SetUpRuntimeTutorial(tutorialRuntimeStepType, tutorialRuntimeTargets);
            if (runtimeTutorialIndexData != TutorialIndexData.None)
                PlayTutorial(runtimeTutorialIndexData.tutorialSequenceIndex, runtimeTutorialIndexData.tutorialSequenceBlockIndex);
        }

        public virtual void StartTutorialRuntime()
        {
            InitOverralData();
            InitializeTutorialSequences();
            hasStartedTutorial = true;
        }

        protected virtual void InitOverralData()
        {
            ClearAllTutorialBlockIndicators();

            isEnoughRequirementToMarkTutorialCompleted = !markTutorialCompletedBySequenceIndex;
            currentPlayingTutorialBlocksData = new HashSet<TutorialBlockData>();
            removalTutorialBlocksData = new HashSet<TutorialBlockData>();
            tutorialBlockIndicatorsDictionary = new Dictionary<TutorialBlockData, List<TutorialBlockIndicator>>();
            TutorialTimer = gameObject.GetOrAddComponent<TutorialTimer>();
            TutorialTimer.Init(useUnscaledTime);

            if (tutorialSequencesData == null)
                tutorialSequencesData = new TutorialSequenceData[0];

            if (tutorialContainerCanvas == null)
                tutorialContainerCanvas = GameObject.Find(ContainerKey.TUTORIAL_CONTAINER_LAYER_NAME).GetComponent<Canvas>();

            if (Camera == null)
            {
                Camera = Camera.main;
                Camera.depthTextureMode = DepthTextureMode.Depth;
            }

            currentSequenceStep = (tutorialSequencesData != null && tutorialSequencesData.Length > 0) ? 1 : 0;
            currentPlayingSequenceData = null;
            hasCompletedAllSequences = false;
        }

        protected void InitializeTutorialSequences()
        {
            for (int i = 0; i < tutorialSequencesData.Length; i++)
                InitializeTutorialSequence(tutorialSequencesData[i], i + 1);
        }

        protected void InitializeTutorialSequence(TutorialSequenceData tutorialSequenceData, int sequenceIndex)
        {
            SetState(ref tutorialSequenceData.state, PlayState.Idle);
            tutorialSequenceData.CurrentStep = 1;
            tutorialSequenceData.Init(sequenceIndex);
            InitializeTutorialSequenceTriggerData(tutorialSequenceData, true);

            for (int i = 0; i < tutorialSequenceData.tutorialBlocksData.Length; i++)
                InitializedTutorialBlock(tutorialSequenceData.tutorialBlocksData[i]);
        }

        protected void InitializeTutorialSequenceTriggerData(TutorialSequenceData tutorialSequenceData, bool isStartTriggerData = true)
        {
            var triggerData = isStartTriggerData ? tutorialSequenceData.startTriggerData : tutorialSequenceData.endTriggerData;
            Component component = null;
            bool isTriggerExist = false;
            switch (triggerData.triggerType)
            {
                case TriggerType.Manual:
                    break;

                case TriggerType.Collider:
                    if (triggerData.colliderTriggerData.collider != null)
                    {
                        component = triggerData.colliderTriggerData.collider;
                        var tutorialTriggerObjects = triggerData.colliderTriggerData.collider.GetComponents<TutorialTriggerObject>();
                        for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                        {
                            if (tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                            {
                                isTriggerExist = true;
                                break;
                            }
                        }
                    }
                    break;

                case TriggerType.UI:
                    if (triggerData.graphicTriggerData.graphic != null)
                    {
                        component = triggerData.graphicTriggerData.graphic;
                        var tutorialTriggerObjects = triggerData.graphicTriggerData.graphic.GetComponents<TutorialTriggerObject>();
                        for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                        {
                            if (tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                            {
                                isTriggerExist = true;
                                break;
                            }
                        }
                    }
                    break;
            }

            if (triggerData.triggerType == TriggerType.Collider || triggerData.triggerType == TriggerType.UI)
            {
                if (!isTriggerExist && component != null)
                {
                    var tutorialTriggerObject = component.gameObject.AddComponent<TutorialTriggerObject>();
                    tutorialTriggerObject.SetupTriggerData(triggerData, tutorialSequenceData.sequenceIndex, -1, isStartTriggerData, true);
                }
                else
                {
                    LogWarning("Cannot add trigger object to the component: " + " on " + (isStartTriggerData ? "Start Trigger" : "End Trigger") +
                               " with sequence index: " + tutorialSequenceData.sequenceIndex + ", as there is another trigger object on it. " +
                               "Please use different collider/graphic for different sequence");
                }
            }
        }

        protected bool CanPlaySequence(TutorialSequenceData tutorialSequenceData)
        {
            if (tutorialSequenceData.state != PlayState.Idle)
            {
                LogWarning("Sequence: " + tutorialSequenceData.sequenceName + " cannot be started, The sequence is at state: " +
                                tutorialSequenceData.state + ". Please reset the sequence object");
                return false;
            }

            return true;
        }

        protected void StartSequence(TutorialSequenceData tutorialSequenceData, bool autoStart = true)
        {
            currentPlayingSequenceData = tutorialSequenceData;
            currentSequenceStep = tutorialSequenceData.sequenceIndex;
            UpdateTutorialContainer(tutorialSequenceData);
            ClearTutorialSequenceTriggerData(tutorialSequenceData, true);
            InitializeTutorialSequenceTriggerData(tutorialSequenceData, false);
            SetState(ref tutorialSequenceData.state, PlayState.Start);
            RunEvent(tutorialSequenceData.EventsDictionary, EventType.OnStart);

            for (int i = 0; i < tutorialSequenceData.tutorialBlocksData.Length; i++)
                InitializeTutorialBlockTriggerData(tutorialSequenceData.tutorialBlocksData[i], true);

            if (autoStart)
            {
                var firstBlockIndex = 1;
                PlayTutorial(firstBlockIndex);
            }

            SetState(ref tutorialSequenceData.state, PlayState.Executing);
        }

        protected void StopSquence(bool autoAdvance)
        {
            SetState(ref currentPlayingSequenceData.state, PlayState.Ending);
            RunEvent(currentPlayingSequenceData.EventsDictionary, EventType.OnEnding);

            foreach (var tutorialBlockData in currentPlayingTutorialBlocksData)
                StopTutorial(tutorialBlockData.blockIndex, false);

            if (markTutorialCompletedBySequenceIndex && !isEnoughRequirementToMarkTutorialCompleted)
                isEnoughRequirementToMarkTutorialCompleted = markTutorialCompletedSequenceIndex == currentSequenceStep;
            currentPlayingTutorialBlocksData.Clear();
            SetState(ref currentPlayingSequenceData.state, PlayState.End);
            RunEvent(currentPlayingSequenceData.EventsDictionary, EventType.OnComplete);
            var switchData = currentPlayingSequenceData.switchData;
            currentPlayingSequenceData = null;

            if (currentSequenceStep < tutorialSequencesData.Length)
            {
                currentSequenceStep++;
                bool addStartTriggerData = true;
                if (autoAdvance)
                {
                    switch (switchData.switchType)
                    {
                        case SwitchType.None:
                            break;

                        case SwitchType.Automatic:
                            addStartTriggerData = false;
                            PlaySequence(currentSequenceStep);
                            break;

                        case SwitchType.Index:
                            addStartTriggerData = false;
                            PlaySequence(switchData.index);
                            break;

                        case SwitchType.Name:
                            addStartTriggerData = false;
                            PlaySequence(switchData.name);
                            break;
                    }
                }

                if (addStartTriggerData)
                {
                    var newSequence = GetSequence(currentSequenceStep);
                    if (newSequence != null)
                        InitializeTutorialSequenceTriggerData(newSequence, true);
                }
            }
        }

        protected bool CanPlayTutorial(TutorialBlockData tutorialBlockData)
        {
            if (!currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
            {
                for (int i = 0; i < tutorialBlockData.blockIndicatorsData.Length; i++)
                {
                    if (tutorialBlockData.blockIndicatorsData[i].indicatorPrefab == null)
                    {
                        LogWarning("Tutorial Block " + tutorialBlockData.name + " do not have indicator prefab for: " + tutorialBlockData.blockIndicatorsData[i].GetType());
                        return false;
                    }
                }

                return true;
            }
            else LogWarning("Tutorial Block " + tutorialBlockData.name + " cannot be started, it is currently playing.");

            return false;
        }

        protected void StartTutorial(TutorialBlockData tutorialBlockData)
        {
            if (!currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
                currentPlayingTutorialBlocksData.Add(tutorialBlockData);

            currentPlayingSequenceData.CurrentStep = tutorialBlockData.blockIndex;
            savedCurrentTutorialIndexData = new TutorialIndexData(currentPlayingSequenceData.sequenceIndex, currentPlayingSequenceData.CurrentStep);
            ClearTutorialBlockTriggerData(tutorialBlockData, true);
            InitializeTutorialBlockTriggerData(tutorialBlockData, false);
            RunEvent(tutorialBlockData.EventsDictionary, EventType.OnStart);
            InitializeBlockIndicators(tutorialBlockData);
        }

        protected void StopTutorialBlock(TutorialBlockData tutorialBlockData, bool autoAdvance = true, bool stepBack = false)
        {
            RunEvent(tutorialBlockData.EventsDictionary, EventType.OnEnding);
            ClearTutorialBlockTriggerData(tutorialBlockData, false);

            if (tutorialBlockIndicatorsDictionary.ContainsKey(tutorialBlockData))
            {
                for (int i = 0; i < tutorialBlockIndicatorsDictionary[tutorialBlockData].Count; i++)
                {
                    var tutorialBlockIndicator = tutorialBlockIndicatorsDictionary[tutorialBlockData][i];
                    if (tutorialBlockIndicator != null)
                        tutorialBlockIndicator.Stop();
                }

                SetTutorialComplete(tutorialBlockData);

                if (!stepBack)
                {
                    if (CheckSequenceEnd(tutorialBlockData))
                    {
                        StopSequence();
                    }
                    else
                    {
                        if (currentPlayingSequenceData != null && tutorialBlockData.switchData.switchType == SwitchType.Automatic)
                            currentPlayingSequenceData.CurrentStep++;

                        bool addStartTriggerData = true;
                        if (autoAdvance)
                        {
                            switch (tutorialBlockData.switchData.switchType)
                            {
                                case SwitchType.None:
                                    break;

                                case SwitchType.Automatic:
                                    addStartTriggerData = false;
                                    PlayTutorial();

                                    break;
                                case SwitchType.Index:
                                    addStartTriggerData = false;
                                    PlayTutorial(tutorialBlockData.switchData.index);
                                    break;

                                case SwitchType.Name:
                                    addStartTriggerData = false;
                                    PlayTutorial(tutorialBlockData.switchData.name);
                                    break;
                            }
                        }

                        if (addStartTriggerData)
                        {
                            var tutorialBlock = GetCurrentTutorialBlock(tutorialBlockData.sequenceIndex, tutorialBlockData.blockIndex + 1);
                            if (tutorialBlock != null)
                                InitializeTutorialBlockTriggerData(tutorialBlock, true);
                        }
                    }
                }
            }
        }

        protected void SetTutorialComplete(TutorialBlockData tutorialBlockData)
        {
            if (tutorialBlockIndicatorsDictionary.ContainsKey(tutorialBlockData))
            {
                for (int i = 0; i < tutorialBlockIndicatorsDictionary[tutorialBlockData].Count; i++)
                {
                    var tutorialBlockIndicator = tutorialBlockIndicatorsDictionary[tutorialBlockData][i];
                    if (tutorialBlockIndicator != null)
                        tutorialBlockIndicator.gameObject.SetActive(false);
                }
            }

            if (currentPlayingTutorialBlocksData.Contains(tutorialBlockData))
            {
                if (!removalTutorialBlocksData.Contains(tutorialBlockData))
                    removalTutorialBlocksData.Add(tutorialBlockData);
                else
                    LogWarning("Removal list contains tutorial block " + tutorialBlockData.name);
            }
            else LogWarning("Could not find " + tutorialBlockData.name + " when removing from playing list.");

            RunEvent(tutorialBlockData.EventsDictionary, EventType.OnComplete);
            ClearTutorialBlockIndicators(tutorialBlockData, true);
        }

        protected void InitializeBlockIndicators(TutorialBlockData tutorialBlockData)
        {
            ClearTutorialBlockIndicators(tutorialBlockData);
            var tutorialBlockIndicators = new List<TutorialBlockIndicator>();
            for (int i = 0; i < tutorialBlockData.blockIndicatorsData.Length; i++)
            {
                var blockIndicator = CreateBlockIndicator(tutorialBlockData.blockIndicatorsData[i], tutorialBlockData);
                tutorialBlockIndicators.Add(blockIndicator);
            }
            for (int i = 0; i < tutorialBlockIndicators.Count; i++)
                tutorialBlockIndicators[i].RearrangeOrder();
        }

        protected TutorialBlockIndicator CreateBlockIndicator(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            var tutorialBlockIndicatorGameObject = Instantiate(tutorialBlockIndicatorData.indicatorPrefab);
            var tutorialBlockIndicator = tutorialBlockIndicatorGameObject.GetComponent<TutorialBlockIndicator>();
            if (tutorialBlockIndicator != null)
            {
                if (!tutorialBlockIndicatorsDictionary.ContainsKey(tutorialBlockData))
                    tutorialBlockIndicatorsDictionary.Add(tutorialBlockData, new List<TutorialBlockIndicator>() { tutorialBlockIndicator });
                else
                    tutorialBlockIndicatorsDictionary[tutorialBlockData].Add(tutorialBlockIndicator);
                tutorialBlockIndicator.transform.SetParent(tutorialContainerCanvas.transform, false);
                tutorialBlockIndicator.transform.localPosition = Vector3.zero;
                tutorialBlockIndicator.transform.localRotation = Quaternion.identity;
                tutorialBlockIndicator.transform.localScale = Vector3.one;
                (tutorialBlockIndicator.transform as RectTransform).sizeDelta = Vector2.zero;
                tutorialBlockIndicator.Init(tutorialBlockIndicatorData, tutorialBlockData);
                return tutorialBlockIndicator;
            }
            else
            {
                LogWarning("Could not find the indicator prefab in the tutorial block data :" + tutorialBlockData.name);
                return null;
            }
        }

        protected void InitializedTutorialBlock(TutorialBlockData tutorialBlockData, bool reset = true)
        {
            tutorialBlockData.InitializedEvents();
            UninitializeTargetInfo(tutorialBlockData);
            InitializeTargetInfo(tutorialBlockData, reset);
        }

        protected void InitializeTargetInfo(TutorialBlockData tutorialBlockData, bool reset = true)
        {
            if (tutorialBlockData != null)
            {
                for (int i = 0; i < tutorialBlockData.focusTargetsData.Length; i++)
                    UpdateTargetRect(tutorialBlockData.focusTargetsData[i], reset);
            }
        }

        protected void UninitializeTargetInfo(TutorialBlockData tutorialBlockData)
        {
            if (tutorialBlockData != null)
            {
                for (int i = 0; i < tutorialBlockData.focusTargetsData.Length; i++)
                {
                    tutorialBlockData.focusTargetsData[i].runtimeTarget = null;
                    tutorialBlockData.focusTargetsData[i].ownerCanvas = null;
                }
            }
        }

        protected void UpdateTargetRect(TutorialBlockTargetData tutorialBlockTargetData, bool reset = true)
        {
            if (tutorialBlockTargetData != null && tutorialBlockTargetData.target != null)
            {
                tutorialBlockTargetData.runtimeTarget = tutorialBlockTargetData.target;
                if (reset && tutorialBlockTargetData.targetBoundType != TutorialTargetBoundType.RectTransform)
                {
                    var parent = tutorialBlockTargetData.runtimeTarget.transform.parent;
                    tutorialBlockTargetData.runtimeTarget.transform.SetParent(null);
                    tutorialBlockTargetData.UnscaledBoundsDictionary.Clear();
                    tutorialBlockTargetData.UnscaledBoundsDictionary = TransformUtility.CalculateAllTransBound(tutorialBlockTargetData.runtimeTarget.transform);
                    tutorialBlockTargetData.UpdateTargetBound();
                    tutorialBlockTargetData.runtimeTarget.transform.SetParent(parent);
                }

                tutorialBlockTargetData.UpdateCanvas();
                tutorialBlockTargetData.UpdateTargetScreenRect();
            }
        }

        protected void InitializeTutorialBlockTriggerData(TutorialBlockData tutorialBlockData, bool isStartTriggerData)
        {
            var triggerData = isStartTriggerData ? tutorialBlockData.startTriggerData : tutorialBlockData.endTriggerData;
            Component component = null;
            bool isTriggerExist = false;
            switch (triggerData.triggerType)
            {
                case TriggerType.Manual:
                    break;

                case TriggerType.Collider:
                    if (triggerData.colliderTriggerData.collider != null)
                    {
                        component = triggerData.colliderTriggerData.collider;
                        var tutorialTriggerObjects = triggerData.colliderTriggerData.collider.GetComponents<TutorialTriggerObject>();
                        for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                        {
                            if (!tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                            {
                                isTriggerExist = true;
                                break;
                            }
                        }
                    }
                    break;

                case TriggerType.UI:
                    if (triggerData.graphicTriggerData.graphic != null)
                    {
                        component = triggerData.graphicTriggerData.graphic;
                        var tutorialTriggerObjects = triggerData.graphicTriggerData.graphic.GetComponents<TutorialTriggerObject>();
                        for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                        {
                            if (!tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                            {
                                isTriggerExist = true;
                                break;
                            }
                        }
                    }
                    break;
            }

            if (triggerData.triggerType == TriggerType.Collider || triggerData.triggerType == TriggerType.UI)
            {
                if (!isTriggerExist && component != null)
                {
                    var tutorialTriggerObject = component.gameObject.AddComponent<TutorialTriggerObject>();
                    tutorialTriggerObject.SetupTriggerData(triggerData, tutorialBlockData.sequenceIndex, tutorialBlockData.blockIndex, isStartTriggerData, false);
                }
                else LogWarning("Cannot add trigger object to the Component: " + " on " + (isStartTriggerData ? "Start Trigger" : "End Trigger") +
                                " with turorial block's name: " + tutorialBlockData.name + ", as there is another trigger object on it.");
            }
        }

        protected void UpdateTutorialContainer(TutorialSequenceData tutorialSequenceData)
        {
            var tutorialContainer = tutorialContainerCanvas.GetComponent<TutorialContainer>();
            tutorialContainer.Init(TutorialType, tutorialSequenceData.CanSkip);
        }

        protected void ResetTutorialContainer()
        {
            var tutorialContainer = tutorialContainerCanvas.GetComponent<TutorialContainer>();
            tutorialContainer.ResetState();
        }

        protected bool CheckSequenceEnd(TutorialBlockData tutorialBlockData)
        {
            if (currentPlayingSequenceData != null)
            {
                if (tutorialBlockData.blockIndex == currentPlayingSequenceData.tutorialBlocksData.Length)
                    return true;
            }
            return false;
        }

        protected bool CheckSequenceEnd(int index)
        {
            if (currentPlayingSequenceData != null)
            {
                if (index == currentPlayingSequenceData.tutorialBlocksData.Length)
                    return true;
            }
            return false;
        }

        protected void ClearTutorialBlockIndicators(TutorialBlockData tutorialBlockData, bool remove = false)
        {
            if (tutorialBlockIndicatorsDictionary.ContainsKey(tutorialBlockData))
            {
                for (int i = tutorialBlockIndicatorsDictionary[tutorialBlockData].Count - 1; i >= 0; i--)
                {
                    var indicatorGameObject = tutorialBlockIndicatorsDictionary[tutorialBlockData][i].gameObject;
                    Destroy(indicatorGameObject);
                }

                tutorialBlockIndicatorsDictionary[tutorialBlockData].Clear();
                if (remove)
                    tutorialBlockIndicatorsDictionary.Remove(tutorialBlockData);
            }
        }

        protected void ClearAllTutorialBlockIndicators()
        {
            if (tutorialBlockIndicatorsDictionary != null)
            {
                foreach (var entry in tutorialBlockIndicatorsDictionary)
                    ClearTutorialBlockIndicators(entry.Key, false);
                tutorialBlockIndicatorsDictionary.Clear();
            }
        }

        protected TutorialBlockData GetCurrentTutorialBlockData(TutorialSequenceData tutorialSequenceData, int step)
        {
            if (step < tutorialSequenceData.tutorialBlocksData.Length && tutorialSequenceData.tutorialBlocksData.Length > 0)
                return tutorialSequenceData.tutorialBlocksData[Mathf.Max(0, step)];
            else
                return null;
        }

        protected TutorialBlockData GetCurrentTutorialBlockData(TutorialSequenceData stageObj, string name)
        {
            for (int i = 0; i < stageObj.tutorialBlocksData.Length; i++)
            {
                if (stageObj.tutorialBlocksData[i].name == name)
                    return stageObj.tutorialBlocksData[i];
            }
            return null;
        }

        protected TutorialBlockData GetCurrentTutorialBlock(int sequenceIndex, int step)
        {
            var sequence = GetSequence(sequenceIndex);
            if (sequence != null)
                return GetCurrentTutorialBlockData(sequence, step);
            else
                return null;
        }

        protected TutorialSequenceData GetSequence(int sequenceStep)
        {
            if (sequenceStep < tutorialSequencesData.Length && tutorialSequencesData.Length > 0)
                return tutorialSequencesData[Mathf.Max(0, sequenceStep)];
            else
                return null;
        }

        protected TutorialSequenceData GetCurrentSequence(string sequenceName)
        {
            for (int i = 0; i < tutorialSequencesData.Length; i++)
            {
                if (tutorialSequencesData[i].sequenceName == sequenceName)
                    return tutorialSequencesData[i];
            }
            return null;
        }

        protected TutorialIndexData SetUpRuntimeTutorial(TutorialRuntimeStepType tutorialRuntimeStepType, params TutorialRuntimeTarget[] tutorialRuntimeTargets)
        {
            var tutorialIndexData = TutorialIndexData.None;
            var tutorialRuntimeTargetSetUpData = tutorialRuntimeTargetsSetUpData.FirstOrDefault(x => x.tutorialRuntimeStepType == tutorialRuntimeStepType);
            if (tutorialRuntimeTargetSetUpData != null)
            {
                foreach (var tutorialRuntimeTargetSetUpIndexData in tutorialRuntimeTargetSetUpData.tutorialRuntimeTargetSetUpIndexesData)
                {
                    var sequenceIndex = tutorialRuntimeTargetSetUpIndexData.tutorialIndexData.tutorialSequenceIndex;
                    var blockIndex = tutorialRuntimeTargetSetUpIndexData.tutorialIndexData.tutorialSequenceBlockIndex;
                    var mainTutorialRuntimeTarget = tutorialRuntimeTargets.Length > 0 ? tutorialRuntimeTargets[0] : null;
                    UnityAction onClickActionCallback = null;
                    if (mainTutorialRuntimeTarget != null)
                    {
                        onClickActionCallback = () => {
                            var clickable = mainTutorialRuntimeTarget.gameObject.GetComponentInChildren<IClickable>();
                            if (clickable != null)
                                clickable.Click();
                        };
                    }
                    if (tutorialSequencesData != null && sequenceIndex > 0 && sequenceIndex <= tutorialSequencesData.Length)
                    {
                        var tutorialSequenceData = tutorialSequencesData[sequenceIndex - 1];
                        if (tutorialSequenceData != null && tutorialSequenceData.tutorialBlocksData != null && blockIndex > 0 && blockIndex <= tutorialSequenceData.tutorialBlocksData.Length)
                        {
                            var tutorialBlockData = tutorialSequenceData.tutorialBlocksData[blockIndex-1];
                            if (tutorialBlockData != null)
                            {
                                tutorialBlockData.focusTargetsData = new TutorialBlockTargetData[tutorialRuntimeTargets.Length];
                                for (int i = 0; i < tutorialRuntimeTargets.Length; i++)
                                {
                                    var tutorialBlockTargetData = new TutorialBlockTargetData(tutorialRuntimeTargets[i].gameObject, tutorialRuntimeTargets[i].targetBoundType);
                                    tutorialBlockData.focusTargetsData[i] = tutorialBlockTargetData;
                                }
                            }
                            foreach (var tutorialBlockIndicatorData in tutorialBlockData.blockIndicatorsData)
                            {
                                if (tutorialBlockIndicatorData is TutorialBlockFocusZoneIndicatorData)
                                {
                                    var tutorialBlockFocusZoneIndicatorData = tutorialBlockIndicatorData as TutorialBlockFocusZoneIndicatorData;
                                    if (tutorialBlockFocusZoneIndicatorData.endTutorialByClickInFocusImage)
                                        tutorialBlockFocusZoneIndicatorData.AddEndTutorialAction(onClickActionCallback);
                                }
                            }
                        }
                    }
                    if (tutorialRuntimeTargetSetUpIndexData.isReponsibleForStartTutorial)
                        tutorialIndexData = new TutorialIndexData(sequenceIndex, blockIndex);
                }
            }
            return tutorialIndexData;
        }

        protected void ClearTutorialSequenceTriggerData(TutorialSequenceData tutorialSequenceData, bool isStartTriggerData)
        {
            var triggerData = isStartTriggerData ? tutorialSequenceData.startTriggerData : tutorialSequenceData.endTriggerData;
            Transform triggerTransform = null;

            switch (triggerData.triggerType)
            {
                case TriggerType.Collider:
                    triggerTransform = triggerData.colliderTriggerData.collider.transform;
                    break;

                case TriggerType.UI:
                    triggerTransform = triggerData.graphicTriggerData.graphic.transform;
                    break;
            }

            if (triggerTransform != null)
            {
                var tutorialTriggerObjects = triggerTransform.GetComponents<TutorialTriggerObject>();
                for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                {
                    if (tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                        Destroy(tutorialTriggerObjects[i]);
                }
            }
        }

        protected void ClearTutorialBlockTriggerData(TutorialBlockData tutorialBlockData, bool isStartTriggerData)
        {
            var triggerData = isStartTriggerData ? tutorialBlockData.startTriggerData : tutorialBlockData.endTriggerData;
            Transform triggerTransform = null;

            switch (triggerData.triggerType)
            {
                case TriggerType.Collider:
                    triggerTransform = triggerData.colliderTriggerData.collider.transform;
                    break;

                case TriggerType.UI:
                    triggerTransform = triggerData.graphicTriggerData.graphic.transform;
                    break;
            }

            if (triggerTransform != null)
            {
                var tutorialTriggerObjects = triggerTransform.GetComponents<TutorialTriggerObject>();
                for (int i = 0; i < tutorialTriggerObjects.Length; i++)
                {
                    if (!tutorialTriggerObjects[i].IsSequence && tutorialTriggerObjects[i].IsStartTriggerData == isStartTriggerData)
                        Destroy(tutorialTriggerObjects[i]);
                }
            }
        }

        protected void CheckSequenceEventTrigger(TutorialSequenceData tutorialSequenceData, bool isStartTriggerData = true)
        {
            var triggerData = isStartTriggerData ? tutorialSequenceData.startTriggerData : tutorialSequenceData.endTriggerData;
            if (triggerData != null)
            {
                switch (triggerData.triggerType)
                {
                    case TriggerType.KeyCode:
                        if (InputManager.GetKeyDown(triggerData.keyCode))
                            if (isStartTriggerData)
                                PlaySequence(tutorialSequenceData.sequenceIndex);
                            else
                                StopSequence();
                        break;
                }
            }
        }

        protected void CheckBlockEventTrigger(TutorialBlockData tutorialBlockData, bool isStartTriggerData = true)
        {
            var triggerData = isStartTriggerData ? tutorialBlockData.startTriggerData : tutorialBlockData.endTriggerData;
            if (triggerData != null)
            {
                switch (triggerData.triggerType)
                {
                    case TriggerType.KeyCode:
                        if (InputManager.GetKeyDown(triggerData.keyCode))
                        {
                            if (isStartTriggerData)
                                PlayTutorial(tutorialBlockData.sequenceIndex, tutorialBlockData.blockIndex);
                            else
                                StopTutorial(tutorialBlockData.sequenceIndex, tutorialBlockData.blockIndex);
                        }
                        break;
                }
            }
        }

        protected void UpdateSequence(TutorialSequenceData tutorialSequenceData)
        {
            if (tutorialSequenceData.state != PlayState.Idle && tutorialSequenceData.state != PlayState.End)
                RunEvent(tutorialSequenceData.EventsDictionary, EventType.OnUpdate);
        }

        protected void UpdateBlockEvent(TutorialBlockData tutorialBlockData)
            => RunEvent(tutorialBlockData.EventsDictionary, EventType.OnUpdate);

        protected bool RunEvent(Dictionary<EventType, TutorialEventData> eventsDictionary, EventType type)
        {
            var foundEvent = TutorialEventData.GetEvent(eventsDictionary, type);
            if (foundEvent != null)
            {
                foundEvent.subEvent?.Invoke();
                return true;
            }
            return false;
        }

        protected void SetState(ref PlayState state, PlayState value)
            => state = value;

        protected void LogWarning(string message) { }

        protected void LeaveApplication()
        {
            if (currentPlayingSequenceData != null)
                RunEvent(currentPlayingSequenceData.EventsDictionary, EventType.OnQuitGame);
        }

        protected void SendTutorialTracking(string tutorialSequenceId, int tutorialSequenceStepIndex)
        {
#if TRACKING
            FirebaseManager.Instance.TrackTutorial(tutorialSequenceId, tutorialSequenceStepIndex);
#endif
        }

        #endregion Class Methods
    }

    [Serializable]
    public class TutorialRuntimeTargetSetUpData
    {
        #region Members

        public TutorialRuntimeStepType tutorialRuntimeStepType;
        public TutorialRuntimeTargetSetUpIndexData[] tutorialRuntimeTargetSetUpIndexesData;

        #endregion Members
    }

    [Serializable]
    public struct TutorialRuntimeTargetSetUpIndexData
    {
        #region Members

        public bool isReponsibleForStartTutorial;
        public TutorialIndexData tutorialIndexData;

        #endregion Members
    }

    [Serializable]
    public struct TutorialIndexData
    {
        #region Members

        public int tutorialSequenceIndex;
        public int tutorialSequenceBlockIndex;
        public static readonly TutorialIndexData None = new TutorialIndexData(-1, - 1);

        #endregion Members

        #region Struct Methods

        public TutorialIndexData(int tutorialSequenceIndex, int tutorialSequenceBlockIndex)
        {
            this.tutorialSequenceIndex = tutorialSequenceIndex;
            this.tutorialSequenceBlockIndex = tutorialSequenceBlockIndex;
        }

        public static bool operator ==(TutorialIndexData x, TutorialIndexData y)
        {
            return x.tutorialSequenceIndex == y.tutorialSequenceIndex &&
                   x.tutorialSequenceBlockIndex == y.tutorialSequenceBlockIndex;
        }

        public static bool operator !=(TutorialIndexData x, TutorialIndexData y)
        {
            return x.tutorialSequenceIndex != y.tutorialSequenceIndex &&
                   x.tutorialSequenceBlockIndex != y.tutorialSequenceBlockIndex;
        }

        public override bool Equals(object other)
        {
            if (!(other is TutorialIndexData))
                return false;
            return this == (TutorialIndexData)other;
        }

        public override int GetHashCode()
        {
            return tutorialSequenceIndex.GetHashCode() +
                   tutorialSequenceBlockIndex.GetHashCode();
        }

        #endregion Struct Methods
    }

    public enum TutorialRuntimeStepType
    {
        LightenNextStageButton = 1,
        LightenFirstEquipmentItem = 2,
        LightenEquipButton = 3,
        LightenGachaButton = 4,
        LightenHeroEquipButton = 5,
        LightenBattleButton = 6,
        LightenJuiceFactoryObject = 7,
        ShowJuiceFactoryInfo = 8,
        ShowHowToMakeJuiceJug = 9,
        NotifyUnlockJuiceHouse = 10,
        LightenJuiceFactoryOutput = 11,
        LightenOrderStructureObject = 12,
        ShowOrderStructureInfo = 13,
        LightenSendOrderReturnButton = 14,
        ReceiveGachaToken = 15,
        LightenStageItem = 16,
        LightenPlayStageButton = 17,
        LightenGachaBackButton = 18,
        LightenSkipTimeButton = 19,
        LightenUseSkipFiveMinuteButton = 20,
        LightenSkillTreeTab = 21,
        LightenUpgradeSkillTreeButton = 22,
        LightenSkillTreeSecondBrandItemButton = 23,
        LightenGacha1PremiumButton = 24,
    }
}