using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    class DebuggStepThroughRemover
    {
        [MenuItem("Debug/Remove DebugStepThrough Attributes in Burst", priority = 1)]
        public static void RemoveDebugStepThrough()
        {
            var info = System.IO.Directory.CreateDirectory($"{Application.dataPath}/../Library/PackageCache/");
            var dirs = info.GetDirectories();
            System.IO.DirectoryInfo burstDir = null;
            foreach (var d in dirs)
            {
                if (d.Name.Contains("burst"))
                {
                    burstDir = d;
                    break;
                }
            }
            if (burstDir == null)
            {
                Debug.LogError("Could not find burst directory");
                return;
            }
            Debug.Log(burstDir.FullName);

            var files = System.IO.Directory.CreateDirectory($"{burstDir.FullName}/Runtime/Intrinsics").GetFiles("*.cs");
            foreach (var f in files)
            {
                string text = System.IO.File.ReadAllText(f.FullName);
                text = text.Replace("[DebuggerStepThrough]", "//[DebuggerStepThrough]");
                System.IO.File.WriteAllText(f.FullName, text);
            }

            files = System.IO.Directory.CreateDirectory($"{burstDir.FullName}/Runtime/Intrinsics/x86").GetFiles("*.cs");
            foreach (var f in files)
            {
                string text = System.IO.File.ReadAllText(f.FullName);
                text = text.Replace("[DebuggerStepThrough]", "//[DebuggerStepThrough]");
                System.IO.File.WriteAllText(f.FullName, text);
            }
        }
    }
}
