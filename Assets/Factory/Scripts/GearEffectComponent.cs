using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearEffectComponent : MonoBehaviour
{
    [SerializeField]
    ParticleSystem electricEffect;
    [SerializeField]
    ParticleSystem dropEffect;

    [SerializeField]
    Transform vfxParent;

    public bool isActive = false;

    public void Start()
    {
        ShowAll();
    }

    public void HideAll()
    {
        isActive = false;
        vfxParent.gameObject.SetActive(false);
    }
    public void ShowAll()
    {
        isActive = true;
        vfxParent.gameObject.SetActive(true);
    }

    public void PlayEffect(string effectName)
    {
        switch (effectName)
        {
            case "Electric":
                electricEffect.gameObject.SetActive(true);
                electricEffect.Play();
                break;
            case "Drop":
                dropEffect.gameObject.SetActive(true);
                dropEffect.Play();
                break;
            default:
                Debug.LogWarning($"Effect '{effectName}' not recognized.");
                break;
        }
    }
    public void StopEffect(string effectName)
    {
        switch (effectName)
        {
            case "Electric":
                electricEffect.gameObject.SetActive(false);
                electricEffect.Stop();
                break;
            default:
                Debug.LogWarning($"Effect '{effectName}' not recognized.");
                break;
        }
    }
}
