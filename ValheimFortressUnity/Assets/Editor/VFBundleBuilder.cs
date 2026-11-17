using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Builds the "vfbundle" asset bundle from every asset tagged with it and copies it into the mod project
// (ValheimFortress/AssetsEmbedded/vfbundle), where the mod's build embeds it. Its asset list is written to
// vfbundle.txt in this project, which is committed, so a change in what the bundle holds shows up in a diff.
//
// From the editor: menu Valheim Fortress/Build Asset Bundle.
// Headless, with the editor closed:
//   Unity.exe -batchmode -quit -projectPath ValheimFortressUnity -executeMethod VFBundleBuilder.BuildFromCommandLine -logFile build.log
// List another bundle's assets (for example the one embedded now):
//   ... -executeMethod VFBundleBuilder.ListFromCommandLine -bundle <path> -out <file>
public static class VFBundleBuilder
{
    private const string BundleName = "vfbundle";

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    [MenuItem("Valheim Fortress/Build Asset Bundle")]
    public static bool Build()
    {
        string[] names = AssetDatabase.GetAllAssetBundleNames();
        if (names.Length != 1 || names[0] != BundleName)
        {
            Debug.LogError($"[VF] Expected only the '{BundleName}' bundle, found: {string.Join(", ", names)}. Untag the others first.");
            return false;
        }

        string stage = Path.Combine(ProjectRoot, "AssetBundles");
        Directory.CreateDirectory(stage);
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(stage, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        if (manifest == null)
        {
            Debug.LogError("[VF] The asset bundle build failed; see the errors above.");
            return false;
        }

        string built = Path.Combine(stage, BundleName);
        string[] assets = ListAssets(built);
        if (assets == null) { return false; }
        File.WriteAllLines(Path.Combine(ProjectRoot, BundleName + ".txt"), assets);

        string target = Path.GetFullPath(Path.Combine(ProjectRoot, "..", "ValheimFortress", "AssetsEmbedded", BundleName));
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        File.Copy(built, target, true);
        Debug.Log($"[VF] Built '{BundleName}' ({assets.Length} assets, {new FileInfo(target).Length / 1024} KB) and copied it to {target}. Rebuild the mod to embed it.");
        return true;
    }

    // Loading the bundle here proves it loads in game too; a bundle of the same name must not be loaded already.
    private static string[] ListAssets(string path)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError($"[VF] {path} does not load as an asset bundle.");
            return null;
        }
        string[] assets = bundle.GetAllAssetNames().OrderBy(n => n, StringComparer.Ordinal).ToArray();
        bundle.Unload(true);
        return assets;
    }

    public static void BuildFromCommandLine()
    {
        EditorApplication.Exit(Build() ? 0 : 1);
    }

    public static void ListFromCommandLine()
    {
        string[] args = Environment.GetCommandLineArgs();
        string bundle = Argument(args, "-bundle");
        string output = Argument(args, "-out");
        string[] assets = bundle == null ? null : ListAssets(bundle);
        if (assets == null || output == null)
        {
            Debug.LogError("[VF] Usage: -executeMethod VFBundleBuilder.ListFromCommandLine -bundle <path> -out <file>");
            EditorApplication.Exit(1);
            return;
        }
        File.WriteAllLines(output, assets);
        Debug.Log($"[VF] {assets.Length} assets in {bundle} written to {output}.");
        EditorApplication.Exit(0);
    }

    private static string Argument(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
