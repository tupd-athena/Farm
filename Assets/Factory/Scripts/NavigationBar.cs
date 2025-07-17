using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using UnityEngine;

public class NavigationBar : MonoBehaviour
{
    public List<NavigationButton> NavigationButtons = new List<NavigationButton>();
    public bool isWaitingForTabChange = false;
    public string currentTab = "Inventory";

    public void Start()
    {
        GameManager.Instance.homeUI.StartPopupController.OnPlayButtonClick = null; // Reset previous callback to avoid multiple invocations
        GameManager.Instance.homeUI.StartPopupController.OnPlayButtonClick = () =>
        {
            HideAllTabsAndBar();
            GameManager.Instance.NewGame();
        };
        foreach (var button in NavigationButtons)
        {
            switch (button.ButtonName)
            {
                case "Inventory":
                    button.OnButtonClick = () =>
                    {
                        ChangeTab("Inventory");
                    };
                    break;
                case "Play":
                    button.OnButtonClick = () =>
                    {
                        ChangeTab("Play");
                    };
                    break;
                case "Treasures":
                    button.OnButtonClick = () =>
                    {
                        ChangeTab("Treasures");
                    };
                    break;
                default:
                    button.OnButtonClick = () => Debug.Log($"{button.ButtonName} button clicked.");
                    break;
            }
        }
        ChangeTab("Play"); // Default tab
    }

    public async void ChangeTab(string tabName)
    {
        GameManager.Instance.homeUI.WavePanel.SetActive(false);
        GameManager.Instance.homeUI.GoldContainer.gameObject.SetActive(false);
        GameManager.Instance.homeUI.buttonQuit.gameObject.SetActive(false);
        GameManager.Instance.DuneObject.SetActive(false);
        GameManager.Instance.homeUI.TicketPanel.SetActive(true);
        if (isWaitingForTabChange || currentTab == tabName)
        {
            Debug.Log("Waiting for previous tab change to complete.");
            return;
        }
        ResetAlllbuttons();
        isWaitingForTabChange = true;
        NavigationButtons.Find(b => b.ButtonName == tabName).Clicked();
        switch (tabName)
        {
            case "Inventory":
                GameManager.Instance.homeUI.StartPopupController.HidePopup();
                InventoryManager.Instance.HideSpecialTreasures();
                InventoryManager.Instance.HideTreasures();
                GameManager.Instance.homeUI.ShowInventory();
                break;
            case "Play":
                InventoryManager.Instance.HideSpecialTreasures();
                InventoryManager.Instance.HideTreasures();
                GameManager.Instance.homeUI.HideInventory();
                GameManager.Instance.homeUI.StartPopupController.ShowPopup();
                break;
            case "Treasures":
                GameManager.Instance.homeUI.StartPopupController.HidePopup();
                GameManager.Instance.homeUI.HideInventory();
                InventoryManager.Instance.ShowTreasures();
                break;
            default:
                Debug.Log($"{tabName} button clicked.");
                break;
        }
        currentTab = tabName;
        await Task.Delay(100); // Simulate some delay for UI transition
        isWaitingForTabChange = false;
    }

    public void ResetAlllbuttons()
    {
        foreach (var button in NavigationButtons)
        {
            button?.Reset();
        }
    }

    public async Task HideAllTabsAndBar()
    {
        GameManager.Instance.homeUI.StartPopupController.HidePopup();
        InventoryManager.Instance.HideSpecialTreasures();
        InventoryManager.Instance.HideTreasures();
        GameManager.Instance.homeUI.HideInventory();
        GetComponent<RectTransform>().DOAnchorPosY(-300, 0.5f);
    }

    public async Task ShowAllTabsAndBar()
    {
        GetComponent<RectTransform>().DOAnchorPosY(100, 0.5f);
        await Task.Delay(500); // Wait for the bar to show
        ChangeTab("Inventory"); // Reapply the current tab
    }
}
