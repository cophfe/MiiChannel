using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

//used to generate an overlay texture and draw it on models
public class CastOverlay : MonoBehaviour
{
	[SerializeField] Camera mainCamera;
	[SerializeField] Selector selector;
	[SerializeField] Renderer meshRenderer;
	[SerializeField] Texture2D mainTexture;
	[SerializeField] Material objectMaterial;
	[SerializeField] Material constructorMaterial;

	bool render = false;
	bool selecting = false;
	int positionId;
	int lastTextureId;
	RenderTexture[] overlays;
	int readIndex = 0;
	int writeIndex = 1;
	Vector3 castPosition = Vector3.positiveInfinity;
	void Start()
	{
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
			selecting = true;
		}
		else if (ctx.canceled)
		{
			selecting = false;
		}
		
	}

	private void Update()
	{
		if (selecting)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelect);
		}
	}

	void OnSelect(GameObject gameObject, Vector3 position)
	{
		//gameobject will be null if no 'selectable' component attached

		if (position != Vector3.negativeInfinity)
		{
			render = true;
			castPosition = position;
		}
		else
		{
			render = false;
			castPosition = Vector3.positiveInfinity;
		}
	}

	public void OnRender(ScriptableRenderContext ctx, Camera cam)
	{
		//idk how to get the scriptable render context other than through this, so this is how its happening I guess

		if (cam == mainCamera && render)
		{
			CommandBuffer renderBuffer = CommandBufferPool.Get();
			renderBuffer.SetRenderTarget(overlays[writeIndex]);
			renderBuffer.DrawRenderer(meshRenderer, constructorMaterial);

			constructorMaterial.SetVector(positionId, castPosition);
			constructorMaterial.SetTexture(lastTextureId, overlays[readIndex]);

			int cashe = readIndex;
			readIndex = writeIndex;
			writeIndex = cashe;

			ctx.ExecuteCommandBuffer(renderBuffer);
			renderBuffer.Release();
				
			objectMaterial.SetTexture("_Overlay", overlays[writeIndex]);
		}
	}
}
