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
        public List<GearRarityData> gearRarityDataList;

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
                if (gearData.customValues.Count == 0 || gearData.customValues.Find(c => c.id == "level") == null)
                {
                    var levelCustomValue = new GearDataCustomValue { id = "level", customValue = 1 };
                    gearData.customValues.Add(levelCustomValue);
                }
                if(gearData.customValues.Find(c => c.id == "mult") == null)
                {
                    var levelCustomValue = new GearDataCustomValue { id = "mult", customValue = 0.5f};
                    gearData.customValues.Add(levelCustomValue);
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
        public string description = "";
        public int level = 1;
        public float baseValue;
        public float weight;
        public float size = 56;
        public bool hideInInventory = false;
        public GearRarity rarity = GearRarity.Common;
        public GearBaseColorType gearBaseColorType = GearBaseColorType.TEXT1;

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
            hideInInventory = other.hideInInventory;
            customValues = other.customValues;
            gearTypes = other.gearTypes;
            description = other.description;
            size = other.size;
            gearBaseColorType = other.gearBaseColorType;
            gearBaseColor = other.gearBaseColor;
            rarity = other.rarity;
        }

        public float GetCustomValue(string id)
        {
            if (customValues == null || customValues.Count == 0)
                return 0f;

            var customValue = customValues.Find(c => c.id == id);
            return customValue != null ? customValue.customValue : 0f;
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
    public class GearRarityData
    {
        public GearRarity rarity;
        public Color color;
        public Sprite icon;

        public void Copy(GearRarityData other)
        {
            rarity = other.rarity;
            color = other.color;
            icon = other.icon;
        }
    }

    public enum GearRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic,
    }

    [System.Serializable]
    public class GearBaseColor
    {
        public GearBaseColorType type;
        public Sprite sprite;
    }
}
