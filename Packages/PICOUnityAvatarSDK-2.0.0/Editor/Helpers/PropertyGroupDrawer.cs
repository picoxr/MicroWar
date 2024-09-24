using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
public class DrawEditorFoldGroup
{
	List<DrawPropertyField> items;
	bool isClose = true;
	string groupName;
	public string GroupName { get { return groupName; } }
	public bool IsClose { get { return isClose; } set { isClose = value; } }
	public List<DrawPropertyField> Items { get { return items; } }

	public DrawEditorFoldGroup()
    {
		this.items = new List<DrawPropertyField>();
		this.groupName = "";
		this.isClose = true;
	}
	public void AddGroupItem(DrawPropertyField item)
    {
		if (string.IsNullOrEmpty(this.groupName))
			groupName = (item.AttrbuteInfo.GroupName);
		this.items.Add(item);
	}
}
public class DrawPropertyField
{
	object obj;
	FieldInfo info;
	SerializedPropertyType type;
	FoldGroup attrbuteInfo;

	public FieldInfo Info
	{
		get { return info; }
	}

	public SerializedPropertyType Type
	{
		get { return type; }
	}

	public String Name
	{
		get { return ObjectNames.NicifyVariableName(info.Name); }
	}
	public FoldGroup AttrbuteInfo
	{
		get { return attrbuteInfo; }
	}

	public DrawPropertyField(object obj, FieldInfo info, SerializedPropertyType type, FoldGroup attrbuteInfo)
	{
		this.obj = obj;
		this.info = info;
		this.type = type;
		this.attrbuteInfo = attrbuteInfo;
	}

	public object GetValue()
	{
        try
        {
			var data = info.GetValue(this.obj);
			return data;
		}
		catch(Exception ex)
        {
			Debug.LogError(ex.Message);
			return null;
        }
	
	}
	public void SetValue(object value) {info.SetValue(this.obj,value); }

	public static bool IsStruct(FieldInfo info)
    {
		Type type = info.FieldType;
		 if (type.IsValueType && !type.IsEnum && !type.IsPrimitive)
		{
			return true;
		}
		return false;
	}
	public static bool GetPropertyType(FieldInfo info, out SerializedPropertyType propertyType)
	{
		Type type = info.FieldType;
		propertyType = SerializedPropertyType.Generic;
		if (type == typeof(int))
			propertyType = SerializedPropertyType.Integer;
		else if (type == typeof(float))
			propertyType = SerializedPropertyType.Float;
		else if (type == typeof(bool))
			propertyType = SerializedPropertyType.Boolean;
		else if (type == typeof(string))
			propertyType = SerializedPropertyType.String;
		else if (type == typeof(Vector2))
			propertyType = SerializedPropertyType.Vector2;
		else if (type == typeof(Vector3))
			propertyType = SerializedPropertyType.Vector3;
		else if (type.IsEnum)
			propertyType = SerializedPropertyType.Enum;
		else if (type == typeof(UnityEngine.Camera))
			propertyType = SerializedPropertyType.ObjectReference;
		else if (typeof(MonoBehaviour).IsAssignableFrom(type))
			propertyType = SerializedPropertyType.ObjectReference;
		return propertyType != SerializedPropertyType.Generic;
	}
}

