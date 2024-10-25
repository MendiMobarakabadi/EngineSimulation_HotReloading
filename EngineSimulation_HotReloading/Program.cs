using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
//using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static FileSystemWatcher watcher;

    private static CancellationTokenSource cancellationTokenSource;

    static async Task Main(string[] args)
    {
        Console.WriteLine("...........,,,,,,,,,,,,,............");
        Version dotnetVersion = Environment.Version;
        Console.WriteLine($"Current .NET Version: {dotnetVersion}");
        Console.WriteLine("...........,,,,,,,,,,,,,............");

        string codeFilePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "MyProgram.cs");


        watcher = new FileSystemWatcher();
        string pathToWatch = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
        watcher.Path = pathToWatch;

        watcher.Filter = "*.cs";

        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size;

        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnChanged;
        watcher.EnableRaisingEvents = true;

        // اولین کامپایل و اجرا
        await CompileAndRun(codeFilePath);

        Console.WriteLine("Watching for code changes... Press any key to exit.");
        Console.ReadKey();
    }

    private static async void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.Name == "MyProgram.cs")
        {
            Console.WriteLine("Code change detected. Recompiling...");
            await CompileAndRun(e.FullPath);
        }
    }
    private static async Task WaitForFile(string filePath)
    {
        const int retries = 10;
        const int delay = 200; 

        for (int i = 0; i < retries; i++)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return; 
                }
            }
            catch (IOException)
            {
                await Task.Delay(delay);
            }
        }

        throw new IOException($"The file {filePath} is being used by another process.");
    }



    private static async Task CompileAndRun(string filePath)
    {
        try
        {
            
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            await WaitForFile(filePath);
            
            string code = File.ReadAllText(filePath);

            
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = new[]
            {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\6.0.35\System.Runtime.dll") //System.Runtime.dll
        };

            var compilation = CSharpCompilation.Create("MyAssembly")
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(references);

            
            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (result.Success)
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    
                    var assembly = Assembly.Load(ms.ToArray());
                    var type = assembly.GetType("MyProgram");
                    var method = type.GetMethod("Run");

                    if (method != null)
                    {
                        var instance = Activator.CreateInstance(type);
                        Console.WriteLine("Running the updated program...");

                        
                        await (Task)method.Invoke(instance, new object[] { cancellationTokenSource.Token });
                    }
                }
                else
                {
                    
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.WriteLine($"Error: {diagnostic.GetMessage()}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during compilation: {ex.Message}");
        }
    }

}




