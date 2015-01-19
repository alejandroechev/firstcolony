using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GoblinXNA.SceneGraph;

namespace ARGame
{
    public abstract class AbstractGame : Game
    {
        public SpriteFont font;
        public SpriteFont bigFont;
        public GraphicsDeviceManager graphics;
        public ContentManager contentManager;
        public Scene sceneGraph;
    }

    public abstract class AbstractLevel
    {
        public abstract List<GameObject> ObjectsList { get; }
        public abstract Dictionary<int, GameObject> ObjectsTable { get; }
        public abstract List<ARAstronaut> Players { get; }
        public virtual event Action<bool> LevelEnded;
        public virtual event Action<bool> SubLevelEnded;
        public abstract string File {get;set;}

        public int PlayerId { get; set; }

        protected AbstractGame game;

        public AbstractLevel(AbstractGame game)
        {
            this.game = game;
        }

        public abstract void Load(string file);
        public abstract void Init();
        public virtual void Close() { }
        public virtual void Draw() { }
        public abstract void Update(float elpasedTime);
        public virtual void ResetSubLevel() { }
        public virtual void PassSubLevel() { }
        public virtual void Pause(bool pause) { }

    }
}
