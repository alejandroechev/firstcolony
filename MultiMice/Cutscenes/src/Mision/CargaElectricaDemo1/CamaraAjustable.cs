using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;


namespace CargaElectricaDemo1
{

    public class CamaraAjustable
    {
        public Vector3 cameraPosition;
        public Vector3 cameraLookAt;
        public Matrix cameraProjectionMatrix;
        public Matrix cameraViewMatrix;

        public CamaraAjustable(GraphicsDeviceManager graphics)
        {
            cameraLookAt = new Vector3(0.0f,0.0f,0.0f);
            cameraPosition = new Vector3(0.0f, 350.0f,700.0f);
            cameraViewMatrix = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            cameraProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40.0f),
                                     graphics.GraphicsDevice.Viewport.AspectRatio,1.0f,30000.0f);
        } 


        public Vector3 RandomPointOnCircle(Vector3 centro)
        {
            const float radius = 3;
            Random random = new Random();

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius + centro.X, centro.Y, y * radius + centro.Z);
        }


    }

}
