using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paralaxing.Anima
{
    public class Animation
    {
        private AnimaModel _animation;

        private float _timer;
        public float Layer { get; set; }

        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }

        public float Rotation { get; set; }
        public float Scale { get; set; }

        public Animation(AnimaModel animation)
        {
            _animation = animation;
            Scale = 1f;
        }

        public void Play(AnimaModel animation)
        {
            if (_animation == animation)
                return;

            _animation = animation;
            _animation.CurrentFrame = 0;
            _timer = 0;
        }

        public void Stop()
        {
            _timer = 0f;
            _animation.CurrentFrame = 0;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer > _animation.FrameSpeed)
            {
                _timer = 0f;
                _animation.CurrentFrame++;

                if (_animation.CurrentFrame >= _animation.FrameCount)
                    _animation.CurrentFrame = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animation.Texture,
           Position,
           new Rectangle(_animation.CurrentFrame * _animation.FrameWidth,
                         0,
                         _animation.FrameWidth,
                         _animation.FrameHeight),
           Color.White);
        }
    }
}
