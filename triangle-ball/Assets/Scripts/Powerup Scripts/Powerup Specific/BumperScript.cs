using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class BumperScript : MonoBehaviour
{
    [HideInInspector] public float duration;
    //private readonly float bumpForce = 20f;
    private Animator anim;

    private IEnumerator Start()
    {
        anim = GetComponent<Animator>();

        yield return new WaitForSeconds(duration);
        ReplayManager.RemoveReplayObjectFromRecordScenes(GetComponent<ReplayObject>());
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (rb == null) return;
        

        anim.SetTrigger("Hit");
        Vector2 dir = (collision.gameObject.transform.position - transform.position).normalized;

        float force = collision.gameObject.CompareTag("Ball") ? GameInfo.bumperBumpForce / 3f : GameInfo.bumperBumpForce;


        float shakeIntensity = Mathf.Clamp(force / 10f, 0f, 2f);
        if (collision.gameObject.GetComponent<BumperScript>() != null)
            shakeIntensity /= 3f;
        GameManager.instance.ShakeCamera(shakeIntensity, 0.2f);

        rb.AddForce(dir * force, ForceMode2D.Impulse);
    }
}
