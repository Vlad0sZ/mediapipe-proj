
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Runtime.Game.ScriptableData;

public class FoodCreateWindow : EditorWindow
{
    [MenuItem("Tools/Food Object Creator")]
    public static void ShowExample()
    {
        FoodCreateWindow wnd = GetWindow<FoodCreateWindow>();
        wnd.titleContent = new GUIContent("Food Object Creator");
        wnd.minSize = new Vector2(400, 250);
    }

    private DefaultAsset prefabsFolder;
    private DefaultAsset textureAtlasFolder;
    private DefaultAsset outputFolder;
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 1. Folder Field для папки с префабами
        EditorGUILayout.LabelField("Папка с префабами", EditorStyles.boldLabel);
        prefabsFolder =
            (DefaultAsset) EditorGUILayout.ObjectField("Prefabs Folder", prefabsFolder, typeof(DefaultAsset), false);

        GUILayout.Space(15);

        // 2. Texture Atlas Field для атласа
        EditorGUILayout.LabelField("Папка с текстурами", EditorStyles.boldLabel);
        textureAtlasFolder = (DefaultAsset) EditorGUILayout.ObjectField("Texture Atlas Folder", textureAtlasFolder,
            typeof(DefaultAsset), false);

        GUILayout.Space(15);

        // 3. Folder Field для конечной папки
        EditorGUILayout.LabelField("Выходная папка", EditorStyles.boldLabel);
        outputFolder =
            (DefaultAsset) EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);

        GUILayout.Space(25);

        // 4. Кнопка "начать"
        EditorGUI.BeginDisabledGroup(!IsReadyToProcess());
        if (GUILayout.Button("Начать создание Food Objects", GUILayout.Height(40)))
        {
            ProcessFoodObjects();
        }

        if (GUILayout.Button("Переименовать", GUILayout.Height(40)))
        {
            ProcessRename();
        }

        EditorGUI.EndDisabledGroup();

        // Сообщение о готовности
        if (!IsReadyToProcess())
        {
            EditorGUILayout.HelpBox("Заполните все поля для начала работы", MessageType.Info);
        }

        EditorGUILayout.EndScrollView();
    }

    private bool IsReadyToProcess()
    {
        return prefabsFolder != null && textureAtlasFolder != null && outputFolder != null;
    }

    private void ProcessRename()
    {
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);
        string[] prefabGuids = AssetDatabase.FindAssets($"t:{typeof(FoodWithIcon)}", new[] {outputPath});

        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefabName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            if (prefabName.StartsWith("Food_"))
                AssetDatabase.RenameAsset(prefabPath, prefabName.Replace("Food_", ""));
        }
    }

    private void ProcessFoodObjects()
    {
        string prefabsPath = AssetDatabase.GetAssetPath(prefabsFolder);
        string texturesPath = AssetDatabase.GetAssetPath(textureAtlasFolder);
        string outputPath = AssetDatabase.GetAssetPath(outputFolder);

        // Находим все префабы в папке
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] {prefabsPath});

        if (prefabGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Ошибка", "В указанной папке не найдено префабов", "OK");
            return;
        }

        // Находим все текстуры в папке
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] {texturesPath});

        if (textureGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("Ошибка", "В указанной папке не найдено текстур", "OK");
            return;
        }

        // Создаем словарь текстур для быстрого поиска
        Dictionary<string, Sprite> textureDictionary = new Dictionary<string, Sprite>();

        foreach (string textureGuid in textureGuids)
        {
            string texturePath = AssetDatabase.GUIDToAssetPath(textureGuid);
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();

            foreach (Sprite sprite in sprites)
            {
                if (!textureDictionary.ContainsKey(sprite.name))
                {
                    textureDictionary.Add(sprite.name, sprite);
                }
            }
        }

        int createdCount = 0;
        int skippedCount = 0;

        // Обрабатываем каждый префаб
        foreach (string prefabGuid in prefabGuids)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null) continue;

            string prefabName = prefab.name;
            string expectedTextureName = $"Food_{prefabName}";

            // Ищем соответствующую текстуру
            if (textureDictionary.TryGetValue(expectedTextureName, out Sprite sprite))
            {
                CreateFoodObject(prefab, sprite, outputPath, prefabName);
                createdCount++;
            }
            else
            {
                Debug.LogWarning($"Не найдена текстура для префаба {prefabName}. Ожидаемое имя: {expectedTextureName}");
                skippedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Готово",
            $"Создано Food Objects: {createdCount}\nПропущено: {skippedCount}",
            "OK");
    }

    private void CreateFoodObject(GameObject prefab, Sprite sprite, string outputPath, string foodName)
    {
        // Создаем ScriptableObject
        FoodWithIcon foodObject = ScriptableObject.CreateInstance<FoodWithIcon>();

        // Устанавливаем поля через SerializedObject для обхода readonly полей
        SerializedObject serializedObject = new SerializedObject(foodObject);
        serializedObject.FindProperty("prefab").objectReferenceValue = prefab;
        serializedObject.FindProperty("image").objectReferenceValue = sprite;
        serializedObject.ApplyModifiedProperties();

        // Создаем ассет
        string assetPath = Path.Combine(outputPath, $"Food_{foodName}.asset");

        // Убеждаемся, что файл с таким именем не существует
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(foodObject, assetPath);
        Debug.Log($"Создан Food Object: {assetPath}");
    }
}
#endif