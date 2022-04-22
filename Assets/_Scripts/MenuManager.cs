using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
	[SerializeField] Animator rightClickMenu;
	[SerializeField] RectTransform blockPanel;

	// Start is called before the first frame update
	void Start()
    {
		blockPanel.gameObject.SetActive(false);
		rightClickMenu.gameObject.SetActive(false);
	}

    public void OnRightClick(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			blockPanel.gameObject.SetActive(true);
			rightClickMenu.gameObject.SetActive(true);
			rightClickMenu.SetBool("Open", true);

			Vector2 mousePos = Mouse.current.position.ReadValue();
			rightClickMenu.GetComponent<RectTransform>().anchoredPosition = mousePos;
		}
	}

	public void OnPanelPressed()
	{
		blockPanel.gameObject.SetActive(false);
		rightClickMenu.SetBool("Open", false);

	}
}
