using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnstableLandmines.Patches;


namespace UnstableLandmines
{


    [BepInPlugin(modGUID, modName, modVersion)]
    public class UnstableLandminesBase : BaseUnityPlugin
    {

        private const string modGUID = "MasterAli2.UnstableLandmines";
        private const string modName = "Unstable Landmines";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static UnstableLandminesBase instance;

        public ConfigEntry<bool> configExperimental;

        public ConfigEntry<bool> configLandmineUnstabilityTick;


        public ConfigEntry<float> configLandmineTickDuration;

        public ConfigEntry<float> configRemoteExplosionChance;

        public ConfigEntry<float> configRemoteRange;


        internal ManualLogSource mls;


        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            SetupConfig();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("UnstableLandmines has awaken");

            harmony.PatchAll(typeof(UnstableLandminesBase));
            harmony.PatchAll(typeof(UnstableLandminesPatch));

        }

        void SetupConfig()
        {

            configLandmineTickDuration = Config.Bind("General",
                                                     "LandmineTickDuration",
                                                     120f,
                                                     "if the player is on the landmine for more than X seconds it will go boom if value is smaller than 0 it will be turned off");

            configRemoteExplosionChance = Config.Bind("General",
                                                      "RemoteExplosionChance",
                                                      60f,
                                                      "The chances of a landmine exploding after usage on a landmine (looking at it) the other x% will be the chance of disabling the landmine");
            configRemoteRange = Config.Bind("General",
                                            "RemoteRange",
                                             10f,
                                            "The range the remote needs to be close to the landmine for it to take affect");
            configLandmineUnstabilityTick = Config.Bind("General",
                                                        "configLandmineUnstabilityTick",
                                                         false,
                                                        "shuldd landmines randomly blew up randomly blew up? kinda experimental idk");


            configExperimental = Config.Bind("Experimental",
                                            "Experimental",
                                            false,
                                            "Use Experimental features? [some things that make landmine go boom if use walkie near landmine (random)]");

    }

    }
}
