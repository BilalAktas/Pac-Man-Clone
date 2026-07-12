using UnityEngine;

namespace Core
{
    public class Grid : Singleton<Grid>
    {
        
		private Node[,] _grid;
        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private Vector2 _boxNodeSize;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private Vector3 _worldPositionOffset;

        [SerializeField] private GameObject _pacmanPrefab;
        private int _spawnAmount;
        
        private void Start()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            _grid = Helpers.CreateGrid(
                _gridSize,
                origin: Vector3.zero,
                nodeSize: _boxNodeSize,
                worldOffset: _worldPositionOffset,
                createNode: (worldPos, gridPos) =>
                {
                    var clone = Instantiate(_nodePrefab, transform);
                    clone.transform.localPosition = worldPos;

                    return new Node(worldPos, gridPos);
                },
                afterCreate: (node, worldPos, gridPos) =>
                {
      
                    if (_spawnAmount == 0)
                    {
                        var clone = Instantiate(_pacmanPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        node.SetCharacter(clone.GetComponent<Character>());
                        _spawnAmount++;
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
            => pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;
        
        public bool IsNodeClear(Vector2Int pos) => !_grid[pos.x, pos.y].Character;
        
    }
}