﻿#if SFML
using Point = SFML.System.Vector2i;
using Vector2 = SFML.System.Vector2f;
using SFML.System;
using Matrix = SFML.Graphics.Transform;
using SFML.Graphics;
#else
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif

using System;
using System.Runtime.Serialization;

namespace SadConsole.Consoles
{
    /// <summary>
    /// Draws a text surface to the screen.
    /// </summary>
    [DataContract]
    public class TextSurfaceRenderer : ITextSurfaceRenderer
    {
        //private Matrix absolutePositionTransform;
        //private Matrix positionTransform;
        //private Point oldPosition;
        //private Point oldAbsolutePosition;

        /// <summary>
        /// The sprite batch used for drawing to the screen.
        /// </summary>
        public SpriteBatch Batch { get; private set; }

        /// <summary>
        /// A method called when the <see cref="SpriteBatch"/> has been created and transformed, but before any text characters are drawn.
        /// </summary>
        public Action<SpriteBatch> BeforeRenderCallback { get; set; }

        /// <summary>
        /// A method called when all text characters have been drawn and any tinting has been applied.
        /// </summary>
        public Action<SpriteBatch> AfterRenderCallback { get; set; }

        /// <summary>
        /// Creates a new renderer.
        /// </summary>
        public TextSurfaceRenderer()
        {
#if MONOGAME
            Batch = new SpriteBatch(Engine.Device);
#else
            Batch = new SpriteBatch();
#endif
        }


        /// <summary>
        /// Renders a surface to the screen.
        /// </summary>
        /// <param name="surface">The surface to render.</param>
        /// <param name="renderingMatrix">Display matrix for the rendered console.</param>
        public virtual void Render(ITextSurfaceRendered surface, Matrix renderingMatrix)
        {
#if MONOGAME
            Batch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, null, renderingMatrix);

            BeforeRenderCallback?.Invoke(Batch);

            if (surface.Tint.A != 255)
            {
                Cell cell;


                if (surface.DefaultBackground.A != 0)
                    Batch.Draw(surface.Font.FontImage, surface.AbsoluteArea, surface.Font.GlyphIndexRects[surface.Font.SolidGlyphIndex], surface.DefaultBackground, 0f, PrimitiveStatic.Vector2Zero, SpriteEffects.None, 0.2f);

                for (int i = 0; i < surface.RenderCells.Length; i++)
                {
                    cell = surface.RenderCells[i];

                    if (cell.IsVisible)
                    {
                        if (cell.ActualBackground != Color.Transparent && cell.ActualBackground != surface.DefaultBackground)
                            Batch.Draw(surface.Font.FontImage, surface.RenderRects[i], surface.Font.GlyphIndexRects[surface.Font.SolidGlyphIndex], cell.ActualBackground, 0f, Vector2.Zero, SpriteEffects.None, 0.3f);

                        if (cell.ActualForeground != Color.Transparent)
                            Batch.Draw(surface.Font.FontImage, surface.RenderRects[i], surface.Font.GlyphIndexRects[cell.ActualGlyphIndex], cell.ActualForeground, 0f, Vector2.Zero, cell.ActualSpriteEffect, 0.4f);
                    }
                }

                if (surface.Tint.A != 0)
                    Batch.Draw(surface.Font.FontImage, surface.AbsoluteArea, surface.Font.GlyphIndexRects[surface.Font.SolidGlyphIndex], surface.Tint, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }
            else
            {
                Batch.Draw(surface.Font.FontImage, surface.AbsoluteArea, surface.Font.GlyphIndexRects[surface.Font.SolidGlyphIndex], surface.Tint, 0f, Vector2.Zero, SpriteEffects.None, 0.5f);
            }

            AfterRenderCallback?.Invoke(Batch);

            Batch.End();
#elif SFML
            Batch.Start(surface, renderingMatrix);

            BeforeRenderCallback?.Invoke(Batch);

            if (surface.Tint.A != 255)
            {
                Cell cell;

                if (surface.DefaultBackground.A != 0)
                {
                    Batch.DrawSurfaceFill(surface.DefaultBackground, Color.Transparent);

                    for (int i = 0; i < surface.RenderCells.Length; i++)
                    {
                        cell = surface.RenderCells[i];

                        if (cell.IsVisible)
                        {
                            Batch.DrawCell(cell, surface.RenderRects[i], surface.DefaultBackground, surface.Font);
                        }
                    }

                    if (surface.Tint.A != 0)
                        Batch.DrawSurfaceFill(surface.Tint, Color.Transparent);
                }
                else
                    if (surface.Tint.A != 0)
                    Batch.DrawSurfaceFill(surface.Tint, Color.Transparent);

            }
            AfterRenderCallback?.Invoke(Batch);
            Batch.End(Engine.Device, RenderStates.Default);
#endif
        }

        /// <summary>
        /// Renders a surface to the screen.
        /// </summary>
        /// <param name="surface">The surface to render.</param>
        /// <param name="position">Calculates the rendering position on the screen based on the size of the <paramref name="surface"/> parameter.</param>
        /// <param name="usePixelPositioning">Ignores the <paramref name="surface"/> font for positioning and instead treats the <paramref name="position"/> parameter in pixels.</param>
        public void Render(ITextSurfaceRendered surface, Point position, bool usePixelPositioning = false)
        {
            Matrix matrix;

            if (usePixelPositioning)
            {
                //if (oldAbsolutePosition != position)
                //{
                //    absolutePositionTransform = GetPositionTransform(position, surface.Font.Size, true);
                //    oldAbsolutePosition = position;
                //}

                //matrix = absolutePositionTransform;

                matrix = GetPositionTransform(position, surface.Font.Size, true);
            }
            else
            {
                //if (position != oldPosition)
                //{
                //    positionTransform = GetPositionTransform(position, surface.Font.Size, false);
                //    oldPosition = position;
                //}

                //matrix = positionTransform;

                matrix = GetPositionTransform(position, surface.Font.Size, false);
            }

            Render(surface, matrix);
        }

        /// <summary>
        /// Gets the Matrix transform that positions the console on the screen.
        /// </summary>
        /// <returns>The transform.</returns>
        public virtual Matrix GetPositionTransform(Point position, Point CellSize, bool absolutePositioning)
        {
            Point worldLocation;

            if (absolutePositioning)
                worldLocation = position;
            else
                worldLocation = position.ConsoleLocationToWorld(CellSize.X, CellSize.Y);

#if SFML
            var transform = Matrix.Identity;
            transform.Translate(worldLocation.X, worldLocation.Y);
            
            return transform;
#elif MONOGAME
            return Matrix.CreateTranslation(worldLocation.X, worldLocation.Y, 0f);
#endif
        }

        [OnDeserialized]
        private void AfterDeserialized(StreamingContext context)
        {
#if SFML
            Batch = new SpriteBatch();
#elif MONOGAME
            Batch = new SpriteBatch(Engine.Device);
#endif
        }
    }
}