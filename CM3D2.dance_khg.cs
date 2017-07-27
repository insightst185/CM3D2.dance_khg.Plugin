using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using System.Reflection;
using System.Collections;

namespace CM3D2.dance_khg
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("dance_khg"),
    PluginVersion("0.0.0.6")]
    public class dance_khg : PluginBase
    {

        private int level;
        private XmlManager xmlManager;
        // presetリスト 4人分でいいか       ↓ここを変えると最大追加人数変えられる
        private const int MAX_LISTED_MAID = 4;
        private int[] presetPos = new int[MAX_LISTED_MAID];
        private Boolean maidSetting = false;
        private Boolean[] motionSetting = new Boolean[MAX_LISTED_MAID];
        private Boolean[] maidSync = new Boolean[MAX_LISTED_MAID];
        private Maid maid0;
        private Maid maid;
        private String lastBlend;
        private String nowBlend;
        private String kuchipaku;
        private Hashtable anmHash = new Hashtable();
        private Hashtable timingHash = new Hashtable();
        
        private int[] sceneList = new int[]{
            (int)TargetLevel.Scene_f1
          , (int)TargetLevel.Scene_end
          , (int)TargetLevel.Scene_smt
          , (int)TargetLevel.Scene_sp2
          , (int)TargetLevel.Scene_kano
          , (int)TargetLevel.Scene_khg
        };

        private string[] anmList = new string[]{
            "dance_cm3d_001_f1.anm"
          , "dance_cm3d_002_end_f1.anm"
          , "dance_cm3d2_002_smt_f.anm"
          , "dance_cm3d_003_sp2_f1.anm"
          , "dance_cm3d_004_kano_f1.anm"
          , "dance_cm3d2_005_khg_f.anm"
        };

        private float[] timingList = new float[]{
            -0.400f
          , -0.400f
          , -0.400f
          , -0.400f
          , -0.400f
          , -0.400f
        };

        private enum TargetLevel
        {
            Scene_f1 = 20
          , Scene_end = 22
          , Scene_smt = 26
          , Scene_sp2 = 28
          , Scene_kano = 32
          , Scene_khg = 36
        }
        private DanceMain danceMain = null;
        private FieldInfo field;
        
        private void SetPreset(Maid maid, string fileName)
        {
            var preset = GameMain.Instance.CharacterMgr.PresetLoad(Path.Combine(Path.GetFullPath(".\\") + "Preset", fileName));
            GameMain.Instance.CharacterMgr.PresetSet(maid, preset);
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
            xmlManager = new XmlManager();
            for(int i = 0; i < sceneList.Length ;i++){
                anmHash.Add(sceneList[i],anmList[i]);
                timingHash.Add(sceneList[i],timingList[i]);
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            this.level = level;
            if (!Enum.IsDefined(typeof(TargetLevel), level)) return;
            maidSetting = false;
            for(int i = 0; i < MAX_LISTED_MAID; i++){
                motionSetting[i] = false;
                maidSync[i] = false;
            }
            lastBlend = null;
            kuchipaku = null;
            danceMain = (DanceMain)FindObjectOfType(typeof(DanceMain));
        }

        private void Update()
        {

            if(Input.GetKey(KeyCode.Escape)){
                xmlManager = new XmlManager();
            }
            if (!Enum.IsDefined(typeof(TargetLevel), level)) return;
            if(maidSetting == true){
                maid0 = GameMain.Instance.CharacterMgr.GetMaid(0);
                nowBlend = maid0.ActiveFace;
                for(int i = 0; i < MAX_LISTED_MAID; i++){
                    if(xmlManager.listPreset[i] != null && motionSetting[i] == true){
                        maid = GameMain.Instance.CharacterMgr.GetMaid(i + 1);
                        maid.FoceKuchipakuUpdate(maid.body0.m_Bones.GetComponent<Animation>()[(string)anmHash[level]].time);
                        if(nowBlend != lastBlend){
                            maid.FaceBlend(maid.ActiveFace = nowBlend);
                        }

                        if(maidSync[i] == false){
                            field = (typeof(DanceMain)).GetField("m_eMode", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            int m_eMode = (int)field.GetValue(danceMain);
                            if(maid.IsBusy == false && m_eMode == 3){
                                
                                field = (typeof(DanceMain)).GetField("m_fOffsetTime", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                                float m_fOffsetTime = (float)field.GetValue(danceMain);
                                maid.body0.m_Bones.GetComponent<Animation>()[(string)anmHash[level]].time = m_fOffsetTime;
                                maid.StartKuchipakuPattern(m_fOffsetTime,kuchipaku,true);
                                maidSync[i] = true;

                            }
                        }
                    }
                }
                lastBlend = nowBlend;
                return;
            }
            for(int i = 0; i < MAX_LISTED_MAID; i++){
                if(xmlManager.listPreset[i] != null){
                   maid = GameMain.Instance.CharacterMgr.GetMaid(i + 1);
                   if(maid == null){
                       maid = GameMain.Instance.CharacterMgr.AddStockMaid();
                       GameMain.Instance.CharacterMgr.SetActiveMaid(maid,i + 1);
                   }
                   SetPreset(maid,xmlManager.listPreset[i]);
                   maid.SetPos(xmlManager.listPos[i]);
                   maid.Visible = true;
                }
            }
            maidSetting = true;
        }
        
        private void LateUpdate(){

            if (!Enum.IsDefined(typeof(TargetLevel), level)) return;
            GameMain.Instance.MainCamera.GetComponent<Camera>().fieldOfView = xmlManager.fov;
            if(maidSetting == false) return;
            for(int i = 0; i < MAX_LISTED_MAID; i++){
                if (motionSetting[i] == false){
                    if(xmlManager.listPreset[i] != null){
                        maid = GameMain.Instance.CharacterMgr.GetMaid(i + 1);
                        if(maid.IsBusy == false){
                            FieldInfo field = (typeof(Maid)).GetField("m_MotionLoad", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            int m_MotionLoad = (int)field.GetValue(maid);

                            if(m_MotionLoad == 0){
                                if(kuchipaku == null){
                                    maid0 = GameMain.Instance.CharacterMgr.GetMaid(0);
                                    kuchipaku = System.Convert.ToBase64String(maid0.m_baKuchipakuPattern);
                                }
//                                maid.CrossFade((string)anmHash[level], false, false, false, 0.0f, xmlManager.weight);
//                                maid.EyeToReset(0.0f);
                                  maid.EyeToCamera(Maid.EyeMoveType.目だけ向ける,0.0f);
                                maid.CrossFade((string)anmHash[level], false, false, false, 0.0f, 1.0f);
//                                maid.body0.m_Bones.GetComponent<Animation>()[(string)anmHash[level]].time = (float)timingHash[level];
//                                maid.StartKuchipakuPattern((float)timingHash[level],kuchipaku,true);
                                motionSetting[i] = true;
                            }
                        }
                    }
                    else{
                        motionSetting[i] = true;
                    }
                }
            }
        }
        
        //------------------------------------------------------xml--------------------------------------------------------------------
        private class XmlManager
        {
            private string xmlFileName = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\Config\Dance_khg.xml";
            private XmlDocument xmldoc = new XmlDocument();
            public string[] listPreset = new string[MAX_LISTED_MAID];
            public Vector3[] listPos = new Vector3[MAX_LISTED_MAID];
            public float fov;
//            public float weight;
            
            public XmlManager()
            {
                for(int i = 0; i < MAX_LISTED_MAID; i++){
                    listPreset[i] = null;
                }
                try{
                    InitXml();
                }
                catch(Exception e){
                    Debug.LogError("Dance_khg.Plugin:" + e.Source + e.Message + e.StackTrace);
                }
            }

            private void InitXml()
            {
                xmldoc.Load(xmlFileName);
                // PresetList
                XmlNodeList presetList = xmldoc.GetElementsByTagName("PresetList");
                foreach (XmlNode presetFile in presetList)
                {
                    int maidNo = Int32.Parse(((XmlElement)presetFile).GetAttribute("maidNo")) - 1;
                    listPreset[maidNo] =((XmlElement)presetFile).GetAttribute("FileName");
                    listPos[maidNo] = new Vector3();
                    listPos[maidNo].x =float.Parse(((XmlElement)presetFile).GetAttribute("X"));
                    listPos[maidNo].y =float.Parse(((XmlElement)presetFile).GetAttribute("Y"));
                    listPos[maidNo].z =float.Parse(((XmlElement)presetFile).GetAttribute("Z"));
                }
                presetList = xmldoc.GetElementsByTagName("Camera");
                fov =float.Parse(((XmlElement)presetList[0]).GetAttribute("fieldOfView"));
//                weight =float.Parse(((XmlElement)presetList[0]).GetAttribute("weight"));
            }
        }

    }
}

