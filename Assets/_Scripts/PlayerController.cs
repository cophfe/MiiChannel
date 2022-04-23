using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	[SerializeField] string outlineLayerName = "Outlined";
	[SerializeField] Material outlineMaterial;
	[SerializeField] Color selectedColour;
	[SerializeField] Color hovorColour;
	Selector selector;
	GameObject selected = null;
	int lastLayer;
	int outlineLayer;

	//if no gameobject has been clicked on yet
	bool selecting = true;

	private void Start()
	{
		outlineLayer = LayerMask.NameToLayer(outlineLayerName);
		selector = GameManager.Instance.Selector;
		outlineMaterial.SetColor("_OutlineColour", hovorColour);
	}

	private void Update()
	{
		if (selecting && !GameManager.Instance.UI.Interacting)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelectedObject);
		}
	}

	public void OnClick(InputAction.CallbackContext ctx)
	{
		if (ctx.performed && !GameManager.Instance.UI.Interacting)
		{
			selecting = false;
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelectedObject);
		}
	}

	void OnSelectedObject(GameObject gameObject, Vector3 position)
	{
		if (selected != null)
		{
			RecursiveSetLayer(selected, lastLayer);
		}
		selected = gameObject;

		if (gameObject)
		{
			Debug.Log(gameObject.name + " selected.");

			lastLayer = selected.layer;
			RecursiveSetLayer(selected, outlineLayer);

			if (!selecting)
			{
				outlineMaterial.SetColor("_OutlineColour", selectedColour);
			}
		}
		else if (!selecting)
		{
			outlineMaterial.SetColor("_OutlineColour", hovorColour);
			selecting = true;
		}
		
		
		//Debug.DrawLine(position, Camera.main.transform.position - new Vector3(0, 0.05f, 0));
	}

	void RecursiveSetLayer(GameObject gO, int layer)
	{
		if (gO == null)
			return;

		gO.layer = layer;
		foreach (Transform child in gO.transform)
		{
			RecursiveSetLayer(child.gameObject, layer);
		}
	}
}
