using System.Collections.Generic;

namespace StarWorld.Common.Pool
{
    public abstract class Pool<T> : IPool<T>
    {
        public int CurCount
        {
            get { return mCacheStack.Count; }
        }
        protected IObjectFactory<T> mFactory;
        protected Stack<T> mCacheStack = new Stack<T>();

        public virtual T Allocate()
        {
            return mCacheStack.Count == 0 ? mFactory.Create() : mCacheStack.Pop();
        }

        public abstract bool Recycle(T obj);
    }
}
