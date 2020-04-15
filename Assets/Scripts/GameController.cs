using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;


public class GameController : MonoBehaviour	//This class is a Singleton class that ties all the game functions together
{
	public static GameController control;	//A static reference to this class

	[SerializeField]
	private List<GameObject> enemies, attacks;	//All the enemies and attacks in the game. They will only be included if put in these lists
	[SerializeField]
	private float enemySpawnDelay;	//The time we wait between spawning enemies
	public float maxPitchVariance;	//The max pitch variance of sound effects. It is public so we can reuse it in other scripts

	[Header("Object References")]
	[SerializeField]
	private Transform enemySpawnPos;	//The enemy spawning location
	[SerializeField]
	private GameObject enemyUIParent, attackParent, damageText, damageTextSpawn;	//The various UI objects we manage
	[SerializeField]
	private TextMeshProUGUI enemyTaunt, armorClassText, healthText;	//The various UI Texts we manage
	[SerializeField]
	private DiceRoller roller;	//The script that governs dice rolls
	[SerializeField]
	private AudioSource hitSound, missSound;	//The sounds we play when we hit and miss respectively

	private Enemy currentEnemy;	//The current spawned enemy
	private AttackHolder currentAttack;	//The attack we are currently using
	private List<AttackHolder> activeAttacks;	//All the UI objects containing the attack info

	private void Awake()
	{
		if (DoSingletonCheck())
		{
			Invoke("SpawnEnemy", enemySpawnDelay);	//If we pass the singleton check we spawn the first enemy
			PopulateAttackList();	//Populate the UI list of available attacks
		}
	}
	bool DoSingletonCheck() //This logic checks if this is the only copy of this class in existance. It also returns a bool so we can do some logic only after this returns true, though we're not currently implementing that
	{
		if (control != this)    //Is this object *already* the static reference?
		{
			if (control)    //If not, then is there *another* static reference?
			{
				Destroy(gameObject);    //If yes, we don't want this one
				return false;
			}
			else
			{
				control = this; //If else, make this the new static reference

				//We could make this object persist between scenes here if we needed it to:
				//DontDestroyOnLoad(this);

				return true;
			}
		}
		else
		{
			return true;
		}
	}

	private void PopulateAttackList()	//This function fills in the list of available attacks
	{
		activeAttacks = new List<AttackHolder>();	//Initialize the list
		for (int i = 0; i < attacks.Count; i++)
		{
			GameObject newAttack = Instantiate(attacks[i], attackParent.transform);	//We instantiate the attack prefabs. Their scripts will do the setup for the text elements
			activeAttacks.Add(newAttack.GetComponent<AttackHolder>());	//Add the newly created attack script to the list of attack scripts. We do this so we can enable/disable the buttons
		}
	}

   private void SpawnEnemy()	//Spawns an enemy from the right
    {
		GameObject newEnemy = Instantiate(enemies[Random.Range(0, enemies.Count)], enemySpawnPos.position, Quaternion.identity);	//Intantiate a random enemy selected from the enemies list
		currentEnemy = newEnemy.GetComponent<Enemy>();	//Retrieve the enemy script
		LetPlayerAttack(true);	//Enable the attack buttons
    }

	public void LetPlayerAttack(bool state) //This function enables or disables the attack option
	{
		for (int i = 0; i < activeAttacks.Count; i++)
		{
			activeAttacks[i].myButton.interactable = state;
		}
	}
	public void ChangeEnemyUI(string taunt, int AC, int health) //This function is called from the enemy script and updates the UI
	{
		enemyUIParent.SetActive(true);	//Show the enemy UI
		enemyTaunt.text = taunt;	//Set the taunt text
		armorClassText.text = AC.ToString();	//Set the armor value
		healthText.text = health.ToString();	//Set the health value
	}

	public void UpdateEnemyHealthUI(int amount)	//Called from the enemy script when it is damaged
	{
		healthText.rectTransform.DOShakePosition(0.2f);	//Shake the text to draw attention to it
		healthText.text = amount.ToString();	//Update the health value
	}

