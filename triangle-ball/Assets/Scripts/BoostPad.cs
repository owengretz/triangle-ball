using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    //private readonly float respawnTime = 6f;
    [HideInInspector] public bool up;
    private Animator anim;

    private void Start()
    {
        //up = true;
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        up = true;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!up || !collision.CompareTag("Player"))
            return;

        if (!collision.GetComponent<PlayerMovement>().PickupBoost())
            return;

        ChangeState(false);
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameInfo.boostRespawnTime);
        ChangeState(true);
    }

    public void ChangeState(bool setToUp)
    {
        anim.SetBool("Up", setToUp);
        up = setToUp;
    }
}
