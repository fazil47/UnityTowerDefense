using UnityEditor;
using UnityEngine;

public class Tower : GameTileContent
{
    [SerializeField, Range(1.5f, 10.5f)] float targetingRange = 1.5f;
    [SerializeField, Range(1f, 100f)] float damagePerSecond = 10f;
    [SerializeField] Transform turret = default, laserBeam = default;

    private TargetPoint _target;
    private const int _enemyLayerMask = 1 << 9;
    private static Collider[] _targetsBuffer = new Collider[10];
    private Vector3 _laserBeamScale;

    public override void GameUpdate()
    {
        if (TrackTarget() || AcquireTarget())
        {
            Shoot();
        }
        else
        {
            laserBeam.localScale = Vector3.zero;
        }
    }

    private void Awake()
    {
        _laserBeamScale = laserBeam.localScale;
    }

    private void Shoot()
    {
        Vector3 point = _target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        _laserBeamScale.z = d;
        laserBeam.localScale = _laserBeamScale;
        laserBeam.localPosition =
            turret.localPosition + 0.5f * d * laserBeam.forward;
        _target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }

    private bool AcquireTarget()
    {
        Vector3 a = transform.localPosition;
        Vector3 b = a;
        b.y += 2f;
        int hits = Physics.OverlapCapsuleNonAlloc(
            a, b, targetingRange, _targetsBuffer, _enemyLayerMask
        );
        if (hits > 0)
        {
            _target = _targetsBuffer[Random.Range(0, hits)].GetComponent<TargetPoint>();
            Debug.Assert(_target != null, "Targeted non-enemy!", _targetsBuffer[0]);
            return true;
        }

        _target = null;
        return false;
    }

    private bool TrackTarget()
    {
        if (_target == null)
        {
            return false;
        }

        Vector3 a = transform.localPosition;
        Vector3 b = _target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f * _target.Enemy.Scale;
        if (x * x + z * z > r * r)
        {
            _target = null;
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
        if (_target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, _target.Position);
        }
    }
}