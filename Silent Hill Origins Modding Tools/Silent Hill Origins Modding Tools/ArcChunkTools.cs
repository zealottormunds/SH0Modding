using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace Silent_Hill_Origins_Modding_Tools
{
    public class ArcChunkTools
    {
        public FileParser fp;
        public ArcFile arc_file;
        public Form1 formdata;
        public Dictionary<uint, string> fileTypes = new Dictionary<uint, string>();
        public Dictionary<string, string> fileTypes_ext = new Dictionary<string, string>();

        public ArcChunkTools(FileParser fileparser, ArcFile f, Form1 form_data)
        {
            fp = fileparser;
            arc_file = f;
            formdata = form_data;

            fileTypes.Add(0xFFD8FFE0, "JPG");
            fileTypes_ext.Add(".xml", "XML");
        }

        public byte[] DecompressFileChunk(SH0File f)
        {
            byte[] decomp_data = fp.ReadByteArray(arc_file.fileBytes, f.file_offset, f.file_size);
            byte[] outd;
            DecompressData(decomp_data, out outd);
            return outd;
        }

        public void CompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream, zlibConst.Z_BEST_COMPRESSION))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public void DecompressData(byte[] inData, out byte[] outData)
        {
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            using (Stream inMemoryStream = new MemoryStream(inData))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                outData = outMemoryStream.ToArray();
            }
        }

        public void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public string GetFileType(string name, byte[] data)
        {
            string t = "UNKNOWN";

            if (data.Length < 4) return t;

            int index = 0;
            uint magic = fp.ReadUint(data, ref index, false, true);
            bool found_type = false;

            if (fileTypes.ContainsKey(magic))
            {
                t = fileTypes[magic];
                found_type = true;
            }

            if (!found_type)
            {
                string ext = Path.GetExtension(name);
                if (fileTypes_ext.ContainsKey(ext))
                {
                    t = fileTypes_ext[ext];
                    found_type = true;
                }
            }

            // Test if chunk file
            if (!found_type && data.Length > 12)
            {
                index = 8;
                uint magic2 = fp.ReadUint(data, ref index, false, true);
                if (magic2 == 0x6500021C)
                {
                    t = "CHUNK";
                }
            }

            return t;
        }
    }
}
