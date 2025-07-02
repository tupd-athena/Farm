using System.Collections;
using System.Collections.Generic;
using Factory;
using UnityEngine;
using UnityEngine.EventSystems;

public class GearTrigger : MonoBehaviour, IDropHandler
{
    public GearController gearController;

    public void OnDrop(PointerEventData eventData)
    {
        if (GameManager.Instance.GameState.CurrentState == GameStateType.Main)
            return;

        var droppedGear = eventData.pointerDrag.GetComponent<GearController>();
        if (!droppedGear)
            return;

        if (
            gearController.isHead
            || gearController.isInShop
            || droppedGear == this
            || droppedGear.gearData == null
        )
            return;

        if (CanMergeAmplifierGears(droppedGear) && gearController.gearData.level < 4 && droppedGear != gearController)
        {
            if (droppedGear.isInShop)
            {
                droppedGear.OnDropShop?.Invoke(droppedGear.gearData);
            }
            MergeAmplifierGears(droppedGear);
            return;
        }
        else if (droppedGear.isInShop && gearController.gearData != null)
        {
            return;
        }

        if (
            gearController.gearData != null
            && !string.IsNullOrEmpty(gearController.gearData.itemName)
            && !string.IsNullOrEmpty(droppedGear.gearData.itemName)
        )
        {
            SwapGears(droppedGear);
        }
        else
        {
            if (droppedGear.isInShop && !gearController.isInShop)
            {
                if (!GameManager.Instance.CheckGold((int)droppedGear.gearData.cost))
                {
                    Debug.Log("Not enough gold");
                    GameManager.Instance.HomeUI.WarningGoldPanel();
                    return;
                }
                droppedGear.OnDropShop?.Invoke(droppedGear.gearData);
            }
            TransferGear(droppedGear);
        }
        GameManager.Instance.CheckFirstOpenShop();
    }

    protected bool CanMergeAmplifierGears(GearController otherGear)
    {
        if (
            GameManager.Instance.GameState.CurrentState == GameStateType.Shop
            && otherGear.isInShop
            && !gearController.isInShop
            && !GameManager.Instance.CheckGold((int)otherGear.gearData.cost)
        )
        {
            Debug.Log("Not enough gold");
            GameManager.Instance.HomeUI.WarningGoldPanel();
            return false;
        }
        return gearController.gearData != null
            && otherGear.gearData.id == gearController.gearData.id
            && gearController.gearData.id == 0
            && otherGear.gearData.level == gearController.gearData.level
            && otherGear.gearData.gearTypes.Contains(GearType.Text)
            && otherGear != gearController;
    }

    protected void MergeAmplifierGears(GearController otherGear)
    {
        Debug.Log("MergeAmplifierGears");
        gearController.gearData.level++;
        gearController.LevelText.text = gearController.gearData.level.ToString();
        otherGear.Hide();
        gearController.UpdateGearIconLevel();
    }

    protected void SwapGears(GearController otherGear)
    {
        GearData tempGearData = new GearData();
        tempGearData.Copy(gearController.gearData);

        gearController.SetGearData(otherGear.gearData);
        gearController.SetItemData(otherGear.gearData);
        gearController.Show();
        Debug.Log("SwapGears");
        otherGear.SetGearData(tempGearData);
        otherGear.SetItemData(tempGearData);
        otherGear.Show();
    }

    protected void TransferGear(GearController otherGear)
    {
        Debug.Log("TransferGear");
        gearController.SetGearData(otherGear.gearData);
        gearController.SetItemData(otherGear.gearData);
        gearController.Show();
        otherGear.Hide();
        gearController.GetComponent<CanvasGroup>().alpha = 1f;
    }
}
