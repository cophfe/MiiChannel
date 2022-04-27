using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RandomName : MonoBehaviour
{
    [SerializeField] List<string> first = new List<string>();
    [SerializeField] List<string> last = new List<string>();
    [SerializeField] TMP_InputField textBox;

	public void Generate()
	{
		if (first.Count > 0 && last.Count > 0)
			textBox.text = first[Random.Range(0, first.Count - 1)] + " " + last[Random.Range(0, first.Count - 1)];
	}
}
