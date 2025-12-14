using Assets.Scripts.Objects;
using Objects.Rockets;
using UnityEngine;


namespace MirroredAtmospherics.Scripts
{
    struct ConnectionDescription
    {
        public NetworkType Type;
        public ConnectionRole Role;
    }

    /// <summary>
    /// Used to describe a device mirroring to perform
    /// </summary>
    internal class MirrorDefinition
    {
        /// <summary>
        /// the identifier of the device we want to flip
        /// </summary>
        public string deviceName;


        public string mirrorName { get; private set; }
        public int mirrorHash { get; private set; }

        public string mirrorDescription;

        public ConnectionDescription[] connectionsToFlip = {};

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
            this.mirrorDescription = $"Mirrored version of the {{THING:{deviceName}}}";
        }
    }
}
