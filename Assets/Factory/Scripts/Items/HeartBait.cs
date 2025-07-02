using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Factory;
using UnityEngine;

public class HeartBait : MonoBehaviour
{
    public ItemController itemController;

    public async Task Active()
    {
        // await Task.Delay(2000);
        System.Random random = new System.Random();
        Debug.Log("Active");
        itemController.canCollect = false;
        GetComponent<Rigidbody2D>().gravityScale = 0f;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Collider2D>().isTrigger = true;
        itemController.moveTween?.Kill();
        var gearData = GameManager.Instance.GetGearDataByID(itemController.itemData.gearId);
        if (gearData == null)
        {
            Debug.LogError("Gear data not found");
            return;
        }
        var radius = gearData.customValues.FirstOrDefault(v => v.id == "radius").customValue;
        var depth = gearData.customValues.FirstOrDefault(v => v.id == "depth").customValue;
        var duration = gearData.customValues.FirstOrDefault(v => v.id == "duration").customValue;
        depth += random.Next(-10, 10) * 0.01f;
        float speed = 0.3f / itemController.itemData.dropSpeed;
        itemController
            .transform.DOLocalMoveY(depth, speed);
        await Task.Delay((int)(speed * random.Next(500, 800)));
        await Explode(radius, duration);
    }

    public async Task Explode(float radius, float duration)
    {
        var vfx = PoolSystem.Instance.GetObject("HeartBait");
        if (vfx == null)
        {
            Debug.LogError("HeartBait not found in pool");
            return;
        }
        vfx.transform.SetParent(FishManager.Instance._fishParent.transform);
        vfx.SetActive(true);
        var heartBaitVFX = vfx.GetComponent<HeartBaitVFX>();
        heartBaitVFX.PlayVFX(transform.localPosition, duration, radius, 1);
        itemController.transform.DOScale(Vector3.zero, 0.3f);
        await Task.Delay((int)(duration * 200));
        var colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Fish"))
            {
                collider.GetComponent<FishController>().Eat(itemController);
                collider.GetComponent<FishController>().heartVFX.Play();
            }
        }
        GameManager.Instance.CollectItem(itemController);
    }
}
