using Naninovel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NovelManager
{
    public Camera MainCam;

    [RuntimeInitializeOnLoadMethod]
    private static void ModifyConfig()
    {
        if (Engine.Initialized) OnInitializationFinished();
        else Engine.OnInitializationFinished += OnInitializationFinished;

        void OnInitializationFinished()
        {
            Engine.OnInitializationFinished -= OnInitializationFinished;

            var cameraConfig = Engine.GetConfiguration<CameraConfiguration>();
            cameraConfig.ReferenceResolution = new Vector2Int(3840, 2160);

            var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var uiCamera = Engine.GetService<ICameraManager>().UICamera;
            uiCamera.enabled = false;
            var naniCamera = Engine.GetService<ICameraManager>().Camera;

            // 모든 레이어를 비활성화합니다.
            naniCamera.cullingMask = 0;

            // Default 레이어를 활성화합니다.
            int defaultLayer = LayerMask.NameToLayer("Default");
            if (defaultLayer != -1)
            {
                naniCamera.cullingMask |= (1 << defaultLayer);
            }
            else
            {
                Debug.LogError("Default Layer not found.");
            }


            naniCamera.enabled = false;
        }
    }

    public void StartStory(E_MysteryType e_StoryType)
    {
        StartStory(e_StoryType.ToString());
    }

    public async void StartStory(string storyName)
    {
        // 2. Switch cameras.
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;
        var uiCamera = Engine.GetService<ICameraManager>().UICamera;
        uiCamera.enabled = true;

        MainCam = GameManager.GetMainCam();
        MainCam.enabled = false;

        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        await scriptPlayer.PreloadAndPlayAsync(storyName);

        // 4. Enable Naninovel input.
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;

        var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    [CommandAlias("closeStory")]
    public class SwitchToAdventureMode : Command
    {
        public override async UniTask ExecuteAsync(AsyncToken asyncToken)
        {
            // 1. Disable Naninovel input.
            var inputManager = Engine.GetService<IInputManager>();
            inputManager.ProcessInput = false;

            GameManager.Novel.MainCam.enabled = true;

            var naniCamera = Engine.GetService<ICameraManager>().Camera;
            naniCamera.enabled = false;
            var uiCamera = Engine.GetService<ICameraManager>().UICamera;
            uiCamera.enabled = false;

            // 2. Stop script player.
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            scriptPlayer.Stop();

            // 3. Reset state.
            var stateManager = Engine.GetService<IStateManager>();
            await stateManager.ResetStateAsync();

            // 4. Switch cameras.
            var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public async UniTask CallSwitchToAdventureMode()
    {
        var switchToAdventureMode = new SwitchToAdventureMode();
        await switchToAdventureMode.ExecuteAsync(default);
    }
}
