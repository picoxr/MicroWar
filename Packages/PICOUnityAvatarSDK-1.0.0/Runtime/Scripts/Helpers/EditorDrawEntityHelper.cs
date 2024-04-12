using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EditorDrawEntityComponent : MonoBehaviour
{
	public List<string> componentsNames = new List<string>();
}

public class EditorDrawEntity
{
	public string transfromName;
	public string componentsString;

	public List<EditorDrawEntity> childList = new List<EditorDrawEntity>();
	private static GameObject Root = null;

	public static EditorDrawEntity BuildByJson(string json)
	{
		Debug.Log("BuildByJson:" + json);
		var result = JsonConvert.DeserializeObject<JObject>(json);

		var root = new EditorDrawEntity();
		BuildEntityChild(root, result);
		root.CreateGameObject();
		return null;
	}

	static void BuildEntityChild(EditorDrawEntity parent, JObject data)
	{
		EditorDrawEntity logic = parent == null ? new EditorDrawEntity() : parent;
		logic.transfromName = data.Value<string>("transfromName");
		logic.componentsString = data.Value<string>("componentsString");
		var child = data.Value<JArray>("childList");
		if (child != null)
			foreach (var item in child)
			{
				EditorDrawEntity childItem = new EditorDrawEntity();
				logic.childList.Add(childItem);
				BuildEntityChild(childItem, (JObject)item);
			}
	}

	public List<string> GetComponents()
	{
		var result = new List<string>();
		if (string.IsNullOrEmpty(this.componentsString))
			return result;
		var data = this.componentsString.Split(',');
		foreach (var item in data)
		{
			if (!string.IsNullOrEmpty(item))
			{
				string name = item.Replace("[object", "").Replace("]", "");
				result.Add(name.Trim());
			}
		}

		return result;
	}

	public void CreateGameObject()
	{
		if (EditorDrawEntity.Root == null)
			EditorDrawEntity.Root = new GameObject("C++ View");
		CreateObj(this, EditorDrawEntity.Root.transform);
	}

	void CreateObj(EditorDrawEntity temp, Transform parent)
	{
		GameObject tempChild = new GameObject(temp.transfromName);
		var showComp = tempChild.AddComponent<EditorDrawEntityComponent>();
		showComp.componentsNames = GetComponents();
		if (parent)
		{
			tempChild.transform.parent = parent;
		}

		foreach (var item in temp.childList)
		{
			CreateObj(item, tempChild.transform);
		}
	}
}