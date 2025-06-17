using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "GearDataSO", menuName = "GearDataSO")]
    public class GearDataSO : ScriptableObject
    {
        public List<GearData> gearDataList;
        void OnValidate()
        {
            foreach (var gearData in gearDataList)
            {
                if (gearData.level < 1)
                {
                    gearData.level = 1;
                }
            }
        }
    }
    [System.Serializable]
    public class GearData
    {
        public int id;
        public string itemName;
        public string iconName;
        public float tickValue;
        public float maxValue;
        public int cost;
        public int level = 1;
        public float baseValue;
        public float weight;

        public List<GearDataCustomValue> customValues;

        public void Copy(GearData other)
        {
            id = other.id;
            itemName = other.itemName;
            iconName = other.iconName;
            tickValue = other.tickValue;
            maxValue = other.maxValue;
            cost = other.cost;
            weight = other.weight;
            baseValue = other.baseValue;
            level = other.level;
            customValues = other.customValues;
        }
    }
    [System.Serializable]
    public class GearDataCustomValue
    {
        public string id;
        public float customValue;
    }
}
