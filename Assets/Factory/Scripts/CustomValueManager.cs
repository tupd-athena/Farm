using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomValueManager : MonoBehaviour
{
    public static CustomValueManager Instance;
    public List<CustomValueInGame> customValuesInGame = new List<CustomValueInGame>();
    public const string ADD_TICK_VALUE = "addTickValue";
    public const string MULTIPLIER_TICK_VALUE = "multiplierTickValue";
    public const string MULTIPLIER_HEAD_GEAR = "multiplierHeadGear";


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
