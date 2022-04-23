using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//Used to select objects with perfect accuracy, and get the position of the selection
public class Selector : MonoBehaviour
{
	RenderTexture selectTexture;
	//the id property
	int idPropertyID;

	//objects in this list have their ids accessible by the shader
	List<GameObject> selectableObjects;
	
	Camera selectorCam;
	Camera mainCam;
	bool renderCamera = false;
	System.Action<GameObject, Vector3> selectCallback;

	private void Awake()
	{
		selectableObjects = new List<GameObject>();
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
		//this requires the second renderer to be the selector renderer
		additionalData.SetRenderer(1);
		additionalData.antialiasing = AntialiasingMode.None;

	}

	//registers a selectable to be rendered
	public void RegisterSelectable(GameObject selectable)
	{
		selectableObjects.Add(selectable);
		Renderer[] renderers = selectable.GetComponentsInChildren<Renderer>(true);

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
		if (selectCallback != null || callback == null)
			return false;

		//screen point to ray gives the exact position and direction the selectorcam should point
		Ray screenRay = mainCam.ScreenPointToRay(screenSpacePosition);
		selectorCam.transform.position = screenRay.origin;
		selectorCam.transform.forward = screenRay.direction;
		//size should be 2 pixels. orthagraphic size is half the vertical size, therefore 2 pixels is equal to 2 * (mainCam.orthagraphicSize/2) / Screen.height
		selectorCam.orthographicSize = mainCam.orthographicSize / Screen.height; 

		renderCamera = true;
		selectCallback = callback;
		return true;
	}

	void OnReadTexture(AsyncGPUReadbackRequest request)
	{
		var dataArray = request.GetData<Vector4>(0);
		if (dataArray != null && dataArray.Length > 0)
		{
			Vector3 position = dataArray[0];
			if(selectCallback != null)
			{
				int instanceId = (int)dataArray[0].w;
				foreach (var selectable in selectableObjects)
				{
					if (instanceId == selectable.GetInstanceID())
					{
						selectCallback.Invoke(selectable, position);
						selectCallback = null;
						return;
					}
					
				}

				//if could not find object matching id
				selectCallback.Invoke(null, position);
				selectCallback = null;
				return;
			}
		}

		selectCallback.Invoke(null, Vector3.negativeInfinity);
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
