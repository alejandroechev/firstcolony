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
    public class Objeto3D: IGameObject
    {

        public Model model;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public bool enableLight;

        public Vector3 diffuse;
        public Vector3 ambient;
        public Vector3 emmisive;

        public bool enableDirectionalLight;

        public Vector3 lightDirection;
        public Vector3 lightDiffuse;
        public Vector3 lightSpecular;

        public bool boneTransforms = true;

        private Matrix[] transforms;

        public Objeto3D(Model modelObj,Vector3 positionObj,Vector3 scaleObj, Vector3 rotationObj)
        {
            model = modelObj;
            position = positionObj;
            rotation = rotationObj;
            scale =scaleObj;
            isAlive = true;

            diffuse = new Vector3(1, 1, 1);
            ambient = new Vector3(0.2f, 0.2f, 0.2f);
            emmisive = new Vector3(0, 0, 0);

            enableDirectionalLight = false;
            enableLight = false;

            lightDirection = new Vector3(0, 0, 0);
            lightDiffuse = new Vector3(1, 1, 1);
            lightSpecular = new Vector3(1, 1, 1);

            transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            
        }


        public override void Draw(AbstractGame Game)
        {
            foreach (ModelMesh mesh in this.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.LightingEnabled = enableLight;
                    effect.AmbientLightColor = ambient;
                    effect.DiffuseColor = diffuse;
                    effect.EmissiveColor = emmisive;

                    effect.DirectionalLight0.Enabled = enableDirectionalLight;
                    effect.DirectionalLight0.Direction = lightDirection;
                    effect.DirectionalLight0.DiffuseColor = lightDiffuse;
                    effect.DirectionalLight0.SpecularColor = lightSpecular;

                    if (boneTransforms)
                    {

                        effect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] *
                                                                    Matrix.CreateFromYawPitchRoll(this.rotation.X,
                                                                     this.rotation.Y,
                                                                     this.rotation.Z) *
                                                                     Matrix.CreateScale(this.scale) *
                                                                     Matrix.CreateTranslation(this.position));
                    }
                    else
                    {
                        effect.Parameters["World"].SetValue(Matrix.CreateFromYawPitchRoll(this.rotation.X,
                                                                  this.rotation.Y,
                                                                  this.rotation.Z) *
                                                                  Matrix.CreateScale(this.scale) *
                                                                  Matrix.CreateTranslation(this.position));
                    
                    }
                    effect.Parameters["Projection"].SetValue(Game.Camara.cameraProjectionMatrix);
                    effect.Parameters["View"].SetValue(Game.Camara.cameraViewMatrix);
                }
                mesh.Draw();
            }
        }

        public override void Update(GameTime gameTime)
        {
        }

    }


   



}
