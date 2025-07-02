using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using UnityEngine;

public class TheTrippleFood : MonoBehaviour
{
    public ItemController itemController;

    public async Task Active()
    {
        GetComponentInChildren<Animator>().Play("Pudding");
        await Task.Delay(400); // Ensure itemController is ready
        System.Random random = new System.Random();
        GetComponent<Rigidbody2D>().gravityScale = 0f; // Reduced gravity for slower fall
        GetComponent<Collider2D>().isTrigger = true;
        itemController.canCollect = true;
        itemController.isInWater = true;
        itemController.SetSprite(
            Resources.Load<Sprite>("Sprites/" + itemController.itemData.iconName)
        );
        List<ItemController> copies = new List<ItemController>();
        for (int i = 0; i < 3; i++)
        {
            var itemDataSub = GameManager.Instance.GetItemDataByItemID(7);
            var copy = GameManager
                .Instance.SpawnItem(transform.position, itemDataSub)
                .GetComponent<ItemController>();
            copy.isInWater = true;
            copy.OnDropToSurface = null;
            copy.dropCompleted = true; // Prevents further drop handling
            copy.GetComponent<Rigidbody2D>().gravityScale = 0f; // Reduced gravity for slower fall
            copy.GetComponent<Collider2D>().isTrigger = true;
            copy.canCollect = true;
            copy.isInWater = true;
            copy.SetSprite(Resources.Load<Sprite>("Sprites/" + copy.itemData.iconName));
            copy.transform.DOJump(
                    itemController.transform.position
                        + new Vector3(-random.Next(-10, 10) * 0.1f, -.3f, 0),
                    1.5f,
                    1,
                    0.5f
                )
                .OnComplete(() =>
                {
                    var item = copy.GetComponent<ItemController>();
                    item.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    item.SetStandardDrop();
                });
            copies.Add(copy);
            await Task.Delay(100);
        }
        itemController.transform.DOScale(0, 0.5f).SetEase(Ease.InCubic);
    }
}
