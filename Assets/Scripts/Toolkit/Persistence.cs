
// TODO:
//using Lenovo.XR.StARstudio.PluginsInstance.ARPackage.DataModel;
//using Newtonsoft.Json;
//using StarWorld.Viewer;
//using StudioEditor.Tool;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

//namespace stARstudio.UI
//{
//    public static class Persistence
//    {
//        public class LoginData
//        {
//            public bool StoredData;
//            public string LastInputType;
//            public string LastAuthType;

//            public string LastUserName;
//            public string lsw;
//            public string UserId;
//            public long AppId;
//            public string DeveloperToken;
//            public string DeveloperRefreshToken;
//            public string CameraClearFlags;
//            public string UnityWindowStatus;

//            public string QRCodeText;
//            public KubeInfo KubeInfo;
//            public ProJectModel ProjectModel;
//            public List<Account> AccountList = new List<Account>();
//            public Dictionary<string, Account> AccountDic = new Dictionary<string, Account>();
//            //public LicenseInfo SelectedLicense;
//            //public SceneInfo SelectedScene;
//            public ARPackageTemplate ARPackageTemplate;
//            //public string SelectedLicenseName;
//            //public string SelectedLicenseKey;
//            //public string SelectedLicenseSecret;
//            public TabRactTransform TaskBarRact;
//            public TabRactTransform SceneBarRact;
//            public TabRactTransform AssetBarRact;
//            public TabRactTransform AttributeBarRact;
//            public TabRactTransform TaskTabRact;
//            public TabRactTransform SceneTabRact;
//            public TabRactTransform AssetTabRact;
//            public TabRactTransform AttributeTabRact;
//        }

//        private static string dbPath;
//        private static LoginData data;

       

//        static Persistence()
//        {
//            dbPath = Application.persistentDataPath + "/persistence.db";
//        }

//        public static LoginData Data
//        {
//            get
//            {
//                if (data == null)
//                {
//                    data = Read();
//                }

//                return data;
//            }
//        }

//        private static LoginData Read()
//        {
//            var data = new LoginData { /*StoredData = false9*/ };

//            if (File.Exists(dbPath))
//            {
//                string read = File.ReadAllText(dbPath, Encoding.UTF8);
//                data = JsonConvert.DeserializeObject<LoginData>(read);
//                if (!string.IsNullOrEmpty(data.lsw) && EncryptDecipherUtil.IsBase64Formatted(data.lsw))
//                {
//                    data.lsw = EncryptDecipherUtil.Decipher(data.lsw);
//                }
//            }

//            return data;
//        }
//        public static void SetInputData(string userName, string pwd)
//        {
//            Data.LastUserName = userName;
//            Data.lsw = pwd;
//            Data.UserId = string.Empty;
//            Account ac = new Account
//            {
//                UserName = userName,
//                sw = pwd,
//                LastLoadTime = TimestampTool.GetNowTimestamp()
//            };
//            //for (int i = 0; i < Data.AccountList.Count; i++)
//            //{
//            //    if (Data.AccountList[i].UserName == userName)
//            //    {
//            //        Debug.LogError("save1");
//            //        Save(Data);
//            //        return;
//            //    }
//            //}
//            //Debug.LogError("save");
//            //Data.AccountList.Add(ac);
//            if (Data.AccountDic.ContainsKey(userName))
//            {
//                Data.AccountDic.Remove(userName);
//            }
//            Data.AccountDic.Add(userName, ac);
//            Save(Data);
//        }
//        public static void SetInputData(string userName)
//        {
//            Data.LastUserName = userName;
//            Data.lsw = string.Empty;
//        }
//        public static void SetToken(string token)
//        {
//            Data.DeveloperToken = token;
//            Save(Data);
//        }
//        private static void Save(LoginData data)
//        {
//            if (data == null)
//            {
//                Debug.LogError("Persistence Save Error, data == null");
//                return;
//            }
//            LoginData target = new LoginData();
//            JsonSerializerSettings settings = new JsonSerializerSettings();
//            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
//            string json = JsonConvert.SerializeObject(data, settings);
//            target = JsonConvert.DeserializeObject<LoginData>(json);
//            if (!string.IsNullOrEmpty(target.lsw))
//            {
//                target.lsw = EncryptDecipherUtil.Encrypt(target.lsw);
//            }
//            json = JsonConvert.SerializeObject(target, settings);
//            File.WriteAllText(dbPath, json, Encoding.UTF8);
//        }
//        internal static void Save(KubeInfo kubeInfo)
//        {
//            Data.LastAuthType = "Kube";
//            Data.KubeInfo = kubeInfo;
//            Data.AppId = kubeInfo.appId;
//            Data.UserId = kubeInfo.userId.ToString();
//            Data.DeveloperToken = kubeInfo.access_token;
//            Data.DeveloperRefreshToken = kubeInfo.refresh_token;
//            Save(Data);
//        }
//        //internal static void Save(ARPackageTemplate arp)
//        //{
//        //    Data.ARPackageTemplate = arp;
//        //    Save(Data);
//        //}
//        internal static void Save()
//        {
//            Save(Data);
//        }
//        //internal static void Save(LicenseInfo licenseInfo)
//        //{
//        //    Data.SelectedLicense = licenseInfo;
//        //    Save(Data);
//        //}
//        //internal static void Save(SceneInfo sceneInfo)
//        //{
//        //    Data.SelectedScene = sceneInfo;
//        //    Save(Data);
//        //}
//        public static void Clear()
//        {
//            if (File.Exists(dbPath))
//                File.Delete(dbPath);
//        }
//    }
//}
