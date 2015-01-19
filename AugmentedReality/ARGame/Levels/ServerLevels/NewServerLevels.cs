using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GoblinXNA.UI.UI2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameNetwork;

namespace ARGame
{
    public class SharedResultantActionLevel : ServerLevel
    {
        public SharedResultantActionLevel(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal", Color.White, game.font);
        }
    }

    public class ComplementarityLevel : ServerLevel
    {
        public ComplementarityLevel(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal. No pueden modificar su carga.", Color.White, game.font);
        }
    }


    public class SharedOperativeActionLevel : ServerLevel
    {
        private bool[] activePlayers = new bool[3];

        public SharedOperativeActionLevel(AbstractGame game)
            : base(game)
        {


        }

        public override void SetPlayerInfo(PlayerState playerState, int numPlayers)
        {
            Players[playerState.Id].SetPosition(playerState.Position);
            Players[playerState.Id].Charge = playerState.Charge;
            activePlayers[playerState.Id] = playerState.IsAlive;

            if (AllPlayersActive(numPlayers))
            {
                for (int i = 0; i < numPlayers; i++)
                    Players[i].IsActive = true;
            }
            else
            {
                for (int i = 0; i < numPlayers; i++)
                    Players[i].IsActive = false;
            }
        }

        private bool AllPlayersActive(int numPlayers)
        {
            bool allActive = true;
            for (int i = 0; i < numPlayers; i++)
                allActive &= activePlayers[i];
            return allActive;
        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Lleven el cristal al portal. Todos deben activar para mover.", Color.White, game.font);
        }
    }


    
}
