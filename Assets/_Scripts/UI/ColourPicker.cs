using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class ColourPicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] RectTransform handle;
	[SerializeField] Material hsvMaterial;
	[SerializeField] string hueID;

	[SerializeField] Image hueHandleImage;
	[SerializeField] Slider hueSlider;
	[SerializeField] Image colorImage;
	[SerializeField] Image backgroundColorImage;
	[SerializeField] Image backgroundImage;
	new RectTransform transform;
	bool holdingDown;
	Color hueColour = Color.red;
	Color colour = Color.white;
	Image handleImage;
	int huePropertyId;
	Camera cam;
	Vector3[] cornersArray = new Vector3[4];
	public Action OnColourChanged 
	void Start()
    {
		cam = Camera.main;
        transform = GetComponent<RectTransform>();
		hueSlider.onValueChanged.AddListener(OnHueChanged);
		handleImage = handle.GetComponent<Image>();

		huePropertyId = Shader.PropertyToID(hueID);
	}

	private void Update()
	{
		if (holdingDown)
		{
			//this was actually good code then i had to make the canvas into camera screen space so now its shitty


			transform.GetWorldCorners(cornersArray);
			float width = cornersArray[2].x - cornersArray[0].x;
			float height = cornersArray[2].y - cornersArray[0].y;

			Vector2 screenMousePos = Mouse.current.position.ReadValue();
			Vector2 mousePos = cam.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, cornersArray[0].z));

			float saturation = Mathf.Clamp01((mousePos.x - cornersArray[0].x) / width);
			float value = Mathf.Clamp01((mousePos.y - cornersArray[0].y) / height);
			handle.position = new Vector3(cornersArray[0].x + width * saturation, cornersArray[0].y + height * value, handle.position.z);
			colour = HSVtoRGB(hueColour, saturation, value);
			handleImage.color = colour;
			colorImage.color = colour;
			backgroundColorImage.color = new Color(1 - colour.r, 1 - colour.g, 1 - colour.b);
		}
	}

	public Color GetCurrentColour()
	{
		return colour;
	}

	public Color GetCurrentHue()
	{
		return hueColour;
	}

	void OnHueChanged (float hue)
	{
		hueColour = HueToRGB(hue);
		hueHandleImage.color = hueColour;
		backgroundImage.color = hueColour;
		hsvMaterial.SetFloat(huePropertyId, hue);

		float width = cornersArray[2].x - cornersArray[0].x;
		float height = cornersArray[2].y - cornersArray[0].y;
		float saturation = (handle.position.x - cornersArray[0].x) / width;
		float value = (handle.position.y - cornersArray[0].y) / height;

		colour = HSVtoRGB(hueColour, saturation, value);
		handleImage.color = colour;
		colorImage.color = colour;
		backgroundColorImage.color = new Color(1 - colour.r, 1 - colour.g, 1 - colour.b);
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

	Color HSVtoRGB(Color rgbHue, float s, float v)
	{
		Vector3 sat = (Vector4)rgbHue;
		Vector3 col = ((sat - Vector3.one) * s + Vector3.one) * v;
		return new Color(col.x, col.y, col.z);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		holdingDown = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		holdingDown = false;
	}
}
