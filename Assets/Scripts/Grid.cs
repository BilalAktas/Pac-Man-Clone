using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class Grid : Singleton<Grid>
    {
        [SerializeField] private WallMapVisualSettings _wallMapVisualSettings;
        [SerializeField] private SpriteRenderer _wallMapRenderer;
        
		private Node[,] _grid;
        public WallLevelDesignData CurrentLevelDesignData;
        
        private static readonly Dictionary<MoveDirection, Vector2Int> _directionOffsets = new()
        {
            { MoveDirection.Up,    new Vector2Int(0, 1) },
            { MoveDirection.Down,  new Vector2Int(0, -1) },
            { MoveDirection.Left,  new Vector2Int(-1, 0) },
            { MoveDirection.Right, new Vector2Int(1, 0) },
        };
        
        private void Awake()
        {
            CreateGrid();
            RenderWallMap();
        }

        private void CreateGrid()
        {
            _grid = Helpers.CreateGrid(
                CurrentLevelDesignData.GridSize,
                origin: Vector3.zero,
                nodeSize: CurrentLevelDesignData.NodeSize,
                worldOffset: CurrentLevelDesignData.WorldPositionOffset,
                createNode: (worldPos, gridPos) =>
                {
                    var clone = Instantiate(CurrentLevelDesignData.NodePrefab, transform);
                    clone.transform.localPosition = worldPos;

                    return new Node(worldPos, gridPos, clone);
                },
                afterCreate: (node, worldPos, gridPos) =>
                {
                    if (CurrentLevelDesignData.PlayerSpawnCell.x == gridPos.x && CurrentLevelDesignData.PlayerSpawnCell.y == gridPos.y)
                    {
                        var clone = Instantiate(CurrentLevelDesignData.PacmanPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        var character = clone.GetComponent<Character>();
                        character.SetNode(node);
                    }

                    if (CurrentLevelDesignData.IsPointSpawn(gridPos.x, gridPos.y))
                    {
                        var clone = Instantiate(CurrentLevelDesignData.PointPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        node.SetCollectable(clone.GetComponentInChildren<ICollectable>());
                    }
                    
                    if (CurrentLevelDesignData.IsPowerPelletSpawn(gridPos.x, gridPos.y))
                    {
                        var clone = Instantiate(CurrentLevelDesignData.PowerPelletPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        node.SetCollectable(clone.GetComponentInChildren<ICollectable>());
                    }
                    
                    if (CurrentLevelDesignData.IsEnemySpawn(gridPos.x, gridPos.y))
                    {
                        var clone = Instantiate(CurrentLevelDesignData.EnemyPrefab, transform);
                        clone.transform.localPosition = worldPos;
                        clone.GetComponent<Character>().SetNode(node);
                        clone.GetComponent<Ghost>().Config =
                            CurrentLevelDesignData.GetEnemyTypeAt(gridPos.x, gridPos.y);
                    }
                    
                    if (CurrentLevelDesignData.IsWall(gridPos.x, gridPos.y))
                    {
                        node.SetWall();
                    }
                }
            );
        }
        
        private void RenderWallMap()
        {
            var tex = WallTextureGenerator.Generate(CurrentLevelDesignData, _wallMapVisualSettings);

            if (_wallMapRenderer == null)
            {
                var go = new GameObject("WallMap");
                go.transform.SetParent(transform, false);
                _wallMapRenderer = go.AddComponent<SpriteRenderer>();
            }

            var nodeSize = CurrentLevelDesignData.NodeSize;
            var pixelsPerUnit = _wallMapVisualSettings.PixelsPerCell / nodeSize.x;

            _wallMapRenderer.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                Vector2.zero,
                pixelsPerUnit);
            

            _wallMapRenderer.transform.position =
                _grid[0, 0].WorldPosition - new Vector3(nodeSize.x, nodeSize.y, 0f) * 0.5f;
        }

        public Node GetNodeByDirection(Node currentNode, MoveDirection dir)
        {
            var offset = _directionOffsets[dir];
            var p = new Vector2Int(currentNode.GridPosition.x + offset.x, currentNode.GridPosition.y + offset.y);
            return IsPositionInBound(p) ? _grid[p.x, p.y] : null;
        }
        
        private bool IsPositionInBound(Vector2Int pos)
            => pos.x >= 0 && pos.x < CurrentLevelDesignData.GridSize.x && pos.y >= 0 && pos.y < CurrentLevelDesignData.GridSize.y;
        

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

        public MoveDirection GetOppositeDirection(MoveDirection dir)
        {
            return dir switch
            {
                MoveDirection.Up => MoveDirection.Down,
                MoveDirection.Down => MoveDirection.Up,
                MoveDirection.Left => MoveDirection.Right,
                MoveDirection.Right => MoveDirection.Left,
                _ => MoveDirection.None
            };
        }
        
         private int ManhattanDistance(Vector2Int currentPos, Vector2Int targetPos) {
            return (Mathf.Abs(currentPos.x - targetPos.x)) + (Mathf.Abs(currentPos.y - targetPos.y));
        }
         
        private List<Node> GetNeighbors(Vector2Int pos)
        {
            var _neighbors = new List<Node>();
            foreach (var dir in new Vector2Int[]
                     {
                         Vector2Int.right,
                         Vector2Int.left,
                         Vector2Int.up,
                         Vector2Int.down
                     })
            {
                var cPos = pos + dir;
                if(IsPositionInBound(cPos))
                    _neighbors.Add(_grid[cPos.x, cPos.y]);
            }

            return _neighbors;
        }

        public List<Node> GetPath(Vector2Int startPos, Vector2Int targetPos)
        {
            var gCost = new Dictionary<Vector2Int, int>();
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var closed = new HashSet<Vector2Int>();
            var open = new List<Vector2Int>();

            gCost[startPos] = 0;
            open.Add(startPos);

            int F(Vector2Int p) => gCost[p] + ManhattanDistance(p, targetPos);

            while (open.Count > 0)
            {
                var current = open[0];
                foreach (var p in open)
                {
                    if (F(p) < F(current) ||
                        (F(p) == F(current) &&
                         ManhattanDistance(p, targetPos) < ManhattanDistance(current, targetPos)))
                        current = p;
                }

                if (current == targetPos)
                {
                    var path = new List<Node> { _grid[current.x, current.y] };
                    while (parent.TryGetValue(current, out var prev))
                    {
                        path.Add(_grid[prev.x, prev.y]);
                        current = prev;
                    }
                    path.Reverse();
                    return path;
                }

                open.Remove(current);
                closed.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    var np = neighbor.GridPosition;
                    if (neighbor.IsWall || closed.Contains(np)) continue;

                    var g = gCost[current] + 1;
                    if (!gCost.TryGetValue(np, out var oldG) || g < oldG)
                    {
                        gCost[np] = g;
                        parent[np] = current;
                        if (!open.Contains(np)) open.Add(np);
                    }
                }
            }

            return new List<Node>();
        }
    }
}