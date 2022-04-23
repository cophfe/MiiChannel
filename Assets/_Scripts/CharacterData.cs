using System.Collections.Generic;
using UnityEngine;

struct CharacterData
{
	public int shirtMaterialIndex;
	public int beltMaterialIndex;
	public int pantsMaterialIndex;
	public int shoesMaterialIndex;

	public int facialHairIndex;
	public int facialHairMaterialIndex;

	public int hairIndex;
	public int hairMaterialIndex;

	public int hatIndex;
}

struct HatData
{
	public List<Material> hatMaterials;
	
}