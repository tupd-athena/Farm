using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coffee.UIExtensions;
using DG.Tweening;
using Factory;
using UnityEngine;

public class HomingBait : MonoBehaviour
{
    public ItemController itemController;
    public FishController targetFish;

    void OnDisable()
    {
        Destroy(this);
    }

    public async Task Active()
    {
        System.Random random = new System.Random();
        itemController.canCollect = false;
        itemController.rb.gravityScale = 0f;
        itemController.rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().isTrigger = true;
        targetFish = null;
        await transform
            .DOMoveY(
                GameManager.Instance.GetBottomYWithOffset() + random.Next(0, 10) * 0.1f,
                5f / itemController.itemData.dropSpeed
            )
            .SetEase(Ease.InCubic)
            .AsyncWaitForCompletion();
        GetComponentInChildren<Animator>().Play("Pudding_1");
        await Task.Delay(500);
        FindTarget();
    }

    public void FindTarget()
    {
        System.Random random = new System.Random();
        var fishController = FishManager.Instance.GetRandomFish();
        if (fishController == null)
        {
            Debug.LogWarning("No fish controllers found");
            Invoke(nameof(FindTarget), random.Next(500, 2000) * 0.001f);
            return;
        }
        targetFish = fishController;
        MoveToFish(targetFish);
    }

    private async Task MoveToFish(FishController fish)
    {
        if (fish == null)
        {
            Debug.LogError("Fish is null, cannot move to fish");
            FindTarget();
            return;
        }
        try
        {
            Debug.Log("haha0");
            transform.DOKill();
            transform.DOScale(0, 0.3f).SetEase(Ease.InExpo);
            targetFish = fish;
            var particle = Instantiate(Resources.Load("Prefabs/HomingVFX") as GameObject);
            var main = particle
                .GetComponent<UIParticle>()
                .particles[0]
                .GetComponent<ParticleSystem>();
            main.emission.SetBurst(
                0,
                new ParticleSystem.Burst(
                    1,
                    GameManager.Instance.GetCustomValueForMultiplyByLevel(
                        itemController.itemData.gearId
                    )
                )
            );
            Debug.Log("haha1");
            if (particle == null)
            {
                Debug.LogError("HomingBait particle not found in pool");
                return;
            }
            particle.transform.SetParent(GameManager.Instance.TempContainerUI.transform);
            particle.transform.position = transform.position;
            GameObject attractor = Instantiate(
                Resources.Load("Prefabs/ParticleAttractor") as GameObject,
                GameManager.Instance.TempContainerUI.transform
            );
            Debug.Log("haha2");
            if (attractor == null)
            {
                Debug.LogError("Attractor not found in pool");
                return;
            }
            var attractorComponent = attractor.GetComponent<UIParticleAttractor>();
            attractorComponent.transform.SetParent(fish.transform);
            attractorComponent.transform.localPosition = Vector3.zero;
            attractorComponent.AddParticleSystem(particle.GetComponentInChildren<ParticleSystem>());
            attractorComponent.movement = UIParticleAttractor.Movement.Sphere;
            attractorComponent.maxSpeed = 0.15f;
            attractorComponent.onAttracted.RemoveAllListeners();
            Debug.Log("haha3");
            attractorComponent.onAttracted.AddListener(() =>
            {
                if (targetFish != null)
                {
                    Debug.Log("Target fish found, eating item");
                    targetFish.Eat(itemController, false);
                    targetFish.cloverVFX.Play();
                }
                else
                {
                    Debug.LogError("Target fish is null");
                }
            });
            particle.SetActive(true);
            particle.GetComponentInChildren<ParticleSystem>().Play();
            Debug.Log("haha4");
            await Task.Delay(2000);
            attractorComponent.RemoveParticleSystem(
                particle.GetComponentInChildren<ParticleSystem>()
            );
            Debug.Log("haha5");
            Destroy(particle);
            GameManager.Instance.CollectItem(itemController);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in MoveToFish: " + e.Message);
        }
    }
}
