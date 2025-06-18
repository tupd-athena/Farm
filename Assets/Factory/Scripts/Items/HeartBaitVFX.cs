using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Factory;
using UnityEngine;

public class HeartBaitVFX : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem mainParticle;

    [SerializeField]
    private ParticleSystem glowParticle;

    public void SetRadius(float radius = 2)
    {
        var main = glowParticle.main;
        main.startSize = radius * 2;
    }

    public async Task PlayVFX(Vector3 position, float duration = 1, float radius = 2, int cycle = 1)
    {
        SetRadius(radius);
        transform.localScale = Vector3.one;
        transform.localPosition = position;
        for (int i = 0; i < cycle; i++)
        {
            mainParticle.Play();
            await Task.Delay((int)(duration * 1000));
        }
        await Task.Delay(1000);
        mainParticle.Stop();
        PoolSystem.Instance.ReturnObject(gameObject, "HeartBait");
    }
}
