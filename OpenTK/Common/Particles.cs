using System;

//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
/*
namespace Marler.OpenTK.Common
{
    public class Particle
    {
        Texture2D texture;
        int width, height;

        Vector2 position;
        Vector2 velocity;

        float angle;
        float angularVelocity;

        Color color;
        float size;

        public int ttl;

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity, float angle, float angularVelocity,
            Color color, float size, int ttl)
        {
            this.texture = texture;
            this.width = texture.Width;
            this.height = texture.Height;

            this.position = position;
            this.velocity = velocity;
            this.angle = angle;
            this.angularVelocity = angularVelocity;
            this.color = color;
            this.size = size;
            this.ttl = ttl;
        }

        public void Update()
        {
            ttl--;
            position += velocity;
            angle += angularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, width, height);
            Vector2 origin = new Vector2(width / 2, height / 2);

            spriteBatch.Draw(texture, position, sourceRectangle, color,
                angle, origin, size, SpriteEffects.None, 0f);
        }
    }

    public class ParticleEngine
    {
        Random random;
        List<Texture2D> textures;
        List<Particle> particles;

        public ParticleEngine(Random random, List<Texture2D> textures)
        {
            this.random = random;
            this.textures = textures;
            this.particles = new List<Particle>();
        }


        public void Update()
        {
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].ttl <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public void Generate(Vector2 location, Int32 count)
        {
            for (int i = 0; i < count; i++)
            {
                particles.Add(GenerateNewParticle(location));
            }
        }
        private Particle GenerateNewParticle(Vector2 location)
        {
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 velocity = new Vector2(
                                    1f * (float)(random.NextDouble() * 2 - 1),
                                    1f * (float)(random.NextDouble() * 2 - 1));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);
            Color color = new Color(
                        (float)random.NextDouble(),
                        (float)random.NextDouble(),
                        (float)random.NextDouble());
            float size = (float)random.NextDouble();
            int ttl = 20 + random.Next(40);

            return new Particle(texture, location, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
        }
    }
}
*/