using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class NavigationButton : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public TMP_Text buttonText;
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.yellow;
    public string ButtonName;
    public System.Action OnButtonClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Button {ButtonName} clicked.");
        Clicked();
        OnButtonClick?.Invoke();
    }
    public void Reset()
    {
        icon.transform.DOKill();
        icon.transform.DOScale(1f, 0.3f);
        buttonText.color = normalColor;
    }
    public void Clicked()
    {
        icon.transform.DOKill();
        icon.transform.DOScale(1.5f, 0.3f);
        buttonText.color = highlightedColor;
    }
}
