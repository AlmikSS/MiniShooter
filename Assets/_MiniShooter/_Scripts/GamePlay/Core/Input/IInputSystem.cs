using Core.DIServiceLocator;

namespace Core.Input
{
    public interface IInputSystem : IService
    {
        InputSnapshot Snapshot { get; }

        void OpenUI();
        void CloseUI();
    }
}