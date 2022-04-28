using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomColour : MonoBehaviour
{
	[SerializeField] Image image;
    public void RandomizeColor()
    {
        Color colour = new Color(Random.value, Random.value, Random.value);
		image.color = colour;
    }
}
