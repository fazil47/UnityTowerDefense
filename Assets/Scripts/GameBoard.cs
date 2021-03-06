using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Transform ground = default;
    [SerializeField] private GameTile tilePrefab = default;
    [SerializeField] private Texture2D gridTexture = default;

    [SerializeField] private TextMeshProUGUI
        lightningTowersLeftText = default,
        mortarTowersLeftText = default,
        wallsLeftText = default;

    private static readonly int MainTex = Shader.PropertyToID("_BaseMap");

    private Vector2Int _size;
    private GameTile[] _tiles;

    private int
        _mortarTowerCount,
        _lightningTowerCount,
        _wallCount,
        _maxMortarTowerCount,
        _maxLightningTowerCount,
        _maxWallCount;

    private Queue<GameTile> _searchFrontier = new Queue<GameTile>();
    private GameTileContentFactory _contentFactory;
    private bool _showPaths, _showGrid;
    private List<GameTile> _spawnPoints = new List<GameTile>();
    private List<GameTileContent> _updatingContent = new List<GameTileContent>();
    private Vector2Int _initialDestinationPoint, _initialSpawnPointPosition;

    public int SpawnPointCount => _spawnPoints.Count;

    public bool showGrid
    {
        get => _showGrid;
        set
        {
            _showGrid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;
            if (_showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale(MainTex, _size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    public bool showPaths
    {
        get => _showPaths;
        set
        {
            _showPaths = value;
            if (_showPaths)
            {
                foreach (GameTile tile in _tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in _tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }


    public void Initialize(
        Vector2Int size,
        GameTileContentFactory contentFactory,
        Vector2Int destinationPosition,
        Vector2Int spawnPointPosition,
        int maxMortarTowerCount,
        int maxLightningTowerCount,
        int maxWallCount
    )
    {
        _size = size;
        _contentFactory = contentFactory;
        ground.localScale = new Vector3(_size.x, _size.y, 1f);
        _initialDestinationPoint = destinationPosition;
        _initialSpawnPointPosition = spawnPointPosition;
        _maxMortarTowerCount = maxMortarTowerCount;
        _maxLightningTowerCount = maxLightningTowerCount;
        _maxWallCount = maxWallCount;

        Vector2 offset = new Vector2((_size.x - 1) * 0.5f, (_size.y - 1) * 0.5f);

        _tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < _size.y; y++)
        {
            for (int x = 0; x < _size.x; x++, i++)
            {
                Vector3 position = new Vector3(x - offset.x, 0f, y - offset.y);
                GameTile tile = _tiles[i] = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, _tiles[i - 1]);
                }

                if (y > 0)
                {
                    GameTile.MakeNorthSouthNeighbors(tile, _tiles[i - size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }
            }
        }

        Clear();
    }

    public void Clear()
    {
        _mortarTowerCount = _lightningTowerCount = _wallCount = 0;
        lightningTowersLeftText.text = "Lightning Towers Left: " + _maxLightningTowerCount;
        mortarTowersLeftText.text = "Mortar Towers Left: " + _maxMortarTowerCount;
        wallsLeftText.text = "Walls Left: " + _maxWallCount;

        foreach (GameTile tile in _tiles)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
        }

        _spawnPoints.Clear();
        _updatingContent.Clear();
        ToggleDestination(_tiles[TilePositionToIndex(_initialDestinationPoint)]);
        ToggleSpawnPoint(_tiles[TilePositionToIndex(_initialSpawnPointPosition)]);
    }

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1))
        {
            int x = (int)(hit.point.x + _size.x * 0.5f);
            int y = (int)(hit.point.z + _size.y * 0.5f);
            if (x >= 0 && x < _size.x && y >= 0 && y <= _size.y)
            {
                return _tiles[TilePositionToIndex(x, y)];
            }
        }

        return null;
    }

    public void GameUpdate()
    {
        for (int i = 0; i < _updatingContent.Count; i++)
        {
            _updatingContent[i].GameUpdate();
        }
    }

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths())
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            DecrementWallCount();
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths() || !IncrementWallCount())
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public void ToggleTower(GameTile tile, TowerType towerType)
    {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            TowerType tileTowerType = ((Tower)tile.Content).TowerType;
            _updatingContent.Remove(tile.Content);
            if (tileTowerType == towerType)
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
                DecrementTowerCount(towerType);
            }
            else
            {
                if (IncrementTowerCount(towerType))
                {
                    DecrementTowerCount(tileTowerType);
                    tile.Content = _contentFactory.Get(towerType);
                    _updatingContent.Add(tile.Content);
                }
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(towerType);
            if (FindPaths() && IncrementTowerCount(towerType))
            {
                _updatingContent.Add(tile.Content);
            }
            else
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Wall && IncrementTowerCount(towerType))
        {
            DecrementWallCount();
            tile.Content = _contentFactory.Get(towerType);
            _updatingContent.Add(tile.Content);
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (_spawnPoints.Count > 1)
            {
                _spawnPoints.Remove(tile);
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.SpawnPoint);
            _spawnPoints.Add(tile);
        }
    }

    public GameTile GetSpawnPoint(int index)
    {
        return _spawnPoints[index];
    }

    private int TilePositionToIndex(Vector2Int pos)
    {
        return pos.x + (_size.x * pos.y);
    }

    private int TilePositionToIndex(int x, int y)
    {
        return x + (_size.x * y);
    }

    private bool FindPaths()
    {
        foreach (GameTile tile in _tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                _searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if (_searchFrontier.Count == 0)
        {
            return false;
        }

        while (_searchFrontier.Count > 0)
        {
            GameTile tile = _searchFrontier.Dequeue();
            if (tile != null)
            {
                if (tile.IsAlternative)
                {
                    _searchFrontier.Enqueue(tile.GrowPathNorth());
                    _searchFrontier.Enqueue(tile.GrowPathSouth());
                    _searchFrontier.Enqueue(tile.GrowPathEast());
                    _searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    _searchFrontier.Enqueue(tile.GrowPathWest());
                    _searchFrontier.Enqueue(tile.GrowPathEast());
                    _searchFrontier.Enqueue(tile.GrowPathSouth());
                    _searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }

        foreach (GameTile tile in _tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }

        if (_showPaths)
        {
            foreach (GameTile tile in _tiles)
            {
                tile.ShowPath();
            }
        }

        return true;
    }

    private bool IncrementTowerCount(TowerType type)
    {
        if (type == TowerType.Lightning)
        {
            if (++_lightningTowerCount > _maxLightningTowerCount)
            {
                --_lightningTowerCount;
                return false;
            }

            lightningTowersLeftText.text = "Lightning Towers Left: " +
                                           (_maxLightningTowerCount - _lightningTowerCount);
        }
        else if (type == TowerType.Mortar)
        {
            if (++_mortarTowerCount > _maxMortarTowerCount)
            {
                --_mortarTowerCount;
                return false;
            }

            mortarTowersLeftText.text = "Mortar Towers Left: " + (_maxMortarTowerCount - _mortarTowerCount);
        }

        return true;
    }

    private bool IncrementWallCount()
    {
        if (++_wallCount > _maxWallCount)
        {
            --_wallCount;
            return false;
        }

        wallsLeftText.text = "Walls Left: " + (_maxWallCount - _wallCount);

        return true;
    }

    private void DecrementTowerCount(TowerType type)
    {
        if (type == TowerType.Lightning)
        {
            --_lightningTowerCount;
            Debug.Assert(_lightningTowerCount >= 0, "Lightning Tower Count has gone below 0.");
            lightningTowersLeftText.text = "Lightning Towers Left: " +
                                           (_maxLightningTowerCount - _lightningTowerCount);
        }
        else if (type == TowerType.Mortar)
        {
            --_mortarTowerCount;
            Debug.Assert(_mortarTowerCount >= 0, "Mortar Tower Count has gone below 0.");
            mortarTowersLeftText.text = "Mortar Towers Left: " + (_maxMortarTowerCount - _mortarTowerCount);
        }
    }


    private void DecrementWallCount()
    {
        --_wallCount;
        Debug.Assert(_wallCount >= 0, "Wall Count has gone below 0.");
        wallsLeftText.text = "Walls Left: " + (_maxWallCount - _wallCount);
    }
}