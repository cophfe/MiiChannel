using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
	[SerializeField] AIBehaviour behaviour;
	[SerializeField] Transform spineTransform;
	[SerializeField] Transform headTransform;
	//ai
	float walkTimer;
	bool walking = false;
	Vector3 velocity = Vector3.zero; //note: locked on xy plane
	Vector3 walkDirection;
	float getUpTimer = 0;
	//
	
	//references
	CharacterController cController;
	Rigidbody[] ragdollBodies;
	Collider[] colliders;
	Animator animator;
	//transition out of ragdoll stuff
	Transform transitionRigTransform = null;
	StoredTransform spineData;
	//
	public bool CanGetUp { get => canGetUp; set { canGetUp = value; getUpTimer = 0; } }
	bool canGetUp = false;
	bool ragdolling = false;
	//

    void Start()
    {
		Transform bodyTransform = spineTransform.parent.parent;
		animator = bodyTransform.GetComponent<Animator>();
		animator.enabled = true;
		cController = GetComponent<CharacterController>();
		cController.enabled = true;
		ragdollBodies = bodyTransform.GetComponentsInChildren<Rigidbody>();
		foreach (var body in ragdollBodies)
		{
			body.isKinematic = true;
		}
		colliders = bodyTransform.GetComponentsInChildren<Collider>();
		foreach (var collider in colliders)
		{
			collider.enabled = false;
		}
		
		walkTimer = Random.Range(behaviour.waitTime - behaviour.waitTimeVariance, behaviour.waitTime + behaviour.waitTimeVariance);
		
	}

    void Update()
    {
		//if transition rig != null, is transitioning. if transition out of ragdoll returns true, is done transitioning
		if (transitionRigTransform != null)
		{
			if (TransitionOutOfRagdoll())
			{
				transitionRigTransform = null;
				EnableRagdoll(false);
			}
		}
		else if (!ragdolling)
		{
			//get perpendicular to vector3.up facing in direction of mainForward
			Vector3 mainForward = spineTransform.forward;
			Vector3 facingDirection = Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;
			Vector3 rightDirection = new Vector3(-facingDirection.z, facingDirection.y, facingDirection.x);

			//Update animation
			animator.SetFloat("Speed", Vector3.Dot(facingDirection, velocity));
			animator.SetFloat("Strafe Speed", Vector3.Dot(rightDirection, velocity));

			walkTimer -= Time.deltaTime;
			if (walkTimer <= 0)
			{

			}
		}
		else if (CanGetUp && ragdollBodies.Length > 0 && ragdollBodies[0].velocity.sqrMagnitude < 0.02f)
		{
			//Ragdoll stuff
			getUpTimer += Time.deltaTime;
			if (getUpTimer >= behaviour.getUpTime)
			{
				if (Vector3.Dot(spineTransform.forward, Vector3.up) > 0)
					transitionRigTransform = GameManager.Instance.StandUpFacingUpSpine;
				else
					transitionRigTransform = GameManager.Instance.StandUpFacingDownSpine;

				if (transitionRigTransform == null || transitionRigTransform.childCount != spineTransform.childCount)
				{
					//no transition can be done
					transitionRigTransform = null;
					EnableRagdoll(false, true);
				}
				else
				{
					//teleport body to corrent position
					//this is just to make sure the transition to gameobject doesn't cause the body to move really far on the x and z axis

					
					Vector3 torsoPosition = spineTransform.position;
					Vector3 mainForward = (headTransform.position - torsoPosition).normalized;
					//get perpendicular to vector3.up facing in direction of mainForward
					Vector3 facingDirection = Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;

					if (Vector3.Dot(spineTransform.forward, Vector3.up) < 0)
						facingDirection = -facingDirection;

					StoredTransform spineData = new StoredTransform(spineTransform);
					transform.forward = facingDirection;
					transform.position = new Vector3(spineData.position.x, transform.position.y, spineData.position.z);
					spineTransform.position = spineData.position;
					spineTransform.rotation = spineData.rotation;

					//disable ragdolling
					foreach (var body in ragdollBodies)
					{
						body.isKinematic = true;
					}
					foreach (var collider in colliders)
					{
						collider.enabled = false;
					}
				}
			}
		}
	}

	bool TransitionOutOfRagdoll()
	{
		return TransitionOutOfRagdollRecursive(spineTransform, transitionRigTransform);
	}
	//relies on rig of current and target having the same hirarchy
	bool TransitionOutOfRagdollRecursive(Transform current, Transform target)
	{
		
		float currentDist = Mathf.Max((current.localPosition - target.localPosition).magnitude, 0.0001f);
		float currentAngle = Mathf.Clamp(Quaternion.Angle(current.localRotation, target.localRotation), 10, 40);
		current.localPosition = Vector3.MoveTowards(current.localPosition, target.localPosition, behaviour.getUpTransitionMoveSpeed * Time.deltaTime * currentDist);
		current.localRotation = Quaternion.RotateTowards(current.localRotation, target.localRotation, behaviour.getUpTransitionRotateSpeed * Time.deltaTime * currentAngle);

		bool done = (current.localPosition == target.localPosition) && (current.localRotation == target.localRotation);
		int childCount = current.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			done &= TransitionOutOfRagdollRecursive(current.GetChild(i), target.GetChild(i));
		}

		return done;
	}
	public bool IsRagdolling()
	{
		return ragdolling;
	}

	public Collider[] GetRagdollColliders()
	{
		return colliders;
	}

	public void EnableRagdoll(bool enable, bool instantEnable = false)
	{
		if (ragdolling == enable && !(instantEnable && enable && transitionRigTransform != null))
			return;

		ragdolling = enable;
		getUpTimer = 0;
		if (enable)
		{
			cController.enabled = false;
			foreach (var body in ragdollBodies)
			{
				body.isKinematic = false;
			}
			foreach (var collider in colliders)
			{
				collider.enabled = true;
			}
			animator.enabled = false;
		}
		else
		{
			
			if (instantEnable)
			{
				transitionRigTransform = null;

				foreach (var body in ragdollBodies)
				{
					body.isKinematic = true;
				}
				foreach (var collider in colliders)
				{
					collider.enabled = false;
				}

				Vector3 torsoPosition = spineTransform.position;
				Vector3 mainForward = (headTransform.position - torsoPosition).normalized;
				//get perpendicular to vector3.up facing in direction of mainForward
				Vector3 facingDirection = Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;

				if (Vector3.Dot(spineTransform.forward, Vector3.up) < 0)
					facingDirection = -facingDirection;

				transform.forward = facingDirection;
				transform.position = new Vector3(torsoPosition.x, transform.position.y, torsoPosition.z);
			}

			if (Vector3.Dot(spineTransform.forward, Vector3.up) > 0)
				animator.Play("Base Layer.Stand Facing Up", 0, 0);
			else
				animator.Play("Base Layer.Stand Facing Down", 0, 0);
			cController.enabled = true;
			animator.enabled = true;
		}
	}
}

struct StoredTransform
{
	public StoredTransform(Transform t)
	{
		position = t.position;
		rotation = t.rotation;
		localPosition = t.localPosition;
		localRotation = t.localRotation;
	}
	public Vector3 position;
	public Quaternion rotation;
	public Vector3 localPosition;
	public Quaternion localRotation;

}