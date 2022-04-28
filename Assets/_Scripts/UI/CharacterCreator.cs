using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CharacterCreator : MonoBehaviour
{
	[System.Serializable]
	public struct HeadFeature
	{
		public SelectorUI type;
		public SelectorUI material;
		public GetPicker colour;
	}
	public struct ClothingFeature
	{
		public SelectorUI material;
		public GetPicker colour;
	}

	[SerializeField] CharacterVanity character; // get and set data with the vanity

	[Header("Head")]
	[SerializeField] HeadFeature hat;
	[SerializeField] HeadFeature hair;
	[SerializeField] HeadFeature facialHair;

	[Header("Body")]
	[SerializeField] TMP_InputField name;
	[SerializeField] Slider height;
	[SerializeField] GetPicker skinColour;
	[SerializeField] ClothingFeature shirt;
	[SerializeField] ClothingFeature belt;
	[SerializeField] ClothingFeature pants;
	[SerializeField] ClothingFeature shoes;

	[Header("Tattoer")]
	[SerializeField] Selector selector;
	[SerializeField] CastOverlay painter;
	[SerializeField] ColourPicker colourPicker;
	
	//character data material is set with this
	CharacterData data;
}

