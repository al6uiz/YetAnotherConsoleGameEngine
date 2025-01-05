using System;
using System.Collections.Generic;

namespace ConsoleGame.Renderer
{
    public class TerminalInput
    {
        private Queue<ConsoleKeyInfo> inputQueue;

        public TerminalInput()
        {
            inputQueue = new Queue<ConsoleKeyInfo>();
        }

        public void Update()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true); // Intercept the key to prevent it from being displayed
                inputQueue.Enqueue(keyInfo);
            }
        }

        public bool TryGetKey(out ConsoleKeyInfo keyInfo)
        {
            if (inputQueue.Count > 0)
            {
                keyInfo = inputQueue.Dequeue();
                return true;
            }

            keyInfo = default;
            return false;
        }

        public void Clear()
        {
            inputQueue.Clear();
        }
    }
}
