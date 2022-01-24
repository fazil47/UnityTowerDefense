using UnityEditor;
using UnityEngine;

public class LightingTower : Tower
{
    [SerializeField, Range(1f, 100f)] float damagePerSecond = 10f;
    [SerializeField] Transform electrodeCentre, lightning = default;

    private TargetPoint _target;
    private Vector3 _lightningScale;

    public override TowerType TowerType => TowerType.Lightning;

    public override void GameUpdate()
    {
        if (TrackTarget(ref _target) || AcquireTarget(out _target))
        {
            Shoot();
        }
        else
        {
            lightning.localScale = Vector3.zero;
        }
    }

    private void Awake()
    {
        _lightningScale = lightning.localScale;
    }

    private void Shoot()
    {
        Vector3 point = _target.Position;
        electrodeCentre.LookAt(point);
        lightning.localRotation = electrodeCentre.localRotation;
        // lightning.LookAt(point);

        float d = Vector3.Distance(electrodeCentre.position, point);
        _lightningScale.z = d / 2;
        lightning.localScale = _lightningScale;
        lightning.localPosition =
            electrodeCentre.localPosition;
        _target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}