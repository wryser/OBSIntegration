using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace OBSWebServer.Patches
{
    [HarmonyPatch(typeof(ZoneManagement), "SetZones"), HarmonyWrapSafe]
    public static class MapPatch
    {
        public static GTZone[] ActiveZones;

        public static void Prefix(GTZone[] newActiveZones)
        {
            ActiveZones = newActiveZones;
        }
    }

}