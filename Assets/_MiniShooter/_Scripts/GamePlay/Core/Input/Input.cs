namespace Core.Input
{
    public struct Input
    {
        public bool IsStarted;
        public bool IsPressed;
        public bool IsReleased;

        public void Reset()
        {
            IsStarted = false;
            IsPressed = false;
            IsReleased = false;
        }
    }
}