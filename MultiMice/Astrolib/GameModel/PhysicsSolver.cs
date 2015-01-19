using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GameModel
{
    public delegate void ContactBegin(IPhysicBody physObj1, IPhysicBody physObj2);

    public class CoulombPhysics
    {
        protected Dictionary<string, PhysicMaterial> materialTable;

        public CoulombPhysics() 
        {
            materialTable = new Dictionary<string, PhysicMaterial>();
        }

        public void AddPhysicMaterial(PhysicMaterial material)
        {
            materialTable.Add(material.MaterialName1 + "@" + material.MaterialName2, material);
        }

        public void Update(float timeElapsed, IPhysicBody[] bodies)
        {
            FindCollisions(bodies);

            UpdateForces(bodies);

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Update(timeElapsed);
            }
        }

        public void Update(float timeElapsed, List<IPhysicBody> bodies)
        {
            FindCollisions(bodies);

            UpdateForces(bodies);

            foreach (IPhysicBody b in bodies)
            {
                b.Update(timeElapsed);
            }
        }

        private void UpdateForces(IPhysicBody[] aux)
        {
            int n = aux.Length;
            for (int i = 0; i < n; i++)
            {
                IPhysicBody A = aux[i];
                Vector2 posA = new Vector2(A.Position.X, A.Position.Y);
                for (int j = i + 1; j < n; j++)
                {
                    IPhysicBody B = aux[j];
                    Vector2 posB = new Vector2(B.Position.X, B.Position.Y);
                    Vector3 distance = A.Position - B.Position;
                    if (distance.Length() >= A.Radius + B.Radius)
                    {
                        float forceMagnitud = 1000 * A.Charge * B.Charge / (distance).LengthSquared();
                        distance.Normalize();
                        Vector3 force = forceMagnitud * new Vector3(distance.X, distance.Y, 0);
                        A.ApplyForce(force);
                        B.ApplyForce(-force);
                    }
                }
            }
        }

        private void UpdateForces(List<IPhysicBody> aux)
        {
            int i = 0;
            foreach (IPhysicBody A in aux)
            {
                if (A.ActiveForces)
                {
                    int j = 0;
                    Vector2 posA = new Vector2(A.Position.X, A.Position.Y);
                    foreach (IPhysicBody B in aux)
                    {
                        if (j >= i + 1)
                        {
                            if (B.ActiveForces)
                            {
                                Vector2 posB = new Vector2(B.Position.X, B.Position.Y);
                                Vector3 distance = A.Position - B.Position;
                                if (distance.Length() >= A.Radius + B.Radius)
                                {
                                    float forceMagnitud = 1000 * A.Charge * B.Charge / (distance).LengthSquared();
                                    distance.Normalize();
                                    Vector3 force = forceMagnitud * new Vector3(distance.X, distance.Y, 0);
                                    A.ApplyForce(force);
                                    B.ApplyForce(-force);
                                }
                            }
                        }
                        j++;
                    }
                }
                i++;
            }
        }



        private void FindCollisions(IPhysicBody[] aux)
        {
            int n = aux.Length;
            for (int i = 0; i < n; i++)
            {
                IPhysicBody A = aux[i];

                for (int j = i + 1; j < n; j++)
                {
                    IPhysicBody B = aux[j];

                    if (CheckCollision(A, B))
                    {
                        if (materialTable.ContainsKey(A.MaterialName + "@" + B.MaterialName))
                            materialTable[A.MaterialName + "@" + B.MaterialName].ContactBeginCallback(A,B);
                        else if(materialTable.ContainsKey(B.MaterialName + "@" + A.MaterialName))
                            materialTable[B.MaterialName + "@" + A.MaterialName].ContactBeginCallback(B,A);
                    }
                }
            }
        }

        private void FindCollisions(List<IPhysicBody> aux)
        {
            int i = 0;
            foreach (IPhysicBody A in aux)
            {
                if (A.ActiveCollisions)
                {
                    int j = 0;
                    Vector2 posA = new Vector2(A.Position.X, A.Position.Y);
                    foreach (IPhysicBody B in aux)
                    {
                        if (j >= i + 1)
                        {
                            if (B.ActiveCollisions)
                            {
                                if (CheckCollision(A, B))
                                {
                                    if (materialTable.ContainsKey(A.MaterialName + "@" + B.MaterialName))
                                        materialTable[A.MaterialName + "@" + B.MaterialName].ContactBeginCallback(A, B);
                                    else if (materialTable.ContainsKey(B.MaterialName + "@" + A.MaterialName))
                                        materialTable[B.MaterialName + "@" + A.MaterialName].ContactBeginCallback(B, A);
                                }
                            }
                        }
                        j++;
                    }
                }
                i++;
            }
        }



        private bool CheckCollision(IPhysicBody sphere1, IPhysicBody sphere2)
        {
            Vector3 aux = sphere1.Position - sphere2.Position;
            Vector2 d = new Vector2(aux.X, aux.Y);

            if (d.Length() < sphere1.Radius + sphere2.Radius)
            {
                sphere1.Collision(sphere2);
                sphere2.Collision(sphere1);
                return true;
            }
            else return false;

        }

        public Vector3 CalculateForce(IPhysicBody body, IPhysicBody[] aux)
        {
            IPhysicBody A = body;
            Vector3 totalForce = new Vector3();
            Vector2 posA = new Vector2(A.Position.X, A.Position.Y);

            for (int j = 0; j < aux.Length; j++)
            {
                IPhysicBody B = aux[j];
                if (A != B)
                {
                    Vector2 posB = new Vector2(B.Position.X, B.Position.Y);
                    Vector3 distance = A.Position - B.Position;
                    float forceMagnitud = 1000 * A.PotentialCharge * B.PotentialCharge / (distance).LengthSquared();
                    distance.Normalize();
                    Vector3 force = forceMagnitud * new Vector3(distance.X, distance.Y, 0);
                    totalForce += force;
                }
            }
            return totalForce;
        }
    }

    public class PhysicsSolver
    {
        public static void Update(float timeElapsed, IPhysicBody[] bodies)
        {
            CollisionSolver.FindCollisions(bodies);

            ElectricSolver.UpdateForces(bodies);

            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].Update(timeElapsed);
            }
        }
    }

    public class ElectricSolver
    {

        public static void UpdateForces(IPhysicBody[] aux)
        {
            int n = aux.Length;
            for (int i = 0; i < n; i++)
            {
                IPhysicBody A = aux[i];
                Vector2 posA = new Vector2(A.Position.X,A.Position.Y);
                for (int j = i + 1; j < n; j++)
                {
                    IPhysicBody B = aux[j];
                    Vector2 posB = new Vector2(B.Position.X,B.Position.Y);
                    Vector2 distance = posA - posB;
                    float forceMagnitud = 1000 * A.Charge * B.Charge / (distance).LengthSquared();
                    distance.Normalize();
                    Vector3 force = forceMagnitud * new Vector3(distance.X,distance.Y,0);
                    A.ApplyForce(force);
                    B.ApplyForce(-force);
                }
            }
        }
    }

    class CollisionSolver
    {
        public static void CheckCollision(IPhysicBody sphere1, IPhysicBody sphere2)
        {
            Vector3 d = sphere1.Position - sphere2.Position;
            if (d.Length() < sphere1.Radius + sphere2.Radius)
            {
                sphere1.Collision(sphere2);
                sphere2.Collision(sphere1);
            }

        }

        //metodo por si queremos usar boundingBoxes en vez de esferas.
        /* public static bool CheckCollision(IPhysicBody box1, IPhysicBody box2)
         {
             float d;
            //distancia en x
             d = (float)Math.Abs(box1.Position.X - box2.Position.X);
             if (d < box1.Box.X / 2.0f + box2.Box.X / 2.0f) //se topan en x
             {
                 d = (float)Math.Abs(box1.Position.Y - box2.Position.Y);
                 if (d < box1.Box.Y / 2.0f + box2.Box.Y / 2.0f) //se topan en y
                 {
                     d = (float)Math.Abs(box1.Position.Z - box2.Position.Z);
                     if (d < box1.Box.Z / 2.0f + box2.Box.Z / 2.0f) //se topan en z
                     {
                         return true;
                     }
                 }
             }
             return false;
         }*/
        public static void FindCollisions(IPhysicBody[] aux)
        {
            int n = aux.Length;
            for (int i = 0; i < n; i++)
            {
                IPhysicBody A = aux[i];

                for (int j = i + 1; j < n; j++)
                {
                    IPhysicBody B = aux[j];

                    CheckCollision(A, B);
                }
            }
        }
    }
}
