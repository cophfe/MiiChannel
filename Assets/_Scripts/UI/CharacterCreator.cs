using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class CharacterCreator : MonoBehaviour
{
	[System.Serializable]
	public struct TypedFeature
	{
		public Toggle on;
		public SelectorUI type;
		public SelectorUI material;
		public GetPicker colour;
	}

	[System.Serializable]
	public struct NonTypedFeature
	{
		public Toggle on;
		public SelectorUI material;
		public GetPicker colour;
	}

	[SerializeField] CharacterVanity character; // get and set data with the vanity
	[SerializeField] SavedCharacters savedCharacters;
	[SerializeField] CharacterMaterials materials;
	[SerializeField] Animator fadeOuter;

	[Header("Head")]
	[SerializeField] TypedFeature hat;
	[SerializeField] TypedFeature hair;
	[SerializeField] TypedFeature facialHair;
	[SerializeField] Toggle seperateFacialHairColour;

	[Header("Body")]
	[SerializeField] RandomName nameRandomiser;
	[SerializeField] TMP_InputField characterName;
	[SerializeField] Slider height;
	[SerializeField] NonTypedFeature skin;
	[SerializeField] NonTypedFeature shirt;
	[SerializeField] NonTypedFeature belt;
	[SerializeField] NonTypedFeature pants;
	[SerializeField] NonTypedFeature shoes;

	CharacterData initialData;
	bool callbacksDisabled = false;
	private void Awake()
	{
		savedCharacters.LoadFromJson();
		character.SetIndex(GameManager.CurrentlyEditingIndex);
	}
	private void Start()
	{
		fadeOuter.Play("Base Layer.Open");
		fadeOuter.SetBool("Open", false);

		//set UI based on cdata
		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];
		initialData = cData;

		SetupUI();
	}

	void SetupUI()
	{
		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];

		characterName.text = cData.name;
		height.value = cData.height;
		//head
		SetupUIFromTypedFeature(hair, cData.hairData, cData.hairIndex, materials.hairMaterials.Count, character.GetHairCount());
		SetupUIFromTypedFeature(facialHair, cData.facialHairData, cData.facialHairIndex, materials.hairMaterials.Count, character.GetFacialHairCount());
		SetupUIFromTypedFeature(hat, cData.hatData, cData.hatIndex, materials.hatMaterials.Count, character.GetHatCount());
		//clothes
		SetupUIFromFeature(belt, cData.beltData, materials.clothesMaterials.Count);
		SetupUIFromFeature(shoes, cData.shoesData, materials.clothesMaterials.Count);
		SetupUIFromFeature(shirt, cData.shirtData, materials.clothesMaterials.Count);
		SetupUIFromFeature(pants, cData.pantsData, materials.clothesMaterials.Count);
		SetupUIFromFeature(skin, cData.skinData, materials.skinMaterials.Count);

		//now set up callbacks
		characterName.onValueChanged.AddListener(OnChangeName);
		height.onValueChanged.AddListener(h => OnChangeBody(false));

		Action<Color> colourChange = c => OnChangeHair(false);
		Action<int> materialChange = i => OnChangeHair(true);

		hair.colour.SetCallback(colourChange);
		hair.material.SetCallback(materialChange);
		hair.type.SetCallback(materialChange);
		facialHair.colour.SetCallback(colourChange);
		facialHair.material.SetCallback(materialChange);
		facialHair.type.SetCallback(materialChange);
		seperateFacialHairColour.onValueChanged.AddListener(b => OnChangeHair(false));

		materialChange = i => OnChangeHat(true);

		hat.colour.SetCallback(c => OnChangeHat(false));
		hat.material.SetCallback(materialChange);
		hat.type.SetCallback(materialChange);

		colourChange = c => OnChangeBody(false);
		materialChange = i => OnChangeBody(true);

		belt.colour.SetCallback(colourChange);
		belt.material.SetCallback(materialChange);
		shoes.colour.SetCallback(colourChange);
		shoes.material.SetCallback(materialChange);
		shirt.colour.SetCallback(colourChange);
		shirt.material.SetCallback(materialChange);
		pants.colour.SetCallback(colourChange);
		pants.material.SetCallback(materialChange);
		skin.colour.SetCallback(colourChange);
		skin.material.SetCallback(materialChange);

		hair.on.onValueChanged.AddListener(b => OnChangeHair(true));
		facialHair.on.onValueChanged.AddListener(b => OnChangeHair(true));
		hat.on.onValueChanged.AddListener(b => OnChangeHat(true));
		belt.on.onValueChanged.AddListener(b => OnChangeBody(true));
		shoes.on.onValueChanged.AddListener(b => OnChangeBody(true));
		shirt.on.onValueChanged.AddListener(b => OnChangeBody(true));
		pants.on.onValueChanged.AddListener(b => OnChangeBody(true));
		
		void SetupUIFromTypedFeature(TypedFeature feature, MaterialModifierData matData, int typeIndex, int materialCount, int typeCount)
		{
			feature.type.SetCount(typeCount);
			feature.type.SetIndex(typeIndex);
			
			feature.material.SetCount(materialCount);
			feature.material.SetIndex(matData.materialIndex);
			
			feature.colour.SetColour(matData.color);

			feature.on.isOn = matData.on;

		}
		void SetupUIFromFeature(NonTypedFeature feature, MaterialModifierData matData, int materialCount)
		{
			feature.material.SetCount(materialCount);
			feature.material.SetIndex(matData.materialIndex);

			feature.colour.SetColour(matData.color);

			if (feature.on != null)
				feature.on.isOn = matData.on;
		}
	}

	void OnChangeName(string name)
	{
		if (callbacksDisabled)
			return;

		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];
		cData.name = name;
		savedCharacters.datas[index] = cData;
		character.LoadNameVanity();
	}

	void OnChangeBody(bool changeMaterial)
	{
		if (callbacksDisabled)
			return;

		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];

		cData.height = height.value;
		SetFeatureData(belt, ref cData.beltData);
		SetFeatureData(shoes, ref cData.shoesData);
		SetFeatureData(shirt, ref cData.shirtData);
		SetFeatureData(pants, ref cData.pantsData);
		SetFeatureData(skin, ref cData.skinData);

		savedCharacters.datas[index] = cData;
		character.LoadBodyVanity(changeMaterial);
	}

	void OnChangeHair(bool changeMaterial)
	{
		if (callbacksDisabled)
			return;

		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];

		if (!seperateFacialHairColour.isOn)
			facialHair.colour.SetColour(hair.colour.GetColour(), false);

		SetTypedFeatureData(hair, ref cData.hairData, ref cData.hairIndex);
		SetTypedFeatureData(facialHair, ref cData.facialHairData, ref cData.facialHairIndex);
		
		savedCharacters.datas[index] = cData;
		character.LoadHairVanity(changeMaterial);
	}

	void OnChangeHat(bool changeMaterial)
	{
		if (callbacksDisabled)
			return;

		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}
		CharacterData cData = savedCharacters.datas[index];
		SetTypedFeatureData(hat, ref cData.hatData, ref cData.hatIndex);
		savedCharacters.datas[index] = cData;
		character.LoadHatVanity(changeMaterial);
	}

	void SetTypedFeatureData(TypedFeature feature, ref MaterialModifierData matData, ref int index)
	{
		index = feature.type.GetIndex();
		matData.materialIndex = feature.material.GetIndex();
		matData.color = feature.colour.GetColour();
		matData.on = feature.on.isOn;
	}
	void SetFeatureData(NonTypedFeature feature, ref MaterialModifierData matData)
	{
		matData.materialIndex = feature.material.GetIndex();
		matData.color = feature.colour.GetColour();
		if (feature.on != null)
			matData.on = feature.on.isOn;
	}

	public void Finish()
	{
		StartCoroutine(OnFinish());
	}

	public void Cancel()
	{
		StartCoroutine(OnCancel());
	}

	public void Randomize()
	{
		callbacksDisabled = true;

		RandomizeFeature(belt);
		RandomizeFeature(shoes);
		RandomizeFeature(pants);
		RandomizeFeature(shirt);
		RandomizeFeature(skin);

		RandomizeTypedFeature(hat);
		RandomizeTypedFeature(hair);
		RandomizeTypedFeature(facialHair);
		height.value = UnityEngine.Random.Range(height.minValue, height.maxValue);
		nameRandomiser.Generate();

		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
		{
			Debug.LogWarning("Could not apply changes to character.");
			return;
		}

		callbacksDisabled = false;
		OnChangeName(characterName.text);
		OnChangeBody(true);
		OnChangeHair(true);
		OnChangeHat(true);
		
		void RandomizeTypedFeature(TypedFeature feature)
		{
			//more likely to be on than off
			feature.on.isOn = UnityEngine.Random.value > 0.4f;
			feature.type.SetIndex(UnityEngine.Random.Range(0, feature.type.GetCount()));
			feature.colour.SetColour(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
			feature.material.SetIndex(UnityEngine.Random.Range(0, feature.material.GetCount()));
		}
		void RandomizeFeature(NonTypedFeature feature)
		{
			if (feature.on != null)
				feature.on.isOn = UnityEngine.Random.value > 0.4f;
			feature.colour.SetColour(new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value));
			feature.material.SetIndex(UnityEngine.Random.Range(0, feature.material.GetCount()));
		}
	}
	IEnumerator OnCancel()
	{
		fadeOuter.SetBool("Open", true);
		yield return new WaitForSeconds(0.3f);
		
		int index = character.GetIndex();
		if (index < 0 || index > savedCharacters.datas.Count)
			yield return null;

		if (GameManager.CreatingNew)
		{
			savedCharacters.datas.RemoveAt(index);
			savedCharacters.SaveToJson();
		}
		else
			savedCharacters.datas[index] = initialData;
		SceneManager.LoadScene(0);
	}

	IEnumerator OnFinish()
	{
		fadeOuter.SetBool("Open", true);
		yield return new WaitForSeconds(0.3f);

		savedCharacters.SaveToJson();
		SceneManager.LoadScene(0);
	}
}

