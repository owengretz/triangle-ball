using System.Collections;
using System.Collections.Generic;
using UltimateReplay;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    
    public ParticleSystem boostPrefab;
    public Transform boostTransform;
    public Animator anim;

    public int playerNumber;
    private Rigidbody2D rb;

    private bool canBoost = true;
    [HideInInspector] public bool canMove = true;

    [HideInInspector] public bool isBot;

    // movement
    [HideInInspector] public float thrustForce;
    [HideInInspector] public float turnSpeed;
    [HideInInspector] public float boostForce;

    private int boostsAvailable;



    //private readonly float demoThreshold = 6.5f;
    //private readonly float bumpFactor = 30f;
    private Vector3 posLastFrame;
    private float velocityLastFrame;
    private float velocityThisFrame;

    // input
    private string thrustButtonName;
    public bool thrusting;

    private string boostButtonName;
    //private float boostInputValue;

    private string turnAxisName;
    [HideInInspector] public float turnInputValue;

    [HideInInspector] public string powerupButtonName;
    private float timeOfLastBump;

    private LineRenderer graph;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        //graph = GameObject.Find("Linee").GetComponent<LineRenderer>();
    }


    private void Start()
    {
        thrustButtonName = "Player " + (playerNumber + 1) + " Thrust";
        boostButtonName = "Player " + (playerNumber + 1) + " Boost";
        powerupButtonName = "Player " + (playerNumber + 1) + " Use Powerup";
        //boostButtonName = "Boost" + playerNumber;
        turnAxisName = "Horizontal " + (playerNumber + 1);
        //powerupButtonName = "Powerup" + playerNumber;

        thrustForce = GameInfo.playerThrustForce;
        turnSpeed = GameInfo.playerTurnSpeed;
        boostForce = GameInfo.playerBoostForce;
        rb.mass = GameInfo.playerMass;
        rb.drag = GameInfo.playerDrag;

        posLastFrame = Vector3.zero;
        velocityLastFrame = 0;
        velocityThisFrame = 0;
    }

    private void Update()
    {
        //if (Input.GetButton("Test")) Debug.Log("worked");

        if (!canMove) { thrusting = false; turnInputValue = 0f; }

        if (!canMove || isBot)
            return;


        if (Sinput.GetButtonDown(boostButtonName) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            UseBoost();
        }
        thrusting = Sinput.GetButton(thrustButtonName);
        turnInputValue = Sinput.GetAxisRaw(turnAxisName);
    }


    public void UseBoost()
    {
        if (boostsAvailable == 0 || !canBoost || !canMove)
            return;

        GameManager.instance.ShakeCamera(GameInfo.cameraShakes[GameInfo.Shakes.Boost], 0.1f);

        ReplayManager.AddReplayObjectToRecordScenes(Instantiate(boostPrefab, boostTransform.position, boostTransform.rotation).GetComponent<ReplayObject>());

        rb.AddRelativeForce(Vector2.up * boostForce, ForceMode2D.Impulse);

        ChangeBoostsAvailable(-1);

        canBoost = false;
        Invoke("ResetBoostCooldown", 0.5f);
    }
    public void ResetBoostCooldown()
    {
        canBoost = true;
    }

    public bool PickupBoost()
    {
        if (boostsAvailable < 3) ChangeBoostsAvailable(1);
        else return false;
        return true;
    }

    public void ChangeBoostsAvailable(int change, bool setToAmount = false)
    {
        if (!setToAmount)
            boostsAvailable += change;
        else
            boostsAvailable = change;
        anim.SetInteger("Boosts", boostsAvailable);
    }

    private void FixedUpdate()
    {
        if (!canMove) return;

        Turn();

        if (thrusting) Thrust();


        //if (velocityLastFrame > demoThreshold && !trail.isPlaying) trail.Play();
        //else if (velocityLastFrame < demoThreshold && trail.isPlaying) trail.Stop();

        velocityThisFrame = transform.InverseTransformDirection((transform.position - posLastFrame) / Time.fixedDeltaTime).y;
        velocityLastFrame = velocityThisFrame;
        posLastFrame = transform.position;



        //if (isBot) return;

        //Vector3[] positions = new Vector3[graph.positionCount + 1];

        //graph.GetPositions(positions);

        //positions[positions.Length - 1] = new Vector3(40f + 1f * Time.fixedDeltaTime * positions.Length, rb.velocity.magnitude, 0f);

        //graph.positionCount = positions.Length;
        //graph.SetPositions(positions);



    }

    private void Thrust()
    {
        Vector2 movement = transform.up * thrustForce * Time.fixedDeltaTime;

        rb.AddForce(movement);
    }

    private void Turn()
    {
        float turn = turnInputValue * turnSpeed * Time.fixedDeltaTime;

        Vector3 rotationVector = new Vector3(0, 0, -turn);

        rb.transform.Rotate(rotationVector);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // demos disabled
        if (GameInfo.playerDemolishThreshold == -1f)
            return;

        if (!collision.CompareTag("Player"))
            return;

        PlayerManager victim = collision.gameObject.GetComponent<PlayerManager>();


        bool demo = velocityLastFrame > GameInfo.playerDemolishThreshold;

        if (!GameInfo.demosEnabled)
            demo = false;

        else if (!GameInfo.friendlyDemosEnabled && victim.team == GetComponent<PlayerManager>().team)
            demo = false;

        // demo
        if (demo)
        {
            victim.TriggerDie();
        }
        // bump
        else
        {
            // so that we dont bump 2 frames in a row
            if (Time.time - timeOfLastBump < 0.1f)
                return;
            timeOfLastBump = Time.time;

            Vector2 dir = (Vector2)transform.up;

            PentagonPowerup pentagon = GetComponent<PentagonPowerup>();
            if (pentagon != null) dir = pentagon.BumpDir(victim.transform.position);

            victim.GetBumped(dir * 5f);

            GameManager.instance.ShakeCamera(GameInfo.cameraShakes[GameInfo.Shakes.Bump], 0.1f);
        }
    }

    // camera shake
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody2D ballRB = collision.gameObject.GetComponent<Rigidbody2D>();
            if (ballRB != null)
            {
                float shakeAmount = 0.1f;
                if (rb != null)
                {
                    float velDiff = (rb.velocity - collision.gameObject.GetComponent<Rigidbody2D>().velocity).magnitude;
                    shakeAmount = Mathf.Clamp(velDiff / 50f, 0f, 0.3f);
                }
                GameManager.instance.ShakeCamera(shakeAmount, 0.1f);
            }
        }
        else if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Goal"))
        {
            float shakeAmount = Mathf.Clamp(rb.velocity.magnitude / 25f, 0f, 1f);
            GameManager.instance.ShakeCamera(shakeAmount, 0.1f);
        }
    }



    public int BoostsAvailable()
    {
        return boostsAvailable;
    }
    public void SetThrust(bool thrust)
    {
        if (canMove) thrusting = thrust;
    }
    public void SetTurn(float turn)
    {
        if (canMove) turnInputValue = turn;
    }
}
