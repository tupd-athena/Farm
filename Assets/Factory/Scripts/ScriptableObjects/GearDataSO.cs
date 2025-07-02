using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "GearDataSO", menuName = "GearDataSO")]
    public class GearDataSO : ScriptableObject
    {
        public List<GearData> gearDataList;
        public List<GearBaseColor> gearBaseColors;

        void OnValidate()
        {
            foreach (var gearData in gearDataList)
            {
                if (gearData.level < 1)
                {
                    gearData.level = 1;
                }
                var gearBaseColor = gearBaseColors.Find(g => g.type == gearData.gearBaseColorType);
                if (gearBaseColor != null)
                {
                    gearData.gearBaseColor = gearBaseColor;
                }
            }
        }
    }

    public enum GearType
    {
        Text,
        Image,
        Food,
        HeadGear,
        Modifier,
        Special,
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
        public float size = 56;
        public GearBaseColorType gearBaseColorType = GearBaseColorType.TEXT1;

        [System.NonSerialized]
        public GearBaseColor gearBaseColor;
        public List<GearType> gearTypes = new List<GearType>();

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
            gearTypes = other.gearTypes;
            size = other.size;
            gearBaseColorType = other.gearBaseColorType;
            gearBaseColor = other.gearBaseColor;
        }
    }

    [System.Serializable]
    public class GearDataCustomValue
    {
        public string id;
        public float customValue;
    }

    public enum GearBaseColorType
    {
        TEXT1,
        TEXT2,
        TEXT3,
        IMAGE1,
        IMAGE2,
        IMAGE3,
        IMAGE4,
        IMAGE5,
    }

    [System.Serializable]
    public class GearBaseColor
    {
        public GearBaseColorType type;
        public Sprite sprite;
    }
}
