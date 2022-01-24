// using UnityEditor;
// using UnityEngine;
//
// public class LaserTower : Tower
// {
//     [SerializeField, Range(1f, 100f)] float damagePerSecond = 10f;
//     [SerializeField] Transform turret = default, laserBeam = default;
//
//     private TargetPoint _target;
//     private Vector3 _laserBeamScale;
//
//     public override TowerType TowerType => TowerType.Laser;
//
//     public override void GameUpdate()
//     {
//         if (TrackTarget(ref _target) || AcquireTarget(out _target))
//         {
//             Shoot();
//         }
//         else
//         {
//             laserBeam.localScale = Vector3.zero;
//         }
//     }
//
//     private void Awake()
//     {
//         _laserBeamScale = laserBeam.localScale;
//     }
//
//     private void Shoot()
//     {
//         Vector3 point = _target.Position;
//         turret.LookAt(point);
//         laserBeam.localRotation = turret.localRotation;
//
//         float d = Vector3.Distance(turret.position, point);
//         _laserBeamScale.z = d;
//         laserBeam.localScale = _laserBeamScale;
//         laserBeam.localPosition =
//             turret.localPosition + 0.5f * d * laserBeam.forward;
//         _target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
//     }
// }