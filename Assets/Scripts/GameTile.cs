using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField] private Transform arrow = default;

    private GameTile _north, _east, _south, _west, _nextOnPath;
    private int _distance;

    private static Quaternion
        _northRotation = Quaternion.Euler(90f, 0f, 0f),
        _eastRotation = Quaternion.Euler(90f, 90f, 0f),
        _southRotation = Quaternion.Euler(90f, 180f, 0f),
        _westRotation = Quaternion.Euler(90f, 270f, 0f);

    private GameTileContent _content;

    public Direction PathDirection { get; private set; }

    public GameTileContent Content
    {
        get => _content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");
            if (_content != null)
            {
                _content.Recycle();
            }

            _content = value;
            _content.transform.localPosition = transform.localPosition;
        }
    }

    public bool HasPath => _distance != int.MaxValue;
    public bool IsAlternative { get; set; }

    public GameTile GrowPathNorth() => GrowPathTo(_north, Direction.South);

    public GameTile GrowPathEast() => GrowPathTo(_east, Direction.West);

    public GameTile GrowPathSouth() => GrowPathTo(_south, Direction.North);

    public GameTile GrowPathWest() => GrowPathTo(_west, Direction.East);

    public GameTile NextTileOnPath => _nextOnPath;

    public Vector3 ExitPoint { get; private set; }

    public void ClearPath()
    {
        _distance = int.MaxValue;
        _nextOnPath = null;
    }

    public void ShowPath()
    {
        if (_distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            _nextOnPath == _north ? _northRotation :
            _nextOnPath == _east ? _eastRotation :
            _nextOnPath == _south ? _southRotation :
            _westRotation;
    }

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }

    public void BecomeDestination()
    {
        _distance = 0;
        _nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
        Debug.Assert(west._east == null && east._west == null, "Redefining neighbors isn't allowed.");
        west._east = east;
        east._west = west;
    }

    public static void MakeNorthSouthNeighbors(GameTile north, GameTile south)
    {
        Debug.Assert(north._south == null && south._north == null, "Redefining neighbors isn't allowed.");
        north._south = south;
        south._north = north;
    }

    private GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        Debug.Assert(HasPath, "Tile not on path.");
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        neighbor._distance = _distance + 1;
        neighbor._nextOnPath = this;
        neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();
        neighbor.PathDirection = direction;

        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }
}