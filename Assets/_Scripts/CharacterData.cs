using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CharacterData
{
	public string name;
	public float height;
	public Vector2 position;

	public MaterialModifierData shirtData;
	public MaterialModifierData beltData;
	public MaterialModifierData pantsData;
	public MaterialModifierData shoesData;

	public MaterialModifierData skinData;
	public int facialHairIndex;
	public MaterialModifierData facialHairData;
	public int hairIndex;
	public MaterialModifierData hairData;
	public int hatIndex;
	public MaterialModifierData hatData;

	public CharacterData(Vector2 position)
	{
		this.name = "Joe";
		height = 1.7f;
		this.position = position;

		hairIndex = 0;
		facialHairIndex = 0;
		hatIndex = 0;

		shoesData = new MaterialModifierData(Color.cyan);
		pantsData = new MaterialModifierData(Color.blue);
		beltData = new MaterialModifierData(Color.gray);
		shirtData = new MaterialModifierData(Color.magenta);
		skinData = new MaterialModifierData(new Color(0.7529412f, 0.5333333f, 0.4078431f));
		facialHairData = new MaterialModifierData(new Color(0.3882352f, 0.3411764f, 0.2705f), false);
		hairData = new MaterialModifierData(new Color(0.3882352f, 0.3411764f, 0.2705f));
		hatData = new MaterialModifierData(Color.white, false);
	}
}

[System.Serializable]
public struct MaterialModifierData
{
	public MaterialModifierData(Color color, bool on = true)
	{
		this.color = color;
		this.on = on;
		materialIndex = 0;
	}
	public bool on;
	public int materialIndex;
	public Color32 color;
}
