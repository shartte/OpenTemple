using System;
using System.Drawing;
using SpicyTemple.Core.AAS;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Ui;

namespace SpicyTemple.Core.Systems
{
    public struct SectorList
    {
        public SectorLoc Sector;
        public locXY CornerTile; // tile coords
        public Size Extent; // relative to the above tile
    }

    internal struct RenderWorldInfo
    {
        public Rectangle Viewport;
        public TileRect Tiles;
        public SectorList[] Sectors;
    }

    public class GameRenderer : IDisposable
    {
        private readonly IAnimatedModel _model;
        private TimePoint _lastUpdate = TimePoint.Now;

        private readonly AasRenderer _aasRenderer;

        private readonly RenderingDevice mRenderingDevice;
        private readonly MapObjectRenderer mMapObjectRenderer;
        private readonly ParticleSystemsRenderer mParticleSysRenderer;
        private readonly GMeshRenderer mGmeshRenderer;
        private readonly LightningRenderer mLightningRenderer;
        private readonly FogOfWarRenderer mFogOfWarRenderer;
        private readonly IntgameRenderer mIntgameRenderer;
        private readonly SectorDebugRenderer _sectorDebugRenderer;

        public bool RenderSectorDebugInfo { get; set; }

        public ParticleSystemsRenderer GetParticleSysRenderer() => mParticleSysRenderer;

        public MapObjectRenderer GetMapObjectRenderer() => mMapObjectRenderer;

        private readonly GameView _gameView;

        private int _drawEnableCount = 1;

        public GameRenderer(RenderingDevice renderingDevice, GameView gameView)
        {
            mRenderingDevice = renderingDevice;
            _gameView = gameView;
            _aasRenderer = GameSystems.AAS.Renderer;

            mMapObjectRenderer = new MapObjectRenderer(renderingDevice, Tig.MdfFactory, _aasRenderer);
            _sectorDebugRenderer = new SectorDebugRenderer();
        }

        [TempleDllLocation(0x100027E0)]
        public void EnableDrawing()
        {
            _drawEnableCount++;
        }

        [TempleDllLocation(0x100027C0)]
        public void DisableDrawing()
        {
            _drawEnableCount--;
        }

        [TempleDllLocation(0x100027D0)]
        public void DisableDrawingForce()
        {
            _drawEnableCount = 0;
        }

        public void Render()
        {
            using var perfGroup = mRenderingDevice.CreatePerfGroup("Game Renderer");

            if (_drawEnableCount <= 0)
            {
                return;
            }

            var viewportSize = new Rectangle();
            viewportSize.Y = -256;
            viewportSize.Width = _gameView.Width + 512;
            viewportSize.X = -256;
            viewportSize.Height = _gameView.Height + 512;

            if (GameSystems.Location.GetVisibleTileRect(viewportSize, out var tiles))
            {
                RenderWorld(ref tiles);
            }
        }

        private void RenderWorld(ref TileRect tileRect)
        {
            if (mRenderingDevice.BeginFrame())
            {
                GameSystems.Terrain.Render();

                GameSystems.MapFogging.PerformFogChecks();

                GameSystems.Clipping.Render();

                mMapObjectRenderer.RenderMapObjects(
                    tileRect.x1, tileRect.x2,
                    tileRect.y1, tileRect.y2);

                // TODO mGmeshRenderer.Render();

                // TODO mLightningRenderer.Render();

                // TODO mParticleSysRenderer.Render();

                // TODO mFogOfWarRenderer.Render();

                mMapObjectRenderer.RenderOccludedMapObjects(
                    tileRect.x1, tileRect.x2,
                    tileRect.y1, tileRect.y2);

                using (var uiPerfGroup = mRenderingDevice.CreatePerfGroup("World UI"))
                {
                    if (RenderSectorDebugInfo)
                    {
                        _sectorDebugRenderer.Render(tileRect);
                    }

                    GameUiBridge.RenderTurnBasedUI();
                    // TODO renderFuncs.RenderTextBubbles(info);
                    // TODO renderFuncs.RenderTextFloaters(info);

                    AnimGoalsDebugRenderer.RenderAllAnimGoals(
                        tileRect.x1, tileRect.x2,
                        tileRect.y1, tileRect.y2);
                }

                mRenderingDevice.Present();
            }
        }

        public void Dispose()
        {
            _sectorDebugRenderer.Dispose();
        }
    }
}