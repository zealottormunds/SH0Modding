using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class FileParser
{
    public byte[] ReadByteArray(byte[] actual, int index, int count)
    {
        List<byte> a = new List<byte>();
        for (int x = 0; x < count; x++)
        {
            a.Add(actual[index + x]);
        }
        return a.ToArray();
    }

    public int byteArrayToShort(byte[] actual, bool big = false)
    {
        if (big) return actual[1] + actual[0] * 256;

        return actual[0] + actual[1] * 256;
    }

    public int byteArrayToInt(byte[] actual, bool big = false)
    {
        if (big) Array.Reverse(actual);
        return BitConverter.ToInt32(actual, 0);
    }

    public uint byteArrayToUint(byte[] actual, bool big = false)
    {
        if (big) Array.Reverse(actual);
        return BitConverter.ToUInt32(actual, 0);
    }

    public int ReadShort(byte[] fileBytes, ref int index, bool addIndex = false, bool big = false)
    {
        int a = byteArrayToShort(ReadByteArray(fileBytes, index, 2), big);
        if (addIndex) index += 2;

        return a;
    }

    public int ReadShort(byte[] fileBytes, int index, bool big = false)
    {
        int a = byteArrayToShort(ReadByteArray(fileBytes, index, 2), big);
        return a;
    }

    public uint ReadUint(byte[] fileBytes, ref int index, bool addIndex = false, bool big = false)
    {
        uint a = byteArrayToUint(ReadByteArray(fileBytes, index, 4), big);
        if (addIndex) index += 4;

        return a;
    }

    public int ReadInt(byte[] fileBytes, ref int index, bool addIndex = false, bool big = false)
    {
        int a = byteArrayToInt(ReadByteArray(fileBytes, index, 4), big);
        if (addIndex) index += 4;

        return a;
    }

    public int ReadInt(byte[] fileBytes, int index, bool big = false)
    {
        int a = byteArrayToInt(ReadByteArray(fileBytes, index, 4), big);
        return a;
    }

    public float ReadFloat(byte[] actual, int index, bool big = false)
    {
        if (big) Array.Reverse(actual);
        return BitConverter.ToSingle(actual, index);
    }

    public string ReadString(byte[] actual, int index, int count = -1)
    {
        string a = "";
        if (count == -1)
        {
            for (int x2 = index; x2 < actual.Length; x2++)
            {
                if (actual[x2] != 0)
                {
                    string str = a;
                    char c = (char)actual[x2];
                    a = str + c;
                }
                else
                {
                    x2 = actual.Length;
                }
            }
        }
        else
        {
            for (int x = index; x < index + count; x++)
            {
                string str2 = a;
                char c = (char)actual[x];
                a = str2 + c;
            }
        }
        return a;
    }

    public byte[] ReplaceBytes(byte[] actual, byte[] bytesToReplace, int Index, int Invert = 0)
    {
        if (Invert == 0)
        {
            for (int x2 = 0; x2 < bytesToReplace.Length; x2++)
            {
                actual[Index + x2] = bytesToReplace[x2];
            }
        }
        else
        {
            int len = bytesToReplace.Length - 1;
            for (int x = 0; x < bytesToReplace.Length; x++)
            {
                actual[Index + x] = bytesToReplace[len - x];
            }
        }
        return actual;
    }

    public byte[] ReplaceString(byte[] actual, string str, int Index, int Count = -1)
    {
        if (Count == -1)
        {
            for (int x2 = 0; x2 < str.Length; x2++)
            {
                actual[Index + x2] = (byte)str[x2];
            }
        }
        else
        {
            for (int x = 0; x < Count; x++)
            {
                if (str.Length > x)
                {
                    actual[Index + x] = (byte)str[x];
                }
                else
                {
                    actual[Index + x] = 0;
                }
            }
        }
        return actual;
    }

    public byte[] AddBytes(byte[] actual, byte[] bytesToAdd, int Reverse = 0, int index = 0, int count = -1)
    {
        List<byte> a = actual.ToList();
        if (Reverse == 0)
        {
            if (count == -1) count = bytesToAdd.Length;
            for (int x = index; x < index + count; x++)
            {
                a.Add(bytesToAdd[x]);
            }
        }
        else
        {
            if (count == -1) count = bytesToAdd.Length;
            for (int x = index; x < index + count; x++)
            {
                a.Add(bytesToAdd[bytesToAdd.Length - 1 - x]);
            }
        }
        return a.ToArray();
    }

    public byte[] AddShort(byte[] actual, short _num, bool big = false)
    {
        List<byte> a = actual.ToList();
        byte[] b = BitConverter.GetBytes(_num);

        if (big == false) for (int x = 0; x < 2; x++) a.Add(b[x]);
        else for (int x = 0; x < 2; x++) a.Add(b[2 - x]);

        return a.ToArray();
    }

    public byte[] AddInt(byte[] actual, int _num, bool big = false)
    {
        List<byte> a = actual.ToList();
        byte[] b = BitConverter.GetBytes(_num);

        if (big == false)
        {
            for (int x = 0; x < 4; x++) a.Add(b[x]);
        }
        else
        {
            for (int x = 3; x >= 0; x--) a.Add(b[x]);
        }

        return a.ToArray();
    }

    public byte[] ReplaceInt(byte[] actual, int _num, int index, bool big = false)
    {
        byte[] b = BitConverter.GetBytes(_num);

        if (big == false)
        {
            for (int x = 0; x < 4; x++) actual[index + x] = b[x];
        }
        else
        {
            for (int x = 0; x < 4; x++) actual[index + x] = b[3 - x];
        }

        return actual;
    }

    public byte[] AddString(byte[] actual, string _str, int count = -1)
    {
        List<byte> a = actual.ToList();
        for (int x2 = 0; x2 < _str.Length; x2++)
        {
            a.Add((byte)_str[x2]);
        }
        for (int x = _str.Length; x < count; x++)
        {
            a.Add(0);
        }
        return a.ToArray();
    }

    public byte[] AddFloat(byte[] actual, float f, bool big = false)
    {
        List<byte> a = actual.ToList();
        byte[] floatBytes = BitConverter.GetBytes(f);

        if (big == false)
        {
            for (int x = 0; x < 4; x++) a.Add(floatBytes[x]);
        }
        else
        {
            for (int x = 0; x < 4; x++) a.Add(floatBytes[3 - x]);
        }

        return a.ToArray();
    }

    public bool CompareArray(byte[] a, byte[] b)
    {
        bool equal = true;

        for (int x = 0; x < a.Length; x++)
        {
            if (a[x] != b[x])
            {
                equal = false;
                x = a.Length;
            }
        }

        return equal;
    }

    public bool CompareArray(byte[] a, byte[] b, bool[] ignore)
    {
        bool equal = true;

        for (int x = 0; x < a.Length; x++)
        {
            if (ignore[x] == false)
            {
                if (a[x] != b[x])
                {
                    equal = false;
                    x = a.Length;
                }
            }
        }

        return equal;
    }

    public int FindString(byte[] a, string f, int start = 0)
    {
        int found = -1;

        for(int x = start; x < (a.Length - f.Length); x++)
        {
            string s = ReadString(a, x, f.Length);
            if(s == f)
            {
                found = x;
                x = a.Length;
            }
        }

        return found;
    }
}

