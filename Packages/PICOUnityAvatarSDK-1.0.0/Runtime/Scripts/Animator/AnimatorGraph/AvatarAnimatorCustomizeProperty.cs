using System.Collections.Generic;
using System;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		#region StringDropDownStatic Attribute. Need to provide static array.

		public class StringDropDownStatic : PropertyAttribute
		{
			public delegate string[] GetStringList();

			public StringDropDownStatic(params string[] list)
			{
				List = list;
			}

			public StringDropDownStatic(Type type, string methodName)
			{
				var method = type.GetMethod(methodName);
				if (method != null)
				{
					List = method.Invoke(null, null) as string[];
				}
				else
				{
					Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
				}
			}

			public string[] List { get; private set; }
		}

		#endregion


		#region StringDropDown custumize class. Need to provide array or node.

		[Serializable]
		public class StringDropDown
		{
			public string[] stringArray; //float parameters array
			public string currentString; //current selected parameter
			public string name; //display name in window
			public Pico.Avatar.XNode.Node node;

			public StringDropDown(string name = "Parameter")
			{
				stringArray = null;
				this.name = name;
			}

			public StringDropDown(string[] array)
			{
				stringArray = array;
				this.name = "Parameter";
			}

			public StringDropDown(string[] array, string name)
			{
				stringArray = array;
				this.name = name;
			}

			public StringDropDown(Pico.Avatar.XNode.Node node)
			{
				this.node = node;
				this.name = "Parameter";
				try
				{
					stringArray = node.layer.parentAnimator.FloatParametersArray;
				}
				catch (Exception e)
				{
				}
			}

			/// <param name="name">display name</param>
			public StringDropDown(Pico.Avatar.XNode.Node node, string name) : this(node)
			{
				this.name = name;
			}
		}

		#endregion

		#region EnumMultiAttribute. This Attribute allows Enum multiple selection.

		[AttributeUsage(AttributeTargets.Field)]
		public class EnumMultiAttribute : PropertyAttribute
		{
		}

		#endregion

		#region enable array edit when playing

		[AttributeUsage(AttributeTargets.Field)]
		public class EnableArrayEdit : PropertyAttribute
		{
		}

		[AttributeUsage(AttributeTargets.Field)]
		public class DisableEditRuntime : PropertyAttribute
		{
		}

		#endregion

		#region ShowCompare.

		[AttributeUsage(AttributeTargets.Field)]
		public class ShowCompare : PropertyAttribute
		{
			AvatarConditionType required;
			AvatarConditionType curr;

			public ShowCompare(AvatarConditionType required, AvatarConditionType curr)
			{
				this.required = required;
				this.curr = curr;
			}

			public bool isMatch()
			{
				return required == curr;
			}
		}

		#endregion


		#region DictionarySerializable.

		[Serializable]
		public class StringUintDictionary : DictionarySerializable<string, uint>
		{
		}

		[Serializable]
		public class StringFloatDictionary : DictionarySerializable<string, float>
		{
		}

		[Serializable]
		public class StringBoolDictionary : DictionarySerializable<string, bool>
		{
		}

		public class DictionarySerializable<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
		{
			[Delayed] [SerializeField] TKey[] _keys; //key of dictionary
			[SerializeField] TValue[] _values; //values of dictionary

			//after deserialize callback, Convert array to dictionary
			public void OnAfterDeserialize()
			{
				if (_keys != null && _values != null && _keys.Length == _values.Length)
				{
					this.Clear();
					for (int i = 0; i < _keys.Length; i++)
					{
						this[_keys[i]] = _values[i];
					}

					_keys = null;
					_values = null;
				}
			}

			//before serialize callback, Convert dictionary to array
			public void OnBeforeSerialize()
			{
				_keys = new TKey[this.Count];
				_values = new TValue[this.Count];
				int i = 0;
				foreach (var item in this)
				{
					_keys[i] = item.Key;
					_values[i] = item.Value;
					i++;
				}
			}
		}

		#endregion
	}
}