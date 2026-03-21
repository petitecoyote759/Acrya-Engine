using Acrya.Renderer.UI;
using ILGPU.Algorithms.RadixSortOperations;
using ShortTools.General;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace Acrya.Renderer
{
    public static class Handler
    {
        public static bool KeyIsDown(SDL_Keycode key)
        {
            if (keys.ContainsKey(key) == false) { return false; }
            return keys[key];
        }

        internal static Dictionary<SDL_Keycode, bool> keys = new Dictionary<SDL_Keycode, bool>();



        public static void Setup()
        {
            // Called from main thread in RenderTools.Setup, so no using SDL functions here, if you do, move the call to the render thread.
            SDL_Keycode[] keyCodes = (SDL_Keycode[])Enum.GetValues(typeof(SDL_Keycode));

            foreach (SDL_Keycode code in keyCodes)
            {
                keys.Add(code, false);
            }
        }





        public static void HandleEvents(float dt)
        {
            float oldX = Camera.x;
            float oldY = Camera.y;
            float oldZoom = Camera.zoom;

            while (SDL_PollEvent(out SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT: // ensures that quitting works and runs cleanup code
                        Renderer.Stop();
                        break;


                    case SDL_EventType.SDL_KEYDOWN:
                        //RendererTools.debugger.AddLog($"{e.key.keysym.sym}");
                        if (keys.ContainsKey(e.key.keysym.sym)) { keys[e.key.keysym.sym] = true; }
                        break;

                    case SDL_EventType.SDL_KEYUP:
                        //RendererTools.debugger.AddLog($"{e.key.keysym.sym}");
                        if (keys.ContainsKey(e.key.keysym.sym)) { keys[e.key.keysym.sym] = false; }
                        break;


                    case SDL_EventType.SDL_WINDOWEVENT:
                        // RendererTools.debugger.AddLog($"{e.window.windowEvent}");
                        // SDL_WindowEvent_LEAVE
                        // SDL_WindowEvent_Focus_Gained
                        if (e.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
                        {
                            Renderer.debugger.AddLog($"Focus lost, pausing renderer...", WarningLevel.Debug);
                            Renderer.Pause();
                        }
                        else if (e.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                        {
                            Renderer.debugger.AddLog($"Focus gained, resuming renderer...", WarningLevel.Debug);
                            Renderer.Resume();
                        }
                        break;


                    case SDL_EventType.SDL_MOUSEWHEEL:
                        //RendererTools.debugger.AddLog($"{e.wheel.preciseX} {e.wheel.preciseY}", WarningLevel.Debug);
                        // preciseY to get it, + is up, - is down
                        if (e.wheel.preciseY < 0)
                        {
                            Camera.zoom /= Camera.zoomSpeed * -e.wheel.preciseY * Camera.zoomScale;
                        }
                        else if (e.wheel.preciseY > 0)
                        {
                            Camera.zoom *= Camera.zoomSpeed * e.wheel.preciseY * Camera.zoomScale;
                        }
                        // Zoom normalisation
                        // Makes the camera zoom based on the middle
                        Camera.zoom = float.Clamp(Camera.zoom, Camera.zoomMin, Camera.zoomMax);
                        float mx = Renderer.ScreenWidth * (Camera.zoom - oldZoom) / (Camera.zoom * oldZoom);
                        float my = Renderer.ScreenHeight * (Camera.zoom - oldZoom) / (Camera.zoom * oldZoom);

                        _ = SDL_GetMouseState(out int mouseX, out int mouseY);
                        Camera.x += mx * (mouseX / (float)Renderer.ScreenWidth);
                        Camera.y += my * (mouseY / (float)Renderer.ScreenHeight);

                        Renderer.debugger.AddLog($"Camera zoom is {Camera.zoom}");
                        break;


                    case SDL_EventType.SDL_MOUSEMOTION:

                        _ = SDL_GetMouseState(out mouseX, out mouseY);
                        foreach (UIItem item in Renderer.UIItems)
                        {
                            if (item is not Button button) { continue; }

                            if (mouseX < button.targetRect.x || mouseX > button.targetRect.x + button.targetRect.w) { button.HoveredOver = false; continue; }
                            if (mouseY < button.targetRect.y || mouseY > button.targetRect.y + button.targetRect.h) { button.HoveredOver = false; continue; }

                            button.HoveredOver = true;
                        }

                        break;

                    default:

                        Renderer.debugger.AddLog($"{e.type}", WarningLevel.Debug);
                        break;
                }
            }

            Camera.currentSpeed = Camera.speed;

            if (keys[SDL_Keycode.SDLK_LSHIFT])
            {
                Camera.currentSpeed *= 2;
            }
            if (keys[SDL_Keycode.SDLK_w])
            {
                Camera.y -= Camera.currentSpeed * dt;
            }
            if (keys[SDL_Keycode.SDLK_a])
            {
                Camera.x -= Camera.currentSpeed * dt;
            }
            if (keys[SDL_Keycode.SDLK_s])
            {
                Camera.y += Camera.currentSpeed * dt;
            }
            if (keys[SDL_Keycode.SDLK_d])
            {
                Camera.x += Camera.currentSpeed * dt;
            }
            

            if (oldX != Camera.x || oldY != Camera.y || oldZoom != Camera.zoom)
            {
                int width = (int)(Renderer.ScreenWidth / (Camera.zoom * Renderer.pixelsPerTile));
                int height = (int)(Renderer.ScreenHeight / (Camera.zoom * Renderer.pixelsPerTile));

                Camera.x = float.Clamp(Camera.x, 0, Math.Max(AcryaEngine.map!.Width - width, 0));
                Camera.y = float.Clamp(Camera.y, 0, Math.Max(AcryaEngine.map!.Height - height, 0));

                Renderer.Refresh();
            }
        }
    }
}
