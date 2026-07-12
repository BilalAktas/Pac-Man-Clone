using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum WallType
    {
        Empty = 0,
        WallA = 1,
        WallB = 2
    }

    public struct WallCell
    {
        public int X;
        public int Y;
        public WallType Type;

        public WallCell(int x, int y, WallType type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }

    [CreateAssetMenu(fileName = "WallLevelDesignData", menuName = "ScriptableObjects/Wall Level Design Data")]
    public class WallLevelDesignData : ScriptableObject
    {
        [Header("Main Grid")]
        [Min(1)] public int GridWidth = 9;
        [Min(1)] public int GridHeight = 9;
        [SerializeField] private Vector2 _nodeSize;
        [SerializeField] private Vector3 _worldPositionOffset;
        [SerializeField] private GameObject _wallPrefabA;
        [SerializeField] private GameObject _wallPrefabB;
        [SerializeField] private GameObject _pointPrefab;
        [SerializeField] private GameObject _pacmanPrefab;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _enemyPrefab;

        
        [Header("Editor Icons")]
        [SerializeField] private Texture2D _wallIconA;
        [SerializeField] private Texture2D _wallIconB;

        [Header("Spawns")]
        [SerializeField] private Vector2Int _playerSpawnCell = new Vector2Int(-1, -1);
        [SerializeField] private List<Vector2Int> _enemySpawnCells = new List<Vector2Int>();
        [SerializeField] private List<Vector2Int> _pointSpawnCells = new List<Vector2Int>();

        public Vector2Int GridSize => new Vector2Int(GridWidth, GridHeight);
        public Vector2 NodeSize => _nodeSize;
        public Vector3 WorldPositionOffset => _worldPositionOffset;
        public GameObject WallPrefabA => _wallPrefabA;
        public GameObject WallPrefabB => _wallPrefabB;
        public GameObject PointPrefab => _pointPrefab;
        public GameObject PacmanPrefab => _pacmanPrefab;
        public GameObject NodePrefab => _nodePrefab;
        public Texture2D WallIconA => _wallIconA;
        public Texture2D WallIconB => _wallIconB;
        public GameObject EnemyPrefab => _enemyPrefab;

        public Texture2D GetIconFor(WallType type)
        {
            return type switch
            {
                WallType.WallA => _wallIconA,
                WallType.WallB => _wallIconB,
                _ => null
            };
        }

        [SerializeField] private WallType[] _cells;

        public GameObject GetPrefabFor(WallType type)
        {
            return type switch
            {
                WallType.WallA => _wallPrefabA,
                WallType.WallB => _wallPrefabB,
                _ => null
            };
        }

        public void EnsureSize()
        {
            var size = GridWidth * GridHeight;
            if (_cells == null || _cells.Length != size)
            {
                var newCells = new WallType[size];
                if (_cells != null) Array.Copy(_cells, newCells, Mathf.Min(_cells.Length, newCells.Length));
                _cells = newCells;
            }
        }

        public WallType Get(int x, int y)
        {
            EnsureSize();
            return _cells[y * GridWidth + x];
        }

        public void Set(int x, int y, WallType value)
        {
            EnsureSize();
            _cells[y * GridWidth + x] = value;
        }

        public void Fill(WallType value)
        {
            EnsureSize();
            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = value;
        }

        public WallType[] GetRaw()
        {
            EnsureSize();
            return _cells;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return _worldPositionOffset + new Vector3(x * _nodeSize.x, y * _nodeSize.y, 0f);
        }

        // Runtime spawn için: sadece duvar olan (Empty olmayan) hücreleri döndürür.
        public List<WallCell> GetWallCells()
        {
            EnsureSize();
            var result = new List<WallCell>();

            for (var y = 0; y < GridHeight; y++)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    var type = _cells[y * GridWidth + x];
                    if (type != WallType.Empty)
                        result.Add(new WallCell(x, y, type));
                }
            }

            return result;
        }

        public WallCell IsWallCell(int x, int y)
        {
            foreach (var cell in GetWallCells())
            {
                if (cell.X == x && cell.Y == y)
                {
                    return cell;
                }
            }

            return new WallCell();
        }

        public int GetWallAmount()
        {
            EnsureSize();
            var amount = 0;
            foreach (var cell in _cells)
                if (cell != WallType.Empty)
                    amount++;
            return amount;
        }

        // ---- Player Spawn ----

        public bool HasPlayerSpawn => _playerSpawnCell.x >= 0 && _playerSpawnCell.y >= 0;
        public Vector2Int PlayerSpawnCell => _playerSpawnCell;

        public void SetPlayerSpawn(int x, int y)
        {
            _playerSpawnCell = new Vector2Int(x, y);
        }

        public void ClearPlayerSpawn()
        {
            _playerSpawnCell = new Vector2Int(-1, -1);
        }

        public bool IsPlayerSpawn(int x, int y)
        {
            return HasPlayerSpawn && _playerSpawnCell.x == x && _playerSpawnCell.y == y;
        }

        public Vector3 GetPlayerSpawnWorldPosition()
        {
            return HasPlayerSpawn ? GetWorldPosition(_playerSpawnCell.x, _playerSpawnCell.y) : _worldPositionOffset;
        }

        // ---- Enemy Spawns ----

        public IReadOnlyList<Vector2Int> EnemySpawnCells => _enemySpawnCells;

        public bool IsEnemySpawn(int x, int y)
        {
            return _enemySpawnCells.Contains(new Vector2Int(x, y));
        }

        public void ToggleEnemySpawn(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (_enemySpawnCells.Contains(cell))
                _enemySpawnCells.Remove(cell);
            else
                _enemySpawnCells.Add(cell);
        }

        public void ClearEnemySpawns()
        {
            _enemySpawnCells.Clear();
        }

        public List<Vector3> GetEnemySpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _enemySpawnCells)
                result.Add(GetWorldPosition(cell.x, cell.y));
            return result;
        }

        // ---- Point Spawns ----

        public IReadOnlyList<Vector2Int> PointSpawnCells => _pointSpawnCells;

        public bool IsPointSpawn(int x, int y)
        {
            return _pointSpawnCells.Contains(new Vector2Int(x, y));
        }

        public void TogglePointSpawn(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (_pointSpawnCells.Contains(cell))
                _pointSpawnCells.Remove(cell);
            else
                _pointSpawnCells.Add(cell);
        }

        public void ClearPointSpawns()
        {
            _pointSpawnCells.Clear();
        }

        public List<Vector3> GetPointSpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _pointSpawnCells)
                result.Add(GetWorldPosition(cell.x, cell.y));
            return result;
        }
    }
}