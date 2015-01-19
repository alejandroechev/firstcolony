using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using AstroLib;

namespace  Astronautas2D.Utils
{
    public enum Sounds { victory, defeat, shoot, fieldOn, fieldOff, crystalDeath, crystalAppearing, crystalRespawn, crystalIn, crystalDissapear, astroDeath, bulletDeath}

    public class SoundManager
    {
        private bool mute;
        private float frontVolume, backVolume;
        private SoundEffect[] soundEffects;

        private static SoundManager instance;
        public static SoundManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new SoundManager();
                return instance;
            }
        }


        private SoundManager()
        {
            int soundLength = Enum.GetNames(typeof(Sounds)).Length;
            soundEffects = new SoundEffect[soundLength];
            mute = Configuration.Instance.GetBoolParam("sound", "mute");
            frontVolume = Configuration.Instance.GetFloatParam("sound", "frontVolume");
            backVolume = Configuration.Instance.GetFloatParam("sound", "backVolume");
            
        }

        public void Load(ContentManager content)
        {
            // Los sonidos
            soundEffects[(int)Sounds.shoot] = content.Load<SoundEffect>("Sounds\\Astronaut\\Shoot");
            soundEffects[(int)Sounds.fieldOn] = content.Load<SoundEffect>("Sounds\\Astronaut\\fieldOn");
            soundEffects[(int)Sounds.fieldOff] = content.Load<SoundEffect>("Sounds\\Astronaut\\fieldOff");
            soundEffects[(int)Sounds.victory] = content.Load<SoundEffect>("Sounds\\Victory");
            soundEffects[(int)Sounds.defeat] = content.Load<SoundEffect>("Sounds\\Defeat");
            soundEffects[(int)Sounds.crystalDeath] = content.Load<SoundEffect>("Sounds\\Crystal\\Death");
            soundEffects[(int)Sounds.crystalAppearing] = content.Load<SoundEffect>("Sounds\\Crystal\\Appearing");
            soundEffects[(int)Sounds.crystalRespawn] = content.Load<SoundEffect>("Sounds\\Crystal\\Respawn");
            soundEffects[(int)Sounds.crystalIn] = content.Load<SoundEffect>("Sounds\\Portal\\CrystalIn");
            soundEffects[(int)Sounds.crystalDissapear] = content.Load<SoundEffect>("Sounds\\Crystal\\Disappear");
            soundEffects[(int)Sounds.astroDeath] = content.Load<SoundEffect>("Sounds\\Astronaut\\Death");
            soundEffects[(int)Sounds.bulletDeath] = content.Load<SoundEffect>("Sounds\\Bullet\\Death");

        }

        public void Play(Sounds id)
        {
            if (!mute && Configuration.Instance.GetBoolParam("player","narrative"))
            {
                if (id == Sounds.victory || id == Sounds.defeat)
                {
                    soundEffects[(int)id].Play(frontVolume, 0, 0);
                }
                else
                {
                    soundEffects[(int)id].Play(backVolume, 0, 0);
                }
            }
        }
        
    
    }


}
