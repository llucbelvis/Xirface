
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

using System.Text;



namespace Xirface
{
    public class Font
    {
        public Stream Stream;
        public Reader Reader;

        Dictionary<string, (uint offset, uint length)> TableLocation;

        public ushort glyphCount;
        public float unitsPerEm;

        public Dictionary<ushort, Glyph> IndexGlyphDict;
        public Dictionary<char, Glyph> CharacterGlyphDict;
        
        public Dictionary<(ushort left, ushort right), short> IndexKerningDict;
        
        uint[] GlyphLocationMap;
        private (uint unicode, ushort index)[] UnicodeIndexMap;
        private (ushort advanceWidth, short leftSideBearing)[] IndexMetricsMap;
        
        public Font(string path)
        {
            Debug.WriteLine($"_typo : loading font {path}");

            Debug.WriteLine(Environment.CurrentDirectory);


            Stream = File.Open(path, FileMode.Open);
            Reader = new(Stream);

            Reader.SkipBytes(4);

            UInt16 tableCount = Reader.ReadUInt16();

            Reader.SkipBytes(6);

            TableLocation = GetTables(tableCount);

            unitsPerEm = GetUnitsPerEm();
            glyphCount = GetGlyphCount();

            GlyphLocationMap = GetGlyphLocations();
            UnicodeIndexMap = GetUnicodeIndexMap();

            IndexMetricsMap = GetIndexMetrics();
            IndexKerningDict = GetIndexKerning();

            (IndexGlyphDict, CharacterGlyphDict) = GetAllGlyphs();

            Debug.WriteLine($"_typo : finished loading {path}");

            Stream.Close();
        }
        public uint GetUnitsPerEm()
        {
            Reader.GoTo(TableLocation["head"].offset);

            Reader.SkipBytes(18);

            return Reader.ReadUInt16();
        }

        (Dictionary<ushort, Glyph>, Dictionary<char, Glyph>) GetAllGlyphs()
        {
            Dictionary<char, Glyph> CharacterGlyphDict = new();
            Dictionary<ushort, Glyph> IndexGlyphDict = new();

            for (ushort i = 0; i < glyphCount; i++)

                IndexGlyphDict.Add(i, Glyph.GetSimpleGlyph(Reader, i, GlyphLocationMap, GlyphLocationMap[i], IndexMetricsMap[i].advanceWidth, IndexMetricsMap[i].leftSideBearing).Load());
            

            for (ushort i = 0; i < UnicodeIndexMap.Length; i++)
            
                CharacterGlyphDict.TryAdd((char)UnicodeIndexMap[i].unicode, IndexGlyphDict[UnicodeIndexMap[i].index]);
   
            return (IndexGlyphDict, CharacterGlyphDict);
        }

        Dictionary<string, (uint offset, uint length)> GetTables(UInt16 tableCount)
        {
            Dictionary<string, (uint offset, uint length)> TableLocation = new();

            for (int i = 0; i < tableCount; i++)
            {
                string tag = Reader.ReadTag(); 
                uint checkSum = Reader.ReadUInt32();
                uint offset = Reader.ReadUInt32(); 
                uint length = Reader.ReadUInt32(); 

                TableLocation.Add(tag, (offset, length));
            }

            return TableLocation;
        }

        ushort GetGlyphCount()
        {
            Reader.GoTo(TableLocation["maxp"].offset + 4);

            return Reader.ReadUInt16();
        }

        (ushort advanceWidth, short leftSideBearing)[] GetIndexMetrics()
        {
            Reader.GoTo(TableLocation["hhea"].offset);
            Reader.SkipBytes(34);
            ushort entries = Reader.ReadUInt16();


            Reader.GoTo(TableLocation["hmtx"].offset);

            (ushort advanceWidth, short leftSideBearing)[] metrics = new (ushort advanceWidth, short leftSideBearing)[glyphCount];

            for (int i = 0; i < entries; i++)
            {
                metrics[i].advanceWidth = Reader.ReadUInt16();
                metrics[i].leftSideBearing = Reader.ReadInt16();
            }

            ushort lastAdvanceWidth = metrics[entries - 1].advanceWidth;

            if (entries < glyphCount)
            {
                for (int i = entries; i < glyphCount; i++)
                {
                    metrics[i].advanceWidth = lastAdvanceWidth;
                    metrics[i].leftSideBearing = Reader.ReadInt16();
                }
            }

            return metrics;
        }

