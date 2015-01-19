using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using ARUtils;
using GoblinXNA.SceneGraph;

using Microsoft.Xna.Framework;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework.Graphics;


namespace ARGame.Levels
{
    /* Identificación del marcador.
     * El nivel termina cuando el jugador ve su marcador.
     */

    public class Level1 : Level
    {
        public Level1(AbstractGame game) : base(game) { }

        private ARGeneric obj;
        private MarkerNode mynode;

        public override void Init()
        {
            base.Init();
            
            
        }
      
        public override void Draw()
        {
            base.Draw();
            (gameGUI as GameGUI.GamePlayGUI).ForceFieldButton.Visible = false;
            (gameGUI as GameGUI.GamePlayGUI).RayShooterButton.Visible = false;
            (gameGUI as GameGUI.GamePlayGUI).PlusButton.Visible = false;
            (gameGUI as GameGUI.GamePlayGUI).MinusButton.Visible = false;
            (gameGUI as GameGUI.GamePlayGUI).ChargeSlider.Visible = false;
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Encuentra tu marcador", Color.White, game.font);
           
        }
        public override void Load(string file)
        {
            base.Load(file);
            mynode = ARManager.myNode;

            XDocument xmlDoc = XDocument.Load(file);
            XElement xmlScene = xmlDoc.Elements("scene").First();
            XElement xmlObjects = xmlScene.Elements("object_list").First();

            foreach (XElement genericObject in xmlObjects.Elements("generic"))
            {
                Vector3 position = LoadXYZ(genericObject.Elements("position").First());

                Vector3 scale = LoadXYZ(genericObject.Elements("scale").First());

                string model = genericObject.Attribute("model").Value;

                ARGeneric obj = new ARGeneric(this.game.contentManager.GetModel(model), scale, position);

                this.obj = obj;
                
            }

            
        }
        public override void Update(float elpasedTime)
        {
            base.Update(elpasedTime);

            if (game.sceneGraph.MarkerTracker.FindMarker(ARManager.myNode.MarkerID))
            {
                
                if (levelCompleted == 0)
                {
                    this.markerNode.AddChild(this.obj.ObjectNode);
                    PassSubLevel();
                    
                    gameGUI.WriteMessage("\n\n\n\n\nMuy bien novato! Encontraste el marcador!"
                        +"\nRecuerda que este sera tu campo de entrenamiento para futuro. "+
                        "\nSigue practicando mientras esperas nuevas"+"\ninstrucciones del Comandante.");
                    levelCompleted++;
                }
                
            }
            
        }
    }
}
