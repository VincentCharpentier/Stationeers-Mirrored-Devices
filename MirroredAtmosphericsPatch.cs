using Assets.Scripts;
using Assets.Scripts.Inventory;
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
        /// List of mirrored devices to create
        /// </summary>
        private static readonly MirrorDefinition[] atmoMirrorDefs = new[] {
            new MirrorDefinition("StructureFiltration") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Output
                    }
                },
                postfix = mirroredDevice => {
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                    // filtration mirroring tweaks
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderSlot2TypeGasFilter"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderSlot3TypeGasFilter"));
                }
            },
            new MirrorDefinition("StructureAirConditioner") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Output
                    }
                },
                postfix = mirroredDevice => {
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                    // air conditioner mirroring tweaks
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                    // flip control screen
                    FlipTransform(mirroredDevice.FindTransform("PanelNormal"));
                },
            },
            new MirrorDefinition("StructureElectrolyzer")
            {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.PipeLiquid,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Output
                    }
                },
                postfix = mirroredDevice => {
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                }
            },
            new MirrorDefinition("H2Combustor")
            {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input
                    }
                },
                postfix = mirroredDevice => {
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                }
            },
            new MirrorDefinition("StructureNitrolyzer")
            {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input2
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Output
                    }
                },
                postfix = mirroredDevice => {
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("InfoScreen"));
                }
            },
            // Phase change devices
            new MirrorDefinition("StructureCondensationChamber") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.PipeLiquid,
                        Role = ConnectionRole.Output
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input2 // acting as heat dump/pump
                    }
                },
                postfix = mirroredDevice => {
                    // flip info screen (aesthetics)
                    FlipPhaseChangeScreenTransform(mirroredDevice.FindTransform("ScreenNoShadow"));
                    // restore increase/decrease buttons position on setting wheel
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderButton1Trigger"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderButton2Trigger"));
                    // fix switch collider display
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                }
            },
            new MirrorDefinition("StructureEvaporationChamber") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.PipeLiquid,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Output
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Pipe,
                        Role = ConnectionRole.Input2 // acting as heat dump/pump
                    }
                },
                postfix = mirroredDevice => {
                    // flip info screen (aesthetics)
                    FlipPhaseChangeScreenTransform(mirroredDevice.FindTransform("ScreenNoShadow"));
                    // restore increase/decrease buttons position on setting wheel
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderButton1Trigger"));
                    FlipTransform(mirroredDevice.FindTransform("BoxColliderButton2Trigger"));
                    // fix switch collider display
                    FlipTransform(mirroredDevice.FindTransform("SwitchOnOff"));
                }
            },
            new MirrorDefinition("StructureLargeDirectHeatExchangeGastoLiquid") {
                postfix = mirroredDevice => {
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("PhysicalInfoPannel"));
                }
            },
            new MirrorDefinition("StructureSuperLargeDirectHeatExchangeGastoLiquid") {
                postfix = mirroredDevice => {
                    // flip info screen (aesthetics)
                    FlipTransform(mirroredDevice.FindTransform("PhysicalInfoPannel"));
                }
            },
            new MirrorDefinition("StructureSorter") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Output
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Output2
                    }
                },
                postfix = mirroredDevice => {
                    // fix switch collider display
                    FlipTransform(mirroredDevice.transform.Find("SwitchOnOff"));
                    FlipTransform(mirroredDevice.transform.Find("ImportChuteBin/BinImport/ImportSlot"));
                    FlipTransform(mirroredDevice.transform.Find("ExportChuteBin/ExportSlot"));
                    FlipTransform(mirroredDevice.transform.Find("Export2ChuteBin/Export2Slot"));
                    FlipTransform(mirroredDevice.transform.Find("BoxColliderTriggerEntry"));
                    FlipTransform(mirroredDevice.transform.Find("BoxColliderSlot4TriggerTypeDataDisk"));
                }
            },
            new MirrorDefinition("StructureLogicSorter") {
                connectionsToFlip = new[]
                {
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Input
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Output
                    },
                    new ConnectionDescription()
                    {
                        Type = NetworkType.Chute,
                        Role = ConnectionRole.Output2
                    }
                },
                postfix = mirroredDevice => {
                    // fix switch collider display
                    FlipTransform(mirroredDevice.transform.Find("SwitchOnOff"));
                    FlipTransform(mirroredDevice.transform.Find("ImportChuteBin/BinImport/ImportSlot"));
                    FlipTransform(mirroredDevice.transform.Find("ExportChuteBin/ExportSlot"));
                    FlipTransform(mirroredDevice.transform.Find("Export2ChuteBin/Export2Slot"));
                    FlipTransform(mirroredDevice.transform.Find("BoxColliderTriggerEntry"));
                    FlipTransform(mirroredDevice.transform.Find("BoxColliderSlot4TriggerTypeDataDisk"));
                }
            },
        };

        // permanent hidden object to store the new prefabs we will create
        private static readonly GameObject HiddenParent = new GameObject("~HiddenGameObject");

        [HarmonyPatch(typeof(InventoryManager), "SetupConstructionCursors")]
        [HarmonyPostfix]
        [UsedImplicitly]
        static private void MirrorOpenEnds(Dictionary<string, Structure> ____constructionCursors)
        {
            foreach (var mirrorDef in atmoMirrorDefs)
            {
                if (____constructionCursors.TryGetValue(mirrorDef.mirrorName, out Structure structure))
                {
                    var mirroredDevice = structure.GetComponent<Thing>();
                    if (mirroredDevice != null)
                    {
                        SmallGrid smGrid = mirroredDevice.GetComponent<SmallGrid>();
                        foreach (Connection conn in smGrid.OpenEnds)
                        {
                            if (mirrorDef.connectionsToFlip.Any(
                                connDesc =>
                                    connDesc.Type == conn.ConnectionType
                                    && connDesc.Role == conn.ConnectionRole
                                ))
                            {
                                if (conn.HelperRenderer != null)
                                {
                                    conn.HelperRenderer.gameObject.transform.Rotate(0, 180, 0);
                                }
                                else
                                {
                                    Log($"Error: can't find HelperRenderer on {conn.ConnectionType} {conn.ConnectionRole} connection of {mirrorDef.mirrorName}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Log("No thing on structure");
                    }
                }
                else
                {
                    Log($"{mirrorDef.mirrorName}: NOT FOUND");
                }
            }
        }

        [HarmonyPatch(typeof(Prefab), "LoadAll")]
        [HarmonyPrefix]
        [UsedImplicitly]
        static private void LoadMirrorPrefabs()
        {
            // Init prefab parent
            UnityEngine.Object.DontDestroyOnLoad(HiddenParent.gameObject);
            HiddenParent.SetActive(value: false);

            FindMirrorInfos();

            Log("Mirroring devices...");
            foreach (var mirrorDef in atmoMirrorDefs)
            {
                if (mirrorDef.deviceToMirror == null)
                {
                    Log($"Error: device not found {mirrorDef.deviceName}");
                }
                else
                {
                    MirrorAtmosphericDevice(mirrorDef);
                }
            }

            Log("All done");
        }

        static private void FindMirrorInfos()
        {
            // prefilter data for load time optimization and find mirror informations
            WorldManager.Instance.SourcePrefabs.ForEach(thing =>
            {
                if (thing == null)
                {
                    return;
                }
                var ctor = thing.GetComponent<MultiConstructor>();
                if (ctor != null && ctor.Constructables != null)
                {
                    foreach (var mirrorDef in atmoMirrorDefs)
                    {
                        if (ctor.Constructables.Find(p => p != null && p.name == mirrorDef.deviceName) != null)
                        {
                            mirrorDef.constructor = ctor;
                            // don't break, a constructor can have multiple devices to mirror
                        }
                    }
                }
                else if (thing.gameObject != null)
                {
                    foreach (var mirrorDef in atmoMirrorDefs)
                    {
                        if (thing.name == mirrorDef.deviceName)
                        {
                            mirrorDef.deviceToMirror = thing;
                            break;
                        }
                    }
                }
            });
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
            AddToConstructor(mirrorDef, mirroredDevice);

            // run mirror specific postfix
            mirrorDef.postfix(mirroredDevice);
        }

        static private void AddToConstructor(MirrorDefinition mirrorDef, Thing mirroredDevice)
        {
            if (mirrorDef.constructor != null)
            {
                int insertIndex = mirrorDef.constructor.Constructables.FindIndex(p => p.name == mirrorDef.deviceName);
                Structure newStruct = mirroredDevice as Structure;
                mirrorDef.constructor.Constructables.Insert(insertIndex + 1, mirroredDevice as Structure);
            }
            else
            {
                Log($"No constructor for device {mirrorDef.deviceName}");
            }
        }

        static private Thing CreateMirroredThing(MirrorDefinition mirrorDef)
        {
            var device = mirrorDef.deviceToMirror;

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
                // Flip wireframe
                FlipWireframe(blueprintWireframe);
            }

            /// Add to the game as an asset
            WorldManager.Instance.SourcePrefabs.Add(mirroredThing);

            return mirroredThing;
        }
        private static void FlipTransform(Transform transform)
        {
            // mirror the object using scale X -1
            transform.localScale = new Vector3(-1, 1, 1);
        }

        private static void FlipWireframe(Wireframe wireframe)
        {
            wireframe?.WireframeEdges.ForEach(edge =>
            {
                edge.Point1 = new Vector3(-edge.Point1.x, edge.Point1.y, edge.Point1.z);
                edge.Point2 = new Vector3(-edge.Point2.x, edge.Point2.y, edge.Point2.z);
            });
        }

        private static void FlipPhaseChangeScreenTransform(Transform screen)
        {
            FlipTransform(screen);
            screen.localPosition = new Vector3(-.49f, screen.position.y, screen.position.z);
        }

        [HarmonyPatch(typeof(Localization.LanguageFolder), nameof(Localization.LanguageFolder.LoadAll)), HarmonyPrefix]
        private static void Localization_LanguageFolder_LoadAll_Prefix(Localization.LanguageFolder __instance)
        {
            if (__instance.Code != LanguageCode.EN) return;

            foreach (var mirrorDef in atmoMirrorDefs)
            {
                var originalName = __instance.LanguagePages[0].Things.Find(x => x.Key == mirrorDef.deviceName)?.Value;
                if (originalName == null)
                {
                    originalName = "Missing name";
                    Log($"Missing display name for {mirrorDef.deviceName}");
                }
                __instance.LanguagePages[0].Things.Add(new Localization.RecordThing
                {
                    Key = mirrorDef.mirrorName,
                    Value = $"{originalName} (Mirrored)",
                    ThingDescription =
                    mirrorDef.mirrorDescription
                });
            }
        }
    }


}

