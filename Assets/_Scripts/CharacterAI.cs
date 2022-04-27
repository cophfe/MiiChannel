using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : MonoBehaviour
{
	[SerializeField] AIBehaviour behaviour;
	[SerializeField] Transform spineTransform;
	[SerializeField] Transform headTransform;
	//ai
	enum State
	{
		Wandering,
		WanderingContinuously,
		Turning,
		Waiting
	}
	State state = State.Waiting;
	
	float waitTimer = 0;
	float currentSpeed = 0;
	float currentRotateSpeed = 0;
	Vector2 currentDirection;
	Vector2 target;
	float getUpTimer = 0;

	int speedHash;
	//
	
	//references
	CharacterController cController;
	Rigidbody[] ragdollBodies;
	Collider[] colliders;
	Animator animator;
	//transition out of ragdoll stuff
	[System.NonSerialized] public Transform transitionRigTransform = null;
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
		speedHash = Animator.StringToHash("Speed");
		foreach (var body in ragdollBodies)
		{
			body.isKinematic = true;
		}
		colliders = bodyTransform.GetComponentsInChildren<Collider>();
		foreach (var collider in colliders)
		{
			collider.enabled = false;
		}

		this.currentDirection = GetXZVec2(transform.forward);
		ChooseMove();
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
			//bandaid solution
			if (currentDirection.sqrMagnitude < 0.9f)
				currentDirection = GetXZVec2(transform.forward);

			switch (state)
			{
				case State.Turning:
					{
						currentDirection = Vector3.RotateTowards(currentDirection, target, currentRotateSpeed * Time.deltaTime, 0);
						transform.forward = GetVec3FromXZ(currentDirection);
						if (Vector2.Dot(currentDirection, target) > 0.98f)
						{
							currentRotateSpeed = 0;
							ChooseMove();
						}
					}
					break;

				case State.Waiting:
					waitTimer -= Time.deltaTime;
					if (waitTimer <= 0)
					{
						ChooseMove();
					}
					break;

				default:
					Wander();
					break;
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

	void ChooseMove()
	{
		float percent = Random.value;
		if (percent < behaviour.wanderChance)
		{
			if (Random.value < behaviour.wanderContinuouslyChance)
				state = State.WanderingContinuously;
			else
				state = State.Wandering;

			//choose direction and distance to walk to
			Vector2 randomDirection = Random.insideUnitCircle;
			float distance = Random.Range(behaviour.wanderMinDistance, behaviour.wanderMaxDistance);
			//calculate target position, restrict it to within the movement bounds
			target = ConstrictToBounds(GetXZVec2(transform.position) + randomDirection * distance);
		}
		else if (percent < behaviour.wanderChance + behaviour.playRandomAnimationChance)
		{
			int anIndex = behaviour.GetRandomAnimationIndex();
			if (anIndex > 0)
			{
				animator.SetTrigger("DoAnimation");
				animator.SetInteger("AnimationIndex", anIndex);
				state = State.Waiting;
				waitTimer = behaviour.randomAnimations[anIndex].time;
			}
		}
		else if (percent < behaviour.wanderChance + behaviour.playRandomAnimationChance + behaviour.turnChance)
		{
			currentRotateSpeed = Mathf.Deg2Rad * 90.0f / behaviour.turnTime;
			state = State.Turning;

			//turn left
			if (Random.value > 0.5f)
			{
				animator.SetTrigger("TurnLeft");
				target = Vector2.Perpendicular(currentDirection);
			}
			//turn right
			else
			{
				animator.SetTrigger("TurnRight");
				target = -Vector2.Perpendicular(currentDirection);
			}
		}
		else
		{
			state = State.Waiting;
			waitTimer = Random.Range(behaviour.waitTime - behaviour.waitTimeVariance, behaviour.waitTime + behaviour.waitTimeVariance);
		}
	}

	void Wander()
	{
		Vector3 targetDelta = GetVec3FromXZ(target) - transform.position;
		float distance = targetDelta.magnitude;

		if (distance < 0.13f)
		{
			if (state == State.Wandering)
			{
				//should already be at zero, but just in case.
				currentSpeed = 0;
				currentRotateSpeed = 0;
				//finished walking.
				waitTimer = Random.Range(behaviour.wanderWaitMin, behaviour.wanderWaitMax);
				state = State.Waiting;
				return;
			}
			else
			{
				currentRotateSpeed = 0;

				if (Random.value < behaviour.wanderContinuouslyChance)
					state = State.WanderingContinuously;
				else
					state = State.Wandering;

				//choose direction and distance to walk to
				Vector2 randomDirection = Random.insideUnitCircle;
				float wanderDistance = Random.Range(behaviour.wanderMinDistance, behaviour.wanderMaxDistance);
				//calculate target position, restrict it to within the movement bounds
				target = ConstrictToBounds(GetXZVec2(transform.position) + randomDirection * wanderDistance);
				return;
			}
		}
		else
		{
			Vector2 targetDir = GetXZVec2(targetDelta / distance);

			float moveSlowDown = 1;
			if (state == State.Wandering)
			{
				moveSlowDown = 1 - Mathf.Clamp01((distance - 0.1f) / behaviour.slowDownDistance);
				//apply exponential easing
				moveSlowDown = 1 - (moveSlowDown == 0 ? 0 : Mathf.Pow(2, 10 * moveSlowDown - 10));
			}

			float rotateSlowDown = Mathf.Clamp(Vector2.Angle(currentDirection, targetDir)/30,0,1);
			currentRotateSpeed = Mathf.Min(currentRotateSpeed + behaviour.rotateAcceleration * Time.deltaTime, behaviour.rotationSpeed);
			//im pretty sure using the vector3.rotatetowards here wont change anything (no rotate funcitons exist for vector2s so this is easier)
			currentDirection = Vector3.RotateTowards(currentDirection, targetDir, rotateSlowDown * currentRotateSpeed * Time.deltaTime, 0);

			currentSpeed = Mathf.Min(currentSpeed + behaviour.acceleration * Time.deltaTime, behaviour.speed);
			transform.forward = GetVec3FromXZ(currentDirection);
			transform.position += GetVec3FromXZ(currentDirection * moveSlowDown * currentSpeed * Time.deltaTime);

			//Update animation
			Vector3 mainForward = spineTransform.forward;
			Vector3 facingDirection = -Vector3.Cross(Vector3.Cross(mainForward, Vector3.up), Vector3.up).normalized;
			Vector3 rightDirection = new Vector3(-facingDirection.z, facingDirection.y, facingDirection.x);

			animator.SetFloat(speedHash, currentSpeed * behaviour.animationWalkSpeed * moveSlowDown);
		}
	}

	Vector2 GetXZVec2(Vector3 vector)
	{
		return new Vector2(vector.x, vector.z);
	}

	Vector3 GetVec3FromXZ(Vector2 vector, float centreValue = 0)
	{
		return new Vector3(vector.x, centreValue, vector.y);
	}

	//Vector2 GetRandomValidPosition()
	//{
	//	//float radius = cController.radius;
	//	//float x = Random.Range(GameManager.Instance.BoundsMin.x + radius, GameManager.Instance.BoundsMax.x - radius);
	//	//float y = Random.Range(GameManager.Instance.BoundsMin.y + radius, GameManager.Instance.BoundsMax.y - radius);
	//	//return new Vector2(x, y);
	//}

	Vector2 ConstrictToBounds(Vector2 boundsPosition)
	{
		float radius = cController.radius;
		boundsPosition.x = Mathf.Clamp(boundsPosition.x, GameManager.Instance.BoundsMin.x + radius, GameManager.Instance.BoundsMax.x - radius);
		boundsPosition.y = Mathf.Clamp(boundsPosition.y, GameManager.Instance.BoundsMin.y + radius, GameManager.Instance.BoundsMax.y - radius);
		return boundsPosition;
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
			transitionRigTransform = null;
			state = State.Waiting;

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
			state = State.Waiting;

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
			{
				animator.Play("Base Layer.Stand Facing Up", 0, 0);
				waitTimer = behaviour.getUpFromBackWaitTime;
			}
			else
			{
				animator.Play("Base Layer.Stand Facing Down", 0, 0);
				waitTimer = behaviour.getUpFromFaceWaitTime;
			}
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