        uint[] GetGlyphLocations()
        {
            Reader.GoTo(TableLocation["head"].offset);
            Reader.SkipBytes(50);

            bool isTwoByteEntry = (short)Reader.ReadUInt16() == 0;

            uint locationTableStart = TableLocation["loca"].offset;
            uint glyphTableStart = TableLocation["glyf"].offset;
            uint[] glyphLocations = new uint[glyphCount];

            for (int i = 0; i < glyphCount; i++)
            {
                Reader.GoTo(locationTableStart + (uint)(i * (isTwoByteEntry ? 2 : 4)));
                uint glyphDataOffset = isTwoByteEntry ? Reader.ReadUInt16() * 2u : Reader.ReadUInt32();
                glyphLocations[i] = glyphTableStart + glyphDataOffset;
            }

            return glyphLocations;
        }

        public Dictionary<(ushort, ushort), short> GetIndexKerning()
        {
            Dictionary<(ushort, ushort), short> pairs = new();

            if (!TableLocation.ContainsKey("GPOS"))
                return pairs;

            uint gposOffset = TableLocation["GPOS"].offset;
            Reader.GoTo(gposOffset);

            Reader.ReadUInt16();
            Reader.ReadUInt16();
            ushort scriptListOffset = Reader.ReadUInt16();
            ushort featureListOffset = Reader.ReadUInt16();
            ushort lookupListOffset = Reader.ReadUInt16();


            List<ushort> kernLookups = new();

            Reader.GoTo(gposOffset + featureListOffset);
            ushort featureCount = Reader.ReadUInt16();

            for (int i = 0; i < featureCount; i++)
            {
                string tag = Encoding.ASCII.GetString(Reader.ReadBytes(4));
                ushort offset = Reader.ReadUInt16();

                if (tag == "kern")
                {
                    Reader.GoTo(gposOffset + featureListOffset + offset);
                    Reader.ReadUInt16();
                    ushort lc = Reader.ReadUInt16();
                    for (int j = 0; j < lc; j++)
                        kernLookups.Add(Reader.ReadUInt16());
                }
            }

            if (kernLookups.Count == 0)
                return pairs;

            Reader.GoTo(gposOffset + lookupListOffset);
            ushort lookupCount = Reader.ReadUInt16();
            ushort[] lookupOffsets = new ushort[lookupCount];
            for (int i = 0; i < lookupCount; i++)
                lookupOffsets[i] = Reader.ReadUInt16();

            foreach (ushort lookupIndex in kernLookups)
            {
                if (lookupIndex >= lookupCount)
                    continue;

                uint lookupPos = gposOffset + lookupListOffset + lookupOffsets[lookupIndex];
                Reader.GoTo(lookupPos);

                ushort lookupType = Reader.ReadUInt16();
                ushort lookupFlag = Reader.ReadUInt16();
                ushort subCount = Reader.ReadUInt16();


                if ((lookupFlag & 0x0008) != 0)
                    continue;

                if (lookupType != 2)
                    continue;

                ushort[] subOffsets = new ushort[subCount];
                for (int i = 0; i < subCount; i++)
                    subOffsets[i] = Reader.ReadUInt16();

                for (int s = 0; s < subCount; s++)
                {
                    uint subPos = lookupPos + subOffsets[s];
                    Reader.GoTo(subPos);
                    ushort format = Reader.ReadUInt16();

                    if (format == 1)
                    {
                        ushort coverageOffset = Reader.ReadUInt16();
                        ushort valueRecord1Sample = Reader.ReadUInt16();
                        ushort valueRecord2Sample = Reader.ReadUInt16();
                        ushort pairCount = Reader.ReadUInt16();

                        ushort[] pairSetOffsets = new ushort[pairCount];
                        for (int i = 0; i < pairCount; i++)
                            pairSetOffsets[i] = Reader.ReadUInt16();

                        ushort[] leftGlyphs = GetGlyphCoverage(subPos + coverageOffset);

                        int v1Size = GetValueRecordSize(valueRecord1Sample);
                        int v2Size = GetValueRecordSize(valueRecord2Sample);

                        for (int i = 0; i < pairCount && i < leftGlyphs.Length; i++)
                        {
                            uint pairSetPos = subPos + pairSetOffsets[i];
                            Reader.GoTo(pairSetPos);

                            ushort pvc = Reader.ReadUInt16();
                            ushort left = leftGlyphs[i];

                            for (int j = 0; j < pvc; j++)
                            {
                                ushort right = Reader.ReadUInt16();

                                long v1Start = Reader.GetLocation();
                                (short adv, bool has) = GetXAdvance(valueRecord1Sample);
                                Reader.GoTo(v1Start + v1Size);
                                Reader.SkipBytes(v2Size);

                                if (adv != 0)
                                    pairs.Add((left, right), adv);
                            }
                        }
                    }
                    else if (format == 2)
                    {
                        ushort coverageOffset = Reader.ReadUInt16();
                        ushort valueRecord1Sample = Reader.ReadUInt16();
                        ushort valueRecord2Sample = Reader.ReadUInt16();
                        ushort cd1Offset = Reader.ReadUInt16();
                        ushort cd2Offset = Reader.ReadUInt16();
                        ushort c1Count = Reader.ReadUInt16();
                        ushort c2Count = Reader.ReadUInt16();

                        ushort[] leftGlyphs = GetGlyphCoverage(subPos + coverageOffset);
                        var cd1 = GetGlyphClass(subPos + cd1Offset);
                        var cd2 = GetGlyphClass(subPos + cd2Offset);

                        Reader.GoTo(subPos + 14);

                        int v1Size = GetValueRecordSize(valueRecord1Sample);
                        int v2Size = GetValueRecordSize(valueRecord2Sample);

                        short[,] matrix = new short[c1Count, c2Count];

                        for (int c1 = 0; c1 < c1Count; c1++)
                        {

                            for (int c2 = 0; c2 < c2Count; c2++)
                            {
                                long before = Reader.GetLocation();
                                (short adv, bool hasDevice) = GetXAdvance(valueRecord1Sample);
                                long afterV1 = Reader.GetLocation(); Reader.SkipBytes(v2Size);
                                long afterV2 = Reader.GetLocation();
                                matrix[c1, c2] = adv;

                            }
                        }

                        foreach (ushort left in leftGlyphs)
                        {
                            ushort cl1 = cd1.TryGetValue(left, out var v) ? v : (ushort)0;
                            if (cl1 >= c1Count) continue;

                            foreach (var (unicode, index) in UnicodeIndexMap)
                            {
                                ushort right = (ushort)index;
                                ushort cl2 = cd2.TryGetValue(right, out var v2) ? v2 : (ushort)0;
                                if (cl2 >= c2Count) continue;

                                short adv = matrix[cl1, cl2];

                                if (adv != 0)
                                {

                                    pairs.TryAdd((left, right), adv);
                                }
                            }
                        }
                    }
                }
            }

            return pairs;
        }

