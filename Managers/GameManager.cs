using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Naninovel;

public class GameManager : MonoBehaviour
{
    static GameManager s_instance;
    public static GameManager Inst { get { Init(); return s_instance; } }

    PoolManager _pool = new PoolManager();
    UserData _userData = new UserData();
    public static PoolManager Pool { get { return Inst._pool; } }
    public static BattleManager Battle { get { return BattleManager.Inst; } }
    public static UserData UserData { get { return Inst._userData; } }

    private void Awake()
    {
        Init();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@GameManager");
            if (go == null)
            {
                go = new GameObject { name = "@GameManager" };
                go.AddComponent<GameManager>();
            }

            s_instance = go.GetComponent<GameManager>();

            s_instance._pool.Init();
            s_instance._userData.Init();
        }

    }

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
            naniCamera.enabled = false;
        }
    }
    public Camera MainCam;
    [ContextMenu("스토리 시작")]
    public async void ShowGold()
    {
        // 2. Switch cameras.
        var naniCamera = Engine.GetService<ICameraManager>().Camera;
        naniCamera.enabled = true;
        var uiCamera = Engine.GetService<ICameraManager>().UICamera;
        uiCamera.enabled = true;

        MainCam.enabled = false;

        var scriptPlayer = Engine.GetService<IScriptPlayer>();
        await scriptPlayer.PreloadAndPlayAsync("Story2");

        // 4. Enable Naninovel input.
        var inputManager = Engine.GetService<IInputManager>();
        inputManager.ProcessInput = true;

        var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    [CommandAlias("adventure")]
    public class SwitchToAdventureMode : Command
    {
        public override async UniTask ExecuteAsync(AsyncToken asyncToken)
        {
            // 1. Disable Naninovel input.
            var inputManager = Engine.GetService<IInputManager>();
            inputManager.ProcessInput = false;
            Inst.MainCam.enabled = true;

            // 2. Stop script player.
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            scriptPlayer.Stop();

            // 3. Reset state.
            var stateManager = Engine.GetService<IStateManager>();
            await stateManager.ResetStateAsync();

            // 4. Switch cameras.
            var naniCamera = Engine.GetService<ICameraManager>().Camera;
            naniCamera.enabled = false;
            var uiCamera = Engine.GetService<ICameraManager>().UICamera;
            uiCamera.enabled = false;

            var canvasGroup = GameObject.Find("UI").GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
