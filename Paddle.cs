using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PongGame
{
    public class Paddle
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; private set; }
        public float Speed { get; set; }
        public Texture2D Texture { get; set; }

        public Paddle(Texture2D texture, Vector2 position, Vector2 size, float speed)
        {
            Texture = texture;
            Position = position;
            Size = size;
            Speed = speed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y), Color.White);
        }

        public virtual void Update(GameTime gameTime)
        {
            // Future updates for paddle movement or logic
        }
    }
}
