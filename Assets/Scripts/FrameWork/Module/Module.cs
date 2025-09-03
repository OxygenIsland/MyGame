using System.Collections;

namespace StarWorld.FrameWork
{
    public interface IModule
    {
        /// <summary>
        /// 在所有的Controller和Module的OnLoad之前执行
        /// </summary>
        void Preload();

        IEnumerator OnLoad(object data);

        /// <summary>
        /// 在所有的Controller和Module的OnUnload之前执行
        /// </summary>
        void PreUnload();

        IEnumerator OnUnload();
    }
}
