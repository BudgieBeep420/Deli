using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing.Text;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using UnityEngine;
using Mono.Cecil;
using HarmonyLib;
using Random = UnityEngine.Random;

namespace DeliExampleMod
{
    [BepInPlugin("yes", "Mod", "1.0.0")]
    public class Mod : BaseUnityPlugin
    {
        private ConfigEntry<bool> modIsOn;

        private static ConfigEntry<bool> attachmentSpawn;
        private static ConfigEntry<bool> weaponSpawn;
        private static ConfigEntry<bool> magazineSpawn;
        
        private static ConfigEntry<float> attachmentProb;
        private static ConfigEntry<float> weaponProb;
        private static ConfigEntry<float> magazineProb;

        private static FVRObject[] attachmentArray;
        private static FVRObject[] weaponArray;
        private static FVRObject[] magazineArray;
        
        private void Awake()
        {
            modIsOn = Config.Bind("General",
                "Mod Activate",
                true,
                "This is used to turn the mod on and off");
            
            attachmentSpawn = Config.Bind("General",
                "Attachment Spawns",
                true,
                "This is used to turn the attachment spawns on and off");
            
            magazineSpawn = Config.Bind("General",
                "Magazine Spawns",
                true,
                "This is used to turn the magazine spawns on and off");
            
            weaponSpawn = Config.Bind("General",
                "Weapon Spawns",
                true,
                "This is used to turn the weapons spawns on and off");
            
            

            attachmentProb = Config.Bind("Probabilities",
                "Attachment Probability",
                0.2f,
                "This is a float from 0 to 1 to describe the chance of spawning");
            
            weaponProb = Config.Bind("Probabilities",
                "Weapon Probability",
                0.05f,
                "This is a float from 0 to 1 to describe the chance of spawning");
            
            magazineProb = Config.Bind("Probabilities",
                "Magazine Probability",
                0.25f,
                "This is a float from 0 to 1 to describe the chance of spawning");
            
            
            if (modIsOn.Value) Harmony.CreateAndPatchAll(typeof(Mod));
        }

        [HarmonyPatch(typeof(TNH_ShatterableCrate), "Destroy")]
        [HarmonyPrefix]
        private static void BoxDestructionPatch(TNH_ShatterableCrate __instance)
        {
            var attachmentRand = Random.Range(0f, 1f);
            var magazineRand = Random.Range(0f, 1f);
            var weaponRand = Random.Range(0f, 1f);
            
            Debug.Log(attachmentRand);
            Debug.Log(magazineRand);
            Debug.Log(weaponRand);
            Debug.Log("");
            Debug.Log(attachmentProb);
            Debug.Log(magazineProb);
            Debug.Log(weaponProb);
            Debug.Log("Lengths:");
            Debug.Log(attachmentArray.Length);
            Debug.Log(magazineArray.Length);
            Debug.Log(weaponArray.Length);
            
            
            if (attachmentRand <= attachmentProb.Value)
            {
                var lengthOfAttachmentArray = attachmentArray.Length;
                var attachment = Instantiate(attachmentArray[Random.Range(0, lengthOfAttachmentArray + 1)].GetGameObject());
                attachment.transform.position = __instance.transform.position;
            }
            
            if (weaponRand <= weaponProb.Value)
            {
                var lengthOfArray = weaponArray.Length;
                var weapon = Instantiate(weaponArray[Random.Range(0, lengthOfArray + 1)].GetGameObject());
                weapon.transform.position = __instance.transform.position;
            }
            
            if (magazineRand <= magazineProb.Value)
            {
                var lengthOfArray = magazineArray.Length;
                var magazine = Instantiate(magazineArray[Random.Range(0, lengthOfArray + 1)].GetGameObject());
                magazine.transform.position = __instance.transform.position;
            }
        }

        [HarmonyPatch(typeof(TNH_Manager), "Start")]
        [HarmonyPrefix]
        private static void ManagerPatch()
        {
            Debug.Log("Patched tnh manager");

            attachmentArray = IM.OD.Values
                .Where(x => x.Category == FVRObject.ObjectCategory.Attachment)
                .ToArray();

            weaponArray = IM.OD.Values
                .Where(x => x.Category == FVRObject.ObjectCategory.Firearm)
                .ToArray();
            
            magazineArray = IM.OD.Values
                .Where(x => x.Category == FVRObject.ObjectCategory.Magazine)
                .ToArray();
        }
    }
}