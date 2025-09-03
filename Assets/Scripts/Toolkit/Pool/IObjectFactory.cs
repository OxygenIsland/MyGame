namespace StarWorld.Common.Pool
{
    public interface IObjectFactory<T>
    {
        T Create();
    }
}
