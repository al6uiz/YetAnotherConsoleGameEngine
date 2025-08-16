using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ConsoleGame.Renderer
{
    public class TerminalInput
    {
        private Queue<ConsoleKeyInfo> inputQueue;
        private int[] monitoredVKeys;

        public TerminalInput()
        {
            inputQueue = new Queue<ConsoleKeyInfo>();
            monitoredVKeys = BuildDefaultMonitoredKeys();
        }

        public void Update()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
            }

            bool shift = IsDown(0x10);
            bool alt = IsDown(0x12);
            bool ctrl = IsDown(0x11);

            for (int i = 0; i < monitoredVKeys.Length; i++)
            {
                int vk = monitoredVKeys[i];
                if (IsDown(vk))
                {
                    ConsoleKey key = (ConsoleKey)vk;
                    ConsoleKeyInfo keyInfo = new ConsoleKeyInfo('\0', key, shift, alt, ctrl);
                    inputQueue.Enqueue(keyInfo);
                }
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

        public void SetMonitoredKeys(IEnumerable<ConsoleKey> keys)
        {
            if (keys == null)
            {
                monitoredVKeys = BuildDefaultMonitoredKeys();
                return;
            }

            List<int> list = new List<int>();
            foreach (var k in keys)
            {
                list.Add((int)k);
            }
            monitoredVKeys = list.ToArray();
        }

        public bool IsKeyDown(ConsoleKey key)
        {
            return IsDown((int)key);
        }

        private static bool IsDown(int vKey)
        {
            return (GetAsyncKeyState(vKey) & 0x8000) != 0;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static int[] BuildDefaultMonitoredKeys()
        {
            List<int> keys = new List<int>();

            for (int vk = 0x41; vk <= 0x5A; vk++)
            {
                keys.Add(vk);
            }

            for (int vk = 0x30; vk <= 0x39; vk++)
            {
                keys.Add(vk);
            }

            keys.Add(0x25);
            keys.Add(0x26);
            keys.Add(0x27);
            keys.Add(0x28);

            keys.Add(0x20);
            keys.Add(0x1B);
            keys.Add(0x0D);

            keys.Add(0x10);
            keys.Add(0x11);
            keys.Add(0x12);

            return keys.ToArray();
        }
    }
}
