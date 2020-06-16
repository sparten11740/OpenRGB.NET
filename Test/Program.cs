﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenRGB.NET;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            OpenRGBClient openRgb = new OpenRGBClient(port:1338);
            openRgb.Connect();
            var controllerCount = openRgb.GetControllerCount();
            var devices = new List<OpenRGBDevice>();

            for(uint i = 0; i < controllerCount; i++)
                devices.Add(openRgb.GetControllerData(i));

            Console.WriteLine("asds");
            var deviceIndex = devices.FindIndex(d => d.name.Contains("G810"));
            var data = devices[deviceIndex];
            
            var list = new List<OpenRGBColor>(data.leds.Length);
            Color clr = Color.Red;
            for (int i = 0; i < data.leds.Length; i++)
            {
                list.Add(new OpenRGBColor(clr.R, clr.G, clr.B));
                clr = ChangeHue(clr, 360 / data.leds.Length);
            }
            openRgb.SendColors((uint)deviceIndex, list.ToArray());

        }

        public static void ToHsv(Color color, out double hue, out double saturation, out double value)
        {
            var max = Math.Max(color.R, Math.Max(color.G, color.B));
            var min = Math.Min(color.R, Math.Min(color.G, color.B));

            var delta = max - min;

            hue = 0d;
            if (delta != 0)
            {
                if (color.R == max) hue = (color.G - color.B) / (double)delta;
                else if (color.G == max) hue = 2d + (color.B - color.R) / (double)delta;
                else if (color.B == max) hue = 4d + (color.R - color.G) / (double)delta;
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static Color FromHsv(double hue, double saturation, double value)
        {
            saturation = Math.Max(Math.Min(saturation, 1), 0);
            value = Math.Max(Math.Min(value, 1), 0);

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = (byte)(value);
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0: return Color.FromArgb(v, t, p);
                case 1: return Color.FromArgb(q, v, p);
                case 2: return Color.FromArgb(p, v, t);
                case 3: return Color.FromArgb(p, q, v);
                case 4: return Color.FromArgb(t, p, v);
                default: return Color.FromArgb(v, p, q);
            }
        }

        /// <summary>
        /// Changes the hue of <paramref name="color"/>
        /// </summary>
        /// <param name="color">Color to be modified</param>
        /// <param name="offset">Hue offset in degrees</param>
        /// <returns>Color with modified hue</returns>
        public static Color ChangeHue(Color color, double offset)
        {
            if (offset == 0)
                return color;

            ToHsv(color, out var hue, out var saturation, out var value);

            hue += offset;

            while (hue > 360) hue -= 360;
            while (hue < 0) hue += 360;

            return FromHsv(hue, saturation, value);
        }
    }
}