using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour	//This class lives on the dice object and communicates with the DiceRoller
{
	[SerializeField]
	private List<DiceTransformValue> diceTransforms;	//This dice faces and corresponding values

	private Rigidbody rb;	//The attached RigidBody
	private bool settled;	//Has the dice settled?
	[HideInInspector]
	public DiceRoller owner;	//The DiceRoller that we report back to

	private void Awake()
	{
		rb = GetComponent<Rigidbody>();	//Assign the attached RigidBody
	}

	private void FixedUpdate()
	{
		if (rb.IsSleeping() && !settled)	//If the RB has gone to sleep and we are not yet settled, it means we have just settled
		{
			owner.ResultIn(EvaluateDice());	//Tells the DiceRoller that spawned us what the result was
			settled = true;	//We are now settled, so we only send one result to the owner script
		}
	}

	private int EvaluateDice() //This function evaluates what value the dice landed on. It checks all faces to see which one is at the top
	{
		DiceTransformValue upper = new DiceTransformValue();	//The upmost of the checked faces
		upper.value = 0;

		for (int i = 0; i < diceTransforms.Count; i++)	//Loops over all the faces on the dice
		{
			if (upper.face != null)	//If we have a face to compare it to
			{
				if (diceTransforms[i].face.position.y > upper.face.position.y)	//Are we above the compared face?
				{
					upper = diceTransforms[i];	//If so, we are the new upmost face
				}
			}
			else	//If there is no face to check against, we default to this being the new upmost face
			{
				upper = diceTransforms[i];
			}
		}

		return upper.value;	//Returns the upmost face after having checked all faces
	}

}
