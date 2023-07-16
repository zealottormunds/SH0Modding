using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Silent_Hill_Origins_Modding_Tools
{
    public class ChunkFile
    {
        public byte[] data;
        public List<chunkEntry> entries = new List<chunkEntry>();
        public List<List<byte[]>> file_tree = new List<List<byte[]>>();
        public FileParser fp = new FileParser();

        public struct chunkEntry
        {
            public int header_offset;
            public int data_offset;
            public int entry_type;
            public int entry_size;
            public int entry_type2;
            public int magic;

            public chunkEntry(int header_off, int data_off, int entrytype, int entrysize, int entrytype2, int magic_)
            {
                header_offset = header_off;
                data_offset = data_off;
                entry_type = entrytype;
                entry_size = entrysize;
                entry_type2 = entrytype2;
                magic = magic_;
            }
        }
        
        public void Clear()
        {
            data = null;
            entries.Clear();
            file_tree.Clear();
        }

        public void AnalyzeFile()
        {
            int index = 0;
            while (index < data.Length)
            {
                int header_offset = index;
                int data_offset = index + 0xC;

                int entrytype = fp.ReadInt(data, index);
                index += 4;
                int entrysize = fp.ReadInt(data, index);
                index += 4;
                int entrytype2 = fp.ReadInt(data, index);
                index += 4;

                int magic = fp.ReadInt(data, data_offset);
                
                chunkEntry ce = new chunkEntry(header_offset, data_offset, entrytype, entrysize, entrytype2, magic);
                entries.Add(ce);
                int this_tree = file_tree.Count;
                file_tree.Add(new List<byte[]>());

                byte[] current_chunk = fp.ReadByteArray(data, ce.data_offset, ce.entry_size);

                if (entrytype == 0x716)
                {
                    int curr_ind = 0;
                    while (curr_ind < current_chunk.Length)
                    {
                        int datasize = fp.ReadInt(current_chunk, curr_ind);
                        curr_ind += 4;
                        byte[] thisdata = fp.ReadByteArray(current_chunk, curr_ind, datasize);
                        file_tree[this_tree].Add(thisdata);
                        curr_ind += datasize;

                        // Page integrity
                        while (curr_ind % 4 > 0) curr_ind++;
                    }
                }
                else
                {
                    file_tree[this_tree].Add(current_chunk);
                }

                index += entrysize;
            }
        }

        public byte[] PackFile()
        {
            FileParser2 fp2 = new FileParser2();

            for(int x = 0; x < entries.Count; x++)
            {
                chunkEntry ce = entries[x];
                List<byte[]> this_tree = file_tree[x];
                
                // Write type 1
                fp2.AddBytes(BitConverter.GetBytes(ce.entry_type));

                // Write size (temporal)
                int size_offset = fp2.fileBytes.Count;
                fp2.AddBytes(new byte[] { 0, 0, 0, 0 });

                // Write type 2
                fp2.AddBytes(BitConverter.GetBytes(ce.entry_type2));

                if (ce.entry_type == 0x716)
                {
                    int total_size = 0;

                    // Write subfiles
                    for (int y = 0; y < this_tree.Count; y++)
                    {
                        total_size += this_tree[y].Length + 4;
                        fp2.AddBytes(BitConverter.GetBytes(this_tree[y].Length));
                        fp2.AddBytes(this_tree[y]);

                        while(fp2.fileBytes.Count % 4 > 0)
                        {
                            total_size++;
                            fp2.AddBytes(new byte[] { 0x58 });
                        }
                    }

                    fp2.ReplaceData(total_size, size_offset);
                }
                else
                {
                    fp2.AddBytes(this_tree[0]);
                    fp2.ReplaceData(this_tree[0].Length, size_offset);
                }
            }

            return fp2.fileBytes.ToArray();
        }
    }
}
