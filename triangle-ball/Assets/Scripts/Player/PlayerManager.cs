using System;
using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public bool testing;
    [HideInInspector] public PlayerMovement m;
    public int playerNumber;
    public GameManager.Teams team;
    public int localPlayerNumber;
    public Material playerMat;
    public GameObject demoExplosionPrefab;
    public GameObject respawnEffectPrefab;
    [HideInInspector] public Animator anim;

    public bool isBot;

    [HideInInspector] public bool isDead;
    [HideInInspector] public bool canBeKilled = true; // mega powerup

    public event Action Die;
    public void TriggerDie() { Die?.Invoke(); }

    private void Start() { if (testing) Setup(playerNumber); }

    public void Setup(int playerNum)
    {
        m = GetComponent<PlayerMovement>();
        playerNumber = playerNum;
        m.playerNumber = playerNum;
        localPlayerNumber = Mathf.FloorToInt(playerNum / 2f);
        if (!testing) playerMat = GameManager.instance.playerMaterials[playerNum];
        m.ChangeBoostsAvailable((int)GameInfo.kickoffBoostAmount, true);
        if (!testing) anim = GetComponent<Animator>();
        //Color trailColour = playerMat.color
        //trail.GetComponent<Renderer>().material = playerMat;
        //ReplayManager.AddReplayObjectToRecordScenes(trail.GetComponent<ReplayObject>());

        if (!testing)
        {
            if (isBot && GetComponent<BotUtils>() == null) gameObject.AddComponent<BotUtils>();
        }

        Die += GetDemolished;
    }

    public void SetCanMove(bool canMove)
    {
        m.canMove = canMove;
    }

    public void GetDemolished()
    {
        if (!canBeKilled) return;

        GameManager.instance.ShakeCamera(GameInfo.cameraShakes[GameInfo.Shakes.Demo], 0.2f);

        GameObject demoExplosion = Instantiate(demoExplosionPrefab, transform.position, Quaternion.identity);
        ReplayManager.AddReplayObjectToRecordScenes(demoExplosion.GetComponent<ReplayObject>());


        anim.SetBool("Dead", true);
        SetCanMove(false);
        isDead = true;
        //gameObject.SetActive(false);

        // respawn
        Invoke("Respawn", GameInfo.respawnCooldown);
    }
    private void Respawn()
    {
        if (GameManager.instance.state != GameManager.State.Running/* || TimeManager.instance.elapsedTimeSinceKickoff < 4f*/)
            return;

        int respawnIndex = UnityEngine.Random.Range(0, 2);
        string respawnPosName = team + "Respawn" + respawnIndex;
        SetTransform(GameObject.Find(respawnPosName).transform);
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        anim.SetBool("Dead", false);
        SetCanMove(true);
        isDead = false;
        //gameObject.SetActive(true);
        m.ChangeBoostsAvailable(0, true);

        GameObject respawnEffect = Instantiate(respawnEffectPrefab, transform.position, Quaternion.identity);
        respawnEffect.GetComponentInChildren<Renderer>().material = playerMat;
        ReplayManager.AddReplayObjectToRecordScenes(respawnEffect.GetComponent<ReplayObject>());
    }

    //public bool IsSkippingReplay()
    //{
    //    return m.IsPressingAnyControl();
    //}


    public void GetBumped(Vector2 force)
    {
        if (!canBeKilled) return;

        //GetComponent<Rigidbody2D>().AddForce(direction * force, ForceMode2D.Impulse);
        GetComponent<Rigidbody2D>().velocity += force;
    }

    public void SetTransform(Transform trans)
    {
        transform.SetPositionAndRotation(trans.position, trans.rotation);
    }
    public void SetTransform(Vector2 position, float rotation)
    {
        transform.position = position;
        GetComponent<Rigidbody2D>().rotation = rotation;
    }


    private void OnDisable()
    {
        if (!testing) GetComponent<Animator>().enabled = false;
    }

    private void OnEnable()
    {
        if (!testing) GetComponent<Animator>().enabled = true;
    }
}
