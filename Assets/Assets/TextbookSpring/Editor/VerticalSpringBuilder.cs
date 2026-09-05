using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TextbookSpring.Editor
{
    public static class VerticalSpringBuilder
    {
        [MenuItem("Tools/Textbook Spring/Create Vertical Spring Prefab")]
        public static void Create()
        {
            string folder = AssetDatabase.GenerateUniqueAssetPath("Assets/TextbookSpringModel");
            AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
            Shader shader = Shader.Find(GraphicsSettings.currentRenderPipeline == null ? "Standard" : "Universal Render Pipeline/Lit");
            if (shader == null) { Debug.LogError("This builder supports Built-in and URP. Assign a compatible shader for another pipeline."); return; }
            Material silver = MaterialAsset(folder, "SpringSilver", shader, new Color(0.62f, 0.67f, 0.72f), 0.65f);
            Material blue = MaterialAsset(folder, "SupportBlue", shader, new Color(0.12f, 0.28f, 0.52f), 0.1f);
            Material orange = MaterialAsset(folder, "BobOrange", shader, new Color(1f, 0.32f, 0.035f), 0.15f);
            GameObject root = new GameObject("VerticalSpring");
            try
            {
                GameObject support = Part("FixedSupport", PrimitiveType.Cube, root.transform, blue);
                support.transform.localPosition = new Vector3(0f, 0.012f, 0f);
                support.transform.localScale = new Vector3(0.16f, 0.024f, 0.09f);
                GameObject bob = Part("Bob", PrimitiveType.Sphere, root.transform, orange);
                GameObject coil = new GameObject("Spring");
                coil.transform.SetParent(root.transform, false);
                MeshFilter filter = coil.AddComponent<MeshFilter>();
                coil.AddComponent<MeshRenderer>().sharedMaterial = silver;
                VerticalSpring simulation = root.AddComponent<VerticalSpring>();
                simulation.spring = filter;
                simulation.bob = bob.transform;
                Mesh mesh = simulation.CreatePreviewMesh();
                AssetDatabase.CreateAsset(mesh, folder + "/SpringPreview.asset");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, folder + "/VerticalSpring.prefab");
                AssetDatabase.SaveAssets();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created " + folder + "/VerticalSpring.prefab. Drag it into your scene or AR content root.");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static GameObject Part(string name, PrimitiveType type, Transform parent, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        static Material MaterialAsset(string folder, string name, Shader shader, Color color, float metallic)
        {
            Material material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.45f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", 0.45f);
            AssetDatabase.CreateAsset(material, folder + "/" + name + ".mat");
            return material;
        }
    }
}
