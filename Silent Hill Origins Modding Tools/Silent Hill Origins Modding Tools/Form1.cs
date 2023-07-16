using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using zlib;
using System.IO;
using System.IO.Compression;
using Microsoft.WindowsAPICodePack;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Silent_Hill_Origins_Modding_Tools
{
    public partial class Form1 : Form
    {
        public ArcFile arc_file;
        public FileParser fp = new FileParser();
        public ArcChunkTools c;

        public Form1()
        {
            InitializeComponent();
        }

        // Ooen arc file
        private void openarcFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();

            if (o.FileName == "") return;

            arc_file = new ArcFile();
            arc_file.SetArcFile(File.ReadAllBytes(o.FileName));

            c = new ArcChunkTools(fp, arc_file, this);
            arc_file.c = c;

            bool decomp = arc_file.DecompileArc();

            if (decomp == false)
            {
                MessageBox.Show("Error decompressing file.");
                arc_file.Clear();
                arc_file = null;
            }

            // Populate listbox with files
            for (int x = 0; x < arc_file.file_list.Count; x++)
            {
                string file_name = arc_file.file_list[x];
                SH0File file_data = arc_file.file_dictionary[file_name];
                listBox1.Items.Add(file_data.file_name);
            }
        }

        // Save arc file
        private void savearcFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.ShowDialog();

            if (s.FileName == "") return;

            byte[] outfile = arc_file.CompileArc();
            if(outfile.Length > 0)
            {
                File.WriteAllBytes(s.FileName, outfile);
                MessageBox.Show("Written file to: " + s.FileName);
            }
        }

        // Save project data
        private void saveshprojFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        // Close file
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (arc_file == null)
            {
                MessageBox.Show("No file open.");
                return;
            }

            arc_file.Clear();
            listBox1.Items.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            MessageBox.Show("File closed successfully.");
        }

        // Change list index
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;

            string n = arc_file.file_list[index];
            textBox1.Text = arc_file.file_dictionary[n].file_name;
            textBox2.Text = arc_file.file_dictionary[n].replaced_path;

            try
            {
                SH0File f = arc_file.file_dictionary[n];
                byte[] outd = c.DecompressFileChunk(f);
                textBox3.Text = c.GetFileType(n, outd);
                outd = null;
            }
            catch
            {
                textBox3.Text = "Error decompiling";
            }
        }

        // Decompress selected
        private void button1_Click(object sender, EventArgs e)
        {
            if (arc_file == null) return;

            int index = listBox1.SelectedIndex;
            if (index < 0) return;

            string n = arc_file.file_list[index];
            SH0File f = arc_file.file_dictionary[n];

            SaveFileDialog s = new SaveFileDialog();
            s.FileName = f.file_name;
            s.ShowDialog();

            if (s.FileName == "") return;
            string path = s.FileName;

            byte[] outd = c.DecompressFileChunk(f);
            File.WriteAllBytes(path, outd);
        }

        // Decompress all
        private void button2_Click(object sender, EventArgs e)
        {
            if (!(arc_file != null && arc_file.fileBytes.Length > 0)) return;

            CommonOpenFileDialog o = new CommonOpenFileDialog();
            o.IsFolderPicker = true;
            var dlg_result = o.ShowDialog();
            if (dlg_result != CommonFileDialogResult.Ok) return;
            string path = o.FileName;

            string errors = "Errors:\n\n";
            bool had_errors = false;

            for (int x = 0; x < arc_file.file_list.Count; x++)
            {
                string n = arc_file.file_list[x];
                SH0File f = arc_file.file_dictionary[n];

                try
                {
                    byte[] outd = c.DecompressFileChunk(f);
                    File.WriteAllBytes(path + "/" + f.file_name, outd);
                }
                catch
                {
                    errors += n + "\n";
                    had_errors = true;
                }
            }

            if (had_errors)
            {
                MessageBox.Show(errors);
            }
        }
        
        // Import to selected
        private void button3_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;

            OpenFileDialog o = new OpenFileDialog();
            o.ShowDialog();
            if (o.FileName == "") return;

            string n = arc_file.file_list[index];
            arc_file.file_dictionary[n].replaced_path = o.FileName;

            MessageBox.Show("Replaced file " + n + " to " + arc_file.file_dictionary[n].replaced_path);
        }

        // Import all
        private void button4_Click(object sender, EventArgs e)
        {
            /*int index = listBox1.SelectedIndex;
            if (index < 0) return;

            string n = arc_file.file_list[index];
            SH0File f = arc_file.file_dictionary[n];

            SaveFileDialog s = new SaveFileDialog();
            s.FileName = f.file_name;
            s.ShowDialog();

            if (s.FileName == "") return;
            string path = s.FileName;

            byte[] outd = c.DecompressFileChunk(f);
            byte[] ind;
            c.CompressData(outd, out ind);

            File.WriteAllBytes(path, ind);*/
        }

        public struct jpg_file
        {
            public uint offset;
            public uint size;
            public uint id;

            public jpg_file(uint o, uint s, uint i)
            {
                offset = o;
                size = s;
                id = i;
            }
        }

        // JPG
        private void button5_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0) return;

            CommonOpenFileDialog o = new CommonOpenFileDialog();
            o.IsFolderPicker = true;
            var dlg_result = o.ShowDialog();
            if (dlg_result != CommonFileDialogResult.Ok) return;

            string n = arc_file.file_list[index];
            SH0File f = arc_file.file_dictionary[n];
            byte[] outd = c.DecompressFileChunk(f);

            // Find all JFIFs
            int s = fp.FindString(outd, "JFIF", 0);
            List<jpg_file> jpg_list = new List<jpg_file>();
            while(s != -1)
            {
                uint actual_offset = (uint)(s - 6);
                int temp_index = s - 10;
                uint actual_size = fp.ReadUint(outd, ref temp_index, false, false);
                
                if(actual_size > 0)
                {
                    uint id = (uint)(jpg_list.Count);
                    jpg_file new_jpg = new jpg_file(actual_offset, actual_size, id);
                    jpg_list.Add(new_jpg);
                }

                s = fp.FindString(outd, "JFIF", s + 1);
            }

            // Extract
            for(int x = 0; x < jpg_list.Count; x++)
            {
                jpg_file jpg = jpg_list[x];
                byte[] this_jpg_data = fp.ReadByteArray(outd, (int)(jpg.offset), (int)(jpg.size));
                File.WriteAllBytes(o.FileName + "/" + n + "_" + jpg.id.ToString() + ".jpg", this_jpg_data);
            }
        }

        private void chunkViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChunkViewer cw = new ChunkViewer();
            cw.ShowDialog();
        }
    }
}
