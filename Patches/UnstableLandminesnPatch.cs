using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;
using UnityEngine.PlayerLoop;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows;
using Unity.Netcode;

namespace UnstableLandmines.Patches
{

    internal class UnstableLandminesPatch
    {
        [HarmonyPatch(typeof(Landmine), "Start")]
        [HarmonyPostfix]
        static void LandmineStartPatch(Landmine __instance)
        {
            if (UnstableLandminesBase.instance.configLandmineUnstabilityTick.Value)
            {
                __instance.StartCoroutine(UnstableLandminesPatch.UnstabilityTimer(__instance));
            }
        }

        [HarmonyPatch(typeof(Landmine), "OnTriggerEnter")]
        [HarmonyPostfix]
        static void LandmineOnTriggerEnterPatch(Collider other, Landmine __instance)
        {
            if (other.CompareTag("Player") && UnstableLandminesBase.instance.configLandmineTickDuration.Value > 0 && NetworkManager.Singleton.IsHost)
            {
                
                __instance.StartCoroutine(UnstableLandminesPatch.TickTimer(__instance));
            }
        }

        public static IEnumerator TickTimer(Landmine instance)
        {
            
                yield return new WaitForSeconds(UnstableLandminesBase.instance.configLandmineTickDuration.Value);

                if (instance.mineActivated)
                {
                    instance.TriggerMineOnLocalClientByExiting();
                }
            


        }
        public static IEnumerator UnstabilityTimer(Landmine instance)
        {
            
            yield return new WaitForSeconds(UnityEngine.Random.Range(TimeOfDay.Instance.lengthOfHours, TimeOfDay.Instance.totalTime));

            if (instance.mineActivated)
            {
                instance.TriggerMineOnLocalClientByExiting();
            }

        }

        [HarmonyPatch(typeof(RemoteProp), "ItemActivate")]
        [HarmonyPostfix]
        static void RemoteItemActivatePatch()
        {
            // this single function took days to fix.
            Vector3 rayOrigin = GameNetworkManager.Instance.localPlayerController.visorCamera.transform.position;
            Vector3 rayDirection = GameNetworkManager.Instance.localPlayerController.visorCamera.transform.forward;

            Ray ray = new Ray(rayOrigin, rayDirection);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, UnstableLandminesBase.instance.configRemoteRange.Value, 1 << 21) && hit.collider.gameObject.layer == 21)
            {
                Landmine target =  hit.collider.gameObject.GetComponentInChildren<Landmine>();
                if (target != null)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > UnstableLandminesBase.instance.configRemoteExplosionChance.Value)
                    {
                        target.TriggerMineOnLocalClientByExiting();
                    }
                    else
                    {
                        target.mineAnimator.SetTrigger("startIdle");
                        target.mineAnimator.speed = 0f;
                        target.mineActivated = false;
                    }
                }
     
            }
        }

        [HarmonyPatch(typeof(WalkieTalkie), "ItemActivate")]
        [HarmonyPostfix]
        static void WalkieTalkieItemActivatePatch(WalkieTalkie __instance)
        {
            // experimental feature cuz it dosent work properly i dont recemend
            if (UnstableLandminesBase.instance.configExperimental.Value)
            {
                Collider[] array = Physics.OverlapSphere(__instance.transform.position, 10f, 1 << 21);
                for (int i = 0; i < array.Length; i++)
                {
                    Landmine target = array[i].gameObject.GetComponentInChildren<Landmine>();
                    if (target != null)
                    {


                        float Chance = (1 / Mathf.Clamp(Vector3.Distance(__instance.transform.position, array[i].gameObject.transform.position), 1, 10) / 100);

                        //Chance = 1;

                        if (UnityEngine.Random.Range(0f, 1f) < Chance)
                        {
                            target.TriggerMineOnLocalClientByExiting();

                        }
                    }


                }
            }
        }


    }
}