        (uint unicode, ushort index)[] GetUnicodeIndexMap()
        {
            List<(uint unicode, ushort index)> map = new();
            uint cmapOffset = TableLocation["cmap"].offset;

            Reader.GoTo(cmapOffset);

            uint version = Reader.ReadUInt16();
            uint numSubtables = Reader.ReadUInt16();

            uint cmapSubtableOffset = uint.MaxValue;

            bool hasReadMissingCharGlyph = false;

            for (int i = 0; i < numSubtables; i++)
            {
                uint platformID = Reader.ReadUInt16();
                uint platfromSpecificID = Reader.ReadUInt16();
                uint offset = Reader.ReadUInt32();


                if (platformID == 0)
                {
                    uint unicodeVersion = platfromSpecificID;

                    if (unicodeVersion == 4)
                    {
                        cmapSubtableOffset = offset;
                    }

                    if (unicodeVersion == 3 && cmapSubtableOffset == uint.MaxValue)
                    {
                        cmapSubtableOffset = offset;
                    }

                }
            }

            if (cmapSubtableOffset == 0)
            {
                throw new Exception("Font does not contain supported character map type");
            }

            Reader.GoTo(cmapOffset + cmapSubtableOffset);
            uint format = Reader.ReadUInt16();

            if (format != 12 && format != 4)
            {
                throw new Exception("Font cmap format not supported");
            }
            

            else if (format == 12)
            {

                int reserved = Reader.ReadUInt16();
                uint byteLength = Reader.ReadUInt32();
                uint language = Reader.ReadUInt32();
                uint numGroups = Reader.ReadUInt32();

                for (int i = 0; i < numGroups; i++)
                {
                    uint startCharCode = Reader.ReadUInt32();
                    uint endCharCode = Reader.ReadUInt32();
                    uint startGlyphIndex = Reader.ReadUInt32();

                    uint numChars = endCharCode - startCharCode + 1;

                    for (uint charOffset = 0; charOffset < numChars; charOffset++)
                    {
                        uint charCode = (uint)(startCharCode + charOffset);
                        ushort glyphIndex = (ushort)(startGlyphIndex + charOffset);
                        map.Add((charCode, glyphIndex));
                    }
                }
            }
            else if (format == 4)
            {
                int length = Reader.ReadUInt16();
                int languageCode = Reader.ReadUInt16();
                int segCount2X = Reader.ReadUInt16();
                int segCount = segCount2X / 2;
                Reader.SkipBytes(6);

                int[] endCodes = new int[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    endCodes[i] = Reader.ReadUInt16();
                }

                Reader.SkipBytes(2);

                int[] startCodes = new int[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    startCodes[i] = Reader.ReadUInt16();
                }

                int[] idDeltas = new int[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    idDeltas[i] = Reader.ReadUInt16();
                }

                (int offset, int readLoc)[] idRangeOffsets = new (int, int)[segCount];
                for (int i = 0; i < segCount; i++)
                {
                    int readLoc = (int)Reader.GetLocation();
                    int offset = Reader.ReadUInt16();
                    idRangeOffsets[i] = (offset, readLoc);
                }

                for (int i = 0; i < startCodes.Length; i++)
                {
                    int endCode = endCodes[i];
                    int currCode = startCodes[i];

                    if (currCode == 0xFFFF) break;

                    while (currCode <= endCode)
                    {
                        int glyphIndex;
                        if (idRangeOffsets[i].offset == 0)
                        {
                            glyphIndex = (currCode + idDeltas[i]) & 0xFFFF;
                        }
                        else
                        {
                            uint ReaderLocationOld = (uint)Reader.GetLocation();
                            int rangeOffsetLocation = idRangeOffsets[i].readLoc + idRangeOffsets[i].offset;
                            int glyphIndexArrayLocation = 2 * (currCode - startCodes[i]) + rangeOffsetLocation;

                            Reader.GoTo((uint)glyphIndexArrayLocation);
                            glyphIndex = Reader.ReadUInt16();

                            if (glyphIndex != 0)
                            {
                                glyphIndex = (glyphIndex + idDeltas[i]) & 0xFFFF;
                            }

                            Reader.GoTo(ReaderLocationOld);
                        }

                        if (glyphIndex != 0)
                            map.Add(new((uint)currCode, (ushort)glyphIndex));
                        else
                            hasReadMissingCharGlyph = true;
                        
                            currCode++;
                    }
                }

                if (!hasReadMissingCharGlyph)
                {
                    map.Add(new(0xFFFF, 0));
                }
            }

            return map.ToArray();
        }

