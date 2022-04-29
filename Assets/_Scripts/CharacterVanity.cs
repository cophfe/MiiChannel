using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterVanity : MonoBehaviour
{
	[SerializeField] SavedCharacters characters;
	[SerializeField] CharacterMaterials materials;
	[SerializeField] int characterIndex;

	[SerializeField] Renderer skin;
	[SerializeField] Transform hairParent;
	[SerializeField] Transform facialHairParent;
	[SerializeField] Transform hatParent;
	[SerializeField] Transform clothesParent;

	GameObject hat;
	GameObject facialHair;
	GameObject hair;
	GameObject belt;
	GameObject shoes;
	GameObject pants;
	GameObject shirt;

	public void SetIndex(int index)
	{
		characterIndex = index;
	}

	public int GetIndex()
	{
		return characterIndex;
	}

	public int GetHatCount()
	{
		return hatParent.childCount;
	}
	public int GetHairCount()
	{
		return hairParent.childCount;
	}
	public int GetFacialHairCount()
	{
		return facialHairParent.childCount;
	}

	private void Start()
	{
		LoadFromVanity();
	}

	//this is super super super slow
	public void LoadFromVanity()
	{
		LoadNameVanity();
		LoadHairVanity(true);
		LoadHatVanity(true);
		LoadBodyVanity(true);
	}

	public void LoadNameVanity()
	{
		if (characters.datas == null || characterIndex > characters.datas.Count)
			return;
		CharacterData cData = characters.datas[characterIndex];
		gameObject.name = cData.name;
	}

	public void LoadHatVanity(bool materialChanged)
	{
		if (characters.datas == null || characterIndex > characters.datas.Count)
			return;
		CharacterData cData = characters.datas[characterIndex];

		hat = SetUpFeature(cData.hatIndex, hatParent);
		hairParent.gameObject.SetActive(!cData.hatData.on);
		if (hat == null)
			return;

		if (materialChanged)
		{
			//hats why you got to be so different
			//hats have a bunch of different materials with different colours
			//also hat color should be tints, because all hats are differently coloured and a base color wouldn't work well unless it is saved per hat
			if (cData.hatData.materialIndex < materials.hatMaterials.Count)
			{
				Renderer hatRenderer = hat.GetComponent<Renderer>();

				Material[] currentHatMats = hatRenderer.sharedMaterials;
				Material[] newHatMats = new Material[currentHatMats.Length];
				for (int i = 0; i < currentHatMats.Length; i++)
				{
					//Garbage collector will probably be doing a lot of work right here
					Material hatMaterial = Instantiate(materials.hatMaterials[cData.hatData.materialIndex]);
					//copy base color over to new material (and other stuff)
					CopyMaterialProperties(hatMaterial, currentHatMats[i]);

					newHatMats[i] = hatMaterial;
					newHatMats[i].SetColor("_Tint", cData.hatData.color);
				}
				hatRenderer.sharedMaterials = newHatMats;
			}
		}
		else
		{
			Renderer hatRenderer = hat.GetComponent<Renderer>();
			Material[] currentHatMats = hatRenderer.sharedMaterials;
			for (int i = 0; i < currentHatMats.Length; i++)
			{
				currentHatMats[i].SetColor("_Tint", cData.hatData.color);
			}
		}
		hat.SetActive(cData.hatData.on);
	}

	public void LoadHairVanity(bool materialChanged)
	{
		if (characters.datas == null || characterIndex > characters.datas.Count)
			return;
		CharacterData cData = characters.datas[characterIndex];

		facialHair = SetUpFeature(cData.facialHairIndex, facialHairParent);
		hair = SetUpFeature(cData.hairIndex, hairParent);

		if (materialChanged)
		{
			SetupMaterialInformation(hair.GetComponent<Renderer>(), cData.hairData, materials.hairMaterials, materials.defaultHairs[cData.hairIndex]);
			SetupMaterialInformation(facialHair.GetComponent<Renderer>(), cData.facialHairData, materials.hairMaterials, materials.defaultFacialHairs[cData.facialHairIndex]);
		}
		else
		{
			hair.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.hairData.color);
			facialHair.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.facialHairData.color);
		}

		hair.SetActive(cData.hairData.on);
		facialHair.SetActive(cData.facialHairData.on);
	}

	public void LoadBodyVanity(bool materialChanged)
	{
		if (characters.datas == null || characterIndex > characters.datas.Count)
			return;
		CharacterData cData = characters.datas[characterIndex];

		belt = clothesParent.GetChild(0).gameObject;
		shoes = clothesParent.GetChild(1).gameObject;
		pants = clothesParent.GetChild(2).gameObject;
		shirt = clothesParent.GetChild(3).gameObject;

		//setup materials
		if (materialChanged)
		{
			SetupMaterialInformation(skin, cData.skinData, materials.skinMaterials, materials.defaultSkin);
			SetupMaterialInformation(shirt.GetComponent<Renderer>(), cData.shirtData, materials.clothesMaterials, materials.defaultShirt);
			SetupMaterialInformation(pants.GetComponent<Renderer>(), cData.pantsData, materials.clothesMaterials, materials.defaultPants);
			SetupMaterialInformation(shoes.GetComponent<Renderer>(), cData.shoesData, materials.clothesMaterials, materials.defaultShoes);
			SetupMaterialInformation(belt.GetComponent<Renderer>(), cData.beltData, materials.clothesMaterials, materials.defaultBelt);
		}
		else
		{
			skin.sharedMaterial.SetColor("_BaseColor", cData.skinData.color);
			shirt.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.shirtData.color);
			pants.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.pantsData.color);
			shoes.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.shoesData.color);
			belt.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", cData.beltData.color);
		}

		shirt.SetActive(cData.shirtData.on);
		pants.SetActive(cData.pantsData.on);
		shoes.SetActive(cData.shoesData.on);
		belt.SetActive(cData.beltData.on);

		//default height is 1.7 meters
		transform.localScale = Vector3.one * (cData.height / 1.7f);
	}


	void SetupMaterialInformation(Renderer renderer, MaterialModifierData data, List<Material> materials, Material defaultMat)
	{
		int count = materials.Count;
		if (data.materialIndex >= count)
			return;

		Material mat = materials[data.materialIndex];
		renderer.sharedMaterial = mat;

		//create instance of material and set its values
		mat = renderer.material;
		CopyMaterialProperties(mat, defaultMat);
		mat.SetColor("_BaseColor", data.color);
	}

	GameObject SetUpFeature(int index, Transform parent)
	{
		int count = parent.childCount;
		for (int i = 0; i < count; i++)
		{
			parent.GetChild(i).gameObject.SetActive(i == index);
		}

		if (index >= count || index < 0)
			return null;
		else
			return parent.GetChild(index).gameObject;
	}

	void CopyMaterialProperties(Material dest, Material copy)
	{
		//based on property names from the standard shader https://github.com/TwoTailsGames/Unity-Built-in-Shaders/blob/master/DefaultResourcesExtra/Standard.shader
		dest.SetColor("_BaseColor", copy.GetColor("_BaseColor"));
		dest.SetTexture("_BaseMap", copy.GetTexture("_BaseMap"));
		dest.SetTexture("_MetallicGlossMap", copy.GetTexture("_MetallicGlossMap"));
		dest.SetTexture("_BumpMap", copy.GetTexture("_BumpMap"));
		dest.SetFloat("_Smoothness", copy.GetFloat("_Smoothness"));

		//this is probably faster but is really broken when there are different shaders involved
		//mat.CopyPropertiesFromMaterial(defaultMat);
	}

}
