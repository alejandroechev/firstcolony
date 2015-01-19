using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Astronautas2D.GameObjects;
using Astronautas2D.GameObjects.Entities;
using Astronautas2D.Utils;
using Astronautas2D.Visual_Components;
using AstroLib;

namespace Astronautas2D.Factories
{
    

    /// <summary>
    /// Clase estatica que permite la creación de todos los objetos del juego
    /// </summary>
    public class GameObjectFactory
    {
        private Texture2D[,,] objectsTextures;                  // Texturas de los objetos
        private Texture2D[] backgroundTextures;
        private MiceManager miceManager;
        private Vector2[] randomPositions;
        private bool[] randomPositionsUsed;
        private int objectLength, statesLength, backgroundLength, playerLength, soundLength;
        private bool narrative;
        private float[] radius;
        private Rectangle boundries;
        private Vector2 aspectRatio;
        public Vector2 AspectRatio { get { return aspectRatio; } }

        public event Action LevelEnded;

        public GameObjectFactory(MiceManager manager, Rectangle boundries)
        {
            // Obtenemos los largos de los arreglos
            this.boundries = boundries;
            aspectRatio = new Vector2(boundries.Width / 1440f, boundries.Height / 900f);
            this.narrative = Configuration.Instance.GetBoolParam("player", "narrative");
            objectLength = Enum.GetNames(typeof(objectId)).Length;
            statesLength = Enum.GetNames(typeof(state)).Length;
            playerLength = Enum.GetNames(typeof(playerId)).Length;
            backgroundLength = Enum.GetNames(typeof(backgrounds)).Length;
            // Los inicializamos
            objectsTextures = new Texture2D[objectLength, playerLength, statesLength];
            backgroundTextures = new Texture2D[backgroundLength];
            miceManager = manager;
            radius = new float[objectLength];
            this.initializeRadius(narrative);
        }

        public void initializeRadius(bool narrative)
        {
            if(narrative)
            {
                radius[(int)objectId.asteroid] = 32f * aspectRatio.X;
                radius[(int)objectId.astronaut] = 57f * aspectRatio.Y;
                radius[(int)objectId.bullet] = 10f * aspectRatio.X;
                radius[(int)objectId.crystal] = 25f * aspectRatio.X;
                radius[(int)objectId.portal] = 10f * aspectRatio.X;
                radius[(int)objectId.zone] = 30f * aspectRatio.X;
            }
            else
            {
                radius[(int)objectId.asteroid] = 40f * aspectRatio.X;
                radius[(int)objectId.astronaut] = 30f * aspectRatio.X;
                radius[(int)objectId.bullet] = 10f * aspectRatio.X;
                radius[(int)objectId.crystal] = 19f * aspectRatio.X;
                radius[(int)objectId.portal] = 20f * aspectRatio.X;
                radius[(int)objectId.zone] = 30f * aspectRatio.X;
            }
        }

        public void initializeRandomPositions(List<Vector3> RespawnPositions)
        {
            randomPositions = new Vector2[RespawnPositions.Count];
            randomPositionsUsed = new bool[RespawnPositions.Count];
            int counter = 0;
            foreach (Vector3 v in RespawnPositions)
            {
                randomPositions[counter] = new Vector2(v.X, v.Y);
                counter++;
            }
        }

        public Vector2 getRandomPosition()
        {
            Random random = new Random();
            int pos = random.Next(randomPositions.Length);
            while(randomPositionsUsed[pos])
            {
                pos = random.Next(randomPositions.Length);
            }
            randomPositionsUsed[pos] = true;
            return randomPositions[pos];
        }

        public void freePosition(Vector2 position)
        {
            for (int i = 0; i < randomPositions.Length; i++)
            {
                if (randomPositions[i].X == position.X && randomPositions[i].Y == position.Y)
                {
                    randomPositionsUsed[i] = false;
                    return;
                }
            }
        }


