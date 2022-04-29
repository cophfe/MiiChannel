using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
	[SerializeField] SavedCharacters characters;
	[SerializeField] CharacterVanity characterPrefab;
	[SerializeField] Selector selector;
	[SerializeField] PlayerController userController;
	[SerializeField] MenuManager menuManager;
	[SerializeField] Animator gettingUpFacingUp;
	[SerializeField] Animator gettingUpFacingDown;
	[SerializeField] Vector2 boundsSize = Vector2.one;
	[SerializeField] Vector2 boundsOffset;
	public Vector2 BoundsMin => boundsOffset - 0.5f * boundsSize;
	public Vector2 BoundsMax => boundsOffset + 0.5f * boundsSize;
	public Camera MainCamera { get; private set; }
	public Selector Selector => selector;
	public SavedCharacters Saved => characters;
	public MenuManager UI => menuManager;
	public PlayerController UserController => userController;

	//the positions are set to the correct positions after the first update and are used for animation transitions by the character ais
	public Transform StandUpFacingUpSpine { get; private set; }
	public Transform StandUpFacingDownSpine { get; private set; }

	public static GameManager Instance { get; protected set; } = null;
	//please ignore these im lazy
	public static int CurrentlyEditingIndex { get; set; } = 0;
	public static bool CreatingNew { get; set; } = false;

	CharacterVanity[] characterVanities;
	protected void OnEnable()
	{
		if (Instance != null && Instance != this)
		{
			Debug.LogError($"ERROR: More than one instance of GameManager in the scene.");
			Destroy(this);
		}
		else if (Instance != this)
		{
			Instance = this;
			Init();
		}
	}

	private void Init()
	{
		MainCamera = Camera.main;

		StartCoroutine(InitAnimators());

		//create characters from saved characters
		Saved.LoadFromJson();
		characterVanities = new CharacterVanity[characters.datas.Count];
		for (int i = 0; i < characters.datas.Count; i++)
		{
			var character = Instantiate(characterPrefab.gameObject, null, true);
			character.transform.position = new Vector3(characters.datas[i].position.x, 0, characters.datas[i].position.y);
			characterVanities[i] = character.GetComponent<CharacterVanity>();
			characterVanities[i].SetIndex(i);
		}
		
	}

	IEnumerator InitAnimators()
	{
		//get rigs (obviously requires everything to be setup a certain way)
		StandUpFacingUpSpine = gettingUpFacingUp.transform.GetChild(0).GetChild(0);
		StandUpFacingDownSpine = gettingUpFacingDown.transform.GetChild(0).GetChild(0);
		//setup animators
		gettingUpFacingUp.Play("Base Layer.Stand Facing Up", 0, 0);
		gettingUpFacingDown.Play("Base Layer.Stand Facing Down", 0, 0);

		//wait for animators to put everything into intial position
		yield return new WaitForEndOfFrame();

		//deactivate animators
		gettingUpFacingUp.gameObject.SetActive(false);
		gettingUpFacingDown.gameObject.SetActive(false);

		yield return null;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		//bounds are in world space
		Gizmos.DrawWireCube(new Vector3(boundsOffset.x, 5, boundsOffset.y), new Vector3(boundsSize.x, 10, boundsSize.y));
	}

	private void OnApplicationQuit()
	{
		if (Instance == this)
		{
			SaveCurrentPositions();
		}
	}

	public void SaveCurrentPositions()
	{
		foreach (var vanity in characterVanities)
		{
			if (vanity != null && vanity.GetIndex() < Saved.datas.Count)
			{
				var cData = Saved.datas[vanity.GetIndex()];
				cData.position = new Vector2(vanity.transform.position.x, vanity.transform.position.z);
				Saved.datas[vanity.GetIndex()] = cData;
			}
		}
		Saved.SaveToJson();
	}
}
