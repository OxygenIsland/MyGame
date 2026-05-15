using GameFramework;
using GameFramework.Core.Framework;
using GameFramework.Resource;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    void Awake()
    {
        ModuleSystem.GetModule<IUpdateDriver>();
        ModuleSystem.GetModule<IResourceModule>();
        ModuleSystem.GetModule<IDebuggerModule>();
        ModuleSystem.GetModule<IFsmModule>();
        Settings.ProcedureSetting.StartProcedure().Forget();
        DontDestroyOnLoad(this);
    }
}

internal interface IFsmModule
{
}