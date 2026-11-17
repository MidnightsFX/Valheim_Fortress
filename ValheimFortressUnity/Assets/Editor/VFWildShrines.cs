using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// The wild shrines are one base prefab (Base/VF_wild_shrine_base) and a variant per biome holding only what makes
// that biome's shrine its own: its light and particle colours, and for some an extra effect. Rebuild rewrites every
// variant from the table below, so change a shrine here rather than in its prefab. The names are what each shrine's
// WildShrines.yaml entry and enable setting are keyed by, and what worlds store, so they never change.
//
// From the editor: menu Valheim Fortress/Rebuild Wild Shrines, and .../Render Wild Shrine Previews (to Screenshots/).
// Headless: Unity.exe -batchmode -quit -projectPath ValheimFortressUnity -executeMethod VFWildShrines.RebuildFromCommandLine
public static class VFWildShrines
{
    private const string Folder = "Assets/Custom/Locations/VFortress";
    private const string BasePath = Folder + "/Base/VF_wild_shrine_base.prefab";
    // The first shrine, kept as the base's content when the base does not exist yet.
    private const string BaseSource = Folder + "/VF_wild_shrine_purple1.prefab";
    private const string EffectMaterial = "Assets/Custom/Textures/VFX/vf_vfx_point.mat";
    private const string BundleName = "vfbundle";

    private enum Effect { None, Embers, Snow }

    private class Shrine
    {
        public string Name;
        public Color Light;
        public Color? Flicker;
        public Color? FlameMin, FlameMax;
        public Color? GlowMin, GlowMax;
        public Color? SuckMin, SuckMax;
        public Effect Effect;
    }

    private static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    private static readonly Shrine[] Shrines =
    {
        new Shrine { Name = "VF_wild_shrine_green1", Light = C(0.51231945f, 1f, 0.36862743f) },
        new Shrine { Name = "VF_wild_shrine_blue1", Light = C(0.36862743f, 0.93013406f, 1f) },
        new Shrine { Name = "VF_wild_shrine_green2", Light = C(0.08343716f, 0.7075472f, 0.15021789f) },
        new Shrine { Name = "VF_wild_shrine_blue2", Light = C(0.33161268f, 0.5467111f, 0.8679245f) },
        new Shrine { Name = "VF_wild_shrine_yellow1", Light = C(0.92936784f, 0.9528302f, 0.36405307f) },
        new Shrine { Name = "VF_wild_shrine_purple1", Light = C(0.7830189f, 0.21052866f, 0.74699146f) },
        // Ashlands: an ember-red shrine that sheds sparks.
        new Shrine
        {
            Name = "VF_wild_shrine_red1", Light = C(1f, 0.3f, 0.07f),
            FlameMin = C(1f, 0.12f, 0.02f, 0.88f), FlameMax = C(1f, 0.48f, 0.08f, 0.8f),
            GlowMin = C(1f, 0.35f, 0.08f), GlowMax = C(1f, 0.62f, 0.25f),
            SuckMin = C(1f, 0.55f, 0.1f), SuckMax = C(0.75f, 0.06f, 0.02f),
            Effect = Effect.Embers,
        },
        // Deep North: a cold, pale-blue shrine with snow drifting down around it.
        new Shrine
        {
            Name = "VF_wild_shrine_blue3", Light = C(0.72f, 0.88f, 1f), Flicker = C(0.55f, 0.78f, 1f),
            FlameMin = C(0.45f, 0.75f, 1f, 0.85f), FlameMax = C(0.85f, 0.95f, 1f, 0.75f),
            GlowMin = C(0.6f, 0.82f, 1f), GlowMax = C(0.92f, 0.97f, 1f),
            SuckMin = C(0.85f, 0.95f, 1f), SuckMax = C(0.35f, 0.62f, 1f),
            Effect = Effect.Snow,
        },
    };

    [MenuItem("Valheim Fortress/Rebuild Wild Shrines")]
    public static bool Rebuild()
    {
        if (!EnsureBase()) { return false; }
        GameObject baseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BasePath);
        foreach (Shrine shrine in Shrines)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(baseAsset);
            try
            {
                instance.name = shrine.Name;
                Apply(instance, shrine);
                string path = $"{Folder}/{shrine.Name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(instance, path, out bool saved);
                if (!saved)
                {
                    Debug.LogError($"[VF] Could not save {path}.");
                    return false;
                }
                AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(BundleName, "");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[VF] Rebuilt {Shrines.Length} wild shrines from {BasePath}.");
        return true;
    }

    private static bool EnsureBase()
    {
        if (File.Exists(BasePath)) { return true; }
        if (!AssetDatabase.IsValidFolder(Folder + "/Base")) { AssetDatabase.CreateFolder(Folder, "Base"); }
        if (!AssetDatabase.CopyAsset(BaseSource, BasePath))
        {
            Debug.LogError($"[VF] Could not create {BasePath} from {BaseSource}.");
            return false;
        }
        // The base is never loaded by name: only its variants go in the bundle.
        AssetImporter.GetAtPath(BasePath).SetAssetBundleNameAndVariant("", "");
        GameObject root = PrefabUtility.LoadPrefabContents(BasePath);
        root.name = "VF_wild_shrine_base";
        PrefabUtility.SaveAsPrefabAsset(root, BasePath);
        PrefabUtility.UnloadPrefabContents(root);
        return true;
    }

