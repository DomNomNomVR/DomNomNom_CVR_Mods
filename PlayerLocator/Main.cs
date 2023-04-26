using ABI_RC.Core.Player;
using ABI_RC.Core.Player.AvatarTracking.Local;
using ABI_RC.Core.Player.AvatarTracking.Remote;
using Aura2API;
using HarmonyLib;
using MelonLoader;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;

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
            var resourceStream = MelonAssembly.Assembly.GetManifestResourceStream("playerlocator.assetbundle");
            if (resourceStream == null) {
                MelonLogger.Error($"Failed to load assetbundle bundle!");
                return;
            }
            var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);
            var assetBundle = AssetBundle.LoadFromMemory(memoryStream.ToArray());

            CompassNeedlePrefab = assetBundle.LoadAsset<GameObject>("Assets/DomNomNom/mods/PlayerLocator/CompassNeedle.prefab");
            CompassNeedlePrefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;

        } catch (Exception ex) {
            MelonLogger.Error("Failed to Load the asset bundle: " + ex.Message);
            return;
        }
    }

    private string getNeedleName(CVRPlayerEntity target) {
        return $"CompassNeedle-{target.Username}";
    }

    private HashSet<string> InitializeCompassNeedles(Transform compassTransform, List<CVRPlayerEntity> targets) {
        HashSet<string> expectedNeedleNames = new HashSet<string>();
        foreach (CVRPlayerEntity target in targets) {
            string needleName = getNeedleName(target);
            expectedNeedleNames.Add(needleName);
            Transform needleTransform = compassTransform.Find(needleName);
            if (needleTransform) continue;

            GameObject needle = GameObject.Instantiate(CompassNeedlePrefab);
            needle.name = needleName;
            needleTransform = needle.transform;
            needleTransform.parent = compassTransform;
            needleTransform.localPosition = Vector3.zero;

            Transform usernameTransform = needleTransform.Find("Arrow/Tip/Text_username");
            var textMeshPro = usernameTransform.GetComponent<TextMeshPro>();
            textMeshPro.text = target.Username;
        }
        return expectedNeedleNames;

    }


    private void SetArrowLookatConstraint(Transform needleTransform, CVRPlayerEntity target) {
        // Assuming a matching structure in the prefab.
        Transform arrowTransform = needleTransform.Find("Arrow");
        LookAtConstraint look = arrowTransform.GetComponent<LookAtConstraint>();
        ConstraintSource sauce = look.GetSource(0);
        RemoteHeadPoint remoteHeadPoint = target.PuppetMaster._viewPoint;
        try {
            // This sometimes fails if the avatar isn't fully loaded yet or something.
            // note that remoteHeadPoint is not null but GetTransform internally rauses a NullReferenceException
            sauce.sourceTransform = remoteHeadPoint.GetTransform();
        } catch (NullReferenceException) {
            //MelonLogger.Msg("Target avatar not ready - using backup target point");
            sauce.sourceTransform = target.PlayerObject.transform;
        }
        look.SetSource(0, sauce);
    }

    private void SetTipLookatConstraint(Transform needleTransform) { 
        Transform tipTransform = needleTransform.Find("Arrow/Tip");
        LookAtConstraint look = tipTransform.GetComponent<LookAtConstraint>();
        if (!look) {
            MelonLogger.Msg("no tip :(");
            return;
        }
        ConstraintSource sauce = look.GetSource(0);
        if (!PlayerSetup.Instance) {
            MelonLogger.Msg("no PlayerSetup.Instance :(");
            return;
        }
        LocalHeadPoint localHeadPoint = PlayerSetup.Instance._viewPoint;
        if (localHeadPoint) {
            sauce.sourceTransform = localHeadPoint.GetTransform();
        }
        look.SetSource(0, sauce);
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
            compassTransform.localPosition = 1.2f * Vector3.forward + .2f * Vector3.up; // TODO: test / adjust for different avatar sizes.
        }

        // TODO: filter down to a select number of targets
        List<CVRPlayerEntity> targets = CVRPlayerManager.Instance.NetworkPlayers;
        HashSet<string> expectedNeedleNames = InitializeCompassNeedles(compassTransform, targets);


        // Cleanup compass needles that are no longer wanted.
        for (int i = compassTransform.childCount - 1; i >= 0; --i) {
            Transform needleTransform = compassTransform.GetChild(i);
            if (!expectedNeedleNames.Contains(needleTransform.name)) {
                needleTransform.parent = null;
                needleTransform.Destroy();
                continue;
            }
        }

        // TODO: Do these less frequently
        try {
            foreach (CVRPlayerEntity target in targets) {
                Transform needleTransform = compassTransform.Find(getNeedleName(target));
                SetArrowLookatConstraint(needleTransform, target);
            }
        } catch (Exception err) {
            MelonLogger.Error($"SetArrowLookatConstraint failed: {err}");
        }
        try {
            foreach (CVRPlayerEntity target in targets) {
                Transform needleTransform = compassTransform.Find(getNeedleName(target));
                SetTipLookatConstraint(needleTransform);
            }
        } catch (Exception err) {
            MelonLogger.Error($"SetTipLookatConstraint failed: {err}");
        }

        // Update distance to target
        for (int i = compassTransform.childCount - 1; i >= 0; --i) {
            Transform needleTransform = compassTransform.GetChild(i);
            try {
                TextMeshPro textMeshPro = needleTransform.Find("Arrow/Tip/Text_distance").GetComponent<TextMeshPro>();
                Transform arrowTransform = needleTransform.Find("Arrow");
                LookAtConstraint look = arrowTransform.GetComponent<LookAtConstraint>();
                ConstraintSource sauce = look.GetSource(0);
                if (sauce.sourceTransform) {
                    float distance = Vector3.Distance(compassTransform.position, sauce.sourceTransform.position);
                    textMeshPro.text = distance.ToString("0.0");
                } else {
                    MelonLogger.Msg("no sauce :(");
                }
            } catch (Exception err) {
                MelonLogger.Error($"Failed to update distance to follow target {needleTransform.name}: {err}");
            }
        }

        
    }

    [HarmonyPatch]
    private static class HarmonyPatches {

    }
}
