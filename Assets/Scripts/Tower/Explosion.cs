using UnityEngine;

public class Explosion : WarEntity
{
    [SerializeField, Range(0f, 1f)] float duration = 0.5f;
    [SerializeField] AnimationCurve opacityCurve = default, scaleCurve = default;

    private static int _colorPropertyID = Shader.PropertyToID("_Color");
    private static MaterialPropertyBlock _propertyBlock;
    private float _age, _scale;
    private MeshRenderer _meshRenderer;

    public void Initialize(Vector3 position, float blastRadius, float damage = 0f)
    {
        if (damage > 0f)
        {
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++)
            {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }

        transform.localPosition = position;
        _scale = 2f * blastRadius;
    }

    public override bool GameUpdate()
    {
        _age += Time.deltaTime;
        if (_age >= duration)
        {
            OriginFactory.Reclaim(this);
            return false;
        }


        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        float t = _age / duration;
        Color c = Color.clear;
        c.a = opacityCurve.Evaluate(t);
        _propertyBlock.SetColor(_colorPropertyID, c);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
        transform.localScale = Vector3.one * (_scale * scaleCurve.Evaluate(t));
        
        return true;
    }

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        Debug.Assert(_meshRenderer != null, "Explosion without renderer!");
    }
}