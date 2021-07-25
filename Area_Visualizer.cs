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
        public event EventHandler<Vector2> PositionChanged;
        Client client;
        Area area;
        private bool firstUse = false;
        private bool hasJustSaved = true;
        private string overlayDir;
        private string pluginOverlayDir;
        public Area_Visualizer()
        {
            _areaUpdateInterval = 5000;
            //
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
            
            client = new Client("API");
            Log.Debug("Area Visualizer", "Starting Client...");
            _ = Task.Run(client.StartAsync);

            area = new Area();
            _ = Task.Run(CheckAreaChangesAsync);

            this.PositionChanged += (_, input) => Task.Run(async () =>
            {
                if (client.client.IsConnected)
                {
                    await client.rpc.NotifyAsync("SendDataAsync", "AreaVisualizer", "Position", input);
                }
            });
            area.AreaChanged += (_, area) => Task.Run(async () =>
            {
                if (client.client.IsConnected)
                {
                    await client.rpc.NotifyAsync("SendDataAsync", "AreaVisualizer", "Area", area);
                }
            });
        }
        public Vector2 Filter(Vector2 input)
        {
            if (firstUse)
            {
                firstUse = false;
                new Thread(new ThreadStart(CopyFiles)).Start();
            }
            PositionChanged?.Invoke(this, input);
            return input;
        }
        public async Task CheckAreaChangesAsync()
        {
            while(true)
            {
                if (hasJustSaved)
                {
                    hasJustSaved = false;
                }
                else
                {
                    await Task.Delay(_areaUpdateInterval);
                }
                area.UpdateArea();
            }
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
