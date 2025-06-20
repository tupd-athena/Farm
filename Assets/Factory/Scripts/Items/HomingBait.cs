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

    public async Task Active()
    {
        System.Random random = new System.Random();
        itemController.canCollect = false;
        itemController.rb.gravityScale = 0f;
        itemController.rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().isTrigger = true;
        targetFish = null;
        // Kill any existing move tween
        itemController.moveTween?.Kill();
        transform.localEulerAngles = new Vector3(40, 0, 0);
        transform
            .DOLocalRotate(new Vector3(40, 0, random.Next(-30, 30)), 1f)
            .SetLoops(-1, LoopType.Yoyo);
        var y = random.Next(-75, -65) * 0.1f;
        var x = random.Next(-35, 35) * 0.1f;
        transform.localPosition = new Vector3(x, y, 0);
        transform.localScale = new Vector3(0, 0, 0);
        await transform.DOScale(new Vector3(1, 1, 1), 0.5f).AsyncWaitForCompletion();
        await Task.Delay(random.Next(500, 2000));
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
        if(fish == null)
        {
            Debug.LogError("Fish is null, cannot move to fish");
            FindTarget();
            return;
        }
        try
        {
            transform.DOKill();
            await transform
                .DOLocalRotate(new Vector3(0, 0, 180), 1f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InExpo)
                .AsyncWaitForCompletion();

            transform.DOScale(0, 0.3f).SetEase(Ease.InExpo);

            targetFish = fish;
            var particle = PoolSystem.Instance.GetObject("HomingBait");
            if (particle == null)
            {
                Debug.LogError("HomingBait particle not found in pool");
                return;
            }
            particle.transform.SetParent(GameManager.Instance.TempContainerUI.transform);
            particle.transform.position = transform.position;
            var attractor = PoolSystem.Instance.GetObject("Attractor");
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
            attractorComponent.maxSpeed = 0f;
            attractorComponent.onAttracted.RemoveAllListeners();
            attractorComponent.onAttracted.AddListener(() =>
            {
                if (targetFish != null)
                {
                    targetFish.Eat(itemController);
                }
                else
                {
                    Debug.LogError("Target fish is null");
                }
            });
            particle.SetActive(true);
            particle.GetComponentInChildren<ParticleSystem>().Play();
            await Task.Delay(500);
            attractorComponent.maxSpeed = .2f;
            await Task.Delay(2000);
            attractorComponent.RemoveParticleSystem(
                particle.GetComponentInChildren<ParticleSystem>()
            );
            PoolSystem.Instance.ReturnObject(attractor, "Attractor");
            PoolSystem.Instance.ReturnObject(particle, "HomingBait");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in MoveToFish: " + e.Message);
        }
    }
}
