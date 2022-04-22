using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] Selector selector;

	public void OnClick(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelectedObject);
		}
	}

	void OnSelectedObject(GameObject gameObject, Vector3 position)
	{

	}
}
