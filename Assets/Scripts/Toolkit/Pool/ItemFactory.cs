using System;

namespace StarWorld.Common.Pool
{
    public class ItemFactory<T> : IObjectFactory<T>
    {
        protected Func<T> mFactoryMethod;
        public ItemFactory(Func<T> factoryMethod)
        {
            mFactoryMethod = factoryMethod;
        }
        public T Create()
        {
            return mFactoryMethod();
        }
    }
}
