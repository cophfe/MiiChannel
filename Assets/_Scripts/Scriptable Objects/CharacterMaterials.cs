using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Materials", menuName = "ScriptableObjects/Character Materials", order = 1)]
public class CharacterMaterials : ScriptableObject
{
	//need to store default materials for each hair and clothing type, to copy over values using Material CopyPropertiesFromMaterial
	//same is true for skin
	[Header("Defaults")]
	public List<Material> defaultHairs;
	public List<Material> defaultFacialHairs;
	public Material defaultSkin;
	public Material defaultBelt;
	public Material defaultShoes;
	public Material defaultPants;
	public Material defaultShirt;

	[Header("Material Lists")]
	//these are more for shader storage
	public List<Material> hairMaterials = new List<Material>();
	public List<Material> skinMaterials = new List<Material>();
    public List<Material> clothesMaterials = new List<Material>();
    public List<Material> hatMaterials = new List<Material>();


}
