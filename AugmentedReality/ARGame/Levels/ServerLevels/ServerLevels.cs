using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Globalization;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using GoblinXNA;
using GoblinXNA.Graphics;
using GoblinXNA.SceneGraph;
using Model = GoblinXNA.Graphics.Model;
using GoblinXNA.Graphics.Geometry;
using GoblinXNA.Physics;
using GoblinXNA.UI.UI2D;

using ARUtils;
using GameModel;

namespace ARGame
{
    public class ServerLevel1 : ServerLevel
    {
        public ServerLevel1(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 1: Encuentra tu marcador", Color.White, game.font);
        }

    }

    public class ServerLevel2 : ServerLevel
    {
        public ServerLevel2(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 2: Activa tu campo electrico", Color.White, game.font);
        }

    }

    public class ServerLevel3 : ServerLevel
    {
        public ServerLevel3(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 3: Rompe el cristal con tu laser", Color.White, game.font);
        }

    }

    public class ServerLevel4 : ServerLevel
    {
        public ServerLevel4(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 4: Prueba distintas cargas", Color.White, game.font);
        }

    }
    public class ServerLevel4b : ServerLevel
    {
        public ServerLevel4b(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 4b: Lleva el crystal al portal", Color.White, game.font);
        }

    }
    public class ServerLevel5 : ServerLevel
    {
        public ServerLevel5(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 5: Carga fija", Color.White, game.font);
        }

    }

    public class ServerLevel6 : ServerLevel
    {
        public ServerLevel6(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 6: Accion y reaccion", Color.White, game.font);
        }

    }

    public class ServerLevel7 : ServerLevel
    {
        public ServerLevel7(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Ent. 7: Suma de fuerzas", Color.White, game.font);
        }

    }

    public class ServerMission1 : ServerLevel
    {
        public ServerMission1(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Mision 1: Asteroides quietos", Color.White, game.font);
        }

    }

    public class ServerMission2 : ServerLevel
    {
        public ServerMission2(AbstractGame game)
            : base(game)
        {


        }

        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Mision 2: Asteroide gravitatorio", Color.White, game.font);
        }

    }

    public class ServerMission3 : ServerLevel
    {
        public ServerMission3(AbstractGame game)
            : base(game)
        {


        }


        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Mision 3: Cargas fijas", Color.White, game.font);
        }

    }

    public class ServerMissionEnded : ServerLevel
    {
        public ServerMissionEnded(AbstractGame game)
            : base(game)
        {


        }


        public override void Draw()
        {
            UI2DRenderer.WriteText(new Vector2(10, 0), ">> Mision Terminada", Color.White, game.font);
        }

    }

}
