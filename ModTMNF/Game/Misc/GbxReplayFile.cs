using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace ModTMNF.Game
{
    // TODO: Just invoke the native C++ serialization functions and grab the data we want...

    // NOTE: Class ids were implemented as they were seen. It's likely this will fail on many replays in its current form.

    // https://wiki.xaseco.org/wiki/GBX
    // https://github.com/LuksTrackmaniaCorner/1toOne-Converter/blob/Release_1.0/1toOne%20Converter/src/gbx/GBXFile.cs
    public class GbxReplayFile
    {
        private bool seenlookback = false;
        private List<string> stringStore = new List<string>();// TODO: Make this non-static (need to make everything non-static for this)
        static ushort[] supportedVersions = new ushort[] { 6 };

        public List<InputEvent> InputEvents = new List<InputEvent>();
        public InputEvent[] ActiveInputEvents;
        public int ActiveInputEventIndex;

        public string FilePath;

        public int GetValue(InputType type)
        {
            InputEvent inputEvent = ActiveInputEvents[(int)type];
            if (inputEvent != null)
            {
                return inputEvent.Enabled ? 1 : 0;
            }
            return 0;
        }

        public float GetValueF(InputType type)
        {
            InputEvent inputEvent = ActiveInputEvents[(int)type];
            if (inputEvent != null)
            {
                return inputEvent.AnalogValueF_Alt;
            }
            return 0;
        }

        public static GbxReplayFile Load(string path)
        {
            string error;
            return Load(path, out error);
        }

        public static GbxReplayFile Load(string path, out string error)
        {   
            GbxReplayFile result = new GbxReplayFile();
            result.FilePath = path;
            result.ActiveInputEvents = new InputEvent[Enum.GetValues(typeof(InputType)).Length];
            if (!result.LoadImpl(path, out error))
            {
                return null;
            }
            foreach (InputEvent ie in result.InputEvents)
            {
                //if (ie.Type == InputType.Steer)
                {
                    Debug.WriteLine(ie.Time + " - " + ie.Type + " " + ie.RawValue + " " + ie.AnalogValueF + " - " + (ie.AnalogValueF_Alt == ie.AnalogValueF));
                }
            }
            return result;
        }

        private bool LoadImpl(string path, out string error)
        {
            seenlookback = false;
            if (!File.Exists(path))
            {
                error = "Couldn't find file '" + path + "'";
                return false;
            }
            GbxReplayFile result = new GbxReplayFile();
            byte[] bodyBuffer = null;
            List<Node> nodeTable = new List<Node>();
            Node rootNode = new Node();
            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                string magic = new string(br.ReadChars(3));
                if (magic != "GBX")
                {
                    error = "Invalid header magic. Expected to find 'GBX'.";
                    return false;
                }
                ushort version = br.ReadUInt16();
                if (!supportedVersions.Contains(version))
                {
                    error = "Unsupported GBX version " + version;
                    return false;
                }
                if (version >= 3)
                {
                    char byteFormatChar = br.ReadChar();
                    if (byteFormatChar != 'B')
                    {
                        error = "TODO: Support " + byteFormatChar + " format";
                        return false;       
                    }
                    char refTableCompression = br.ReadChar();
                    char bodyCompression = br.ReadChar();
                    if (version >= 4)
                    {
                        br.ReadChar();//R/E
                    }
                    rootNode.ClassId = (EMwClassId)br.ReadInt32();
                    switch (rootNode.ClassId)
                    {
                        case EMwClassId.CGameCtnReplayRecord:
                        case EMwClassId.CTrackManiaReplayRecord:
                            break;
                        default:
                            error = "Not handling " + rootNode.ClassId + " as main entry.";
                            return false;
                    }
                    if (version >= 6)
                    {
                        int userDataSize = br.ReadInt32();
                        if (userDataSize > 0)
                        {
                            int numHeaderChunks = br.ReadInt32();
                            long totalHeaderChunksSize = 0;
                            for (int i = 0; i < numHeaderChunks; i++)
                            {
                                uint chunkId = br.ReadUInt32();
                                uint chunkSize = br.ReadUInt32();
                                if ((chunkSize & 0x80000000) != 0)
                                {
                                    chunkSize = chunkSize & 0x7FFFFFFF;
                                }
                                totalHeaderChunksSize += chunkSize;
                            }
                            br.BaseStream.Position += totalHeaderChunksSize;
                            uint numNodes = br.ReadUInt32();
                        }
                    }

                    // Ref table
                    uint numExternalNodes = br.ReadUInt32();
                    if (numExternalNodes > 0)
                    {
                        int ancestorLevel = br.ReadInt32();
                        ReadRefTableFolders(br, isRoot: true);
                        for (int i = 0; i < numExternalNodes; i++)
                        {
                            uint flags = br.ReadUInt32();
                            if ((flags & 4) == 0)
                            {
                                ReadString(br);//fileName
                            }
                            else
                            {
                                br.ReadUInt32();//resourceIndex
                            }
                            br.ReadUInt32();//nodeIndex
                            if (version >= 5)
                            {
                                ReadBool(br);//useFile
                            }
                            if ((flags & 4) == 0)
                            {
                                br.ReadUInt32();//folderIndex
                            }
                        }
                    }

                    // Body
                    if (bodyCompression == 'C')
                    {
                        uint uncompressedSize = br.ReadUInt32();
                        uint compressedSize = br.ReadUInt32();
                        bodyBuffer = br.ReadBytes((int)compressedSize);
                        Debug.Assert(br.BaseStream.Position == br.BaseStream.Length);
                        Debug.Assert(uncompressedSize > 0 && compressedSize > 0);

                        using (var stream = new LzoStream(new MemoryStream(bodyBuffer), CompressionMode.Decompress))
                        {
                            bodyBuffer = new byte[uncompressedSize];
                            int read = stream.Read(bodyBuffer, 0, (int)uncompressedSize);
                            Debug.Assert(read == uncompressedSize);
                        }
                    }
                    else
                    {
                        bodyBuffer = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    }
                }
            }
            
            // Body (continued)
            using (BinaryReader br = new BinaryReader(new MemoryStream(bodyBuffer)))
            {
                rootNode.BodyOffset = br.BaseStream.Position;
                error = null;
                if (!ReadNode(br, rootNode, nodeTable, ref error))
                {
                    return false;
                }
                uint lastChunkId = br.ReadUInt32();
                Debug.Assert(lastChunkId == 0xFACADE01);
                Debug.Assert(br.BaseStream.Position == br.BaseStream.Length);
            }

            error = null;
            return true;
        }

        private bool ReadNode(BinaryReader br, Node node, List<Node> nodeTable, ref string error)
        {
            while (br.BaseStream.Position < br.BaseStream.Length)
            {
                uint chunkId = br.ReadUInt32();
                if (chunkId == 0xFACADE01)
                {
                    break;
                }

                int skipSize = -1;
                uint peek = br.ReadUInt32();
                if (br.BaseStream.Position < br.BaseStream.Length - 4)
                {
                    if (peek == 0x534B4950)
                    {
                        skipSize = br.ReadInt32();
                    }
                    else
                    {
                        br.BaseStream.Position -= 4;
                    }
                }
                switch (chunkId)
                {
                    //////////////////////////////////////
                    // CGameCtnReplayRecord
                    //////////////////////////////////////
                    case 0x03093002:// Track
                        {
                            int size = br.ReadInt32();
                            if (size > 0)
                            {
                                byte[] trackGbx = br.ReadBytes(size);
                            }
                        }
                        break;
                    case 0x03093007:
                        br.ReadUInt32();
                        break;
                    case 0x03093014:// Ghosts
                        {
                            br.ReadInt32();
                            int numGhosts = br.ReadInt32();
                            for (int i = 0; i < numGhosts; i++)
                            {
                                if (!ReadNodeRef(br, node, nodeTable, ref error))
                                {
                                    return false;
                                }
                            }
                            br.ReadInt32();
                            int numExtra = br.ReadInt32();
                            for (int i = 0; i < numExtra; i++)
                            {
                                br.ReadInt64();
                            }
                        }
                        break;
                    case 0x03093015:// clip
                        {
                            // TODO... though we should have input data by now so who cares
                            if (InputEvents.Count == 0)
                            {
                                error = "TODO: Handle clip data";
                                return false;
                            }
                            return true;
                        }
                    //////////////////////////////////////
                    // CGameCtnGhost
                    //////////////////////////////////////
                    case 0x03092005:
                        {
                            // race time
                            uint raceTime = br.ReadUInt32();
                        }
                        break;
                    case 0x03092008:
                        {
                            // num respawns
                            uint respawns = br.ReadUInt32();
                        }
                        break;
                    case 0x03092009:
                        {
                            // light trail color
                            float u1 = br.ReadSingle();
                            float u2 = br.ReadSingle();
                            float u3 = br.ReadSingle();
                        }
                        break;
                    case 0x0309200A:
                        {
                            // stunt store
                            uint score = br.ReadUInt32();
                        }
                        break;
                    case 0x0309200B:
                        {
                            // cp times
                            int count = br.ReadInt32();
                            for (int i = 0; i < count; i++)
                            {
                                uint time = br.ReadUInt32();
                                br.ReadInt32();
                            }
                        }
                        break;
                    case 0x0309200C:
                        br.ReadInt32();
                        break;
                    case 0x309200E:
                        ReadLookbackString(br);
                        break;
                    case 0x0309200F:
                        {
                            string login = ReadString(br);
                        }
                        break;
                    case 0x03092010:
                        ReadLookbackString(br);
                        break;
                    case 0x03092012:
                        br.ReadInt32();
                        br.ReadInt64();
                        br.ReadInt64();
                        break;
                    case 0x03092013:
                        br.ReadInt32();
                        br.ReadInt32();
                        break;
                    case 0x03092014:
                        br.ReadInt32();
                        break;
                    case 0x03092015:
                        ReadLookbackString(br);
                        break;
                    case 0x03092017:
                        // Skin pack descs / ghost nick / ghost avatar name
                        Debug.Assert(skipSize > 0);
                        br.BaseStream.Position += skipSize;
                        break;
                    case 0x03092018:
                        ReadLookbackString(br);
                        ReadLookbackString(br);
                        ReadLookbackString(br);
                        break;
                    case 0x03092019:
                        {
                            uint eventsDuration = br.ReadUInt32();
                            if (eventsDuration != 0)
                            {
                                br.ReadInt32();
                                int numNames = br.ReadInt32();
                                string[] controlNames = new string[numNames];
                                for (int i = 0; i < numNames; i++)
                                {
                                    string name = ReadLookbackString(br);
                                    controlNames[i] = name;
                                }
                                if (numNames > 0)
                                {
                                    int numEntries = br.ReadInt32();
                                    br.ReadInt32();
                                    for (int i = 0; i < numEntries; i++)
                                    {
                                        uint time = br.ReadUInt32();
                                        byte controlNameIndex = br.ReadByte();
                                        string name = controlNames[controlNameIndex];
                                        uint rawValue = br.ReadUInt32();
                                        if (time < 100000)
                                        {
                                            // The extra time is for respawn? (which gives the countdown?)
                                            time = 0;
                                        }
                                        else
                                        {
                                            time -= 100000;// Every replay has a base time of 100000 for some reason?
                                        }
                                        InputEvents.Add(new InputEvent(name, time, rawValue));
                                    }
                                    string exeVersion = ReadString(br);
                                    int exeChecksum = br.ReadInt32();
                                    int osKind = br.ReadInt32();
                                    int cpuKind = br.ReadInt32();
                                    string raceSettings = ReadString(br);
                                    br.ReadInt32();
                                }
                            }
                        }
                        break;
                    //////////////////////////////////////
                    // CGameGhost
                    //////////////////////////////////////
                    case 0x0303F005:// Ghost data
                        {
                            InputEvents.Clear();
                            uint uncompressedSize = br.ReadUInt32();
                            uint compressedSize = br.ReadUInt32();
                            byte[] ghostBuffer = br.ReadBytes((int)compressedSize);
                            ghostBuffer = Decompress(ghostBuffer);
                            using (BinaryReader br2 = new BinaryReader(new MemoryStream(ghostBuffer)))
                            {
                                EMwClassId cid = (EMwClassId)br2.ReadUInt32();//CSceneVehicleCar / CSceneMobilCharVis
                                if (cid != (EMwClassId)uint.MaxValue)
                                {
                                    bool skipList2 = ReadBool(br2);
                                    br2.ReadInt32();
                                    uint samplePeriod = br2.ReadUInt32();
                                    Debug.Assert(samplePeriod == 100);// Sample period defined by CMwTimer? Should always be 100?
                                    br2.ReadInt32();
                                    int sampleDataLen = br2.ReadInt32();
                                    byte[] sampleData = br2.ReadBytes(sampleDataLen);
                                    uint numSamples = br2.ReadUInt32();
                                    int sizePerSample = -1;
                                    int[] sampleSizes = null;
                                    uint[] sampleTimes = null;
                                    if (numSamples > 0)
                                    {
                                        uint firstSampleOffset = br2.ReadUInt32();
                                        if (numSamples > 1)
                                        {
                                            sizePerSample = br2.ReadInt32();
                                            if (sizePerSample == -1)
                                            {
                                                sampleSizes = new int[numSamples - 1];
                                                for (int i = 0; i < sampleSizes.Length; i++)
                                                {
                                                    sampleSizes[i] = br2.ReadInt32();
                                                }
                                            }
                                        }
                                    }
                                    if (!skipList2)
                                    {
                                        int numSampleTimes = br2.ReadInt32();
                                        sampleTimes = new uint[numSampleTimes];
                                        for (int i = 0; i < numSampleTimes; i++)
                                        {
                                            sampleTimes[i] = br2.ReadUInt32();
                                        }
                                    }
                                    if (numSamples > 0)
                                    {
                                        // TODO: Read sampleData
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        error = "TODO: Handle chunk id " + chunkId.ToString("X8");
                        Debug.Assert(false);
                        return false;
                }
            }
            return true;
        }

        private bool ReadNodeRef(BinaryReader br, Node node, List<Node> nodeTable, ref string error)
        {
            // NOTE: I don't think the whole relationship (i.e. childs etc) stuff here we're doing makes any sense.
            int index = br.ReadInt32() - 1;
            if (index >= 0)
            {
                if (index < nodeTable.Count)
                {
                    error = "TODO: refetch ref";
                    //node.Children.Add(nodeTable[index]);
                    return false;
                }
                else
                {
                    EMwClassId classId = (EMwClassId)br.ReadInt32();
                    Node childNode = new Node();
                    childNode.BodyOffset = br.BaseStream.Position;
                    childNode.ClassId = classId;
                    node.Children.Add(childNode);
                    if (!ReadNode(br, childNode, nodeTable, ref error))
                    {
                        return false;
                    }
                }
            }
            else
            {
                error = "TODO: -1 node ref";
                return false;
            }
            return true;
        }

        private void ReadRefTableFolders(BinaryReader br, bool isRoot)
        {
            if (isRoot)
            {
                ReadString(br);//name
            }
            int numSubFolders = br.ReadInt32();
            for (int i = 0; i < numSubFolders; i++)
            {
                ReadRefTableFolders(br, isRoot: false);
            }
        }

        private static BOOL ReadBool(BinaryReader br)
        {
            return br.ReadInt32() != 0;
        }

        private string ReadString(BinaryReader br)
        {
            return Encoding.UTF8.GetString(br.ReadBytes(br.ReadInt32()));
        }

        private string ReadLookbackString(BinaryReader br)
        {
            if (!seenlookback)
            {
                br.ReadUInt32();
            }
            seenlookback = true;
            uint index = br.ReadUInt32();
            if ((index & 0x3FFF) == 0 && (index >> 30 == 1 || index >> 30 == 2))
            {
                string result = ReadString(br);
                stringStore.Add(result);
                return result;
            }
            else if ((index & 0x3FFF) == 0x3FFF)
            {
                return null;
            }
            else if (index >> 30 == 0)
            {
                return null;
            }
            else if (stringStore.Count > (index & 0x3FFF) - 1)
            {
                return stringStore[(int)(index & 0x3FFF) - 1];
            }
            return null;
        }

        public class Node
        {
            public long BodyOffset;
            public EMwClassId ClassId;
            public List<Node> Children = new List<Node>();
        }

        public class InputEvent
        {
            public uint Time;
            public InputType Type;
            public uint RawValue;

            public bool Enabled
            {
                get { return (RawValue & 1) != 0; }
            }

            /// <summary>
            /// See CInputEventsStore::Convert (there are multiple of these)
            /// </summary>
            public int AnalogValue
            {
                get
                {
                    int val = (int)RawValue;
                    val <<= 8;
                    val >>= 8;
                    return val;
                }
            }

            /// <summary>
            /// See CInputEventsStore::Convert (there are multiple of these)
            /// 
            /// 65536.0
            /// 0.0000152587890625
            /// </summary>
            public float AnalogValueF
            {
                get { return ((float)AnalogValue / (ushort.MaxValue + 1)); }
            }

            /// <summary>
            /// Alternative floating point conversion (should be the same)
            /// </summary>
            public float AnalogValueF_Alt
            {
                get { return (float)((double)AnalogValue * 0.0000152587890625); }
            }

            public InputEvent(string name, uint time, uint rawValue)
            {
                RawValue = rawValue;
                Time = time;
                switch (name)
                {
                    case "Accelerate":
                    case "AccelerateReal":
                        Type = InputType.Accelerate;
                        break;
                    case "SteerLeft":
                        Type = InputType.SteerLeft;
                        break;
                    case "SteerRight":
                        Type = InputType.SteerRight;
                        break;
                    case "Brake":
                    case "BrakeReal":
                        Type = InputType.Brake;
                        break;
                    case "Respawn":
                        Type = InputType.Respawn;
                        break;
                    case "Steer":
                        Type = InputType.Steer;
                        break;
                    case "Gas":
                        Type = InputType.Gas;
                        break;
                    case "Horn":
                        Type = InputType.Horn;
                        break;
                }
            }
        }

        public enum InputType
        {
            Accelerate,
            Brake,
            Respawn,
            Horn,
            SteerLeft,
            SteerRight,
            Steer,
            Gas,
        }

        static byte[] Decompress(byte[] compressedBuffer, int skip = 2)
        {
            using (MemoryStream memStream = new MemoryStream())
            using (MemoryStream result = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(result))
            {
                memStream.Write(compressedBuffer, skip, compressedBuffer.Length - skip);
                byte[] buffer = new byte[ushort.MaxValue];
                memStream.Position = 0;
                using (System.IO.Compression.DeflateStream zip = new System.IO.Compression.DeflateStream(memStream, System.IO.Compression.CompressionMode.Decompress))
                {
                    int totalRead = 0;
                    int read = 0;
                    while ((read = zip.Read(buffer, 0, 1024)) > 0)
                    {
                        totalRead += read;
                        byte[] blockSize = new byte[read];
                        Buffer.BlockCopy(buffer, 0, blockSize, 0, blockSize.Length);
                        writer.Write(blockSize);
                    }
                }
                return result.ToArray();
            }
        }

        // https://github.com/zivillian/lzo.net/tree/ace084a4ea320b40f5a414755b1cb588cca6ea7c
        // Slightly minified to reduce code bloat in this file
        public class LzoStream : Stream{protected readonly Stream Source;private long? _length;private readonly bool _leaveOpen;protected byte[] DecodedBuffer;protected const int MaxWindowSize = (1 << 14) + ((255 & 8) << 11) + (255 << 6) + (255 >> 2);protected RingBuffer RingBuffer = new RingBuffer(MaxWindowSize);protected long OutputPosition;protected int Instruction;protected LzoState State;protected enum LzoState{ZeroCopy = 0,SmallCopy1 = 1,SmallCopy2 = 2,SmallCopy3 = 3,LargeCopy = 4}public LzoStream(Stream stream, CompressionMode mode): this(stream, mode, false) { }public LzoStream(Stream stream, CompressionMode mode, bool leaveOpen){if (mode != CompressionMode.Decompress)throw new NotSupportedException("Compression is not supported");if (!stream.CanRead)throw new ArgumentException("write-only stream cannot be used for decompression");Source = stream;if (!(stream is BufferedStream))Source = new BufferedStream(stream);_leaveOpen = leaveOpen;DecodeFirstByte();}private void DecodeFirstByte(){Instruction = Source.ReadByte();if (Instruction == -1)throw new EndOfStreamException();if (Instruction > 15 && Instruction <= 17){throw new Exception();}}private void Copy(byte[] buffer, int offset, int count){Debug.Assert(count > 0);do{var read = Source.Read(buffer, offset, count);if (read == 0)throw new EndOfStreamException();RingBuffer.Write(buffer, offset, read);offset += read;count -= read;} while (count > 0);}protected virtual int Decode(byte[] buffer, int offset, int count){Debug.Assert(count > 0);Debug.Assert(DecodedBuffer == null);int read;var i = Instruction >> 4;switch (i){case 0:{switch (State){case LzoState.ZeroCopy:{var length = 3;if (Instruction != 0){length += Instruction;}else{length += 15 + ReadLength();}State = LzoState.LargeCopy;if (length <= count){Copy(buffer, offset, length);read = length;}else{Copy(buffer, offset, count);DecodedBuffer = new byte[length - count];Copy(DecodedBuffer, 0, length - count);read = count;}break;}case LzoState.SmallCopy1:case LzoState.SmallCopy2:case LzoState.SmallCopy3:read = SmallCopy(buffer, offset, count);break;case LzoState.LargeCopy:read = LargeCopy(buffer, offset, count);break;default:throw new ArgumentOutOfRangeException();}break;}case 1:{int length = (Instruction & 0x7) + 2;if (length == 2){length += 7 + ReadLength();}var s = Source.ReadByte();var d = Source.ReadByte();if (s != -1 && d != -1){d = ((d << 8) | s) >> 2;var distance = 16384 + ((Instruction & 0x8) << 11) | d;if (distance == 16384)return -1;read = CopyFromRingBuffer(buffer, offset, count, distance, length, s & 0x3);break;}throw new EndOfStreamException();}case 2:case 3:{int length = (Instruction & 0x1f) + 2;if (length == 2){length += 31 + ReadLength();}var s = Source.ReadByte();var d = Source.ReadByte();if (s != -1 && d != -1){d = ((d << 8) | s) >> 2;var distance = d + 1;read = CopyFromRingBuffer(buffer, offset, count, distance, length, s & 0x3);break;}throw new EndOfStreamException();}case 4:case 5:case 6:case 7:{var length = 3 + ((Instruction >> 5) & 0x1);var result = Source.ReadByte();if (result != -1){var distance = (result << 3) + ((Instruction >> 2) & 0x7) + 1;read = CopyFromRingBuffer(buffer, offset, count, distance, length, Instruction & 0x3);break;}throw new EndOfStreamException();}default:{var length = 5 + ((Instruction >> 5) & 0x3);var result = Source.ReadByte();if (result != -1){var distance = (result << 3) + ((Instruction & 0x1c) >> 2) + 1;read = CopyFromRingBuffer(buffer, offset, count, distance, length, Instruction & 0x3);break;}throw new EndOfStreamException();}}Instruction = Source.ReadByte();if (Instruction != -1){OutputPosition += read;return read;}throw new EndOfStreamException();}private int LargeCopy(byte[] buffer, int offset, int count){var result = Source.ReadByte();if (result != -1){var distance = (result << 2) + ((Instruction & 0xc) >> 2) + 2049;return CopyFromRingBuffer(buffer, offset, count, distance, 3, Instruction & 0x3);}throw new EndOfStreamException();}private int SmallCopy(byte[] buffer, int offset, int count){var h = Source.ReadByte();if (h != -1){var distance = (h << 2) + ((Instruction & 0xc) >> 2) + 1;return CopyFromRingBuffer(buffer, offset, count, distance, 2, Instruction & 0x3);}throw new EndOfStreamException();}private int ReadLength(){int b;int length = 0;while ((b = Source.ReadByte()) == 0){if (length >= Int32.MaxValue - 1000){throw new Exception();}length += 255;}if (b != -1) return length + b;throw new EndOfStreamException();}private int CopyFromRingBuffer(byte[] buffer, int offset, int count, int distance, int copy, int state){Debug.Assert(copy >= 0);var result = copy + state;State = (LzoState)state;if (count >= result){var size = copy;if (copy > distance){size = distance;RingBuffer.Copy(buffer, offset, distance, size);copy -= size;var copies = copy / distance;for (int i = 0; i < copies; i++){Buffer.BlockCopy(buffer, offset, buffer, offset + size, size);offset += size;copy -= size;}if (copies > 0){var length = size * copies;RingBuffer.Write(buffer, offset - length, length);}offset += size;}if (copy > 0){if (copy < size)size = copy;RingBuffer.Copy(buffer, offset, distance, size);offset += size;}if (state > 0){Copy(buffer, offset, state);}return result;}if (count <= copy){CopyFromRingBuffer(buffer, offset, count, distance, count, 0);DecodedBuffer = new byte[result - count];CopyFromRingBuffer(DecodedBuffer, 0, DecodedBuffer.Length, distance, copy - count, state);return count;}CopyFromRingBuffer(buffer, offset, count, distance, copy, 0);var remaining = count - copy;DecodedBuffer = new byte[state - remaining];Copy(buffer, offset + copy, remaining);Copy(DecodedBuffer, 0, state - remaining);return count;}private int ReadInternal(byte[] buffer, int offset, int count){Debug.Assert(count > 0);if (_length.HasValue && OutputPosition >= _length)return -1;int read;if (DecodedBuffer == null){if ((read = Decode(buffer, offset, count)) >= 0) return read;_length = OutputPosition;return -1;}var decodedLength = DecodedBuffer.Length;if (count > decodedLength){Buffer.BlockCopy(DecodedBuffer, 0, buffer, offset, decodedLength);DecodedBuffer = null;OutputPosition += decodedLength;return decodedLength;}Buffer.BlockCopy(DecodedBuffer, 0, buffer, offset, count);if (decodedLength > count){var remaining = new byte[decodedLength - count];Buffer.BlockCopy(DecodedBuffer, count, remaining, 0, remaining.Length);DecodedBuffer = remaining;}else{DecodedBuffer = null;}OutputPosition += count;return count;}public override bool CanRead{get{return true;}}public override bool CanSeek{get{return false;}}public override bool CanWrite{get{return false;}}public override long Length{get{if (_length.HasValue)return _length.Value;throw new NotSupportedException();}}public override long Position{get{return OutputPosition;}set{if(OutputPosition == value) return;Seek(value, SeekOrigin.Begin);}}public override void Flush(){}public override int Read(byte[] buffer, int offset, int count) { if (_length.HasValue && OutputPosition >= _length)return 0; var result = 0; while (count > 0) { var read = ReadInternal(buffer, offset, count); if (read == -1)return result; result += read; offset += read; count -= read; } return result; }public override long Seek(long offset, SeekOrigin origin){throw new NotImplementedException();}public override void SetLength(long value){_length = value;}public override void Write(byte[] buffer,int offset,int count){throw new InvalidOperationException("cannot write to readonly stream");}protected override void Dispose(bool disposing){if(!_leaveOpen)Source.Dispose();base.Dispose(disposing);}}
        public class RingBuffer{private readonly byte[] _buffer;private int _position;private readonly int _size;public RingBuffer(int size){_buffer = new byte[size];_size = size;}public void Seek(int offset){_position += offset;if (_position > _size){do{_position -= _size;} while (_position > _size);return;}while (_position < 0){_position += _size;}}public void Copy(byte[] buffer, int offset, int distance, int count){if (_position - distance > 0 && _position + count < _size){if (count < 10){do{var value = _buffer[_position - distance];_buffer[_position++] = value;buffer[offset++] = value;} while (--count > 0);}else{Buffer.BlockCopy(_buffer, _position - distance, buffer, offset, count);Buffer.BlockCopy(buffer, offset, _buffer, _position, count);_position += count;}}else{Seek(-distance);Read(buffer, offset, count);Seek(distance - count);Write(buffer, offset, count);}}public void Read(byte[] buffer, int offset, int count){if (count < 10 && (_position + count) < _size){do{buffer[offset++] = _buffer[_position++];} while (--count > 0);}else{while (count > 0){var copy = _size - _position;if (copy > count){Buffer.BlockCopy(_buffer, _position, buffer, offset, count);_position += count;break;}Buffer.BlockCopy(_buffer, _position, buffer, offset, copy);_position = 0;count -= copy;offset += copy;}}}public void Write(byte[] buffer, int offset, int count){if (count < 10 && (_position + count) < _size){do{_buffer[_position++] = buffer[offset++];} while (--count > 0);}else{while (count > 0){var cnt = _size - _position;if (cnt > count){Buffer.BlockCopy(buffer, offset, _buffer, _position, count);_position += count;return;}Buffer.BlockCopy(buffer, offset, _buffer, _position, cnt);_position = 0;offset += cnt;count -= cnt;}}}public RingBuffer Clone(){var result = new RingBuffer(_size) { _position = _position };Buffer.BlockCopy(_buffer, 0, result._buffer, 0, _size);return result;}}
    }
}
