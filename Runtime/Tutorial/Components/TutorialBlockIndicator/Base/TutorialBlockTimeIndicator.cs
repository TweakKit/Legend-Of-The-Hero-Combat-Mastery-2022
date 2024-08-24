using UnityEngine;

namespace Runtime.Tutorial
{
    /// <summary>
    /// Base indicator for indicators without the target still run.
    /// </summary>
    public class TutorialBlockTimeIndicator<T> : TutorialBlockIndicator where T : TutorialBlockTimeIndicatorData
    {
        #region Properties

        public new T OwnerBlockIndicatorData => base.OwnerBlockIndicatorData as T;

        #endregion Properties

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            InitStuff();
        }

        protected virtual void InitStuff()
        {
            if (OwnerBlockIndicatorData.maskPrefab != null)
            {
                var tutorialBlockIndicatorPropGameObject = Instantiate(OwnerBlockIndicatorData.maskPrefab);
                if (tutorialBlockIndicatorPropGameObject != null)
                {
                    var tutorialBlockIndicatorProp = tutorialBlockIndicatorPropGameObject.GetComponent<TutorialBlockIndicatorProp>();
                    if (tutorialBlockIndicatorProp != null)
                    {
                        if (!props.Contains(tutorialBlockIndicatorProp))
                            props.Add(tutorialBlockIndicatorProp);

                        tutorialBlockIndicatorPropGameObject.transform.SetParent(transform);
                        tutorialBlockIndicatorPropGameObject.transform.localPosition = Vector3.zero;
                        tutorialBlockIndicatorPropGameObject.transform.localRotation = Quaternion.identity;
                        tutorialBlockIndicatorPropGameObject.transform.localScale = Vector3.one;
                        tutorialBlockIndicatorPropGameObject.gameObject.SetActive(false);
                    }
                    else Destroy(tutorialBlockIndicatorPropGameObject);
                }
            }

            PostInit();
        }

        protected override void PostInit()
        {
            base.PostInit();
            for (int i = 0; i < props.Count; i++)
            {
                ConfigProp(props[i]);
                props[i].gameObject.SetActive(true);
            }
            SetVisibility(true);
        }

        protected virtual void ConfigProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp) { }

        #endregion Class Methods
    }
}