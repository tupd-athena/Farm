using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "LevelConfigSO", menuName = "Factory/LevelConfigSO")]
    public class LevelConfigSO : ScriptableObject
    {
        public float scaleTotalHP = 1f;
        public float scaleCoinDrop = 1f;
        public List<LevelConfiguration> levelConfigs;
        public List<DayConfiguration> fixedDayConfigurations;
        public List<DayConfiguration> normalDayConfigurations;
        public List<DayConfiguration> bossDayConfigurations;
        public List<DayConfiguration> specialDayConfigurations;

        [ReadOnly]
        public FishConfigSO fishConfigSO;

        void OnValidate()
        {
            foreach (var day in fixedDayConfigurations)
            {
                int totalFishes = 0;
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                    totalFishes += fish.amount;
                }
                day.numberOfFishes = totalFishes;
                day.maxInPool = Mathf.Max(0, day.maxInPool);
            }
            foreach (var day in normalDayConfigurations)
            {
                int totalFishes = 0;
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                    totalFishes += fish.amount;
                }
                day.numberOfFishes = totalFishes;
                day.maxInPool = Mathf.Max(0, day.maxInPool);
            }
            foreach (var day in bossDayConfigurations)
            {
                int totalFishes = 0;
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                    totalFishes += fish.amount;
                }
                day.numberOfFishes = totalFishes;
                day.maxInPool = Mathf.Max(0, day.maxInPool);
            }
            foreach (var day in specialDayConfigurations)
            {
                int totalFishes = 0;
                foreach (var fish in day.fishConfigs)
                {
                    fish.fishConfig = fishConfigSO.fishConfigs.Find(f =>
                        f.fishType == fish.fishType
                    );
                    totalFishes += fish.amount;
                }
                day.numberOfFishes = totalFishes;
                day.maxInPool = Mathf.Max(0, day.maxInPool);
            }
        }

        public DayConfiguration GetDayConfig(
            DayConfiguration dayConfiguration,
            long maxTotalFishHP,
            int maxCoinDrop
        )
        {
            DayConfiguration newDayConfiguration = new DayConfiguration();
            newDayConfiguration.Copy(dayConfiguration);
            newDayConfiguration.fishConfigs = new List<FishConfigDay>();
            newDayConfiguration.fishConfigs = dayConfiguration.fishConfigs;
            int totalWeight = 0;
            foreach (var fish in newDayConfiguration.fishConfigs)
            {
                var fishConfig = new FishConfig();
                var config = fishConfigSO.fishConfigs.Find(
                    (fishConfig) => fishConfig.fishType == fish.fishType
                );
                if (config == null)
                {
                    Debug.LogError("FishConfig not found: " + fish.fishType);
                    newDayConfiguration.fishConfigs.Remove(fish);
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
            }
            if (totalWeight == 0)
            {
                Debug.LogError("Total weight is 0");
            }
            float moneyOfDay = maxTotalFishHP;
            float coinOfDay = maxCoinDrop;

            foreach (var fish in newDayConfiguration.fishConfigs)
            {
                fish.fishConfig.percentHP =
                    (fish.fishConfig.weight / (float)totalWeight) * moneyOfDay / (float)fish.amount;
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
            return newDayConfiguration;
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

        public void Copy(DayConfiguration other)
        {
            dayNumber = other.dayNumber;
            numberOfFishes = other.numberOfFishes;
            initialDayCurrency = other.initialDayCurrency;
            percentOutputCash = other.percentOutputCash;
            maxInPool = other.maxInPool;
            fishConfigs = new List<FishConfigDay>();
            foreach (var fish in other.fishConfigs)
            {
                var newFish = new FishConfigDay();
                newFish.fishType = fish.fishType;
                newFish.amount = fish.amount;
                newFish.fishConfig = fish.fishConfig;
                fishConfigs.Add(newFish);
            }
        }
    }
}
