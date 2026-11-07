using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "WallLevelDesignData", menuName = "ScriptableObjects/Wall Level Design Data")]
    public class WallLevelDesignData : ScriptableObject
    {
        [Header("Main Grid")]
        [Min(1)] public int GridWidth = 9;
        [Min(1)] public int GridHeight = 9;
        [SerializeField] private Vector2 _nodeSize;
        [SerializeField] private Vector3 _worldPositionOffset;
        [SerializeField] private GameObject _pointPrefab;
        [SerializeField] private GameObject _powerPelletPrefab;
        [SerializeField] private GameObject _pacmanPrefab;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _enemyPrefab;

        [Header("Enemy Types")]
        [SerializeField] private GhostConfig[] _enemyTypePalette;

        [Header("Spawns")]
        [SerializeField] private Vector2Int _playerSpawnCell = new Vector2Int(-1, -1);
        [SerializeField] private List<EnemySpawnCell> _enemySpawnCells = new List<EnemySpawnCell>();
        [SerializeField] private List<Vector2Int> _pointSpawnCells = new List<Vector2Int>();
        [SerializeField] private List<Vector2Int> _powerPelletSpawnCells = new List<Vector2Int>();
        [SerializeField] private Vector2Int _enemyBaseCell = new Vector2Int(-1, -1);

        [SerializeField] private bool[] _cells;

        public Vector2Int GridSize => new Vector2Int(GridWidth, GridHeight);
        public Vector2 NodeSize => _nodeSize;
        public Vector3 WorldPositionOffset => _worldPositionOffset;
        public GameObject PointPrefab => _pointPrefab;
        public GameObject PowerPelletPrefab => _powerPelletPrefab;
        public GameObject PacmanPrefab => _pacmanPrefab;
        public GameObject NodePrefab => _nodePrefab;
        public GameObject EnemyPrefab => _enemyPrefab;
        public GhostConfig[] EnemyTypePalette => _enemyTypePalette;

        public void EnsureSize()
        {
            var size = GridWidth * GridHeight;
            if (_cells == null || _cells.Length != size)
            {
                var newCells = new bool[size];
                if (_cells != null) Array.Copy(_cells, newCells, Mathf.Min(_cells.Length, newCells.Length));
                _cells = newCells;
            }
        }

        public bool IsWall(int x, int y)
        {
            EnsureSize();
            return _cells[y * GridWidth + x];
        }

        public void SetWall(int x, int y, bool value)
        {
            EnsureSize();
            _cells[y * GridWidth + x] = value;
        }

        public void ToggleWall(int x, int y)
        {
            EnsureSize();
            var i = y * GridWidth + x;
            _cells[i] = !_cells[i];
        }

        public void Fill(bool value)
        {
            EnsureSize();
            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = value;
        }

        public int GetWallAmount()
        {
            EnsureSize();
            var amount = 0;
            foreach (var cell in _cells)
                if (cell) amount++;
            return amount;
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return _worldPositionOffset + new Vector3(x * _nodeSize.x, y * _nodeSize.y, 0f);
        }

        // ---- Player Spawn ----
        public bool HasPlayerSpawn => _playerSpawnCell.x >= 0 && _playerSpawnCell.y >= 0;
        public Vector2Int PlayerSpawnCell => _playerSpawnCell;
        public void SetPlayerSpawn(int x, int y) => _playerSpawnCell = new Vector2Int(x, y);
        public void ClearPlayerSpawn() => _playerSpawnCell = new Vector2Int(-1, -1);
        public bool IsPlayerSpawn(int x, int y) => HasPlayerSpawn && _playerSpawnCell.x == x && _playerSpawnCell.y == y;
        public Vector3 GetPlayerSpawnWorldPosition() =>
            HasPlayerSpawn ? GetWorldPosition(_playerSpawnCell.x, _playerSpawnCell.y) : _worldPositionOffset;

        // ---- Enemy Base ----
        public bool HasEnemyBase => _enemyBaseCell.x >= 0 && _enemyBaseCell.y >= 0;
        public Vector2Int EnemyBaseCell => _enemyBaseCell;
        public void SetEnemyBase(int x, int y) => _enemyBaseCell = new Vector2Int(x, y);
        public void ClearEnemyBase() => _enemyBaseCell = new Vector2Int(-1, -1);
        public bool IsEnemyBase(int x, int y) => HasEnemyBase && _enemyBaseCell.x == x && _enemyBaseCell.y == y;
        public Vector3 GetEnemyBaseWorldPosition() =>
            HasEnemyBase ? GetWorldPosition(_enemyBaseCell.x, _enemyBaseCell.y) : _worldPositionOffset;

        public void ToggleEnemyBase(int x, int y)
        {
            if (IsEnemyBase(x, y)) ClearEnemyBase();
            else SetEnemyBase(x, y);
        }

        // ---- Enemy Spawns ----
        public IReadOnlyList<EnemySpawnCell> EnemySpawnCells => _enemySpawnCells;

        private int FindEnemySpawnIndex(int x, int y)
        {
            for (var i = 0; i < _enemySpawnCells.Count; i++)
                if (_enemySpawnCells[i].Cell.x == x && _enemySpawnCells[i].Cell.y == y)
                    return i;
            return -1;
        }

        public bool IsEnemySpawn(int x, int y) => FindEnemySpawnIndex(x, y) >= 0;

        public GhostConfig GetEnemyTypeAt(int x, int y)
        {
            var idx = FindEnemySpawnIndex(x, y);
            return idx >= 0 ? _enemySpawnCells[idx].Type : null;
        }

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

        public void ClearEnemySpawns() => _enemySpawnCells.Clear();

        public List<Vector3> GetEnemySpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _enemySpawnCells)
                result.Add(GetWorldPosition(cell.Cell.x, cell.Cell.y));
            return result;
        }

        // ---- Point Spawns ----
        public IReadOnlyList<Vector2Int> PointSpawnCells => _pointSpawnCells;
        public bool IsPointSpawn(int x, int y) => _pointSpawnCells.Contains(new Vector2Int(x, y));

        public void TogglePointSpawn(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (_pointSpawnCells.Contains(cell)) _pointSpawnCells.Remove(cell);
            else _pointSpawnCells.Add(cell);
        }

        public void ClearPointSpawns() => _pointSpawnCells.Clear();

        public List<Vector3> GetPointSpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _pointSpawnCells)
                result.Add(GetWorldPosition(cell.x, cell.y));
            return result;
        }

        // ---- PowerPellet Spawns ----
        public IReadOnlyList<Vector2Int> PowerPelletSpawnCells => _powerPelletSpawnCells;
        public bool IsPowerPelletSpawn(int x, int y) => _powerPelletSpawnCells.Contains(new Vector2Int(x, y));

        public void TogglePowerPelletSpawn(int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (_powerPelletSpawnCells.Contains(cell)) _powerPelletSpawnCells.Remove(cell);
            else _powerPelletSpawnCells.Add(cell);
        }

        public void ClearPowerPelletSpawns() => _powerPelletSpawnCells.Clear();

        public List<Vector3> GetPowerPelletSpawnWorldPositions()
        {
            var result = new List<Vector3>();
            foreach (var cell in _powerPelletSpawnCells)
                result.Add(GetWorldPosition(cell.x, cell.y));
            return result;
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
}