        public void RandomizeCrystal(Crystal2D crystal, bool inOrder)
        {
            Random random = new Random();

            if (crystal.Charge > 0)
            {
                // Obtendremos una negativa
                crystal.Charge = -crystal.Charge;
            }
            else
            {
                crystal.Charge = -crystal.Charge;
            }

            // Almacenamos la posición actual
            Vector2 previousRespawn = new Vector2(crystal.RespawnPosition.X,crystal.RespawnPosition.Y);

            if (inOrder)
            {
                int aux = -1;
                for (int i = 0; i < randomPositionsUsed.Length; i++)
                {
                    if (!randomPositionsUsed[i])
                        aux = i;
                }
                if (aux == -1)
                {
                    if (LevelEnded != null)
                        LevelEnded();
                }
                else
                {
                    randomPositionsUsed[aux] = true;
                    crystal.RespawnPosition = new Vector3(randomPositions[aux].X, randomPositions[aux].Y, 1);
                    crystal.Position = new Vector3(randomPositions[aux].X, randomPositions[aux].Y, 1);
                }
            }
            else
            {
                // Aleatorizamos la posicion
                int pos = random.Next(randomPositions.Length);
                while (randomPositionsUsed[pos])
                {
                    pos = random.Next(randomPositions.Length);
                }
                randomPositionsUsed[pos] = true;

                for (int i = 0; i < randomPositions.Length; i++)
                {
                    if (randomPositions[i] == previousRespawn)
                    {
                        randomPositionsUsed[i] = false;
                        break;
                    }
                }
                crystal.RespawnPosition = new Vector3(randomPositions[pos].X, randomPositions[pos].Y, 1);
                crystal.Position = new Vector3(randomPositions[pos].X, randomPositions[pos].Y, 1);
            }
            crystal.UpdateColor();
            crystal.Randomize = false;
        }

