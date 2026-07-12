using UnityEngine;

namespace Core
{
    public class Grid : Singleton<Grid>
    {
		private Node[,] _grid;
        [SerializeField] private WallLevelDesignData _currentLevelDesignData;
        
        private void Start()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            _grid = Helpers.CreateGrid(
                _currentLevelDesignData.GridSize,
                origin: Vector3.zero,
                nodeSize: _currentLevelDesignData.NodeSize,
                worldOffset: _currentLevelDesignData.WorldPositionOffset,
                createNode: (worldPos, gridPos) =>
                {
                    var clone = Instantiate(_currentLevelDesignData.NodePrefab, transform);
                    clone.transform.localPosition = worldPos;

                    return new Node(worldPos, gridPos, clone);
                },
                afterCreate: (node, worldPos, gridPos) =>
                {
                    if (_currentLevelDesignData.PlayerSpawnCell.x == gridPos.x && _currentLevelDesignData.PlayerSpawnCell.y == gridPos.y)
                    {
                        var clone = Instantiate(_currentLevelDesignData.PacmanPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        var character = clone.GetComponent<Character>();
                        character.SetNode(node);
                    }

                    if (_currentLevelDesignData.IsPointSpawn(gridPos.x, gridPos.y))
                    {
                        var clone = Instantiate(_currentLevelDesignData.PointPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        node.SetCollectable(clone.GetComponentInChildren<ICollectable>());
                    }
                    
                    if (_currentLevelDesignData.IsEnemySpawn(gridPos.x, gridPos.y))
                    {
                        var clone = Instantiate(_currentLevelDesignData.EnemyPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        clone.GetComponent<Character>().SetNode(node);
                    }
                    
                    var wall = _currentLevelDesignData.IsWallCell(gridPos.x, gridPos.y);
                    if (wall.Type != WallType.Empty)
                    {
                        var clone = Instantiate(wall.Type == WallType.WallA ? _currentLevelDesignData.WallPrefabA : _currentLevelDesignData.WallPrefabB, transform);
                        clone.transform.localPosition = worldPos;
                        node.SetWall();
                    }
                }
            );
        }

        public Node GetNodeByDirection(Vector3 currentPosition, MoveDirection dir)
        {
            switch (dir)
            {
                case MoveDirection.Up:
                    foreach (var node in _grid)
                    {
                        if (node.WorldPosition == currentPosition)
                        {
                            var p = new Vector2Int(node.GridPosition.x, node.GridPosition.y + 1);
                            if (IsPositionInBound(p))
                            {
                                var nextNode = _grid[p.x, p.y];
                                return nextNode;
                            }
                        }
                    }
                    break;
                case MoveDirection.Down:
                    foreach (var node in _grid)
                    {
                        if (node.WorldPosition == currentPosition)
                        {
                            var p = new Vector2Int(node.GridPosition.x, node.GridPosition.y - 1);
                            if (IsPositionInBound(p))
                            {
                                var nextNode = _grid[p.x, p.y];
                                return nextNode;
                            }
                        }
                    }
                    break;
                
                case MoveDirection.Left:
                    foreach (var node in _grid)
                    {
                        if (node.WorldPosition == currentPosition)
                        {
                            var p = new Vector2Int(node.GridPosition.x - 1, node.GridPosition.y);
                            if (IsPositionInBound(p))
                            {
                                var nextNode = _grid[p.x, p.y];
                                return nextNode;
                            }
                        }
                    }
                    break;
                
                case MoveDirection.Right:
                    foreach (var node in _grid)
                    {
                        if (node.WorldPosition == currentPosition)
                        {
                            var p = new Vector2Int(node.GridPosition.x + 1, node.GridPosition.y);
                            if (IsPositionInBound(p))
                            {
                                var nextNode = _grid[p.x, p.y];
                                return nextNode;
                            }
                        }
                    }
                    break;
            }

            return null;
        }
        
        private bool IsPositionInBound(Vector2Int pos)
            => pos.x >= 0 && pos.x < _currentLevelDesignData.GridSize.x && pos.y >= 0 && pos.y < _currentLevelDesignData.GridSize.y;
        
        public bool IsNodeClear(Vector2Int pos) => !_grid[pos.x, pos.y].Character;
        
    }
}