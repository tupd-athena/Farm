using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayTracking : MonoBehaviour
{
    public static GamePlayTracking Instance;

    // PlayerPrefs keys for saving run statistics
    public const string HIGHEST_DAY_KEY = "HighestDay";
    public const string HIGHEST_FISH = "HighestFish";
    public const string HIGHEST_GOLD = "HighestGold";
    public const string HIGHEST_BOSSES = "HighestBosses";

    public const string TOTAL_DAYS = "TotalDays";
    public const string TOTAL_FISH = "TotalFish";
    public const string TOTAL_GOLD_COLLECTED = "TotalGoldCollected";
    public const string TOTAL_BOSSES_DEFEATED = "TotalBossesDefeated";

    public const string TOTAL_GOLD_SPENT = "TotalGoldSpent";
    public const string TOTAL_FISH_DEAD = "TotalFishDead";

    // Current run statistics
    public const string CURRENT_DAY = "CurrentDay";
    public const string CURRENT_FISH = "CurrentFish";
    public const string CURRENT_GOLD = "CurrentGold";
    public const string CURRENT_BOSSES = "CurrentBosses";

    // Additional tracking constants
    public const string TOTAL_TIME = "TotalTime";
    public const string FED_FISH_COUNT = "FedFishCount";
    public const string STARVED_FISH_COUNT = "StarvedFishCount";
    public const string TOTAL_COINS = "TotalCoins";
    public const string TICKET_AMOUNT = "TicketAmount";
    public const string DIAMOND_AMOUNT = "DiamondAmount";

    [Header("Current PlayerPrefs Values - Highest Stats")]
    [SerializeField]
    private int highestDay;

    [SerializeField]
    private int highestFish;

    [SerializeField]
    private int highestGold;

    [SerializeField]
    private int highestBosses;

    [Header("Current PlayerPrefs Values - Total Stats")]
    [SerializeField]
    private int totalDays;

    [SerializeField]
    private int totalFish;

    [SerializeField]
    private int totalGoldCollected;

    [SerializeField]
    private int totalBossesDefeated;

    [SerializeField]
    private int totalGoldSpent;

    [SerializeField]
    private int totalFishDead;

    [Header("Current PlayerPrefs Values - Current Run")]
    [SerializeField]
    private int currentDay;

    [SerializeField]
    private int currentFish;

    [SerializeField]
    private int currentGold;

    [SerializeField]
    private int currentBosses;

    [Header("Debug Controls")]
    [Space(10)]
    public bool refreshValues = false;
    public bool clearAllPlayerPrefs = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateDisplayValues();
    }

    void Update()
    {
        // Update display values every frame for real-time viewing in Inspector
        // UpdateDisplayValues();
    }

    private void UpdateDisplayValues()
    {
        // Update highest stats display
        highestDay = PlayerPrefs.GetInt(HIGHEST_DAY_KEY, 0);
        highestFish = PlayerPrefs.GetInt(HIGHEST_FISH, 0);
        highestGold = PlayerPrefs.GetInt(HIGHEST_GOLD, 0);
        highestBosses = PlayerPrefs.GetInt(HIGHEST_BOSSES, 0);

        // Update total stats display
        totalDays = PlayerPrefs.GetInt(TOTAL_DAYS, 0);
        totalFish = PlayerPrefs.GetInt(TOTAL_FISH, 0);
        totalGoldCollected = PlayerPrefs.GetInt(TOTAL_GOLD_COLLECTED, 0);
        totalBossesDefeated = PlayerPrefs.GetInt(TOTAL_BOSSES_DEFEATED, 0);
        totalGoldSpent = PlayerPrefs.GetInt(TOTAL_GOLD_SPENT, 0);
        totalFishDead = PlayerPrefs.GetInt(TOTAL_FISH_DEAD, 0);

        // Update current run stats display
        currentDay = PlayerPrefs.GetInt(CURRENT_DAY, 1);
        currentFish = PlayerPrefs.GetInt(CURRENT_FISH, 0);
        currentGold = PlayerPrefs.GetInt(CURRENT_GOLD, 0);
        currentBosses = PlayerPrefs.GetInt(CURRENT_BOSSES, 0);
    }

    public void AddByKey(string key, int value)
    {
        int currentValue = PlayerPrefs.GetInt(key, 0);
        PlayerPrefs.SetInt(key, currentValue + value);
    }

    public void SetByKey(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        Save();
        UpdateDisplayValues();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(CURRENT_FISH, PlayerPrefs.GetInt(FED_FISH_COUNT, 0));
        PlayerPrefs.Save();
        if (PlayerPrefs.GetInt(HIGHEST_DAY_KEY, 0) < PlayerPrefs.GetInt(CURRENT_DAY, 1))
        {
            PlayerPrefs.SetInt(HIGHEST_DAY_KEY, PlayerPrefs.GetInt(CURRENT_DAY, 1));
        }
        if (PlayerPrefs.GetInt(HIGHEST_FISH, 0) < PlayerPrefs.GetInt(CURRENT_FISH, 0))
        {
            PlayerPrefs.SetInt(HIGHEST_FISH, PlayerPrefs.GetInt(CURRENT_FISH, 0));
        }
        if (PlayerPrefs.GetInt(HIGHEST_GOLD, 0) < PlayerPrefs.GetInt(CURRENT_GOLD, 0))
        {
            PlayerPrefs.SetInt(HIGHEST_GOLD, PlayerPrefs.GetInt(CURRENT_GOLD, 0));
        }
        if (PlayerPrefs.GetInt(HIGHEST_BOSSES, 0) < PlayerPrefs.GetInt(CURRENT_BOSSES, 0))
        {
            PlayerPrefs.SetInt(HIGHEST_BOSSES, PlayerPrefs.GetInt(CURRENT_BOSSES, 0));
        }
    }

    public void UpdateTotalStats()
    {
        AddByKey(TOTAL_DAYS, PlayerPrefs.GetInt(CURRENT_DAY, 1));
        AddByKey(TOTAL_FISH, PlayerPrefs.GetInt(CURRENT_FISH, 0));
        AddByKey(TOTAL_GOLD_COLLECTED, PlayerPrefs.GetInt(CURRENT_GOLD, 0));
        AddByKey(TOTAL_BOSSES_DEFEATED, PlayerPrefs.GetInt(CURRENT_BOSSES, 0));
        AddByKey(
            TOTAL_GOLD_SPENT,
            PlayerPrefs.GetInt(CURRENT_GOLD, 0) - PlayerPrefs.GetInt(CURRENT_GOLD, 0)
        );
        AddByKey(
            TOTAL_FISH_DEAD,
            PlayerPrefs.GetInt(CURRENT_FISH, 0) - PlayerPrefs.GetInt(CURRENT_FISH, 0)
        );
    }

    public int GetValueByKey(string key)
    {
        return PlayerPrefs.GetInt(key, 0);
    }

    public void TrackGold(int gold)
    {
        if (gold > 0)
        {
            GamePlayTracking.Instance.AddByKey(GamePlayTracking.CURRENT_GOLD, gold);
            PlayerPrefs.SetInt(TOTAL_COINS, PlayerPrefs.GetInt(CURRENT_GOLD, 0));
        }
        else
        {
            GamePlayTracking.Instance.AddByKey(GamePlayTracking.TOTAL_GOLD_SPENT, gold);
        }
    }

    void OnValidate()
    {
        if (refreshValues)
        {
            refreshValues = false;
            UpdateDisplayValues();
        }

        if (clearAllPlayerPrefs)
        {
            clearAllPlayerPrefs = false;
            ClearAllPlayerPrefs();
        }
    }

    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        UpdateDisplayValues();
        Debug.Log("All PlayerPrefs cleared!");
    }

    public void ResetCurrentRunStats()
    {
        PlayerPrefs.SetInt(CURRENT_DAY, 1);
        PlayerPrefs.SetInt(CURRENT_FISH, 0);
        PlayerPrefs.SetInt(CURRENT_GOLD, 0);
        PlayerPrefs.SetInt(CURRENT_BOSSES, 0);
        PlayerPrefs.SetInt(FED_FISH_COUNT, 0);
        PlayerPrefs.SetInt(STARVED_FISH_COUNT, 0);
        PlayerPrefs.SetInt(TOTAL_COINS, 0);
        PlayerPrefs.SetInt(TICKET_AMOUNT, 0);
        PlayerPrefs.SetInt(DIAMOND_AMOUNT, 0);
        Save();
        UpdateDisplayValues();
    }

    // Methods for EndGamePopupController
    public string GetTotalTime()
    {
        float totalTime =
            PlayerPrefs.GetFloat("GameEndTime", 0f) - PlayerPrefs.GetFloat("GameStartTime", 0f);
        int minutes = Mathf.FloorToInt(totalTime / 60f);
        int seconds = Mathf.FloorToInt(totalTime % 60f);
        return $"{minutes}M {seconds}S";
    }

    public void OnDestroy()
    {
        Save();
    }
}
