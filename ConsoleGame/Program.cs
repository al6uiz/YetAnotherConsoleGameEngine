using ConsoleGame.Components;
using ConsoleGame.Entities;
using ConsoleGame.Renderer;

namespace ConsoleGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Example usage
            Framebuffer buffer = new Framebuffer(20, 20);
            buffer.Clear();

            // Create an entity
            BaseEntity player = new BaseEntity(10, 10, new Chexel('@', ConsoleColor.Yellow, ConsoleColor.Black));

            // Add a component to handle movement
            player.AddComponent(new PlayerMovementComponent());

            // Create and start the terminal
            Terminal terminal = new Terminal();
            terminal.AddFrameBuffer(buffer);
            terminal.AddEntity(player);
            terminal.Start();
        }
    }

    // Example component for player movement
    public class PlayerMovementComponent : BaseComponent
    {
        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    Parent.Y--;
                    break;
                case ConsoleKey.DownArrow:
                    Parent.Y++;
                    break;
                case ConsoleKey.LeftArrow:
                    Parent.X--;
                    break;
                case ConsoleKey.RightArrow:
                    Parent.X++;
                    break;
            }
        }
    }
}
