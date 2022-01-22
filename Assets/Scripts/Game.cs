using System;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new Vector2Int(11, 11);
    [SerializeField] private GameBoard board = default;
    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] private GameScenario scenario = default;
    [SerializeField, Range(0, 100)] private int startingPlayerHealth = 10;
    [SerializeField, Range(1f, 10f)] private float playSpeed = 1f;

    private const float _pausedTimeScale = 0f;

    private GameScenario.State _activeScenario;
    private GameBehaviorCollection _enemies = new GameBehaviorCollection();
    private GameBehaviorCollection _nonEnemies = new GameBehaviorCollection();
    private TowerType _selectedTowerType;
    private int _playerHealth;

    private static Game _instance;

    private Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    public static void EnemyReachedDestination()
    {
        _instance._playerHealth -= 1;
    }

    public static Shell SpawnShell()
    {
        Shell shell = _instance.warFactory.Shell;
        _instance._nonEnemies.Add(shell);
        return shell;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = _instance.warFactory.Explosion;
        _instance._nonEnemies.Add(explosion);
        return explosion;
    }

    public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
    {
        GameTile spawnPoint = _instance.board.GetSpawnPoint(
            UnityEngine.Random.Range(0, _instance.board.SpawnPointCount)
        );
        Enemy enemy = factory.Get(type);
        enemy.SpawnOn(spawnPoint);
        _instance._enemies.Add(enemy);
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void Awake()
    {
        _playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.showGrid = true;
        _activeScenario = scenario.Begin();
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

    private void BeginNewGame()
    {
        _playerHealth = startingPlayerHealth;
        _enemies.Clear();
        _nonEnemies.Clear();
        board.Clear();
        _activeScenario = scenario.Begin();
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = Time.timeScale > _pausedTimeScale ? _pausedTimeScale : playSpeed;
        }
        else if (Time.timeScale > _pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        if (_playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        if (!_activeScenario.Progress() && _enemies.IsEmpty)
        {
            Debug.Log("Victory!");
            BeginNewGame();
            // _activeScenario.Progress();
        }

        _activeScenario.Progress();

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
}