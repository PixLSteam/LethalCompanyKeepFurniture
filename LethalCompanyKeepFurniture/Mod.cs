using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace LethalCompanyKeepFurniture
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Mod: BaseUnityPlugin
    {
        public const string modGUID = "PixL.KeepFurniture"; // a unique name for your mod
        public const string modName = "KeepFurniture"; // the name of your mod
        public const string modVersion = "1.0.0.0"; // the version of your mod

        private readonly Harmony harmony = new Harmony(modGUID); // Creating a Harmony instance which will run the mods

        public static ManualLogSource Log;

        void Awake()
        {
            Log = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            harmony.PatchAll(typeof(KeepFurniture));
        }
    }

    class KeepFurniture
    {
        // Disable ResetShipFurniture and enable all colliders instead
        [HarmonyPatch(typeof(StartOfRound), "ResetShipFurniture")]
        [HarmonyPrefix]
        static bool ResetShipFurniture(ref StartOfRound __instance)
        {
            PlaceableShipObject[] placeableShipObjects = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
            for (int i = 0; i < placeableShipObjects.Length; i++)
            {
                var obj = placeableShipObjects[i];
                if (obj.parentObject == null)
                {
                    continue;
                }

                // Note: playersFiredGameOver only disables colliders if `__instance.unlockablesList.unlockables[obj.unlockableID].spawnPrefab` is true
                // However, we cannot check for that as attempting to access __instance.unlockablesList.unlockables causes a TypeLoadException
                Collider[] componentsInChildren = obj.parentObject.GetComponentsInChildren<Collider>();
                for (int j = 0; j < componentsInChildren.Length; j++)
                {
                    componentsInChildren[j].enabled = true;
                }
            }

            return false;
        }

        // Disable ResetUnlockablesListValues
        [HarmonyPatch(typeof(GameNetworkManager), "ResetUnlockablesListValues")]
        [HarmonyPrefix]
        static bool ResetUnlockablesListValues()
        {
            return false;
        }

        // Prevent ES3.DeleteKey from deleting ship unlock-related keys (see GameNetworkManager.ResetSavedGameValues)
        [HarmonyPatch(typeof(ES3), "DeleteKey", new Type[] { typeof(string), typeof(string) })]
        [HarmonyPrefix]
        static bool ES3DeleteKey(string key, string filePath)
        {
            if (key == "UnlockedShipObjects"
                || key.StartsWith("ShipUnlock"))
            {
                return false;
            }
            return true;
        }
    }
}
