using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotUtils : MonoBehaviour
{
    [HideInInspector] public Transform self;
    [HideInInspector] public Transform teammate;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public PlayerMovement m;
    [HideInInspector] public int side;
    [HideInInspector] public Vector3 myGoalPos;
    [HideInInspector] public Vector3 myBackOfGoalPos;

    [HideInInspector] public Transform opponent;
    [HideInInspector] public Rigidbody2D opponentrb;
    [HideInInspector] public PlayerMovement opponentScript;
    [HideInInspector] public Vector3 opponentGoalPos;
    [HideInInspector] public GameManager.Teams opponentTeam;

    [HideInInspector] public Transform ball;
    [HideInInspector] public Rigidbody2D ballRB;

    [HideInInspector] public BoostPad[] boostPadScripts;
    [HideInInspector] public Transform[] boostPadTransforms;

    [HideInInspector] public Vector2 ballPosAtETA;
    [HideInInspector] public bool ballOnTarget;
    public enum BoostTarget { Ball, Goal, Nothing };
    [HideInInspector] public Transform mostDesirableBoost;

    [HideInInspector] public PlayerPowerupScript powerupScript;
    [HideInInspector] public bool usingPowerup;
    [HideInInspector] public bool stopMainLogic;

    public bool humanTesting;

    private List<(Vector2, Vector2)> gizmoLines = new List<(Vector2, Vector2)>();
    private List<Vector2> gizmoBalls = new List<Vector2>();

    private LineRenderer lineRend;
    private Transform ballRend;
    [HideInInspector] public Transform cursor;

    [HideInInspector] public bool easyMode;

    public void Setup(GameManager.Teams team)
    {
        self = transform;
        
        List<PlayerManager> myTeamPlayers = new List<PlayerManager>();
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerManager pManager = player.GetComponent<PlayerManager>();
            if (pManager.team == team) myTeamPlayers.Add(pManager);
        }
        if (myTeamPlayers.Count == 1) teammate = null;
        else
            teammate = myTeamPlayers[0] == GetComponent<PlayerManager>() ? myTeamPlayers[0].transform : myTeamPlayers[1].transform;

        rb = GetComponent<Rigidbody2D>();
        m = GetComponentInParent<PlayerMovement>();
        cursor = GameObject.Find("Bot Cursor " + m.playerNumber).transform;
        m.isBot = true;


        if (team == GameManager.Teams.Blue)
        {
            side = 1;
            opponentTeam = GameManager.Teams.Red;

            myBackOfGoalPos = GameObject.Find("Blue Goal").transform.position;
            myGoalPos = myBackOfGoalPos + new Vector3(1f, 0, 0);
            opponentGoalPos = GameObject.Find("Red Goal").transform.position;
            opponentGoalPos -= new Vector3(1f, 0, 0);
        }
        else if (team == GameManager.Teams.Red)
        {
            side = -1;
            opponentTeam = GameManager.Teams.Blue;

            myBackOfGoalPos = GameObject.Find("Red Goal").transform.position;
            myGoalPos = myBackOfGoalPos - new Vector3(1f, 0, 0);
            opponentGoalPos = GameObject.Find("Blue Goal").transform.position;
            opponentGoalPos += new Vector3(1f, 0, 0);
        }

        foreach (PlayerManager player in GameManager.instance.players) if (player.team == opponentTeam) opponent = player.transform;
        opponentScript = opponent.GetComponent<PlayerMovement>();
        opponentrb = opponent.GetComponent<Rigidbody2D>();


        ball = FindObjectOfType<BallManager>().transform;
        ballRB = ball.GetComponent<Rigidbody2D>();

        BotScript bs = gameObject.AddComponent<BotScript>();
        bs.u = this;
        bs.g = GameManager.instance;
        StartCoroutine(bs.KickOff());
        if (GameManager.instance.botTest)
        {
            StartCoroutine(TestDelay(bs));
        }

        if (GameInfo.showBotCursors)
        {
            Color playerCol = GameManager.instance.playerMaterials[m.playerNumber].color;

            lineRend = GameObject.Find("Bot Rend " + m.playerNumber).GetComponent<LineRenderer>();
            lineRend.startColor = playerCol;
            lineRend.endColor = playerCol;
            ballRend = GameObject.Find("Ball Rend " + m.playerNumber).transform;
            ballRend.GetComponent<SpriteRenderer>().color = playerCol;
        }

        easyMode = GameInfo.botPlayers[m.playerNumber] == 1;
    }
    private IEnumerator TestDelay(BotScript bs) 
    {
        m.canMove = false;
        RegisterBoosts();
        yield return new WaitForSeconds(1f);
        m.canMove = true;
        StartCoroutine(bs.BotLogic());
    }
    public void RegisterBoosts()
    {
        GameObject[] boosts = GameObject.FindGameObjectsWithTag("Boost Pad");
        boostPadScripts = new BoostPad[boosts.Length];
        boostPadTransforms = new Transform[boosts.Length];
        for (int i = 0; i < boosts.Length; i++)
        {
            boostPadScripts[i] = boosts[i].GetComponent<BoostPad>();
            boostPadTransforms[i] = boosts[i].transform;
        }
    }

    public void Thrust(float thrustAmount = 1f)
    {
        if (thrustAmount == 1f) m.SetThrust(true);
        else m.SetThrust(Random.Range(0f, 1f) < thrustAmount);
    }

    public void Turn(float amount)
    {
        m.SetTurn(amount);
    }

    public void UseBoost()
    {
        m.UseBoost();
    }

    public int BoostsAvailable()
    {
        return m.BoostsAvailable();
    }


    public float GetBoostScore(BoostTarget target)
    {
        int boostPadsUp = 0;
        Vector3 targetPos;
        switch (target)
        {
            case BoostTarget.Ball:
                targetPos = ballPosAtETA;
                break;
            case BoostTarget.Goal:
                targetPos = myGoalPos;
                break;
            case BoostTarget.Nothing:
            default:
                targetPos = rb.position + rb.velocity * 5f;
                break;
        }

        (Transform, float) mostDesirable = (boostPadTransforms[0], -1000f);

        Transform[] pads = boostPadTransforms;
        for (int i = 0; i < pads.Length; i++)
        {
            if (!boostPadScripts[i].up) continue;

            boostPadsUp++;

            // on the way
            bool closerThanTarget = (self.position - pads[i].position).magnitude < (self.position - targetPos).magnitude;

            float distFromPath;
            if (Vector2.Angle(rb.velocity, pads[i].position) > 90f)
            {
                distFromPath = 10f;
            }
            else
            {
                float slope = (self.position.y - targetPos.y) / (self.position.x - targetPos.x);
                distFromPath = Mathf.Abs(slope * pads[i].position.x - pads[i].position.y + (self.position.y - slope * self.position.x)) / Mathf.Sqrt(slope * slope + 1);
            }

            bool selfCloserToBallThanOpp = DistanceToBall() + 1f < TheirDistanceToBall();

            float distToPad = (pads[i].position - self.position).magnitude;
            //

            // moving towards
            float velDiffWithBall = CompareSelfVelocity(pads[i].position);
            bool movingTowardsPad = velDiffWithBall < 30f;
            //


            // get boost if we are travelling towards it and are reasonably close

            float score = 100f;

            score -= distFromPath;
            score -= velDiffWithBall / 10f;


            if (target == BoostTarget.Ball && !selfCloserToBallThanOpp)
                score = 0f;

            if (target == BoostTarget.Goal && ImminentDanger())
                score = 0f;

            if (!closerThanTarget || distFromPath > 4f)
                score = 0f;

            if (!movingTowardsPad || SelfHasPossession() || ImminentDanger())
                score = 0f;

            if (BoostsAvailable() == 3)
                score = 0f;

            if (ThemOnside() && pads[i].position.x * side > self.position.x * side && distToPad > 3f)
                score = 0f;

            // replace if more desirable
            if (score > mostDesirable.Item2)
            {
                mostDesirable = (pads[i], score);
            }
        }

        if (boostPadsUp == 0)
        {
            return -1000f;
        }

        //Debug.Log("boost distance from path to target: " + d);

        mostDesirableBoost = mostDesirable.Item1;
        return mostDesirable.Item2;
    }

    // distances
    public float DistanceToMyGoal()
    {
        return (self.position - myGoalPos).magnitude;
    }

    public float DistanceToMyBackOfGoal()
    {
        return (self.position - myBackOfGoalPos).magnitude;
    }

    public float DistanceMyGoalToBall()
    {
        if (ball != null) return (ball.position - myGoalPos).magnitude;
        else return 0f;
    }

    public float DistanceToClosestBoost()
    {
        if (!ClosestBoost()) return 1000f;
        return (self.position - ClosestBoost().position).magnitude;
    }

    public float DistanceToBall()
    {
        return (self.position - ball.position).magnitude;
    }

    // angles
    public float AngleToClosestBoost()
    {
        if (!ClosestBoost()) return 1000f;
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

    //other
    public float ForwardVelocity()
    {
        return self.InverseTransformDirection(rb.velocity).y;
    }

    public Vector2 RelativeVelocity()
    {
        return BallVel() - rb.velocity;
    }


    

    public float ShotAngleValue(bool signed = false, bool opponentShotAngleValue = false, bool ownGoalValue = false)
    {
        int ogv = ownGoalValue ? -1 : 1;

        float shotAngleValue;
        
        Vector3 ballPos = opponentShotAngleValue == false ? (Vector3)ballPosAtETA : ball.position;
        if (ownGoalValue && Mathf.Abs(BallVel().x) > Mathf.Abs(BallVel().y)) ballPos = ball.position;
        Vector3 playerPos = opponentShotAngleValue == false ? self.position : opponent.position;

        float angleToBall = Vector2.SignedAngle(Vector2.right * side * ogv, ballPos - playerPos) * side * ogv;

        Vector3 goalPos = !opponentShotAngleValue && !ownGoalValue ? opponentGoalPos : myGoalPos;

        Vector3 topPost = goalPos + Vector3.up * 1.2f;
        Vector3 bottomPost = goalPos - Vector3.up * 1.2f;

        float angleToTopPost = Vector2.SignedAngle(Vector2.right * side * ogv, topPost - ballPos) * side * ogv;
        float angleToBottomPost = Vector2.SignedAngle(Vector2.right * side * ogv, bottomPost - ballPos) * side * ogv;

        if (angleToBall < angleToTopPost && angleToBall > angleToBottomPost)
        {
            shotAngleValue = 0f;
        }
        else if (angleToBall > angleToTopPost)
        {
            shotAngleValue = Mathf.Abs(angleToBall - angleToTopPost);
            if (signed)
            {
                shotAngleValue *= side;
            }
        }
        else
        {
            shotAngleValue = Mathf.Abs(angleToBall - angleToBottomPost);
            if (signed)
            {
                shotAngleValue *= -side;
            }
        }

        return shotAngleValue;
    }

    public Vector3 CompensatedBallPos()
    {
        Vector2 compensatedBallPos;

        float compensation = 0f;
        float shotAngle = ShotAngleValue(true);
        if (shotAngle != 0f)
        {
            compensation = Mathf.Clamp(Mathf.Sqrt(Mathf.Abs(shotAngle)) / 10f, 0f, 0.38f);
        }

        Vector2 ballDir = (ballPosAtETA - (Vector2)self.position).normalized;
        Vector2 perpDir = new Vector2(-ballDir.y, ballDir.x) * Mathf.Sign(shotAngle);
        Vector2 compensationVector = perpDir * compensation;

        compensatedBallPos = new Vector2(ballPosAtETA.x + compensationVector.x, ballPosAtETA.y + compensationVector.y);

        return compensatedBallPos;
    }

    public float AngleToCompensatedBall()
    {
        return AngleToTarget(CompensatedBallPos());
    }

    public bool BallOnTargetMyNet()
    {
        bool ballOnTarget = false;

        int goalMask = LayerMask.GetMask("Goal");
        float raycastDist = BallVel().magnitude * 2f;
        RaycastHit2D ballToNetRay = Physics2D.Raycast(ball.position, BallVel(), raycastDist, layerMask: goalMask);
        if (ballToNetRay.collider != null)
        {
            if (ballToNetRay.collider.transform.position == myBackOfGoalPos)
            {
                ballOnTarget = true;
            }
        }

        return ballOnTarget;
    }

    public float DistanceToBallPosAtETA()
    {
        return Mathf.Clamp((ballPosAtETA - rb.position).magnitude - 1.5f, 0f, Mathf.Infinity);
    }

    private Vector2 BallPosAtETA()
    {
        int iterations = 3;

        float timeToBall = TimeToTarget(ball.position);

        Vector2 ballPos = FutureBallPos(timeToBall);

        for (int i = 0; i < iterations-1; i++)
        {
            timeToBall = TimeToTarget(ballPos);
            ballPos = FutureBallPos(timeToBall);
        }

        return ballPos;
    }


    public float TimeToTarget(Vector3 target, bool boostAvailable = false)
    {
        float timeToTurn = (2f / 3f) * (Mathf.Abs(AngleToTarget(target)) / 180f);

        float distanceFromTarget = Mathf.Clamp((target - self.position).magnitude - 1.5f, 0f, Mathf.Infinity);
        float initialVelocity = Mathf.Clamp(rb.velocity.magnitude, 0f, 6.3f);


        float timeToTravelDistanceFromZeroVelocity = TimeOverDistance(distanceFromTarget);
        float velocityAfterTravellingDistanceFromZeroVelocity = VelocityOverTime(timeToTravelDistanceFromZeroVelocity);

        float timeToReachCurrentVelocity = TimeOverVelocity(initialVelocity);
        float timeToReachVelocityFromZero = TimeOverVelocity(velocityAfterTravellingDistanceFromZeroVelocity);

        float velocityWhenReachingTarget = VelocityOverTime(Mathf.Clamp(timeToReachCurrentVelocity + timeToReachVelocityFromZero, 0f, 5f));

        float timeOfVelocityWhenReachingTarget = TimeOverVelocity(velocityWhenReachingTarget);
        float timeOfInitialVelocity = TimeOverVelocity(initialVelocity);

        float averageVelocity = AverageVelocityWithinTime(timeOfInitialVelocity, timeOfVelocityWhenReachingTarget);

        return (Vector2.Angle(rb.velocity, (Vector2)target - rb.position) / 360f) * Mathf.Clamp(distanceFromTarget, 0f, 1f) + timeToTurn + distanceFromTarget / averageVelocity;
    }

    private float ConstantA()                       { return Mathf.Exp(Mathf.Log(6.3f) / 4.5f) / 5f; }
    private float VelocityOverTime(float time)      { return -Mathf.Pow(-ConstantA() * (time - 5f), 4.5f) + 6.3f; }
    private float TimeOverVelocity(float velocity)  { return -(Mathf.Pow(6.3f - velocity, 2f / 9f) / ConstantA()) + 5f; }
    private float DistanceOverTime(float time)      { return Mathf.Pow(5 * ConstantA() - ConstantA() * time, 5.5f) / (5.5f * ConstantA()) + (6.3f * time) - 5.727f; }
    private float TimeOverDistance(float distance)  { return Mathf.Pow(0.45f * distance, 0.56f); }
    private float AverageVelocityWithinTime(float startingTime, float endingTime)
    {
        if (endingTime - startingTime <= 0) return VelocityOverTime(endingTime);
        return (1 / (endingTime - startingTime)) * (DistanceOverTime(endingTime) - DistanceOverTime(startingTime));
    }

    public Vector2 BallVel()
    {
        if (ballRB != null)
            return ballRB.velocity;
        else if (FindObjectOfType<SpikesPowerup>() != null)
            return FindObjectOfType<SpikesPowerup>().ballVelocity - rb.velocity;

        return Vector2.zero;
    }


    public bool GoingToOwnGoal() { return OwnGoalValue() < 15f; }
    public float OwnGoalValue()
    {
        return ShotAngleValue(ownGoalValue: true);
    }
    public Vector2 AntiOwnGoalCompensation(float amount = 1.5f)
    {
        Vector2 ballDir = (ballPosAtETA - (Vector2)self.position).normalized;
        Vector2 perpDir = new Vector2(-ballDir.y, ballDir.x) * side;

        Vector2 target1 = ballPosAtETA + (perpDir * amount);
        Vector2 target2 = ballPosAtETA - (perpDir * amount);

        float dist1 = ((Vector2)myGoalPos - target1).magnitude;
        float dist2 = ((Vector2)myGoalPos - target2).magnitude;
        // which side of the ball to go to
        if (Mathf.Abs(dist1 - dist2) > 0.3f)
        {
            if (dist1 < dist2)
            {
                return target1;
            }
            else
            {
                return target2;
            }
        }
        else
        {
            return target1;
        }
    }


    public int OpponentBoostAmount()
    {
        return opponentScript.BoostsAvailable();
    }


    public Transform ClosestBoost(Vector2 ? targetPos = null, bool onlyOnside = false)
    {
        Vector2 target;
        if (targetPos == null) target = self.position;
        else target = (Vector2)targetPos;


        Transform closestBoost = null;
        float closestDist = 9999f;

        for (int i = 0; i < boostPadTransforms.Length; i++)
        {
            if (boostPadScripts[i].up)
            {
                float distance = (target - (Vector2)boostPadTransforms[i].position).magnitude;
                if (distance < closestDist)
                {
                    if (!onlyOnside || (onlyOnside && boostPadTransforms[i].position.x * side < self.position.x * side))
                    {
                        closestBoost = boostPadTransforms[i];
                        closestDist = distance;
                    }
                }
            }
        }
        return closestBoost;
    }

    public Vector2 DirOppositeOfVel()
    {
        return (Vector2)self.position - rb.velocity.normalized;
    }


    // their triangle

    // distances
    public float TheirDistanceToBall()
    {
        return (opponent.position - ball.position).magnitude;
    }

    public float TheirDistanceToTheirGoal()
    {
        return (opponent.position - opponentGoalPos).magnitude;
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
        return (ball.position - opponentGoalPos).magnitude;
    }





    public float AngleToTarget(Vector3 target)
    {
        Vector3 vectorToTarget = target - self.position;

        // -ve bc clockwise is +ve, but for movement script turning right (ccw) is +ve
        float angleToTarget = -Vector2.SignedAngle(self.up, vectorToTarget);

        return angleToTarget;
    }


    public bool BallBehindGoal()
    {
        if (ball == null) return false;
        return TargetBehindGoal(ball.position);
    }

    public bool TargetBehindGoal(Vector3 target)
    {
        RaycastHit2D hit = Physics2D.Raycast(self.position, target - self.position, Mathf.Infinity, LayerMask.GetMask("GoalCol"));

        if (!hit) return false;
        return ((Vector3)hit.point - self.position).magnitude < (target - self.position).magnitude;
    }

    public bool BallPastTheirGoal()
    {
        return ball.position.x * side > (opponentGoalPos.x + 1f * side) * side;
    }

    public bool BallPastMyGoal()
    {
        return ball.position.x * side < myGoalPos.x * side;
    }

    public bool SelfPastMyGoal()
    {
        return self.position.x * side < myGoalPos.x * side;
    }

    public bool SelfPastTheirGoal()
    {
        return self.position.x * side > (opponentGoalPos.x + 1f * side) * side;
    }



    public float DistToTeammate()
    {
        if (teammate == null)
            return 1000f;

        return (self.position - teammate.position).magnitude;
    }

    public float DistanceTeammateToBall()
    {
        if (teammate == null)
            return 1000f;

        return (teammate.position - ball.position).magnitude;
    }


    public float CompareBallVelocity(Vector2 compareWithPos)
    {
        return Vector2.Angle(BallVel(), compareWithPos - (Vector2)ball.position);
    }

    public float CompareSelfVelocity(Vector2 compareWithPos)
    {
        return Vector2.Angle(rb.velocity, compareWithPos - (Vector2)self.position);
    }


    public bool BallGoingToMyNetWithxVelocity(float xVelocity)
    {
        return BallVel().x * side < -xVelocity;
    }


    public bool OpponentAboutToShoot()
    {
        float oppAngleToBall = Vector2.Angle(opponent.position, ball.position - opponent.position);
        float oppCompVelWithBall = (opponentrb.velocity - BallVel()).magnitude;
        float oppDistToBall = TheirDistanceToBall();
        float oppxVel = opponentrb.velocity.x * -side;
        if (OpponentBoostAmount() >= 1) oppDistToBall /= 2f;
        
        if (oppAngleToBall < 45f && oppCompVelWithBall > 3f && oppDistToBall < 3f && oppxVel > 3f)
        {
            return true;
        }

        return false;
    }
    public bool OpponentHasPossession()
    {
        return Vector2.Angle(opponent.up, ball.position) < 60f && ThemCloseToBall() && Vector2.Angle(opponentrb.velocity, ball.position - opponent.position) < 60f;
    }
    public bool SelfHasPossession()
    {
        return Vector2.Angle(self.up, ball.position) < 60f && DistanceToBall() < 5f && Vector2.Angle(rb.velocity, ball.position - self.position) < 60f;
    }
    public bool ImminentDanger()
    {
        return OpponentAboutToShoot() || (OpponentHasPossession() && OpponentBoostAmount() >= 1) || BallVel().x * -side * 1.5f - DistanceMyGoalToBall() > 0f;
    }


    public Vector2 FutureBallPos(float time, bool drawToScreen = true)
    {
        gizmoLines.Clear();
        gizmoBalls.Clear();

        float currentVel = Mathf.Clamp(BallVel().magnitude, 0f, 21.5f);
        float timeForCurrentVel = 6 - (Mathf.Log(currentVel) / Mathf.Log(1.67f));

        float timeForFutureVel = timeForCurrentVel + time;

        float distAtFutureTime = -Mathf.Pow(1.67f, -timeForFutureVel + 6f) / (Mathf.Log(1.67f));
        float distAtCurrentTime = -Mathf.Pow(1.67f, -timeForCurrentVel + 6f) / (Mathf.Log(1.67f));

        float distance = distAtFutureTime - distAtCurrentTime;

        Vector2 pos = ball.position;
        Vector2 dir = BallVel().normalized;

        int iteration = 0;
        while (distance > 0f)
        {
            if (iteration > 50)
            {
                break;
            }

            RaycastHit2D hit = Physics2D.CircleCast(pos, 0.36f - iteration * 0.01f, dir, Mathf.Infinity, LayerMask.GetMask("GoalCol", "Wall", "Goal"));
            if (!hit) break;

            Vector2 bouncePos = hit.centroid;
            Vector2 closestPoint = hit.point;

            // ball going in
            if (hit.collider.CompareTag("Goal") && (hit.point - pos).magnitude < distance)
            {
                if (drawToScreen)
                {
                    gizmoLines.Add((pos, bouncePos));
                    gizmoBalls.Add(bouncePos);
                }
                if (hit.collider.transform.position == myBackOfGoalPos) ballOnTarget = true;
                return bouncePos;
            }

            Vector2 normal = (bouncePos - closestPoint).normalized;
            float diff = Vector2.SignedAngle(-dir, normal) * Mathf.Deg2Rad;
            float normalAngle = Mathf.Atan2(normal.y, normal.x);
            float outgoingAngle = normalAngle + diff;
            Vector2 outgoingDir = new Vector2(Mathf.Cos(outgoingAngle), Mathf.Sin(outgoingAngle));

            float dist = (bouncePos - pos).magnitude;
            if (dist > distance)
            {
                bouncePos = pos + dir.normalized * distance;
            }
            distance -= dist;

            if (drawToScreen)
            {
                gizmoLines.Add((pos, bouncePos));
                gizmoBalls.Add(bouncePos);
            }

            pos = bouncePos;
            dir = outgoingDir;
            if (distance <= 0f)
            {
                break;
            }
            iteration++;
        }

        return pos;
    }

    private void Update()
    {
        if (ball == null)
        {
            ballPosAtETA = Vector2.zero;
        }
        else
        {
            ballPosAtETA = BallPosAtETA();
        }


        if (GameInfo.showBotCursors && lineRend != null && !easyMode)
        {
            List<Vector3> points = new List<Vector3>();

            if (gizmoLines.Count >= 1) points.Add(gizmoLines[0].Item1);

            foreach ((Vector2, Vector2) line in gizmoLines)
            {
                points.Add(line.Item2);
            }

            lineRend.positionCount = points.Count;

            lineRend.SetPositions(points.ToArray());

            if (gizmoBalls.Count >= 1) ballRend.position = gizmoBalls[gizmoBalls.Count - 1];
            else ballRend.position = Vector2.up * -100f;
        }
    }

    private void OnDisable()
    {
        if (cursor != null) cursor.position = Vector2.up * -100f;
        if (ballRend != null) ballRend.position = Vector2.up * -100f;
    }
}
