using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LevelConfig : ScriptableObject
{
    [SerializeField] private int maxMortarTowerCount = 5, maxLightningTowerCount = 5, maxWallCount = 10;

    [SerializeField] private Vector2Int
        destinationPosition = new Vector2Int(5, 5),
        spawnPointPosition = new Vector2Int(0, 0),
        gameBoardSize = new Vector2Int(11, 11);

    [SerializeField, Range(0, 100)] private int startingPlayerHealth = 10;

    public int MaxMortarTowerCount => maxMortarTowerCount;
    public int MaxLightningTowerCount => maxLightningTowerCount;
    public int MaxWallCount => maxWallCount;
    public Vector2Int GameBoardSize => gameBoardSize;
    public Vector2Int DestinationPosition => destinationPosition;
    public Vector2Int SpawnPointPosition => spawnPointPosition;
    public int StartingPlayerHealth => startingPlayerHealth;

    private void OnValidate()
    {
        if (gameBoardSize.x < 2)
        {
            gameBoardSize.x = 2;
        }

        if (gameBoardSize.y < 2)
        {
            gameBoardSize.y = 2;
        }

        if (destinationPosition.x < 0)
        {
            destinationPosition.x = 0;
        }

        if (destinationPosition.y < 0)
        {
            destinationPosition.y = 0;
        }

        if (spawnPointPosition.x < 0)
        {
            spawnPointPosition.x = 0;
        }

        if (spawnPointPosition.y < 0)
        {
            spawnPointPosition.y = 0;
        }

        if (destinationPosition.x >= gameBoardSize.x)
        {
            destinationPosition.x = gameBoardSize.x - 1;
        }

        if (destinationPosition.y >= gameBoardSize.y)
        {
            destinationPosition.y = gameBoardSize.y - 1;
        }

        if (spawnPointPosition.x >= gameBoardSize.x)
        {
            spawnPointPosition.x = gameBoardSize.x - 1;
        }

        if (spawnPointPosition.y >= gameBoardSize.y)
        {
            spawnPointPosition.y = gameBoardSize.y - 1;
        }
    }
}