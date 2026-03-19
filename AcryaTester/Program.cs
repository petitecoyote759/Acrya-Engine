using Acrya;
using SDL2;
using ShortTools.PlanetaryForge;
using System.Runtime.InteropServices;
using static SDL2.SDL;




namespace Tester
{
    internal static class General
    {
        const int mapSize = 1000;
        public static void Main()
        {
            Thread.CurrentThread.Name = "Main Thread";

            Map map = new Map();
            (TileID[][], float[][]) mapPair = MapGenerator.CreateMap(mapSize, mapSize);
            map.map = mapPair.Item1;
            map.altitudes = mapPair.Item2;

            AcryaEngine.Init(map);
        }
    }



    internal class Map : IMap
    {
        public TileID[][] map;
        public float[][] altitudes;

        public int Width => map.Length;
        public int Height => map[0].Length;



        private static readonly (byte, byte, byte) Black = (255, 0, 0);
        private static readonly (byte, byte, byte) White = (0, 255, 0);
        private static readonly Dictionary<TileID, uint> TileColours = new Dictionary<TileID, uint>()
        {
            { TileID.Cliff, 0x4B4B4BFF },
            { TileID.Water, 0x14A0C8FF },
            { TileID.Sand, 0xC8C814FF },
            { TileID.Grass, 0x0A820AFF },
            { TileID.Forest, 0x0F7314FF }
        };
        public IntPtr GenerateMapImage(IntPtr SDLRenderer, out int pixelsPerTile)
        {
            pixelsPerTile = 1;

            IntPtr previousTarget = SDL_GetRenderTarget(SDLRenderer);

            SDL_Rect targetRect = new SDL_Rect();

            targetRect.w = 1;
            targetRect.h = 1;

            // <<Main Image Creation>> //
            IntPtr surface = SDL_CreateRGBSurfaceWithFormat(0, Width * pixelsPerTile, Width * pixelsPerTile, 32, SDL_PIXELFORMAT_RGBA8888);
            SDL_SetSurfaceBlendMode(surface, SDL_BlendMode.SDL_BLENDMODE_NONE);
            SDL_LockSurface(surface);
            for (int x = 0; x < Width; x++)
            {
                targetRect.x = x;
                for (int y = 0; y < Height; y++)
                {
                    targetRect.y = y;

                    RGBA8888Colour colour = new RGBA8888Colour() { data = TileColours[map[x][y]] };

                    colour.r = (byte)(colour.r * ((5 + altitudes[x][y]) / 6f));
                    colour.g = (byte)(colour.g * ((5 + altitudes[x][y]) / 6f));
                    colour.b = (byte)(colour.b * ((5 + altitudes[x][y]) / 6f));

                    SDL_FillRect(surface, ref targetRect, colour.data);
                }
            }
            SDL_UnlockSurface(surface);

            IntPtr texture = SDL_CreateTextureFromSurface(SDLRenderer, surface);
            SDL_FreeSurface(surface);

            SDL_SetRenderTarget(SDLRenderer, previousTarget);
            Console.WriteLine($"Created : {SDL_GetError()}");
            return texture;
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