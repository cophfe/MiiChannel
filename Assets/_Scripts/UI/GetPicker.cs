using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GetPicker : MonoBehaviour
{
	[SerializeField] ColourPicker colourPicker;
	[SerializeField] Image image;

	public void OpenPicker()
	{
		colourPicker.SetCompareColour(image.color);
		colourPicker.SetOnColourCallbacks(OnColourChanged, OnColourCancelled);
		colourPicker.OpenPanel();
	}

	void OnColourChanged(Color color)
	{
		image.color = color;
	}

	void OnColourCancelled(Color color)
	{
		image.color = color;
	}
}
