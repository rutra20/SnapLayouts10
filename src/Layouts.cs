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
            new LayoutDef(0, "2 coloane egale", "Împarte ecranul în două coloane egale.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 1f, "Stânga"),
                new ZoneDef(0.5f, 0f, 0.5f, 1f, "Dreapta")
            }),
            new LayoutDef(1, "2 coloane 70/30", "O coloană mare în stânga și una îngustă în dreapta.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.7f, 1f, "Stânga mare"),
                new ZoneDef(0.7f, 0f, 0.3f, 1f, "Dreapta")
            }),
            new LayoutDef(2, "3 coloane egale", "Trei coloane de lățimi egale.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 1f / 3f, 1f, "Stânga"),
                new ZoneDef(1f / 3f, 0f, 1f / 3f, 1f, "Centru"),
                new ZoneDef(2f / 3f, 0f, 1f / 3f, 1f, "Dreapta")
            }),
            new LayoutDef(3, "Stânga + 2 dreapta", "O zonă mare în stânga și două zone suprapuse în dreapta.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 1f, "Stânga"),
                new ZoneDef(0.5f, 0f, 0.5f, 0.5f, "Dreapta sus"),
                new ZoneDef(0.5f, 0.5f, 0.5f, 0.5f, "Dreapta jos")
            }),
            new LayoutDef(4, "4 cadrane", "Patru zone egale, în grilă 2 x 2.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.5f, 0.5f, "Stânga sus"),
                new ZoneDef(0.5f, 0f, 0.5f, 0.5f, "Dreapta sus"),
                new ZoneDef(0f, 0.5f, 0.5f, 0.5f, "Stânga jos"),
                new ZoneDef(0.5f, 0.5f, 0.5f, 0.5f, "Dreapta jos")
            }),
            new LayoutDef(5, "Stânga mare + 3 dreapta", "O zonă lată în stânga și trei zone suprapuse în dreapta.", new ZoneDef[]
            {
                new ZoneDef(0f, 0f, 0.6f, 1f, "Stânga mare"),
                new ZoneDef(0.6f, 0f, 0.4f, 1f / 3f, "Dreapta sus"),
                new ZoneDef(0.6f, 1f / 3f, 0.4f, 1f / 3f, "Dreapta centru"),
                new ZoneDef(0.6f, 2f / 3f, 0.4f, 1f / 3f, "Dreapta jos")
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