	public void DoAttack(AttackHolder newAttack) //Called from the AttackHolder class when we press a button to attack
	{
		LetPlayerAttack(false);	//Disable attacking until we complete the attack

		currentAttack = newAttack;	//Store the current attack in a variable

		DiceRoll hitDice = new DiceRoll	//We check to hit with a dice roll that consists of one D20 dice 
		{
			amount = 1,
			dice = DiceType.D20
		};

		roller.DoDiceroll(hitDice, CheckIfAttackLanded);	//We tell the diceroller to roll the dice we defined above, then pass the result to the CheckIfAttackLanded function
	}

	private void CheckIfAttackLanded(int roll) //Called from the DiceRoller script when we try to do an attack
	{
		if(currentAttack.plusToHit + roll > currentEnemy.armorClass)	//If our roll plus our hit bonus is higher than the enemy armor, we roll damage
		{
			DisplayDamageText("Hit!");	//We display a rising text with the word "hit"
			hitSound.pitch = 1f + Random.Range(-maxPitchVariance, maxPitchVariance);	//Randomize the sound pitch based on the maxPitchVariance variable
			hitSound.Play();	//Play the hit sound
			Invoke("DoDamageRoll", 1f);	//We invoke the function to do the actual damage roll in one second
		}
		else	//If our roll is too low we miss
		{
			DisplayDamageText("Miss!"); //We display a rising text with the word "miss"
			missSound.pitch = 1f + Random.Range(-maxPitchVariance, maxPitchVariance);   //Randomize the sound pitch based on the maxPitchVariance variable
			missSound.Play();   //Play the miss sound
			LetPlayerAttack(true);	//Enable attacking again
		}
	}

	private void DisplayDamageText(string content)	//This function spawns a rising text object with the requested text
	{
		GameObject newText = Instantiate(damageText, damageTextSpawn.transform.position, Quaternion.identity, damageTextSpawn.transform);	//Instantiate the text object
		newText.GetComponent<TextMeshProUGUI>().text = content;	//Set the text to the requested string
		Tweener t = newText.transform.DOMoveY(newText.transform.position.y + 150f, 2f);	//Tell the object to move up over 2 seconds
		t.SetEase(Ease.OutElastic);	//We set the ease type to elastic to get a springy, dynamic look
		Destroy(newText, 3f);	//After 3 seconds we remove the text
	}

	private void DoDamageRoll()	//Called when we hit with the hit roll and are going to actually roll damage
	{
		roller.DoDiceroll(currentAttack.attackRoll, DamageEnemy);	//Tell the DiceRoller class to do the roll defined in the attack script we are currently attacking with, and return the result to the DamageEnemy function
	}

	private void DamageEnemy(int amount)	//We damage the current enemy
	{
		DisplayDamageText(amount.ToString());	//Display the damage we did as a rising text
		LetPlayerAttack(true);	//Reenable the attacking
		currentEnemy.Damage(amount);	//Tell the enemy script that we damaged it and by how much

		if (currentAttack.attackFX)	//If there is a FX object defined, we display it for a few seconds
		{
			GameObject newFX = Instantiate(currentAttack.attackFX);
			Destroy(newFX, 3f);
		}
	}

	public void EnemyDied(float delay)	//This is called by the enemy script when the enemy dies. It tells us how long it intends to use to die
	{
		LetPlayerAttack(false);	//We make sure we can't attack until a new enemy is spawned in
		Invoke("DisableEnemyUI", delay);	//Hide the enemy UI after the defined waiting time
		Invoke("SpawnEnemy", delay + enemySpawnDelay);	//Spawn the next enemy after the last one has gone and then some
	}

	private void DisableEnemyUI()	//Removes the enemy UI
	{
		enemyUIParent.SetActive(false);
	}
}

#region Global Variables and Structs

public enum DiceType { D4, D6, D8, D10, D12, D20 }; //An enum containing our different dice types
public delegate void AfterRollAction(int rollResult);   //A delegate that describes the type of function we want to call after rolling a dice

[System.Serializable]
public struct DiceRoll	//A struct that can define a roll of N number of any single type of alike dice
{
	public DiceType dice;
	public int amount;
}

[System.Serializable]
public struct DiceTransformValue	//A struct that is used by the Dice script to define which face of the dice corresponds to what value
{
	public Transform face;
	public int value;
}

[System.Serializable]
public struct DiceObjectType	//A struct that pairs off a prefab with a specific type of dice 
{
	public GameObject dicePrefab;
	public DiceType type;
}

#endregion