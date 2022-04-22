using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

//used to generate an overlay texture and draw it on models
public class CastOverlay : MonoBehaviour
{
	[SerializeField] Collider castMesh;
	[SerializeField] Renderer meshRenderer;
	[SerializeField] Texture2D mainTexture;
	[SerializeField] Material objectMaterial;
	[SerializeField] Material constructorMaterial;
	[SerializeField] string overlayTextureName;

	Camera mainCamera;
	bool render = false;
	int positionId;
	int lastTextureId;
	RenderTexture[] overlays;
	int readIndex = 0;
	int writeIndex = 1;

	void Start()
	{
		mainCamera = Camera.main;
		overlays = new RenderTexture[2];
		overlays[0] = new RenderTexture(mainTexture.width, mainTexture.height, 0);
		overlays[1] = new RenderTexture(mainTexture.width, mainTexture.height, 0);

		positionId = Shader.PropertyToID("_DrawPosition");
		lastTextureId = Shader.PropertyToID("_MainTex");
	}
	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += OnRender;
	}
	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnRender;
	}

	public void OnMouse(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			render = true;
		}
		else if (ctx.canceled)
		{
			render = false;
		}
		
	}

	public void OnRender(ScriptableRenderContext ctx, Camera cam)
	{
		//idk how to get the scriptable render context other than through this, so this is how its happening I guess

		if (cam == mainCamera && render)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Ray ray = mainCamera.ScreenPointToRay(mousePos);
			if (castMesh.Raycast(ray, out var hit, Mathf.Infinity))
			{
				CommandBuffer renderBuffer = CommandBufferPool.Get();
				renderBuffer.SetRenderTarget(overlays[writeIndex]);
				renderBuffer.DrawRenderer(meshRenderer, constructorMaterial);

				constructorMaterial.SetVector(positionId, hit.point);
				constructorMaterial.SetTexture(lastTextureId, overlays[readIndex]);

				int cashe = readIndex;
				readIndex = writeIndex;
				writeIndex = cashe;

				ctx.ExecuteCommandBuffer(renderBuffer);
				renderBuffer.Release();
				
				objectMaterial.SetTexture(overlayTextureName, overlays[writeIndex]);
			}
		}
	}
}
