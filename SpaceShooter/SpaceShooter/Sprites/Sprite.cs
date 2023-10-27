using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpaceShooter.Manager;
using SpaceShooter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceShooter.Sprites
{
    public class Sprite : Component, ICloneable
    {
        protected Dictionary<string, Animation> _animations;//словарь анимации
        protected AnimationManager _animationManager;//менеджер анимации
        
        protected float _layer { get; set; }//слой

        protected Vector2 _origin { get; set; }//исходное положение
        protected Vector2 _position { get; set; }//положение

        protected float _rotation { get; set; }//поворот
        protected float _scale { get; set; }// маштаб

        protected Texture2D _texture;

        public List<Sprite> Children { get; set; }

        public Color Colour { get; set; }

        public bool IsRemoved { get; set; }

        public float Layer
        {
            get { return _layer; }
            set
            {
                _layer = value;

                if (_animationManager != null)
                    _animationManager.Layer = _layer;
            }
        }

        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;

                if (_animationManager != null)
                    _animationManager.Origin = _origin;
            }
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;

                if (_animationManager != null)
                    _animationManager.Position = _position;
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                if (_texture != null)
                {
                    return new Rectangle((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, _texture.Width, _texture.Height);
                }

                if (_animationManager != null)
                {
                    var animation = _animations.FirstOrDefault().Value;

                    return new Rectangle((int)Position.X - (int)Origin.X, (int)Position.Y - (int)Origin.Y, animation.FrameWidth, animation.FrameHeight);
                }

                throw new Exception("Unknown sprite");
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;

                if (_animationManager != null)
                    _animationManager.Rotation = value;
            }
        }

        public readonly Color[] TextureData;

        public Matrix Transform
        {
            get
            {
                return Matrix.CreateTranslation(new Vector3(-Origin, 0)) *
                  Matrix.CreateRotationZ(_rotation) *
                  Matrix.CreateTranslation(new Vector3(Position, 0));
            }
        }

        public Sprite Parent;

        /// <summary>
        /// Область спрайта, с которой "потенциально" можно было бы столкнуться
        /// </summary>
        public Rectangle CollisionArea
        {
            get
            {
                return new Rectangle(Rectangle.X, Rectangle.Y, MathHelper.Max(Rectangle.Width, Rectangle.Height), MathHelper.Max(Rectangle.Width, Rectangle.Height));
            }
        }

        public Sprite(Texture2D texture)
        {
            _texture = texture;

            Children = new List<Sprite>();

            Origin = new Vector2(_texture.Width / 2, _texture.Height / 2);

            Colour = Color.White;

            TextureData = new Color[_texture.Width * _texture.Height];
            _texture.GetData(TextureData);
        }

        public Sprite(Dictionary<string, Animation> animations)
        {
            _texture = null;

            Children = new List<Sprite>(); 

            Colour = Color.White;

            TextureData = null;

            _animations = animations;

            var animation = _animations.FirstOrDefault().Value;

            _animationManager = new AnimationManager(animation);

            Origin = new Vector2(animation.FrameWidth / 2, animation.FrameHeight / 2);
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_texture != null)
                spriteBatch.Draw(_texture, Position, null, Colour, _rotation, Origin, 1f, SpriteEffects.None, Layer);
            else if (_animationManager != null)
                _animationManager.Draw(spriteBatch);
        }

        public bool Intersects(Sprite sprite)
        {
            if (this.TextureData == null)
                return false;

            if (sprite.TextureData == null)
                return false;

            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space
            var transformAToB = this.Transform * Matrix.Invert(sprite.Transform);

            /* Когда точка перемещается в локальном пространстве A, она перемещается в локальном пространстве B с
                фиксированным направлением и расстоянием, пропорциональными перемещению в A.
                Этот алгоритм проходит по одному пикселю за раз вдоль осей X и Y A
                Вычислите аналогичные шаги в B: l
            */

            var stepX = Vector2.TransformNormal(Vector2.UnitX, transformAToB);
            var stepY = Vector2.TransformNormal(Vector2.UnitY, transformAToB);

            // Вычислить верхний левый угол A в локальном пространстве B
            // Эта переменная будет повторно использована для отслеживания начала каждой строки
            var yPosInB = Vector2.Transform(Vector2.Zero, transformAToB);

            for (int yA = 0; yA < this.Rectangle.Height; yA++)
            {
                // Start at the beginning of the row
                var posInB = yPosInB;

                for (int xA = 0; xA < this.Rectangle.Width; xA++)
                {
                    // Округлить до ближайшего пикселя
                    var xB = (int)Math.Round(posInB.X);
                    var yB = (int)Math.Round(posInB.Y);

                    if (0 <= xB && xB < sprite.Rectangle.Width &&
                        0 <= yB && yB < sprite.Rectangle.Height)
                    {
                        //Получите цвета перекрывающихся пикселей
                        var colourA = this.TextureData[xA + yA * this.Rectangle.Width];
                        var colourB = sprite.TextureData[xB + yB * sprite.Rectangle.Width];

                        // Если оба пикселя не полностью прозрачны
                        if (colourA.A != 0 && colourB.A != 0)
                        {
                            return true;
                        }
                    }

                    // Перейти к следующему ряду
                    posInB += stepX;
                }

                // Move to the next row
                yPosInB += stepY;
            }

            // Пересечение не найдено
            return false;
        }

        //public virtual void OnCollide(Sprite sprite)
        //{

        //}

        public object Clone()
        {
            var sprite = this.MemberwiseClone() as Sprite;

            if (_animations != null)
            {
                sprite._animations = this._animations.ToDictionary(c => c.Key, v => v.Value.Clone() as Animation);
                sprite._animationManager = sprite._animationManager.Clone() as AnimationManager;
            }

            return sprite;
        }
    }
}
