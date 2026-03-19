using SDL2;
using ShortTools.General;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using Acrya.ECSHandlers;
using Acrya.ECSComponents;
using System.Globalization;



namespace Acrya.Renderer
{
    // Game specific functions.
    internal static partial class Renderer
    {
        private static IntPtr mapTexture;
        public const int drawGridTileSize = 16;

        public static int drawGridWidth = -1;
        public static int drawGridHeight = -1;

        public static int PixelsPerTile => pixelsPerTile;
        internal static int pixelsPerTile = -1;

        public static void Setup()
        {
            while (AcryaEngine.map is null) { Thread.Sleep(10); }

            // <<Map Image Generation>> //

            debugger.AddLog($"Creating surface for map of size {screenWidth}x{screenHeight}", WarningLevel.Debug);
            

            // <<Main Image Creation>> //
            mapTexture = AcryaEngine.map.GenerateMapImage(SDLRenderer, out pixelsPerTile);

            textures.Add(mapTexture);


            // <<Full Map Render>> //
            Refresh();


            Camera.zoom = (float)(AcryaEngine.map.Width * pixelsPerTile) / (float)screenWidth;
            debugger.AddLog($"Initiating with a zoom of {Camera.zoom}");
        }


        public static void Refresh()
        {
            
            drawGridWidth = (int)MathF.Ceiling(screenWidth / (float)drawGridTileSize);
            drawGridHeight = (int)MathF.Ceiling(screenHeight / (float)drawGridTileSize);
            for (int x = 0; x < drawGridWidth; x++)
            {
                for (int y = 0; y < drawGridHeight; y++)
                {
                    gridDrawRequest.Add((x, y));
                }
            }
        }





