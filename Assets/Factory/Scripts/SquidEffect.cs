using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Factory;
using UnityEngine;
using UnityEngine.U2D;

public class SquidEffect : MonoBehaviour
{
    public float range = 10f;
    public float speedScale = 1f;

    public float duration = 1f;

    public List<FishController> fishControllers = new List<FishController>();

    private float delay = 0.2f;

    private Tween _tweenColor;

    public void OnEnable()
    {
        fishControllers.Clear();
        MoveAllFishConfused();
        delay = 0.2f;
    }

    public void OnDisable()
    {
        CancelInvoke(nameof(MoveAllFishConfused));
        foreach (FishController fishController in fishControllers)
        {
            if (fishController!=null && fishController.spriteRenderer != null && fishController.state == FishState.Confused)
            {
                fishController.state = FishState.Moving;
                fishController.spriteRenderer.material.DOComplete();
                fishController.spriteRenderer.material.SetColor(
                    "_Color",
                    new Color32(255, 255, 255, 255)
                );
                fishController.Move();
            }
        }
        fishControllers.Clear();
    }

    void Update()
    {
        delay -= Time.deltaTime;
        if (delay > 0)
        {
            return;
        }
        //raycast to find fish
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(range, range),
            0,
            Vector2.zero
        );
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("Fish"))
            {
                var fishController = hit.collider.GetComponent<FishController>();
                Debug.Log("SquidEffect");
                if (fishController.state != FishState.Moving)
                {
                    return;
                }
                System.Random random = new System.Random();
                if (fishController != null && !fishControllers.Contains(fishController))
                {
                    fishController.state = FishState.Confused;
                    fishController.KillAllTween();
                    fishControllers.Add(fishController);
                }
            }
        }
    }

    public void MoveAllFishConfused()
    {
        System.Random random = new System.Random();
        foreach (
            FishController fishController in fishControllers.FindAll(fish =>
                fish.state == FishState.Confused
            )
        )
        {
            fishController.transform.DOComplete();
            var isFlip = random.Next(0, 2) == 0;
            if (_tweenColor == null)
            {
                _tweenColor = fishController
                    .spriteRenderer.material.DOColor(new Color32(72, 58, 160, 255), 0.5f)
                    .SetLoops(2, LoopType.Yoyo)
                    .OnComplete(() => _tweenColor = null);
            }
            fishController.Confuse();
            fishController.FlipWithDirection(new Vector3(isFlip ? 1 : -1, 1, 1));
        }
        Invoke(nameof(MoveAllFishConfused), 0.2f);
    }
}
