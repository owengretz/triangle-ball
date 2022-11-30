using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;
using static UnityEngine.ParticleSystem;
//using EZCameraShake;

public class BallManager : MonoBehaviour
{
    public GameObject goalExplosionPrefab;

    public float[] timeOfLastTouches = new float[3] { 99f, 99f, 99f }; // blue, red, green
    public GameManager.Teams[] colourOfLastTouches = new GameManager.Teams[2] { GameManager.Teams.None, GameManager.Teams.None }; // most recent, 2nd most recent

    [HideInInspector] public Rigidbody2D rb;

    [HideInInspector] public Vector2 velocity;

    

    
    

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.mass = GameInfo.ballMass;
        rb.drag = GameInfo.ballDrag;

        
        //Instantiate(eyeballSkin, transform).GetComponent<EyeballBall>().ballTrans = transform;
        //Instantiate(colourBallSkin, transform).GetComponent<ColourBall>().ballManager = this;
        //Instantiate(timerBallSkin, transform);
    }

    private IEnumerator BallBounciness()
    {
        yield return new WaitForFixedUpdate();
        
        if (rb != null)
            rb.velocity *= GameInfo.ballBounciness;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (TimeManager.instance.clockValue < 1f && collision.gameObject.CompareTag("Wall"))
        {
            GameManager.instance.OnReplayEnd();
            return;
        }

        if (!collision.gameObject.CompareTag("Player"))
            return;

        StartCoroutine(BallBounciness());

        GameManager.Teams team = collision.gameObject.GetComponent<PlayerManager>().team;

        if (team != colourOfLastTouches[0])
        {
            colourOfLastTouches[1] = colourOfLastTouches[0];
            colourOfLastTouches[0] = team;
        }

        switch (team)
        {
            case GameManager.Teams.Blue: 
                timeOfLastTouches[0] = TimeManager.instance.ElapsedTimeSinceKickoff; break;
            case GameManager.Teams.Red:
                timeOfLastTouches[1] = TimeManager.instance.ElapsedTimeSinceKickoff; break;
            case GameManager.Teams.Green:
                timeOfLastTouches[2] = TimeManager.instance.ElapsedTimeSinceKickoff; break;
        }
    }

    // doesnt change replay colour
    public void Explode(GameManager.Teams scorer)
    {
        GameManager.instance.ShakeCamera(GameInfo.cameraShakes[GameInfo.Shakes.Goal], 0.2f);

        GameObject goalExplosion = Instantiate(goalExplosionPrefab, transform.position, Quaternion.identity);

        SetGEColour.colour = GetColour(scorer);

        ReplayManager.AddReplayObjectToRecordScenes(goalExplosion.GetComponent<ReplayObject>());

        foreach (ReplayObject replayObj in GetComponentsInChildren<ReplayObject>())
        {
            ReplayManager.RemoveReplayObjectFromRecordScenes(replayObj);
        }

        Destroy(gameObject);
    }

    private Color GetColour(GameManager.Teams scorer)
    {
        Material[] playerMats = GameManager.instance.playerMaterials;

        try
        {
            int index = GameManager.instance.teamIndexes[scorer];
        }
        catch (System.Exception)
        {
            Debug.LogError("Couldnt find team index of team " + scorer);
            throw;
        }
        try
        {
            Material mat = playerMats[GameManager.instance.teamIndexes[scorer]];
        }
        catch (System.Exception)
        {
            Debug.LogError("Couldnt find material at index " + GameManager.instance.teamIndexes[scorer]);
            throw;
        }
        try
        {
            Color col = playerMats[GameManager.instance.teamIndexes[scorer]].color;
        }
        catch (System.Exception)
        {
            Debug.LogError("Couldnt get colour of the material " + playerMats[GameManager.instance.teamIndexes[scorer]]);
            throw;
        }
        return playerMats[GameManager.instance.teamIndexes[scorer]].color;
    }


    private void FixedUpdate()
    {
        if (rb != null)
        {
            velocity = rb.velocity;

            if (velocity.magnitude > 25f)
            {
                rb.velocity = velocity.normalized * 25f;
            }
        }
    }
}
