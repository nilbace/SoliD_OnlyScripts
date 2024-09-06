using Naninovel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Naninovel������ ����
/// </summary>
public class NovelManager
{
    public Camera MainCam;

    /// <summary>
    /// ���ϳ뺧 ���� ����� �ʿ��� �������� �߰������� ������
    /// 
    /// ���� �߰� ���� ���
    /// 1.ī�޶� CullingMask����(�̴ϸ��̳� ī�� ���� ������ �ʰ� �ϱ� ����)
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

            // ��� ���̾ ��Ȱ��ȭ�մϴ�.
            naniCamera.cullingMask = 0;

            // Default ���̾ Ȱ��ȭ�մϴ�.
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
    /// Ư�� �̸��� �̾߱⸦ ����
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
