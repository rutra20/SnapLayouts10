using System;
using System.Collections.Generic;
using System.Drawing;

namespace SnapLayouts10
{
    internal sealed class ZoneDef
    {
        public readonly RectangleF Bounds;
        public readonly string Name;

        public ZoneDef(float x, float y, float width, float height, string name)
        {
            Bounds = new RectangleF(x, y, width, height);
            Name = name;
        }
    }

    internal sealed class LayoutDef
    {
        public readonly int Index;
        public readonly string Name;
        public readonly string Description;
        public readonly ZoneDef[] Zones;

        public LayoutDef(int index, string name, string description, ZoneDef[] zones)
        {
            Index = index;
            Name = name;
            Description = description;
            Zones = zones;
        }
    }

    internal static class Layouts
    {
        public static readonly LayoutDef[] All = new LayoutDef[]
        {
            new LayoutDef(0, "2 equal columns", "Two equal columns.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 1f, "Left"),
                new ZoneDef(0.5f, 0f, 0.5f, 1f, "Right")
            }),
            new LayoutDef(1, "2 columns 70/30", "Wide left, narrow right.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.7f, 1f, "Large left"),
                new ZoneDef(0.7f, 0f, 0.3f, 1f, "Right")
            }),
            new LayoutDef(2, "3 equal columns", "Three equal columns.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 1f / 3f, 1f, "Left"),
                new ZoneDef(1f / 3f, 0f, 1f / 3f, 1f, "Center"),
                new ZoneDef(2f / 3f, 0f, 1f / 3f, 1f, "Right")
            }),
            new LayoutDef(3, "Left + 2 right", "Large left, two right.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 1f, "Left"),
                new ZoneDef(0.5f, 0f, 0.5f, 0.5f, "Right top"),
                new ZoneDef(0.5f, 0.5f, 0.5f, 0.5f, "Right bottom")
            }),
            new LayoutDef(4, "4 quadrants", "Four equal zones.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 0.5f, "Top left"),
                new ZoneDef(0.5f, 0f, 0.5f, 0.5f, "Top right"),
                new ZoneDef(0f, 0.5f, 0.5f, 0.5f, "Bottom left"),
                new ZoneDef(0.5f, 0.5f, 0.5f, 0.5f, "Bottom right")
            }),
            new LayoutDef(5, "Large left + 3 right", "Large left, three right.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.6f, 1f, "Large left"),
                new ZoneDef(0.6f, 0f, 0.4f, 1f / 3f, "Right top"),
                new ZoneDef(0.6f, 1f / 3f, 0.4f, 1f / 3f, "Right middle"),
                new ZoneDef(0.6f, 2f / 3f, 0.4f, 1f / 3f, "Right bottom")
            })
        };

        public static int Count
        {
            get { return All.Length; }
        }

        public static LayoutDef Get(int index)
        {
            if (index < 0 || index >= All.Length)
                return null;
            return All[index];
        }

        public static List<LayoutDef> GetEnabled(AppSettings settings)
        {
            List<LayoutDef> result = new List<LayoutDef>();
            for (int i = 0; i < All.Length; i++)
            {
                if (settings.IsLayoutEnabled(i))
                    result.Add(All[i]);
            }
            return result;
        }
    }
}
