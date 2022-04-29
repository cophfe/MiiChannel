using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomColour : MonoBehaviour
{
	[SerializeField] GetPicker color;
    public void RandomizeColor()
    {
        Color colour = new Color(Random.value, Random.value, Random.value);
		color.SetColour(colour);
    }
}
