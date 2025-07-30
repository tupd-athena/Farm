using System;
using System.Linq;
using System.Threading.Tasks;
using Factory;
using UnityEngine;

// Tutorial Step Implementations
public class PlayButtonTutorialStep : TutorialStep
{
    public override int StepIndex => 0;
    private Action eventAction;

    public PlayButtonTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        var playButton = GameManager.Instance.homeUI.StartPopupController.playButtonRect;
        tutorialManager.ShowMask(playButton.GetComponent<RectTransform>());

        eventAction = () =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.StartPopupController.OnPlayButtonClick -= eventAction;
        };

        GameManager.Instance.homeUI.StartPopupController.OnPlayButtonClick += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.StartPopupController.OnPlayButtonClick -= eventAction;
    }
}

public class BuyGearTutorialStep : TutorialStep
{
    public override int StepIndex => 1;
    private Action eventAction;

    public BuyGearTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        tutorialManager.ShowDialog("Buy Gear by dragging it onto the board.", new Vector2(0, 0.2f));

        var gearInShop0 = GameManager.Instance.homeUI.ShopItems[0].gear;
        var gearInShop1 = GameManager.Instance.homeUI.ShopItems[1].gear;
        var headGear = GameManager.Instance.GetHeadGear();
        var neighborHeader0 = headGear.connectedGears[0];
        var neighborHeader1 = neighborHeader0.gear.connectedGears.LastOrDefault(g=> g.gear != headGear);
        if (neighborHeader1 == null)
        {
            neighborHeader1 = headGear.connectedGears[2];
        }
        tutorialManager.ShowTutorialSwipe(
            gearInShop1.GetComponent<RectTransform>().position,
            neighborHeader1.gear.GetComponent<RectTransform>().position,
            3f,
            true
        );

        neighborHeader0.gear.SetGearData(gearInShop0.gearData);
        neighborHeader0.gear.SetItemData(gearInShop0.gearData);
        neighborHeader0.gear.Show();
        gearInShop0.Hide();

        eventAction = () =>
        {
            tutorialManager.HideTutorialSwipe();
            tutorialManager.HideDialog();
            tutorialManager.NextStep();
            tutorialManager.OnTouchScreen -= eventAction;
        };

        tutorialManager.OnTouchScreen += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            tutorialManager.OnTouchScreen -= eventAction;
        tutorialManager.HideTutorialSwipe();
        tutorialManager.HideDialog();
    }
}

public class TreasuresButtonTutorialStep : TutorialStep
{
    public override int StepIndex => 2;
    private Action eventAction;
    private Action gameEndAction;

    public TreasuresButtonTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        if (GameManager.Instance.GameState.CurrentState != GameStateType.None)
        {
            gameEndAction = () =>
            {
                Debug.Log("Game ended, showing tutorial step 2");
                tutorialManager.ShowTutorialStep();
                GameManager.Instance.ChangeTab -= gameEndAction;
            };
            GameManager.Instance.ChangeTab += gameEndAction;
            return;
        }

        var treasuresButton = GameManager.Instance.homeUI.NavigationBar.NavigationButtons[2];
        tutorialManager.ShowMask(treasuresButton.GetComponent<RectTransform>());
        Debug.Log("Tutorial Step 2: Show Treasures Button");

        eventAction = () =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.NavigationBar.OnTreasuresButtonClick -= eventAction;
        };

        GameManager.Instance.homeUI.NavigationBar.OnTreasuresButtonClick += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.NavigationBar.OnTreasuresButtonClick -= eventAction;
        if (gameEndAction != null)
            GameManager.Instance.ChangeTab -= gameEndAction;
    }
}

public class RerollTicketTutorialStep : TutorialStep
{
    public override int StepIndex => 3;
    private Action eventAction;

    public RerollTicketTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        InventoryManager.Instance.inventoryData.tickets =
            InventoryManager.Instance.inventoryData.tickets > 0
                ? InventoryManager.Instance.inventoryData.tickets
                : 1;

