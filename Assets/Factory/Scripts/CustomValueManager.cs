using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class CustomValueManager : MonoBehaviour
{
    public static CustomValueManager Instance;
    public List<CustomValueInGame> customValuesInGame = new List<CustomValueInGame>();
    public const string ADD_TICK_VALUE = "addTickValue";
    public const string MULTIPLIER_TICK_VALUE = "multiplierTickValue";
    public const string MULTIPLIER_HEAD_GEAR_BY_SPEEDUP = "multiplierHeadGearBySpeedup";
    public const string MULTIPLIER_HEAD_GEAR_BY_SCARED_TOTEM = "multiplierHeadGearByScaredTotem";
    public const string REDUCE_FISH_TICK_RATE = "reduceFishTickRate";
    public const string FISH_GOLD_BONUS = "fishGoldBonus";
    public const string HEART_BONUS = "heartBonus";


    [SerializeField, ReadOnly]
    private List<string> specialTextGear = new List<string>() { "Multiplier", "SpeedUP" };

    void Awake()
    {
        Instance = this;
    }

    public void AddCustomValueInGame(string id, float value)
    {
        var customValue = customValuesInGame.Find(x => x.id == id);
        if (customValue == null)
        {
            customValuesInGame.Add(new CustomValueInGame { id = id, customValue = value });
        }
        else
        {
            customValue.customValue += value;
        }
    }

    public void RemoveCustomValueInGame(string id, float value)
    {
        var customValue = customValuesInGame.Find(x => x.id == id);
        if (customValue != null)
        {
            customValue.customValue -= value;
        }
    }

    public void ClearCustomValueInGame()
    {
        customValuesInGame = new List<CustomValueInGame>();
    }

    public float GetCustomValueInGame(string id)
    {
        var customValue = customValuesInGame.Find(x => x.id == id);
        return customValue != null ? customValue.customValue : 0;
    }

    [System.Serializable]
    public class CustomValueInGame
    {
        public string id;
        public float customValue;
    }
}
