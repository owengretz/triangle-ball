using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotScript : MonoBehaviour
{
    public float friction;

    public BotUtils u;
    public GameManager g;

    private bool gettingBoost = false;

    public IEnumerator KickOff()
    {
        while (g.state != GameManager.State.Running) yield return null;

        u.RegisterBoosts();



        int kickoffStrat = g.GetBotKickoffStrat();
        if (u.easyMode) kickoffStrat = 5;
        //kickoffStrat = 4;

        bool corner = Mathf.Abs(u.self.position.y) > 1f;

        switch (kickoffStrat)
        {
            // go for ball, turn randomly a bit before hitting ball to add randomness
            case 1:
                while (u.BoostsAvailable() > 0)
                {
                    u.UseBoost();
                    yield return null;
                }
                while (u.DistanceToBall() > 2.1f + Random.Range(-0.2f, 0.2f))
                {
                    u.Thrust();
                    SteerToTarget(u.ball.position);
                    yield return null;
                }
                float turnDir = Random.Range(0.5f, 1f) * Random.Range(0, 2) * 2 - 1; // second part gives -1 or 1 randomly
                while (u.BallVel() == Vector2.zero)
                {
                    u.Thrust();
                    u.Turn(turnDir);
                    yield return null;
                }
                break;
            // turn inwards on diagonal, pause for a bit on straight
            case 2:
                if (corner)
                {
                    while (Mathf.Abs(u.self.position.y) > 2f)
                    {
                        Retreat();
                        yield return null;
                    }
                }
                else
                {
                    yield return new WaitForSeconds(Random.Range(0.1f, 1f));
                }
                break;
            // just head back to net
            case 3:
                while (Retreat()) // calling retreat makes us go back to net
                {
                    yield return null;
                }
                break;
            // head back to net then wait until opponent does something, max 5s
            case 4:
                float timer = Random.Range(2f, 5f);
                if (true)
                {
                    while (Retreat()) // calling retreat makes us go back to net
                    {
                        yield return null;
                    }
                }
                while (timer > 0f && u.BallVel() == Vector2.zero && u.opponent.position.x * u.side > 0f)
                {
                    FaceBall();
                    timer -= Time.deltaTime;
                    yield return null;
                }
                break;
            // play normally
            case 5:
                break;
        }

        if (u.easyMode) StartCoroutine(EasyBotLogic());
        else StartCoroutine(BotLogic());
    }

    
    public IEnumerator EasyBotLogic()
    {
        while ((g.state != GameManager.State.GoalScored && !u.stopMainLogic) || GameManager.instance.botTest)
        {
            // get ball from behind my net
            while (u.BallPastMyGoal())
            {
                SteerToTarget(u.ball.position);
                u.Thrust();
                yield return null;
            }

            if (u.MeOnside())
            {
                Vector2 target = u.ball.position;
                if (Mathf.Abs(u.AngleToTarget(target)) < 10f) u.UseBoost();
                SteerToTarget(target);
                u.Thrust();
            }
            else
            {
                yield return StartCoroutine(ReturnToGoalEasyMode());
            }
            yield return null;
        }
    }
    private IEnumerator ReturnToGoalEasyMode()
    {
        while (u.DistanceToMyBackOfGoal() > 2f)
        {
            SteerToTarget(u.myBackOfGoalPos);
            u.Thrust();
            yield return null;
        }
        while (Mathf.Abs(u.AngleToBall()) > 10f)
        {
            FaceBall();
            yield return null;
        }
        float timer = 3f;
        while (u.ball.position.x * u.side > 0f && timer > 0f)
        {
            FaceBall();
            timer -= Time.deltaTime;
            yield return null;
        }
    }


    public IEnumerator BotLogic()
    {
        while ((g.state != GameManager.State.GoalScored && !u.stopMainLogic) || GameManager.instance.botTest)
        {
            if (gettingBoost)
            {
                GetBoost(u.mostDesirableBoost);
                yield return null;
                continue;
            }

            //if (u.DistanceTeammateToBall() < u.DistanceToBall())
            //{
            //    yield return StartCoroutine(ReturnToGoal());
            //}

            // get ball from behind my net
            while (u.BallPastMyGoal())
            {
                SteerToTarget(u.ball.position);
                u.Thrust();
                yield return null;
            }

            // if ball behind their net chill, doesnt run if ball is coming towards our goal
            while (u.BallPastTheirGoal() && (u.BallVel().x * u.side > -1f || u.BallBehindGoal()))
            {
                if (u.GetBoostScore(BotUtils.BoostTarget.Nothing) > 0f)
                {
                    gettingBoost = true;
                    break;
                }
                else
                    Retreat();

                yield return null;
            }

            // if we're about to own goal, dont
            if (u.GoingToOwnGoal()) DontOwnGoal();
            // if ball is going in try to make a save
            else if (u.BallOnTargetMyNet()/* && u.ShotAngleValue() > 25f*/) yield return StartCoroutine(MakeSave());
            // get boost if convenient
            else if (u.GetBoostScore(BotUtils.BoostTarget.Ball) > 0f) gettingBoost = true;
            else if (u.MeOnside())
            {
                // go for ball
                ATBA();
            }
            else
            {
                // if not onside then go back to net
                yield return StartCoroutine(ReturnToGoal());
            }
            yield return null;
        }
    }


    private void DontOwnGoal()
    {
        Vector2 target = u.AntiOwnGoalCompensation();
        SteerToTarget(target);
        u.Thrust();
        if ((u.OpponentAboutToShoot() || u.BallOnTargetMyNet()) && Mathf.Abs(u.AngleToTarget(target)) < 10f && u.BoostsAvailable() >= 1)
        {
            u.UseBoost();
        }
    }


    private RaycastHit2D CircleCastBall(float radius)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(u.ball.position, radius, u.BallVel(), Mathf.Infinity, LayerMask.GetMask("Player"));
        if (hits.Length >= 1)
        {
            foreach (RaycastHit2D hit in hits)
            {
                GameObject hitObj = hit.collider.transform.gameObject;
                if (hit.collider.transform.parent != null) hitObj = hit.collider.transform.parent.gameObject;

                if (hitObj == u.self.gameObject)
                    return hit;
            }
        }
        return Physics2D.Raycast(Vector2.zero, Vector2.zero);
    }
    private IEnumerator MakeSave()
    {
        bool inBallPath = false;
        Vector2 ballDir = u.BallVel().normalized;

        while (u.BallOnTargetMyNet())
        {
            Vector2 target = u.ballPosAtETA;
            bool possiblyBoost = false;
            bool slowSave = Mathf.Abs(u.rb.velocity.y) < 4f;

            // opponent about to hit the ball, desperation save, boost at ball
            if (u.OpponentAboutToShoot())
            {
                if (u.BoostsAvailable() >= 1 && Mathf.Abs(u.AngleToTarget(target)) < 5f)
                {
                    SteerToTarget(target);
                    u.Thrust();
                    u.UseBoost();
                }
            }
            // just make sure to save the ball
            else
            {
                // if we are directly in the way of the ball, steer towards ball
                if ((CircleCastBall(0.1f) || inBallPath) && slowSave)
                {
                    if (!inBallPath) inBallPath = true; // once we get this close, dont go back to aiming for ballposateta
                    if (!CircleCastBall(0.36f)) inBallPath = false; // if we happen to get off course exit out of this

                    SteerToTarget(u.ball.position, true);
                    target = u.ball.position;
                    if (u.AngleToTarget(u.ball.position) < 10f)
                        u.Thrust(1f);
                    else
                        u.Thrust(0f);
                }
                // if we are partially in the way of the ball, steer towards ball pos at eta
                else if (CircleCastBall(0.36f) && slowSave)
                {
                    SteerToTarget(u.ballPosAtETA, true);
                    target = u.ballPosAtETA;
                    if (u.AngleToTarget(u.ballPosAtETA) < 90f)
                        u.Thrust(1f);
                    else
                        u.Thrust(0f);
                }
                // if we are almost in the way, get in the way
                else if (CircleCastBall(1.5f) && u.DistanceToBall() > 1.14f && slowSave)
                {
                    RaycastHit2D hit = CircleCastBall(1.5f);

                    SteerToTarget(hit.centroid, true);
                    target = hit.centroid;
                    if (u.AngleToTarget(hit.centroid) < 90f) u.Thrust();
                    Vector2 selfToBall = (u.ball.position - u.self.position);
                    if (Mathf.Abs(selfToBall.y) > Mathf.Abs(selfToBall.x) && u.AngleToTarget(hit.centroid) < 15f)
                    {
                        u.UseBoost();
                    }
                        
                }
                // ball in between us and the net
                else if (u.GoingToOwnGoal())
                {
                    DontOwnGoal();
                    target = u.AntiOwnGoalCompensation();
                    possiblyBoost = true;
                }
                else
                {
                    SteerToTarget(target);
                    u.Thrust();
                    possiblyBoost = true;
                }

                // if we need to boost to reach ball in time
                if (possiblyBoost && u.BoostsAvailable() >= 1 && Mathf.Abs(u.AngleToTarget(target)) < 10f && u.ballOnTarget)
                {
                    u.UseBoost();
                }
            }

            // if opponent hits the ball then recalculate
            if (u.BallVel().normalized != ballDir)
            {
                inBallPath = false;
            }


            yield return null;
        }

        
    }

    private bool Retreat()
    {
        float eta = u.TimeToTarget(u.myGoalPos);

        // if the ball is on target then MakeSave() will be called so no need to add extra conditions for having only 1 boost
        bool boostingWouldHelp = u.BoostsAvailable() >= 1 && Vector2.Angle(Vector2.left * u.side, u.self.up) < 30f && u.DistanceToMyGoal() > 5f;
        bool shouldBoostIfHaveExtra = u.BoostsAvailable() >= 2f && u.BallVel().x * u.side < -2f;
        bool alreadyBoosted = u.rb.velocity.x * u.side < -10f;

        if (boostingWouldHelp && (shouldBoostIfHaveExtra || u.ImminentDanger()) && !alreadyBoosted)
        {
            u.UseBoost();
        }

        // go back to net
        if ((u.DistanceToMyBackOfGoal() > 2f && eta > 0.3f) || u.TargetBehindGoal(u.myGoalPos))
        {
            SteerToTarget(u.myBackOfGoalPos);
            u.Thrust();
        }
        else
        {
            return false;
        }
        return true;
    }

    private bool FaceBall()
    {
        // once we're almost back to net, turn around and thrust a bit to cancel out momentum and end up stationary in the middle of the goal
        // if we're moving back into the net, turn around and thrust
        if (u.rb.velocity.x * u.side < -0.1f && Mathf.Abs(u.rb.velocity.x) > 0.3f)
        {
            Vector2 target = u.DirOppositeOfVel();

            if (Mathf.Abs(u.AngleToTarget(target)) > 165f)
            {
                if (Mathf.Abs(u.ball.position.y) > 1f)
                {
                    if (u.ball.position.y * u.side > 0f) u.Turn(1f);
                    else u.Turn(1f);
                }
                else
                {
                    if (u.self.position.y * u.side > 0f) u.Turn(-1f);
                    else u.Turn(1f);
                }
            }
            else
            {
                SteerToTarget(target, true);
            }

            if (Mathf.Abs(u.AngleToTarget(target)) < 8f)
                u.Thrust(1f);
            else
                u.Thrust(0f);
        }
        // chill and face ball
        else
        {
            u.Thrust(0f);
            SteerToTarget(u.CompensatedBallPos(), true);

            if (Mathf.Abs(u.AngleToCompensatedBall()) < 5f)
                return false;
        }
        return true;
    }


    private bool OpponentDirectlyInWay()
    {
        RaycastHit2D perfectShotRay = Physics2D.Raycast(u.ball.position, u.ball.position - u.self.position, Mathf.Infinity, LayerMask.GetMask("Player"));
        return u.ShotAngleValue() == 0f && perfectShotRay;
    }
    private bool OpponentBlocking()
    {
        return OpponentDirectlyInWay()
            || BallToOpponentNetCast(u.opponentGoalPos + Vector3.up)
            || BallToOpponentNetCast(u.opponentGoalPos)
            || BallToOpponentNetCast(u.opponentGoalPos + Vector3.down)
            || (u.TheirDistanceToTheirGoal() < 5f && u.OpponentBoostAmount() >= 1);
    }
    private bool BallToOpponentNetCast(Vector2 target)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(u.ball.position, 0.2f, target, Mathf.Infinity, LayerMask.GetMask("Player"));
        if (hits.Length >= 1)
        {
            foreach (RaycastHit2D hit in hits)
            {
                GameObject hitObj = hit.collider.transform.gameObject;
                if (hit.collider.transform.parent != null) hitObj = hit.collider.transform.parent.gameObject;

                if (hitObj == u.opponent.gameObject)
                    return true;
            }
        }
        return false;
    }
    private void ATBA()
    {
        SteerToTarget(u.CompensatedBallPos());


        bool opponentBlocking = OpponentBlocking();

        if (u.BoostsAvailable() >= 1 && u.ShotAngleValue() == 0f && Mathf.Abs(u.AngleToCompensatedBall()) < 5f && (u.CompareSelfVelocity(u.CompensatedBallPos()) < 45f || u.DistanceToBall() < 2f))
        {

            bool shootOn1boost = Random.Range(1, Mathf.RoundToInt(0.3f / Time.deltaTime)) == 1;

            if (u.BoostsAvailable() >= 2 || !opponentBlocking || (shootOn1boost && u.rb.velocity.magnitude < 8f && u.BoostsAvailable() == 1 && !OpponentDirectlyInWay() && u.DistanceToBall() < 3f))
            {
                u.UseBoost();
            }
        }

        u.Thrust();
    }

    

    private void GetBoost(Transform boost)
    {
        int index = 0;
        for (int i = 0; i < u.boostPadTransforms.Length; i++)
        {
            if (boost == u.boostPadTransforms[i])
                index = i;
        }

        u.Thrust();
        SteerToTarget(boost.position);
        if (!u.boostPadScripts[index].up || u.CompareSelfVelocity(boost.position) > 45f || u.BoostsAvailable() == 3)
        {
            gettingBoost = false;
        }

        if (u.ballOnTarget && Mathf.Abs(u.AngleToTarget(boost.position)) < 10f && u.BoostsAvailable() >= 1)
            u.UseBoost();
            
    }

    private IEnumerator ReturnToGoal()
    {
        bool returning = true;
        bool turningAround = false;

        while (returning || turningAround)
        {

            if (returning)
            {
                returning = Retreat();
                if (!returning) turningAround = true;
            }
            else
            {
                turningAround = FaceBall();
                if (!turningAround) break;
            }

            // if we are done going back to net (returned to net and turned around)
            // if we are both on my side of the ball
            // if ball is on target (then exit this to call MakeSave)
            // if ball is behind my goal (then exit this to go get it)
            // if we can get to the ball before them
            // if we're about to own goal (exit to compensate)
            if ((!returning && !turningAround) || (u.MeOnside() && !u.ThemOnside()) || u.BallOnTargetMyNet() || u.BallBehindGoal()
                || /*(u.BallVel().x * u.side < 0f && u.DistanceToBall() + (u.OpponentBoostAmount() >= 1 ? 5f : 1f) < u.TheirDistanceToBall()) || */u.GoingToOwnGoal())
            {
                returning = false;
                turningAround = false;
            }


            if (u.GetBoostScore(BotUtils.BoostTarget.Goal) > 0f)
            {
                gettingBoost = true;
                returning = false;
                turningAround = false;
            }

            yield return null;
        }
    }


    private void SteerToTarget(Vector3 targetPos, bool dontCompensateForVel = false)
    {
        Vector3 vectorToTarget = targetPos - u.self.position;

        // -ve bc clockwise is +ve, but for movement script turning right (ccw) is +ve
        float angleToTarget = -Vector2.SignedAngle(u.self.up, vectorToTarget);


        if (u.TargetBehindGoal(targetPos)) angleToTarget = CompensateForNet(vectorToTarget);


        if (GameInfo.showBotCursors) u.cursor.position = targetPos;


        // turn extra to compensate for velocity
        bool ballCondition = u.DistanceToBallPosAtETA() > 0.5f || (u.rb.velocity - u.BallVel()).magnitude > 3f || Vector2.Angle(u.rb.velocity, u.BallVel()) > 30f;
        if (u.rb.velocity.magnitude > 1f && !dontCompensateForVel && ballCondition)
        {
            float velocityDifference = Vector2.SignedAngle(u.rb.velocity, (Vector2)targetPos - u.rb.position);

            float turnAmount = angleToTarget - velocityDifference;
            angleToTarget = Mathf.Clamp(turnAmount, angleToTarget - 30f, angleToTarget + 30f);
        }


        if (Mathf.Abs(angleToTarget) < 3f)
        {
            u.Turn(0f);
        }
        else
        {
            u.Turn(Mathf.Sign(angleToTarget));
        }
    }
    private float CompensateForNet(Vector2 vectorToTarget)
    {
        Vector3 targetPos;
        float angleToTarget;

        float increment = 5f;

        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        float offset;
        float newAngle;
        Vector3 newTarget;

        offset = increment;
        newAngle = angle * Mathf.Deg2Rad;
        newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        while (Physics2D.Raycast(u.self.position, newTarget, Mathf.Infinity, LayerMask.GetMask("GoalCol")))
        {
            offset += increment;
            newAngle = (angle + offset) * Mathf.Deg2Rad;
            newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        }
        newAngle = (angle + offset + 10f) * Mathf.Deg2Rad;
        newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        Vector3 targetPosCW = Physics2D.Raycast(u.self.position, newTarget, Mathf.Infinity, LayerMask.GetMask("Wall")).point;

        offset = increment;
        newAngle = angle * Mathf.Deg2Rad;
        newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        while (Physics2D.Raycast(u.self.position, newTarget, Mathf.Infinity, LayerMask.GetMask("GoalCol")))
        {
            offset += increment;
            newAngle = (angle - offset) * Mathf.Deg2Rad;
            newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        }
        newAngle = (angle - offset - 10f) * Mathf.Deg2Rad;
        newTarget = new Vector3(Mathf.Cos(newAngle), Mathf.Sin(newAngle), 0f);
        Vector3 targetPosCCW = Physics2D.Raycast(u.self.position, newTarget, Mathf.Infinity, LayerMask.GetMask("Wall")).point;

        // go whichever way we're alr moving in
        if (u.rb.velocity.magnitude > 0.1f)
        {
            float cwAngle = Mathf.Abs(Vector3.Angle(u.rb.velocity, targetPosCW));
            float ccwAngle = Mathf.Abs(Vector3.Angle(u.rb.velocity, targetPosCCW));

            targetPos = cwAngle < ccwAngle ? targetPosCW : targetPosCCW;
        }
        // go whichever way is closest
        else
        {
            targetPos = (targetPosCW - u.self.position).magnitude < (targetPosCCW - u.self.position).magnitude ? targetPosCW : targetPosCCW;
        }

        vectorToTarget = targetPos - u.self.position;
        angleToTarget = -Vector2.SignedAngle(u.self.up, vectorToTarget);
        return angleToTarget;
    }

    private void Update()
    {
        // so that bot chases opponent after goal is scored
        if (g.state == GameManager.State.GoalScored && u.opponent != null || u.ball == null)
        {
            StopAllCoroutines();
            Vector2 target = u.opponentScript.canMove ? u.opponent.position : (u.ClosestBoost() != null ? u.ClosestBoost().position : u.myGoalPos);
            SteerToTarget(target);
            u.Thrust();
            if (u.AngleToTarget(u.opponent.position) < 5f)
            {
                u.UseBoost();
            }
        }

        if (u.powerupScript != null)
        {
            if (!u.usingPowerup && u.powerupScript.canUsePowerup && u.ball != null && g.state != GameManager.State.GoalScored)
            {
                u.usingPowerup = true;
                StartCoroutine(HandlePowerup());
            }
        }
    }

    #region Powerup Handling

    private IEnumerator HandlePowerup()
    {
        if (u.easyMode)
        {
            if (u.powerupScript.powerupDrop == PowerupManager.Powerups.Spikes) yield return StartCoroutine(UseSpikesEasyMode());
            else if (u.powerupScript.powerupDrop == PowerupManager.Powerups.Hijack) yield return StartCoroutine(UseHijackEasyMode());
            else u.powerupScript.isPowerupButtonPressed = true;
        }
        else
        {
            switch (u.powerupScript.powerupDrop)
            {
                case PowerupManager.Powerups.Bumper:
                    yield return StartCoroutine(UseBumper());
                    break;
                case PowerupManager.Powerups.Grapple:
                    yield return StartCoroutine(UseGrapple());
                    break;
                case PowerupManager.Powerups.Hijack:
                    yield return StartCoroutine(UseHijack());
                    break;
                case PowerupManager.Powerups.Plunger:
                    yield return StartCoroutine(UsePlunger());
                    break;
                case PowerupManager.Powerups.Spikes:
                    yield return StartCoroutine(UseSpikes());
                    break;
                case PowerupManager.Powerups.Teleport:
                    yield return StartCoroutine(UseTeleport());
                    break;
                default:
                    u.powerupScript.isPowerupButtonPressed = true;
                    break;
            }
        }

        yield return null;

        u.powerupScript.isPowerupButtonPressed = false;
        u.usingPowerup = false;
    }

    // uses when we have a direct shot only
    private IEnumerator UseBumper()
    {
        while (Mathf.Abs(u.ShotAngleValue()) != 0f || Mathf.Abs(u.AngleToBall()) > 10f || u.DistanceToBall() > 8f || u.DistanceToBall() < 2f)
            yield return null;

        u.powerupScript.isPowerupButtonPressed = true;
    }

    private IEnumerator UseGrapple()
    {
        while (Mathf.Abs(u.ShotAngleValue()) != 0f && (!u.BallOnTargetMyNet() || u.GoingToOwnGoal()) || (u.DistanceToBall() < 1f && Mathf.Abs(u.AngleToBall()) > 5f)) 
            yield return null;

        u.powerupScript.isPowerupButtonPressed = true;

        while (!u.powerupScript.usingPowerup)
            yield return null;

        u.stopMainLogic = true;

        while (u.powerupScript.usingPowerup)
        {
            SteerToTarget(u.ball.position, true);
            yield return null;
        }

        u.stopMainLogic = false;
        StartCoroutine(BotLogic());
    }

    private IEnumerator UseHijackEasyMode()
    {
        u.powerupScript.isPowerupButtonPressed = true;

        while (!u.powerupScript.usingPowerup)
            yield return null;

        HijackBallPowerup hijackScript = GetComponent<HijackBallPowerup>();

        while (u.powerupScript.usingPowerup)
        {
            hijackScript.horInputValue = u.side;

            yield return null;
        }
    }
    private IEnumerator UseHijack()
    {
        bool hijack = false;
        while (!hijack)
        {
            if ((u.ball.position.x - 2f * u.side) * u.side > u.opponent.position.x * u.side || u.OpponentBoostAmount() == 0 || (u.BallOnTargetMyNet() && !u.MeOnside()))
                hijack = true;
            yield return null;
        }

        u.powerupScript.isPowerupButtonPressed = true;

        while (!u.powerupScript.usingPowerup)
            yield return null;

        HijackBallPowerup hijackScript = GetComponent<HijackBallPowerup>();


        float dir = u.ball.position.y > 0f ? -1f : 1f;
        while (u.powerupScript.usingPowerup)
        {
            float vertValue = 0f;
            if (u.ball.position.y + u.BallVel().y > 1f) vertValue = -1f;
            else if (u.ball.position.y + u.BallVel().y < -1f) vertValue = 1f;
            float horValue = u.side;


            RaycastHit2D hit = Physics2D.CircleCast(u.ball.position + (u.opponentGoalPos - u.ball.position).normalized, 1f, u.opponentGoalPos, Mathf.Infinity, LayerMask.GetMask("Player"));

            if (hit && u.opponentrb.velocity != Vector2.zero)
            {
                //Vector2 oppDir = (u.opponent.position - u.ball.position).normalized;
                //Vector2 perpDir = new Vector2(-oppDir.y, oppDir.x) * Mathf.Sign(u.opponentrb.velocity.y) * -u.side;

                //vertValue = Mathf.Sign(perpDir.y);
                if (u.ball.position.y > 4f) dir = -1f;
                if (u.ball.position.y < -4f) dir = 1f;
                vertValue = dir;
            }

            hijackScript.vertInputValue = vertValue;
            hijackScript.horInputValue = horValue;

            yield return null;
        }
    }

    private IEnumerator UsePlunger()
    {
        while (u.self.position.x * u.side < u.ball.position.x * u.side)
        {
            yield return null;
        }
        u.stopMainLogic = true;
        u.powerupScript.isPowerupButtonPressed = false;

        while (!u.powerupScript.isPowerupButtonPressed)
        {
            SteerToTarget(u.opponentGoalPos);
            u.Thrust();
            u.UseBoost();

            float plungerRange = GameInfo.plungerRange != -1 ? GameInfo.plungerRange : 9999f;
            if      ((u.self.position - u.opponentGoalPos).magnitude < 2f 
                ||  (u.DistanceToBall() > plungerRange * 0.5f && !u.MeOnside())
                ||  (u.BallOnTargetMyNet() && (u.self.position.x + 2f * u.side) * u.side > u.ball.position.x * u.side)
                ||  (u.DistanceMyGoalToBall() < 5f && (u.self.position.x + 2f * u.side) * u.side > u.ball.position.x * u.side)
                ||  (u.BallVel().x * u.side < -7f && (u.self.position.x + 2f * u.side) * u.side > u.ball.position.x * u.side))
            {
                u.powerupScript.isPowerupButtonPressed = true;
            }
            yield return null;
        }

        while (u.powerupScript.usingPowerup)
        {
            SteerToTarget(u.opponentGoalPos);
            u.Thrust();
            yield return null;
        }
        u.stopMainLogic = false;
        StartCoroutine(BotLogic());
    }

    private IEnumerator UseSpikesEasyMode()
    {
        bool exit = false;

        u.powerupScript.isPowerupButtonPressed = true;

        SpikesBallScript ballScript = FindObjectOfType<SpikesBallScript>();
        while (ballScript == null)
        {
            ballScript = FindObjectOfType<SpikesBallScript>();
            yield return null;
        }

        u.powerupScript.isPowerupButtonPressed = false;

        while (!ballScript.firstTouch && !exit)
        {
            if (ballScript.playerNumber != u.m.playerNumber)
                exit = true;

            SteerToTarget(u.ball.position, true);
            u.Thrust();

            yield return null;
        }

        u.stopMainLogic = true;

        while (u.powerupScript.usingPowerup && !exit)
        {
            SteerToTarget(u.opponentGoalPos);
            u.Thrust();
            if (Mathf.Abs(u.AngleToTarget(u.opponentGoalPos)) < 10f)
                u.UseBoost();

            yield return null;
        }
        u.stopMainLogic = false;
        StartCoroutine(BotLogic());
    }
    private IEnumerator UseSpikes()
    {
        bool exit = false;

        while (u.DistanceToBall() > 1.5f || Mathf.Abs(u.AngleToBall()) > 10f || u.ForwardVelocity() < u.rb.velocity.magnitude * 0.7f)
        {
            if (u.DistanceToBall() < 3f && Mathf.Abs(u.AngleToBall()) < 90f)
            {
                if (!u.stopMainLogic)
                {
                    u.stopMainLogic = true;
                }
                SteerToTarget(u.ball.position, true);
                u.Thrust();
            }
            else if (u.stopMainLogic)
            {
                u.stopMainLogic = false;
                StartCoroutine(BotLogic());
            }

            yield return null;
        }

        u.powerupScript.isPowerupButtonPressed = true;

        SpikesBallScript ballScript = FindObjectOfType<SpikesBallScript>();
        while (ballScript == null)
        {
            ballScript = FindObjectOfType<SpikesBallScript>();
            yield return null;
        }

        u.powerupScript.isPowerupButtonPressed = false;

        while (!ballScript.firstTouch && !exit)
        {
            if (ballScript.playerNumber != u.m.playerNumber)
                exit = true;

            SteerToTarget(u.ball.position, true);
            u.Thrust();

            yield return null;
        }

        u.stopMainLogic = true;

        float previousRotation = u.rb.rotation;
        float totalRotation = 0f;
        float turnDir;
        float angleToOpponentGoal = u.AngleToTarget(u.opponentGoalPos);
        if (angleToOpponentGoal > 0f)
            turnDir = -1f;
        else
            turnDir = 1f;

        bool closeEnough = false;
        while (u.powerupScript.usingPowerup && !exit)
        {
            float currentRotation = u.rb.rotation;
            totalRotation += Mathf.Abs(currentRotation - previousRotation);
            previousRotation = currentRotation;

            bool oppBlocking = OpponentBlocking();

            if (closeEnough)
            {
                u.Turn(turnDir);
            }
            else if ((u.DistanceTheirGoalToBall() < 12f && !oppBlocking) || (oppBlocking && u.TheirDistanceToBall() < 7f) || u.TheirDistanceToBall() < 3f)
                closeEnough = true;
            else
            {
                SteerToTarget(u.opponentGoalPos);
                if (Mathf.Abs(u.AngleToTarget(u.opponentGoalPos)) < 90f)
                    u.Thrust();
                if (Mathf.Abs(u.AngleToTarget(u.opponentGoalPos)) < 10f)
                    u.UseBoost();
            }

            if (totalRotation > 30f && u.CompareBallVelocity(u.opponentGoalPos) < 5f && closeEnough)
            {
                u.powerupScript.isPowerupButtonPressed = true;
            }

            yield return null;
        }
        u.stopMainLogic = false;
        StartCoroutine(BotLogic());
    }

    private IEnumerator UseTeleport()
    {
        bool teleport = false;

        while (!teleport)
        {
            teleport = (u.MeOnside() && u.ThemOnside() && u.BallVel().x * u.side > 2f)
                    || ((u.BallOnTargetMyNet() || u.ImminentDanger()) && !u.MeOnside() && u.TheirDistanceToBall() < u.DistanceToBall());

            yield return null;
        }

        u.powerupScript.isPowerupButtonPressed = true;
    }

    #endregion
}
