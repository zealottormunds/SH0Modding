using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_Hill_Origins_Modding_Tools
{
    public class SH0File
    {
        public string file_name = "";
        public int file_offset = 0;
        public int file_size = 0;
        public int file_decompressed_size = 0;
        public string replaced_path = "";

        public SH0File(string name, int offset, int size, int decompsize)
        {
            file_name = name;
            file_offset = offset;
            file_size = size;
            file_decompressed_size = decompsize;
        }
    }
}
