using System;
using ImGuiNET;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Systems.DebugUI
{
    public class DebugUISystem : IDisposable
    {
        private readonly ImGuiRenderer _renderer;

        private readonly  WorldCamera _camera;

        public DebugUISystem(IMainWindow mainWindow, RenderingDevice device, WorldCamera camera)
        {
            _camera = camera;
            var hwnd = mainWindow.NativeHandle;
            var d3dDevice = device.mD3d11Device;
            var context = device.mContext;

            var guiContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(guiContext);

            ImGui.GetIO().Fonts.AddFontDefault();

            _renderer = new ImGuiRenderer();
            if (!_renderer.ImGui_ImplDX11_Init(hwnd, d3dDevice, context)) {
                throw new Exception("Unable to initialize IMGui!");
            }

            mainWindow.SetWindowMsgFilter(HandleMessage);
        }

        public void Dispose()
        {
            _renderer.ImGui_ImplDX11_Shutdown();
        }

        public void NewFrame()
        {
            _renderer.ImGui_ImplDX11_NewFrame((int) _camera.GetScreenWidth(), (int) _camera.GetScreenHeight());
        }

        public void Render()
        {
            ImGui.Render();
            var drawData = ImGui.GetDrawData();
            _renderer.ImGui_ImplDX11_RenderDrawLists(drawData);
        }

        private bool HandleMessage(uint message, ulong wParam, long lParam)
        {
            var io = ImGui.GetIO();
            switch (message)
            {
                case WM_LBUTTONDOWN:
                    io.MouseDown[0] = true;
                    return io.WantCaptureMouse;
                case WM_LBUTTONUP:
                    io.MouseDown[0] = false;
                    return io.WantCaptureMouse;
                case WM_RBUTTONDOWN:
                    io.MouseDown[1] = true;
                    return io.WantCaptureMouse;
                case WM_RBUTTONUP:
                    io.MouseDown[1] = false;
                    return io.WantCaptureMouse;
                case WM_MBUTTONDOWN:
                    io.MouseDown[2] = true;
                    return io.WantCaptureMouse;
                case WM_MBUTTONUP:
                    io.MouseDown[2] = false;
                    return io.WantCaptureMouse;
                case WM_MOUSEWHEEL:
                    io.MouseWheel += ((short) (wParam >> 16)) > 0 ? +1.0f : -1.0f;
                    return io.WantCaptureMouse;
                case WM_MOUSEMOVE:
                    io.MousePos.X = (short) (lParam);
                    io.MousePos.Y = (short) (lParam >> 16);
                    return false; // Always update, never take it
                case WM_KEYDOWN:
                    if (wParam < 256)
                        io.KeysDown[(int) wParam] = true;
                    return io.WantCaptureKeyboard;
                case WM_KEYUP:
                    if (wParam < 256)
                        io.KeysDown[(int) wParam] = false;
                    return io.WantCaptureKeyboard;
                case WM_CHAR:
                    // You can also use ToAscii()+GetKeyboardState() to retrieve characters.
                    if (wParam > 0 && wParam < 0x10000)
                        io.AddInputCharacter((ushort)wParam);
                    return io.WantCaptureKeyboard;
                default:
                    return false;
            }
        }

        private const uint WM_CHAR = 0x0102;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;
        private const uint WM_MBUTTONDOWN = 0x0207;
        private const uint WM_MBUTTONUP = 0x0208;
        private const uint WM_MOUSEMOVE = 0x0200;
        private const uint WM_MOUSEWHEEL = 0x020A;
        private const uint WM_RBUTTONDOWN = 0x0204;
        private const uint WM_RBUTTONUP = 0x0205;
    }
}