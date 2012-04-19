using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Windows7.Multitouch;
using Windows7.Multitouch.WinForms;
using System.Windows.Forms;
using System.Drawing;

namespace RDSim
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D[] fieldBuffers;
        int currentBufferIdx = 0;
        Texture2D dummyTex;
        Effect rdFX;
        float coef_F = 0.027f;
        float coef_k = 0.054f;

		TouchHandler tHandler = null;
		Form gameWindowForm;

		class TouchPoint {
			public int  x;
			public int  y;
			public uint id;
		}
        int lose = 0;
		List<TouchPoint> touches = new List<TouchPoint>();

		bool rdDemo = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            IsMouseVisible = false;
        }



        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            this.IsFixedTimeStep = false;

            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;
            fieldBuffers = new RenderTarget2D[2];
            for (int i = 0; i < 2; ++i)
            {
                fieldBuffers[i] = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Vector2, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
            }

            Random rand = new Random();

            /*Vector2[] arr = new Vector2[w * h];
            for (int i = 0; i < w*h; ++i)
            {
                arr[i] = new Vector2(1, 0);
            }
            for (int i = 0; i < 100; ++i)
            {
                int x1 = (int)(rand.NextDouble() * w);
                int y1 = (int)(rand.NextDouble() * h);
                int x2 = Math.Min(x1+32, w), y2 = Math.Min(y1+32, h);
                for (int y = y1; y < y2; ++y)
                    for (int x = x1; x < x2; ++x)
                        arr[x + y * w].Y = (float)rand.NextDouble();

                //for 
            } */
            //fieldBuffers[currentBufferIdx].SetData(arr);

            dummyTex = new Texture2D(GraphicsDevice, 1, 1);
            base.Initialize();
        }


        Texture2D asteroid;

        protected override void LoadContent()
        {

            spriteBatch = new SpriteBatch(GraphicsDevice);

            int w = fieldBuffers[0].Width;
            int h = fieldBuffers[0].Height;
            rdFX = Content.Load<Effect>("RDEffect");
            rdFX.Parameters["dp"].SetValue(new Vector2(1.0f / w, 1.0f / h));

            asteroid = Content.Load<Texture2D>("a");
            
			gameWindowForm = (Form)Form.FromHandle(this.Window.Handle);


			if (!Windows7.Multitouch.TouchHandler.DigitizerCapabilities.IsMultiTouchReady) {
				Console.WriteLine("Multitouch is not availible");
			}
			else {
				tHandler = Factory.CreateHandler<TouchHandler>(gameWindowForm);
                tHandler.TouchDown += (s, e) => { TouchDown((uint)e.Id, e.Location); };
                tHandler.TouchMove += (s, e) => { TouchMove((uint)e.Id, e.Location); };
                tHandler.TouchUp   += (s, e) => { TouchUp((uint)e.Id, e.Location); };
            }

        }


        List<Asteroid> asteroids = new List<Asteroid>();

		void TouchDown(uint mid, System.Drawing.Point p)
		{
			var tp = new TouchPoint() { id=mid, x = p.X, y = p.Y };
               touches.Add(tp);
            //foreach(var a in asteroids)
            //{
            //if(new Vector2(a.pos.X-tp.x,a.pos.X-tp.x).Length()<10)
            //{
            //    asteroids.Remove(a); 
            //}
            //}
            var r = new System.Drawing.Rectangle(100, 100, 1920 - 200, 1080 - 200);
            if (!r.Contains(p))
            {
                asteroids.RemoveAll(a => (a.pos - new Vector2(p.X, p.Y)).Length() < 100);
            }
            else
            {         asteroids.Add(new Asteroid());
            int i = asteroids.Count - 1;
            asteroids[i].pos.X = p.X;
            asteroids[i].pos.Y = p.Y;
            asteroids[i].dir.X = (float)rand.NextDouble()*2-1;
            asteroids[i].dir.Y = (float)rand.NextDouble()*2-1;
            asteroids[i].self_angle = MathHelper.ToRadians((float)rand.NextDouble()*360);
            asteroids[i].omega = (float)rand.NextDouble() * 2f - 1f;

            }

        }
		void TouchMove(uint mid, System.Drawing.Point p)
		{
			touches.Single( tp => tp.id == mid ).x = p.X;
			touches.Single( tp => tp.id == mid ).y = p.Y;
		}

		void TouchUp(uint mid, System.Drawing.Point p)
		{
			touches.RemoveAll( tp => tp.id == mid );
		}



        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }



		void DrawSpot (int x, int y)
		{
			GraphicsDevice.SetRenderTarget(fieldBuffers[currentBufferIdx]);
			rdFX.CurrentTechnique = rdFX.Techniques["Spot"];
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, rdFX);
			spriteBatch.Draw(dummyTex, new Microsoft.Xna.Framework.Rectangle(x - 10, y - 10, 20, 20), Microsoft.Xna.Framework.Color.Black);
			spriteBatch.End();
			GraphicsDevice.SetRenderTarget(null);
		}



        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            /*if (GamePad.GetState(PlayerIndex.One).Buttons.Back == Microsoft.Xna.Framework.Input.Buttons.Pressed)
                this.Exit();*/
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)) {
				this.Exit();
			}

			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F1)) {
				rdDemo = true;
			}
			if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.F2)) {
				rdDemo = false;
			}



            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                var x = mouseState.X;
                var y = mouseState.Y;
				//DrawSpot(x, y);
            }

			foreach (var tp in touches)	{
				DrawSpot( tp.x, tp.y );
			}

            foreach (var a in asteroids)
            {
                a.self_angle += a.omega * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            var kbState = Keyboard.GetState();
            float step = 0.01f * (float)gameTime.ElapsedGameTime.TotalSeconds; 
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                coef_k -= step;
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                coef_k += step;
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                coef_F -= step;
            if (kbState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                coef_F += step;
            rdFX.Parameters["coef_F"].SetValue(coef_F);
            rdFX.Parameters["coef_k"].SetValue(coef_k);

            base.Update(gameTime);
        }


		float time = 0;

        Random rand = new Random();
         class Asteroid
         {
            public Vector2 dir;    
            public Vector2 pos;
            public float angle;
            public float self_angle;
            public float omega;
        }

       

        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.CornflowerBlue);
			time = (float)gameTime.TotalGameTime.TotalSeconds;
            
            int w = fieldBuffers[0].Width;
            int h = fieldBuffers[0].Height;

			if (rdDemo) {

				GraphicsDevice.SetRenderTarget(fieldBuffers[1 - currentBufferIdx]);
				rdFX.CurrentTechnique = rdFX.Techniques["UpdateField"];
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, rdFX);
				spriteBatch.Draw(fieldBuffers[currentBufferIdx], new Microsoft.Xna.Framework.Rectangle(0, 0, w, h), Microsoft.Xna.Framework.Color.Black);
				spriteBatch.End();
				GraphicsDevice.SetRenderTarget(null);
				currentBufferIdx = 1 - currentBufferIdx;

				rdFX.CurrentTechnique = rdFX.Techniques["Visualize"];
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, rdFX);
				spriteBatch.Draw(fieldBuffers[currentBufferIdx], new Microsoft.Xna.Framework.Rectangle(0, 0, w, h), Microsoft.Xna.Framework.Color.Black);
				spriteBatch.End();

			} else {

				rdFX.CurrentTechnique = rdFX.Techniques["Interference"];

				rdFX.Parameters[ "time"	].SetValue( time );
				rdFX.Parameters[ "xform"].SetValue( Matrix.CreateOrthographicOffCenter(0,w, h, 0, -100,100) );

	            var mouseState = Mouse.GetState();
				if (mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed) {
					//rdFX.Parameters[ "p0"	].SetValue( new Vector3(mouseState.X,mouseState.Y,1) );
				} else {
					rdFX.Parameters[ "p0"	].SetValue( new Vector3(0,0,0) );
				}

				for (int i=0; i<6; i++) {
					rdFX.Parameters[ string.Format("p{0}",i+1) ].SetValue( Vector3.Zero );
				}
				for (int i=0; i<Math.Min(touches.Count, 6); i++) {
					rdFX.Parameters[ string.Format("p{0}",i+1) ].SetValue( new Vector3( touches[i].x, touches[i].y, 1 ) );
				}

				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, rdFX);
                spriteBatch.Draw(fieldBuffers[currentBufferIdx], new Microsoft.Xna.Framework.Rectangle(0, 0, w, h), Microsoft.Xna.Framework.Color.Black);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
                foreach (var t in asteroids)
                {
                    t.pos += t.dir * (float)gameTime.ElapsedGameTime.TotalSeconds * 200;
                    //spriteBatch.Draw(asteroid, new Microsoft.Xna.Framework.Rectangle((int)t.pos.X - 30, (int)t.pos.Y - 20, 60, 40), Microsoft.Xna.Framework.Color.White);
                    spriteBatch.Draw(asteroid, new Vector2((int)t.pos.X - 30, (int)t.pos.Y - 20), null, Microsoft.Xna.Framework.Color.White, t.self_angle, new Vector2(0f, 0f), 0.5f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0f);
                }
                
                spriteBatch.End();


			}
            

            base.Draw(gameTime);
        }
    }
}
