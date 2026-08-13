using eft_dma_radar.Common.Misc;
using System.Collections.Frozen;
using System.IO;
using System.Text.Json;

namespace eft_dma_radar.Common.Maps
{
    /// <summary>
    /// Maintains Map Resources for this application.
    /// </summary>
    public static class XMMapManager
    {
        private static readonly Lock _sync = new();
        private static FrozenDictionary<string, XMMapConfig> _maps;
        private static string _mapsDirectory;
        private static Task<XMSvgMap> _mapLoadTask;
        private static string _loadingMapId;

        /// <summary>
        /// Currently Loaded Map.
        /// </summary>
        public static IXMMap Map { get; private set; }

        /// <summary>
        /// Initialize this Module.
        /// ONLY CALL ONCE!
        /// </summary>
        public static void ModuleInit()
        {
            try
            {
                _mapsDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Maps");

                if (!Directory.Exists(_mapsDirectory))
                    throw new DirectoryNotFoundException($"Maps directory not found: {_mapsDirectory}");

                var mapsBuilder = new Dictionary<string, XMMapConfig>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in Directory.EnumerateFiles(_mapsDirectory, "*.json", SearchOption.TopDirectoryOnly))
                {
                    using var stream = File.OpenRead(file);
                    var config = JsonSerializer.Deserialize<XMMapConfig>(stream);

                    if (config == null || config.MapID == null)
                        continue;

                    config.CaptureDefaults();

                    foreach (var id in config.MapID)
                        mapsBuilder[id] = config;
                }

                if (mapsBuilder.Count == 0)
                    throw new Exception("No map configs found in Maps directory.");

                _maps = mapsBuilder.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to Initialize Maps!", ex);
            }
        }

        /// <summary>
        /// Start loading a map without blocking the render loop. The current map remains
        /// valid until the new base layer has been prepared successfully.
        /// </summary>
        /// <param name="mapId">Id of map to load.</param>
        public static void LoadMap(string mapId)
        {
            lock (_sync)
            {
                if (mapId.Equals(Map?.ID, StringComparison.OrdinalIgnoreCase) ||
                    mapId.Equals(_loadingMapId, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!_maps.TryGetValue(mapId, out var config))
                    config = _maps["default"];

                _loadingMapId = mapId;
                _mapLoadTask = Task.Run(() => new XMSvgMap(_mapsDirectory, mapId, config));
                var pendingTask = _mapLoadTask;
                _ = pendingTask.ContinueWith(task => CompleteLoad(mapId, pendingTask, task),
                    TaskScheduler.Default);
            }
        }

        private static void CompleteLoad(string mapId, Task<XMSvgMap> pendingTask, Task<XMSvgMap> task)
        {
            IXMMap previousMap = null;
            XMSvgMap loadedMap = null;

            try
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    loadedMap = task.Result;
                else if (task.Exception is not null)
                    XMLogging.WriteLine($"[Map] Failed to load '{mapId}': {task.Exception.GetBaseException().Message}");

                lock (_sync)
                {
                    if (!ReferenceEquals(_mapLoadTask, pendingTask))
                    {
                        loadedMap?.Dispose();
                        return;
                    }

                    _mapLoadTask = null;
                    _loadingMapId = null;

                    if (loadedMap is null)
                        return;

                    previousMap = Map;
                    Map = loadedMap;
                }

                previousMap?.Dispose();
            }
            catch (Exception ex)
            {
                loadedMap?.Dispose();
                XMLogging.WriteLine($"[Map] Unable to activate '{mapId}': {ex.Message}");
            }
        }
    }
}
