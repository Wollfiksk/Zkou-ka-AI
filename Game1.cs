using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PongGame;

public enum GameState
{
    PreServe,
    Playing,
    GameOver
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Paddle _playerPaddle;
    private Paddle _opponentPaddle;
    private Ball _ball;
    private Texture2D _pixelTexture; // For drawing paddles and ball
    private int _playerScore;
    private int _opponentScore;
    private SpriteFont _scoreFont;
    private System.Random _random = new System.Random();
    private GameState _gameState;
    private string _winnerMessage = "";
    private KeyboardState _previousKeyboardState; // To detect single key presses


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = 800;
        _graphics.PreferredBackBufferHeight = 600;
        _graphics.ApplyChanges();

        // Instantiate paddles and ball - texture will be assigned in LoadContent
        _playerPaddle = new Paddle(null, new Vector2(50, _graphics.PreferredBackBufferHeight / 2 - 50), new Vector2(20, 100), 300f);
        _opponentPaddle = new Paddle(null, new Vector2(_graphics.PreferredBackBufferWidth - 70, _graphics.PreferredBackBufferHeight / 2 - 50), new Vector2(20, 100), 300f);
        _ball = new Ball(null, new Vector2(_graphics.PreferredBackBufferWidth / 2 - 10, _graphics.PreferredBackBufferHeight / 2 - 10), new Vector2(20, 20), new Vector2(200f, 200f));

