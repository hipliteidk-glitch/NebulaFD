using Nebula;
using Nebula.Core.FileReaders;
using Nebula.Core.Memory;
using Nebula.Core.Utilities;
using System.IO;
class Program {
 static void Main(string[] args){
 try{
 var exeDir = AppContext.BaseDirectory;
 Directory.SetCurrentDirectory(exeDir);
 Console.WriteLine($"CWD {exeDir}");
 string filePath = args.Length>0? args[0] : "";
 if (string.IsNullOrWhiteSpace(filePath)) {
 var candidates = new[] {
 "/data/data/com.termux/files/home/fnaf4_extract/Five Nights at Freddy's 4.exe",
 "/data/data/com.termux/files/home/fnaf4_real_decompile/five-nights-at-freddys-4-1-0.exe",
 "/data/data/com.termux/files/home/storage/downloads/Five Nights at Freddy's 4.exe"
 };
 filePath = candidates.FirstOrDefault(File.Exists)?? "";
 if (string.IsNullOrEmpty(filePath)) { Console.WriteLine("Usage: dotnet Nebula.Headless.dll \"/path/to/game.exe\""); return; }
 }
 if (!File.Exists(filePath)) { Console.WriteLine($"NOT FOUND: {filePath}"); return; }
 Console.WriteLine($"Using: {filePath} size={new FileInfo(filePath).Length}");
 try { NebulaCore.Init(); } catch (Exception ex) { Console.WriteLine("Init warn: "+ex.Message); }
 NebulaCore.FilePath = filePath;
 var reader = new ByteReader(filePath, FileMode.Open);
 var fr = new EXEFileReader();
 NebulaCore.CurrentReader = fr;
 Console.WriteLine("Loading...");
 fr.LoadGame(reader, filePath);
 Console.WriteLine($"Loaded! App={NebulaCore.PackageData.AppName} Build={NebulaCore.Build} Frames={NebulaCore.PackageData.Frames.Count} Images={NebulaCore.PackageData.ImageBank.Images.Count}");
 Directory.CreateDirectory("Dumps");
 new Nebula.Tools.GameDumper.MFAParser().Execute();
 Console.WriteLine("DONE Dumps/"+Path.Combine(exeDir,"Dumps"));
 }catch(Exception ex){ Console.WriteLine("FATAL: "+ex.ToString()); }
 }
}
