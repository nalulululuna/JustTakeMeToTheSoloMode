using HMUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JustTakeMeToTheSoloMode
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class JustTakeMeToTheSoloModeController : MonoBehaviour
    {
        private static readonly string ButtonTextContinue = "Continue";
        private static readonly string ButtonTextSolo = "SoloFreePlayButton";

        private void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            Logger.log?.Debug($"nextScene.name={nextScene.name}");

            if (nextScene.name == "HealthWarning")
            {
                StartCoroutine(buttonClickCoroutine(ButtonTextContinue));
            }
            else if (nextScene.name == "MenuViewControllers")
            {
                StartCoroutine(buttonClickCoroutine(ButtonTextSolo));
            }
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
            yield return new WaitUntil(() => Resources.FindObjectsOfTypeAll<Button>().Any(x => x.name == buttonName));
            Logger.log?.Debug($"Time.time={Time.time}, Found");
            Button button = Resources.FindObjectsOfTypeAll<Button>().Where(x => x.name == buttonName).First();

            // need some wait
            yield return new WaitForSecondsRealtime(Configuration.PluginConfig.Instance.wait);

            button.onClick.Invoke();

            if ((Configuration.PluginConfig.Instance.selectTab >= 0) || (Configuration.PluginConfig.Instance.selectTab <= 3))
            {
                // need some wait
                yield return new WaitForSecondsRealtime(1f);

                LevelFilteringNavigationController levelFilteringNavigationController = Resources.FindObjectsOfTypeAll<LevelFilteringNavigationController>().First();
                TabBarViewController tabBarViewController = levelFilteringNavigationController.GetPrivateField<TabBarViewController>("_tabBarViewController");
                tabBarViewController.SelectItem(Configuration.PluginConfig.Instance.selectTab);
                levelFilteringNavigationController.SwitchToPlaylists();
            }

            // no longer needed
            if (buttonName == ButtonTextSolo)
            {
                Destroy(gameObject);
            }
        }

        public static JustTakeMeToTheSoloModeController instance { get; private set; }

        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            if (instance != null)
            {
                Logger.log?.Warn($"Instance of {this.GetType().Name} already exists, destroying.");
                GameObject.DestroyImmediate(this);
                return;
            }
            GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            instance = this;
            Logger.log?.Debug($"{name}: Awake()");
        }
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            Logger.log?.Debug($"{name}: Start()");

        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {

        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Logger.log?.Debug($"{name}: OnDestroy()");
            instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
        }
        #endregion
    }
}
