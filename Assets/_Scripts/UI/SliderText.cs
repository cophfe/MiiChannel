using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SliderText : MonoBehaviour
{
	[Range(0, 10)]
	[SerializeField] int decimalCount = 2;
	[SerializeField] Slider slider;
	[SerializeField] TextMeshProUGUI sliderText;

	private void Awake()
	{
		slider.onValueChanged.AddListener(Evaluate);
		Evaluate(slider.value);
	}
	public void Evaluate(float value)
	{
		sliderText.text = String.Format("{0:F" + decimalCount + "}", value);
	}

}
