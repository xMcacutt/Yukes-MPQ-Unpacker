using System.Text;
using System.Text.RegularExpressions;

namespace Yukes_MPQ_Unpacker
{
    public class Program
    {
        static string _path;
        static bool _run = true;
        public static bool LittleEndian = false;

        public static void Main(string[] args)
        {
            foreach(string arg in args)
            {
                if (File.Exists(arg)) Extract(arg);
            }
            if (args.Length > 0) return;
            while (_run)
            {
                Console.Clear();
                Console.WriteLine("MPQ THP Extractor for the Wii Version of The DOG Island");
                Console.WriteLine("Please enter path to MPQ");
                _path = Console.ReadLine().Replace("\"", "");
                while (!File.Exists(_path) || !_path.EndsWith(".mpq", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("Path was invalid");
                    Console.WriteLine("Please enter path to MPQ");
                    _path = Console.ReadLine().Replace("\"", " ");
                }
                Console.WriteLine("Extracting...");
                Extract(_path);
                Console.WriteLine("Extract completed successfully");
                Console.ReadLine();
            }
        }

        public static void Extract(string path)
        {
            string pacName = Path.GetFileNameWithoutExtension(path);
            string inDirPath = Path.GetDirectoryName(path);
            string outDirPath = Path.Combine(inDirPath, pacName + " Extract");
            if (!Directory.Exists(outDirPath)) Directory.CreateDirectory(outDirPath);

            var f = File.OpenRead(path);
            f.Seek(0xC, SeekOrigin.Begin);
            uint fileCount = BEBitConv.ToUInt32(ReadBytes(f, new byte[4]), 0);
            List<uint> filePtrs = new List<uint>();
            for(int i = 0; i < fileCount; i++)
            {
                filePtrs.Add(BEBitConv.ToUInt32(ReadBytes(f, new byte[4]), 0));
            }
            f.Seek(filePtrs[0], SeekOrigin.Begin);
            string extension = Regex.Replace(Encoding.ASCII.GetString(ReadBytes(f, new byte[4])), @"[\x00-\x1F\x7F]", "");
            for (int i = 0; i < fileCount; i++)
            {
                uint startPoint = filePtrs[i];
                uint endPoint = i == fileCount - 1 ? (uint)f.Length : filePtrs[i + 1];
                f.Seek(startPoint, SeekOrigin.Begin);
                FileStream outFile = File.Create(Path.Combine(outDirPath, pacName + "_" + i + "." + extension));
                outFile.Write(ReadBytes(f, new byte[endPoint - startPoint]));
                outFile.Close();
            }
        }

        public static byte[] ReadBytes(FileStream f, byte[] buffer)
        {
            f.Read(buffer, 0, buffer.Length);
            return buffer;
        }
    }
}

