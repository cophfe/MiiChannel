using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GetPicker : MonoBehaviour
{
	[SerializeField] ColourPicker colourPicker;
	[SerializeField] Image image;
	Action<Color> onColorChange;

	public void OpenPicker()
	{
		colourPicker.SetCompareColour(image.color);
		colourPicker.SetOnColourCallbacks(OnColourChanged, OnColourCancelled);
		colourPicker.OpenPanel();
	}

	public void SetCallback(Action<Color> onColorChange)
	{
		this.onColorChange = onColorChange;
	}

	void OnColourChanged(Color color)
	{
		image.color = color;
		onColorChange?.Invoke(color);
	}

	void OnColourCancelled(Color color)
	{
		image.color = color;
		onColorChange?.Invoke(color);
	}

	public Color GetColour()
	{
		return image.color;
	}

	public void SetColour(Color color, bool invokeOnColourChange = true)
	{
		image.color = new Color(color.r, color.g, color.b);
		if (invokeOnColourChange)
			onColorChange?.Invoke(color);
	}
}
