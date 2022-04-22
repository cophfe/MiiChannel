using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{
	private void Start()
	{
		if (GameManager.Instance && GameManager.Instance.Selector)
			GameManager.Instance.Selector.RegisterSelectable(gameObject);
	}
}
