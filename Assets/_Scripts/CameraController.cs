using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
	[SerializeField] float speed = 5;
	[SerializeField] float drag = 1.0f;

	bool moving = false;
	Vector2 cameraMovement;
	Vector2 velocity;
	float velocityMultiplier;

	// Start is called before the first frame update
	void Start()
    {
		Vector3 camForward = Camera.main.transform.forward;
		velocityMultiplier = Vector3.Dot(camForward, Vector3.down);
	}

    // Update is called once per frame
    void Update()
    {
        if (moving)
		{
			velocity += speed * cameraMovement * Time.deltaTime;
		}

		velocity -= velocity * drag * Time.deltaTime;
		transform.position += new Vector3(velocity.x * velocityMultiplier, 0, velocity.y);
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
