#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class BehavioralEntityEditor<BT> : Editor where BT : Enum
    {
        #region Members

        protected const string BEHAVIORS_TYPES_PROPERTY_NAME = "behaviorTypes";
        protected static readonly string NotityBehaviorTypeEmpty = "Behavior can not be empty!";
        protected static readonly string NotityBehaviorTypeDuplicated = "Behavior can not be duplicated!";
        protected SerializedProperty behaviorTypesProperty;
        protected ReorderableList behaviorTypesReorderableList;

        #endregion Members

        #region Properties

        protected abstract BehavioralEntity<BT> BehavioralEntity { get; }

        #endregion Properties

        #region API Methods

        private void OnEnable()
        {
            behaviorTypesProperty = serializedObject.FindProperty(BEHAVIORS_TYPES_PROPERTY_NAME);
            behaviorTypesReorderableList = new ReorderableList(serializedObject, behaviorTypesProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, behaviorTypesProperty.displayName),
                drawElementCallback = (rect, index, focused, active) => {
                    var element = behaviorTypesProperty.GetArrayElementAtIndex(index);
                    var behaviorTypes = BehavioralEntity.BehaviorTypes;
                    var color = GUI.color;
                    string addedBehaviorType = Enum.ToObject(typeof(BT), element.enumValueIndex).ToString();
                    if (element.enumValueIndex < 0)
                        GUI.color = Color.red;
                    else if (behaviorTypes.Any(x => x.ToString().Equals(addedBehaviorType)) && behaviorTypes.Count(x => x.ToString().Equals(addedBehaviorType)) > 1)
                        GUI.color = Color.red;
                    GUI.color = color;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element)), element);
                    if (element.enumValueIndex < 0)
                    {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), NotityBehaviorTypeEmpty, MessageType.Error);
                    }
                    else if (behaviorTypes.Any(x => x.ToString().Equals(addedBehaviorType)) && behaviorTypes.Count(x => x.ToString().Equals(addedBehaviorType)) > 1)
                    {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), NotityBehaviorTypeDuplicated, MessageType.Error);
                    }
                },
                elementHeightCallback = index => {
                    var element = behaviorTypesProperty.GetArrayElementAtIndex(index);
                    var behaviorTypes = BehavioralEntity.BehaviorTypes;
                    var height = EditorGUI.GetPropertyHeight(element);
                    string addedBehaviorType = Enum.ToObject(typeof(BT), element.enumValueIndex).ToString();
                    if (element.enumValueIndex < 0)
                        height += EditorGUIUtility.singleLineHeight;
                    else if (behaviorTypes.Any(x => x.ToString().Equals(addedBehaviorType)) && behaviorTypes.Count(x => x.ToString().Equals(addedBehaviorType)) > 1)
                        height += EditorGUIUtility.singleLineHeight;
                    return height;
                },
                onAddCallback = list => {
                    list.serializedProperty.arraySize++;
                    var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    newElement.intValue = -1;
                }
            };
        }

        #endregion API Methods

        #region Class Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            behaviorTypesReorderableList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        #endregion Class Methods
    }
}

#endif