﻿using System;
using System.Collections.Generic;
using System.IO;
using FF8Mod.Archive;

namespace FF8Mod
{
    public class MessageFile
    {
        public List<string> Messages;

        public MessageFile()
        {
            Messages = new List<string>();
        }

        public static MessageFile FromBytes(byte[] data)
        {
            var result = new MessageFile();
            if (data.Length == 0) return result;

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                // strings are variable length so the header contains offsets for each
                var positions = new List<uint>() { reader.ReadUInt32() };
                while (stream.Position < positions[0])
                {
                    positions.Add(reader.ReadUInt32());
                }

                // eof
                positions.Add((uint)stream.Length);

                // read & decode strings
                for (int i = 1; i < positions.Count; i++)
                {
                    var messageData = reader.ReadBytes((int)positions[i] - (int)stream.Position);
                    result.Messages.Add(FF8String.Decode(messageData));
                }
            }

            return result;
        }

        public static MessageFile FromSource(FileSource source, string path)
        {
            return FromBytes(source.GetFile(path));
        }

        public byte[] Encode()
        {
            // encode
            var encodedMessages = new List<byte[]>();
            foreach (var m in Messages) encodedMessages.Add(FF8String.Encode(m));

            // write header
            var result = new List<byte>();
            var position = Messages.Count * 4;
            foreach (var e in encodedMessages)
            {
                result.AddRange(BitConverter.GetBytes(position));
                position += e.Length;
            }

            // write messages
            foreach (var e in encodedMessages)
            {
                result.AddRange(e);
            }

            return result.ToArray();
        }
    }
}
