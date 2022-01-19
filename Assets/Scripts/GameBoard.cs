using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField] private Transform ground = default;
    [SerializeField] private GameTile tilePrefab = default;
    [SerializeField] private Texture2D gridTexture = default;

    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private Vector2Int _size;
    private GameTile[] _tiles;
    private Queue<GameTile> _searchFrontier = new Queue<GameTile>();
    private GameTileContentFactory _contentFactory;
    private bool _showPaths, _showGrid;
    private List<GameTile> _spawnPoints = new List<GameTile>();

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


    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        _size = size;
        _contentFactory = contentFactory;
        ground.localScale = new Vector3(_size.x, _size.y, 1f);

        Vector2 offset = new Vector2((_size.x - 1) * 0.5f, (_size.y - 1) * 0.5f);

        _tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < _size.y; y++)
        {
            for (int x = 0; x < _size.x; x++, i++)
            {
                Vector3 position = new Vector3(x - offset.x, 0f, y - offset.y);
                GameTile tile = _tiles[i] = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                // tile.transform.SetParent(transform, false);
                // tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

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

                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        // FindPaths();
        ToggleDestination(_tiles[_tiles.Length / 2]);
        ToggleSpawnPoint(_tiles[0]);
    }

    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + _size.x * 0.5f);
            int y = (int)(hit.point.z + _size.y * 0.5f);
            if (x >= 0 && x < _size.x && y >= 0 && y <= _size.y)
            {
                return _tiles[x + (y * _size.x)];
            }
        }

        return null;
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
            tile.Content = _contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = _contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = _contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
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

        // _tiles[_tiles.Length / 2].BecomeDestination();
        // _searchFrontier.Enqueue(_tiles[_tiles.Length / 2]);

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
}