public class PropertyGroupDrawer
{
	public static List<DrawEditorFoldGroup> GetDrawGroupData(List<DrawPropertyField> data)
    {
		var results = new List<DrawEditorFoldGroup>();
		Dictionary<string, DrawEditorFoldGroup> mapData = new Dictionary<string, DrawEditorFoldGroup>();
		foreach(var item in data)
        {
			string groupName = item.AttrbuteInfo.GroupName;
			DrawEditorFoldGroup value = null;
			if (mapData.ContainsKey(groupName))
            {
				value = mapData[groupName];
			}
			if (value == null)
            {
				value = new DrawEditorFoldGroup();
				mapData[groupName] = value;
			}

			value.AddGroupItem(item);
		}
		foreach(var item in mapData)
        {
			results.Add(item.Value);
		}
		return results;
	}
	public static List<DrawPropertyField> GetProperties(object obj)
	{
		var fields = new List<DrawPropertyField>();
		Type type = obj.GetType();

		
	  // 获取类型的指定特性
		MemberInfo[] member = type.GetMembers();
		foreach(var item in member)
        {
			Attribute[] classAttr = Attribute.GetCustomAttributes(item, typeof(FoldGroup));
			foreach (var aa in classAttr)
            {
				var propType = SerializedPropertyType.Integer;
				var fieldInfo = type.GetField(item.Name);
				if(DrawPropertyField.IsStruct(fieldInfo))
                {
					var structType = fieldInfo.FieldType;
					var structValue = fieldInfo.GetValue(obj);
					FieldInfo[] fieldInfos = structType.GetFields();
					foreach(var structItem in fieldInfos)
                    {
						if (DrawPropertyField.GetPropertyType(structItem, out propType))
						{
							var itemField = new DrawPropertyField(structValue, structItem, propType, aa as FoldGroup);
							fields.Add(itemField);
						}
					}
				}
				else
                {
					if (DrawPropertyField.GetPropertyType(fieldInfo, out propType))
					{
						var itemField = new DrawPropertyField(obj, fieldInfo, propType, aa as FoldGroup);
						fields.Add(itemField);
					}
				}
            }
		}

		return fields;
	}
	public static bool ShowPropertyGroup(List<DrawEditorFoldGroup> properties,UnityEngine.Object target)
	{
		bool isDirty = false;
		foreach (DrawEditorFoldGroup field in properties)
		{
			EditorGUI.indentLevel = 0;
			field.IsClose = EditorGUILayout.Foldout(field.IsClose, field.GroupName);
			if(!field.IsClose)
            {
				EditorGUI.indentLevel = 1;
				var result = ShowPropertyGroupItem(field.Items, target);
				if (!isDirty && result)
					isDirty = true;
			}
		}
		EditorGUI.indentLevel = 0;
		return isDirty;
	}
		public static bool ShowPropertyGroupItem(List<DrawPropertyField> properties,UnityEngine.Object target)
	{
		bool isDirty = false;
		if (properties == null)
			return isDirty;
		var emptyOptions = new GUILayoutOption[0];
		EditorGUILayout.BeginVertical(emptyOptions);
		foreach (DrawPropertyField field in properties)
		{
			EditorGUILayout.BeginHorizontal(emptyOptions);
			EditorGUI.BeginChangeCheck();
			if (field.Type == SerializedPropertyType.Integer)
			{
				var oldValue = (int)field.GetValue();
				int newValue = oldValue;
				if (field.AttrbuteInfo.RangType)
				{
					newValue = EditorGUILayout.IntSlider(field.Name, oldValue, field.AttrbuteInfo.RangIntMin, field.AttrbuteInfo.RangIntMax, emptyOptions);
				}
				else
				    newValue = EditorGUILayout.IntField(field.Name, oldValue, emptyOptions);

				if (oldValue != newValue)
                {
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.Float)
			{
				var oldValue = (float)field.GetValue();
				float newValue = oldValue;
				if (field.AttrbuteInfo.RangType)
                {
					newValue = EditorGUILayout.Slider(field.Name,oldValue, field.AttrbuteInfo.RangFloatMin, field.AttrbuteInfo.RangFloatMax, emptyOptions);
				}
				else
					newValue = EditorGUILayout.FloatField(field.Name, oldValue, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.Boolean)
			{
				var oldValue = (bool)field.GetValue();
				var newValue = EditorGUILayout.Toggle(field.Name, oldValue, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.String)
			{
				var oldValue = (string)field.GetValue();
				var newValue = EditorGUILayout.TextField(field.Name, oldValue, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.Vector2)
			{
				var oldValue = (Vector2)field.GetValue();
				var newValue = EditorGUILayout.Vector2Field(field.Name, oldValue, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.Vector3)
			{
				var oldValue = (Vector3)field.GetValue();
				var newValue = EditorGUILayout.Vector3Field(field.Name, oldValue, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.Enum)
			{
				var oldValue = (Enum)field.GetValue();
				var newValue = EditorGUILayout.EnumPopup(field.Name, oldValue, emptyOptions);
				if (!oldValue.Equals(newValue))
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			else if (field.Type == SerializedPropertyType.ObjectReference)
			{
				UnityEngine.Object oldValue = (UnityEngine.Object)field.GetValue();
				UnityEngine.Object newValue = EditorGUILayout.ObjectField(field.Name, oldValue, field.Info.FieldType, emptyOptions);
				if (oldValue != newValue)
				{
					field.SetValue(newValue);
					isDirty = true;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "PicoAvatarApp" + field.Name);
				UnityEditor.EditorUtility.SetDirty(target);
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		return isDirty;
		}
}


