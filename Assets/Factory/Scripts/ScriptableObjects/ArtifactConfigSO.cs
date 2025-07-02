using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactConfig", menuName = "Factory/ArtifactConfigSO", order = 1)]
public class ArtifactConfigSO : ScriptableObject
{
    public List<ArtifactData> artifactDatas = new List<ArtifactData>();
    public List<ArtifactTypeData> artifactTypeDatas = new List<ArtifactTypeData>();

    public void OnValidate()
    {
    }
}

[System.Serializable]
public class ArtifactData
{
    public string artifactId;
    public string artifactName;
    public string artifactDescription;
    public int artifactLevel;
    public List<ArtifactValue> values = new List<ArtifactValue>();
    public ArtifactType artifactType;
    public ArtifactTypeData artifactTypeData;
    public int weight;

    public void CopyFrom(ArtifactData other)
    {
        artifactId = other.artifactId;
        artifactName = other.artifactName;
        artifactDescription = other.artifactDescription;
        artifactLevel = other.artifactLevel;
        values.Clear();
        foreach (var value in other.values)
        {
            ArtifactValue newValue = new ArtifactValue();
            newValue.CopyFrom(value);
            values.Add(newValue);
        }
        artifactType = other.artifactType;
        artifactTypeData = new ArtifactTypeData();
        artifactTypeData.CopyFrom(other.artifactTypeData);
        weight = other.weight;
    }

    public float GetValueByName(string valueName)
    {
        foreach (var value in values)
        {
            if (value.valueName == valueName)
            {
                return value.value;
            }
        }
        return 0f; // Return 0 if the value is not found
    }
}

[System.Serializable]
public class ArtifactValue
{
    public string valueName;
    public float value;

    public void CopyFrom(ArtifactValue other)
    {
        valueName = other.valueName;
        value = other.value;
    }
}

[System.Serializable]
public class ArtifactTypeData
{
    public ArtifactType artifactType;
    public Sprite artifactFrame;
    public int weight;
    public void CopyFrom(ArtifactTypeData other)
    {
        artifactType = other.artifactType;
        artifactFrame = other.artifactFrame;
        weight = other.weight;
    }
}
public enum ArtifactType
{
    N,
    R,
    SR,
    SSR,
    UR

}
