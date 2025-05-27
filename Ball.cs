using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame
{
    public class Ball
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; private set; }
        public Vector2 Velocity { get; set; } // Represents speed and direction
        public Texture2D Texture { get; set; }

        public Ball(Texture2D texture, Vector2 position, Vector2 size, Vector2 initialVelocity)
        {
            Texture = texture;
            Position = position;
            Size = size;
            Velocity = initialVelocity;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), Color.White);
        }
    }
}
