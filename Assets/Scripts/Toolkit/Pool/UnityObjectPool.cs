using System;

namespace StarWorld.Common.Pool
{
    public class UnityObjectPool<T> : Pool<T>
    {
        readonly Action<T> mResetMethod;

        public UnityObjectPool(
            Func<T> factoryMethod,
            Action<T> resetMethod = null,
            int initCount = 0
        )
        {
            mFactory = new ItemFactory<T>(factoryMethod);
            mResetMethod = resetMethod;

            for (int i = 0; i < initCount; i++)
            {
                mCacheStack.Push(mFactory.Create());
            }
        }

        public override bool Recycle(T obj)
        {
            if (mCacheStack.Contains(obj))
            {
                return false;
            }
            mResetMethod.Invoke(obj);
            mCacheStack.Push(obj);
            return true;
        }
    }
}
