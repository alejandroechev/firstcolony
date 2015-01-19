using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Astronautas2D.Levels;
using Astronautas2D.Factories;
using Astronautas2D.GameObjects;
using Astronautas2D.GameObjects.Entities;
using Astronautas2D.GUI.GUIElements;
using Microsoft.Xna.Framework;
using Astronautas2D.Visual_Components;
using Astronautas2D.Objectives;
using Microsoft.Xna.Framework.Graphics;

namespace Astronautas2D.Utils
{
    public class PlayerManager
    {
        private const int totalPlayers = 3;
        private Player[] players;
        private MiceManager miceManager;
        // -- Player GUI
        private HUD[] huds;
        private Codec codec;
        private ObjectiveBoard board;
        // --
        private GUIManager guiManager;
        private TimeManager timeManager;
        private ScoreManager scoreManager;
        private float wait = 1500f;
        private Objective currentObjective;
        private bool teamMode;
        public bool TeamMode { set { teamMode = value; } }
        private bool moveEnabled;
        private bool shootEnabled;
        private bool forceActivationEnabled;
        private bool chargeChangeEnabled;
        private bool showCharge;
        private bool allAtOnce;
        private int groupNumber;
        public int GroupNumber
        {
            set { 
                groupNumber = value;
                if(board != null) board.GroupNumber = value;
            }
        }

        public PlayerManager(Objective target, MiceManager miceManager, GUIManager guiManager, TimeManager tm, ScoreManager sm, bool[] conditions, bool allAtOnce, int groupNumber)
        {
            this.groupNumber = groupNumber;
            this.allAtOnce = allAtOnce;
            players = new Player[totalPlayers];
            this.miceManager = miceManager;
            this.miceManager.centerMice();
            this.guiManager = guiManager;
            this.timeManager = tm;
            this.scoreManager = sm;
            teamMode = true;
            huds = new HUD[totalPlayers + 1];
            codec = guiManager.createCodec();
            this.currentObjective = target;
            this.moveEnabled = conditions[(int)levelConditions.move];
            this.shootEnabled = conditions[(int)levelConditions.shoot];
            this.forceActivationEnabled = conditions[(int)levelConditions.forceActivation];
            this.chargeChangeEnabled = conditions[(int)levelConditions.chargeChange];
            this.showCharge = conditions[(int)levelConditions.showCharge];
        }

        public void ActivateMouseEvents()
        {
            if(moveEnabled) miceManager.LeftClick += onLeftClick;
            if(shootEnabled) miceManager.RightClick += onRightClick;
            if(forceActivationEnabled) miceManager.CenterClick += onCenterClick;
            if(chargeChangeEnabled) miceManager.WheelChange += onWheelChange;
        }

        public void resetPlayers()
        {
            foreach (Player p in players)
            {
                p.Reset();
            }
        }

        public void DeactivateMouseEvents()
        {
            if (moveEnabled) miceManager.LeftClick -= onLeftClick;
            if (shootEnabled) miceManager.RightClick -= onRightClick;
            if (forceActivationEnabled) miceManager.CenterClick -= onCenterClick;
            if (chargeChangeEnabled) miceManager.WheelChange -= onWheelChange;
        }

        public void createMessageBoard()
        {
            this.board = guiManager.createObjectiveBoard(groupNumber, currentObjective.Description);
        }

        public void createHUD()
        {
            if (teamMode)
            {
                huds[(int)playerId.team] = guiManager.createTeamHUD();
            }
            else
            {
                huds[(int)playerId.circle] = guiManager.createPlayerHUD(playerId.circle);
                huds[(int)playerId.square] = guiManager.createPlayerHUD(playerId.square);
                huds[(int)playerId.triangle] = guiManager.createPlayerHUD(playerId.triangle);
            }
        }

        public Astronaut2D getAstronaut(playerId index)
        {
            return players[(int)index].Astronaut;
        }

        public Bullet2D[] getBullets(playerId index)
        {
            return players[(int)index].Bullets;
        }

