using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
	[SerializeField] float hoverSpeed = 5;
	[SerializeField] float hoverDrag = 1.0f;
	bool moving = false;
	Vector2 cameraMovement;
	Vector2 velocity;
	float velocityMultiplier;
	Camera attached;
	float targetOrthagraphicSize;

	// Start is called before the first frame update
	void Start()
    {
		attached = GetComponentInChildren<Camera>();
		Vector3 camForward = attached.transform.forward;
		velocityMultiplier = Vector3.Dot(camForward, Vector3.down);
		targetOrthagraphicSize = attached.orthographicSize;
	}

    // Update is called once per frame
    void Update()
    {
		if (moving)
		{
			velocity += hoverSpeed * cameraMovement * Time.deltaTime;
		}
		velocity -= velocity * hoverDrag * Time.deltaTime;

		//ensure camera does not exit bounds
		attached.orthographicSize = targetOrthagraphicSize;
		UpdateCollision();
		
		transform.position =  transform.position + new Vector3(velocity.x * velocityMultiplier, 0, velocity.y);
	}

	void UpdateCollision()
	{
		Plane groundPlane = new Plane(Vector3.up, 0);
		Vector2 boundsMin = GameManager.Instance.BoundsMin;
		Vector2 boundsMax = GameManager.Instance.BoundsMax;

		Vector2 halfSize = new Vector2(attached.orthographicSize * attached.aspect, attached.orthographicSize);
		Vector3 camUp = transform.up;
		Vector3 camRight = transform.right;

		Vector3 rayOffset = camUp * halfSize.y + camRight * halfSize.x;
		Ray cameraMaxRay = new Ray(transform.position + rayOffset, transform.forward);
		Ray cameraMinRay = new Ray(transform.position - rayOffset, transform.forward);
		groundPlane.Raycast(cameraMaxRay, out float camMaxDist);
		groundPlane.Raycast(cameraMinRay, out float camMinDist);
		Vector3 camMax = cameraMaxRay.GetPoint(camMaxDist);
		Vector3 camMin = cameraMinRay.GetPoint(camMinDist);
		if (camMin.x > camMax.x)
		{
			Vector3 min = camMax;
			camMax = camMin;
			camMin = min;
		}

		//restrict size so it cant be bigger than the bounds
		if (camMax.x - camMin.x > boundsMax.x - boundsMin.x + 0.0001f)
		{
			float maxXSize = boundsMax.x - boundsMin.x;
			float maxOrthographicSize = maxXSize * 0.5f / attached.aspect;
			//size is too big, shrink orthagraphic size and try again
			attached.orthographicSize = maxOrthographicSize;
			UpdateCollision();
			return;
		}
		if (camMax.z - camMin.z > boundsMax.y - boundsMin.y + 0.0001f)
		{
			float maxYSize = boundsMax.y - boundsMin.y;
			float maxOrthographicSize = maxYSize * 0.5f;
			//size is too big, shrink orthagraphic size and try again
			attached.orthographicSize = maxOrthographicSize;
			UpdateCollision();
			return;
		}

		if (camMin.x < boundsMin.x)
		{
			transform.position += Vector3.right * (boundsMin.x - camMin.x);
			velocity.x = 0;
		}
		else if (camMax.x > boundsMax.x)
		{
			transform.position -= Vector3.right * (camMax.x - boundsMax.x);
			velocity.x = 0;
		}

		if (camMin.z < boundsMin.y)
		{
			transform.position += Vector3.forward * (boundsMin.y - camMin.z);
			velocity.y = 0;
		}
		else if (camMax.z > boundsMax.y)
		{
			transform.position -= Vector3.forward * (camMax.z - boundsMax.y);
			velocity.y = 0;
		}



	}

	public void OnDragCamera(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			moving = true;
		}
		else if (ctx.canceled)
		{
			moving = false;
		}
	}

	public void OnMouseMove(InputAction.CallbackContext ctx)
	{
		cameraMovement = ctx.ReadValue<Vector2>();
	}
}
