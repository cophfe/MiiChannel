using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

	//Outline Info
	[SerializeField] string outlineLayerName = "Outlined";
	[SerializeField] Material outlineMaterial;
	[SerializeField] Color selectedColour;
	[SerializeField] Color hovorColour;
	//Ragdoll Drag
	[SerializeField] float ragdollDragStartDistance = 2;
	[SerializeField] SpringJoint ragdollJoint;

	//Layer Masks for outlining
	int lastLayer;
	int outlineLayer;
	//Ragdoll drag
	LineRenderer jointLine;
	float dragDistance = 0;
	bool draggingRagdoll = false;
	Plane dragPlane = new Plane(Vector3.up, Vector3.zero);
	Rigidbody jointBody;
	CharacterAI draggedAI;
	bool mouseHeld = false;
	//Selection data
	Selector selector;
	bool selecting = true;
	Vector3 selectedPosition;
	GameObject selected = null;

	float storedDrag;
	float storedAngularDrag;

	private void Start()
	{
		outlineLayer = LayerMask.NameToLayer(outlineLayerName);
		selector = GameManager.Instance.Selector;
		outlineMaterial.SetColor("_OutlineColour", hovorColour);

		jointBody = ragdollJoint.GetComponent<Rigidbody>();
		jointLine = ragdollJoint.GetComponent<LineRenderer>();
		jointLine.enabled = false;
	}

	private void Update()
	{
		if (selecting && !GameManager.Instance.UI.Interacting)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelectedObject);
		}
		else if (draggingRagdoll && ragdollJoint.connectedBody != null)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Ray cameraRay = GameManager.Instance.MainCamera.ScreenPointToRay(mousePos);
			if (dragPlane.Raycast(cameraRay, out float planeDistance))
			{
				ragdollJoint.transform.position = cameraRay.GetPoint(planeDistance);
				Vector3 connectedPosWorld = ragdollJoint.connectedBody.transform.TransformPoint(ragdollJoint.connectedAnchor);
				ragdollJoint.anchor = Vector3.zero;
				jointLine.SetPosition(1, ragdollJoint.transform.InverseTransformPoint(connectedPosWorld));
				//Debug.DrawLine(ragdollJoint.transform.position, connectedPosWorld, Color.red);
			}
		}
	}

	public void OnClick(InputAction.CallbackContext ctx)
	{
		if (ctx.performed && !GameManager.Instance.UI.Interacting)
		{
			mouseHeld = true;
			selecting = false;
			Vector2 mousePos = Mouse.current.position.ReadValue();
			selector.GetSelectedObject(mousePos, OnSelectedObject);
		}
		else if(ctx.canceled)
		{
			mouseHeld = false;
			if (draggingRagdoll && ragdollJoint.connectedBody != null)
			{
				ClearSpringJoint();
				draggingRagdoll = false;
			}
		}
	}

	public void OnMouseDrag(InputAction.CallbackContext ctx)
	{
		//if selected a character to drag
		if (mouseHeld && !draggingRagdoll && !selecting && selected != null)
		{
			var ai = selected.GetComponent<CharacterAI>();
			if (ai != null)
			{
				dragDistance += ctx.ReadValue<Vector2>().magnitude;
				if (dragDistance > ragdollDragStartDistance)
				{
					draggingRagdoll = true;

					//turn into ragdoll and start dragging ragdoll with spring joint
					ai.EnableRagdoll(true, true);
					draggedAI = ai;
					draggedAI.CanGetUp = false;

					Collider[] colliders = draggedAI.GetRagdollColliders();
					//Get point on the closest ragdoll rigidbody from point on skinned mesh
					float sqrDistance = Mathf.Infinity;
					Collider closestCollider = null;
					Vector3 rigidbodyPoint = Vector3.zero;
					for (int i = 0; i < colliders.Length; i++)
					{
						Vector3 newRigidbodyPoint = colliders[i].ClosestPoint(selectedPosition);
						float newRigidbodySqrDist = (selectedPosition - newRigidbodyPoint).sqrMagnitude;
						if (newRigidbodySqrDist < sqrDistance)
						{
							sqrDistance = newRigidbodySqrDist;
							rigidbodyPoint = newRigidbodyPoint;
							closestCollider = colliders[i];

							if (sqrDistance < 0.01f)
								break;
						}
					}
					
					//when found close enough collider, create joint on ragdoll
					if (sqrDistance != Mathf.Infinity && closestCollider.attachedRigidbody != null)
					{
						SetupSpringJoint(closestCollider.attachedRigidbody, rigidbodyPoint);
					}
					else
					{
						ClearSpringJoint();
					}
				}
			}
		}
	}

	void SetupSpringJoint(Rigidbody connected, Vector3 position)
	{
		dragPlane = new Plane(Vector3.up, position);
		ragdollJoint.connectedBody = connected;
		//ragdollJoint.connectedAnchor = Vector3.zero;
		ragdollJoint.connectedAnchor = connected.transform.InverseTransformPoint(position);
		ragdollJoint.transform.position = position;
		ragdollJoint.anchor = Vector3.zero;
		jointLine.enabled = true;

		storedDrag = connected.drag;
		storedAngularDrag = connected.angularDrag;
		connected.drag = jointBody.drag;
		connected.angularDrag = jointBody.angularDrag;
	}

	void ClearSpringJoint()
	{
		ragdollJoint.connectedBody.drag = storedDrag;
		ragdollJoint.connectedBody.angularDrag = storedAngularDrag;
		ragdollJoint.connectedBody = null;
		dragDistance = 0;
		jointLine.enabled = false;
		if (draggedAI)
			draggedAI.CanGetUp = true;
	}

	void OnSelectedObject(GameObject gameObject, Vector3 position)
	{
		if (selected != null)
		{
			RecursiveSetLayer(selected, lastLayer);
		}
		selected = gameObject;

		if (gameObject)
		{
			lastLayer = selected.layer;
			RecursiveSetLayer(selected, outlineLayer);

			//if this finishes a selection 
			if (!selecting)
			{
				//select this selector
				outlineMaterial.SetColor("_OutlineColour", selectedColour);
				dragDistance = 0;
				selectedPosition = position;
			}
		}
		else if (!selecting)
		{
			outlineMaterial.SetColor("_OutlineColour", hovorColour);
			selecting = true;
		}
		
		
		//Debug.DrawLine(position, Camera.main.transform.position - new Vector3(0, 0.05f, 0));
	}

	void RecursiveSetLayer(GameObject gO, int layer)
	{
		if (gO == null)
			return;

		gO.layer = layer;
		foreach (Transform child in gO.transform)
		{
			RecursiveSetLayer(child.gameObject, layer);
		}
	}
}