public class FileParser2
{
    public List<byte> fileBytes = new List<byte>();

    public void AddBytes(byte[] b, int start = 0, int count = -1)
    {
        if(count == -1)
        {
            count = b.Length;
        }

        for(int x = 0; x < count; x++)
        {
            fileBytes.Add(b[start + x]);
        }
    }

    public void AddData(uint i, bool reverse = false)
    {
        byte[] b = BitConverter.GetBytes(i);

        if(!reverse)
        {
            for (int x = 0; x < 4; x++)
            {
                fileBytes.Add(b[x]);
            }
            return;
        }

        for (int x = 0; x < 4; x++)
        {
            fileBytes.Add(b[3 - x]);
        }
    }

    public void AddData(string s, bool separator = false)
    {
        for(int x = 0; x < s.Length; x++)
        {
            fileBytes.Add((byte)(s[x]));
        }

        if(separator)
        {
            fileBytes.Add(0x0);
        }
    }

    public void ReplaceData(int i, int index, bool reverse = false)
    {
        byte[] b = BitConverter.GetBytes(i);

        if (!reverse)
        {
            for (int x = 0; x < 4; x++)
            {
                fileBytes[index + x] = b[x];
            }
            return;
        }

        for (int x = 0; x < 4; x++)
        {
            fileBytes[index + x] = b[3 - x];
        }
    }
}
