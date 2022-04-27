using System.Collections.Generic;
using UnityEngine;

struct CharacterData
{
	public int id;

	MaterialModifierData shirtMaterialData;
	MaterialModifierData beltMaterialData;
	MaterialModifierData pantsMaterialData;
	MaterialModifierData shoesMaterialData;

	MaterialModifierData skinMaterialData;
	public int facialHairIndex;
	MaterialModifierData facialHairMaterialData;
	public int hairIndex;
	MaterialModifierData hairMaterialData;
	public int hatIndex;
	MaterialModifierData hatMaterialData;
}

struct MaterialModifierData
{
	public int materialIndex;
	public Color32 baseColor;
	public float shinyness;
}

struct HatData
{
	public List<Material> hatMaterials;
	
}