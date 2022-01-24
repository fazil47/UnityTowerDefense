using System;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField] private GameBoard board = default;
    [SerializeField] private GameTileContentFactory tileContentFactory = default;
    [SerializeField] private WarFactory warFactory = default;
    [SerializeField] private GameScenario scenario = default;
    [SerializeField] private LevelConfig levelConfig = default;
    [SerializeField, Range(1f, 10f)] private float playSpeed = 1f;
    [SerializeField] private TextMeshProUGUI playerHealthText = default, selectedTowerText = default;
    [SerializeField] private GameObject gameWonPanel = default, gameOverPanel = default, gamePausePanel = default;


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
        _instance.playerHealthText.text = "Player Health: " + _instance._playerHealth;
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

    public void BeginNewGame()
    {
        Time.timeScale = playSpeed;
        gameWonPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gamePausePanel.SetActive(false);

        _selectedTowerType = TowerType.Lightning;
        selectedTowerText.text = "Selected Tower: Lightning";
        _playerHealth = levelConfig.StartingPlayerHealth;
        playerHealthText.text = "Player Health: " + _playerHealth;

        _enemies.Clear();
        _nonEnemies.Clear();
        board.Clear();

        _activeScenario = scenario.Begin();
    }

    public void QuitGame()
    {
        if (Application.isEditor)
        {
            EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }

    private void OnEnable()
    {
        _instance = this;
    }

    private void Awake()
    {
        _playerHealth = levelConfig.StartingPlayerHealth;
        board.Initialize(
            levelConfig.GameBoardSize,
            tileContentFactory,
            levelConfig.DestinationPosition,
            levelConfig.SpawnPointPosition,
            levelConfig.MaxMortarTowerCount,
            levelConfig.MaxLightningTowerCount,
            levelConfig.MaxWallCount
        );
        board.showGrid = true;

        BeginNewGame();
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
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
            _selectedTowerType = TowerType.Lightning;
            selectedTowerText.text = "Selected Tower: Lightning";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _selectedTowerType = TowerType.Mortar;
            selectedTowerText.text = "Selected Tower: Mortar";
        }


        if (Time.timeScale > 0)
        {
            Time.timeScale = playSpeed;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gamePausePanel.SetActive(!gamePausePanel.activeSelf);
            Time.timeScale = !gamePausePanel.activeSelf ? playSpeed : 0;
        }


        if (_playerHealth <= 0 && levelConfig.StartingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            Time.timeScale = 0;
            gameOverPanel.SetActive(true);
        }

        if (!_activeScenario.Progress() && _enemies.IsEmpty)
        {
            Debug.Log("Victory!");
            Time.timeScale = 0;
            gameWonPanel.SetActive(true);
        }

        _activeScenario.Progress();

        _enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        _nonEnemies.GameUpdate();
    }

    private void HandleTouch()
    {
        if (Time.timeScale == 0)
        {
            return;
        }

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