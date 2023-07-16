using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_Hill_Origins_Modding_Tools
{
    public class ArcFile
    {
        public Dictionary<string, SH0File> file_dictionary = new Dictionary<string, SH0File>();
        public List<string> file_list = new List<string>();
        public byte[] fileBytes;
        public FileParser fp = new FileParser();
        public ArcChunkTools c;

        public void SetArcFile(byte[] data)
        {
            fileBytes = data;
        }

        public void Clear()
        {
            file_dictionary.Clear();
            file_list.Clear();
            fileBytes = null;
        }

        public bool DecompileArc()
        {
            if (fileBytes == null) return false;

            int file_count = fp.ReadInt(fileBytes, 4);
            int data_start = fp.ReadInt(fileBytes, 8);
            int string_table = fp.ReadInt(fileBytes, 0xC);
            int string_table_size = fp.ReadInt(fileBytes, 0x10);
            int index = 0x14;

            for (int x = 0; x < file_count; x++)
            {
                int file_name_offset = fp.ReadInt(fileBytes, index) + string_table;
                string file_name = fp.ReadString(fileBytes, file_name_offset);

                index += 0x4;
                int file_offset = fp.ReadInt(fileBytes, index);

                index += 0x4;
                int file_size = fp.ReadInt(fileBytes, index);

                index += 0x4;
                int file_decompressed_size = fp.ReadInt(fileBytes, index);

                index += 0x4;
                SH0File sh0_file = new SH0File(file_name, file_offset, file_size, file_decompressed_size);
                file_dictionary.Add(file_name, sh0_file);
                file_list.Add(file_name);
            }

            return true;
        }

        public byte[] CompileArc()
        {
            if (fileBytes == null) return new byte[0];

            FileParser2 fw = new FileParser2();

            fw.AddData(0x41322E30, true);
            fw.AddData((uint)(file_list.Count));
            fw.AddData((uint)0);
            fw.AddData((uint)0);
            fw.AddData((uint)0);

            Dictionary<string, SH0File> metadata = new Dictionary<string, SH0File>();

            // For each file
            foreach (string key in file_list)
            {
                SH0File sh0f = file_dictionary[key];

                int off_name = fw.fileBytes.Count;
                fw.AddData((int)0);

                int off_data = fw.fileBytes.Count;
                fw.AddData((int)0);

                int off_size = fw.fileBytes.Count;
                fw.AddData((int)0);

                int off_decsize = fw.fileBytes.Count;
                fw.AddData((int)0);

                metadata.Add(sh0f.file_name, new SH0File(sh0f.file_name, off_data, off_size, off_decsize));
            }

            fw.AddData(0x98013200, true);
            fw.AddData(0x98013200, true);
            fw.AddData(0x5053325C, true);

            int data_offset_file = fw.fileBytes.Count;
            fw.ReplaceData(data_offset_file, 0x8);

            // For each file
            foreach (string key in file_list)
            {
                SH0File sh0f = file_dictionary[key];
                byte[] compressed_data = new byte[0];

                int start_off = fw.fileBytes.Count;

                if (sh0f.replaced_path == "")
                {
                    compressed_data = fp.ReadByteArray(fileBytes, sh0f.file_offset, sh0f.file_size);
                }
                else
                {
                    byte[] uncompressed_data = File.ReadAllBytes(sh0f.replaced_path);
                    c.CompressData(uncompressed_data, out compressed_data);
                }

                // Add data
                fw.AddBytes(compressed_data);

                int end_off = compressed_data.Length;

                // Replace sizes in header
                fw.ReplaceData(start_off, metadata[sh0f.file_name].file_offset);
                fw.ReplaceData(end_off, metadata[sh0f.file_name].file_size);
                fw.ReplaceData(sh0f.file_decompressed_size, metadata[sh0f.file_name].file_size + 4);

                // Compress
                while (fw.fileBytes.Count % 32 != 0)
                {
                    fw.AddBytes(new byte[] { 0 });
                }
            }

            // String table offset
            int string_table_off = fw.fileBytes.Count;
            fw.ReplaceData(string_table_off, 0xC);

            // For each name
            foreach (string key in file_list)
            {
                int curr_off = fw.fileBytes.Count;
                SH0File sh0f = file_dictionary[key];
                fw.AddData(sh0f.file_name, true);

                // Replace in header
                fw.ReplaceData(curr_off - string_table_off, metadata[key].file_offset - 4);
            }

            int string_table_size = fw.fileBytes.Count - string_table_off;
            fw.ReplaceData(string_table_size, 0x10);

            return fw.fileBytes.ToArray();
        }
    }

}
