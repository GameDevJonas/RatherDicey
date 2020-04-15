using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour	//This class sits on an enemy object and contains the enemies stats
{
	public int hitPoints, armorClass;	//The starting hit points and natural armor
	[Multiline]
	[SerializeField]
	private List<string> taunts;	//The taunts are displayed by the GameController in the UI when this enemy is spawned
	[SerializeField]
	private float slideInTime, deathDelay;	//How fast we slide onto screen and how long we linger after dead
	[SerializeField]
	private GameObject deathFX;	//The object with particles/sounds that spawns when we die
    void Start()
    {
		Tweener t = transform.DOMove(Vector3.zero, slideInTime); //Moves the enemy to the center screen
		t.SetEase(Ease.OutBounce);	//Makes the enemy spring into place
		Invoke("UpdateUI", slideInTime);	//When we arrive at the center invoke UpdateUI
    }

	private void UpdateUI() 
	{
		GameController.control.ChangeEnemyUI(taunts[Random.Range(0, taunts.Count)], armorClass, hitPoints);	//We tell the gamecontroller to update the UI with our stats
	}

	public void Damage(int amount) //Called from the GameController when we take damage
	{
		hitPoints -= amount;	//Subtract the damaged amount from our hitpoints
		GameController.control.UpdateEnemyHealthUI(hitPoints);	//Tell the GameController to reflect the new HP value in the UI
		if(hitPoints <= 0)	//Kill us if we are below 0 health after the attack
		{
			Kill();
		}
	}

	private void Kill() 
	{
		GameController.control.EnemyDied(deathDelay);	//We tell the GameController that we died and how long we intend to linger before it can spawn a new enemy
		Invoke("Die", deathDelay);	//We waint a while to die so the player gets time to process what happened
	}

	private void Die() 
	{
		if (deathFX)	//If we have defined a particle system / sounds object we show it for a few seconds
		{
			GameObject newFX = Instantiate(deathFX);
			Destroy(newFX, 2f);
		}

		Destroy(gameObject);	//Destroys this enemy
	}
}
