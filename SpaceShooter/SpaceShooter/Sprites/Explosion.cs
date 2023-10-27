using Microsoft.Xna.Framework;
using SpaceShooter.Models;
using SpaceShooter.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceShooter.Sprites
{
    public class Explosion: Sprite
    {
        public Explosion(Dictionary<string, Animation> animations) : base(animations) { }

        private float _timer = 0f;

        public override void Update(GameTime gameTime)
        {
            _animationManager.Update(gameTime);
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            //взрыв будет удален через 0ю3 сек
            if(_timer > _animationManager.CurrentAnimation.FrameCount * _animationManager.CurrentAnimation.FrameSpeed)
                IsRemoved= true;
        }
    }
}
