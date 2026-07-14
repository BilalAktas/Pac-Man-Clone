using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum WallType
    {
        Empty = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 3,
        BottomRight = 4,
        Left = 5,
        LeftFull = 6,
        Right = 7,
        RightFull = 8
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

    [Serializable]
    public struct EnemySpawnCell
    {
        public Vector2Int Cell;
        public GhostConfig Type;

        public EnemySpawnCell(Vector2Int cell, GhostConfig type)
        {
            Cell = cell;
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
        [SerializeField] private GameObject[] _wallPrefab;
        [SerializeField] private GameObject _pointPrefab;
        [SerializeField] private GameObject _powerPelletPrefab;
        [SerializeField] private GameObject _pacmanPrefab;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Editor Icons")]
        [SerializeField] private Texture2D[] _wallIcon;

        [Header("Enemy Types")]
        [SerializeField] private GhostConfig[] _enemyTypePalette;

        [Header("Spawns")]
        [SerializeField] private Vector2Int _playerSpawnCell = new Vector2Int(-1, -1);
        [SerializeField] private List<EnemySpawnCell> _enemySpawnCells = new List<EnemySpawnCell>();
        [SerializeField] private List<Vector2Int> _pointSpawnCells = new List<Vector2Int>();
        [SerializeField] private List<Vector2Int> _powerPelletSpawnCells = new List<Vector2Int>();

        public Vector2Int GridSize => new Vector2Int(GridWidth, GridHeight);
        public Vector2 NodeSize => _nodeSize;
        public Vector3 WorldPositionOffset => _worldPositionOffset;
        public GameObject[] WallPrefab => _wallPrefab;
        public GameObject PointPrefab => _pointPrefab;
        public GameObject PowerPelletPrefab => _powerPelletPrefab;
        public GameObject PacmanPrefab => _pacmanPrefab;
        public GameObject NodePrefab => _nodePrefab;
        public Texture2D[] WallIcon => _wallIcon;
        public GameObject EnemyPrefab => _enemyPrefab;
        public GhostConfig[] EnemyTypePalette => _enemyTypePalette;

        public Texture2D GetIconFor(WallType type)
        {
            return _wallIcon[(int)type];
            // return type switch
            // {
            //     WallType.WallA => _wallIcon,
            //     WallType.WallB => _wallIconB,
            //     _ => null
            // };
        }

        [SerializeField] private WallType[] _cells;

        public GameObject GetPrefabFor(WallType type)
        {
            return _wallPrefab[(int)type];
            // return type switch
            // {
            //     WallType.TopLeft => _wallPrefab[0],
            //     _ => null
            // };
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

        public IReadOnlyList<EnemySpawnCell> EnemySpawnCells => _enemySpawnCells;

        private int FindEnemySpawnIndex(int x, int y)
        {
            for (var i = 0; i < _enemySpawnCells.Count; i++)
            {
                if (_enemySpawnCells[i].Cell.x == x && _enemySpawnCells[i].Cell.y == y)
                    return i;
            }
            return -1;
        }

        public bool IsEnemySpawn(int x, int y)
        {
            return FindEnemySpawnIndex(x, y) >= 0;
        }

        public GhostConfig GetEnemyTypeAt(int x, int y)
        {
            var idx = FindEnemySpawnIndex(x, y);
            return idx >= 0 ? _enemySpawnCells[idx].Type : null;
        }

        /// <summary>
        /// Hücredeki enemy spawn'ı, _enemyTypePalette içinde bir sonraki (veya dir=-1 ile önceki)
        /// GhostConfig'e döngüsel olarak ilerletir. Palet biterse hücre boşa döner (spawn kaldırılır).
        /// Palet boşsa/atanmamışsa sadece varlık (null type ile) toggle edilir.
        /// </summary>
        public void CycleEnemySpawn(int x, int y, int dir = 1)
        {
            var cell = new Vector2Int(x, y);
            var idx = FindEnemySpawnIndex(x, y);

            if (_enemyTypePalette == null || _enemyTypePalette.Length == 0)
            {
                if (idx >= 0) _enemySpawnCells.RemoveAt(idx);
                else _enemySpawnCells.Add(new EnemySpawnCell(cell, null));
                return;
            }

            if (idx < 0)
            {
                var startIndex = dir >= 0 ? 0 : _enemyTypePalette.Length - 1;
                _enemySpawnCells.Add(new EnemySpawnCell(cell, _enemyTypePalette[startIndex]));
                return;
            }

            var current = _enemySpawnCells[idx].Type;
            var currentIndex = Array.IndexOf(_enemyTypePalette, current);
            var nextIndex = currentIndex + dir;

            if (nextIndex < 0 || nextIndex >= _enemyTypePalette.Length)
            {
                _enemySpawnCells.RemoveAt(idx);
                return;
            }

            var entry = _enemySpawnCells[idx];
            entry.Type = _enemyTypePalette[nextIndex];
            _enemySpawnCells[idx] = entry;
        }

        public void ClearEnemySpawns()
        {
            _enemySpawnCells.Clear();
        }

        public List<Vector3> GetEnemySpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _enemySpawnCells)
                result.Add(GetWorldPosition(cell.Cell.x, cell.Cell.y));
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

        // ---- PowerPellet Spawns ----

        public IReadOnlyList<Vector2Int> PowerPelletSpawnCells => _powerPelletSpawnCells;

        public bool IsPowerPelletSpawn(int x, int y)
        {
            return _powerPelletSpawnCells.Contains(new Vector2Int(x, y));
        }

        public void TogglePowerPelletSpawn(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (_powerPelletSpawnCells.Contains(cell))
                _powerPelletSpawnCells.Remove(cell);
            else
                _powerPelletSpawnCells.Add(cell);
        }

        public void ClearPowerPelletSpawns()
        {
            _powerPelletSpawnCells.Clear();
        }

        public List<Vector3> GetPowerPelletSpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _powerPelletSpawnCells)
                result.Add(GetWorldPosition(cell.x, cell.y));
            return result;
        }
    }
}