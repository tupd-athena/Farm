using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using UnityEngine;

public class TheTwinFood : MonoBehaviour
{
    public ItemController itemController;

    public async Task Active()
    {
        System.Random random = new System.Random();
        GetComponent<Rigidbody2D>().gravityScale = 0f; // Reduced gravity for slower fall
        GetComponent<Collider2D>().isTrigger = true;
        itemController.canCollect = true;
        itemController.isInWater = true;
        itemController.transform.DOJump(
            itemController.transform.position + new Vector3(random.Next(0, 10) * 0.1f, -0.3f, 0),
            0.5f,
            1,
            0.5f
        );
        itemController.SetSprite(
            Resources.Load<Sprite>("Sprites/" + itemController.itemData.iconName)
        );
        var twin = GameManager
            .Instance.SpawnItem(transform.position, itemController)
            .GetComponent<ItemController>();
        twin.isInWater = true;
        twin.OnDropToSurface = null;
        twin.dropCompleted = true; // Prevents further drop handling
        twin.GetComponent<Rigidbody2D>().gravityScale = 0f; // Reduced gravity for slower fall
        twin.GetComponent<Collider2D>().isTrigger = true;
        twin.canCollect = true;
        twin.isInWater = true;
        twin.SetSprite(Resources.Load<Sprite>("Sprites/" + itemController.itemData.iconName));
        twin.transform.DOJump(
            itemController.transform.position + new Vector3(-random.Next(0, 10) * 0.1f, -.3f, 0),
            0.5f,
            1,
            0.5f
        );
        await Task.Delay(400);
        twin.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        itemController.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        itemController.SetStandardDrop();
        twin.SetStandardDrop();
    }
}
