using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BankArtifact : MonoBehaviour
{
    public GameObject bubble;
    public TMP_Text goldText;
    public int goldAmount;
    public bool isHidden = true;
    public bool isClickable = true;

    private void Start()
    {
        goldAmount = 0;
        goldText.text = goldAmount.ToString();
        bubble.transform.localScale = Vector3.zero;

        FishManager.Instance.OnFishFull += (fish) =>
        {
            AddGold(1);
        };
    }
    void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        goldAmount = 0;
        goldText.text = goldAmount.ToString();
    }

    public void AddGold(int amount)
    {
        goldAmount += amount;
        goldText.text = goldAmount.ToString();
        bubble.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack);
    }

    public void Click()
    {
        if (!isClickable)
        {
            Debug.Log("BankArtifact is not clickable or already hidden.");
            return;
        }
        Debug.Log("BankArtifact clicked, hiding bubble.");
        isClickable = false;
        SpawnCoins();
    }

    public async Task SpawnCoins()
    {
        int count = Mathf.Clamp(goldAmount, 0, 10);
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(100);
            var coin = PoolSystem.Instance.GetObject("Coin");
            coin.transform.position = transform.position;

            coin.SetActive(true);
            coin.GetComponent<CoinController>().value = goldAmount / count;
            coin.GetComponent<CoinController>().OnComplete = () =>
            {
                coin.GetComponent<CoinController>().value += (int)
                    CustomValueManager.Instance.GetCustomValueInGame(
                        CustomValueManager.FISH_GOLD_BONUS
                    );
                FishManager.Instance.SpawnTextFloating(coin.GetComponent<CoinController>().value.ToString());
                GameManager.Instance.AddGold(Mathf.Clamp(coin.GetComponent<CoinController>().value, 1, 999999));
                PoolSystem.Instance.ReturnObject(coin, "Coin");
                AudioManager.Instance.PlaySound("Coin");
            };
            coin.GetComponent<CoinController>().Active();
        }
        goldAmount = 0;
        goldText.text = goldAmount.ToString();
        isClickable = true;
        HideBubble();
    }

    public void HideBubble()
    {
        bubble.transform.DOKill();
        bubble
            .transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                isHidden = true;
            });
    }
}
