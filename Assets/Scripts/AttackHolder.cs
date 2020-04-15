using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AttackHolder : MonoBehaviour	//This class defines the types of attacks we can use
{
	[SerializeField]
	private string _name;	//The name of the attack
	public int plusToHit;	//The number we add to the hit dice to overcome the enemy armor
	public DiceRoll attackRoll;	//Defines what dice we roll for damage if we hit
	public GameObject attackFX;	//The object that contains particles and sounds to spawn in if we hit

	[Header("Object References")]
	public Button myButton;	//The attached button
	[SerializeField]
	private TextMeshProUGUI attackNameText, plusToHitText, damageText;	//The attached text elements we manage

	private void Start()	//We set the text objects to reflect the values of the attack
	{
		attackNameText.text = _name;	//Sets the name
		plusToHitText.text = plusToHit >= 0 ? "+" + plusToHit.ToString() : plusToHit.ToString();	//Sets the bonus to hit dice. This will show a "+" in front of the number if it is positive.
		damageText.text = attackRoll.amount.ToString() + attackRoll.dice.ToString();	//Sets the damage
	}

	public void AttackPressed()	//This is called from the attached UI button when we press it
	{
		GameController.control.DoAttack(this);	//Tells the GameController that we selected this attack
	}
    
}