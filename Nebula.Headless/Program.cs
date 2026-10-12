using Nebula;
using Nebula.Core.FileReaders;
using Nebula.Core.Memory;
using Nebula.Core.Utilities;
using System.IO;
class Program {
 static void Main(string[] args){
 try{
 var exeDir = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
 Directory.SetCurrentDirectory(exeDir!);
 Console.WriteLine($"CWD set to {exeDir}");
 string filePath = args.Length>0? args[0] : "/data/data/com.termux/files/home/storage/downloads/R36S-FNAF4-REAL/Five Nights at Freddy's 4.exe";
 Console.WriteLine($"Using: {filePath} size={new FileInfo(filePath).Length}");
 NebulaCore.Init();
 NebulaCore.FilePath = filePath;
 var reader = new ByteReader(filePath, FileMode.Open);
 var fr = new EXEFileReader();
 NebulaCore.CurrentReader = fr;
 Console.WriteLine("Loading...");
 fr.LoadGame(reader, filePath);
 Console.WriteLine($"Loaded! AppName={NebulaCore.PackageData.AppName} Fusion={NebulaCore.Fusion} Build={NebulaCore.Build} Frames={NebulaCore.PackageData.Frames.Count} Images={NebulaCore.PackageData.ImageBank.Images.Count}");
 Directory.CreateDirectory("Dumps");
 Console.WriteLine("Executing MFAParser...");
 var mfaParser = new Nebula.Tools.GameDumper.MFAParser();
 mfaParser.Execute();
 Console.WriteLine("MFA dumped to "+Path.Combine(exeDir!,"Dumps"));
 }catch(Exception ex){
 Console.WriteLine("FATAL ERROR: "+ex);
 Console.WriteLine(ex.StackTrace);
 }
 }
}
