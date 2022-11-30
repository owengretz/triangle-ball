using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikesBallScript : MonoBehaviour
{
    public int playerNumber;
    public bool firstTouch = false;
    public bool hitOff = false;


    // tells spikesPowerup script when to stick the ball or when it gets hit off by another player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        if (firstTouch == false) firstTouch = collision.gameObject.GetComponent<PlayerManager>().playerNumber == playerNumber;
        else hitOff = collision.gameObject.GetComponent<PlayerManager>().playerNumber != playerNumber;
    }
}
