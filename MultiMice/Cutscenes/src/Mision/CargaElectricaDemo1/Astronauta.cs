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
using RawInputSharp;
using XNAnimation;
using XNAnimation.Controllers;
//using XNAnimationPipeline;
using XNAnimation.Effects;



namespace CargaElectricaDemo1
{
    public class Astronauta: Objeto3D
    {
        public SkinnedModel skinnedModel;
        public int cargaElectrica;
        public int id;
        public Vector3 destino;
        public Vector3 direccion;
        public Vector3 retorno;
        public BoundingSphere efectoElectrico;
        public bool moviendose;
        public Vector3 scaleModel;

        public Astronauta( Model modelObj,Vector3 positionObj,Vector3 scaleObj, Vector3 rotationObj,int idObj)
                    :base(modelObj,positionObj,Vector3.Zero,rotationObj)
        {
            id = idObj;
            cargaElectrica = 0;
            destino = Vector3.Zero;
            direccion = Vector3.Zero;
            retorno = Vector3.Zero;
            efectoElectrico = new BoundingSphere(positionObj, 50);
            moviendose = false;
            scaleModel = scaleObj;
        }


        public override void Draw(AbstractGame Game)
        {
            foreach (ModelMesh modelMesh in this.skinnedModel.Model.Meshes)
            {
                foreach (SkinnedModelBasicEffect effect in modelMesh.Effects)
                {
                    // Setup camera
                    effect.View = Game.Camara.cameraViewMatrix;
                    effect.Projection = Game.Camara.cameraProjectionMatrix;

                    // Set the animated bones to the model
                    if(Game.animationController[id] != null)
                        effect.Bones = Game.animationController[id].SkinnedBoneTransforms;

                    effect.World = Matrix.CreateFromYawPitchRoll(this.rotation.X,
                                                                this.rotation.Y,
                                                                this.rotation.Z) *
                                                                Matrix.CreateScale(scaleModel) *
                                                                Matrix.CreateTranslation(this.position);

                    // OPTIONAL - Configure material
                    effect.Material.DiffuseColor = new Vector3(0.8f);
                    effect.Material.SpecularColor = new Vector3(0.3f);
                    effect.Material.SpecularPower = 8;

                    // OPTIONAL - Configure lights
                    effect.AmbientLightColor = new Vector3(0.01f);
                    effect.LightEnabled = true;
                    effect.EnabledLights = EnabledLights.One;
                    effect.PointLights[0].Color = Vector3.One;
                    effect.PointLights[0].Position = new Vector3(30);                  
                }
                // Draw a model mesh
                modelMesh.Draw();
            }

            base.Draw(Game);
        }


        public void Update(GameTime gameTime, Vector3 newPosition, Astronauta[] Astronautas, Objeto3D[] campoElectrico)
        {

            BoundingSphere LM = new BoundingSphere(Vector3.Zero, 150);       
            float length2;
                    
            if (!near(newPosition, destino))
            {               
                direccion = newPosition - position;               
                destino = newPosition;
                direccion.Normalize();
                if(direccion.Z>=0)
                    rotation.X=(float)Math.Asin(direccion.X);
                else
                    rotation.X = (float)-Math.Asin(direccion.X)+MathHelper.Pi;
            }

            if (newPosition.Y != 1 && !near(position,destino) )
            {              
                    position.X += direccion.X;
                    position.Z += direccion.Z;
                    moviendose = true;
            }
          

            else
                moviendose = false;

            if( !LM.Intersects(efectoElectrico))
            {
                 position.X -= 2*direccion.X;
                 position.Z -= 2*direccion.Z;
                 destino = position;
                 moviendose = false;
            }



            //Mover astronautas cuando ambos tienen su carga activada
            foreach (Astronauta player in Astronautas)
            {
                if ((player.id != id) && (player.id % 3 == id % 3) && (campoElectrico[player.id].isAlive && campoElectrico[id].isAlive))
                {
                    if (efectoElectrico.Intersects(player.efectoElectrico))
                    {

                        direccion = player.position - position;
                        length2 = direccion.LengthSquared();
                        direccion.Normalize();

                        if (player.cargaElectrica*cargaElectrica>0 && length2 > 100)
                        {
                            position -= Math.Abs(player.cargaElectrica * cargaElectrica) * direccion / length2;
                            player.position += Math.Abs(player.cargaElectrica * cargaElectrica) * direccion / length2;

                            // SALIRSE DEL MAPA
                            if (!LM.Intersects(efectoElectrico))
                            {
                                direccion = position;
                                direccion.Normalize();
                                position -= 2 * direccion;
                            }

                            if (!LM.Intersects(player.efectoElectrico))
                            {
                                direccion = player.position;
                                direccion.Normalize();
                                player.position -= 2 * player.direccion;
                            }
                            
                        }

                        if (player.cargaElectrica*cargaElectrica<=0 && length2 > 900)
                        {
                            position += Math.Abs(player.cargaElectrica * cargaElectrica) * direccion / length2;
                            player.position -= Math.Abs(player.cargaElectrica * cargaElectrica) * direccion / length2;
                        }
                    }
                    player.destino = player.position;
                    destino = position;
                }

            }      

            efectoElectrico.Center.Y = 0;
            efectoElectrico.Center = position;
            base.Update(gameTime);
        }


        public bool near(Vector3 position, Vector3 destino)
        {
            if ((position.X < destino.X + 2 && position.X > destino.X - 2) && (position.Z < destino.Z + 2 && position.Z > destino.Z - 2))
            {
                return true;
            }
            else
                return false;
        }


        bool[] previousMouseState = new bool[9];
        bool[] previousMouseState0 = new bool[9];
       

    }
}