        static HashSet<(int, int)> gridDrawRequest = new HashSet<(int, int)>();
        static HashSet<int> entityDrawRequests = new HashSet<int>();
        private static void Render(float dt)
        {
            ECSHandler.DoEntityRenderTasks(dt);

            srcRect.w = drawGridTileSize + 2;
            srcRect.h = srcRect.w;
            srcRect.x = 0; srcRect.y = 0;

            targetRect.w = (int)((drawGridTileSize + 2) * Camera.zoom);
            targetRect.h = targetRect.w;

            _ = SDL_SetTextureBlendMode(screenTexture, SDL_BlendMode.SDL_BLENDMODE_NONE);
            lock (gridDrawRequest)
            {
                foreach ((int, int) coordinate in gridDrawRequest)
                {
                    targetRect.x = GetPx((coordinate.Item1 * drawGridTileSize) - 1); // draw with an extra pixel of buffer to help with zoom float issues
                    targetRect.y = GetPy((coordinate.Item2 * drawGridTileSize) - 1);
                    if (targetRect.x < -targetRect.w || targetRect.y < -targetRect.w ||
                        targetRect.x >= screenWidth || targetRect.y >= screenHeight)
                    {
                        continue;
                    }

                    srcRect.x = (coordinate.Item1 * drawGridTileSize) - 1; srcRect.y = (coordinate.Item2 * drawGridTileSize) - 1;
                    //SDL_RenderCopy(SDLRenderer, mapTexture, ref srcRect, ref srcRect);
                    _ = SDL_RenderCopy(SDLRenderer, mapTexture, ref srcRect, ref targetRect);
                }
                gridDrawRequest.Clear();
            }
            _ = SDL_SetTextureBlendMode(screenTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
            lock (entityDrawRequests)
            {
                foreach (int uid in entityDrawRequests)
                {
                    bool success = ECSHandler.GetEntityComponent(uid, out EC_Render renderComponent);
                    success &= ECSHandler.GetEntityComponent(uid, out EC_Entity entityData);
                    if (!success) { continue; }

                    if (renderComponent.Image == IntPtr.Zero) 
                    {
                        targetRect.w = 1; targetRect.h = 1;
                        targetRect.x = GetPx(entityData.x);
                        targetRect.y = GetPy(entityData.y);


                        _ = SDL_SetRenderDrawColor(SDLRenderer, 60, 10, 70, 255);

                        _ = SDL_RenderDrawPoint(SDLRenderer, targetRect.x, targetRect.y);
                    }
                    else
                    {
                        targetRect.x = GetPx(entityData.x);
                        targetRect.y = GetPy(entityData.y);
                        targetRect.w = (int)(renderComponent.width * Camera.zoom);
                        targetRect.h = (int)(renderComponent.height * Camera.zoom);

                        _ = SDL_RenderCopyEx(SDLRenderer, renderComponent.Image, IntPtr.Zero, ref targetRect, 
                            renderComponent.angle, IntPtr.Zero, SDL_RendererFlip.SDL_FLIP_NONE);
                    }
                }
                entityDrawRequests.Clear();
            }


            // <<FPS Drawing>> //
            Write(0, 0, 20, 30, currentFPS.ToString(CultureInfo.CurrentCulture));
            Write(0, 40, 20, 30, ECSHandler.currentFPS.ToString(CultureInfo.CurrentCulture));
            int topLeftX = (int)(Camera.x / drawGridTileSize);
            int topLeftY = (int)(Camera.y / drawGridTileSize);
            // refresh that area to make sure it renders text correctly
            gridDrawRequest.Add((topLeftX, topLeftY));
            gridDrawRequest.Add((topLeftX + 1, topLeftY));
            gridDrawRequest.Add((topLeftX, topLeftY + 1));
            gridDrawRequest.Add((topLeftX + 1, topLeftY + 1));
        }


        private static Dictionary<string, IntPtr> textCache = new Dictionary<string, IntPtr>();
        private static Queue<string> textCacheQueue = new Queue<string>();
        private const int textCacheLength = 100;
        private static void Write(int posX, int posY, int widthPerChar, int height, string text, string font = "Aller_Bd")
        {
            IntPtr textImage;
            if (textCache.ContainsKey(text))
            {
                textImage = textCache[text];
            }
            else
            {
                IntPtr surface = SDL_ttf.TTF_RenderText_Solid(fonts[font], text, Black);
                textImage = SDL_CreateTextureFromSurface(SDLRenderer, surface);
                SDL_FreeSurface(surface);
                textCache.Add(text, textImage);
                textures.Add(textImage);
                textCacheQueue.Enqueue(text);

                if (textCacheQueue.Count > textCacheLength)
                {
                    string textToRemove = textCacheQueue.Dequeue();
                    SDL_DestroyTexture(textCache[textToRemove]);
                    textCache.Remove(textToRemove);
                }
            }

            targetRect.x = posX; targetRect.y = posY;
            targetRect.w = widthPerChar * text.Length; targetRect.h = height;
            _ = SDL_RenderCopy(SDLRenderer, textImage, IntPtr.Zero, ref targetRect);
        }




        private static int GetPx(float x)
        {
            return (int)(Camera.zoom * (x - Camera.x));
        }
        private static int GetPy(float y)
        {
            return (int)(Camera.zoom * (y - Camera.y));
        }










        // <<Requests>> //
        public static void RequestDrawGrid(int gridX, int gridY)
        {
            lock (gridDrawRequest)
            {
                gridDrawRequest.Add((gridX, gridY));
            }
        }
        public static void RequestEntityDraw(int gridX, int gridY, int uid)
        {
            lock (entityDrawRequests)
            {
                entityDrawRequests.Add(uid);
            }
        }
    }



    [StructLayout(LayoutKind.Explicit)]
    internal struct RGBA8888Colour
    {
        [FieldOffset(0)] public uint data;
        [FieldOffset(0)] public byte r;
        [FieldOffset(1)] public byte g;
        [FieldOffset(2)] public byte b;
        [FieldOffset(3)] public byte a;
    }
}
