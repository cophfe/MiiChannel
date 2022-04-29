using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	[SerializeField] Animator rightClickMenu;
	[SerializeField] Animator fadePanel;
	[SerializeField] GameObject characterMenu;
	[SerializeField] GameObject nothingMenu;
	[SerializeField] RectTransform blockPanel;
	public bool Interacting { get; private set; } = false;
	// Start is called before the first frame update
	void Start()
    {
		fadePanel.Play("Base Layer.Open");
		fadePanel.SetBool("Open", false);
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

	public void New()
	{
		var savedCharacters = GameManager.Instance.Saved;

		Vector2 mousePos = Mouse.current.position.ReadValue();
		Plane ground = new Plane(Vector3.up, 0);
		Ray camRay = GameManager.Instance.MainCamera.ScreenPointToRay(mousePos);
		ground.Raycast(camRay, out float dist);
		Vector3 point = camRay.GetPoint(dist);
		Vector2 position = new Vector2(Mathf.Clamp(point.x, GameManager.Instance.BoundsMin.x, GameManager.Instance.BoundsMax.x), 
			Mathf.Clamp(point.z, GameManager.Instance.BoundsMin.y, GameManager.Instance.BoundsMax.y));
		savedCharacters.datas.Add(new CharacterData(position));
		GameManager.CurrentlyEditingIndex = savedCharacters.datas.Count - 1;
		GameManager.CreatingNew = true;

		StartCoroutine(StartEdit());
	}

	public void Delete()
	{
		var sel = GameManager.Instance.UserController.GetSelected();
		if (!sel) return;
		var ai = sel.GetComponent<CharacterAI>();
		if (ai)
		{
			ai.Kill(); 
			var vanity = sel.GetComponent<CharacterVanity>();
			if (vanity)
				GameManager.Instance.Saved.datas.RemoveAt(vanity.GetIndex());

		}
	}

	public void Edit()
	{
		var sel = GameManager.Instance.UserController.GetSelected();
		if (!sel) return;
		var vanity = sel.GetComponent<CharacterVanity>();
		if (vanity)
		{
			int index = vanity.GetIndex();
			var savedCharacters = GameManager.Instance.Saved;
			if (index < savedCharacters.datas.Count)
			{
				GameManager.CurrentlyEditingIndex = index;
				GameManager.CreatingNew = false;
				StartCoroutine(StartEdit());
			}
		}
	}

	IEnumerator StartEdit()
	{
		fadePanel.SetBool("Open", true);
		yield return new WaitForSeconds(0.3f);

		GameManager.Instance.SaveCurrentPositions();
		SceneManager.LoadScene(1);
	}

	public void Exit()
	{
		StartCoroutine(StartExit());
	}

	IEnumerator StartExit()
	{
		fadePanel.SetBool("Open", true);
		yield return new WaitForSeconds(0.3f);

		GameManager.Instance.SaveCurrentPositions();
		Application.Quit();
	}
}
