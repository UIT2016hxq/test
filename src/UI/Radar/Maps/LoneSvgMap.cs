using eft_dma_radar.Common.Misc;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Svg.Skia;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace eft_dma_radar.Common.Maps
{
    /// <summary>
    /// SVG map implementation with a bounded, on-demand raster cache.
    /// The map coordinate system always uses the configured SvgScale; texture resolution
    /// is independent, so reducing memory use never changes marker alignment.
    /// </summary>
    public sealed class XMSvgMap : IXMMap
    {
        private const int InitialRasterDimension = 1536;
        private const int MinimumRasterDimension = 1024;
        private const int MaximumRasterDimension = 4096;
        private const float LayerBoundaryTolerance = 0.08f;

        private readonly MapLayer[] _layers;
        private readonly float _mapWidth;
        private readonly float _mapHeight;
        private bool _disposed;

        public string ID { get; }
        public XMMapConfig Config { get; }
        public int LayerCount => _layers.Length;

        private static readonly SKPaint SvgPaint = new()
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High
        };

        public XMSvgMap(string mapsDirectory, string id, XMMapConfig config)
        {
            ID = id;
            Config = config;

            var layers = config.MapLayers
                .Select(layer => new MapLayer(Path.Combine(mapsDirectory, layer.Filename), layer))
                .Where(layer => File.Exists(layer.Filename))
                .OrderBy(layer => !layer.IsBaseLayer)
                .ThenBy(layer => layer.SortHeight)
                .ToArray();

            if (layers.Length == 0)
                throw new InvalidOperationException("No valid SVG map layers loaded.");

            try
            {
                // The base layer is the only texture required before the map can appear.
                // All floor overlays are decoded later on a worker thread.
                var baseLayer = layers.FirstOrDefault(layer => layer.IsBaseLayer) ?? layers[0];
                baseLayer.LoadImmediately(InitialRasterDimension);

                if (!baseLayer.TryGetLogicalSize(config.SvgScale, out _mapWidth, out _mapHeight))
                    throw new InvalidOperationException("Failed to determine SVG map dimensions.");

                _layers = layers;
            }
            catch
            {
                foreach (var layer in layers)
                    layer.Dispose();
                throw;
            }
        }

        public void Draw(SKCanvas canvas, float playerHeight, SKRect mapBounds, SKRect windowBounds, int? layerOverride = null)
        {
            if (_disposed || _layers.Length == 0 || _mapWidth <= 0 || _mapHeight <= 0)
                return;

            var activeLayer = GetActiveLayer(playerHeight, layerOverride);
            var rasterDimension = GetRequiredRasterDimension(mapBounds, windowBounds);

            for (int i = 0; i < _layers.Length; i++)
            {
                var layer = _layers[i];
                if (!ShouldDrawLayer(layer, i, activeLayer, playerHeight, layerOverride))
                    continue;

                // The base layer is already available. Other required layers appear as
                // soon as their background rasterisation finishes, without blocking a frame.
                layer.RequestRaster(rasterDimension);

                var paint =
                    (activeLayer > 0 &&
                     i != activeLayer &&
                     !(layer.IsBaseLayer && HasNonDimLayerAbove(i)))
                        ? SharedPaints.PaintBitmapAlpha
                        : SharedPaints.PaintBitmap;

                layer.Draw(canvas, mapBounds, windowBounds, _mapWidth, _mapHeight, paint);
            }

            PrewarmAdjacentLayers(activeLayer, rasterDimension);
            TrimLayerCache(activeLayer, playerHeight, layerOverride);
        }

        private int GetActiveLayer(float playerHeight, int? layerOverride)
        {
            if (layerOverride is int forcedLayer)
                return Math.Clamp(forcedLayer, 0, _layers.Length - 1);

            var activeLayer = 0; // Keep the base map visible when a height is temporarily unavailable.
            for (int i = 0; i < _layers.Length; i++)
            {
                if (_layers[i].IsHeightInRange(playerHeight, LayerBoundaryTolerance))
                    activeLayer = i;
            }

            return activeLayer;
        }

        private static bool ShouldDrawLayer(MapLayer layer, int index, int activeLayer, float playerHeight, int? layerOverride)
        {
            if (layerOverride is not null)
                return layer.IsBaseLayer || index == activeLayer;

            return layer.IsBaseLayer ||
                   (index <= activeLayer && layer.IsHeightInRange(playerHeight, LayerBoundaryTolerance));
        }

        private void PrewarmAdjacentLayers(int activeLayer, int rasterDimension)
        {
            for (int i = Math.Max(0, activeLayer - 1); i <= Math.Min(_layers.Length - 1, activeLayer + 1); i++)
                _layers[i].RequestRaster(rasterDimension);
        }

        private void TrimLayerCache(int activeLayer, float playerHeight, int? layerOverride)
        {
            for (int i = 0; i < _layers.Length; i++)
            {
                var layer = _layers[i];
                var keep = layer.IsBaseLayer || Math.Abs(i - activeLayer) <= 1;
                if (layerOverride is null)
                    keep |= layer.IsHeightInRange(playerHeight, LayerBoundaryTolerance);

                if (!keep)
                    layer.ReleaseRaster();
            }
        }

        private int GetRequiredRasterDimension(SKRect mapBounds, SKRect windowBounds)
        {
            if (mapBounds.Width <= 0 || mapBounds.Height <= 0 || windowBounds.Width <= 0 || windowBounds.Height <= 0)
                return InitialRasterDimension;

            var requiredWidth = _mapWidth * windowBounds.Width / mapBounds.Width;
            var requiredHeight = _mapHeight * windowBounds.Height / mapBounds.Height;
            return Math.Clamp((int)MathF.Ceiling(MathF.Max(requiredWidth, requiredHeight)),
                MinimumRasterDimension, MaximumRasterDimension);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasNonDimLayerAbove(int index)
        {
            for (int i = index + 1; i < _layers.Length; i++)
            {
                if (!_layers[i].DimBaseLayer)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Map parameters for WPF map view.
        /// </summary>
        public XMMapParams GetParameters(SKGLElement element, int zoom, ref Vector2 localPlayerMapPos)
        {
            zoom = Math.Clamp(zoom, 1, 800);

            float zoomMul = 0.01f * zoom;
            float zoomWidth = _mapWidth * zoomMul;
            float zoomHeight = _mapHeight * zoomMul;

            var canvasSize = element.CanvasSize;

            var bounds = new SKRect(
                localPlayerMapPos.X - zoomWidth * 0.5f,
                localPlayerMapPos.Y - zoomHeight * 0.5f,
                localPlayerMapPos.X + zoomWidth * 0.5f,
                localPlayerMapPos.Y + zoomHeight * 0.5f)
                .AspectFill(canvasSize);

            return new XMMapParams
            {
                Map = Config,
                Bounds = bounds,
                XScale = canvasSize.Width / bounds.Width,
                YScale = canvasSize.Height / bounds.Height
            };
        }

        /// <summary>
        /// Map parameters for ESP / MiniRadar.
        /// </summary>
        public XMMapParams GetParametersE(SKSize control, float zoom, ref Vector2 localPlayerMapPos)
        {
            zoom = Math.Clamp(zoom, 1f, 800f);

            float zoomMul = 0.01f * zoom;
            float zoomWidth = _mapWidth * zoomMul;
            float zoomHeight = _mapHeight * zoomMul;

            var bounds = new SKRect(
                localPlayerMapPos.X - zoomWidth * 0.5f,
                localPlayerMapPos.Y - zoomHeight * 0.5f,
                localPlayerMapPos.X + zoomWidth * 0.5f,
                localPlayerMapPos.Y + zoomHeight * 0.5f)
                .AspectFill(control);

            return new XMMapParams
            {
                Map = Config,
                Bounds = bounds,
                XScale = control.Width / bounds.Width,
                YScale = control.Height / bounds.Height
            };
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            foreach (var layer in _layers)
                layer.Dispose();
        }

        private sealed class MapLayer : IDisposable
        {
            // Limit background map decoding so a rapid map/floor change cannot create a memory spike.
            private static readonly SemaphoreSlim RasterizationGate = new(2, 2);

            private readonly object _sync = new();
            private readonly XMMapConfig.Layer _definition;
            private SKImage _image;
            private Task _rasterTask;
            private int _rasterDimension;
            private int _generation;
            private bool _disposed;
            private float _svgWidth;
            private float _svgHeight;

            public string Filename { get; }
            public bool IsBaseLayer => _definition.MinHeight is null && _definition.MaxHeight is null;
            public bool DimBaseLayer => _definition.DimBaseLayer;
            public float SortHeight => _definition.MinHeight ?? float.MinValue;

            public MapLayer(string filename, XMMapConfig.Layer definition)
            {
                Filename = filename;
                _definition = definition;
            }

            public bool IsHeightInRange(float height, float tolerance) =>
                height >= (_definition.MinHeight ?? float.MinValue) - tolerance &&
                height <= (_definition.MaxHeight ?? float.MaxValue) + tolerance;

            public void LoadImmediately(int rasterDimension)
            {
                var image = Rasterize(rasterDimension, out var svgWidth, out var svgHeight);
                lock (_sync)
                {
                    if (_disposed)
                    {
                        image?.Dispose();
                        return;
                    }

                    _image?.Dispose();
                    _image = image;
                    _rasterDimension = image is null ? 0 : Math.Max(image.Width, image.Height);
                    _svgWidth = svgWidth;
                    _svgHeight = svgHeight;
                }
            }

            public bool TryGetLogicalSize(float svgScale, out float width, out float height)
            {
                lock (_sync)
                {
                    width = _svgWidth * svgScale;
                    height = _svgHeight * svgScale;
                    return width > 0 && height > 0;
                }
            }

            public void RequestRaster(int rasterDimension)
            {
                lock (_sync)
                {
                    if (_disposed || (_image is not null && _rasterDimension >= rasterDimension))
                        return;

                    if (_rasterTask is not null && !_rasterTask.IsCompleted)
                        return;

                    var generation = _generation;
                    _rasterTask = Task.Run(() => RasterizeAndStore(rasterDimension, generation));
                }
            }

            public void Draw(SKCanvas canvas, SKRect mapBounds, SKRect windowBounds, float mapWidth, float mapHeight, SKPaint paint)
            {
                lock (_sync)
                {
                    if (_image is null || mapWidth <= 0 || mapHeight <= 0)
                        return;

                    var source = new SKRect(
                        mapBounds.Left / mapWidth * _image.Width,
                        mapBounds.Top / mapHeight * _image.Height,
                        mapBounds.Right / mapWidth * _image.Width,
                        mapBounds.Bottom / mapHeight * _image.Height);
                    canvas.DrawImage(_image, source, windowBounds, paint);
                }
            }

            public void ReleaseRaster()
            {
                lock (_sync)
                {
                    if (_disposed || IsBaseLayer)
                        return;

                    _generation++;
                    _rasterTask = null;
                    _image?.Dispose();
                    _image = null;
                    _rasterDimension = 0;
                }
            }

            private void RasterizeAndStore(int rasterDimension, int generation)
            {
                try
                {
                    RasterizationGate.Wait();
                    try
                    {
                        var image = Rasterize(rasterDimension, out var svgWidth, out var svgHeight);
                        lock (_sync)
                        {
                            if (_disposed || generation != _generation || image is null)
                            {
                                image?.Dispose();
                                return;
                            }

                            if (_image is not null && _rasterDimension >= Math.Max(image.Width, image.Height))
                            {
                                image.Dispose();
                                return;
                            }

                            _image?.Dispose();
                            _image = image;
                            _rasterDimension = Math.Max(image.Width, image.Height);
                            _svgWidth = svgWidth;
                            _svgHeight = svgHeight;
                        }
                    }
                    finally
                    {
                        RasterizationGate.Release();
                    }
                }
                catch (Exception ex)
                {
                    XMLogging.WriteLine($"[Map] Failed to rasterize '{Filename}': {ex.Message}");
                }
            }

            private SKImage Rasterize(int rasterDimension, out float svgWidth, out float svgHeight)
            {
                using var stream = File.OpenRead(Filename);
                using var svg = SKSvg.CreateFromStream(stream);
                var picture = svg.Picture ?? throw new InvalidOperationException("SVG contains no picture.");
                var cull = picture.CullRect;
                if (cull.Width <= 0 || cull.Height <= 0)
                    throw new InvalidOperationException("SVG has invalid bounds.");

                svgWidth = cull.Width;
                svgHeight = cull.Height;

                var scale = Math.Min(rasterDimension / cull.Width, rasterDimension / cull.Height);
                var info = new SKImageInfo(
                    Math.Max(1, (int)MathF.Ceiling(cull.Width * scale)),
                    Math.Max(1, (int)MathF.Ceiling(cull.Height * scale)));

                using var surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create map surface.");
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.Scale(scale);
                canvas.DrawPicture(picture, SvgPaint);
                return surface.Snapshot();
            }

            public void Dispose()
            {
                lock (_sync)
                {
                    if (_disposed)
                        return;

                    _disposed = true;
                    _generation++;
                    _image?.Dispose();
                    _image = null;
                    _rasterTask = null;
                }
            }
        }
    }
}
