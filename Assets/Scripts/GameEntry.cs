// ===================================================================
// GameEntry.cs  （Unity 层 · 挂载于场景根 GameObject）
//
// 职责：
//  1. 作为唯一的 Unity 生命周期驱动者（Awake / Update / OnDestroy）
//  2. 向纯 C# 层注入依赖（LogHelper 等）
//  3. 启动流程状态机（ProcedureFSM）
//  4. 暴露各模块的静态访问属性供业务层使用
//
// 场景要求：
//  · 仅 Launch 场景（第0个场景）挂载此脚本
//  · 此 GameObject 调用 DontDestroyOnLoad 跨场景存活
// ===================================================================

using UnityEngine;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Event;
using GameFramework.Procedure;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 游戏入口（Unity 层）
    /// 挂载在场景中名为 "GameEntry" 的 GameObject 上
    /// </summary>
    public sealed class GameEntry : MonoBehaviour
    {

        private IFsm<GameEntry> m_ProcedureFsm;


        private void Awake()
        {
            // ① 保证全局唯一，跨场景存活
            DontDestroyOnLoad(gameObject);

            // ② 最先初始化 Log（后续所有模块都需要它）
            InitLogSystem();

            // ③ 初始化各框架模块
            InitFrameworkModules();

            // ④ 启动流程状态机
            StartProcedure();

            Log.Info("[GameEntry] 框架初始化完毕，应用启动。");
        }

        private void Update()
        {
            // 将 Unity 的帧驱动转发给纯 C# 框架层
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            GameFrameworkEntry.Shutdown();
            Log.Info("[GameEntry] 框架已关闭。");
        }

        /// <summary>
        /// 步骤 1：初始化日志系统
        /// Log 必须是第一个初始化的，后续所有模块都依赖它
        /// </summary>
        private void InitLogSystem()
        {

        }

        /// <summary>
        /// 步骤 2：创建并注册所有框架模块
        /// 顺序：基础模块（FSM、Event）→ 资源模块 → 业务模块
        /// </summary>
        private void InitFrameworkModules()
        {
            ServiceLocator.Initialize();

            Log.Info("[GameEntry] 所有框架模块初始化完毕。");
        }

        /// <summary>
        /// 步骤 3：创建流程 FSM 并启动第一个流程
        /// 所有流程必须在此注册，否则运行时切换会报错
        /// </summary>
        private void StartProcedure()
        {

            // 注册所有流程（顺序不重要，FSM 内部用字典存储）
            m_ProcedureFsm = ServiceLocator.Fsm.CreateFsm(
                "MainProcedure",
                this,
                new ProcedureLaunch(),
                new ProcedureCheckVersion(),
                new ProcedurePreload(),
                new ProcedureMain()
            );

            // 从启动流程开始运行
            m_ProcedureFsm.Start<ProcedureLaunch>();

            Log.Info("[GameEntry] 流程状态机启动，当前流程：ProcedureLaunch。");
        }
    }
}
