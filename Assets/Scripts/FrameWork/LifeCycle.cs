namespace StarWorld.FrameWork
{
    public interface ILifeCycleOwner
    {
        void AddObserver(ILifeCycleObserver observer);
    }

    public interface ILifeCycleObserver
    {
        void OnStart();

        void OnStop();
    }
}