        public void addPlayer(GameObjectFactory objectFactory, playerId id, float mass, Vector3 scale, Vector3 position, float chargeValue)
        {
            Astronaut2D astronaut;
            Slider slider;
            if (chargeChangeEnabled)
            {
                astronaut = objectFactory.createAstronaut(id, mass, scale, position);
                slider = guiManager.createSlider(new Vector2(position.X, position.Y), id, astronaut.ChargeLevels);
            }
            else
            {
                astronaut = objectFactory.createAstronaut(id, chargeValue, mass, scale, position);
                slider = guiManager.createSlider(new Vector2(position.X, position.Y), id, astronaut.ChargeLevels);
                slider.moveMarker(astronaut.ChargeLevel);
            }
            
            Bullet2D[] bullets = new Bullet2D[2];
            bullets[0] = objectFactory.createBullet(position, new Vector2(objectFactory.AspectRatio.X,objectFactory.AspectRatio.X));
            bullets[1] = objectFactory.createBullet(position, new Vector2(objectFactory.AspectRatio.X, objectFactory.AspectRatio.X));
            Player player = new Player(id, astronaut, bullets, slider, showCharge);
            players[(int)id] = player;
        }

        public void Update(float elapsedTime)
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                players[i].Update(elapsedTime);

                if (allAtOnce && (players[i].Astronaut.CurrentState == state.dying))
                {
                    for (int j = 0; j < totalPlayers; j++)
                    {
                        if (i != j) players[j].Astronaut.ActiveForces = false;
                    }
                }

            }
            if (teamMode)
            {
                this.huds[(int)playerId.team].UpdateTime(timeManager.getPlayerTime(playerId.team));
                this.huds[(int)playerId.team].UpdateScore(scoreManager.getScore(playerId.team).ToString()+"/3");
            }
            else
            {
                this.huds[(int)playerId.circle].UpdateTime(timeManager.getPlayerTime(playerId.circle));
                this.huds[(int)playerId.circle].UpdateScore(scoreManager.getScore(playerId.circle).ToString() + "/3");
                this.huds[(int)playerId.square].UpdateTime(timeManager.getPlayerTime(playerId.square));
                this.huds[(int)playerId.square].UpdateScore(scoreManager.getScore(playerId.square).ToString() + "/3");
                this.huds[(int)playerId.triangle].UpdateTime(timeManager.getPlayerTime(playerId.triangle));
                this.huds[(int)playerId.triangle].UpdateScore(scoreManager.getScore(playerId.triangle).ToString() + "/3");
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch, Writer fontWriter)
        {
            for (int i = 0; i < totalPlayers; i++)
            {
                players[i].DrawAstronaut(gameTime, spriteBatch, fontWriter);
            }

            for (int i = 0; i < totalPlayers; i++)
            {
                players[i].DrawBullets(gameTime, spriteBatch, fontWriter);
            }
        }

        public void onCenterClick(object sender, MiceClickArgs e)
        {
            if(allAtOnce)
            {
                players[e.Index].Astronaut.UpdateGlow();
                bool result = 
                    (players[(int)playerId.circle].Astronaut.IsGlowing 
                    && players[(int)playerId.square].Astronaut.IsGlowing
                    && players[(int)playerId.triangle].Astronaut.IsGlowing);
                for (int i = 0; i < totalPlayers; i++)
                {
                    players[i].Astronaut.ActiveForces = result;
                }
            }
            else
            {
                players[e.Index].onCenterClick(e.Position);
            }            
        }

        public void onLeftClick(object sender, MiceClickArgs e)
        {
            players[e.Index].onLeftClick(e.Position);
        }

        public void onRightClick(object sender, MiceClickArgs e)
        {
            players[e.Index].onRightClick(e.Position);
        }

        public void onWheelChange(object sender, MiceWheelArgs e)
        {
            players[e.Index].onWheelChange(e.Delta);
            players[e.Index].TimeCounter = wait;
        }

        public void showScoreProfit(playerId id, int profit, Vector2 position)
        {
            guiManager.showScore(id, position, profit.ToString());
        }
    }
}
