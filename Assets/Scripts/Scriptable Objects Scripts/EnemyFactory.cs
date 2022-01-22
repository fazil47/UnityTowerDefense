using UnityEngine;

public enum EnemyType
{
    Green,
    Purple,
    Red
}

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{
    [System.Serializable]
    private class EnemyConfig
    {
        public Enemy prefab = default;

        [FloatRangeSlider(0.5f, 2f)] public FloatRange scale = new FloatRange(1f);

        [FloatRangeSlider(0.2f, 5f)] public FloatRange speed = new FloatRange(1f);

        [FloatRangeSlider(-0.4f, 0.4f)] public FloatRange pathOffset = new FloatRange(0f);

        [FloatRangeSlider(10f, 1000f)] public FloatRange health = new FloatRange(100f);
    }

    [SerializeField] private EnemyConfig green = default, purple = default, red = default;

    public Enemy Get(EnemyType type = EnemyType.Green)
    {
        EnemyConfig config = GetConfig(type);
        Enemy instance = CreateGameObjectInstance(config.prefab);
        instance.OriginFactory = this;
        instance.Initialize(
            config.scale.RandomValueInRange,
            config.speed.RandomValueInRange,
            config.pathOffset.RandomValueInRange,
            config.health.RandomValueInRange
        );
        return instance;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(enemy.gameObject);
    }

    private EnemyConfig GetConfig(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Green: return green;
            case EnemyType.Purple: return purple;
            case EnemyType.Red: return red;
        }

        Debug.Assert(false, "Unsupported enemy type!");
        return null;
    }
}