using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBot : MonoBehaviour
{
    private Scenario scenario;
    //public int startWithBoosts;
    //private Transform startingBallPos;
    //private Transform startingBotPos;

    private Rigidbody2D rb;
    private PlayerMovement m;
    private Transform ball;
    private Rigidbody2D ballRB;

    private GameManager.Teams opponentTeam;

    public Transform opponent;

    private Vector3 myGoalPos;
    private Vector3 opponentGoalPos;

    private BoostPad[] boostPads;
    private List<Transform> availableBoosts = new List<Transform>();


    private bool haveBoost;
    private bool haveFullBoost;
    private bool haveNoBoost;

    private int side;

    private Vector2 futurePos;

    private Coroutine goBackToNet;


    public IEnumerator Start()
    {
        rb = GetComponent<Rigidbody2D>();
        m = GetComponent<PlayerMovement>();
        m.isBot = GetComponent<PlayerManager>().isBot;
        ball = GameObject.Find("Ball").transform;
        ballRB = ball.GetComponent<Rigidbody2D>();

        side = -1;

        myGoalPos = GameObject.Find("Red Goal").transform.position;
        myGoalPos -= new Vector3(1.5f, 0, 0);
        opponentGoalPos = GameObject.Find("Blue Goal").transform.position;
        opponentGoalPos += new Vector3(1.5f, 0, 0);

        GameObject[] boosts = GameObject.FindGameObjectsWithTag("Boost Pad");
        boostPads = new BoostPad[boosts.Length];
        for (int i = 0; i < boosts.Length; i++) boostPads[i] = boosts[i].GetComponent<BoostPad>();

        scenario = FindObjectOfType<Scenario>();
        m.ChangeBoostsAvailable(scenario.startingBoost, true);
        transform.SetPositionAndRotation(scenario.botPos.position, scenario.botPos.rotation);
        ball.SetPositionAndRotation(scenario.ballPos.position, scenario.ballPos.rotation);

        m.canMove = false;

        yield return new WaitForSeconds(1f);

        ballRB.velocity = scenario.ballVel;
        rb.velocity = scenario.botVel;

        m.canMove = true;
        
        if (m.isBot) StartCoroutine(Logic());

        //StartCoroutine(UpdatePos());
    }

    private void Update()
    {
        //Vector2 targetPos;


        //float eta = (ball.position - transform.position).magnitude / Mathf.Clamp(rb.velocity.magnitude, 4f, 10f);
        //futurePos = ball.GetComponent<Rigidbody2D>().position + ball.GetComponent<Rigidbody2D>().velocity * eta;


        //targetPos = CompensatedBallPos();

        haveNoBoost = m.BoostsAvailable() == 0;
        haveBoost = m.BoostsAvailable() > 0;
        haveFullBoost = m.BoostsAvailable() == 3;

        availableBoosts.Clear();
        for (int i = 0; i < boostPads.Length; i++) if (boostPads[i] != null) if (boostPads[i].up) availableBoosts.Add(boostPads[i].transform);
    }


    private IEnumerator Logic()
    {
        while (true)
        {
            if (MeOnside())
            {
                GoForBall();
            }
            else
            {
                yield return goBackToNet = StartCoroutine(ReturnToGoal());
            }
            yield return null;
        }
    }

    private void GoForBall()
    {
        SteerToTarget(CompensatedBallPos());

        if (Mathf.Abs(ShotAngleValue()) < 0.2f && RelativeVelocity().y < 3f && ForwardVelocity() > 3f && haveBoost)
        {
            m.UseBoost();
        }
        if (CloseToBall() && AngleToBall() > 45)
        {
            SetThrust(0.3f);
        }
        else
        {
            m.SetThrust(true);
        }
    }

    private IEnumerator ReturnToGoal()
    {
        while (!MeOnside())
        //while (transform.position.x * side > (myGoalPos.x + (2f * side)) * side/* && ThemOnside() && ThemCloseToBall()*/)
        {
            //if (MeOnside())
            //{
            //    if (!ThemOnside() || !ThemCloseToBall())
            //    {
            //        StopCoroutine(goBackToNet);
            //        Debug.Log("stop");
            //    }
            //}
            SteerToTarget(myGoalPos);
            if (DistanceToMyGoal() < 5f)
            {
                SetThrust(DistanceToMyGoal() / 5f);
            }
            else
            {
                m.SetThrust(true);
            }

            if (m.BoostsAvailable() >= 2f && AngleToMyGoal() < 25f && DistanceToMyGoal() > 6f)
            {
                m.UseBoost();
            }

            yield return null;
        }

        if (!ThemOnside() && !ThemCloseToBall())
            yield return StartCoroutine(FaceBall());
    }
    private IEnumerator FaceBall()
    {
        while (Mathf.Abs(AngleToCompensatedBall()) > 50f)
        {
            SteerToTarget(CompensatedBallPos());
            yield return null;
        }
    }

    //IEnumerator UpdatePos()
    //{
    //    GameObject.Find("Bot Cursor 0").transform.position = futurePos;
    //    yield return new WaitForSeconds(3f);
    //    StartCoroutine(UpdatePos());
    //}


    public Vector3 CompensatedBallPos()
    {
        float yValueToBall = Vector3.Normalize(ball.position - transform.position).y;
        float yBallValueToGoal = Vector3.Normalize(opponentGoalPos - ball.position).y;
        float yTurn = yValueToBall - yBallValueToGoal;

        float xValueToBall = Vector3.Normalize(ball.position - transform.position).x;
        float xBallValueToGoal = Vector3.Normalize(opponentGoalPos - ball.position).x;
        float xTurn = xValueToBall - xBallValueToGoal;

        Vector2 compensatedBallPos = ball.position;

        float clamp = 0.4f;

        if (yTurn > 0.1f)
        {
            yTurn = clamp;
        }
        else if (yTurn < -0.1f)
        {
            yTurn = -clamp;
        }
        if (Mathf.Abs(yTurn) > 0.1f)
        {
            compensatedBallPos.y += yTurn;
        }

        if (xTurn > 0.1f)
        {
            xTurn = clamp;
        }
        else if (xTurn < -0.1f)
        {
            xTurn = -clamp;
        }
        if (Mathf.Abs(xTurn) > 0.1f)
        {
            compensatedBallPos.x += xTurn;
        }

        compensatedBallPos.x += RelativeVelocity().x / 6f;
        compensatedBallPos.y += RelativeVelocity().y / 6f;

        return compensatedBallPos;
    }

    private void SteerToTarget(Vector3 targetPos)
    {
        GameObject.Find("Bot Cursor 1").transform.position = targetPos;


        Vector3 vectorToTarget = targetPos - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        angle -= 90f;

        float myRotationZ = transform.rotation.eulerAngles.z;

        if (angle < 0) angle += 360f; 
        if (myRotationZ < 0f) myRotationZ += 360f;

        float angleToTarget = myRotationZ - angle;

        if (angleToTarget > 180f)
        {
            angleToTarget -= 360f;
        }
        if (angleToTarget < -180f)
        {
            angleToTarget += 360f;
        }

        if (angleToTarget < 5f && Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            m.SetTurn(0f);
        }
        else if (angleToTarget > 5f)
        {
            m.SetTurn(1f);
        }
        else if (angleToTarget < -5f)
        {
            m.SetTurn(-1f);
        }
        else
        {
            m.SetTurn(0f);
        }
    }

    public void SetThrust(float thrustAmount)
    {
        m.SetThrust(Random.Range(0f, 1f) < thrustAmount);
    }


    // our triangle

    // distances
    public float DistanceToMyGoal()
    {
        return Vector3.Distance(transform.position, myGoalPos);
    }

    public float DistanceMyGoalToBall()
    {
        if (ball != null) return Vector3.Distance(ball.position, myGoalPos);
        else return 0f;
    }

    public float DistanceToClosestBoost()
    {
        return Vector3.Distance(transform.position, ClosestBoost().position);
    }

    public float DistanceToBall()
    {
        return Vector3.Distance(transform.position, ball.position);
    }

    // angles
    public float AngleToClosestBoost()
    {
        return AngleToTarget(ClosestBoost().position);
    }

    public float AngleToBall()
    {
        return AngleToTarget(ball.position); //if positive ball is on right side if negative ball is on left side
    }

    public float AngleToMyGoal()
    {
        return AngleToTarget(myGoalPos);
    }

    // bools
    public bool MeOnside()
    {
        return DistanceToMyGoal() < DistanceMyGoalToBall();
    }

    public bool CloseToBall()
    {
        return DistanceToBall() < 5f;
    }

    //other
    public float ForwardVelocity()
    {
        return transform.InverseTransformDirection(rb.velocity).y;
    }

    public Vector2 RelativeVelocity()
    {
        //if (ball.GetComponent<Rigidbody2D>() != null)
        //{
        return ballRB.velocity - rb.velocity;
        //}
        //else
        //{
        //    return FindObjectOfType<SpikesPowerup>().ballVelocity - rb.velocity;
        //}
    }

    public float ShotAngleValue()
    {
        Vector3 compensatedBallPos = CompensatedBallPos();

        float valueToAngleToCompensatedBall = Vector3.Normalize(compensatedBallPos - transform.position).y;
        float angleToCompensatedBallValueToGoal = Vector3.Normalize(opponentGoalPos - compensatedBallPos).y;
        float shotAngleValue = valueToAngleToCompensatedBall - angleToCompensatedBallValueToGoal;

        return shotAngleValue;
    }

    public float AngleToCompensatedBall()
    {
        return AngleToTarget(CompensatedBallPos());
    }

    public Transform ClosestBoost()
    {
        //finding closest boost - doesnt count "offside" boosts
        Transform closestBoost = ball;
        if (availableBoosts.Count > 0)
        {
            for (int i = 0; i < availableBoosts.Count; i++)
            {
                if (Vector3.Distance(transform.position, availableBoosts[i].transform.position) < Vector3.Distance(transform.position, closestBoost.position)
                    && availableBoosts[i].transform.position.y < (transform.position.y * side))
                {
                    closestBoost = availableBoosts[i].transform;
                }
            }
        }
        return closestBoost;
    }




    // their triangle

    // distances
    public float TheirDistanceToBall()
    {
        return Vector3.Distance(opponent.position, ball.position);
    }

    public float TheirDistanceToTheirGoal()
    {
        return Vector3.Distance(opponent.position, opponentGoalPos);
    }

    //bools
    public bool ThemCloseToBall()
    {
        return TheirDistanceToBall() < 5f;
    }

    public bool ThemOnside()
    {
        return TheirDistanceToTheirGoal() < DistanceTheirGoalToBall();
    }

    // other
    public float DistanceTheirGoalToBall()
    {
        return Vector3.Distance(ball.position, opponentGoalPos);
    }

    private float AngleToTarget(Vector3 target)
    {

        Vector3 vectorToTarget = target - transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        angle -= 90;

        float myRotationZ = transform.rotation.eulerAngles.z;

        if (angle < 0)
        {
            angle += 360;
        }
        if (myRotationZ < 0)
        {
            myRotationZ += 360;
        }

        float angleToTarget = myRotationZ - angle;

        if (angleToTarget > 180)
        {
            angleToTarget -= 360;
        }
        if (angleToTarget < -180)
        {
            angleToTarget += 360;
        }
        return angleToTarget;
    }

    private float RotationToTarget(Vector3 origin, Vector3 target)
    {
        Vector3 vectorToTarget = target - origin;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;

        if (side == -1/* && target != opponentGoalPos*/)
        {
            angle += 180f;
            angle = -angle;
            if (angle < -180f)
            {
                angle += 360f;
            }
        }

        return angle;
    }
}
