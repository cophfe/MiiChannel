using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SelectorUI : MonoBehaviour
{
	[SerializeField] Button back;
	[SerializeField] Button forward;
	[SerializeField] TextMeshProUGUI indexText;
	int count = 0;
	int index = 0;
	Action<int> onChangeIndexCallback;

	private void Awake()
	{
		back.onClick.AddListener(Deiterate);
		forward.onClick.AddListener(Iterate);
	}

	public void SetIndex(int index)
	{
		this.index = Mathf.Clamp(index, 0, count - 1);
		indexText.text = index.ToString();
		onChangeIndexCallback?.Invoke(index);
	}

	public int GetIndex()
	{
		return index;
	}

	//this way it can be used to index into any type of array
	public void SetCount(int count)
	{
		this.count = count;
	}

	public int GetCount()
	{
		return count;
	}

	public void SetCallback(Action<int> onChangeIndex)
	{
		this.onChangeIndexCallback = onChangeIndex;
	}

	void Iterate()
	{
		if (count == 0)
		{
			Debug.LogWarning("Nothing connected to selector UI");
			return;
		}

		index = (index + 1) % count;
		indexText.text = index.ToString();
		onChangeIndexCallback?.Invoke(index);
	}

	void Deiterate()
	{
		if (count == 0)
		{
			Debug.LogWarning("Nothing connected to selector UI");
			return;
		}

		index = index > 0 ? index - 1 : count - 1;
		indexText.text = index.ToString();
		onChangeIndexCallback?.Invoke(index);
	}
}
