namespace Core.TicksSystem
{
    public interface ITickable
    {
        TickPhase UpdatePhase { get; }
        void Tick(float deltaTime);
    }
}