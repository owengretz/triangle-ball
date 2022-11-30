using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    public static GameInfo instance;

    //public enum Gamemode { Standard, Powerups, Boomer, Bumpers };

    public static MapInfo mapInfo;
    public static Sprite ballSkin;
    public static int specialBallIndex;
    public static bool powerupsOn;
    public static int numberOfPlayers;
    public static List<int> botPlayers = new List<int>();
    public static float matchLength;
    public static int maxScore;

    public MapInfo defaultMap;
    public Sprite defaultBallSkin;

    #region Extra Settings

    public static bool[] enabledPowerups = new bool[System.Enum.GetNames(typeof(PowerupManager.Powerups)).Length];

    public static bool powerupFairness;
    public static bool powerupEquality;
    public static bool showBotCursors;
    public static bool pentagonCanBeDemolished;
    public static bool megaCanBeDemolished;

    public static bool disableGoalReset;

    public static bool demosEnabled;
    public static bool friendlyDemosEnabled;

    public static float powerupCooldown;
    public static float respawnCooldown;
    public static float boostRespawnTime;
    public static float kickoffBoostAmount;
    public static float plungerDuration;
    public static float plungerRange;
    public static float plungerForce;
    public static float plungerLatchSpeed;
    public static float spikesDuration;
    public static float spikesReleaseSpeed;
    public static float grappleLatchSpeed;
    public static float grappleGrappleSpeed;
    public static float pentagonDuration;
    public static float clonesDuration;
    public static float clonesAmount;
    public static float bumperDuration;
    public static float bumperLaunchSpeed;
    public static float bumperBumpForce;
    public static float teleportDelay;
    public static float megaDuration;
    public static float megaMassIncrease;
    public static float hijackDuration;
    public static float hijackMoveForce;
    public static float playerThrustForce;
    public static float playerBoostForce;
    public static float playerTurnSpeed;
    public static float playerMass;
    public static float playerDrag;
    public static float playerDemolishThreshold;
    public static float playerBumpForce;
    public static float ballMass;
    public static float ballDrag;
    public static float ballBounciness;


    public static float shakeRoughness = 5f;
    public enum Shakes { Goal, Demo, Boost, Bump };
    public static Dictionary<Shakes, float> cameraShakes = new Dictionary<Shakes, float>()
    {
        { Shakes.Goal   , 2f },
        { Shakes.Demo   , 1.5f },
        { Shakes.Boost  , 0.2f },
        { Shakes.Bump, 0.5f },
    };

    #endregion

    public static bool tutorial;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            ResetDefaults();
            botPlayers = new List<int>() { 0, 0, 2, 2 };

            //powerupsOn = false;
        }
    }

    public static void SetInfo(MapInfo _mapInfo, Sprite _ballSkin, int _specialBallIndex, bool _powerupsOn, int _numberOfPlayers, List<int> _botPlayers, float _matchLength, int _maxScore)
    {
        mapInfo = _mapInfo;
        ballSkin = _ballSkin;
        specialBallIndex = _specialBallIndex;
        powerupsOn = _powerupsOn;
        numberOfPlayers = _numberOfPlayers;
        botPlayers = _botPlayers;
        matchLength = _matchLength;
        maxScore = _maxScore;
    }

    public void ResetDefaults()
    {
        mapInfo = defaultMap;
        ballSkin = defaultBallSkin;
        specialBallIndex = -1;
        powerupsOn = false;
        numberOfPlayers = 2;
        botPlayers = new List<int>() { 2, 2, 2, 2 };
        matchLength = -1f;
        maxScore = 9999;

        for (int i = 0; i < enabledPowerups.Length; i++) enabledPowerups[i] = true;

        powerupFairness = false;
        powerupEquality = false;
        showBotCursors = false;
        pentagonCanBeDemolished = true;
        megaCanBeDemolished = false;

        disableGoalReset = false;

        demosEnabled = true;
        friendlyDemosEnabled = false;

        powerupCooldown = 3f;
        //powerupCooldown = SettingsSlider.defaults[SettingsSlider.Property.PowerupCooldown];
        respawnCooldown = SettingsSlider.defaults[SettingsSlider.Property.RespawnCooldown];
        boostRespawnTime = SettingsSlider.defaults[SettingsSlider.Property.BoostRespawnTime];
        kickoffBoostAmount = SettingsSlider.defaults[SettingsSlider.Property.KickoffBoostAmount];
        plungerDuration = SettingsSlider.defaults[SettingsSlider.Property.PlungerDuration];
        plungerRange = SettingsSlider.defaults[SettingsSlider.Property.PlungerRange];
        plungerForce = SettingsSlider.defaults[SettingsSlider.Property.PlungerForce];
        plungerLatchSpeed = SettingsSlider.defaults[SettingsSlider.Property.PlungerLatchSpeed];
        spikesDuration = SettingsSlider.defaults[SettingsSlider.Property.SpikesDuration];
        spikesReleaseSpeed = SettingsSlider.defaults[SettingsSlider.Property.SpikesReleaseSpeed];
        grappleLatchSpeed = SettingsSlider.defaults[SettingsSlider.Property.GrappleLatchSpeed];
        grappleGrappleSpeed = SettingsSlider.defaults[SettingsSlider.Property.GrappleGrappleSpeed];
        pentagonDuration = SettingsSlider.defaults[SettingsSlider.Property.PentagonDuration];
        clonesDuration = SettingsSlider.defaults[SettingsSlider.Property.ClonesDuration];
        clonesAmount = SettingsSlider.defaults[SettingsSlider.Property.ClonesAmount];
        bumperDuration = SettingsSlider.defaults[SettingsSlider.Property.BumperDuration];
        bumperLaunchSpeed = SettingsSlider.defaults[SettingsSlider.Property.BumperLaunchSpeed];
        bumperBumpForce = SettingsSlider.defaults[SettingsSlider.Property.BumperBumpForce];
        teleportDelay = SettingsSlider.defaults[SettingsSlider.Property.TeleportDelay];
        megaDuration = SettingsSlider.defaults[SettingsSlider.Property.MegaDuration];
        megaMassIncrease = SettingsSlider.defaults[SettingsSlider.Property.MegaMassIncrease];
        hijackDuration = SettingsSlider.defaults[SettingsSlider.Property.HijackDuration];
        hijackMoveForce = SettingsSlider.defaults[SettingsSlider.Property.HijackMoveForce];
        playerThrustForce = SettingsSlider.defaults[SettingsSlider.Property.PlayerThrustForce];
        playerBoostForce = SettingsSlider.defaults[SettingsSlider.Property.PlayerBoostForce];
        playerTurnSpeed = SettingsSlider.defaults[SettingsSlider.Property.PlayerTurnSpeed];
        playerMass = SettingsSlider.defaults[SettingsSlider.Property.PlayerMass];
        playerDrag = SettingsSlider.defaults[SettingsSlider.Property.PlayerDrag];
        playerDemolishThreshold = SettingsSlider.defaults[SettingsSlider.Property.PlayerDemolishThreshold];
        playerBumpForce = SettingsSlider.defaults[SettingsSlider.Property.PlayerBumpForce];
        ballMass = SettingsSlider.defaults[SettingsSlider.Property.BallMass];
        ballDrag = SettingsSlider.defaults[SettingsSlider.Property.BallDrag];
        ballBounciness = SettingsSlider.defaults[SettingsSlider.Property.BallBounciness];
    }
}
