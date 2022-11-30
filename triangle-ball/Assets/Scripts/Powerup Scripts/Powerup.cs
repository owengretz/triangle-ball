using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    //public float duration = 8f;
    public bool endPowerup;

    public virtual void UsePowerup()
    {
        GetComponent<PlayerManager>().Die += Die;
    }

    public virtual void EndPowerup()
    {
        GetComponent<PlayerManager>().Die -= Die;
    }

    public virtual void Die()
    {
        endPowerup = true;
        EndPowerup();
    }
}
