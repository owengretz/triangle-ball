using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGEColour : MonoBehaviour
{
    /// <summary>
    /// sets the colour of goal explosion and is ignored by the replay manager so that the colour shows in the replay
    /// </summary>

    public static Color colour;

    private void Start()
    {
        var main = GetComponent<ParticleSystem>().main;

        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, colour);
    }
}
