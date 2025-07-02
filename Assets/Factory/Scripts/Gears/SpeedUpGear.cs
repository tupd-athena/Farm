using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Factory;
using UnityEngine;

public class SpeedUpGear : MonoBehaviour
{
    private float _coolDown = 0;
    private float _coolDownMax = 0;
    public GearController gearController;
    private Tween tween;

    public void Start()
    {
        gearController = GetComponent<GearController>();
        gearController.OnFillComplete += SetCoolDown;
        gearController.OnDestroy += Destroy;
    }

    private void Destroy()
    {
        gearController.OnFillComplete -= SetCoolDown;
        gearController.OnDestroy -= Destroy;
        this.enabled = false;
    }

    private void SetCoolDown()
    {
        _coolDown = 0;
        _coolDownMax = gearController.gearData.customValues.Find(x => x.id == "coolDown").customValue;
        CustomValueManager.Instance.AddCustomValueInGame(
            CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SPEEDUP,
            gearController.gearData.customValues.Find(x => x.id == "multiplier").customValue
        );
        gearController.isNotAddTickValue = true;
        tween?.Kill();
        tween = DOVirtual
            .Float(
                0,
                1,
                _coolDownMax,
                (x) =>
                {
                    _coolDown = x;
                    gearController.FillItemIcon(1 - _coolDown);
                }
            ).OnStart(() =>
            {
                gearController.FillItemIcon(1);
                gearController.isNotAddTickValue = true;
            })
            .OnComplete(() =>
            {
                gearController.FillItemIcon(0);
                CustomValueManager.Instance.RemoveCustomValueInGame(
                    CustomValueManager.MULTIPLIER_HEAD_GEAR_BY_SPEEDUP,
                    gearController.gearData.customValues.Find(x => x.id == "multiplier").customValue
                );
                gearController.isNotAddTickValue = false;
            });
    }
}
