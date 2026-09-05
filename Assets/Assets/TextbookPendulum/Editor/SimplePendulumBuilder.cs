using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace TextbookPendulum.Editor
{
    public static class SimplePendulumBuilder
    {
        [MenuItem("Tools/Textbook Pendulum/Create Simple Pendulum Prefab")]
        public static void Create()
        {
            bool srp = GraphicsSettings.currentRenderPipeline != null;
            Shader lit = Shader.Find(srp ? "Universal Render Pipeline/Lit" : "Standard");
            Shader unlit = Shader.Find(srp ? "Universal Render Pipeline/Unlit" : "Unlit/Color");
            if (!lit || !unlit) { Debug.LogError("Builder supports Built-in and URP. Adapt shaders for other pipelines."); return; }
            string folder = AssetDatabase.GenerateUniqueAssetPath("Assets/SimplePendulumModel");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            Material blue = MaterialAsset(folder,"SupportBlue",lit,new Color(.04f,.5f,.72f));
            Material orange = MaterialAsset(folder,"BobOrange",lit,new Color(1f,.35f,.035f));
            Material cordMat = MaterialAsset(folder,"Cord",lit,new Color(.12f,.25f,.31f));
            Material guideMat = MaterialAsset(folder,"Guides",unlit,new Color(.05f,.65f,.82f));
            Material arcMat = MaterialAsset(folder,"Displacement",unlit,new Color(1f,.43f,.05f));
            GameObject root = new GameObject("SimplePendulum");
            try
            {
                Transform p=root.transform;
                SimplePendulum sim=root.AddComponent<SimplePendulum>();
                Part("FixedSupport",PrimitiveType.Cube,p,blue,new Vector3(0f,.012f,0f),new Vector3(.16f,.024f,.085f));
                Part("Pivot",PrimitiveType.Sphere,p,cordMat,Vector3.zero,Vector3.one*.013f);
                sim.cord=Part("Cord",PrimitiveType.Cylinder,p,cordMat,Vector3.zero,Vector3.one);
                sim.bob=Part("Bob",PrimitiveType.Sphere,p,orange,Vector3.zero,Vector3.one*.044f);
                sim.annotations=new GameObject("Annotations");sim.annotations.transform.SetParent(p,false);
                Transform a=sim.annotations.transform;
                GameObject guide=new GameObject("DashedGuides");guide.transform.SetParent(a,false);
                sim.guides=guide.AddComponent<MeshFilter>();guide.AddComponent<MeshRenderer>().sharedMaterial=guideMat;
                sim.angleArc=Arc("AngleAlpha",a,guideMat,.0015f);
                sim.displacementArc=Arc("ArcDisplacementS",a,arcMat,.002f);
                sim.angleLabel=Label("α",a);
                sim.lengthLabel=Label("l",a);
                sim.displacementLabel=Label("s",a);
                sim.equilibriumLabel=Label("O",a);
                sim.zeroPotentialLabel=Label("Wt = 0",a);
                Mesh preview=sim.CreatePreviewMesh();sim.guides.sharedMesh=preview;
                AssetDatabase.CreateAsset(preview,folder+"/GuidesPreview.asset");
                GameObject prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/SimplePendulum.prefab");
                AssetDatabase.SaveAssets();Selection.activeObject=prefab;EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created "+folder+"/SimplePendulum.prefab. Root is the suspension point; view from local negative Z.");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static Material MaterialAsset(string folder,string name,Shader shader,Color color)
        {
            Material m=new Material(shader){name=name,color=color};
            if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",.4f);
            AssetDatabase.CreateAsset(m,folder+"/"+name+".mat");return m;
        }
        static Transform Part(string name,PrimitiveType type,Transform parent,Material material,Vector3 pos,Vector3 scale)
        {
            GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        static LineRenderer Arc(string name,Transform parent,Material material,float width)
        {
            GameObject go=new GameObject(name);go.transform.SetParent(parent,false);
            LineRenderer line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=material;
            line.startWidth=width;line.endWidth=width;line.positionCount=25;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;return line;
        }
        static Transform Label(string label,Transform parent)
        {
            GameObject go=new GameObject("Label_"+label);go.transform.SetParent(parent,false);
            TextMesh text=go.AddComponent<TextMesh>();text.text=label;text.fontSize=64;
            text.characterSize=.033f;text.anchor=TextAnchor.MiddleCenter;text.color=new Color(.06f,.13f,.19f);
            #if UNITY_2022_2_OR_NEWER
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            #else
            text.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
            #endif
            if(text.font) go.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
            return go.transform;
        }
    }
}
