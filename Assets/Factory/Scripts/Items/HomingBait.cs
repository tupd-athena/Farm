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
        Debug.Log("Active");
        itemController.canCollect = false;
        GetComponent<Rigidbody2D>().gravityScale = 0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Collider2D>().isTrigger = true;
        targetFish = null;
        // Kill any existing move tween
        itemController.moveTween?.Kill();
        await itemController.transform.DOLocalMoveY(-7.6f, 1f).AsyncWaitForCompletion();
        var fish = FishManager.Instance.GetHungriest();
        if (fish != null)
        {
            MoveToFish(fish);
        }
        else
        {
            Debug.Log("No fish found");
            targetFish = null;
            itemController.canCollect = true;
            GetComponent<Rigidbody2D>().gravityScale = 0.5f;
        }
    }

    private async Task MoveToFish(FishController fish)
    {
        targetFish = fish;
        // itemController
        //     .transform.DOLocalJump(targetFish.transform.localPosition, -1f, 1, 0.3f)
        //     .OnComplete(() =>
        //     {
        //         targetFish.Eat(itemController);
        //     });
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
        attractorComponent.RemoveParticleSystem(particle.GetComponentInChildren<ParticleSystem>());
        PoolSystem.Instance.ReturnObject(attractor, "Attractor");
        PoolSystem.Instance.ReturnObject(particle, "HomingBait");
    }
}