        tutorialManager.ShowMask(GameManager.Instance.homeUI.InventoryManager.buttonReroll1Ticket);
        Debug.Log("Tutorial Step 3: Show Reroll 1 Ticket Button");

        eventAction = () =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.InventoryManager.OnButtonReroll1Ticket -= eventAction;
        };

        GameManager.Instance.homeUI.InventoryManager.OnButtonReroll1Ticket += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.InventoryManager.OnButtonReroll1Ticket -= eventAction;
    }
}

public class CardContainerTutorialStep : TutorialStep
{
    public override int StepIndex => 4;
    private Action<InventoryItemCard> eventAction;

    public CardContainerTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        var cardContainer = GameManager.Instance.homeUI.InventoryManager.cardContainer;
        tutorialManager.ShowMask(cardContainer.GetComponent<RectTransform>());
        Debug.Log("Tutorial Step 4: Show Card Container");

        eventAction = (card) =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.InventoryManager.OnCardClicked -= eventAction;
        };

        GameManager.Instance.homeUI.InventoryManager.OnCardClicked += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.InventoryManager.OnCardClicked -= eventAction;
    }
}

public class InventoryButtonTutorialStep : TutorialStep
{
    public override int StepIndex => 5;
    private Action eventAction;

    public InventoryButtonTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        var inventoryButton = GameManager.Instance.homeUI.NavigationBar.NavigationButtons[0];
        tutorialManager.ShowMask(inventoryButton.GetComponent<RectTransform>());
        Debug.Log("Tutorial Step 5: Show Inventory Button");

        eventAction = () =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.NavigationBar.OnInventoryButtonClick -= eventAction;
        };

        GameManager.Instance.homeUI.NavigationBar.OnInventoryButtonClick += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.NavigationBar.OnInventoryButtonClick -= eventAction;
    }
}

public class InventoryItemTutorialStep : TutorialStep
{
    public override int StepIndex => 6;
    private Action<InventoryItemController> eventAction;

    public InventoryItemTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        var item1 = GameManager.Instance.homeUI.InventoryManager.inventoryItemControllers[1];
        tutorialManager.ShowMask(item1.GetComponent<RectTransform>());
        Debug.Log("Tutorial Step 6: Show Inventory Item");

        eventAction = (item) =>
        {
            tutorialManager.NextStep();
            GameManager.Instance.homeUI.InventoryManager.OnItemClicked -= eventAction;
        };

        GameManager.Instance.homeUI.InventoryManager.OnItemClicked += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.InventoryManager.OnItemClicked -= eventAction;
    }
}

public class LevelUpButtonTutorialStep : TutorialStep
{
    public override int StepIndex => 7;
    private Action eventAction;

    public LevelUpButtonTutorialStep(TutorialManager manager)
        : base(manager) { }

    public override async Task ExecuteAsync()
    {
        var buttonLevelUp = GameManager.Instance.homeUI.InventoryManager.LevelUpButton;

        if (buttonLevelUp.interactable == false)
        {
            Debug.LogWarning("Level Up Button is not active, skipping tutorial step 7.");
            tutorialManager.NextStep();
            return;
        }

        tutorialManager.ShowMask(buttonLevelUp.GetComponent<RectTransform>());
        Debug.Log("Tutorial Step 7: Show Level Up Button");

        eventAction = () =>
        {
            tutorialManager.NextStep();
            tutorialManager.maskRect.gameObject.SetActive(false);
            GameManager.Instance.homeUI.InventoryManager.OnLevelUpButtonClicked -= eventAction;
        };

        GameManager.Instance.homeUI.InventoryManager.OnLevelUpButtonClicked += eventAction;
        await Task.CompletedTask;
    }

    public override void Cleanup()
    {
        if (eventAction != null)
            GameManager.Instance.homeUI.InventoryManager.OnLevelUpButtonClicked -= eventAction;
    }
}
