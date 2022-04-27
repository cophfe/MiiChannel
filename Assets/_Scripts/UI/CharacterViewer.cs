using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Resizes character viewer rendertexture
public class CharacterViewer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField] RenderTexture characterViewerTexture;
	[SerializeField] RectTransform viewerPanel;
	[SerializeField] Camera viewerCamera;
	[SerializeField] Orbiter orbiter;

	Vector2 panelSizeDelta = Vector2.zero;
	RawImage viewerPanelImage;
	
    // Start is called before the first frame update
    void Start()
    {
		viewerPanelImage = viewerPanel.GetComponent<RawImage>();
	}

    // Update is called once per frame
    void Update()
    {
        if (viewerPanel.sizeDelta != panelSizeDelta)
		{
			panelSizeDelta= viewerPanel.sizeDelta;

			RenderTexture newCharacterViewerTexture = new RenderTexture((int)panelSizeDelta.x, (int)panelSizeDelta.y, 24, characterViewerTexture.format, characterViewerTexture.mipmapCount);
			newCharacterViewerTexture.antiAliasing = characterViewerTexture.antiAliasing;

			characterViewerTexture.Release();
			characterViewerTexture = newCharacterViewerTexture;
			viewerCamera.targetTexture = characterViewerTexture;
			viewerPanelImage.texture = newCharacterViewerTexture;
			
		}

	}

	public void OnPointerDown(PointerEventData eventData)
	{
		orbiter.IsInputEnabled = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		orbiter.IsInputEnabled = false;
	}
}

