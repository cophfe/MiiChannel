using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; protected set; } = null;

	protected void OnEnable()
	{
		if (Instance != null && Instance != this)
		{
			Debug.LogError($"ERROR: More than one instance of GameManager in the scene.");
			Destroy(this);
		}
		else
		{
			Instance = this;
		}

		MainCamera = Camera.main;
	}

	public Camera MainCamera { get; private set; }
	public Selector Selector { get; private set; }

	public void RegisterSelector(Selector s)
	{
		Selector = s;
	}
}
