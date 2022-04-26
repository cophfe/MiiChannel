using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Behaviour", menuName = "ScriptableObjects/Character Behaviour", order = 1)]
public class AIBehaviour : ScriptableObject
{
	public float speed = 2;
	public float acceleration = 2;
	public float rotateAcceleration = 2;
	public float rotationSpeed = 10;
	public float slowDownDistance = 1;
	public float wanderMaxDistance = 10;
	public float wanderMinDistance = 2;
	public float wanderWaitMin = 0.5f;
	public float wanderWaitMax = 1.5f;
	public float animationWalkSpeed = 0.5f;
	[Range(0,1)] public float wanderChance = 0.4f;
	[Range(0,1)] public float playRandomAnimationChance = 0.2f;
	[Range(0,1)] public float turnChance = 0.2f;
	[Range(0,1)] public float wanderContinuouslyChance = 0.5f;
	public float waitTime = 6;
	public float waitTimeVariance = 4;
	public float getUpWaitTime = 1.5f;
	public float getUpTime = 1;
	public float getUpTransitionMoveSpeed = 0.3f;
	public float getUpTransitionRotateSpeed = 2.0f;

	public List<AnimationInformation> randomAnimations;
	float inverseTotalWeight;

	private void OnEnable()
	{
		OnValidate();	
	}

	private void OnValidate()
	{
		if (wanderWaitMax < wanderWaitMin)
			wanderWaitMax = wanderWaitMin;

		if (wanderMaxDistance < wanderMinDistance)
			wanderMaxDistance = wanderMinDistance;

		if (wanderChance + playRandomAnimationChance > 1)
		{
			playRandomAnimationChance = 1 - wanderChance;
			turnChance = 0;
		}
		else if (wanderChance + playRandomAnimationChance + turnChance > 1)
		{
			turnChance = 1 - (wanderChance + playRandomAnimationChance);
		}

		if (randomAnimations != null)
		{
			float tW = 0;
			foreach (var animation in randomAnimations)
			{
				tW += animation.weight;
			}
			inverseTotalWeight = tW == 0 ? 0 : 1 / tW;
		}
	}

	public int GetRandomAnimationIndex()
	{
		float percent = Random.value;

		//qwik performance
		if (percent > 0.5f)
		{
			float currentPercent = 1;

			for (int i = randomAnimations.Count - 1; i >= 0; i--)
			{
				if (currentPercent < percent)
					return i;
				currentPercent -= randomAnimations[i].weight * inverseTotalWeight;
			}
		}
		else
		{
			float currentPercent = 0;
			
			for (int i = 0; i < randomAnimations.Count; i++)
			{
				currentPercent += randomAnimations[i].weight * inverseTotalWeight;
				if (currentPercent > percent)
					return i;
			}
		}
		return -1;
	}

	[System.Serializable]
	public struct AnimationInformation
	{
		public string name;
		public float weight;
		public float time;
	}
}
