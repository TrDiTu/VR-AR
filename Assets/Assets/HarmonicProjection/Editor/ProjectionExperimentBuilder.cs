using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace HarmonicProjection.Editor
{
    public static class ProjectionExperimentBuilder
    {
        [MenuItem("Tools/Harmonic Projection/Create Experiment Prefab")]
        public static void Create()
        {
            bool srp = GraphicsSettings.currentRenderPipeline != null;
            Shader lit = Shader.Find(srp ? "Universal Render Pipeline/Lit" : "Standard");
            Shader unlit = Shader.Find(srp ? "Universal Render Pipeline/Unlit" : "Unlit/Color");
            if (!lit || !unlit) { Debug.LogError("Builder supports Built-in and URP. Adapt shaders for other pipelines."); return; }
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/HarmonicProjectionModel");
            AssetDatabase.CreateFolder("Assets", path.Substring(7));
            Material metal = Mat(path, "Metal", lit, new Color(.55f,.62f,.68f));
            Material blue = Mat(path, "Blue", lit, new Color(.08f,.24f,.38f));
            Material orange = Mat(path, "Orange", lit, new Color(1f,.35f,.07f));
            Material screen = Mat(path, "Screen", unlit, new Color(1f,.72f,.52f));
            Material ink = Mat(path, "Ink", unlit, new Color(.08f,.12f,.16f));
            Material red = Mat(path, "Rays", unlit, new Color(1f,.12f,.03f));
            Material guide = Mat(path, "Guides", unlit, new Color(.3f,.5f,.55f));
            GameObject root = new GameObject("HarmonicProjectionExperiment");
            try
            {
                Transform p = root.transform;
                ProjectionExperiment sim = root.AddComponent<ProjectionExperiment>();
                Part("Base", PrimitiveType.Cube,p,blue,new Vector3(-.11f,.009f,.055f),new Vector3(.98f,.018f,.4f));
                Part("RearPost",PrimitiveType.Cube,p,metal,new Vector3(.07f,.355f,.14f),new Vector3(.018f,.69f,.018f));
                Part("TopBeam",PrimitiveType.Cube,p,metal,new Vector3(.07f,.702f,.065f),new Vector3(.1f,.024f,.19f));
                Part("MotorStand",PrimitiveType.Cube,p,metal,new Vector3(-.35f,.15f,.065f),new Vector3(.025f,.27f,.025f));
                Part("Motor",PrimitiveType.Cube,p,blue,new Vector3(-.35f,.32f,.075f),new Vector3(.075f,.065f,.09f));
                Transform shaft = Part("MotorShaft",PrimitiveType.Cylinder,p,metal,new Vector3(-.35f,.32f,.017f),new Vector3(.013f,.035f,.013f));
                shaft.localRotation = Quaternion.Euler(90f,0f,0f);
                sim.arm = Part("RotatingArm",PrimitiveType.Cylinder,p,metal,Vector3.zero,Vector3.one);
                sim.rotatingTip = Part("CylindricalTip",PrimitiveType.Cylinder,p,orange,Vector3.zero,new Vector3(.04f,.022f,.04f));
                sim.rotatingTip.localRotation = Quaternion.Euler(0f,0f,90f);
                sim.bob = Part("SpringBob",PrimitiveType.Sphere,p,orange,Vector3.zero,Vector3.one*.04f);
                GameObject coil = new GameObject("Spring");
                coil.transform.SetParent(p,false);
                coil.transform.localPosition = new Vector3(ProjectionExperiment.SpringX,ProjectionExperiment.AnchorY,0f);
                sim.spring = coil.AddComponent<MeshFilter>();
                coil.AddComponent<MeshRenderer>().sharedMaterial = metal;
                Part("ProjectionScreen",PrimitiveType.Cube,p,screen,new Vector3(.34f,.33f,0f),new Vector3(.012f,.62f,.34f));
                Part("ScreenEdgeFront",PrimitiveType.Cube,p,orange,new Vector3(.34f,.33f,-.175f),new Vector3(.02f,.64f,.012f));
                Part("ScreenEdgeRear",PrimitiveType.Cube,p,orange,new Vector3(.34f,.33f,.175f),new Vector3(.02f,.64f,.012f));
                sim.shadow = Part("CoincidentShadow",PrimitiveType.Sphere,p,ink,Vector3.zero,new Vector3(.003f,.04f,.04f));
                float sx = ProjectionExperiment.ShadowX;
                Line("ScreenAxis",p,ink,.0015f,new Vector3(sx,.1f,0f),new Vector3(sx,.59f,0f));
                Line("AxisArrowA",p,ink,.0015f,new Vector3(sx,.59f,0f),new Vector3(sx,.575f,.008f));
                Line("AxisArrowB",p,ink,.0015f,new Vector3(sx,.59f,0f),new Vector3(sx,.575f,-.008f));
                sim.upperTick = Tick("+A",p,ink);
                sim.lowerTick = Tick("-A",p,ink);
                Transform zero = Tick("O",p,ink);
                zero.localPosition = new Vector3(sx,.32f,0f);
                Label("x",p,new Vector3(sx-.001f,.61f,-.025f),true);
                sim.movingLabel = Label("Y",p,Vector3.zero,true);
                sim.orbit = Line("CircularOrbit",p,guide,.0016f,Vector3.zero,Vector3.zero);
                sim.projectionRay = Line("CoincidentProjectionRay",p,red,.0012f,Vector3.zero,Vector3.zero);
                sim.incidentRays = new GameObject("ParallelLightRays");
                sim.incidentRays.transform.SetParent(p,false);
                for(int i=0;i<11;i++)
                {
                    float y=.16f+i*.032f;
                    Line("Ray",sim.incidentRays.transform,red,.0012f,new Vector3(-.65f,y,0f),new Vector3(-.515f,y,0f));
                    Line("Arrow",sim.incidentRays.transform,red,.0012f,new Vector3(-.523f,y+.003f,0f),new Vector3(-.515f,y,0f),new Vector3(-.523f,y-.003f,0f));
                }
                Label("LIGHT",p,new Vector3(-.62f,.54f,0f),false);
                Label("MOTOR",p,new Vector3(-.4f,.51f,.015f),false);
                Label("SPRING",p,new Vector3(.015f,.75f,0f),false);
                Mesh preview=sim.CreatePreviewMesh();
                sim.spring.sharedMesh=preview;
                AssetDatabase.CreateAsset(preview,path+"/SpringPreview.asset");
                GameObject prefab=PrefabUtility.SaveAsPrefabAsset(root,path+"/ProjectionExperiment.prefab");
                AssetDatabase.SaveAssets(); Selection.activeObject=prefab; EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created "+path+"/ProjectionExperiment.prefab. View from negative X and negative Z to see the screen front.");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static Material Mat(string path,string name,Shader shader,Color color)
        {
            Material m=new Material(shader){name=name,color=color};
            if(m.HasProperty("_BaseColor")) m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",.35f);
            AssetDatabase.CreateAsset(m,path+"/"+name+".mat"); return m;
        }
        static Transform Part(string name,PrimitiveType type,Transform p,Material m,Vector3 pos,Vector3 scale)
        {
            GameObject go=GameObject.CreatePrimitive(type); go.name=name; go.transform.SetParent(p,false);
            go.transform.localPosition=pos; go.transform.localScale=scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            Renderer r=go.GetComponent<Renderer>();r.sharedMaterial=m;
            // Educational projection is explicit; avoid unrelated shadows on the screen.
            r.shadowCastingMode=ShadowCastingMode.Off; r.receiveShadows=false;
            return go.transform;
        }
        static LineRenderer Line(string name,Transform p,Material m,float width,params Vector3[] points)
        {
            GameObject go=new GameObject(name);go.transform.SetParent(p,false);
            LineRenderer l=go.AddComponent<LineRenderer>();l.useWorldSpace=false;l.sharedMaterial=m;
            l.startWidth=width;l.endWidth=width;l.positionCount=points.Length;l.SetPositions(points);
            l.shadowCastingMode=ShadowCastingMode.Off;l.receiveShadows=false;return l;
        }
        static Transform Tick(string text,Transform p,Material m)
        {
            GameObject go=new GameObject("Tick_"+text);go.transform.SetParent(p,false);
            Line("Tick",go.transform,m,.002f,new Vector3(0f,0f,-.012f),new Vector3(0f,0f,.012f));
            Label(text,go.transform,new Vector3(-.001f,0f,-.037f),true);return go.transform;
        }
        static Transform Label(string text,Transform p,Vector3 pos,bool onScreen)
        {
            GameObject go=new GameObject("Label_"+text);go.transform.SetParent(p,false);go.transform.localPosition=pos;
            if(onScreen) go.transform.localRotation=Quaternion.Euler(0f,90f,0f);
            TextMesh t=go.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=.035f;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.08f,.12f,.16f);
            #if UNITY_2022_2_OR_NEWER
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            #else
            t.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
            #endif
            if(t.font) go.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;
            return go.transform;
        }
    }
}
