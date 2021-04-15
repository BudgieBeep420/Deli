using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Security.Policy;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using UnityEngine;
using Mono.Cecil;
using HarmonyLib;
using MonoMod.Utils;
using Random = UnityEngine.Random;

namespace BetterTNHCrates
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
                0.2f,
                "This is a float from 0 to 1 to describe the chance of spawning");
            
            magazineProb = Config.Bind("Probabilities",
                "Magazine Probability",
                0.2f,
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
            
            var boxPosition = __instance.transform.position; 

            if (attachmentRand <= attachmentProb.Value) GenerateAttachment(boxPosition);
            if (weaponRand <= weaponProb.Value) GenerateWeapon(boxPosition);
            if (magazineRand <= magazineProb.Value) GenerateSingleMagazine(boxPosition);
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
            
            /*Finds the current character */
            var gameManager = FindObjectOfType<TNH_Manager>();
            var currentCharacter = gameManager.C;
            
            Debug.Log("Current character: " + currentCharacter.DisplayName);

            Debug.Log("Here is the first object in the table defs: " + currentCharacter.EquipmentPool.Entries[0].TableDef.name);
        }
        
        /*Debug*/

        /*
        [HarmonyPatch(typeof(FVRWristMenu), "Awake")]
        [HarmonyPrefix]
        private static void DebugPatch()
        {
            var tnhObjectFile = 
                File.ReadAllText(Application.dataPath.Replace("h3vr_Data", @"TNH_Tweaker/pool_contents.txt"));
            
            var parsed = tnhObjectFile.Split(new[] { "Pool:" }, StringSplitOptions.None);
            var categories = new string[parsed.Length][];

            for (var i = 0; i < parsed.Length; i++)
            {
                categories[i] = parsed[i]
                    .Trim()
                    .Split(new[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
            }

            Debug.Log("This is the final cut");
            foreach (var item in categories)
            {
                for (var i = 0; i < item.Length; i++)
                {
                    item[i] = item[i].Trim();
                    Debug.Log(item[i]);
                }
            }
        }*/

        private static IEnumerator SpawnObject(Vector3 boxPosition, FVRObject fvrObject)
        {
            Debug.Log("IEnum has fired");
            var objectCallBack = fvrObject.GetGameObjectAsync();
            yield return new WaitUntil(() => objectCallBack.IsCompleted);
            
            var magazine = Instantiate(objectCallBack.Result);
                    
            magazine.transform.position = boxPosition;
        }

        private static void GenerateAttachment(Vector3 boxPosition)
        {
            var lengthOfAttachmentArray = attachmentArray.Length;
            var attachment = Instantiate(attachmentArray[Random.Range(0, lengthOfAttachmentArray + 1)].GetGameObject());
            attachment.transform.position = boxPosition;
        }

        private static void GenerateWeapon(Vector3 boxPosition)
        {
            var lengthOfArray = weaponArray.Length;
            Debug.Log("weapon array length:" + weaponArray.Length);
            var weaponFVRObject = weaponArray[Random.Range(0, lengthOfArray + 1)];
            var weapon = Instantiate(weaponFVRObject.GetGameObject());

            weapon.transform.position = boxPosition;

            if (Random.Range(0, 2) == 1) return; /* 50% Chance of spawning a magazine with the gun */
            
            /*var compatibleMags = weaponFVRObject.CompatibleMagazines;
            
            Debug.Log("magazine array lenght" + compatibleMags.Count);
            var magazineFVRObject = compatibleMags[Random.Range(0, compatibleMags.Count + 1)];*/

            var randomCompatibleMag = weaponFVRObject.GetRandomAmmoObject(weaponFVRObject);
            
            Debug.Log("Getting Object");

            AnvilManager.Run(SpawnObject(boxPosition, randomCompatibleMag));
        }
        
        private static void GenerateSingleMagazine(Vector3 boxPosition)
        {
            var lengthOfArray = magazineArray.Length;
            var magazine = Instantiate(magazineArray[Random.Range(0, lengthOfArray + 1)].GetGameObject());
            magazine.transform.position = boxPosition;
        }
    }
}