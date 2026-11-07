#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [CustomEditor(typeof(WallLevelDesignData))]
    public class WallLevelDesignDataEditor : Editor
    {
        private enum PaintMode { Wall, PlayerSpawn, EnemySpawn, EnemyBase, PointSpawn, PowerPellet }

        private const float CELL_SIZE = 32f;
        private const float CELL_PAD = 10f;

        private static readonly Color WallColor = new Color(0.25f, 0.55f, 1f, 1f);

        private PaintMode _paintMode = PaintMode.Wall;

        private SerializedProperty _nodeSize;
        private SerializedProperty _worldPositionOffset;
        private SerializedProperty _nodePrefab;
        private SerializedProperty _pacmanPrefab;
        private SerializedProperty _pointPrefab;
        private SerializedProperty _powerPelletPrefab;
        private SerializedProperty _enemyPrefab;
        private SerializedProperty _enemyTypePalette;

        private void OnEnable()
        {
            _nodeSize = serializedObject.FindProperty("_nodeSize");
            _worldPositionOffset = serializedObject.FindProperty("_worldPositionOffset");
            _nodePrefab = serializedObject.FindProperty("_nodePrefab");
            _pacmanPrefab = serializedObject.FindProperty("_pacmanPrefab");
            _pointPrefab = serializedObject.FindProperty("_pointPrefab");
            _powerPelletPrefab = serializedObject.FindProperty("_powerPelletPrefab");
            _enemyPrefab = serializedObject.FindProperty("_enemyPrefab");
            _enemyTypePalette = serializedObject.FindProperty("_enemyTypePalette");
        }

        public override void OnInspectorGUI()
        {
            var data = (WallLevelDesignData)target;

            var newWidth = Mathf.Max(1, EditorGUILayout.IntField("GridWidth", data.GridWidth));
            var newHeight = Mathf.Max(1, EditorGUILayout.IntField("GridHeight", data.GridHeight));

            serializedObject.Update();

            EditorGUILayout.PropertyField(_nodeSize);
            EditorGUILayout.PropertyField(_worldPositionOffset);
            EditorGUILayout.PropertyField(_nodePrefab);
            EditorGUILayout.PropertyField(_pacmanPrefab);
            EditorGUILayout.PropertyField(_pointPrefab);
            EditorGUILayout.PropertyField(_powerPelletPrefab);
            EditorGUILayout.PropertyField(_enemyPrefab);

            GUILayout.Space(4);
            EditorGUILayout.PropertyField(_enemyTypePalette, new GUIContent("Enemy Type Palette (GhostConfig)"), true);

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

            GUILayout.Space(10);
            DrawModeToolbar();

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Main Grid (Cells)  (0,0 = bottom-left)", EditorStyles.boldLabel);

            if (_paintMode == PaintMode.Wall)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Fill Empty")) PaintFill(data, false);
                    if (GUILayout.Button("Fill All Wall")) PaintFill(data, true);
                    if (GUILayout.Button("Fill Random")) PaintRandom(data);
                }
            }
            else if (_paintMode == PaintMode.EnemySpawn)
            {
                EditorGUILayout.HelpBox(
                    "Sol tık: sıradaki Enemy tipine geç (palet biterse boşa döner).\nSağ tık: bir önceki tipe dön.",
                    MessageType.Info);

                if (GUILayout.Button("Clear Enemy Spawns"))
                {
                    Undo.RecordObject(data, "Clear Enemy Spawns");
                    data.ClearEnemySpawns();
                    EditorUtility.SetDirty(data);
                }

                GUILayout.Space(4);
                DrawEnemyPaletteLegend(data);
            }
            else if (_paintMode == PaintMode.PlayerSpawn)
            {
                if (GUILayout.Button("Clear Player Spawn"))
                {
                    Undo.RecordObject(data, "Clear Player Spawn");
                    data.ClearPlayerSpawn();
                    EditorUtility.SetDirty(data);
                }
            }
            else if (_paintMode == PaintMode.EnemyBase)
            {
                if (GUILayout.Button("Clear Enemy Base"))
                {
                    Undo.RecordObject(data, "Clear Enemy Base");
                    data.ClearEnemyBase();
                    EditorUtility.SetDirty(data);
                }
            }
            else if (_paintMode == PaintMode.PointSpawn)
            {
                if (GUILayout.Button("Clear Point Spawns"))
                {
                    Undo.RecordObject(data, "Clear Point Spawns");
                    data.ClearPointSpawns();
                    EditorUtility.SetDirty(data);
                }
            }
            else if (_paintMode == PaintMode.PowerPellet)
            {
                if (GUILayout.Button("Clear PowerPellet Spawns"))
                {
                    Undo.RecordObject(data, "Clear PowerPellet Spawns");
                    data.ClearPowerPelletSpawns();
                    EditorUtility.SetDirty(data);
                }
            }

            GUILayout.Space(6);
            DrawGridMain(data);

            GUILayout.Space(10);
            EditorGUILayout.LabelField($"Wall Count: {data.GetWallAmount()}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(
                data.HasPlayerSpawn ? $"Player Spawn: {data.PlayerSpawnCell}" : "Player Spawn: (not set)",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField(
                data.HasEnemyBase ? $"Enemy Base: {data.EnemyBaseCell}" : "Enemy Base: (not set)",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Enemy Spawns: {data.EnemySpawnCells.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"Point Spawns: {data.PointSpawnCells.Count}", EditorStyles.miniBoldLabel);
            EditorGUILayout.LabelField($"PowerPellet Spawns: {data.PowerPelletSpawnCells.Count}", EditorStyles.miniBoldLabel);
        }

        private void DrawModeToolbar()
        {
            EditorGUILayout.LabelField("Paint Mode", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawModeButton(PaintMode.Wall, "Wall");
                DrawModeButton(PaintMode.PlayerSpawn, "Player Spawn");
                DrawModeButton(PaintMode.EnemySpawn, "Enemy Spawn");
                DrawModeButton(PaintMode.EnemyBase, "Enemy Base");
                DrawModeButton(PaintMode.PointSpawn, "Point Spawn");
                DrawModeButton(PaintMode.PowerPellet, "PowerPellet Spawn");
            }
        }

        private void DrawModeButton(PaintMode mode, string label)
        {
            var isActive = _paintMode == mode;
            var prevColor = GUI.backgroundColor;
            if (isActive) GUI.backgroundColor = new Color(0.35f, 0.65f, 1f, 1f);
            if (GUILayout.Button(label, GUILayout.Height(24))) _paintMode = mode;
            GUI.backgroundColor = prevColor;
        }

        private static void PaintRandom(WallLevelDesignData data)
        {
            Undo.RecordObject(data, "Randomize Main Grid");
            for (var y = 0; y < data.GridHeight; y++)
                for (var x = 0; x < data.GridWidth; x++)
                    data.SetWall(x, y, Random.value > 0.5f);
            EditorUtility.SetDirty(data);
        }

        private static void PaintFill(WallLevelDesignData data, bool value)
        {
            Undo.RecordObject(data, "Fill Main Grid");
            data.Fill(value);
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
                        DrawCell(data, x, y);
                }
                GUILayout.Space(CELL_PAD);
            }
        }

        private void DrawCell(WallLevelDesignData data, int x, int y)
        {
            var r = GUILayoutUtility.GetRect(CELL_SIZE, CELL_SIZE, GUILayout.Width(CELL_SIZE), GUILayout.Height(CELL_SIZE));

            var isWall = data.IsWall(x, y);

            // Wall hücreleri dolgu rengiyle, boş hücreler koyu arka planla gösterilir
            EditorGUI.DrawRect(r, isWall ? WallColor : new Color(0.18f, 0.18f, 0.18f, 1f));

            var isPlayerSpawn = data.IsPlayerSpawn(x, y);
            var isEnemySpawn = data.IsEnemySpawn(x, y);
            var isEnemyBase = data.IsEnemyBase(x, y);
            var isPointSpawn = data.IsPointSpawn(x, y);
            var isPowerPelletSpawn = data.IsPowerPelletSpawn(x, y);
            var enemyType = isEnemySpawn ? data.GetEnemyTypeAt(x, y) : null;
            var enemyIndex = enemyType != null ? System.Array.IndexOf(data.EnemyTypePalette, enemyType) : -1;

            if (isPlayerSpawn) DrawBorder(r, new Color(0.3f, 0.6f, 1f, 1f), 2f);
            if (isEnemySpawn) DrawBorder(r, new Color(1f, 0.3f, 0.3f, 1f), 2f);
            if (isEnemyBase) DrawBorder(r, new Color(0.75f, 0.4f, 1f, 1f), 2f);
            if (isPointSpawn) DrawBorder(r, new Color(1f, 0.85f, 0.2f, 1f), 2f);
            if (isPowerPelletSpawn) DrawBorder(r, new Color(.7f, 0.85f, 0.2f, 1f), 2f);

            if (isEnemySpawn && enemyType != null)
            {
                var enemyIcon = AssetPreview.GetMiniThumbnail(enemyType);
                if (enemyIcon != null)
                {
                    var padded = new Rect(r.x + 6, r.y + 6, r.width - 12, r.height - 12);
                    GUI.DrawTexture(padded, enemyIcon, ScaleMode.ScaleToFit);
                }
                GUI.Label(r, new GUIContent(string.Empty, enemyType.name));
            }

            if (isPlayerSpawn) DrawCornerLabel(r, "P", new Color(0.35f, 0.7f, 1f), TextAnchor.UpperLeft);
            if (isEnemySpawn) DrawCornerLabel(r, enemyIndex >= 0 ? $"E{enemyIndex + 1}" : "E?", new Color(1f, 0.4f, 0.4f), TextAnchor.UpperRight);
            if (isPointSpawn) DrawCornerLabel(r, "$", new Color(1f, 0.9f, 0.3f), TextAnchor.LowerLeft);
            if (isPowerPelletSpawn) DrawCornerLabel(r, "PP", new Color(0.8f, 0.95f, 0.3f), TextAnchor.LowerRight);
            if (isEnemyBase) DrawCornerLabel(r, "EB", new Color(0.85f, 0.55f, 1f), TextAnchor.MiddleCenter);

            HandleCellInput(data, r, x, y);

            GUILayout.Space(CELL_PAD);
        }

        private static void DrawCornerLabel(Rect r, string text, Color color, TextAnchor anchor)
        {
            var style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = anchor,
                fontSize = 9,
                normal = { textColor = color }
            };
            var padded = new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2);
            GUI.Label(padded, text, style);
        }

        private void HandleCellInput(WallLevelDesignData data, Rect r, int x, int y)
        {
            var evt = Event.current;
            if (evt.type != EventType.MouseDown || !r.Contains(evt.mousePosition))
                return;

            var dir = evt.button == 1 ? -1 : 1;
            OnCellClicked(data, x, y, dir);

            evt.Use();
            EditorUtility.SetDirty(data);
        }

        private void OnCellClicked(WallLevelDesignData data, int x, int y, int dir)
        {
            switch (_paintMode)
            {
                case PaintMode.Wall:
                    Undo.RecordObject(data, "Toggle Wall");
                    data.ToggleWall(x, y);
                    break;

                case PaintMode.PlayerSpawn:
                    Undo.RecordObject(data, "Set Player Spawn");
                    data.SetPlayerSpawn(x, y);
                    break;

                case PaintMode.EnemySpawn:
                    Undo.RecordObject(data, "Cycle Enemy Spawn");
                    data.CycleEnemySpawn(x, y, dir);
                    break;

                case PaintMode.EnemyBase:
                    Undo.RecordObject(data, "Toggle Enemy Base");
                    data.ToggleEnemyBase(x, y);
                    break;

                case PaintMode.PointSpawn:
                    Undo.RecordObject(data, "Toggle Point Spawn");
                    data.TogglePointSpawn(x, y);
                    break;

                case PaintMode.PowerPellet:
                    Undo.RecordObject(data, "Toggle PowerPellet Spawn");
                    data.TogglePowerPelletSpawn(x, y);
                    break;
            }
        }

        private static void DrawBorder(Rect r, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), color);
            EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), color);
            EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), color);
        }

        private static void DrawEnemyPaletteLegend(WallLevelDesignData data)
        {
            var palette = data.EnemyTypePalette;
            if (palette == null || palette.Length == 0)
            {
                EditorGUILayout.HelpBox("Enemy Type Palette boş. Yukarıdan en az bir GhostConfig ekleyin.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Enemy Palette (hücredeki E{n} numarası bu listeye karşılık gelir)", EditorStyles.miniBoldLabel);

            for (var i = 0; i < palette.Length; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var r = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
                    EditorGUI.DrawRect(r, new Color(0.18f, 0.18f, 0.18f, 1f));

                    if (palette[i] != null)
                    {
                        var thumb = AssetPreview.GetMiniThumbnail(palette[i]);
                        if (thumb != null) GUI.DrawTexture(r, thumb, ScaleMode.ScaleToFit);
                    }

                    var name = palette[i] != null ? palette[i].name : "(null)";
                    GUILayout.Label($"E{i + 1}  —  {name}", EditorStyles.miniLabel);
                }
            }
        }
    }
}
#endif