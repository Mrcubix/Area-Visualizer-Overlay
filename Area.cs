using System.Numerics;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace Area_Visualizer
{
    public class Area
    {
        private DigitizerIdentifier digitizer = Info.Driver.OutputMode.Tablet.Digitizer;
        public Area FullArea;
        public float lpmm = Info.Driver.OutputMode.Tablet.Digitizer.MaxX / Info.Driver.OutputMode.Tablet.Digitizer.Width;
        public Vector2 position;
        public Vector2 size;
        public void SetArea(Vector2 position, Vector2 size)
        {
            this.position = position;
            this.size = size;
        }
        public Area() {
            Vector2 fullarea = new Vector2(digitizer.Width, digitizer.Height);
            FullArea = new Area(fullarea, fullarea / 2);
        }
        public Area(Vector2 size, Vector2 position)
        {
            this.position = position;
            this.size = size;
        }
        public void SetAreaIfChanged()
        {
            Vector2 definedSize = new Vector2();
            Vector2 definedPosition = new Vector2();
            if (Info.Driver.OutputMode is AbsoluteOutputMode absoluteOutputMode)
            {
                definedSize = new Vector2(absoluteOutputMode.Input.Width, absoluteOutputMode.Input.Height);
                definedPosition = absoluteOutputMode.Input.Position;
            }
            else
            {
                definedSize = new Vector2(Info.Driver.OutputMode.Tablet.Digitizer.Width, Info.Driver.OutputMode.Tablet.Digitizer.Height);
                definedPosition = definedSize / 2;
            }
            if (size != definedSize || position != definedPosition)
            {
                Vector2 fullarea = new Vector2(digitizer.Width, digitizer.Height);
                FullArea = new Area(fullarea, fullarea / 2);
                size = definedSize;
                position = definedPosition;
            }
        }
    }
}