using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//is a scriptable object so it is saved between scenes and is editible and reviewable in the inspector
[CreateAssetMenu(fileName = "Saved Characters", menuName = "ScriptableObjects/Saved Characters", order = 1)]
public class SavedCharacters : ScriptableObject
{
	public List<CharacterData> datas;

	string location;
	private void OnEnable()
	{
		location = Application.persistentDataPath + "\\characterData.txt";
	}
	
	public void SaveToJson()
	{
		using StreamWriter sw = File.CreateText(location);
		foreach (CharacterData data in datas)
		{
			string textData = JsonUtility.ToJson(data);
			sw.WriteLine(textData);
		}
		sw.Close();
	}

	public void LoadFromJson()
	{
		if (File.Exists(location))
		{
			string[] lines = File.ReadAllLines(location);
			datas.Clear();
			datas.Capacity = lines.Length;
			foreach (var line in lines)
			{
				CharacterData data = JsonUtility.FromJson<CharacterData>(line);
				datas.Add(data);
			}
		}
		else
		{
			datas.Clear();
		}
	}
}
