using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Grid : Singleton<Grid>
    {
		private Node[,] _grid;
        [SerializeField] private WallLevelDesignData _currentLevelDesignData;
        
        private static readonly Dictionary<MoveDirection, Vector2Int> _directionOffsets = new()
        {
            { MoveDirection.Up,    new Vector2Int(0, 1) },
            { MoveDirection.Down,  new Vector2Int(0, -1) },
            { MoveDirection.Left,  new Vector2Int(-1, 0) },
            { MoveDirection.Right, new Vector2Int(1, 0) },
        };
        
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

        public Node GetNodeByDirection(Node currentNode, MoveDirection dir)
        {
            var offset = _directionOffsets[dir];
            var p = new Vector2Int(currentNode.GridPosition.x + offset.x, currentNode.GridPosition.y + offset.y);
            return IsPositionInBound(p) ? _grid[p.x, p.y] : null;
        }
        
        private bool IsPositionInBound(Vector2Int pos)
            => pos.x >= 0 && pos.x < _currentLevelDesignData.GridSize.x && pos.y >= 0 && pos.y < _currentLevelDesignData.GridSize.y;
        
        public bool IsNodeClear(Vector2Int pos) => !_grid[pos.x, pos.y].Character;

        public List<Node> GetAllNodesByDirection(MoveDirection dir, int distance, Node baseNode)
        {
            var nodes = new List<Node>();
            var offset = _directionOffsets[dir];

            var x = baseNode.GridPosition.x + offset.x;
            var y = baseNode.GridPosition.y + offset.y;

            for (var i = 0; i < distance && IsPositionInBound(new Vector2Int(x, y)); i++)
            {
                var node = _grid[x, y];

                if (!node.IsWalkable)
                    break;

                nodes.Add(node);

                x += offset.x;
                y += offset.y;
            }

            return nodes;
        }
        
        public MoveDirection GetDirectionBetweenNodes(Node from, Node to)
        {
            var dx = to.GridPosition.x - from.GridPosition.x;
            var dy = to.GridPosition.y - from.GridPosition.y;

            if (dx == 0 && dy == 1) return MoveDirection.Up;
            if (dx == 0 && dy == -1) return MoveDirection.Down;
            if (dx == -1 && dy == 0) return MoveDirection.Left;
            if (dx == 1 && dy == 0) return MoveDirection.Right;

            return MoveDirection.None;
        }
    }
}