        /// <summary>
        /// Método que carga las texturas de los objetos
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            if (narrative)
            {
                // Astronauta
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle,(int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Circle\\front");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.up] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Circle\\up");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.down] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Circle\\down");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.left] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Circle\\left");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.right] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Circle\\right");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.dying] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");

                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Square\\front");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.up] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Square\\up");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.down] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Square\\down");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.left] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Square\\left");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.right] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Square\\right");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.dying] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");

                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Triangle\\front");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.up] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Triangle\\up");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.down] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Triangle\\down");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.left] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Triangle\\left");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.right] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\Triangle\\right");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.dying] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Astronaut\\dying");

                // Simbolos
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\circle");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\square");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\triangle");
                objectsTextures[(int)objectId.astronaut, (int)playerId.team, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\team");
                
                // Asteoride
                objectsTextures[(int)objectId.asteroid, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Asteroid\\idle");
                objectsTextures[(int)objectId.asteroid, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\dying");
                //Portal
                objectsTextures[(int)objectId.portal, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Portal\\idle");
                objectsTextures[(int)objectId.portal, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Portal\\idle");
                objectsTextures[(int)objectId.portal, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Portal\\idle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Portal\\idle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.portal, (int)playerId.circle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\circle");
                objectsTextures[(int)objectId.portal, (int)playerId.square, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\square");
                objectsTextures[(int)objectId.portal, (int)playerId.triangle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\triangle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\team");
                // Cristal
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\idle2");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.dying] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.signal] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Crystal\\signal");
                objectsTextures[(int)objectId.crystal, (int)playerId.circle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\circle");
                objectsTextures[(int)objectId.crystal, (int)playerId.square, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\square");
                objectsTextures[(int)objectId.crystal, (int)playerId.triangle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\triangle");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\team");
                // Flecha
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\noSymbol");
                objectsTextures[(int)objectId.arrow, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\circle");
                objectsTextures[(int)objectId.arrow, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\square");
                objectsTextures[(int)objectId.arrow, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\triangle");
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\arrowBody");
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Arrow\\arrowHead");
                // Bala
                objectsTextures[(int)objectId.bullet, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Bullet\\idle");
                // Campo de fuerza
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\ForceField\\idle");
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\ForceField\\appearing");
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.dying] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\ForceField\\dying");
                // Zonas
                objectsTextures[(int)objectId.zone, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Zone\\circleZone");
                objectsTextures[(int)objectId.zone, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Zone\\squareZone");
                objectsTextures[(int)objectId.zone, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Zone\\triangleZone");
                objectsTextures[(int)objectId.zone, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\Narrative\\GameObjects\\Zone\\zone");

                // Fondos
                /*backgroundTextures[(int)backgrounds.tutorial1] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial2] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial3] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial4] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial5] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial6] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.tutorial7] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                */
                backgroundTextures[(int)backgrounds.tutorial] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Tutorial");
                backgroundTextures[(int)backgrounds.mission] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Mission");
                /*
                backgroundTextures[(int)backgrounds.mission2] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Mission");
                backgroundTextures[(int)backgrounds.mission3] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Mission");
                backgroundTextures[(int)backgrounds.mission4] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Mission");
                backgroundTextures[(int)backgrounds.mission5] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\Mission");

                backgroundTextures[(int)backgrounds.ending] = content.Load<Texture2D>("Sprites\\Narrative\\Backgrounds\\ending");*/
            }
            else
            {
                // Astronauta
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\circle");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.circle, (int)state.dying] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");

                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\square");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.square, (int)state.dying] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");

                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\triangle");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");
                objectsTextures[(int)objectId.astronaut, (int)playerId.triangle, (int)state.dying] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Astronaut\\dying");

                // Asteoride
                objectsTextures[(int)objectId.asteroid, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Asteroid\\idle");
                objectsTextures[(int)objectId.asteroid, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\dying");
                //Portal
                objectsTextures[(int)objectId.portal, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Portal\\idleCircle");
                objectsTextures[(int)objectId.portal, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Portal\\idleSquare");
                objectsTextures[(int)objectId.portal, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Portal\\idleTriangle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Portal\\idle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.portal, (int)playerId.circle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\circle");
                objectsTextures[(int)objectId.portal, (int)playerId.square, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\square");
                objectsTextures[(int)objectId.portal, (int)playerId.triangle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\triangle");
                objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\team");
                // Cristal
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\idle");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.dying] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\dying");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.signal] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Crystal\\signal");
                objectsTextures[(int)objectId.crystal, (int)playerId.circle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\circle");
                objectsTextures[(int)objectId.crystal, (int)playerId.square, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\square");
                objectsTextures[(int)objectId.crystal, (int)playerId.triangle, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\triangle");
                objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.symbol] = content.Load<Texture2D>("Sprites\\Symbols\\team");
                // Flecha
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\noSymbol");
                objectsTextures[(int)objectId.arrow, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\circle");
                objectsTextures[(int)objectId.arrow, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\square");
                objectsTextures[(int)objectId.arrow, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\triangle");
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\arrowBody");
                objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Arrow\\arrowHead");
                // Bala
                objectsTextures[(int)objectId.bullet, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Bullet\\idle");
                // Campo de fuerza
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\ForceField\\idle");
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.appearing] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\ForceField\\appearing");
                objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.dying] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\ForceField\\dying");
                // Zonas
                objectsTextures[(int)objectId.zone, (int)playerId.circle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Zone\\targetCircle");
                objectsTextures[(int)objectId.zone, (int)playerId.square, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Zone\\targetSquare");
                objectsTextures[(int)objectId.zone, (int)playerId.triangle, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Zone\\targetTriangle");
                objectsTextures[(int)objectId.zone, (int)playerId.team, (int)state.idle] = content.Load<Texture2D>("Sprites\\NoNarrative\\GameObjects\\Zone\\targetCircle");
                
                // Fondos
                backgroundTextures[(int)backgrounds.tutorial] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                /*backgroundTextures[(int)backgrounds.tutorial3] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.tutorial4] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.tutorial5] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.tutorial6] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.tutorial7] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.tutorial8] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission1] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission2] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission3] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission4] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                backgroundTextures[(int)backgrounds.mission5] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\White");
                */
                backgroundTextures[(int)backgrounds.ending] = content.Load<Texture2D>("Sprites\\NoNarrative\\Backgrounds\\ending");
            }
        }

        public Texture2D getBackground(backgrounds index)
        {
            return backgroundTextures[(int)index];
        }

        public Astronaut2D createAstronaut(playerId id, float mass, Vector3 scale, Vector3 position)
        {
            Texture2D[] fieldTextures = new Texture2D[Enum.GetNames(typeof(state)).Length];
            fieldTextures[(int)state.appearing] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.appearing];
            fieldTextures[(int)state.idle] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.idle];
            fieldTextures[(int)state.dying] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.dying];

            Animation[] anim = this.createAstronautAnimations(id);

            Vector3 newScale = this.getNewScale(scale, aspectRatio);
            Rectangle body = getAstronautBody(newScale, anim);
            Astronaut2D astronaut = new Astronaut2D(body, radius[(int)objectId.astronaut], id, mass, newScale, position, anim, fieldTextures, narrative, aspectRatio);         
            return astronaut;
        }

        public Astronaut2D createAstronaut(playerId id, float charge, float mass, Vector3 scale, Vector3 position)
        {
            Texture2D[] fieldTextures = new Texture2D[Enum.GetNames(typeof(state)).Length];
            fieldTextures[(int)state.appearing] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.appearing];
            fieldTextures[(int)state.idle] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.idle];
            fieldTextures[(int)state.dying] = objectsTextures[(int)objectId.forceField, (int)playerId.team, (int)state.dying];

            Animation[] anim = this.createAstronautAnimations(id);

            Vector3 newScale = this.getNewScale(scale, aspectRatio);
            Rectangle body = getAstronautBody(newScale, anim);

            Astronaut2D astronaut = new Astronaut2D(body, radius[(int)objectId.astronaut], id, mass, newScale, position, anim, fieldTextures, narrative, aspectRatio);

            astronaut.Charge = charge;
            astronaut.OfflineCharge = charge;
            astronaut.UpdateColor();

            return astronaut;
        }

        private Vector3 getNewScale(Vector3 scale, Vector2 aspectRatio)
        {
            // La escala recibida se pondera por el aspect ratio
            // Nos quedaremos con el mínimo para ambos
            float minRation = (float)(Math.Min(aspectRatio.X, aspectRatio.Y));
            return new Vector3(scale.X * minRation, scale.Y * minRation, scale.Z);
        }

        private Rectangle getAstronautBody(Vector3 scale, Animation[] anim)
        {
            Rectangle body;

            if (narrative)
            {
                body = new Rectangle(0, 0, (int)(55 * scale.X), (int)(95 * scale.Y));
            }
            else
            {
                body = new Rectangle(0, 0, (int)(anim[(int)state.idle].FrameWidth * scale.X), (int)(anim[(int)state.idle].FrameHeight * scale.Y));
            }
            return body;

        }


        public Bullet2D createBullet(Vector3 position, Vector2 scale)
        {
            Texture2D[] sprites = new Texture2D[(int)Enum.GetNames(typeof(state)).Length];
            sprites[(int)state.idle] = objectsTextures[(int)objectId.bullet, (int)playerId.team, (int)state.idle];
            sprites[(int)state.dying] = objectsTextures[(int)objectId.crystal,(int)playerId.team, (int)state.dying];
            return new Bullet2D(radius[(int)objectId.bullet], position, sprites, scale);
        }


        public Asteroid2D createAsteroid(float mass, Vector3 scale, Vector3 position, Vector3 velocity, bool inertial)
        {
            Texture2D[] sprites = new Texture2D[(int)Enum.GetNames(typeof(state)).Length];
            sprites[(int)state.idle] = objectsTextures[(int)objectId.asteroid,(int)playerId.team, (int)state.idle];
            sprites[(int)state.appearing] = objectsTextures[(int)objectId.asteroid,(int)playerId.team, (int)state.appearing];
            float maxSize = Math.Max(scale.X * aspectRatio.X, scale.Y * aspectRatio.Y);
            Vector3 newScale = new Vector3(maxSize, maxSize, scale.Z);

            return new Asteroid2D(radius[(int)objectId.asteroid], mass, newScale, position, velocity, inertial, sprites);
        }

        public Crystal2D createCrystal(playerId id, float mass, Vector3 scale, Vector3 position,
            Vector3 velocity, bool inertial, float charge, bool isAlive, bool isDivisible, bool showTotalForce, bool showIndividualForces)
        {
            //Escala
            float maxSize = Math.Max(scale.X * aspectRatio.X, scale.Y * aspectRatio.Y);
            Vector3 newScale = new Vector3(maxSize, maxSize, scale.Z);
            // Cargamos las texturas utiles
            Texture2D[] sprites = new Texture2D[(int)Enum.GetNames(typeof(state)).Length];
            sprites[(int)state.idle] = objectsTextures[(int)objectId.crystal, (int)playerId.team,(int)state.idle];
            sprites[(int)state.dying] = objectsTextures[(int)objectId.crystal, (int)playerId.team,(int)state.dying];
            sprites[(int)state.appearing] = objectsTextures[(int)objectId.crystal, (int)playerId.team,(int)state.appearing];
            sprites[(int)state.signal] = objectsTextures[(int)objectId.crystal, (int)playerId.team, (int)state.signal];
            sprites[(int)state.symbol] = objectsTextures[(int)objectId.crystal, (int)id, (int)state.symbol];
            // Generamos la animación
            Animation[] anim = this.createCrystalAnimations(sprites);
            //Texturas de la flecha
            Texture2D[,] arrowTextures = new Texture2D[(int)Enum.GetNames(typeof(playerId)).Length, (int)Enum.GetNames(typeof(arrow)).Length];
            arrowTextures[(int)playerId.circle, (int)arrow.head] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal];
            arrowTextures[(int)playerId.circle, (int)arrow.body] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving];
            arrowTextures[(int)playerId.triangle, (int)arrow.head] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal];
            arrowTextures[(int)playerId.triangle, (int)arrow.body] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving];
            arrowTextures[(int)playerId.square, (int)arrow.head] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal];
            arrowTextures[(int)playerId.square, (int)arrow.body] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving];
            arrowTextures[(int)playerId.team, (int)arrow.head] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.signal];
            arrowTextures[(int)playerId.team, (int)arrow.body] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.moving];
            //Symbolos
            arrowTextures[(int)playerId.circle, (int)arrow.symbol] = objectsTextures[(int)objectId.arrow, (int)playerId.circle, (int)state.idle];
            arrowTextures[(int)playerId.square, (int)arrow.symbol] = objectsTextures[(int)objectId.arrow, (int)playerId.square, (int)state.idle];
            arrowTextures[(int)playerId.triangle, (int)arrow.symbol] = objectsTextures[(int)objectId.arrow, (int)playerId.triangle, (int)state.idle];
            arrowTextures[(int)playerId.team, (int)arrow.symbol] = objectsTextures[(int)objectId.arrow, (int)playerId.team, (int)state.idle];

            Crystal2D crystal = new Crystal2D(radius[(int)objectId.crystal], mass, newScale, position, velocity, inertial, charge, isAlive, isDivisible, anim, arrowTextures, showTotalForce, showIndividualForces, narrative, aspectRatio);
            crystal.ListIndex = (int)id;

            return crystal;
        }


        public Portal2D createPortal(Vector3 scale, Vector3 position, playerId id)
        {
            Texture2D[] sprites = new Texture2D[(int)Enum.GetNames(typeof(state)).Length];
            sprites[(int)state.idle] = objectsTextures[(int)objectId.portal, (int)id, (int)state.idle];
            sprites[(int)state.appearing] = objectsTextures[(int)objectId.portal, (int)playerId.team, (int)state.appearing];
            sprites[(int)state.symbol] = objectsTextures[(int)objectId.portal, (int)id, (int)state.symbol];
            return new Portal2D(radius[(int)objectId.portal], scale, position, sprites, narrative);
        }

        public Zone2D createZone(Vector2 scale, Vector2 position, int id)
        {
            playerId playerId = (playerId)id;
            Zone2D zone = new Zone2D(radius[(int)objectId.zone], scale, playerId, position, objectsTextures[(int)objectId.zone, (int)playerId, (int)state.idle]);
            return zone;
        }

        /// <summary>
        /// Construye las animaciones de un crystal, dependiendo de si se tiene o no narrativa
        /// </summary>
        /// <param name="textures">Texturas del crystal</param>
        /// <returns></returns>
        private Animation[] createCrystalAnimations(Texture2D[] textures)
        {
            Animation[] animations = new Animation[statesLength];

            if (narrative)
            {
                animations[(int)state.idle] = new Animation(textures[(int)state.idle], 0.1f, true, 1);
            }
            else
            {
                animations[(int)state.idle] = new Animation(textures[(int)state.idle], 0.1f, true, 8);
            }
            animations[(int)state.appearing] = new Animation(textures[(int)state.appearing], 0.1f, false, 4);
            animations[(int)state.dying] = new Animation(textures[(int)state.dying], 0.1f, false, 4);
            animations[(int)state.signal] = new Animation(textures[(int)state.signal], 0.0716f, true, 6);
            animations[(int)state.symbol] = new Animation(textures[(int)state.symbol], 0.1f, false, 1);

            return animations;
        }

        private Animation[] createAstronautAnimations(playerId id)
        {
            Animation[] animations = new Animation[statesLength];

            if (narrative)
            {
                animations[(int)state.idle] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.idle], 0.1f, true, 5);
                animations[(int)state.up] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.up], 0.1f, true, 5);
                animations[(int)state.down] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.down], 0.1f, true, 5);
                animations[(int)state.left] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.left], 0.1f, true, 5);
                animations[(int)state.right] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.right], 0.1f, true, 5);
            }
            else
            {
                animations[(int)state.idle] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.idle], 0.15f, false, 1);
                animations[(int)state.up] = animations[(int)state.idle];
                animations[(int)state.down] = animations[(int)state.idle];
                animations[(int)state.left] = animations[(int)state.idle];
                animations[(int)state.right] = animations[(int)state.idle];


            }
            animations[(int)state.appearing] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.appearing], 0.1f, true, 4);
            animations[(int)state.dying] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.dying], 0.15f, true, 4);
            animations[(int)state.symbol] = new Animation(objectsTextures[(int)objectId.astronaut, (int)id, (int)state.symbol], 0.1f, false, 1);

            return animations;
        }

    }
}

