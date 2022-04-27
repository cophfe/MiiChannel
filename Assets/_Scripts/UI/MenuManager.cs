using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
	[SerializeField] Animator rightClickMenu;
	[SerializeField] GameObject characterMenu;
	[SerializeField] GameObject nothingMenu;
	[SerializeField] RectTransform blockPanel;
	public bool Interacting { get; private set; } = false;
	// Start is called before the first frame update
	void Start()
    {
		blockPanel.gameObject.SetActive(false);
		rightClickMenu.gameObject.SetActive(false);
	}

    public void OpenMenu()
	{
		GameObject selected = GameManager.Instance.UserController.GetSelected();
		SetupRClickMenu(selected);

		blockPanel.gameObject.SetActive(true);
		rightClickMenu.gameObject.SetActive(true);
		rightClickMenu.SetBool("Open", true);
		Interacting = true;
		Vector2 mousePos = Mouse.current.position.ReadValue();
		rightClickMenu.GetComponent<RectTransform>().anchoredPosition = mousePos;
	}

	public void OnPanelPressed()
	{
		blockPanel.gameObject.SetActive(false);
		rightClickMenu.SetBool("Open", false);
		Interacting = false;

	}

	void SetupRClickMenu(GameObject selectedCharacter)
	{
		bool hasCharacter = selectedCharacter != null;
		characterMenu.SetActive(hasCharacter);
		nothingMenu.SetActive(!hasCharacter);
	}
}
