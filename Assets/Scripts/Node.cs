using UnityEngine;

namespace Core
{
    public class Node
    {
        public Vector3 WorldPosition { get; set; }
        public Vector2Int GridPosition { get; set; }
        public Character Character { get; set; }
        
        public Node(Vector3 worldPosition, Vector2Int gridPosition)
        {
            WorldPosition = worldPosition;
            GridPosition = gridPosition;
        }
        
        public void SetCharacter(Character character) => Character = character;
    }
}