    private static void Apply(GameObject shrine, Shrine spec)
    {
        SetLight(shrine, "Point_light", spec.Light);
        if (spec.Flicker.HasValue) { SetLight(shrine, "Point light", spec.Flicker.Value); }
        SetStartColor(shrine, "blue flames", spec.FlameMin, spec.FlameMax);
        SetStartColor(shrine, "Particle System", spec.GlowMin, spec.GlowMax);
        SetStartColor(shrine, "suck particles", spec.SuckMin, spec.SuckMax);
        if (spec.Effect == Effect.Embers) { AddEmbers(shrine); }
        if (spec.Effect == Effect.Snow) { AddSnow(shrine); }
    }

    private static Transform Find(Transform root, string name)
    {
        if (root.name == name) { return root; }
        foreach (Transform child in root)
        {
            Transform found = Find(child, name);
            if (found != null) { return found; }
        }
        return null;
    }

    private static void SetLight(GameObject shrine, string name, Color color)
    {
        Light light = Find(shrine.transform, name)?.GetComponent<Light>();
        if (light == null) { throw new InvalidOperationException($"{shrine.name} has no light '{name}'."); }
        light.color = color;
    }

    private static void SetStartColor(GameObject shrine, string name, Color? min, Color? max)
    {
        if (!min.HasValue || !max.HasValue) { return; }
        ParticleSystem system = Find(shrine.transform, name)?.GetComponent<ParticleSystem>();
        if (system == null) { throw new InvalidOperationException($"{shrine.name} has no particle system '{name}'."); }
        ParticleSystem.MainModule main = system.main;
        main.startColor = new ParticleSystem.MinMaxGradient(min.Value, max.Value);
    }

    private static ParticleSystem AddEffect(GameObject shrine, string name, Vector3 position, Vector3 area)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(shrine.transform, false);
        go.transform.localPosition = position;
        ParticleSystem system = go.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.prewarm = true;
        main.startSpeed = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = area;

        ParticleSystem.NoiseModule noise = system.noise;
        noise.enabled = true;
        noise.frequency = 0.5f;

        ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(EffectMaterial);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        return system;
    }

    private static void SetVelocity(ParticleSystem system, float minY, float maxY)
    {
        ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        // Every axis must use the same curve mode.
        velocity.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        velocity.y = new ParticleSystem.MinMaxCurve(minY, maxY);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
    }

    private static void FadeInOut(ParticleSystem system, Color start, Color end)
    {
        ParticleSystem.ColorOverLifetimeModule colour = system.colorOverLifetime;
        colour.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(start, 0f), new GradientColorKey(end, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
        colour.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private static void AddEmbers(GameObject shrine)
    {
        ParticleSystem system = AddEffect(shrine, "Embers", new Vector3(0f, 0.3f, 0f), new Vector3(5f, 0.3f, 5f));
        ParticleSystem.MainModule main = system.main;
        main.duration = 5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        main.startColor = new ParticleSystem.MinMaxGradient(C(1f, 0.75f, 0.3f), C(1f, 0.35f, 0.05f));
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 18f;
        ParticleSystem.NoiseModule noise = system.noise;
        noise.strength = 0.35f;
        SetVelocity(system, 0.5f, 1.3f);
        FadeInOut(system, Color.white, C(1f, 0.25f, 0.05f));
    }

    private static void AddSnow(GameObject shrine)
    {
        ParticleSystem system = AddEffect(shrine, "Snow", new Vector3(0f, 6f, 0f), new Vector3(9f, 0.3f, 9f));
        ParticleSystem.MainModule main = system.main;
        main.duration = 8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 9f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
        main.startColor = new ParticleSystem.MinMaxGradient(C(1f, 1f, 1f, 0.95f), C(0.85f, 0.93f, 1f, 0.8f));
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 30f;
        ParticleSystem.NoiseModule noise = system.noise;
        noise.strength = 0.3f;
        SetVelocity(system, -0.95f, -0.55f);
        FadeInOut(system, Color.white, Color.white);
    }

    [MenuItem("Valheim Fortress/Render Wild Shrine Previews")]
    public static void RenderPreviews()
    {
        string output = Path.Combine(Path.GetFullPath(Path.Combine(Application.dataPath, "..")), "Screenshots");
        Directory.CreateDirectory(output);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = C(0.32f, 0.34f, 0.4f);

        Camera camera = new GameObject("Preview Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = C(0.05f, 0.06f, 0.09f);
        camera.fieldOfView = 50f;
        camera.transform.position = new Vector3(0f, 3.4f, -11f);
        camera.transform.LookAt(new Vector3(0f, 2f, 0f));
        Light sun = new GameObject("Sun").AddComponent<Light>();
        sun.type = LightType.Directional;
        sun.intensity = 0.35f;
        sun.transform.rotation = Quaternion.Euler(40f, 30f, 0f);
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.localScale = new Vector3(3f, 1f, 3f);

        RenderTexture target = new RenderTexture(1280, 720, 24);
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        foreach (Shrine shrine in Shrines)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{Folder}/{shrine.Name}.prefab");
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            foreach (ParticleSystem system in instance.GetComponentsInChildren<ParticleSystem>())
            {
                system.Simulate(4f, false, true);
            }
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            File.WriteAllBytes(Path.Combine(output, shrine.Name + ".png"), image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(instance);
        }
        camera.targetTexture = null;
        Debug.Log($"[VF] Rendered {Shrines.Length} wild shrine previews to {output}.");
    }

    public static void RebuildFromCommandLine()
    {
        bool ok = Rebuild();
        if (ok) { RenderPreviews(); }
        EditorApplication.Exit(ok ? 0 : 1);
    }
}
