using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class Node
    {
        public Vector3 WorldPosition { get; private set; }
        public Vector2Int GridPosition { get; private set; }
        public GameObject Visual { get; private set; }
        public List<Character> Characters { get; private set; }
        public bool IsWall  { get; private set; }
        
        public ICollectable Collectable { get; private set; }
        

        public Node(Vector3 worldPosition, Vector2Int gridPosition, GameObject visual)
        {
            WorldPosition = worldPosition;
            GridPosition = gridPosition;
            if(visual) Visual = visual;
            Characters = new List<Character>();
        }

        public void AddCharacter(Character character) => Characters.Add(character);
        public void RemoveCharacter(Character character) => Characters.Remove(character);
        
        public void SetWall() => IsWall = true;
        
        public void SetCollectable(ICollectable collectable) => Collectable = collectable;

        public bool Collect()
        {
            if (Collectable == null) return false;
            
            var collectable = Collectable;
            SetCollectable(null);

            collectable.Collect();
            EventBus.Raise(new OnCollectableCollectedEvent());
            return true;
        }

        public bool IsWalkable => !IsWall;
    }
}