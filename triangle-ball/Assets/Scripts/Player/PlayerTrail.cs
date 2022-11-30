using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class PlayerTrail : ReplayBehaviour
{
    private ParticleSystem trail;

    private float velocity;
    private Vector3 posLastFrame;

    [ReplayVar]
    private bool showTrail;

    private void Start()
    {
        trail = GetComponent<ParticleSystem>();

        posLastFrame = Vector3.zero;
    }

    private void FixedUpdate()
    {
        velocity = transform.InverseTransformDirection((transform.position - posLastFrame) / Time.fixedDeltaTime).y;

        showTrail = velocity > 6.5f;

        if (showTrail && !trail.isPlaying) trail.Play();
        else if (!showTrail && trail.isPlaying) trail.Stop();

        posLastFrame = transform.position;
    }
}
