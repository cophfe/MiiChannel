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

	//this way it can be used to index into any type of array
	public void ConnectToSelector(int count, Action<int> onChangeIndex)
	{
		this.count = count;
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
