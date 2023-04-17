using ABI_RC.Core.Player;
using ABI_RC.Core.Savior;
using ABI_RC.Core.Util;
using HarmonyLib;
using MelonLoader;
using UnityEngine;


namespace PlayerLocator;

public class PlayerLocator : MelonMod {

    private static MelonPreferences_Category _melonCategory;
    internal static MelonPreferences_Entry<bool> LocateOnJoiningOthers;
    internal static MelonPreferences_Entry<float> UnfollowDistance;



    public override void OnInitializeMelon() {
        // Melon Config
        _melonCategory = MelonPreferences.CreateCategory(nameof(PlayerLocator));

        LocateOnJoiningOthers = _melonCategory.CreateEntry("LocateOnJoiningOthers", true,
            description: "Whether to locate the person you just joined off.");

        UnfollowDistance = _melonCategory.CreateEntry("UnfollowDistance", 2f,
            description: "Stop locating the target player once you are this distance away.");
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

        // Create a needle for each player to be located.
        foreach (CVRPlayerEntity target in targets) {
            string needleName = $"CompassNeedle-{target.Username}";
            Transform needleTransform = compassTransform.Find(needleName);
            if (!needleTransform) {
                GameObject needle = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                needle.name = needleName;
                needleTransform = needle.transform;
                needleTransform.parent = compassTransform;
                needleTransform.localPosition = Vector3.zero;
            }
            needleTransform.LookAt(target.PlayerObject.transform);
        }

        // Cleanup compass needles that are no longer wanted.

        //
        //_visualizer.name = "[TheClapper] Visualizer";
        //_visualizer.layer = LayerMask.NameToLayer("UI Internal");
        //_visualizer.transform.position = transform.position;
        //_visualizer.transform.rotation = transform.rotation;
        //_visualizer.transform.localScale = Vector3.one * MinimumDistance * 2;

        //_visualizer.transform.SetParent(transform, true);

        //PlayerSetup

        //PlayerSetup.Instance.eyeMovement.isLocal;
        //PlayerSetup.Instance._avatarDescriptor != null 
        //!string.IsNullOrEmpty(MetaPort.Instance.currentAvatarGuid) && avatarId == MetaPort.Instance.currentAvatarGuid

        /*
        foreach (CVRPlayerEntity player in CVRPlayerManager.Instance.NetworkPlayers)        {
            if (player.Username == MetaPort.Instance.username) {
                MelonLogger.Msg($"found self: {player.Username}");
            }
            else {
                MelonLogger.Msg($"found other: {player.Username}");
            }
        }
        */
    }

    [HarmonyPatch]
    private static class HarmonyPatches {

    }
}
