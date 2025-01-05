using ConsoleGame.Entities;

namespace ConsoleGame.Components
{
    public class BaseComponent
    {
        public BaseEntity Parent;

        public virtual void Update(double deltaTime)
        {
            // Base does nothing
        }

        public virtual void HandleInput(ConsoleKeyInfo keyInfo)
        {
            // Base does nothing
        }
    }
}
