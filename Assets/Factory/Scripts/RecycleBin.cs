using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Factory;
using UnityEngine;
using UnityEngine.EventSystems;

public class RecycleBin : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var gear = eventData.pointerDrag.GetComponent<GearController>();
        if (gear != null)
        {
            transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutSine).SetLoops(2, LoopType.Yoyo);
            GameManager.Instance.AddGold((int)(gear.gearData.cost * 0.5f));
            gear.Hide();
            GameManager.Instance.CheckGoldAllGearsInShop();
        }
    }
}
