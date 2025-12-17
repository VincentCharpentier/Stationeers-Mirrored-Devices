using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MirroredAtmospherics.Scripts
{
    [BepInPlugin("net.Apolo.stationeers.MirroredAtmospherics.Scripts", "Mirrored Atmospherics", "0.0.5.0")]   
    public class MirroredAtmosphericsPlugin : BaseUnityPlugin
    {
        public static MirroredAtmosphericsPlugin Instance;


        public void Log(string line)
        {
            Debug.Log("[MirroredAtmospherics]: " + line);
        }

        void Awake()
        {
            MirroredAtmosphericsPlugin.Instance = this;

            try
            {
                var harmony = new Harmony("net.Apolo.stationeers.MirroredAtmospherics.Scripts");
                harmony.PatchAll();
                Log("Patch succeeded");
            }
            catch (Exception e)
            {
                Log("Patch Failed");
                Log(e.ToString());
            }
        }
    }
}