        _gameState = GameState.PreServe;
        ResetBall(); // Initialize ball position and velocity, sets state to PreServe
        _previousKeyboardState = Keyboard.GetState();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        _playerPaddle.Texture = _pixelTexture;
        _opponentPaddle.Texture = _pixelTexture;
        _ball.Texture = _pixelTexture;
        _scoreFont = Content.Load<SpriteFont>("ScoreFont");
    }

    private void ResetBall(bool initialServe = false)
    {
        _ball.Position = new Vector2(
            _graphics.PreferredBackBufferWidth / 2 - _ball.Size.X / 2,
            _graphics.PreferredBackBufferHeight / 2 - _ball.Size.Y / 2
        );

        // Only set velocity if it's the initial serve from Playing state,
        // otherwise, wait for space press in PreServe
        if (initialServe)
        {
            float speedX = 200f;
            float speedY = 200f;
            if (_random.Next(2) == 0) speedX *= -1;
            if (_random.Next(2) == 0) speedY *= -1;
            _ball.Velocity = new Vector2(speedX, speedY);
        }
        else
        {
            _ball.Velocity = Vector2.Zero; // Ball is stationary during PreServe
        }
        _gameState = GameState.PreServe;
    }

    private void CheckForWin()
    {
        if (_playerScore >= 5) // Winning score
        {
            _gameState = GameState.GameOver;
            _winnerMessage = "Player Wins!";
        }
        else if (_opponentScore >= 5)
        {
            _gameState = GameState.GameOver;
            _winnerMessage = "Opponent Wins!";
        }
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        var kState = Keyboard.GetState();

        switch (_gameState)
        {
            case GameState.PreServe:
                if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    _gameState = GameState.Playing;
                    // Set initial ball velocity
                    float speedX = 200f; float speedY = 200f;
                    if (_random.Next(2) == 0) speedX *= -1;
                    if (_random.Next(2) == 0) speedY *= -1;
                    _ball.Velocity = new Vector2(speedX, speedY);
                }
                break;

            case GameState.Playing:
                // Player Paddle Control
                if (kState.IsKeyDown(Keys.Up))
                {
                    _playerPaddle.Position -= new Vector2(0, _playerPaddle.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                if (kState.IsKeyDown(Keys.Down))
                {
                    _playerPaddle.Position += new Vector2(0, _playerPaddle.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
                _playerPaddle.Position = new Vector2(_playerPaddle.Position.X, MathHelper.Clamp(_playerPaddle.Position.Y, 0, _graphics.PreferredBackBufferHeight - _playerPaddle.Size.Y));

                _ball.Update(gameTime);

                // Ball Bouncing off Top and Bottom Walls
                if (_ball.Position.Y < 0)
                {
                    _ball.Velocity = new Vector2(_ball.Velocity.X, -_ball.Velocity.Y);
                    _ball.Position = new Vector2(_ball.Position.X, 0);
                }
                else if (_ball.Position.Y + _ball.Size.Y > _graphics.PreferredBackBufferHeight)
                {
                    _ball.Velocity = new Vector2(_ball.Velocity.X, -_ball.Velocity.Y);
                    _ball.Position = new Vector2(_ball.Position.X, _graphics.PreferredBackBufferHeight - _ball.Size.Y);
                }

                // Scoring Logic
                if (_ball.Position.X < 0)
                {
                    _opponentScore++;
                    CheckForWin();
                    if (_gameState != GameState.GameOver) ResetBall();
                }
                else if (_ball.Position.X + _ball.Size.X > _graphics.PreferredBackBufferWidth)
                {
                    _playerScore++;
                    CheckForWin();
                    if (_gameState != GameState.GameOver) ResetBall();
                }

                // Ball Bouncing off Paddles
                var ballRect = new Rectangle((int)_ball.Position.X, (int)_ball.Position.Y, (int)_ball.Size.X, (int)_ball.Size.Y);
                var playerPaddleRect = new Rectangle((int)_playerPaddle.Position.X, (int)_playerPaddle.Position.Y, (int)_playerPaddle.Size.X, (int)_playerPaddle.Size.Y);
                var opponentPaddleRect = new Rectangle((int)_opponentPaddle.Position.X, (int)_opponentPaddle.Position.Y, (int)_opponentPaddle.Size.X, (int)_opponentPaddle.Size.Y);

                if (ballRect.Intersects(playerPaddleRect))
                {
                    _ball.Velocity = new Vector2(-_ball.Velocity.X, _ball.Velocity.Y);
                    _ball.Position = new Vector2(_playerPaddle.Position.X + _playerPaddle.Size.X, _ball.Position.Y);
                    _ball.Velocity *= 1.05f;
                }
                else if (ballRect.Intersects(opponentPaddleRect))
                {
                    _ball.Velocity = new Vector2(-_ball.Velocity.X, _ball.Velocity.Y);
                    _ball.Position = new Vector2(_opponentPaddle.Position.X - _ball.Size.X, _ball.Position.Y);
                    _ball.Velocity *= 1.05f;
                }

                // Opponent AI Logic
                float opponentPaddleCenterY = _opponentPaddle.Position.Y + _opponentPaddle.Size.Y / 2;
                float ballCenterY = _ball.Position.Y + _ball.Size.Y / 2;
                float paddleSpeed = _opponentPaddle.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (ballCenterY < opponentPaddleCenterY - _opponentPaddle.Size.Y * 0.1f)
                {
                    _opponentPaddle.Position -= new Vector2(0, paddleSpeed);
                }
                else if (ballCenterY > opponentPaddleCenterY + _opponentPaddle.Size.Y * 0.1f)
                {
                    _opponentPaddle.Position += new Vector2(0, paddleSpeed);
                }
                _opponentPaddle.Position = new Vector2(_opponentPaddle.Position.X, MathHelper.Clamp(_opponentPaddle.Position.Y, 0, _graphics.PreferredBackBufferHeight - _opponentPaddle.Size.Y));
                break;

            case GameState.GameOver:
                if (kState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    _playerScore = 0;
                    _opponentScore = 0;
                    _winnerMessage = "";
                    ResetBall(); // This will set state to PreServe
                }
                break;
        }

        _previousKeyboardState = kState;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _playerPaddle.Draw(_spriteBatch);
        _opponentPaddle.Draw(_spriteBatch);
        _ball.Draw(_spriteBatch);

        // Draw Scores
        _spriteBatch.DrawString(_scoreFont, _playerScore.ToString(), new Vector2(50, 20), Color.White);
        _spriteBatch.DrawString(_scoreFont, _opponentScore.ToString(), new Vector2(_graphics.PreferredBackBufferWidth - 100, 20), Color.White);

        // Conditional Drawing based on _gameState
        if (_gameState == GameState.PreServe)
        {
            var serveText = "Press Space to Serve";
            var textSize = _scoreFont.MeasureString(serveText);
            _spriteBatch.DrawString(_scoreFont, serveText, new Vector2(_graphics.PreferredBackBufferWidth / 2 - textSize.X / 2, _graphics.PreferredBackBufferHeight / 2 - textSize.Y / 2), Color.Yellow);
        }
        else if (_gameState == GameState.GameOver)
        {
            var gameOverTextSize = _scoreFont.MeasureString(_winnerMessage);
            _spriteBatch.DrawString(_scoreFont, _winnerMessage, new Vector2(_graphics.PreferredBackBufferWidth / 2 - gameOverTextSize.X / 2, _graphics.PreferredBackBufferHeight / 2 - gameOverTextSize.Y / 2 - 30), Color.Yellow);
            var restartText = "Press Space to Restart";
            var restartTextSize = _scoreFont.MeasureString(restartText);
            _spriteBatch.DrawString(_scoreFont, restartText, new Vector2(_graphics.PreferredBackBufferWidth / 2 - restartTextSize.X / 2, _graphics.PreferredBackBufferHeight / 2 - restartTextSize.Y / 2 + 30), Color.Yellow);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
