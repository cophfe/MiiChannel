using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Orbiter : MonoBehaviour
{
	[SerializeField] Transform target;
	[SerializeField] Vector3 targetOffset;
	[SerializeField] Vector3 defaultDirection = Vector3.forward;
	[SerializeField] float orbitRadius = 1;
	[SerializeField] float maxRotateAngleHorizontal = 60;
	[SerializeField] float maxRotateAngleVertical = 30;
	[SerializeField] float movementSpeed = 5;
	[SerializeField] float rotateSpeed = 5;
	[SerializeField] float sensitivity = 1;
	[SerializeField] float horizontalSensitivityMod = 1;
	
	public bool IsInputEnabled { get; set; } = false;
	bool clickHeld;

	Vector3 orbitTarget;
	Vector3 targetOrbitVector;
	Vector3 orbitVector;
	Vector2 rotation = Vector2.zero;

	private void Start()
	{
		orbitVector = defaultDirection;
		targetOrbitVector = orbitVector * orbitRadius;
		orbitTarget = target.position + targetOffset;
	}

	private void OnValidate()
	{
		defaultDirection = defaultDirection.normalized;
	}

	public void SetInputEnabled(bool enabled)
	{
		IsInputEnabled = enabled;
	}

	public void SetTarget(Transform target)
	{
		this.target = target;
	}

	public void SetRadius(float radius)
	{
		orbitRadius = radius;
		targetOrbitVector = targetOrbitVector.normalized * orbitRadius;
	}

	private void Update()
	{

		orbitVector = Vector3.RotateTowards(orbitVector, targetOrbitVector, Time.deltaTime * Mathf.Deg2Rad * Vector3.Angle(orbitVector, targetOrbitVector) * rotateSpeed, Time.deltaTime * movementSpeed * Mathf.Abs(orbitVector.magnitude - targetOrbitVector.magnitude));
		Vector3 targetOrbitTarget = target.position + targetOffset;

		orbitTarget = Vector3.MoveTowards(orbitTarget, targetOrbitTarget, Time.deltaTime * Vector3.Magnitude(orbitTarget - targetOrbitTarget) * movementSpeed);
		transform.position = orbitTarget + orbitVector;
		
		transform.LookAt(orbitTarget, Vector3.up);
	}

	public void OnLook(InputAction.CallbackContext ctx)
	{
		if (ctx.performed && IsInputEnabled && clickHeld)
		{
			Vector2 value = ctx.ReadValue<Vector2>();
			rotation.x = Mathf.Clamp(rotation.x + value.y * sensitivity, -maxRotateAngleVertical, maxRotateAngleVertical);
			rotation.y = Mathf.Clamp(rotation.y + value.x * sensitivity * horizontalSensitivityMod, -maxRotateAngleHorizontal, maxRotateAngleHorizontal);

			Quaternion rotationQuat = Quaternion.Euler(rotation.x, rotation.y, 0);
			targetOrbitVector = rotationQuat * defaultDirection * orbitRadius;

		}
	}

	public void OnClick(InputAction.CallbackContext ctx)
	{
		if (ctx.performed)
		{
			clickHeld = true;
		}
		else if (ctx.canceled)
		{
			clickHeld = false;
			targetOrbitVector = defaultDirection * orbitRadius;
			rotation = Vector2.zero;
		}
	}
}
