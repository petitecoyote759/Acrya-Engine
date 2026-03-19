using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrya.Renderer
{
    internal static class Camera
    {
        public const float speed = 50f;
        public static float currentSpeed = speed;
        public const float zoomSpeed = 1.1f;
        public static float zoomScale = 1f;
        public static float zoomMax = 100;
        public static float zoomMin = 1;
        public static float zoom = 1;
        // position of the top left of the camera
        public static float x;
        public static float y;
    }
}
