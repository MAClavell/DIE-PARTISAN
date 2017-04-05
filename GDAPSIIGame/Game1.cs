﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using GDAPSIIGame.Map;
using System.Threading;

namespace GDAPSIIGame
{
    enum GameState { Menu, NewGame, LoadingScreen, GamePlay, GameOver, PauseMenu}
    
    public class Game1 : Game
    {
        //Fields
        EntityManager entityManager;
        ProjectileManager projectileManager;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState kbState;
		KeyboardState previousKbState;
		ChunkManager chunkManager;
        MapManager mapManager;
        UIManager uiManager;
        Texture2D theTexture;
        Camera mainCamera;
		GameState gameState;
		Texture2D mouseTex;
        Texture2D wallTexture;
        Vector2 mousePos;
		MouseState mState;
		Vector2 mouseScale;

		public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
			this.IsMouseVisible = false;
            //this.graphics.IsFullScreen = true;
            //graphics.ToggleFullScreen();
			
            //Initialize entity manager
            entityManager = EntityManager.Instance;

            //Initialize the chunk manager
            chunkManager = ChunkManager.Instance;

			//Initialize projectile manager
			projectileManager = ProjectileManager.Instance;

			//Initialize map manager
			mapManager = new MapManager();

            //Initialize ui manager
            uiManager = new UIManager();

			//Initialize keyboards
			kbState = new KeyboardState();
			previousKbState = kbState;

			gameState = GameState.Menu;
			base.Initialize();
        }

        protected override void LoadContent()
        {
			spriteBatch = new SpriteBatch(GraphicsDevice);
			//Load entities
			entityManager.LoadContent(Content);
            //Make the Camera
            Camera.Instance.setPosition(GraphicsDevice.Viewport);
            //Load projectiles
            projectileManager.LoadContent(Content);
            //Load UI Assets
            uiManager.LoadContent(Content);
			//Load the one and only texture
			theTexture = Content.Load<Texture2D>("playernew");
			//Initiate mouse
			mState = Mouse.GetState();
			mouseTex = Content.Load<Texture2D>("playernew");
			mousePos = new Vector2(mState.X, mState.Y);
			mouseScale = new Vector2((float)16 / mouseTex.Width, (float)16 / mouseTex.Height);
            //Grab different wall texture
            wallTexture = Content.Load<Texture2D>("playerBullet");
            //Init Map
            mapManager.initMap(theTexture, wallTexture);
        }

		protected override void UnloadContent()
        {

        }

		protected override void Update(GameTime gameTime)
		{
            base.Update(gameTime);

			//Update mouse texture's position
			mState = Mouse.GetState();
			mousePos = mState.Position.ToVector2();

			switch (gameState)
            {
				//The menu of the game
				case GameState.Menu:
					kbState = Keyboard.GetState();
					if (kbState.IsKeyDown(Keys.Enter))
					{
						gameState = GameState.NewGame;
					}
					break;

				//When the player starts a new game
				case GameState.NewGame:
					//Reset the player character
					Player.Instance.ResetPlayer();

					//Go to loading screen to create a new level
					gameState = GameState.LoadingScreen;
					break;

				//Loading screen that generates a new level
				case GameState.LoadingScreen:
					//Empty all entities and walls
					entityManager.RemoveEnemies();
					chunkManager.DeleteWalls();

					//Create the new map
					mapManager = new MapManager();
					mapManager.initMap(theTexture, wallTexture);

					//Go to gameplay
					gameState = GameState.GamePlay;
					break;

				//Player playing a level
				case GameState.GamePlay:
					if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
					{
						Exit();
					}

					previousKbState = kbState;
					kbState = Keyboard.GetState();

					//Check if the player has paused the game
					if (kbState.IsKeyDown(Keys.Enter) && !previousKbState.IsKeyDown(Keys.Enter))
					{
						gameState = GameState.PauseMenu;
					}

					//Update entities
					entityManager.Update(gameTime);

					//Update projectiles
					projectileManager.Update(gameTime, previousKbState, kbState);

					//Update chunks
					chunkManager.Update();

					//Update UI
					uiManager.Update(gameTime);

					//Initialize Camera
					if (mainCamera == null)
					{
						mainCamera = Camera.Instance;
					}

					//Check if the player has beat the levels
					if (entityManager.BeatLevel)
					{
						gameState = GameState.LoadingScreen;
					}

					//Check if the player has died
					if (Player.Instance.Health <= 0)
					{
						gameState = GameState.GameOver;
					}

					break;

				//When the game is paused
				case GameState.PauseMenu:
					previousKbState = kbState;
					kbState = Keyboard.GetState();
					if (kbState.IsKeyDown(Keys.Enter) && !previousKbState.IsKeyDown(Keys.Enter))
					{
						gameState = GameState.GamePlay;
					}
					break;

				//When the Player dies
				case GameState.GameOver:
					gameState = GameState.Menu;
					break;
			}
		}

		protected override void Draw(GameTime gameTime)
        {
			GraphicsDevice.Clear(Color.CornflowerBlue);

			//Begin SpriteBatch
			spriteBatch.Begin();

			switch (gameState)
			{	
				//Drawing for main menu
				case GameState.Menu:
					//Draw the menu
					spriteBatch.Draw(theTexture, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);

					//Draw the mouse texture
					spriteBatch.Draw(mouseTex,
							mousePos,
							null,
							null,
							Vector2.Zero,
							0.0f,
							mouseScale,
							null,
							0);
					break;
				
				//Drawing for loading screen
				case GameState.LoadingScreen:
					break;

				//Drawing for gameplay
				case GameState.GamePlay:
					//Draw Map
					mapManager.Draw(spriteBatch, theTexture, wallTexture);

					//Draw entities
					entityManager.Draw(gameTime, spriteBatch);

					//Draw projectiles
					projectileManager.Draw(gameTime, spriteBatch);

					//Draw UI
					uiManager.Draw(gameTime, spriteBatch);

					//Draw the mouse texture
					spriteBatch.Draw(mouseTex,
							mousePos,
							null,
							null,
							Vector2.Zero,
							0.0f,
							mouseScale,
							null,
							0);

					break;

				//Drawing for pause menu
				case GameState.PauseMenu:
					//Draw Map
					mapManager.Draw(spriteBatch, theTexture, wallTexture);

					//Draw entities
					entityManager.Draw(gameTime, spriteBatch);

					//Draw projectiles
					projectileManager.Draw(gameTime, spriteBatch);

					//Draw UI
					uiManager.Draw(gameTime, spriteBatch);

					//Draw the mouse texture
					spriteBatch.Draw(mouseTex,
							mousePos,
							null,
							null,
							Vector2.Zero,
							0.0f,
							mouseScale,
							null,
							0);
					break;

				//Drawing for game over
				case GameState.GameOver:
					//Draw the mouse texture
					spriteBatch.Draw(mouseTex,
							mousePos,
							null,
							null,
							Vector2.Zero,
							0.0f,
							mouseScale,
							null,
							0);
					break;

			}

			//End SpriteBatch
			spriteBatch.End();

			base.Draw(gameTime);
        }
    }
}
