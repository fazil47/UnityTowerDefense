using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTileContentType
{
    Empty,
    Destination,
    Wall,
    SpawnPoint,
    Tower
}

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    [SerializeField] private GameTileContentType type = default;

    private GameTileContentFactory originFactory;

    public GameTileContentFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Origin factory can't be reassigned.");
            originFactory = value;
        }
    }

    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    public GameTileContentType Type => type;

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }

    public virtual void GameUpdate()
    {
    }
}