using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Factory
{
    [CreateAssetMenu(fileName = "FishConfigSO", menuName = "Factory/FishConfigSO")]
    public class FishConfigSO : ScriptableObject
    {
        public List<FishConfig> fishConfigs;

        void OnValidate()
        {
            int index = 0;
            foreach (var fishConfig in fishConfigs)
            {
                fishConfig.fishType = (FishType)index;
                index++;
                if (index > 51)
                {
                    fishConfigs.Remove(fishConfig);
                    Debug.LogError("FishConfig duplicate: " + fishConfig.fishType);
                }
            }
        }
    }

    [System.Serializable]
    public enum FishType
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
        F = 5,
        G = 6,
        H = 7,
        I = 8,
        J = 9,
        K = 10,
        L = 11,
        M = 12,
        N = 13,
        O = 14,
        P = 15,
        Q = 16,
        R = 17,
        S = 18,
        T = 19,
        U = 20,
        V = 21,
        W = 22,
        X = 23,
        Y = 24,
        Z = 25,
        A1 = 26,
        B1 = 27,
        C1 = 28,
        D1 = 29,
        E1 = 30,
        F1 = 31,
        G1 = 32,
        H1 = 33,
        I1 = 34,
        J1 = 35,
        K1 = 36,
        L1 = 37,
        M1 = 38,
        N1 = 39,
        O1 = 40,
        P1 = 41,
        Q1 = 42,
        R1 = 43,
        S1 = 44,
        T1 = 45,
        U1 = 46,
        V1 = 47,
        W1 = 48,
        X1 = 49,
        Y1 = 50,
        Z1 = 51,
    }

    [System.Serializable]
    public class FishConfig
    {
        [ReadOnly]
        public FishType fishType;
        public int weight;
        [ReadOnly]
        public int fishCurrencyValue;
        public int patienceValue;
        public int dropCoinValue = 0;
        public float percentDecrease = 0;
        public float timeDecreasePerTick = 0;

        public float percentHP = 1;

        public string fishPrefabName;

        public bool isBoss = false;

        public List<FishSpriteByHP> sprites;

        public float speed = 1;
        public float size = 1;

        [Range(-8, 0)]
        public float depth = 0;

        [Range(0, 4)]
        public float moveArea = 0;

        public void Copy(FishConfig other)
        {
            fishType = other.fishType;
            weight = other.weight;
            fishCurrencyValue = other.fishCurrencyValue;
            patienceValue = other.patienceValue;
            dropCoinValue = other.dropCoinValue;
            percentDecrease = other.percentDecrease;
            timeDecreasePerTick = other.timeDecreasePerTick;
            depth = other.depth;
            moveArea = other.moveArea;
            sprites = other.sprites;
            fishPrefabName = other.fishPrefabName;
            isBoss = other.isBoss;
            speed = other.speed;
            size = other.size;
            percentHP = other.percentHP;
        }
    }
}
