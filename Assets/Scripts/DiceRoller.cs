using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;	//We use LINQ to search through a custom dictionary structure for our dice rolls 

public class DiceRoller : MonoBehaviour	//This class manages the dice rolls
{
	[Header("Object References")]
	[SerializeField]
	private List<DiceObjectType> dice;	//References to a struct that holds our dice prefabs and their corresponding dice types
	[SerializeField]
	private AudioSource diceSound;	//Sound to play when we spawn in dice
	private int waitingResult, totalResult;     //The number of dice which are still rolling and the total rolled number
	
	//This function is called from the GameController
	public void DoDiceroll(DiceRoll roll, AfterRollAction next)	//The DiceRoll describes this roll. AfterRollAction holds the function we want to call after this roll has completed.
    {
		StartCoroutine(SpawnDice(roll, next));	//This starts the coroutine below. We do it like this because you cannot start a coroutine in another class directly
	}

	private IEnumerator SpawnDice(DiceRoll rollToSpawn, AfterRollAction nextAction)
	{
		List<GameObject> currentDice = new List<GameObject>();	//A list of the dice we are about to spawn so we can keep track of them
		waitingResult = 0;	//The number of dice which have yet to settle
		totalResult = 0;	//The total number we roll

		diceSound.pitch = 1f + Random.Range(-GameController.control.maxPitchVariance, GameController.control.maxPitchVariance);	//We ranomize the pitch based on the maxPitchVariance variable in the GameController
		diceSound.Play();	//Play the dice sound

		GameObject diceToSpawn = (dice.SingleOrDefault(toCheck => toCheck.type == rollToSpawn.dice)).dicePrefab;	//This line uses LINQ to get the appropriate dice from the list

		for (int i = 0; i < rollToSpawn.amount; i++)	//We loop for each dice in the roll
		{
			GameObject newDice = Instantiate(diceToSpawn, transform.position, Random.rotation);	//Spawn a new dice of the type we determined above. The random rotation is what makes the roll random
			newDice.GetComponent<Dice>().owner = this;	//We tell the new dice that it should report its result to this script
			currentDice.Add(newDice);	//We add the newly created dice to the list that keeps track of the dice this roll
			waitingResult++;	//Increment the number of dice for which results are yet to come in
			yield return new WaitForSeconds(0.2f);	//Wait a bit before continuing so that dice don't spawn inside one another
		}

		while(waitingResult > 0)	//As long as there are still dice that haven't settled we yield
		{
			yield return new WaitForEndOfFrame();
		}

		nextAction(totalResult);	//When all dice have settled we return to the GameController by calling the function we are told to do next

		yield return new WaitForSeconds(1f);	//We wait a second before destroying the dice objects

		for (int i = 0; i < currentDice.Count; i++)	//Destroys all the dice from this roll
		{
			Destroy(currentDice[i]);
		}
	}

	public void ResultIn(int value) //This function is called from each individual dice. It tells us that it has settled on a value
	{
		totalResult += value;	//We add the value we settled on to the total
		waitingResult--;	//We decrement the amount of dice which are yet to settle
	}
}