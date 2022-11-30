using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Custom Gamemode", menuName = "Custom Gamemode")]
public class CustomGamemode : ScriptableObject
{
    public string modeName;

    public bool plungerEnabled;

    [ConditionalHide("plungerEnabled", true)]
    public float plungerDuration;
    [ConditionalHide("plungerEnabled", true)]
    public float plungerRange;
    [ConditionalHide("plungerEnabled", true)]
    public float plungerForce;
    [ConditionalHide("plungerEnabled", true)]
    public float plungerLatchSpeed;


    public bool spikesEnabled;

    [ConditionalHide("spikesEnabled", true)]
    public float spikesDuration;
    [ConditionalHide("spikesEnabled", true)]
    public float spikesReleaseSpeed;


    public bool grappleEnabled;

    [ConditionalHide("grappleEnabled", true)]
    public float grappleLatchSpeed;
    [ConditionalHide("grappleEnabled", true)]
    public float grappleGrappleSpeed;


    public bool pentagonEnabled;

    [ConditionalHide("pentagonEnabled", true)]
    public float pentagonDuration;


    public bool clonesEnabled;

    [ConditionalHide("clonesEnabled", true)]
    public float clonesDuration;
    [ConditionalHide("clonesEnabled", true)]
    public float clonesAmount;


    public bool bumperEnabled;

    [ConditionalHide("bumperEnabled", true)]
    public float bumperDuration;
    [ConditionalHide("bumperEnabled", true)]
    public float bumperLaunchSpeed;
    [ConditionalHide("bumperEnabled", true)]
    public float bumperBumpForce;


    public bool teleportEnabled;

    [ConditionalHide("teleportEnabled", true)]
    public float teleportDelay;


    public bool megaEnabled;

    [ConditionalHide("megaEnabled", true)]
    public float megaDuration;
    [ConditionalHide("megaEnabled", true)]
    public float megaMassIncrease;


    public bool hijackEnabled;

    [ConditionalHide("hijackEnabled", true)]
    public float hijackDuration;
    [ConditionalHide("hijackEnabled", true)]
    public float hijackMoveForce;


     

    [SerializeField] private bool gameValues;

    [ConditionalHide("gameValues", true)]
    public float powerupCooldown;
    [ConditionalHide("gameValues", true)]
    public float respawnCooldown;
    [ConditionalHide("gameValues", true)]
    public float boostRespawnTime;
    [ConditionalHide("gameValues", true)]
    public float kickoffBoostAmount;



    [SerializeField] private bool physics;

    [ConditionalHide("physics", true)]
    public float playerThrustForce;
    [ConditionalHide("physics", true)]
    public float playerBoostForce;
    [ConditionalHide("physics", true)]
    public float playerTurnSpeed;
    [ConditionalHide("physics", true)]
    public float playerMass;
    [ConditionalHide("physics", true)]
    public float playerDrag;
    [ConditionalHide("physics", true)]
    public float playerDemolishThreshold;
    [ConditionalHide("physics", true)]
    public float playerBumpForce;

    [ConditionalHide("physics", true)]
    public float ballMass;
    [ConditionalHide("physics", true)]
    public float ballDrag;
    [ConditionalHide("physics", true)]
    public float ballBounciness;
}
