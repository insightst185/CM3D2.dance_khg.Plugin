using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using System.Reflection;

namespace CM3D2.dance_cm3d2_006_ssn
{
    [PluginFilter("CM3D2x64"),
    PluginFilter("CM3D2x86"),
    PluginFilter("CM3D2VRx64"),
    PluginName("MaidToMaid"),
    PluginVersion("0.0.0.1")]
    public class dance_khg : PluginBase
    {

        private int level;
        private XmlManager xmlManager;
        // presetリスト 4人分でいいか       ↓ここを変えると最大追加人数変えられる
        private const int MAX_LISTED_MAID = 4;
        private int[] presetPos = new int[MAX_LISTED_MAID];
        private Boolean maidSetting = false;
        private Boolean[] motionSetting = new Boolean[MAX_LISTED_MAID];
        private Maid maid;
        
        private void SetPreset(Maid maid, string fileName)
        {
            var preset = GameMain.Instance.CharacterMgr.PresetLoad(Path.Combine(Path.GetFullPath(".\\") + "Preset", fileName));
            GameMain.Instance.CharacterMgr.PresetSet(maid, preset);
        }

        public void Awake()
        {
            UnityEngine.Object.DontDestroyOnLoad(this);
            xmlManager = new XmlManager();
        }

        private enum TargetLevel
        {
            Scene_khg = 36
        }

        private void OnLevelWasLoaded(int level)
        {
            this.level = level;
            maidSetting = false;
            for(int i = 0; i < MAX_LISTED_MAID; i++){
                motionSetting[i] = false;
            }
        }

        private void Update()
        {

            if(Input.GetKey(KeyCode.Escape)){
                xmlManager = new XmlManager();
            }
            if (!Enum.IsDefined(typeof(TargetLevel), level)) return;
            if(maidSetting == true) return;
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
                                maid.CrossFade("dance_cm3d2_005_khg_f.anm", false, false, false, 0.0f, 1f);
                                maid.body0.m_Bones.GetComponent<Animation>()["dance_cm3d2_005_khg_f.anm"].time = -0.400f;
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
            }
        }

    }
}

