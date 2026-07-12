#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [CustomEditor(typeof(WallLevelDesignData))]
    public class WallLevelDesignDataEditor : Editor
    {
        private enum PaintMode
        {
            Wall,
            PlayerSpawn,
            EnemySpawn,
            PointSpawn
        }

        private static readonly WallType[] _cycleOrder =
        {
            WallType.Empty, WallType.WallA, WallType.WallB
        };

        private const float CELL_SIZE = 28f;
        private const float CELL_PAD = 10f; // hücreler arası boşluk büyütüldü

        private PaintMode _paintMode = PaintMode.Wall;

        private SerializedProperty _nodeSize;
        private SerializedProperty _worldPositionOffset;
        private SerializedProperty _wallPrefabA;
        private SerializedProperty _wallPrefabB;
        private SerializedProperty _wallIconA;
        private SerializedProperty _wallIconB;
        private SerializedProperty _nodePrefab;
        private SerializedProperty _pacmanPrefab;
        private SerializedProperty _pointPrefab;
        private SerializedProperty _enemyPrefab;

        private void OnEnable()
        {
            _nodeSize = serializedObject.FindProperty("_nodeSize");
            _worldPositionOffset = serializedObject.FindProperty("_worldPositionOffset");
            _wallPrefabA = serializedObject.FindProperty("_wallPrefabA");
            _wallPrefabB = serializedObject.FindProperty("_wallPrefabB");
            _wallIconA = serializedObject.FindProperty("_wallIconA");
            _wallIconB = serializedObject.FindProperty("_wallIconB");
            _nodePrefab = serializedObject.FindProperty("_nodePrefab");
            _pacmanPrefab = serializedObject.FindProperty("_pacmanPrefab");
            _pointPrefab = serializedObject.FindProperty("_pointPrefab");
            _enemyPrefab = serializedObject.FindProperty("_enemyPrefab");
        }

        public override void OnInspectorGUI()
        {
            var data = (WallLevelDesignData)target;

            var newWidth = Mathf.Max(1, EditorGUILayout.IntField("GridWidth", data.GridWidth));
            var newHeight = Mathf.Max(1, EditorGUILayout.IntField("GridHeight", data.GridHeight));

            serializedObject.Update();

            EditorGUILayout.PropertyField(_nodeSize);
            EditorGUILayout.PropertyField(_worldPositionOffset);
            EditorGUILayout.PropertyField(_wallPrefabA);
            EditorGUILayout.PropertyField(_wallPrefabB);
            EditorGUILayout.PropertyField(_wallIconA);
            EditorGUILayout.PropertyField(_wallIconB);
            EditorGUILayout.PropertyField(_nodePrefab);
            EditorGUILayout.PropertyField(_pacmanPrefab);
            EditorGUILayout.PropertyField(_pointPrefab);
            EditorGUILayout.PropertyField(_enemyPrefab);

            serializedObject.ApplyModifiedProperties();

            if (newWidth != data.GridWidth || newHeight != data.GridHeight)
            {
                Undo.RecordObject(data, "Resize Main Grid");
                data.GridWidth = newWidth;
                data.GridHeight = newHeight;

                data.EnsureSize();
                EditorUtility.SetDirty(data);
            }

            data.EnsureSize();

            GUILayout.Space(8);
            DrawLegend(data);

            GUILayout.Space(10);
            DrawModeToolbar(data);

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Main Grid (Cells)  (0,0 = bottom-left)", EditorStyles.boldLabel);

            if (_paintMode == PaintMode.Wall)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Fill Empty")) PaintFillMain(data, WallType.Empty);
                    if (GUILayout.Button("Fill Wall A")) PaintFillMain(data, WallType.WallA);
                    if (GUILayout.Button("Fill Wall B")) PaintFillMain(data, WallType.WallB);
                    if (GUILayout.Button("Fill Random")) PaintRandomMain(data);
                }
            }
            else if (_paintMode == PaintMode.EnemySpawn)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Clear Enemy Spawns"))
                    {
                        Undo.RecordObject(data, "Clear Enemy Spawns");
                        data.ClearEnemySpawns();
                        EditorUtility.SetDirty(data);
                    }
                }
            }
            else if (_paintMode == PaintMode.PlayerSpawn)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Clear Player Spawn"))
                    {
                        Undo.RecordObject(data, "Clear Player Spawn");
                        data.ClearPlayerSpawn();
                        EditorUtility.SetDirty(data);
                    }
                }
            }
            else if (_paintMode == PaintMode.PointSpawn)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Clear Point Spawns"))
                    {
                        Undo.RecordObject(data, "Clear Point Spawns");
                        data.ClearPointSpawns();
                        EditorUtility.SetDirty(data);
                    }
                }
            }

            GUILayout.Space(6);
            DrawGridMain(data);

            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Wall Count: {data.GetWallAmount()}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(
                data.HasPlayerSpawn ? $"Player Spawn: {data.PlayerSpawnCell}" : "Player Spawn: (not set)",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Enemy Spawns: {data.EnemySpawnCells.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Point Spawns: {data.PointSpawnCells.Count}", EditorStyles.miniBoldLabel);
        }

        private void DrawModeToolbar(WallLevelDesignData data)
        {
            EditorGUILayout.LabelField("Paint Mode", EditorStyles.miniBoldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawModeButton(PaintMode.Wall, "Wall");
                DrawModeButton(PaintMode.PlayerSpawn, "Player Spawn");
                DrawModeButton(PaintMode.EnemySpawn, "Enemy Spawn");
                DrawModeButton(PaintMode.PointSpawn, "Point Spawn");
            }
        }

        private void DrawModeButton(PaintMode mode, string label)
        {
            var isActive = _paintMode == mode;
            var prevColor = GUI.backgroundColor;
            if (isActive) GUI.backgroundColor = new Color(0.35f, 0.65f, 1f, 1f);

            if (GUILayout.Button(label, GUILayout.Height(24)))
                _paintMode = mode;

            GUI.backgroundColor = prevColor;
        }

        private static void PaintRandomMain(WallLevelDesignData data)
        {
            Undo.RecordObject(data, "Randomize Main Grid");

            for (var y = 0; y < data.GridHeight; y++)
            {
                for (var x = 0; x < data.GridWidth; x++)
                {
                    var randomType = (WallType)Random.Range(0, _cycleOrder.Length);
                    data.Set(x, y, randomType);
                }
            }

            EditorUtility.SetDirty(data);
        }

        private static void PaintFillMain(WallLevelDesignData data, WallType t)
        {
            Undo.RecordObject(data, "Fill Main Grid");
            data.Fill(t);
            EditorUtility.SetDirty(data);
        }

        private void DrawGridMain(WallLevelDesignData data)
        {
            for (var y = data.GridHeight - 1; y >= 0; y--)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space(2);
                    for (var x = 0; x < data.GridWidth; x++)
                    {
                        DrawCell(data, x, y);
                    }
                }

                GUILayout.Space(CELL_PAD);
            }
        }

        private void DrawCell(WallLevelDesignData data, int x, int y)
        {
            var r = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE, GUILayout.Width(CELL_SIZE),
                GUILayout.Height(CELL_SIZE));

            var current = data.Get(x, y);

            // Boş hücreler için sabit koyu arka plan
            EditorGUI.DrawRect(r, new Color(0.18f, 0.18f, 0.18f, 1f));

            var icon = data.GetIconFor(current);
            if (icon != null)
            {
                var padded = new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4);
                GUI.DrawTexture(padded, icon, ScaleMode.ScaleToFit);
            }

            // Player / Enemy / Point spawn overlay (moda bakılmaksızın her zaman görünür)
            var isPlayerSpawn = data.IsPlayerSpawn(x, y);
            var isEnemySpawn = data.IsEnemySpawn(x, y);
            var isPointSpawn = data.IsPointSpawn(x, y);

            if (isPlayerSpawn)
                DrawBorder(r, new Color(0.3f, 0.6f, 1f, 1f), 2f);

            if (isEnemySpawn)
                DrawBorder(r, new Color(1f, 0.3f, 0.3f, 1f), 2f);

            if (isPointSpawn)
                DrawBorder(r, new Color(1f, 0.85f, 0.2f, 1f), 2f);

            if (isPlayerSpawn || isEnemySpawn || isPointSpawn)
            {
                var label = isPlayerSpawn ? "P" : isEnemySpawn ? "E" : "$";
                var labelColor = isPlayerSpawn
                    ? new Color(0.3f, 0.6f, 1f)
                    : isEnemySpawn
                        ? new Color(1f, 0.3f, 0.3f)
                        : new Color(1f, 0.85f, 0.2f);

                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = 9,
                    normal = { textColor = labelColor }
                };
                GUI.Label(r, label, style);
            }

            if (GUI.Button(r, GUIContent.none, GUIStyle.none))
                OnCellClicked(data, x, y, current);

            GUILayout.Space(CELL_PAD);
        }

        private void OnCellClicked(WallLevelDesignData data, int x, int y, WallType current)
        {
            switch (_paintMode)
            {
                case PaintMode.Wall:
                    Undo.RecordObject(data, "Paint Main Cell");
                    data.Set(x, y, Cycle(current, +1));
                    break;

                case PaintMode.PlayerSpawn:
                    Undo.RecordObject(data, "Set Player Spawn");
                    data.SetPlayerSpawn(x, y);
                    break;

                case PaintMode.EnemySpawn:
                    Undo.RecordObject(data, "Toggle Enemy Spawn");
                    data.ToggleEnemySpawn(x, y);
                    break;

                case PaintMode.PointSpawn:
                    Undo.RecordObject(data, "Toggle Point Spawn");
                    data.TogglePointSpawn(x, y);
                    break;
            }

            EditorUtility.SetDirty(data);
        }

        private static void DrawBorder(Rect r, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), color);
            EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), color);
        }

        private static void DrawLegend(WallLevelDesignData data)
        {
            EditorGUILayout.LabelField("Click a cell to cycle: Empty → Wall A → Wall B", EditorStyles.miniBoldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLegendIcon("Empty", null);
                DrawLegendIcon("Wall A", data.WallIconA);
                DrawLegendIcon("Wall B", data.WallIconB);
            }
        }

        private static void DrawLegendIcon(string label, Texture2D icon)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(90)))
            {
                var r = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
                EditorGUI.DrawRect(r, new Color(0.18f, 0.18f, 0.18f, 1f));
                if (icon != null) GUI.DrawTexture(r, icon, ScaleMode.ScaleToFit);
                GUILayout.Label(label, EditorStyles.miniLabel);
            }
        }

        private static WallType Cycle(WallType current, int dir)
        {
            var idx = 0;
            for (var i = 0; i < _cycleOrder.Length; i++)
            {
                if (_cycleOrder[i] == current)
                {
                    idx = i;
                    break;
                }
            }

            idx = (idx + dir) % _cycleOrder.Length;
            if (idx < 0) idx += _cycleOrder.Length;
            return _cycleOrder[idx];
        }
    }
}
#endif