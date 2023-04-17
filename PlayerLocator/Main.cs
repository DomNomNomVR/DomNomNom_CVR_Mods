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
        MelonLogger.Msg("HELLO WORLDDDDDD :D");
    }

    [HarmonyPatch]
    private static class HarmonyPatches {

    }
}
