using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Behaviour", menuName = "ScriptableObjects/Character Behaviour", order = 1)]
public class AIBehaviour : ScriptableObject
{
	public float speed = 2;
	public float acceleration = 2;
	public float walkDistance = 15;
	public float walkDistanceVarience = 5;
	[Range(0,1)] public float walkChance = 0.4f;
	public float waitTime = 6;
	public float waitTimeVariance = 4;
	public float getUpTime = 1;
	public float getUpTransitionMoveSpeed = 0.3f;
	public float getUpTransitionRotateSpeed = 2.0f;

}
