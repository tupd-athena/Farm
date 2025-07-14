using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Factory;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemCard : MonoBehaviour, IPointerClickHandler
{
    public Image cardImage;
    public Image iconImage;
    public Image glowImage;
    public InventoryItemData itemData;
    public ParticleSystem sparkleEffect;
    public ParticleSystem sparkleExplosionEffect;
    public bool isCoin = false;

    public bool canClick = false;

    void OnEnable()
    {
        transform.localScale = Vector3.zero;
    }

    public void ShowCard(InventoryItemData itemData)
    {
        if (isCoin)
        {
            canClick = true;
            return;
        }
        if (cardImage != null)
        {
            cardImage.sprite = GameManager
                .Instance.GetGearRarityData(itemData.gearData.rarity)
                .icon;
        }
        if (iconImage != null)
        {
            iconImage.sprite = Resources.Load<Sprite>("Sprites/" + itemData.gearData.iconName);
            glowImage.color = GameManager
                .Instance.GetGearRarityData(itemData.gearData.rarity)
                .color;

            var sparkleMain = sparkleEffect.main;
            sparkleMain.startColor = GameManager
                .Instance.GetGearRarityData(itemData.gearData.rarity)
                .color;
            var sparkleExplosionMain = sparkleExplosionEffect.main;
            sparkleExplosionMain.startColor = GameManager
                .Instance.GetGearRarityData(itemData.gearData.rarity)
                .color;
        }
        transform
            .DOScale(Vector3.one, 0.2f)
            .OnComplete(() =>
            {
                canClick = true;
            });
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClick)
        {
            InventoryManager.Instance.OnCardClicked?.Invoke(this);
            canClick = false;
            sparkleExplosionEffect.gameObject.SetActive(true);
            sparkleEffect.Stop();
            glowImage.gameObject.SetActive(false);
            sparkleExplosionEffect.Play();
        }
    }
}
