using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
	[SerializeField] float speed = 2;
	[SerializeField] float acceleration= 2;
	[SerializeField] float walkTime = 3;
	[SerializeField] float walkTimeVariance = 2;
	[SerializeField] float waitTime = 6;
	[SerializeField] float waitTimeVariance = 4;
	float walkTimer;
	bool walking;
	Vector3 velocity = Vector3.zero; //note: locked on xy plane
	Vector3 walkDirection;
	
	//ragdoll
	[SerializeField] Transform torsoTransform;
	[SerializeField] float getUpTime = 1;
	float getUpTimer = 0;
	Rigidbody[] ragdollBodies;
	Animator animator;
	public bool DisallowRagdollCancel { get; set; } = false;
	bool ragdolling = false;
	//

    void Start()
    {
		animator = GetComponent<Animator>();
		animator.enabled = true;

		ragdollBodies = GetComponentsInChildren<Rigidbody>();
		foreach (var body in ragdollBodies)
		{
			body.isKinematic = true;
		}
    }

    void Update()
    {
		//Update animation

		//get perpendicular to vector3.up facing in direction of mainForward
		Vector3 mainForward = torsoTransform.forward;
		Vector3 facingDirection = Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;
		Vector3 rightDirection = new Vector3(-facingDirection.z, facingDirection.y, facingDirection.x);

		animator.SetFloat("Speed", Vector3.Dot(facingDirection, velocity));
		animator.SetFloat("Strafe Speed", Vector3.Dot(rightDirection, velocity));
		//

		//Ragdoll stuff
		if (ragdolling && !DisallowRagdollCancel && ragdollBodies.Length > 0 && ragdollBodies[0].velocity.sqrMagnitude < 0.02f)
		{
			getUpTimer += Time.deltaTime;
			if (getUpTimer >= getUpTime)
			{
				EnableRagdoll(false);
				getUpTimer = 0;
			}
		}
		else
			getUpTimer = 0;

	}

	public void EnableRagdoll(bool enable)
	{
		if (ragdolling == enable)
			return;

		ragdolling = enable;
		if (enable)
		{
			foreach (var body in ragdollBodies)
			{
				body.isKinematic = false;
			}
			animator.enabled = false;
		}
		else
		{
			Vector3 mainPosition = torsoTransform.position;
			Vector3 mainForward = torsoTransform.forward;
			//get perpendicular to vector3.up facing in direction of mainForward
			Vector3 facingDirection = Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;

			animator.SetTrigger("Get Up");
			transform.position = new Vector3(mainPosition.x, transform.position.y, mainPosition.z);
			transform.forward = facingDirection;

			foreach (var body in ragdollBodies)
			{
				body.isKinematic = true;
			}
			animator.enabled = true;
		}
	}
}
