using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "LevelConfigSO", menuName = "Factory/LevelConfigSO")]
    public class LevelConfigSO : ScriptableObject
    {
        public List<LevelConfiguration> levelConfigs;
        public List<DayConfiguration> normalDayConfigurations;
        public List<DayConfiguration> bossDayConfigurations;
        public List<DayConfiguration> specialDayConfigurations;

        [ReadOnly]
        public FishConfigSO fishConfigSO;

        void OnValidate()
        {
            foreach (var day in normalDayConfigurations)
            {
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                }
            }
            foreach (var day in bossDayConfigurations)
            {
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                }
            }
            foreach (var day in specialDayConfigurations)
            {
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                }
            }
        }

        public DayConfiguration GetDayConfig(
            DayConfiguration dayConfiguration,
            long maxTotalFishHP,
            int maxCoinDrop
        )
        {
            List<FishConfigDay> fishes = dayConfiguration.fishConfigs;
            int totalWeight = 0;
            int totalFishes = 0;
            foreach (var fish in fishes)
            {
                var fishConfig = new FishConfig();
                var config = fishConfigSO.fishConfigs.Find(
                    (fishConfig) => fishConfig.fishType == fish.fishType
                );
                if (config == null)
                {
                    Debug.LogError("FishConfig not found: " + fish.fishType);
                    fishes.Remove(fish);
                    continue;
                }
                fishConfig.Copy(
                    fishConfigSO.fishConfigs.Find(
                        (fishConfig) => fishConfig.fishType == fish.fishType
                    )
                );
                if (fishConfig == null)
                {
                    Debug.LogError("FishConfig not found: " + fish.fishType);
                    continue;
                }
                fish.fishConfig.Copy(fishConfig);
                totalWeight += fishConfig.weight;
                totalFishes += fish.amount;
            }
            if (totalFishes == 0)
            {
                Debug.LogError("Total fishes is 0");
            }
            if (totalWeight == 0)
            {
                Debug.LogError("Total weight is 0");
            }
            dayConfiguration.numberOfFishes = totalFishes;
            float moneyOfDay = dayConfiguration.percentOutputCash * maxTotalFishHP / 100;
            float coinOfDay = dayConfiguration.percentOutputCash * maxCoinDrop / 100;

            foreach (var fish in fishes)
            {
                fish.fishConfig.fishCurrencyValue = Mathf.Max(
                    Mathf.RoundToInt(
                        (fish.fishConfig.weight / (float)totalWeight)
                            * moneyOfDay
                            / (float)fish.amount
                    ),
                    1
                );
                fish.fishConfig.dropCoinValue = Mathf.Max(
                    Mathf.RoundToInt(
                        (fish.fishConfig.weight / (float)totalWeight)
                            * coinOfDay
                            / (float)fish.amount
                    ),
                    1
                );
            }

            return dayConfiguration;
        }
    }

    [System.Serializable]
    public class LevelConfiguration
    {
        public int levelNumber;
        public long maxTotalFishHP = 1000;
        public int maxCoinDrop = 1000;
        public Vector2Int gridSize = new Vector2Int(7, 4);
        public int maxHeadGearSlots = 1;
        public int initialLevelCurrency = 50;
    }

    [System.Serializable]
    public class FishConfigDay
    {
        public FishType fishType;
        public int amount;

        [ReadOnly]
        public FishConfig fishConfig;
    }

    [System.Serializable]
    public class DayConfiguration
    {
        public int dayNumber;
        public int numberOfFishes = 5;

        public List<FishConfigDay> fishConfigs;
        public int initialDayCurrency = 50;
        public int percentOutputCash;
        public int maxInPool = 10;
    }
}
