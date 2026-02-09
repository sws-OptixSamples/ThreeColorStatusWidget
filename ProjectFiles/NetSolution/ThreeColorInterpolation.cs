#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using System.Drawing;
using FTOptix.DataLogger;
using FTOptix.Store;
using FTOptix.SQLiteStore;
using FTOptix.Alarm;
using FTOptix.SerialPort;
#endregion

public class ThreeColorInterpolation : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        BlendStatusColor();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    /// <summary>
    /// As the value changes within the low-mid-high range this method will blend the FillColor property of the parent object.  
    /// </summary>
    /// <param name="value">The input value (0-100) to determine the color.</param>


    [ExportMethod]
    public void BlendStatusColor()
    {
        // Get a reference to the parent object (the object that owns this NetLogic)
        var parent = Owner;

        // Get the status value
        var value = (double)parent.GetVariable("StatusValue").Value;

        // Color choices
        var colorMin = (FTOptix.Core.Color)parent.GetVariable("ColorMin").Value;
        var colorMid = (FTOptix.Core.Color)parent.GetVariable("ColorMid").Value;
        var colorMax = (FTOptix.Core.Color)parent.GetVariable("ColorMax").Value;

        // Color choices
        var valueMin = (double)parent.GetVariable("ValueMin").Value;
        var valueMid = (double)parent.GetVariable("ValueMid").Value;
        var valueMax = (double)parent.GetVariable("ValueMax").Value;

        // Ensure the value is within the min and max range
        value = Math.Max(valueMin, Math.Min(valueMax, value));

        // Canonical, or most correct choice is a byte data type for color components
        byte r, g, b, a;

        // First half of the blend: colorMin to colorMid
        if (value <= valueMid)
        {
            double percentage = (double)(value - valueMin) / (valueMid - valueMin);
            r = (byte)Math.Round(colorMin.R + (colorMid.R - colorMin.R) * percentage);
            g = (byte)Math.Round(colorMin.G + (colorMid.G - colorMin.G) * percentage);
            b = (byte)Math.Round(colorMin.B + (colorMid.B - colorMin.B) * percentage);
            a = (byte)Math.Round(colorMin.A + (colorMid.A - colorMin.A) * percentage);
        }
        else
        {
            // Second half of the blend: colorMid to colorMax
            double percentage = (double)(value - valueMid) / (valueMax - valueMid);
            r = (byte)Math.Round(colorMid.R + (colorMax.R - colorMid.R) * percentage);
            g = (byte)Math.Round(colorMid.G + (colorMax.G - colorMid.G) * percentage);
            b = (byte)Math.Round(colorMid.B + (colorMax.B - colorMid.B) * percentage);
            a = (byte)Math.Round(colorMid.A + (colorMax.A - colorMid.A) * percentage);
        }

        // Combine the components into a single 32-bit integer (AARRGGBB)
        // Bitwise operations are used to shift and combine the byte values.
        // Despite the components being bytes, the compiler will promote to int for the bitwise operations.
        // Then the temp 32-bit integers with shifted bits are combined into a single UInt32 value via logical OR.
        var newColor = (UInt32)((a << 24) | (r << 16) | (g << 8) | b);
        parent.GetVariable("StatusColor").Value = newColor;

    }
}
