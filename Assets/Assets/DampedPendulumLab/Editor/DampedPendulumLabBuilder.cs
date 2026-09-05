using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DampedPendulumLab.Editor
{
    public static class DampedPendulumLabBuilder
    {
        [MenuItem("Tools/Damped Pendulum Lab/Create Apparatus Prefab")]
        public static void Create()
        {
            bool srp=GraphicsSettings.currentRenderPipeline!=null;
            Shader lit=Shader.Find(srp?"Universal Render Pipeline/Lit":"Standard");
            Shader unlit=Shader.Find(srp?"Universal Render Pipeline/Unlit":"Unlit/Color");
            if(!lit || !unlit) { Debug.LogError("Builder supports Built-in and URP. Adapt shaders for other pipelines."); return; }
            string folder=AssetDatabase.GenerateUniqueAssetPath("Assets/DampedPendulumLabModel");
            AssetDatabase.CreateFolder("Assets",folder.Substring(7));
            Material metal=Mat(folder,"Metal",lit,new Color(.61f,.66f,.69f),.6f);
            Material dark=Mat(folder,"Dark",lit,new Color(.06f,.10f,.13f),.05f);
            Material wood=Mat(folder,"Base",lit,new Color(.39f,.19f,.08f),0f);
            Material white=Mat(folder,"PlasticSheet",lit,new Color(.9f,.95f,.91f),0f);
            Material brass=Mat(folder,"BobBrass",lit,new Color(.75f,.47f,.12f),.55f);
            Material blue=Mat(folder,"MotorBlue",lit,new Color(.06f,.34f,.5f),.15f);
            Material red=Mat(folder,"Red",lit,new Color(.75f,.055f,.045f),0f);
            Material green=Mat(folder,"PowerLamp",unlit,new Color(.08f,.8f,.22f),0f);
            GameObject root=new GameObject("DampedPendulumApparatus");
            try
            {
                Transform p=root.transform;
                DampedPendulumExperiment sim=root.AddComponent<DampedPendulumExperiment>();
                Box("WoodenBase",p,wood,new Vector3(0f,.045f,0f),new Vector3(.5f,.09f,.96f));
                for(int x=-1;x<=1;x+=2) for(int z=-1;z<=1;z+=2)
                    Box("RubberFoot",p,dark,new Vector3(x*.20f,.006f,z*.41f),new Vector3(.045f,.012f,.045f));
                for(int side=-1;side<=1;side+=2)
                {
                    Cylinder("GuideRail",p,metal,new Vector3(side*.185f,.104f,0f),.014f,.83f,Quaternion.Euler(90f,0f,0f));
                    Box("RailStop",p,dark,new Vector3(side*.185f,.102f,-.425f),new Vector3(.035f,.028f,.035f));
                    Box("RailStop",p,dark,new Vector3(side*.185f,.102f,.425f),new Vector3(.035f,.028f,.035f));
                }
                sim.boardCarriage=Group("MovingCarriage",p);
                Transform carriage=sim.boardCarriage;
                Box("CarriageFrame",carriage,blue,new Vector3(0f,.115f,0f),new Vector3(.412f,.012f,.5f));
                Box("BlankPlasticSheet",carriage,white,new Vector3(0f,.125f,0f),new Vector3(.31f,.008f,.49f));
                for(int side=-1;side<=1;side+=2)
                    for(int z=-1;z<=1;z+=2)
                        Box("SheetClamp",carriage,metal,new Vector3(side*.16f,.13f,z*.20f),new Vector3(.018f,.009f,.035f));
                // Post behind the travel area; crossbar projects over the sheet.
                Box("StandBase",p,metal,new Vector3(0f,.04f,.53f),new Vector3(.17f,.08f,.13f));
                sim.supportPost=Cylinder("StandPost",p,metal,Vector3.zero,.012f,.65f,Quaternion.identity);
                sim.topArm=Box("TopArm",p,metal,Vector3.zero,Vector3.one);
                sim.pivot=Part("SuspensionPoint",PrimitiveType.Sphere,p,dark,Vector3.zero,Vector3.one*.013f);
                sim.cord=Cylinder("PendulumCord",p,dark,Vector3.zero,.0018f,.48f,Quaternion.identity);
                sim.bob=Cylinder("PendulumWeight",p,brass,Vector3.zero,.04f,.04f,Quaternion.identity);
                sim.markerBody=Cylinder("MarkerBody",p,dark,Vector3.zero,.006f,.034f,Quaternion.identity);
                sim.markerNib=Cylinder("SlidingMarkerTip",p,red,Vector3.zero,.0025f,.02f,Quaternion.identity);
                Box("DriveMotor",p,blue,new Vector3(.185f,.145f,-.44f),new Vector3(.078f,.078f,.075f));
                Cylinder("DriveShaft",p,metal,new Vector3(.185f,.145f,-.393f),.008f,.034f,Quaternion.Euler(90f,0f,0f));
                sim.motorRotor=Group("MotorRotor",p);sim.motorRotor.localPosition=new Vector3(.185f,.145f,-.373f);
                Cylinder("DriveWheel",sim.motorRotor,dark,Vector3.zero,.031f,.009f,Quaternion.Euler(90f,0f,0f));
                Box("RotationMarker",sim.motorRotor,white,new Vector3(.006f,0f,-.006f),new Vector3(.012f,.003f,.002f));
                // Decorative belt cover indicates the drive connection; no hidden motor physics.
                Box("BeltCover",p,dark,new Vector3(.225f,.115f,0f),new Vector3(.024f,.025f,.81f));
                Box("PowerSupply",p,white,new Vector3(-.37f,.065f,-.30f),new Vector3(.18f,.13f,.15f));
                Box("PowerFront",p,blue,new Vector3(-.37f,.067f,-.378f),new Vector3(.17f,.11f,.006f));
                Cylinder("VoltageKnob",p,dark,new Vector3(-.345f,.065f,-.393f),.032f,.025f,Quaternion.Euler(90f,0f,0f));
                Part("PowerIndicator",PrimitiveType.Sphere,p,green,new Vector3(-.417f,.095f,-.385f),Vector3.one*.01f);
                Box("MeterWindow",p,dark,new Vector3(-.41f,.052f,-.383f),new Vector3(.04f,.025f,.003f));
                Cable("RedPowerCable",p,red,new Vector3(-.28f,.05f,-.31f),new Vector3(-.25f,.018f,-.38f),new Vector3(-.1f,.012f,-.52f),new Vector3(.18f,.015f,-.52f),new Vector3(.20f,.13f,-.477f));
                Cable("BlackPowerCable",p,dark,new Vector3(-.28f,.04f,-.34f),new Vector3(-.25f,.012f,-.42f),new Vector3(-.08f,.008f,-.55f),new Vector3(.22f,.01f,-.55f),new Vector3(.22f,.12f,-.477f));
                sim.PreparePreview();
                GameObject prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/DampedPendulumApparatus.prefab");
                AssetDatabase.SaveAssets();Selection.activeObject=prefab;EditorGUIUtility.PingObject(prefab);
                Debug.Log("Created "+folder+"/DampedPendulumApparatus.prefab. Play runs a finite carriage pass; reset for another run.");
            }
            finally { Object.DestroyImmediate(root); }
        }
        static Transform Group(string name,Transform parent)
        { GameObject go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform; }
        static Transform Box(string name,Transform p,Material m,Vector3 pos,Vector3 scale)
        { return Part(name,PrimitiveType.Cube,p,m,pos,scale); }
        static Transform Cylinder(string name,Transform p,Material m,Vector3 pos,float diameter,float height,Quaternion rotation)
        { Transform t=Part(name,PrimitiveType.Cylinder,p,m,pos,new Vector3(diameter,height*.5f,diameter));t.localRotation=rotation;return t; }
        static Transform Part(string name,PrimitiveType type,Transform p,Material m,Vector3 pos,Vector3 scale)
        {
            GameObject go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(p,false);
            go.transform.localPosition=pos;go.transform.localScale=scale;Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial=m;return go.transform;
        }
        static Material Mat(string folder,string name,Shader shader,Color color,float metallic)
        {
            Material m=new Material(shader){name=name,color=color};
            if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",metallic);
            if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.35f);
            AssetDatabase.CreateAsset(m,folder+"/"+name+".mat");return m;
        }
        static void Cable(string name,Transform p,Material m,params Vector3[] points)
        {
            Transform t=Group(name,p);LineRenderer l=t.gameObject.AddComponent<LineRenderer>();l.sharedMaterial=m;l.useWorldSpace=false;
            l.positionCount=points.Length;l.SetPositions(points);l.startWidth=.003f;l.endWidth=.003f;l.numCornerVertices=4;l.numCapVertices=4;
            l.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
}
