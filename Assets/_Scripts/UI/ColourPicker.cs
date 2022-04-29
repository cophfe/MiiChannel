using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class ColourPicker : MonoBehaviour
{
	[SerializeField] RectTransform colourPicker;
	[SerializeField] RectTransform handle;
	[SerializeField] Material hsvMaterial;
	[SerializeField] string hueID;
	[SerializeField] Animator pickerPanel;
	[SerializeField] Button cancelButton;

	[SerializeField] Image hueHandleImage;
	[SerializeField] Slider hueSlider;
	[SerializeField] Image colorImage;
	[SerializeField] Image backgroundColorImage;
	[SerializeField] Image backgroundImage;
	[SerializeField] Image compareColorImage;
	[SerializeField] Image backgroundCompareColorImage;
	bool holdingDown;
	Color hueColour = Color.red;
	Color colour = Color.white;
	Color compareColour = Color.white; //colour used for comparison
	Image handleImage;
	int huePropertyId;
	Camera cam;
	Vector3[] cornersArray = new Vector3[4];
	Action<Color> onColourChanged;
	Action<Color> onColourCancelled;

	void Start()
    {
		cam = Camera.main;
		hueSlider.onValueChanged.AddListener(OnHueChanged);
		handleImage = handle.GetComponent<Image>();

		huePropertyId = Shader.PropertyToID(hueID);
	}

	private void Update()
	{
		if (holdingDown)
		{
			//this was actually good code then i had to make the canvas into camera screen space so now its shitty
			colourPicker.GetWorldCorners(cornersArray);
			float width = cornersArray[2].x - cornersArray[0].x;
			float height = cornersArray[2].y - cornersArray[0].y;

			Vector2 screenMousePos = Mouse.current.position.ReadValue();
			Vector2 mousePos = cam.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, cornersArray[0].z));

			float saturation = Mathf.Clamp01((mousePos.x - cornersArray[0].x) / width);
			float value = Mathf.Clamp01((mousePos.y - cornersArray[0].y) / height);
			handle.position = new Vector3(cornersArray[0].x + width * saturation, cornersArray[0].y + height * value, handle.position.z);
			colour = HSVtoRGB(hueColour, saturation, value);
			UpdateColorVisuals();
			cancelButton.interactable = true;

			onColourChanged?.Invoke(colour);
		}
	}

	public void SetOnColourCallbacks(Action<Color> onColourChanged, Action<Color> onColourCancelled)
	{
		//overrides last values, there can only be one!!!
		this.onColourChanged = onColourChanged;
		this.onColourCancelled = onColourCancelled;
	}

	public Color GetCurrentColour()
	{
		return colour;
	}

	public Color GetCurrentHue()
	{
		return hueColour;
	}

	//https://www.chilliant.com/rgb2hsv.html

	void OnHueChanged (float hue)
	{
		hueColour = HueToRGB(hue);
		UpdateHueVisuals();

		float width = cornersArray[2].x - cornersArray[0].x;
		float height = cornersArray[2].y - cornersArray[0].y;
		float saturation = (handle.position.x - cornersArray[0].x) / width;
		float value = (handle.position.y - cornersArray[0].y) / height;

		cancelButton.interactable = true;
		colour = HSVtoRGB(hueColour, saturation, value);
		UpdateColorVisuals();
		onColourChanged?.Invoke(colour);
	}

	void UpdateColorVisuals()
	{
		handleImage.color = colour;
		colorImage.color = colour;
		backgroundColorImage.color = new Color(1 - colour.r, 1 - colour.g, 1 - colour.b);
	}

	void UpdateHueVisuals()
	{
		hueHandleImage.color = hueColour;
		backgroundImage.color = hueColour;
		hsvMaterial.SetFloat(huePropertyId, hueSlider.value);
	}

	Color HueToRGB(float h)
	{
		float h6 = 6 * h;
		float r = Mathf.Clamp01(Mathf.Abs(h6 - 3) - 1);
		float g = Mathf.Clamp01(2 - Mathf.Abs(h6 - 2));
		float b = Mathf.Clamp01(2 - Mathf.Abs(h6 - 4));
		return new Color(r, g, b);
	}

	Color HSVtoRGB(float h, float s, float v)
	{
		Vector3 sat = (Vector4)HueToRGB(h);
		Vector3 col = ((sat - Vector3.one) * s + Vector3.one) * v;
		return new Color(col.x, col.y, col.z);
	}

	Vector3 RGBtoHSV(Color rgb)
	{
		Vector4 p = (rgb.g < rgb.b) ? new Vector4(rgb.b, rgb.g, -1.0f, 2.0f / 3.0f) : new Vector4(rgb.g, rgb.b, 0.0f, -1.0f / 3.0f);
		Vector4 q = (rgb.r < p.x) ? new Vector4(p.x, p.y, p.w, rgb.r) : new Vector4(rgb.r, p.y, p.z, p.x);
		float chroma = q.x - Mathf.Min(q.w, q.y);
		float hue = Mathf.Abs((q.w - q.y) / (6 * chroma + 0.0000000001f) + q.z);
		float value = q.x;
		float saturation = chroma / (value + 0.0000000001f);
		return new Vector3(hue, saturation, value);
	}

	Color HSVtoRGB(Color rgbHue, float s, float v)
	{
		Vector3 sat = (Vector4)rgbHue;
		Vector3 col = ((sat - Vector3.one) * s + Vector3.one) * v;
		return new Color(col.x, col.y, col.z);
	}

	public void OnPointerDown()
	{
		holdingDown = true;
	}

	public void OnPointerUp()
	{
		holdingDown = false;
	}

	public void SetCompareColour(Color colour)
	{
		compareColour = colour;
		compareColorImage.color = colour;
		backgroundCompareColorImage.color = new Color(1 - this.colour.r, 1 - this.colour.g, 1 - this.colour.b);
	}
	public void ClosePanel()
	{
		pickerPanel.SetBool("Open", false);
	}

	public void OpenPanel()
	{
		pickerPanel.gameObject.SetActive(true);
		pickerPanel.SetBool("Open", true);
		cancelButton.interactable = false;
		Cancel();
	}

	public void Cancel()
	{
		colour = compareColour;
		Vector3 hsv = RGBtoHSV(colour);

		float width = cornersArray[2].x - cornersArray[0].x;
		float height = cornersArray[2].y - cornersArray[0].y;
		handle.position = new Vector3(cornersArray[0].x + width * hsv.y, cornersArray[0].y + height * hsv.z, handle.position.z);
		hueSlider.value = hsv.x;

  		UpdateHueVisuals();
		UpdateColorVisuals();
		
		onColourCancelled?.Invoke(colour);
	}
}
