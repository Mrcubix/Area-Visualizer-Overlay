using System.Numerics;
using System.Threading;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin;
using System.Threading.Tasks;
using System;
using System.IO;

namespace Area_Visualizer
{
    [PluginName("Area Visualizer Overlay")]
    public class Area_Visualizer : IFilter
    {
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);
        Server server;
        Area area;
        private bool serverIsRunning = false;
        private bool firstUse = false;
        private bool hasJustSaved = true;
        private Vector2 currentPosition = new Vector2();
        private string overlayDir;
        private string pluginOverlayDir;
        public Area_Visualizer()
        {
            overlayDir = Path.Combine(Directory.GetCurrentDirectory(), "Overlays");
            if (!Directory.Exists(overlayDir))
            {
                Directory.CreateDirectory(overlayDir);
                firstUse = true;
            }
            pluginOverlayDir = Path.Combine(overlayDir,"AreaVisualizer");
            if (!Directory.Exists(pluginOverlayDir))
            {
                Directory.CreateDirectory(pluginOverlayDir);
                firstUse = true;
            }
            
            if (!serverIsRunning) 
            {
                serverIsRunning = true;
                server = new Server("AreaVisualizer", this);
                area = new Area();
                Log.Debug("Area Visualizer", "Starting server");
                _ = Task.Run(server.StartAsync);
            }
        }
        public Vector2 Filter(Vector2 input)
        {
            if (firstUse)
            {
                firstUse = false;
                new Thread(new ThreadStart(CopyFiles)).Start();
            }
            currentPosition = input;
            resetEvent.Set();
            resetEvent.Reset();
            return input;
        }
        public Task<string> GetMethodsAsync()
        {
            string methods = "[\"GetPositionAsync\", \"GetAreaAsync\"]";
            return Task.FromResult(methods);
        }
        public async Task<Vector2> GetPositionAsync()
        {
            await Task.Run(() => resetEvent.WaitOne());
            return currentPosition;
        }
        public async Task<Area> GetAreaAsync()
        {
            if (hasJustSaved)
            {
                hasJustSaved = false;
            }
            else
            {
                await Task.Delay(_areaUpdateInterval);
            }
            area.SetAreaIfChanged();
            return area;
        }
        public FilterStage FilterStage => FilterStage.PreTranspose;
        // http://msdn.microsoft.com/en-us/library/cc148994.aspx
        public void CopyFiles()
        {
            DirectoryInfo source = new DirectoryInfo(Path.Combine(_pluginsPath, "AreaVisualizer"));
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(pluginOverlayDir, file.Name), true);
                Log.Debug("Area Visualizer", $"Moving {file.Name} to {pluginOverlayDir}");
            }
            foreach(DirectoryInfo directory in source.GetDirectories())
            {
                string targetFolder = Path.Combine(pluginOverlayDir, directory.Name);
                Directory.CreateDirectory(targetFolder);
                Log.Debug("Area Visualizer", $"Created folder {directory.Name} in {pluginOverlayDir}");
                DirectoryInfo directoryTarget = new DirectoryInfo(targetFolder);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.CopyTo(Path.Combine(targetFolder, file.Name), true);
                    Log.Debug("Area Visualizer", $"Moving {file.Name} to {targetFolder}");
                }
            }
        }
        [Property("Area Update Interval"),
         Unit("ms"),
         DefaultPropertyValue(5000),
         ToolTip("Area Visualizer Overlay:\n\n" +
                 "Time taken to transition from an area to another.\n\n" +
                 "It is recommended to keep this value relatively high as to not impact performance.")
        ]
        public int areaUpdateInterval
        {
            set
            {
                _areaUpdateInterval = value;
            } 
            get => _areaUpdateInterval;
        }
        public int _areaUpdateInterval;
        [Property("Plugin folder path"),
         ToolTip("Proxy API:\n\n" +
                 "Folder where this plugin is located in.\n\n" +
                 "E.g: 'C:\\Users\\{user}\\AppData\\Local\\OpenTabletDriver\\Plugins\\Area Visualizer' on windows.")
        ]
        public string pluginsPath 
        {
            get => @_pluginsPath; 
            set
            {
                _pluginsPath = @value;
            }
        }
        public string _pluginsPath;
    }
}
