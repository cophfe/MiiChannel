using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//Used to select objects with perfect accuracy
public class Selector : MonoBehaviour
{
	RenderTexture selectTexture;
	int idPropertyID;
	List<GameObject> selectedObjects;
	
	Camera selectorCam;
	Camera mainCam;
	bool renderCamera = false;
	System.Action<GameObject, Vector3> selectCallback;

	private void Awake()
	{
		selectedObjects = new List<GameObject>();
		idPropertyID = Shader.PropertyToID("_ID");
		selectTexture = new RenderTexture(2, 2, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		selectTexture.filterMode = FilterMode.Point;
		selectTexture.wrapMode = TextureWrapMode.Clamp;
		mainCam = Camera.main;
		//orthagraphic size is half the size of the vertical viewing volume

		selectorCam = gameObject.AddComponent<Camera>();
		selectorCam.targetTexture = selectTexture;
		selectorCam.orthographic = true;
		selectorCam.orthographicSize = mainCam.orthographicSize / Screen.height;
		selectorCam.enabled = false;
		var additionalData = gameObject.AddComponent<UniversalAdditionalCameraData>();
		additionalData.SetRenderer(1);
		additionalData.antialiasing = AntialiasingMode.None;

	}

	public void RegisterSelectable(GameObject selectable)
	{
		selectedObjects.Add(selectable);
		Renderer[] renderers = selectable.GetComponentsInChildren<Renderer>();

		if (renderers != null && renderers.Length > 0)
		{
			MaterialPropertyBlock block = new MaterialPropertyBlock();
			block.SetInt(idPropertyID, selectable.GetInstanceID());

			for (int i = 0; i < renderers.Length; i++)
			{
				renderers[i].SetPropertyBlock(block);
			}
		}
	}

	public bool GetSelectedObject(Vector2 screenSpacePosition, System.Action<GameObject, Vector3> callback)
	{
		if (selectCallback != null)
			return false;

		Ray screenRay = mainCam.ScreenPointToRay(screenSpacePosition);
		selectorCam.transform.position = screenRay.origin;
		selectorCam.transform.forward = screenRay.direction;

		renderCamera = true;
		selectCallback = callback;
		return true;
	}

	void OnReadTexture(AsyncGPUReadbackRequest request)
	{
		var dataArray = request.GetData<float>(0);
		if (dataArray != null && dataArray.Length > 0)
			Debug.Log(dataArray[0]);
		else
			Debug.Log("did ney work :(");
		selectCallback = null;
	}

	private void OnEnable()
	{
		GameManager.Instance.RegisterSelector(this);
		RenderPipelineManager.beginCameraRendering += OnRender;
	}
	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnRender;
	}

	public void OnRender(ScriptableRenderContext ctx, Camera cam)
	{
		if (renderCamera)
		{
			UniversalRenderPipeline.RenderSingleCamera(ctx, selectorCam);

			Texture2D copyTexture = new Texture2D(2, 2, TextureFormat.RGBAFloat, false);
			AsyncGPUReadback.Request(selectTexture, 0, OnReadTexture);
			renderCamera = false;
		}
	}
}
