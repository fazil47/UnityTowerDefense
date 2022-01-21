using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField] private GameBoard board = default;
    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] private EnemyFactory enemyFactory = default;
    [SerializeField, Range(0.1f, 10f)] private float spawnSpeed = 1f;

    private float _spawnProgress = 0f;
    private GameBehaviorCollection _enemies = new GameBehaviorCollection();
    private GameBehaviorCollection _nonEnemies = new GameBehaviorCollection();
    private TowerType _selectedTowerType;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    static Game instance;

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance._nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance._nonEnemies.Add(explosion);
        return explosion;
    }

    private void OnEnable()
    {
        instance = this;
    }

    private void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.showGrid = true;
    }

    private void OnValidate()
    {
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }

        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternateTouch();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.showPaths = !board.showPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.showGrid = !board.showGrid;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _selectedTowerType = TowerType.Laser;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedTowerType = TowerType.Mortar;
        }

        _spawnProgress += spawnSpeed * Time.deltaTime;
        while (_spawnProgress >= 1f)
        {
            _spawnProgress -= 1f;
            SpawnEnemy();
        }

        _enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        _nonEnemies.GameUpdate();
    }

    private void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, _selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }

    private void HandleAlternateTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else
            {
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    private void SpawnEnemy()
    {
        GameTile spawnPoint =
            board.GetSpawnPoint(UnityEngine.Random.Range(0, board.SpawnPointCount));
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);
        _enemies.Add(enemy);
    }
}