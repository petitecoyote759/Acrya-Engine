using Acrya;
using SDL2;
using static SDL2.SDL;
using ShortTools.PlanetaryForge;




namespace Tester
{
    internal static class General
    {
        public static void Main()
        {
            Map map = new Map();
            (TileID[][], float[][]) mapPair = MapGenerator.CreateMap(100, 100);
            map.map = mapPair.Item1;

            AcryaEngine.Init(map);
        }
    }



    internal class Map : IMap
    {
        public TileID[][] map;

        public int Width => map.Length;
        public int Height => map[0].Length;



        private static readonly (byte, byte, byte) Black = (255, 0, 0);
        private static readonly (byte, byte, byte) White = (0, 255, 0);
        private static readonly Dictionary<TileID, (byte, byte, byte)> TileColours = new Dictionary<TileID, (byte, byte, byte)>()
        {
            { TileID.Cliff, (0x4B, 0x4B, 0x4B) },
            { TileID.Water, (0x14, 0xA0, 0xC8) },
            { TileID.Sand, (0xC8, 0xC8, 0x14) },
            { TileID.Grass, (0x0A, 0x82, 0x0A) },
            { TileID.Forest, (0x0F, 0x73, 0x14) }
        };
        public IntPtr GenerateMapImage(IntPtr SDLRenderer, out int pixelsPerTile)
        {
            pixelsPerTile = 1;

            IntPtr texture = SDL_CreateTexture(SDLRenderer, SDL_PIXELFORMAT_RGBA8888, (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width * pixelsPerTile, Height * pixelsPerTile);

            IntPtr previousTarget = SDL_GetRenderTarget(SDLRenderer);
            SDL_SetRenderTarget(SDLRenderer, texture);


            SDL_Rect destRect = new SDL_Rect();
            destRect.w = pixelsPerTile;
            destRect.h = pixelsPerTile;

            for (int x = 0; x < Width; x++)
            {
                destRect.x = x * pixelsPerTile;
                for (int y = 0; y < Height; y++)
                {
                    destRect.y = y * pixelsPerTile;

                    (byte, byte, byte) colour = TileColours[map[x][y]];
                    SDL_SetRenderDrawColor(SDLRenderer, colour.Item1, colour.Item2, colour.Item3, 255);
                    SDL_RenderFillRect(SDLRenderer, ref destRect);
                }
            }

            SDL_SetRenderTarget(SDLRenderer, previousTarget);
            Console.WriteLine($"Created : {SDL_GetError()}");
            return texture;
        }

    }
}