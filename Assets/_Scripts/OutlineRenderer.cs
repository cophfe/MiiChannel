using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineRenderer : MonoBehaviour
{
	[SerializeField, Range(0.1f, 1)] float outlineResolutionScaling = 1.0f;
	[SerializeField] Material outlineMaterial = null;

	RenderTexture outlineTexture;

	//note:this does require background colour to be black
	Camera mainCamera;
	UniversalAdditionalCameraData mainCameraAddition;

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += OnRender;
		mainCamera = Camera.main;
		mainCameraAddition = mainCamera.GetComponent<UniversalAdditionalCameraData>();

		if (outlineTexture == null)
		{
			Vector2Int outlineTextureSize = new Vector2Int((int)(outlineResolutionScaling * Screen.width), (int)(outlineResolutionScaling * Screen.height));
			outlineTexture = new RenderTexture(outlineTextureSize.x, outlineTextureSize.y, 0);
			outlineMaterial?.SetTexture("_OutlineData", outlineTexture);
		}
	}
	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnRender;
	}

	public void OnRender(ScriptableRenderContext ctx, Camera cam)
	{
		if (cam == mainCamera)
		{
			Vector2Int outlineTextureSize = new Vector2Int((int)(outlineResolutionScaling * Screen.width), (int)(outlineResolutionScaling * Screen.height));
			if (outlineTexture.width != outlineTextureSize.x || outlineTexture.height != outlineTextureSize.y)
			{
				outlineTexture.Release();
				outlineTexture = new RenderTexture(outlineTextureSize.x, outlineTextureSize.y, 0);
				outlineMaterial?.SetTexture("_OutlineData", outlineTexture);
			}

			cam.targetTexture = outlineTexture;
			mainCameraAddition.SetRenderer(2);
			UniversalRenderPipeline.RenderSingleCamera(ctx, cam);

			cam.targetTexture = null;
			mainCameraAddition.SetRenderer(0);

		}
	}
}
