using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using IPALogger = IPA.Logging.Logger;

namespace JustTakeMeToTheSoloMode
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin instance { get; private set; }
        internal static string Name => "JustTakeMeToTheSoloMode";

        [Init]
        public Plugin(IPALogger logger, Config conf)
        {
            instance = this;
            Logger.log = logger;
            Logger.log.Debug("Logger initialized.");

            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Logger.log.Debug("Config loaded");

            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private static readonly string ButtonTextContinue = "Continue";
        private static readonly string ButtonTextSolo = "SoloFreePlayButton";

        private void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            Logger.log?.Debug($"nextScene.name={nextScene.name}");

            if (nextScene.name == "HealthWarning")
            {
                PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(buttonClickCoroutine(ButtonTextContinue));
            }
            else if (nextScene.name == "MenuViewControllers")
            {
                Logger.log?.Debug($"OnActiveSceneChanged Disabled");
                SceneManager.activeSceneChanged -= OnActiveSceneChanged;

                PersistentSingleton<SharedCoroutineStarter>.instance.StartCoroutine(buttonClickCoroutine(ButtonTextSolo));
            }
        }

        private bool AreSongsLoading(MonoBehaviour loader)
        {
            var prop = loader.GetType().GetProperty("AreSongsLoading", BindingFlags.Public | BindingFlags.Static);
            return (bool)prop.GetValue(loader);
        }

        private IEnumerator buttonClickCoroutine(string buttonName)
        {
            /*
            foreach (Button b in Resources.FindObjectsOfTypeAll<Button>())
            {
                Logger.log?.Debug($"Button.name={b.name}");
            }
            */

            Logger.log?.Debug($"Time.time={Time.time}, WaitUntil={buttonName}");
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<Button>().Any(x => x != null && x.name == buttonName));
            Logger.log?.Debug($"Time.time={Time.time}, Found");
            Button button = Resources.FindObjectsOfTypeAll<Button>().Where(x => x != null && x.name == buttonName).First();

            // need some wait
            yield return new WaitForSecondsRealtime(Configuration.PluginConfig.Instance.wait);

            // wait for SongCore
            GameObject songCoreLoader = GameObject.Find("SongCore Loader");
            if (songCoreLoader != null)
            {
                Logger.log?.Debug($"Time.time={Time.time}, SongCore Found");
                foreach (var monoBehaviour in songCoreLoader.GetComponents<MonoBehaviour>())
                {
                    if (monoBehaviour.GetType().Name == "Loader")
                    {
                        Logger.log?.Debug($"Time.time={Time.time}, SongCore Loader Found");

                        Logger.log?.Debug($"Time.time={Time.time}, WaitUntil AreSongsLoading");
                        yield return new WaitUntil(() => AreSongsLoading(monoBehaviour) == false);
                        Logger.log?.Debug($"Time.time={Time.time}, SongsLoaded");

                        // need some wait for update screen
                        yield return new WaitForSecondsRealtime(1f);

                        break;
                    }
                }
            }

            button.onClick.Invoke();

            if ((Configuration.PluginConfig.Instance.selectTab >= 0) && (Configuration.PluginConfig.Instance.selectTab <= 3))
            {
                // need some wait
                yield return new WaitForSecondsRealtime(Configuration.PluginConfig.Instance.wait);

                LevelFilteringNavigationController levelFilteringNavigationController = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
                TabBarViewController tabBarViewController = levelFilteringNavigationController?.GetField<TabBarViewController, LevelFilteringNavigationController>("_tabBarViewController");
                if (levelFilteringNavigationController != null && tabBarViewController != null)
                {
                    tabBarViewController.SelectItem(Configuration.PluginConfig.Instance.selectTab);
                    levelFilteringNavigationController.SwitchToPlaylists();
                }
            }

            Logger.log?.Debug($"Time.time={Time.time}, Done");
        }
    }
}
