using Ressy;
using Nebula.Core.Data;
using Nebula.Core.Data.PackageReaders;
using Nebula.Core.Memory;
using System.Text;
using TsudaKageyu;
namespace Nebula.Core.FileReaders
{
    public class EXEFileReader : IFileReader
    {
        public string Name => "Normal EXE";
        public Dictionary<int, System.Drawing.Bitmap> Icons { get; set; } = new();
        public string FilePath { get; set; } = "";
        private string _filePath = "";
        public CCNPackageData Package = new();
        public void LoadGame(ByteReader fileReader, string filePath)
        {
            fileReader.Close();
            _filePath = filePath;
            fileReader = new ByteReader(filePath, FileMode.Open);
            calculateEntryPoint(fileReader);
            bool hasData = fileReader.HasMemory(1);
            System.Console.WriteLine($"[EXEFileReader] hasData={hasData} pos={fileReader.Tell()} size={fileReader.Size()}");
            if (!hasData)
            {
                try {
                    var pe = new PortableExecutable(filePath);
                    foreach(var id in pe.GetResourceIdentifiers())
                        if (id.Type.Code==6 && id.Name.Code==11) {
                            Package.ModulesDir = Utilities.Utilities.ClearName(Encoding.Unicode.GetString(pe.GetResource(id).Data),'\\');
                            break;
                        }
                } catch (Exception ex) { System.Console.WriteLine(ex.Message); }
                var dat = Path.ChangeExtension(filePath,"dat");
                if (File.Exists(dat)) {
                    fileReader = new ByteReader(dat, FileMode.Open);
                    NebulaCore.Unpacked = true;
                } else {
                    long scan = Math.Min(fileReader.Size(), 20*1024*1024);
                    fileReader.Seek(fileReader.Size()-scan);
                    var buf = fileReader.ReadBytes((int)scan);
                    for(int i=0;i<buf.Length-4;i++) {
                        if(buf[i]==67 && buf[i+1]==32 && buf[i+2]==67 && buf[i+3]==78) {
                            System.Console.WriteLine($"Found C CN at {fileReader.Size()-scan+i}");
                            fileReader.Seek(fileReader.Size()-scan+i);
                            break;
                        }
                    }
                }
            }
            Package.PackData.Read(fileReader);
            Package.Read(fileReader);
        }
        public bool CheckInstaller(ByteReader r){ calculateEntryPoint(r); string h=""; if(r.HasMemory(4)) h=r.ReadAscii(4); r.Seek(0); return h=="wwgT"; }
        public bool CheckChowdren(ByteReader r){ r.Seek(60); var o=r.ReadUShort(); r.Seek(o+6); var n=r.ReadUShort(); r.Skip(240); bool c=false; for(int i=0;i<n;i++){ var hh=r.ReadAsciiStop(16); r.Skip(24); if(c=hh==".gfids") break; } r.Seek(0); return c && File.Exists(Path.Combine(Path.GetDirectoryName(NebulaCore.FilePath)!,"Assets.dat")); }
        private void calculateEntryPoint(ByteReader e){ var sig=e.ReadAscii(2); e.Seek(60); var hdr=e.ReadUShort(); e.Seek(hdr+6); var num=e.ReadUShort(); e.Skip(240); uint pos=0, reloc=0; for(int i=0;i<num;i++){ var name=e.ReadAsciiStop(8); e.Skip(8); uint st=e.ReadUInt(); uint sz=e.ReadUInt(); e.Skip(16); if(pos==0) pos=st+sz; else pos+=st; if(name==".reloc") reloc=st+sz; System.Console.WriteLine($"Section {name} st={st} sz={sz} acc={pos}"); } e.Seek(pos); if(!e.HasMemory(1)&&reloc!=pos&&reloc!=0) e.Seek(reloc); }
        public PackageData getPackageData()=>Package;
        public IFileReader Copy()=> new EXEFileReader{ Package=Package, Icons=Icons };
    }
}
