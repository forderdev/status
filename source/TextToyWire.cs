using System;
using Mirror;
using UnityEngine;
using BaseTextToy = AdminToys.TextToy;
using TextToy = LabApi.Features.Wrappers.TextToy;

namespace PlayerStatus
{
    internal static class TextToyWire
    {
        private const ulong PositionBit = 0x01;
        private const ulong RotationBit = 0x02;
        private const ulong ScaleBit = 0x04;
        private const ulong SmoothingBit = 0x08;
        private const ulong IsStaticBit = 0x10;
        private const ulong DisplaySizeBit = 0x20;
        private const ulong TextFormatBit = 0x40;
        private const ulong KnownBits = 0x7F;

        public static void SendPose(NetworkConnectionToClient connection, BaseTextToy toy, Quaternion localRotation, Vector3 localScale)
        {
            using (NetworkWriterPooled writer = NetworkWriterPool.Get())
            {
                Compression.CompressVarUInt(writer, 1UL << toy.ComponentIndex);

                int safetyPosition = writer.Position;
                writer.WriteByte(0);
                int start = writer.Position;

                const ulong dirty = RotationBit | ScaleBit;
                writer.WriteULong(0UL);
                writer.WriteULong(dirty);
                writer.WriteQuaternion(localRotation);
                writer.WriteVector3(localScale);
                writer.WriteULong(dirty);

                int end = writer.Position;
                writer.Position = safetyPosition;
                writer.WriteByte((byte)((end - start) & 0xFF));
                writer.Position = end;

                connection.Send(new EntityStateMessage { netId = toy.netId, payload = writer.ToArraySegment() });
            }
        }

        public static string Validate()
        {
            TextToy probe = null;

            try
            {
                probe = TextToy.Create(networkSpawn: false);
                BaseTextToy toy = probe.Base;

                if (toy.GetType() != typeof(BaseTextToy))
                    return $"unexpected component {toy.GetType().FullName}";

                Quaternion rotation = Quaternion.Euler(10f, 20f, 30f);
                Vector3 scale = new Vector3(0.25f, 0.5f, 0.75f);

                toy.NetworkRotation = rotation;
                toy.NetworkScale = scale;
                toy.TextFormat = "probe";

                using (NetworkWriterPooled writer = NetworkWriterPool.Get())
                {
                    toy.OnSerialize(writer, false);

                    using (NetworkReaderPooled reader = NetworkReaderPool.Get(writer.ToArraySegment()))
                    {
                        if (reader.ReadULong() != 0UL)
                            return "sync objects are dirty";

                        ulong first = reader.ReadULong();

                        if ((first & ~KnownBits) != 0)
                            return $"unknown bits 0x{first:X}";

                        if ((first & (RotationBit | ScaleBit | TextFormatBit)) != (RotationBit | ScaleBit | TextFormatBit))
                            return $"missing bits 0x{first:X}";

                        if ((first & PositionBit) != 0)
                            reader.ReadVector3();

                        if (reader.ReadQuaternion() != rotation)
                            return "rotation mismatch";

                        if (reader.ReadVector3() != scale)
                            return "scale mismatch";

                        if ((first & SmoothingBit) != 0)
                            reader.ReadByte();

                        if ((first & IsStaticBit) != 0)
                            reader.ReadBool();

                        if (reader.ReadULong() != first)
                            return "second mask mismatch";

                        if ((first & DisplaySizeBit) != 0)
                            reader.ReadVector2();

                        if (reader.ReadString() != "probe")
                            return "text mismatch";

                        if (reader.Remaining != 0)
                            return $"{reader.Remaining} extra byte(s)";
                    }
                }

                return null;
            }
            catch (Exception exception)
            {
                return $"{exception.GetType().Name}: {exception.Message}";
            }
            finally
            {
                if (probe != null && !probe.IsDestroyed)
                    UnityEngine.Object.Destroy(probe.GameObject);
            }
        }
    }
}