        Dictionary<ushort, ushort> GetGlyphClass(uint pos) 
        {
            Reader.GoTo(pos);
            ushort format = Reader.ReadUInt16();
            Dictionary<ushort, ushort> map = new();

            if (format == 1)
            {
                ushort startGlyph = Reader.ReadUInt16();
                ushort count = Reader.ReadUInt16();
                for (ushort i = 0; i < count; i++)
                    map[(ushort)(startGlyph + i)] = Reader.ReadUInt16();
            }
            else if (format == 2)
            {
                ushort rangeCount = Reader.ReadUInt16();
                for (int i = 0; i < rangeCount; i++)
                {
                    ushort start = Reader.ReadUInt16();
                    ushort end = Reader.ReadUInt16();
                    ushort cls = Reader.ReadUInt16();
                    for (ushort g = start; g <= end; g++)
                        map[g] = cls;
                }
            }

            return map;
        }

        ushort[] GetGlyphCoverage(uint pos)
        {
            Reader.GoTo(pos);
            ushort format = Reader.ReadUInt16();

            if (format == 1)
            {
                ushort count = Reader.ReadUInt16();
                ushort[] glyphs = new ushort[count];
                for (int i = 0; i < count; i++)
                    glyphs[i] = Reader.ReadUInt16();
                return glyphs;
            }

            if (format == 2)
            {
                ushort rangeCount = Reader.ReadUInt16();
                List<ushort> glyphs = new();

                for (int i = 0; i < rangeCount; i++)
                {
                    ushort start = Reader.ReadUInt16();
                    ushort end = Reader.ReadUInt16();
                    Reader.ReadUInt16();
                    for (ushort g = start; g <= end; g++)
                        glyphs.Add(g);
                }

                return glyphs.ToArray();
            }

            return Array.Empty<ushort>();
        }

        int GetValueRecordSize(ushort vf)
        {
            int size = 0;

            if ((vf & 0x0001) != 0) size += 2;
            if ((vf & 0x0002) != 0) size += 2;
            if ((vf & 0x0004) != 0) size += 2;
            if ((vf & 0x0008) != 0) size += 2;

            if ((vf & 0x0010) != 0) size += 2;
            if ((vf & 0x0020) != 0) size += 2;
            if ((vf & 0x0040) != 0) size += 2;
            if ((vf & 0x0080) != 0) size += 2;

            return size;
        }


        (short, bool) GetXAdvance(ushort vf)
        {
            short xAdvance = 0;
            bool hasDevice = false;

            if ((vf & 0x0001) != 0) Reader.ReadInt16();
            if ((vf & 0x0002) != 0) Reader.ReadInt16();
            if ((vf & 0x0004) != 0) xAdvance = Reader.ReadInt16();
            if ((vf & 0x0008) != 0) Reader.ReadInt16();

            if ((vf & 0x0010) != 0) { Reader.ReadUInt16(); hasDevice = true; }
            if ((vf & 0x0020) != 0) { Reader.ReadUInt16(); hasDevice = true; }
            if ((vf & 0x0040) != 0) { Reader.ReadUInt16(); hasDevice = true; }
            if ((vf & 0x0080) != 0) { Reader.ReadUInt16(); hasDevice = true; }

            return (xAdvance, hasDevice);
        }
    }
}
