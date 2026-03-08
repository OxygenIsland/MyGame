// 在单独的文件中：ServiceLocator.cs
using GameFramework;
using GameFramework.Event;
using GameFramework.Fsm;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 游戏服务定位器（模块容器）
    /// 职责：统一暴露所有框架模块的访问接口
    /// </summary>
    public static class ServiceLocator
    {
        public static IFsmManager Fsm { get; private set; }
        public static IEventManager Event { get; private set; }
        // 后续可扩展：
        // public static IResourceManager Resource { get; private set; }
        // public static IUIManager UI { get; private set; }

        /// <summary>
        /// 框架启动时调用一次（由 GameEntry 调用）
        /// </summary>
        internal static void Initialize()
        {
            Fsm = GameFrameworkEntry.GetModule<IFsmManager>();
            Event = GameFrameworkEntry.GetModule<IEventManager>();
            
            Log.Info("[ServiceLocator] 所有服务已注册。");
        }

        /// <summary>
        /// 框架关闭时调用一次（由 GameEntry 调用）
        /// </summary>
        internal static void Shutdown()
        {
            GameFrameworkEntry.Shutdown();
        }
    }
}