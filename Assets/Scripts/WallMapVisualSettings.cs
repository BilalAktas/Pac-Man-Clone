using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Wall Map Visual Settings", fileName = "WallMapVisualSettings")]
    public class WallMapVisualSettings : ScriptableObject
    {
        [Min(1)] public int PixelsPerCell = 24;
        public Color BackgroundColor = new Color(0, 0, 0, 0);
        public Color WallColor = new Color(0.2f, 0.5f, 1.0f);
        [Range(1, 32)] public int WallThickness = 4;
        [Range(0, 16)] public int CornerRadius = 6;
    }
}