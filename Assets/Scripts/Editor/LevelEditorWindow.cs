using UnityEditor;
using UnityEngine;

namespace PopperBurst
{
    public class LevelEditorWindow : EditorWindow
    {
        private LevelData levelData;
        private int selectedRow, selectedCol;
        private PopperType selectedFrogType = PopperType.Yellow;

        [MenuItem("Tools/Popper Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Popper Level Editor");
        }

        void OnGUI()
        {
            GUILayout.Label("Popper Level Editor", EditorStyles.boldLabel);
            levelData = (LevelData)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelData), false);

            if (levelData == null) return;

            levelData.rows = EditorGUILayout.IntField("Rows", levelData.rows);
            levelData.cols = EditorGUILayout.IntField("Cols", levelData.cols);

            if (GUILayout.Button("Initialize Grid"))
            {
                levelData.gridData = new PopperType[levelData.rows * levelData.cols];
                EditorUtility.SetDirty(levelData);
            }

            selectedFrogType = (PopperType)EditorGUILayout.EnumPopup("Selected Popper Type", selectedFrogType);

            for (int y = 0; y < levelData.rows; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = 0; x < levelData.cols; x++)
                {
                    int index = y * levelData.cols + x;
                    GUI.color = GetColor(levelData.gridData[index]);

                    if (GUILayout.Button(levelData.gridData[index].ToString(), GUILayout.Width(60)))
                    {
                        levelData.gridData[index] = selectedFrogType;
                        EditorUtility.SetDirty(levelData);
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUI.color = Color.white;
            if (GUILayout.Button("Save Level"))
            {
                EditorUtility.SetDirty(levelData);
                AssetDatabase.SaveAssets();
            }
        }

        private Color GetColor(PopperType type)
        {
            return type switch
            {
                PopperType.Blue => Color.blue,
                PopperType.Yellow => Color.yellow,
                PopperType.Purple => Color.cyan,
                _ => Color.gray,
            };
        }
    }
}