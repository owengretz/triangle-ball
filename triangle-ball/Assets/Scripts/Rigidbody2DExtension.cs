using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Rigidbody2DExtension
{
    public static void AddExplosionForce(this Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
    {
        var dir = (body.transform.position - explosionPosition);
        float wearoff = 1 - (dir.magnitude / explosionRadius);
        if (wearoff > 0)
        {
            body.AddForce(dir.normalized * explosionForce * wearoff);
        }
    }
}
