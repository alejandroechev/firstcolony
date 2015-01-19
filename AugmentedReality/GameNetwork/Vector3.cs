using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameNetwork
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }

        public Vector3(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public Vector3(float x, float y, float z) 
            : this(x,y,z,1)
        {
        }

        public Vector3(float x, float y)
            : this(x, y, 0, 1)
        {
        }

        public Vector3()
            : this(0, 0, 0, 1)
        {
        }

        /// <summary>
        /// Returns the magnitude of a 3-dimensional vector
        /// </summary>
        /// <returns></returns>
        public float Magnitude3()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>
        /// Normalizes a 3-dimensional vector
        /// </summary>
        public Vector3 Normalize3()
        {
            float currentMagnitude = Magnitude3();
            X /= currentMagnitude;
            Y /= currentMagnitude;
            Z /= currentMagnitude;
            return this;
        }

        public Vector3 Clamp3()
        {
            X = Math.Min(X, 1);
            Y = Math.Min(Y, 1);
            Z = Math.Min(Z, 1);
            return this;
        }

        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z, v1.W + v2.W);
        }

        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z, v1.W - v2.W);
        }

        public static Vector3 operator *(float scalar, Vector3 v)
        {
            return new Vector3(scalar * v.X, scalar * v.Y, scalar * v.Z, scalar*v.W);
        }

        public static Vector3 operator *(Vector3 v, float scalar)
        {
            return new Vector3(scalar * v.X, scalar * v.Y, scalar * v.Z, scalar * v.W);
        }

        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z, v1.W * v2.W);
        }

        public static Vector3 operator /(Vector3 v, float scalar)
        {
            return new Vector3(v.X / scalar, v.Y / scalar, v.Z / scalar, v.W / scalar);
        }

        public static Vector3 Cross3(Vector3 v1, Vector3 v2)
        {
            Vector3 crossVec = new Vector3();
            crossVec.X = v1.Y * v2.Z - v2.Y * v1.Z;
            crossVec.Y = v2.X * v1.Z - v1.X * v2.Z;
            crossVec.Z = v1.X * v2.Y - v2.X * v1.Y;
            crossVec.W = 0.0f;
            return crossVec;
        }

        /// <summary>
        /// Dot product of a 3-dimensional vector
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static float Dot3(Vector3 v1, Vector3 v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

    }
}
