using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace MirroredAtmospherics.Scripts
{
    internal class WireframeStorage
    {
        private static readonly string WireframeDataFilePath = Application.persistentDataPath + "/mirroredWireframe.dat";
        private static BinaryFormatter _bin = new BinaryFormatter();

        [Serializable]
        public struct KeyValuePair<K, V>
        {
            public K Key { get; set; }
            public V Value { get; set; }
        }

        [Serializable]
        private class SerializableVector3
        {
            public float x;
            public float y;
            public float z;
            public static SerializableVector3 FromVector3(Vector3 vector)
            {
                return new SerializableVector3()
                {
                    x = vector.x,
                    y = vector.y,
                    z = vector.z
                };
            }

            public Vector3 ToVector3()
            {
                return new Vector3(this.x, this.y, this.z);
            }
        }

        [Serializable]
        private class SerializableEdge
        {
            public SerializableVector3 Point1;
            public SerializableVector3 Point2;

            public static SerializableEdge fromEdge(Edge edge)
            {
                return new SerializableEdge()
                {
                    Point1 = SerializableVector3.FromVector3(edge.Point1),
                    Point2 = SerializableVector3.FromVector3(edge.Point2)
                };
            }

            public Edge ToEdge()
            {
                return new Edge()
                {
                    Point1 = this.Point1.ToVector3(),
                    Point2 = this.Point2.ToVector3(),
                };
            }
        }

        public static Dictionary<string, List<Edge>> LoadWireframeData()
        {
            Dictionary<string, List<Edge>> data = new Dictionary<string, List<Edge>>();
            if (File.Exists(WireframeDataFilePath))
            {
                var file = File.OpenRead(WireframeDataFilePath);
                try
                {
                    var fileData = (KeyValuePair<string, List<SerializableEdge>>[])_bin.Deserialize(file);
                    data = ParseCache(fileData);
                }
                finally
                {
                    file.Close();
                }

            }
            return data;
        }

        static KeyValuePair<string, List<SerializableEdge>>[] FormatForSave(Dictionary<string, List<Edge>> data)
        {
            return data.Select((entry) =>
                new KeyValuePair<string, List<SerializableEdge>>()
                {
                    Key = entry.Key,
                    Value = entry.Value.Select(edge => SerializableEdge.fromEdge(edge)).ToList()
                }
            ).ToArray();
        }

        static Dictionary<string, List<Edge>> ParseCache(KeyValuePair<string, List<SerializableEdge>>[] cacheData)
        {
            return new Dictionary<string, List<Edge>>(
                cacheData.Select(entry =>
                    new System.Collections.Generic.KeyValuePair<string, List<Edge>>(
                        entry.Key,
                        entry.Value.Select(serializableEdge => serializableEdge.ToEdge()).ToList()
                    )
                )
            );
        }

        public static void SaveWireframeData(Dictionary<string, List<Edge>> data)
        {
            using (var file = File.OpenWrite(WireframeDataFilePath))
            {
                _bin.Serialize(file, FormatForSave(data));
                file.Close();
            }
        }
    }
}
