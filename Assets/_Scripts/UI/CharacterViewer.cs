using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Resizes character viewer rendertexture
public class CharacterViewer : MonoBehaviour
{
	[SerializeField] RenderTexture characterViewerTexture;
	[SerializeField] RectTransform viewerPanel;
	[SerializeField] Camera viewerCamera;

	Camera main;
	Vector2 panelSize = Vector2.zero;
	RawImage viewerPanelImage;
	Vector3[] worldCorners = new Vector3[4];

    // Start is called before the first frame update
    void Start()
    {
		main = Camera.main;
		viewerPanelImage = viewerPanel.GetComponent<RawImage>();
	}

    // Update is called once per frame
    void Update()
    {
		viewerPanel.GetWorldCorners(worldCorners);
		Vector2 panelSize = main.WorldToScreenPoint(worldCorners[2]) - main.WorldToScreenPoint(worldCorners[0]);

		if (panelSize != this.panelSize)
		{
			RenderTexture newCharacterViewerTexture = new RenderTexture((int)panelSize.x, (int)panelSize.y, 24, characterViewerTexture.format, characterViewerTexture.mipmapCount);
			newCharacterViewerTexture.antiAliasing = characterViewerTexture.antiAliasing;
			this.panelSize = panelSize;

			characterViewerTexture.Release();
			characterViewerTexture = newCharacterViewerTexture;
			viewerCamera.targetTexture = characterViewerTexture;
			viewerPanelImage.texture = newCharacterViewerTexture;
			
		}

	}
}

