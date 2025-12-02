using Assets.Scripts;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using HarmonyLib;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace MirroredAtmospherics.Scripts
{
    /// <summary>
    /// Harmony patch to inject our prefabs in the game
    /// </summary>
    [HarmonyPatch]
    public static class PrefabLoadPatch
    {
        private static void Log(string message)
        {
            MirroredAtmosphericsPlugin.Instance.Log(message);
        }
        /// <summary>
        /// Used to describe a device mirroring to perform
        /// </summary>
        private class MirrorDefinition
        {
            /// <summary>
            /// the identifier of the device we want to flip
            /// </summary>
            public string deviceName;


            public string mirrorName { get; private set; }
            public int mirrorHash { get; private set; }

            public string mirrorDisplayName;
            public string mirrorDescription;

            public delegate void MirrorPostFix(Thing mirroredThing);
            /// <summary>
            /// Deleguate to run tweaks on the mirrored device
            /// </summary>
            public MirrorPostFix postfix;

            public MirrorDefinition(string deviceName)
            {
                this.deviceName = deviceName;
                /// To make sure this version is different than the original one, we have to change it's game Prefab Name and Hash:
                /// avoids having two prefabs with the same name
                this.mirrorName = $"{deviceName}Mirrored";
                this.mirrorHash = Animator.StringToHash(this.mirrorName);
            }
        }

        /// <summary>
        /// List of mirrored devices to create
        /// </summary>
        private static readonly MirrorDefinition[] atmoMirrorDefs = new[] {
            new MirrorDefinition("StructureFiltration") {
                mirrorDisplayName = "Filtration (Mirrored)",
                mirrorDescription =
                    "Mirrored version of the standard {THING:StructureFiltration}.",
                postfix = (Thing mirroredDevice) => {
                    // filtration mirroring tweaks
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderSlot2TypeGasFilter"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderSlot3TypeGasFilter"));
                    // update input / output arrow display
                    InvertInputOutput(mirroredDevice);
                }
            },
            new MirrorDefinition("StructureAirConditioner") {
                mirrorDisplayName = "Air Conditioner (Mirrored)",
                mirrorDescription =
                    "Mirrored version of the standard {THING:StructureAirConditioner}.",
                postfix = (Thing mirroredDevice) => {
                    // air conditioner mirroring tweaks
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    // flip control screen
                    FlipTransform(mirroredDevice.FindTransform("PanelNormal"));
                    // update input / output arrow display
                    InvertInputOutput(mirroredDevice);
                },
            },
            new MirrorDefinition("StructureElectrolyzer")
            {
                mirrorDisplayName = "Electrolyzer (Mirrored)",
                mirrorDescription =
                    "Mirrored version of the standard {THING:StructureElectrolyzer}.",
                postfix = (Thing mirroredDevice) => {
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    // update input / output arrow display
                    InvertInputOutput(mirroredDevice);
                }
            },
            new MirrorDefinition("H2Combustor")
            {
                mirrorDisplayName = "H2 Combustor (Mirrored)",
                mirrorDescription =
                    "Mirrored version of the standard {THING:H2Combustor}.",
                postfix = (Thing mirroredDevice) => {
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    // update input / output arrow display
                    InvertInputOutput(mirroredDevice);
                }
            },
            new MirrorDefinition("StructureNitrolyzer")
            {
                mirrorDisplayName = "Nitrolyzer (Mirrored)",
                mirrorDescription =
                    "Mirrored version of the standard Nitrolyzer Unit",
                postfix = (Thing mirroredDevice) => {
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    // update input / output arrow display
                    InvertInputOutput(mirroredDevice);
                }
            }
        };

        // permanent hidden object to store the new prefabs we will create
        private static readonly GameObject HiddenParent = new GameObject("~HiddenGameObject");

        // filtering / search optimization
        private static List<MultiConstructor> Constructors = new List<MultiConstructor>();
        private static List<Thing> Devices = new List<Thing>();
        // Wireframe caching
        private static Dictionary<string, List<Edge>> WireframeData;

        [HarmonyPatch(typeof(Prefab), "LoadAll")]
        [HarmonyPrefix]
        [UsedImplicitly]
        static private void LoadMirrorPrefabs()
        {
            // Init prefab parent
            UnityEngine.Object.DontDestroyOnLoad(HiddenParent.gameObject);
            HiddenParent.SetActive(value: false);

            Log("Loading wireframe data");
            WireframeData = WireframeStorage.LoadWireframeData();
            Log("Prefiltering assets");
            // Load wireframe data
            // prefilter data for load time optimization
            WorldManager.Instance.SourcePrefabs.ForEach(thing =>
            {
                if (thing == null)
                {
                    return;
                }
                var ctor = thing.GetComponent<MultiConstructor>();
                if (ctor != null)
                {
                    Constructors.Add(ctor);
                }
                else if (thing.gameObject != null)
                {
                    foreach (var mirrorDef in atmoMirrorDefs)
                    {
                        if (thing.name == mirrorDef.deviceName)
                        {
                            Devices.Add(thing);
                            break;
                        }
                    }
                }
            });

            Log("Mirroring devices...");
            foreach (var mirrorDef in atmoMirrorDefs)
            {
                MirrorAtmosphericDevice(mirrorDef);
            }

            Log("All done");
        }

        static private void MirrorAtmosphericDevice(MirrorDefinition mirrorDef)
        {
            // Mirror device
            Log($"Mirroring: {mirrorDef.deviceName}");
            Thing mirroredDevice = CreateMirroredThing(mirrorDef);

            if (mirroredDevice == null)
            {
                return;
            }

            // add to atmospherics constructor
            AddToConstructor(mirrorDef, mirroredDevice, "ItemKitAtmospherics");

            // run mirror specific postfix
            mirrorDef.postfix(mirroredDevice);
        }

        static private void AddToConstructor(MirrorDefinition mirrorDef, Thing mirroredDevice, string ctorName)
        {
            var ctors = FindConstructors(mirrorDef.deviceName);
            foreach (var deviceCtor in ctors)
            {
                int insertIndex = deviceCtor.Constructables.FindIndex(p => p.name == mirrorDef.deviceName);
                Structure newStruct = mirroredDevice as Structure;
                deviceCtor.Constructables.Insert(insertIndex + 1, mirroredDevice as Structure);
            }
        }

        static private MultiConstructor[] FindConstructors(string deviceName)
        {
            return Constructors.FindAll(thing =>
            {
                var ctor = thing as MultiConstructor;
                if (ctor != null && ctor.Constructables != null)
                {
                    return ctor.Constructables.Find(p => p != null && p.name == deviceName) != null;
                }
                return false;
            }).Select(thing => thing as MultiConstructor).ToArray();
        }

        static private Thing CreateMirroredThing(MirrorDefinition mirrorDef)
        {
            var device = Devices.Find(p => p != null && p.name == mirrorDef.deviceName);

            if (device == null)
            {
                Log("Cannot find device for " + mirrorDef.deviceName);
                return null;
            }

            GameObject source = device.gameObject;


            if (source == null)
            {
                Log("Cannot find gameobject for " + mirrorDef.deviceName);
                return null;
            }

            /// Clone the original object and adjust the name
            GameObject mirroredGameObject = GameObject.Instantiate(source, HiddenParent.transform);
            mirroredGameObject.name = mirrorDef.mirrorName;
            /// To make sure this version is different than the original one, we have to change it's game Prefab Name and Hash:
            /// avoids having two prefabs with the same name
            Thing mirroredThing = mirroredGameObject.GetComponent<Thing>();
            mirroredThing.PrefabName = mirrorDef.mirrorName;
            mirroredThing.PrefabHash = mirrorDef.mirrorHash;

            FlipTransform(mirroredGameObject.transform);


            if (mirroredThing.Blueprint != null)
            {
                // copy blueprint game object
                mirroredThing.Blueprint = GameObject.Instantiate(mirroredThing.Blueprint, HiddenParent.transform);
                // mirror blueprint
                FlipTransform(mirroredThing.Blueprint.transform);

                // update blueprint wireframe
                Wireframe blueprintWireframe = mirroredThing.Blueprint.GetComponent<Wireframe>();
                if (blueprintWireframe != null)
                {
                    List<Edge> edges;
                    // try getting cached wireframe data
                    if (!WireframeData.TryGetValue(mirrorDef.mirrorName, out edges))
                    {
                        Log($"Regenerating wireframe for {mirrorDef.mirrorName}...");
                        // regenerate wireframe edges
                        WireframeGenerator wfGen = new WireframeGenerator(mirroredThing.Blueprint.transform);

                        edges = wfGen.Edges;
                        WireframeData.Add(mirrorDef.mirrorName, edges);
                        WireframeStorage.SaveWireframeData(WireframeData);
                    }
                    blueprintWireframe.WireframeEdges = edges;
                }
            }

            /// Add to the game as an asset
            Prefab.RegisterExisting(mirroredThing);
            WorldManager.Instance.SourcePrefabs.Add(mirroredThing);

            return mirroredThing;
        }
        private static void FlipTransform(Transform transform)
        {
            // mirror the object using scale X -1
            transform.localScale = new Vector3(-1, 1, 1);
        }

        /// <summary>
        /// Invert the role of each IO connection in the device
        /// </summary>
        /// <param name="mirroredDevice"></param>
        public static void InvertInputOutput(Thing mirroredDevice)
        {

            SmallGrid smGrid = mirroredDevice.GetComponent<SmallGrid>();
            Connection input = smGrid.OpenEnds.Find(x => x.ConnectionRole == ConnectionRole.Input);
            Connection input2 = smGrid.OpenEnds.Find(x => x.ConnectionRole == ConnectionRole.Input2);
            Connection output = smGrid.OpenEnds.Find(x => x.ConnectionRole == ConnectionRole.Output);
            Connection output2 = smGrid.OpenEnds.Find(x => x.ConnectionRole == ConnectionRole.Output2);

            if (input != null)
            {
                input.ConnectionRole = ConnectionRole.Output;
            }
            if (input2 != null)
            {
                input2.ConnectionRole = ConnectionRole.Output2;
            }
            if (output != null)
            {
                output.ConnectionRole = ConnectionRole.Input;
            }
            if (output2 != null)
            {
                output2.ConnectionRole = ConnectionRole.Input2;
            }
        }


        [HarmonyPatch(typeof(Localization.LanguageFolder), nameof(Localization.LanguageFolder.LoadAll)), HarmonyPrefix]
        private static void Localization_LanguageFolder_LoadAll_Prefix(Localization.LanguageFolder __instance)
        {
            if (__instance.Code != LanguageCode.EN) return;

            foreach (var mirrorDef in atmoMirrorDefs)
            {
                __instance.LanguagePages[0].Things.Add(new Localization.RecordThing
                {
                    Key = mirrorDef.mirrorName,
                    Value = mirrorDef.mirrorDisplayName,
                    ThingDescription =
                    mirrorDef.mirrorDescription
                });
            }
        }
    }


}

