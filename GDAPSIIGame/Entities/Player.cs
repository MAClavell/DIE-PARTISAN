﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GDAPSIIGame.Entities;
using System.Threading;
using System.Runtime.CompilerServices;
using GDAPSIIGame.Map;

namespace GDAPSIIGame
{
	class Player : Entity
	{
		//Fields
		static private Player instance;
		private Weapon weapon;
		private MouseState mouseState;
		private MouseState prevMouseState;
		private GameTime currentTime;
		private KeyboardState keyState;
		private KeyboardState prevKeyState;
		private float hurting;
        private Color color;
		private float angle;
		private SpriteEffects effect;
		private float timeMult;

		//Singleton

		private Player(Weapon weapon, int health, int moveSpeed, Texture2D texture, Vector2 position, Rectangle boundingBox) : base(health, moveSpeed, texture, position, boundingBox)
		{
			this.weapon = weapon;
			mouseState = Mouse.GetState();
			prevMouseState = Mouse.GetState();
			keyState = Keyboard.GetState();
			prevKeyState = Keyboard.GetState();
			hurting = 0;
            color = Color.White;
			angle = 0;
			effect = new SpriteEffects();
			timeMult = 0;
		}

		static public Player Instantiate(Weapon weapon, int health, int moveSpeed, Texture2D texture, Vector2 position, Rectangle boundingBox)
		{
			if (instance == null)
			{
				instance = new Player(weapon, health, moveSpeed, texture, position, boundingBox);
			}
			return instance;
		}

		//Properties

		/// <summary>
		/// The only instance of the player class
		/// </summary>
		static public Player Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// The weapon the player is holding
		/// </summary>
		public Weapon Weapon
		{
			get { return weapon; }
			set { weapon = value; }
		}

        /// <summary>
        /// Whether the player is hurting or not
        /// </summary>
        public bool IsHurting
        {
            get { return hurting > 0; }
            set { if (value) { hurting = 0.5f; } }
        }

		//Methods
		public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

			//Mouse state
			prevMouseState = mouseState;
			mouseState = Mouse.GetState();

			//Current keyboard state
			prevKeyState = keyState;
			keyState = Keyboard.GetState();

			//Update player movement
			UpdateInput(gameTime, keyState, prevKeyState);

            //Update the weapons rotation
            weapon.Angle = -((float)Math.Atan2(mouseState.X - Weapon.Position.X, mouseState.Y - Weapon.Position.Y));
			//Update weapon position
			weapon.X = this.X + (BoundingBox.Width / 2);
			weapon.Y = this.Y + (BoundingBox.Height / 2);
			//Update weapon
			weapon.Update(gameTime);

			//Fire weapon only if previous frame didn't have left button being pressed
			if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
			{
				Vector2 direction = new Vector2((mouseState.X - Weapon.X) / 1, (mouseState.Y - Weapon.Y) / 1);
				direction.Normalize();
				this.Weapon.Fire(direction);
			}

            //Determine if the player hurting color should be playing
            if (hurting > 0)
            {
                //Subtract from the hurting timer if the player is hurting
                hurting -= (float)gameTime.ElapsedGameTime.TotalSeconds;
			}
        }

		public override void Draw(SpriteBatch spriteBatch)
        {
			//Find the right sprite to draw
			//Determine player direction and get the corresponding sprite
			switch (this.Dir)
			{
				case Entity_Dir.Up:
					effect = SpriteEffects.None;
					break;
				case Entity_Dir.UpLeft:
					effect = SpriteEffects.FlipHorizontally;
					break;
				case Entity_Dir.Left:
					effect = SpriteEffects.FlipHorizontally;
					break;
				case Entity_Dir.DownLeft:
					effect = SpriteEffects.FlipHorizontally;
					break;
				case Entity_Dir.Down:
					effect = SpriteEffects.None;
					break;
				case Entity_Dir.DownRight:
					effect = SpriteEffects.None;
					break;
				case Entity_Dir.Right:
					effect = SpriteEffects.None;
					break;
				case Entity_Dir.UpRight:
					effect = SpriteEffects.None;
					break;
			}

			spriteBatch.Draw(this.Texture,
				this.Position,
				null,
				null,
				Vector2.Zero,
				0.0f,
				this.Scale,
				color,
				effect);

			weapon.Draw(spriteBatch);
        }

        /// <summary>
        /// Parses Input during updates
        /// </summary>
        /// <param name="keyState">KeyboardState</param>
        public void UpdateInput(GameTime gameTime, KeyboardState keyState, KeyboardState prevKeyState)
        {
            timeMult = (float)gameTime.ElapsedGameTime.TotalSeconds / ((float)1/60);

			//Basic keyboard movement
			if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up))
			{
				this.Y -= this.MoveSpeed * timeMult;
			}
			else if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down))
			{
				this.Y += this.MoveSpeed * timeMult;
			}

			if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right))
			{
				this.X += this.MoveSpeed * timeMult;
			}
			else if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left))
			{
				this.X -= this.MoveSpeed * timeMult;
			}

			//Player reloading
			if (keyState.IsKeyDown(Keys.R) && prevKeyState.IsKeyUp(Keys.R))
			{
				this.weapon.Reload();
			}

			//Calculates the angle between the player and the mouse
			//See below
			//   180
			//-90   90
			//    0
			angle = MathHelper.ToDegrees((float)Math.Atan2(mouseState.X - Position.X, mouseState.Y - Position.Y));

			//Use angle to find player direction
			if ((angle < -157.5) || (angle > 157.5) && this.Dir != Entity_Dir.Up)
			{
				this.Dir = Entity_Dir.Up;
				weapon.Dir = Weapon_Dir.Up;
			}
			else if ((angle < 157.5) && (angle > 112.5) && this.Dir != Entity_Dir.UpRight)
			{
				this.Dir = Entity_Dir.UpRight;
				weapon.Dir = Weapon_Dir.UpRight;
			}
			else if ((angle < 112.5) && (angle > 67.5) && this.Dir != Entity_Dir.Right)
			{
				this.Dir = Entity_Dir.Right;
				weapon.Dir = Weapon_Dir.Right;
			}
			else if ((angle < 67.5) && (angle > 22.5) && this.Dir != Entity_Dir.DownRight)
			{
				this.Dir = Entity_Dir.DownRight;
				weapon.Dir = Weapon_Dir.DownRight;
			}
			else if ((angle < -22.5) && (angle > -67.5) && this.Dir != Entity_Dir.DownLeft)
			{
				this.Dir = Entity_Dir.DownLeft;
				weapon.Dir = Weapon_Dir.DownLeft;
			}
			else if ((angle < -67.5) && (angle > -112.5) && this.Dir != Entity_Dir.Left)
			{
				this.Dir = Entity_Dir.Left;
				weapon.Dir = Weapon_Dir.Left;
			}
			else if ((angle < -112.5) && (angle > -157.5) && this.Dir != Entity_Dir.UpLeft)
			{
				this.Dir = Entity_Dir.UpLeft;
				weapon.Dir = Weapon_Dir.UpLeft;
			}
			else if ((angle < 22.5) && (angle > -22.5) && this.Dir != Entity_Dir.Down)
			{
				this.Dir = Entity_Dir.Down;
				weapon.Dir = Weapon_Dir.Down;
			}
        }
    }
}
