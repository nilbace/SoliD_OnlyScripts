using Naninovel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Naninovel에셋을 관리
/// </summary>
public class NovelManager
{
    public Camera MainCam;

    /// <summary>
    /// 나니노벨 엔진 실행시 필요한 로직들을 추가적으로 설정함
    /// 
    /// 현재 추가 로직 목록
    /// 1.카메라 CullingMask관리(미니맵이나 카드 등을 보이지 않게 하기 위함)
    /// </summary>
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

    /// <summary>
    /// 특정 이름의 이야기를 시작
    /// </summary>
    /// <param name="e_StoryType"></param>
    public void StartStory(E_MysteryType e_StoryType)
    {
        StartStory(e_StoryType.ToString());
    }

    private async void StartStory(string storyName)
    {
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;
        var uiCamera = Engine.GetService<ICameraManager>().UICamera;
        uiCamera.enabled = true;

        MainCam = GameManager.GetMainCam();
        MainCam.enabled = false;

        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        await scriptPlayer.PreloadAndPlayAsync(storyName);

        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;

        var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    [CommandAlias("closeStory")]
    public class CloseStory : Command
    {
        public override async UniTask ExecuteAsync(AsyncToken asyncToken)
        {
            var inputManager = Engine.GetService<IInputManager>();
            inputManager.ProcessInput = false;

            GameManager.Novel.MainCam.enabled = true;

            var naniCamera = Engine.GetService<ICameraManager>().Camera;
            naniCamera.enabled = false;
            var uiCamera = Engine.GetService<ICameraManager>().UICamera;
            uiCamera.enabled = false;

            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            scriptPlayer.Stop();

            var stateManager = Engine.GetService<IStateManager>();
            await stateManager.ResetStateAsync();

            var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public async UniTask CallSwitchToAdventureMode()
    {
        var closeStory = new CloseStory();
        await closeStory.ExecuteAsync(default);
    }
}
