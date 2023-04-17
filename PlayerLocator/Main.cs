using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Util;
using HarmonyLib;
using MelonLoader;
using Unity.Entities;
using UnityEngine;


namespace PlayerLocator;

public class PlayerLocator : MelonMod {

    private static MelonPreferences_Category _melonCategory;
    internal static MelonPreferences_Entry<bool> LocateOnJoiningOthers;
    internal static MelonPreferences_Entry<float> UnfollowDistance;
    private GameObject CompassNeedlePrefab;


    public override void OnInitializeMelon() {
        // Melon Config
        _melonCategory = MelonPreferences.CreateCategory(nameof(PlayerLocator));

        LocateOnJoiningOthers = _melonCategory.CreateEntry("LocateOnJoiningOthers", true,
            description: "Whether to locate the person you just joined off.");

        UnfollowDistance = _melonCategory.CreateEntry("UnfollowDistance", 2f,
            description: "Stop locating the target player once you are this distance away.");

        // Import asset bundle
        try {

            MelonLogger.Msg($"Loading the asset bundle...");
            using var resourceStream = MelonAssembly.Assembly.GetManifestResourceStream("playerlocator.assetbundle");
            using var memoryStream = new MemoryStream();
            if (resourceStream == null) {
                MelonLogger.Error($"Failed to load assetbundle bundle!");
                return;
            }
            resourceStream.CopyTo(memoryStream);
            var assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());

            CompassNeedlePrefab = assetBundle.LoadAsset<GameObject>("Assets/DomNomNom/mods/PlayerLocator/CompassNeedle.prefab");
            CompassNeedlePrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        } catch (Exception ex) {
            MelonLogger.Error("Failed to Load the asset bundle: " + ex.Message);
            return;
        }
    }


    public override void OnUpdate() {
        
        if (!PlayerSetup.Instance) return;
        var avi = PlayerSetup.Instance._avatar;
        if (!avi) return;
        Transform playerRoot = avi.transform;
        if (!playerRoot) return;
        Transform compassTransform = playerRoot.Find("PlayerLocatorCompass");
        if (compassTransform == null) {
            GameObject compass = new GameObject("PlayerLocatorCompass");
            compass.layer = LayerMask.NameToLayer("UI Internal");
            compassTransform = compass.transform;
            compassTransform.parent = playerRoot;
            compassTransform.localPosition = 1.2f * Vector3.forward + .2f * Vector3.up;
        }

        List<CVRPlayerEntity> targets = CVRPlayerManager.Instance.NetworkPlayers;

        // TODO: filter down to a select number of targets

        // Create a needle for each player to be located.
        foreach (CVRPlayerEntity target in targets) {
            string needleName = $"CompassNeedle-{target.Username}";
            Transform needleTransform = compassTransform.Find(needleName);
            if (!needleTransform) {
                GameObject needle = GameObject.Instantiate(CompassNeedlePrefab);
                needle.name = needleName;
                needleTransform = needle.transform;
                needleTransform.parent = compassTransform;
                needleTransform.localPosition = Vector3.zero;
            }
            needleTransform.LookAt(target.PlayerObject.transform);
        }

        // TODO: Cleanup compass needles that are no longer wanted.
    }

    [HarmonyPatch]
    private static class HarmonyPatches {

    }
}
