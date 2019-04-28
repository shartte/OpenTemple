using System;
using System.Runtime.InteropServices;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.TigSubsystems
{
    public class SystemEventPump
    {
        [TempleDllLocation(0x11E74578)] private TimePoint system_events_processed_time;

        [TempleDllLocation(0x101DF440)]
        public void PumpSystemEvents()
        {
            system_events_processed_time = TimePoint.Now;

            Tig.MessageQueue.Enqueue(new Message(MessageType.UPDATE_TIME));

            Tig.Keyboard.Update();

            ProcessWindowMessages();

            // TODO sound_process_loop(); 0x101e4360
        }

        [TempleDllLocation(0x101de880)]
        private void ProcessWindowMessages()
        {
            while (PeekMessage(out var msg, IntPtr.Zero, 0, 0, 1))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
            uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }
    }
}