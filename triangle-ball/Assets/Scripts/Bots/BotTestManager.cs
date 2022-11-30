using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotTestManager : MonoBehaviour
{
    //public static BotTestManager instance;

    public BotTestSituation currentSituation;
    public bool flip;

    private void Awake()
    {
        GameInfo.botPlayers = new List<int>
        {
            !flip ? 2 : 0,
            flip ? 2 : 0,
            0,
            0
        };

        //instance = this;
    }

    public void SetUpTestSituation()
    {
        PlayerManager bot;
        PlayerManager opponent;

        if (!flip)
        {
            bot = GameManager.instance.players[0];
            opponent = GameManager.instance.players[1];
        }
        else
        {
            bot = GameManager.instance.players[1];
            opponent = GameManager.instance.players[0];
            currentSituation.bot.position *= -1;
            currentSituation.bot.rotation = Quaternion.Inverse(currentSituation.bot.rotation);
            currentSituation.botVelocity *= -1;
            currentSituation.opponent.position *= -1;
            currentSituation.opponent.rotation = Quaternion.Inverse(currentSituation.opponent.rotation);
            currentSituation.opponentVelocity *= -1;
            currentSituation.ball.position *= -1;
            currentSituation.ballVelocity *= -1;

        }


        bot.SetTransform(currentSituation.bot);
        bot.GetComponent<Rigidbody2D>().velocity = currentSituation.botVelocity;
        bot.GetComponent<PlayerMovement>().ChangeBoostsAvailable(currentSituation.boostAmount, true);

        opponent.SetTransform(currentSituation.opponent);
        opponent.GetComponent<Rigidbody2D>().velocity = currentSituation.opponentVelocity;
        opponent.GetComponent<PlayerMovement>().ChangeBoostsAvailable(3, true);

        

        Transform ball = FindObjectOfType<BallManager>().transform;
        ball.position = currentSituation.ball.position;
        ball.GetComponent<Rigidbody2D>().velocity = currentSituation.ballVelocity;


    }
}
