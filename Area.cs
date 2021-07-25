using System;
using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace Area_Visualizer
{
    public class Area
    {
        public event EventHandler<Area> AreaChanged; 
        private DigitizerIdentifier digitizer = Info.Driver.OutputMode.Tablet.Digitizer;
        public Area fullArea;
        public float lpmm = Info.Driver.OutputMode.Tablet.Digitizer.MaxX / Info.Driver.OutputMode.Tablet.Digitizer.Width;
        public Vector2 position;
        public Vector2 size;
        public void SetArea(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }
        public Area() 
        {
            UpdateArea();
        }
        public Area(Vector2 size, Vector2 position)
        {
            this.position = position;
            this.size = size;
        }
        public void UpdateArea()
        {
            Vector2 fullarea = new Vector2(digitizer.Width, digitizer.Height);
            fullArea = new Area(fullarea, fullarea / 2);
            if (Info.Driver.OutputMode is AbsoluteOutputMode absoluteOutputMode)
            {
                size = new Vector2(absoluteOutputMode.Input.Width, absoluteOutputMode.Input.Height);
                position = absoluteOutputMode.Input.Position;
            }
            else
            {
                size = new Vector2(Info.Driver.OutputMode.Tablet.Digitizer.Width, Info.Driver.OutputMode.Tablet.Digitizer.Height);
                position = size / 2;
            }
            AreaChanged?.Invoke(this, this);
        }
    }
}