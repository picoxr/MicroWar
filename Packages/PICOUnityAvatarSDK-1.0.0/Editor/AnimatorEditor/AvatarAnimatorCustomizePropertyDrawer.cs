using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pico
{
    namespace Avatar
    {

        #region StringDropDownStatic Attribute. Need to provide static array.
        [CustomPropertyDrawer(typeof(StringDropDownStatic))]
        public class StringInListDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var stringInList = attribute as StringDropDownStatic;
                var list = stringInList.List;
                if (property.propertyType == SerializedPropertyType.String)
                {
                    int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                    index = EditorGUI.Popup(position, property.displayName, index, list);
                    property.stringValue = list[index];
                }
                else if (property.propertyType == SerializedPropertyType.Integer)
                {
                    property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, list);
                }
                else
                {
                    base.OnGUI(position, property, label);
                }
            }
        }
        #endregion


        #region StringDropDown custumize class. Need to provide array or node.
        [CustomPropertyDrawer(typeof(StringDropDown))]
        public class StringDropDownDrawer : PropertyDrawer
        {
            List<string> stringList = new List<string>();
            string[] strArray;
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                //find property
                SerializedProperty stringArray = property.FindPropertyRelative(nameof(StringDropDown.stringArray));
                SerializedProperty currentString = property.FindPropertyRelative(nameof(StringDropDown.currentString));
                SerializedProperty name = property.FindPropertyRelative(nameof(StringDropDown.name));

                //update array
                stringList.Clear();
                for (int i = 0; i < stringArray.arraySize; i++)
                {
                    stringList.Add(stringArray.GetArrayElementAtIndex(i).stringValue);
                }
                strArray = stringList.ToArray();

                //draw popup
                Rect popupRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
                int index = Mathf.Max(0, Array.IndexOf(strArray, currentString.stringValue));
                if (strArray.Length > 0)
                {
                    index = EditorGUI.Popup(popupRect, name.stringValue, index, strArray);
                    currentString.stringValue = strArray[index];
                }

                EditorGUI.EndProperty();
            }
        }
        #endregion


        #region EnumMultiAttribute. This Attribute allows Enum multiple selection.
        [CustomPropertyDrawer(typeof(EnumMultiAttribute))]
        public class EnumMultiAttributeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EnumMultiAttribute enumMultiAttribute = attribute as EnumMultiAttribute;
                Rect lableRect = new Rect(position) { width = EditorGUIUtility.labelWidth };
                Rect maskFieldRect = new Rect(lableRect)
                {
                    x = lableRect.x + lableRect.width + 2,
                    width = EditorGUIUtility.currentViewWidth - lableRect.width - lableRect.x - 6
                };
                EditorGUI.LabelField(lableRect, label);
                //Use MaskFIeld for enum multiple selection
                property.intValue = EditorGUI.MaskField(maskFieldRect, property.intValue, property.enumDisplayNames);

                // AvatarAnimationLayerGraph.GetAvatarMask((AvatarLayerMask)property.intValue);
            }

        }
        #endregion

        #region enable array edit
        [CustomPropertyDrawer(typeof(EnableArrayEdit))]
        public class EnableArrayEditDrawer : PropertyDrawer
        {
            PropertyDrawer drawer = null;
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                /*
                  The array is forbidden to modify. So if you want to modify the properties, 
                  you need to end the disableGroup first, draw the properties, 
                  and finally open the DisableGroup.
                */
                if(Application.isPlaying)
                    EditorGUI.EndDisabledGroup();

                if(property.name == nameof(BlendDirectClipItem.parameter))
                {
                    if(drawer == null)
                        drawer = UnityEditorAttributes.PropertyDrawerFinder.FindDrawerForType(typeof(StringDropDown));
                    drawer.OnGUI(position, property, label);
                }
                else 
                    EditorGUI.PropertyField(position,property);

                if(Application.isPlaying)
                    EditorGUI.BeginDisabledGroup(true);
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return base.GetPropertyHeight(property, label);
            }
        }

        [CustomPropertyDrawer(typeof(DisableEditRuntime))]
        public class DisableEditRuntimeDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                if(Application.isPlaying)
                    EditorGUI.BeginDisabledGroup(true);

                EditorGUI.PropertyField(position,property);

                if(Application.isPlaying)
                    EditorGUI.EndDisabledGroup();
            }
        }
        #endregion

        #region Condtion.
        [CustomPropertyDrawer(typeof(Transition))]
        public class TransitionDrawer : PropertyDrawer
        {
            float singleHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float labelWidth = EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing;

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position,label,property);
                
                Rect nameRect = new Rect(position)
                {
                    height = singleHeight
                };
                EditorGUI.PropertyField(nameRect, property, label, false);

                if (property.isExpanded)
                {
                    SerializedProperty autoSwitch = property.FindPropertyRelative(nameof(Transition.autoSwitch));
                    SerializedProperty duration = property.FindPropertyRelative(nameof(Transition.duration));
                    SerializedProperty conditionType = property.FindPropertyRelative(nameof(Transition.conditionType));
                    SerializedProperty parameter = property.FindPropertyRelative(nameof(Transition.parameter));
                    SerializedProperty boolOperator = property.FindPropertyRelative(nameof(Transition.boolOperator));
                    SerializedProperty uintOperator = property.FindPropertyRelative(nameof(Transition.uintOperator));
                    SerializedProperty floatOperator = property.FindPropertyRelative(nameof(Transition.floatOperator));
                    SerializedProperty comparedUint = property.FindPropertyRelative(nameof(Transition.comparedUint));
                    SerializedProperty comparedFloat = property.FindPropertyRelative(nameof(Transition.comparedFloat));

                    AvatarConditionType type = (AvatarConditionType)Enum.Parse(typeof(AvatarConditionType), conditionType.enumNames[conditionType.enumValueIndex]);


                    if(duration.floatValue < 0) duration.floatValue = 0;

                    Rect pos = new Rect(nameRect) { y = nameRect.y + singleHeight };
                    EditorGUI.PropertyField(pos, autoSwitch);

                    pos = new Rect(pos) { y = pos.y + singleHeight };
                    EditorGUI.PropertyField(pos, duration);

                    if(autoSwitch.boolValue == false)
                    {
                        if(Application.isPlaying)
                            EditorGUI.BeginDisabledGroup(true);
                        
                        pos = new Rect(pos) { y = pos.y + singleHeight };
                        EditorGUI.PropertyField(pos, conditionType);

                        if(Application.isPlaying)
                            EditorGUI.EndDisabledGroup();

                        Rect tempRect = new Rect(pos) { y = pos.y + singleHeight, width = labelWidth };
                        EditorGUI.LabelField(tempRect, "Parameter");
                        tempRect = new Rect(pos) { y = pos.y + singleHeight, x = pos.x + labelWidth, width = pos.width - labelWidth };
                        EditorGUI.PropertyField(tempRect, parameter);

                        pos = new Rect(pos) { y = pos.y + singleHeight * 2 };
                        if (type == AvatarConditionType.Bool)
                        {
                            EditorGUI.PropertyField(pos, boolOperator);
                        }
                        else if (type == AvatarConditionType.Uint)
                        {
                            EditorGUI.PropertyField(pos, uintOperator);
                            pos = new Rect(pos) { y = pos.y + singleHeight };
                            EditorGUI.PropertyField(pos, comparedUint);
                        }
                        else if (type == AvatarConditionType.Float)
                        {
                            EditorGUI.PropertyField(pos, floatOperator);
                            pos = new Rect(pos) { y = pos.y + singleHeight };
                            EditorGUI.PropertyField(pos, comparedFloat);
                        }
                    }
                }

                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                if(property.isExpanded)
                {
                    SerializedProperty autoSwitch = property.FindPropertyRelative(nameof(Transition.autoSwitch));
                    SerializedProperty conditionType = property.FindPropertyRelative(nameof(Transition.conditionType));
                    AvatarConditionType type = (AvatarConditionType)Enum.Parse(typeof(AvatarConditionType), conditionType.enumNames[conditionType.enumValueIndex]);
                    if(autoSwitch.boolValue == false)
                    {
                        if(type == AvatarConditionType.Trigger)
                            return singleHeight * 5;
                        else if(type == AvatarConditionType.Bool)
                            return singleHeight * 6;
                        else
                            return singleHeight * 7;
                    }else{
                        return singleHeight * 3;
                    } 
                }
                else 
                    return singleHeight;
            }

        }
        #endregion


        #region DictionarySerializable.
        [CustomPropertyDrawer(typeof(StringUintDictionary))]
        [CustomPropertyDrawer(typeof(StringFloatDictionary))]
        [CustomPropertyDrawer(typeof(StringBoolDictionary))]
        public class AnyDictionaryDrawer : DictionarySerializableDrawer { }

        public class DictionarySerializableDrawer : PropertyDrawer
        {
            const string keyName = "_keys";
            const string valueName = "_values";
            const float keyValueBlank = 15f;

            //Add and minus button icon
            GUIContent _iconPlus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus").image, "Add item");
            GUIContent _iconMinus = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus").image, "Remove item");
            GUIStyle _style = GUIStyle.none;
            //Add and minus button size of x
            float buttonSize = 0;
            //Action of add or minus
            ItemAction buttonAction = ItemAction.None;
            //Action index
            int buttonActionIndex = -1;
            //store all keys
            HashSet<string> keyNameSet = new HashSet<string>();

            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                label = EditorGUI.BeginProperty(position, label, property);

                //init button size and button action
                buttonSize = _style.CalcSize(_iconPlus).x;
                buttonAction = ItemAction.None;
                buttonActionIndex = -1;

                //get key and value array property 
                SerializedProperty keyArrayProperty = property.FindPropertyRelative(keyName);
                SerializedProperty valueArrayProperty = property.FindPropertyRelative(valueName);

                Rect nameRect = new Rect(position) { height = EditorGUIUtility.singleLineHeight };
                if (property.isExpanded) nameRect.width -= buttonSize;
                //draw proerty name label
                EditorGUI.PropertyField(nameRect, property, label, false);

                if (property.isExpanded)
                {
                    Rect addRect = new Rect(nameRect)
                    {
                        x = nameRect.xMax,
                        height = EditorGUIUtility.singleLineHeight
                    };
                    //draw add icon
                    if (GUI.Button(addRect, _iconPlus, _style))
                    {
                        buttonAction = ItemAction.Add;
                        buttonActionIndex = keyArrayProperty.arraySize;
                        // Debug.Log("click add item !!! buttonActionIndex = " + buttonActionIndex);
                    }

                    EditorGUI.indentLevel++;

                    Rect firstRect = new Rect(position)
                    {
                        y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                        height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                        xMax = position.xMax - buttonSize
                    };
                    keyNameSet.Clear();
                    //draw each item
                    foreach (var item in GetDictionaryItemsEnumerable(keyArrayProperty, valueArrayProperty))
                    {
                        DrawDictionaryItem(item, firstRect);
                        keyNameSet.Add(item.keyProperty.stringValue);
                    }

                    EditorGUI.indentLevel--;
                }

                //add or remove item
                switch (buttonAction)
                {
                    case ItemAction.Add:
                        AddKeyArrayAtIndex(keyArrayProperty, buttonActionIndex);
                        valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
                        break;
                    case ItemAction.Remove:
                        DeleteArrayAtIndex(keyArrayProperty, buttonActionIndex);
                        DeleteArrayAtIndex(valueArrayProperty, buttonActionIndex);
                        break;
                    case ItemAction.None:
                    default:
                        break;
                }

                EditorGUI.EndProperty();
            }

            //draw each dictionary item
            void DrawDictionaryItem(DictionaryItem item, Rect firstRect)
            {
                SerializedProperty keyProperty = item.keyProperty;
                SerializedProperty valueProperty = item.valueProperty;
                int index = item.index;

                Rect keyRect = new Rect(firstRect)
                {
                    y = firstRect.y + firstRect.height * index,
                    width = EditorGUIUtility.labelWidth - keyValueBlank,
                    height = EditorGUI.GetPropertyHeight(keyProperty)
                };
                //draw key
                EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none, false);

                Rect valueRect = new Rect(firstRect)
                {
                    x = firstRect.xMin + EditorGUIUtility.labelWidth,
                    y = firstRect.y + firstRect.height * index,
                    xMax = firstRect.xMax,
                    height = EditorGUI.GetPropertyHeight(keyProperty)
                };
                //draw value
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none, false);

                Rect removeRect = new Rect(valueRect)
                {
                    x = valueRect.xMax,
                    width = buttonSize,
                    height = EditorGUIUtility.singleLineHeight
                };
                //draw remove button
                if (GUI.Button(removeRect, _iconMinus, _style))
                {
                    buttonAction = ItemAction.Remove;
                    buttonActionIndex = index;
                    // Debug.Log("click minus button !!!");
                }
            }

            //delete from array
            public void DeleteArrayAtIndex(SerializedProperty arrayProperty, int index)
            {
                SerializedProperty itemProperty = arrayProperty.GetArrayElementAtIndex(index);
                if (itemProperty.propertyType == SerializedPropertyType.ObjectReference)
                    itemProperty.objectReferenceValue = null;
                arrayProperty.DeleteArrayElementAtIndex(index);
            }

            //add key to keyArray
            public void AddKeyArrayAtIndex(SerializedProperty keyArrayProperty, int index)
            {
                keyArrayProperty.InsertArrayElementAtIndex(index);
                SerializedProperty keyProperty = keyArrayProperty.GetArrayElementAtIndex(index);
                SetKeyName(keyProperty);
            }

            //set key name, ensure does not have the same name
            public void SetKeyName(SerializedProperty keyProperty)
            {
                StringBuilder newKeyName = new StringBuilder(keyProperty.stringValue);
                //If the name is the same, it will automatically append 0
                while (keyNameSet.Contains(newKeyName.ToString()))
                {
                    newKeyName.Append("0");
                }
                keyProperty.stringValue = newKeyName.ToString();
            }

            //get dictionary IEnumerable, loop through the dictionary
            IEnumerable<DictionaryItem> GetDictionaryItemsEnumerable(SerializedProperty keyArrayProperty, SerializedProperty valueArrayProperty, int startIndex = 0)
            {
                if (keyArrayProperty.arraySize > startIndex)
                {
                    int index = startIndex;
                    SerializedProperty keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
                    SerializedProperty valueProperty = valueArrayProperty.GetArrayElementAtIndex(startIndex);
                    SerializedProperty endKeyProperty = keyArrayProperty.GetEndProperty();
                    do
                    {
                        yield return new DictionaryItem(keyProperty, valueProperty, index);
                        index++;
                    } while (keyProperty.Next(false) && valueProperty.Next(false)
                            && !SerializedProperty.EqualContents(keyProperty, endKeyProperty));
                }
            }

            //get dictionary property height
            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                float height = base.GetPropertyHeight(property, label);
                //If expanded, calculate the height of all items
                if (property.isExpanded)
                {
                    SerializedProperty keyArrayProperty = property.FindPropertyRelative(keyName);
                    SerializedProperty valueArrayProperty = property.FindPropertyRelative(valueName);

                    foreach (var item in GetDictionaryItemsEnumerable(keyArrayProperty, valueArrayProperty))
                    {
                        var keyProperty = item.keyProperty;
                        var valueProperty = item.valueProperty;
                        float currentLineHeight = Mathf.Max(EditorGUI.GetPropertyHeight(keyProperty), EditorGUI.GetPropertyHeight(valueProperty))
                            + EditorGUIUtility.standardVerticalSpacing;
                        height += currentLineHeight;
                    }
                }

                return height;
            }

            struct DictionaryItem
            {
                public SerializedProperty keyProperty;      //SerializedProperty of the key
                public SerializedProperty valueProperty;    //SerializedProperty of the value
                public int index;   //index of the list
                public DictionaryItem(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
                {
                    this.keyProperty = keyProperty;
                    this.valueProperty = valueProperty;
                    this.index = index;
                }
            }

            enum ItemAction
            {
                None,
                Add,    //add action
                Remove  //remove action
            }
        }
        #endregion

    }
}
