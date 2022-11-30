using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningParticle : MonoBehaviour
{
    private readonly float spinSpeed = 360f;

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}
