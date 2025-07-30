using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Factory
{
    public class CoinController : MonoBehaviour
    {
        public int value;

        public bool isCollected = false;
        public System.Action OnCollect;
        public System.Action OnComplete;
        public bool canClick = true;


        public ParticleSystem sparkleEffect;

        public void Awake()
        {
            var canvas = GetComponentInChildren<Canvas>();
            canvas.worldCamera = GameManager.Instance.mainCamera;
        }

        public void Active()
        {
            isCollected = false;
            transform.DOComplete();
            transform.DOLocalMoveY(GameManager.Instance.GetBottomYWithOffset(0.4f), 1f).SetEase(Ease.InSine);
            transform.DOLocalMoveX(transform.localPosition.x + Random.Range(-0.3f, 0.3f), 2f);
            if (isCollected || gameObject.activeSelf == false)
                return;
            CancelInvoke("OnClick");
            Invoke("OnClick", 5f);
        }

        public void OnClick()
        {
            if (isCollected || gameObject.activeSelf == false || !canClick)
                return;
            CancelInvoke("OnClick");
            isCollected = true;
            transform.DOComplete();
            transform
                .DOMove(GameManager.Instance.homeUI.TotalGoldText.transform.position, 0.5f)
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    OnComplete?.Invoke();
                });
            OnCollect?.Invoke();
        }
    }
}
