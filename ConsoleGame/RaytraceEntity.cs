// File: RaytraceRenderer.cs
using ConsoleGame.Components;
using ConsoleGame.Entities;
using ConsoleGame.Renderer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRayTracing
{
    public partial class RaytraceEntity : BaseComponent
    {
        private readonly Framebuffer fb;
        private readonly RaytraceRenderer renderer;
        private Vec3 camPos;
        private float yaw;
        private float pitch;
        private float lastDeltaTime = 1.0f / 60.0f;

        public RaytraceEntity(BaseEntity entity, Framebuffer framebuffer, Scene scene, float fovDeg, int pxW, int pxH, int superSample)
        {
            Parent = entity;
            this.fb = framebuffer;
            this.camPos = new Vec3(0.0, 1.0, 0.0);
            this.yaw = 0.0f;
            this.pitch = 0.0f;
            Parent.X = 0;
            Parent.Y = 0;
            Parent.Chexel = new Chexel(' ', ConsoleColor.Black, ConsoleColor.Black);

            renderer = new RaytraceRenderer(framebuffer, scene, fovDeg, pxW, pxH, superSample);
            renderer.SetCamera(camPos, yaw, pitch);
        }

        public override void HandleInput(ConsoleKeyInfo keyInfo)
        {
            float moveSpeed = 3.0f;
            float rotSpeed = 1.8f;

            float dt = lastDeltaTime;
            if (dt < 0.0) dt = 0.0f;

            if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                yaw -= rotSpeed * dt;
            }

            if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                yaw += rotSpeed * dt;
            }

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                pitch += rotSpeed * dt;
            }

            if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                pitch -= rotSpeed * dt;
            }

            float limit = (MathF.PI * 0.5f) - 0.01f;
            if (pitch > limit)
            {
                pitch = limit;
            }
            if (pitch < -limit)
            {
                pitch = -limit;
            }

            float cy = MathF.Cos(yaw);
            float sy = MathF.Sin(yaw);
            Vec3 forwardXZ = new Vec3(sy, 0.0, -cy);
            Vec3 rightXZ = new Vec3(cy, 0.0, sy);
            Vec3 up = new Vec3(0.0, 1.0, 0.0);

            if (keyInfo.Key == ConsoleKey.W)
            {
                camPos = camPos + forwardXZ * (moveSpeed * dt);
            }

            if (keyInfo.Key == ConsoleKey.S)
            {
                camPos = camPos - forwardXZ * (moveSpeed * dt);
            }

            if (keyInfo.Key == ConsoleKey.A)
            {
                camPos = camPos - rightXZ * (moveSpeed * dt);
            }

            if (keyInfo.Key == ConsoleKey.D)
            {
                camPos = camPos + rightXZ * (moveSpeed * dt);
            }

            if (keyInfo.Key == ConsoleKey.E)
            {
                camPos = camPos + up * (moveSpeed * dt);
            }

            if (keyInfo.Key == ConsoleKey.Q)
            {
                camPos = camPos - up * (moveSpeed * dt);
            }

            renderer.SetCamera(camPos, yaw, pitch);
        }

        public override void Update(double deltaTime)
        {
            lastDeltaTime = (float)deltaTime;
            renderer.SetCamera(camPos, yaw, pitch);
            renderer.TryFlipAndBlit(fb);
        }
    }
}
