using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Silent_Hill_Origins_Modding_Tools
{
    public partial class ChunkViewer : Form
    {
        public ChunkViewer()
        {
            InitializeComponent();
        }

        public ChunkFile cf = new ChunkFile();
        public FileParser fp = new FileParser();
        public string original_path = "";
        public string original_name = "";

        public void ClearData()
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            cf.Clear();
            original_path = "";
            original_name = "";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();

            if (o.FileName == "") return;
            
            // Clear old data
            ClearData();

            // Set new data
            original_path = o.FileName;
            original_name = Path.GetFileName(o.FileName);
            byte[] data = File.ReadAllBytes(o.FileName);
            cf.data = data;
            cf.AnalyzeFile();

            // Populate listbox
            for(int x = 0; x < cf.entries.Count; x++)
            {
                int id = x;
                ChunkFile.chunkEntry ce = cf.entries[x];

                string format = "UNK";
                if (ce.magic == 0x16) format = "TXD";

                listBox1.Items.Add(id.ToString("X2") + " (" + format + ") - Type: " + ce.entry_type.ToString("X2") + " - Size: " + ce.entry_size.ToString("X2"));
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Backup
            byte[] original_data = File.ReadAllBytes(original_path);
            File.WriteAllBytes(original_path + ".bak", original_data);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.ShowDialog();

            if (s.FileName == "") return;

            byte[] outdata = cf.PackFile();
            File.WriteAllBytes(s.FileName, outdata);
            MessageBox.Show("Written file to " + s.FileName);
        }

        // Extract all chunks
        private void button3_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog o = new CommonOpenFileDialog();
            o.IsFolderPicker = true;
            var result = o.ShowDialog();
            if (result != CommonFileDialogResult.Ok) return;

            for(int x = 0; x < cf.entries.Count; x++)
            {
                ChunkFile.chunkEntry ce = cf.entries[x];
                byte[] data = fp.ReadByteArray(cf.data, ce.data_offset, ce.entry_size);
                File.WriteAllBytes(o.FileName + "/" + original_name + "_" + x.ToString(), data);
            }

            MessageBox.Show("Extracted " + cf.entries.Count.ToString() + " files.");
        }

        // Extract chunk
        private void button1_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;

            SaveFileDialog s = new SaveFileDialog();
            s.FileName = original_name + "_" + index.ToString("X2") + ".chunk";
            var result = s.ShowDialog();

            if (result != DialogResult.OK) return;

            ChunkFile.chunkEntry ce = cf.entries[index];

            byte[] data = fp.ReadByteArray(cf.data, ce.data_offset, ce.entry_size);
            File.WriteAllBytes(s.FileName, data);
            MessageBox.Show("File extracted to " + s.FileName);
        }

        // Import chunk
        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();

            int index = listBox1.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            ChunkFile.chunkEntry ce = cf.entries[index];

            switch (ce.entry_type)
            {
                default:
                    {
                        List<byte[]> this_tree = cf.file_tree[index];
                        listBox2.Items.Add("0 : Unknown Format - Full Chunk");
                    }
                    break;
                case 0x716:
                    {
                        List<byte[]> this_tree = cf.file_tree[index];
                        for (int x = 0; x < this_tree.Count; x++)
                        {
                            byte[] this_data = this_tree[x];
                            byte[] magicbytes = new byte[] { this_data[0], this_data[1], this_data[2], this_data[3] };
                            uint thismagic = BitConverter.ToUInt32(magicbytes, 0);
                            string format = GetFormatByMagic(thismagic);
                            if (format == "UNK") format = GetFormatScan(this_data);

                            listBox2.Items.Add(x.ToString("X2") + " : " + format + " (Size: " + this_data.Length.ToString("X2") + ")");
                        }
                    }
                    break;
            }
        }

        // Extract subfile
        private void button4_Click(object sender, EventArgs e)
        {
            int index_l1 = listBox1.SelectedIndex;
            int index_l2 = listBox2.SelectedIndex;
            if (index_l1 == -1 || index_l2 == -1) return;

            SaveFileDialog s = new SaveFileDialog();

            byte[] toextract = cf.file_tree[index_l1][index_l2];
            byte[] magicbytes = new byte[] { toextract[0], toextract[1], toextract[2], toextract[3] };
            uint thismagic = BitConverter.ToUInt32(magicbytes, 0);

            // Format search
            string format = GetFormatByMagic(thismagic);
            if (format == "UNK") format = GetFormatScan(toextract);

            s.FileName = original_name + "_" + index_l1.ToString("X2") + "_" + index_l2.ToString("X2") + "." + format;

            var result = s.ShowDialog();
            if (result != DialogResult.OK) return;

            File.WriteAllBytes(s.FileName, toextract);
            MessageBox.Show("Extracted subfile to " + s.FileName);
        }

        // Import subfile
        private void button5_Click(object sender, EventArgs e)
        {
            int index_l1 = listBox1.SelectedIndex;
            int index_l2 = listBox2.SelectedIndex;
            if (index_l1 == -1 || index_l2 == -1) return;

            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();
            if (o.FileName == "") return;

            byte[] newdata = File.ReadAllBytes(o.FileName);

            cf.file_tree[index_l1][index_l2] = newdata;
        }

        public string GetFormatByMagic(uint magic)
        {
            string f = "UNK";

            switch(magic)
            {
                case 0x10:
                    f = "DFF";
                    break;
                case 0x16:
                    f = "TXD";
                    break;
                case 0xE0FFD8FF:
                    f = "JPG";
                    break;
            }

            return f;
        }
        
        public string GetFormatScan(byte[] fdat)
        {
            string format = "UNK";

            if(fp.FindString(fdat, "rwID_CBSP") > -1)
            {
                format = "COL";
            }

            return format;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
