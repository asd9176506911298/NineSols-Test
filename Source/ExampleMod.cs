using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using NineSolsAPI;
using NineSolsAPI.Utils;
using UnityEngine.SceneManagement;
using System;
using HarmonyLib;
using Cysharp.Threading.Tasks.Triggers;
using Com.LuisPedroFonseca.ProCamera2D;
using RCGMaker.Core;
using System.IO;
using System.Collections.Generic;
using Auto.Utils;

namespace ExampleMod;

// [BepInDependency(NineSolsAPICore.PluginGUID)] if you want to use the API
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ExampleMod : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    private ConfigEntry<bool> enableSomethingConfig;
    private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut;

    private AssetBundle assetBundles;
    private AssetBundle cubeBundle;
    public static AssetBundle sceneBundle;
    private AssetBundle testBuundle;
    private AssetBundle monstersBundle;
    private AssetBundle tree;

    private GameObject cube;
    private GameObject pepe;
    private GameObject danceRemoveObject;

    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);

        enableSomethingConfig = Config.Bind("General.Something", "Enable", true, "Enable the thing");
        somethingKeyboardShortcut = Config.Bind("General.Something", "Shortcut",
            new KeyboardShortcut(KeyCode.Q, KeyCode.LeftShift), "Shortcut to execute");

        // If you want to use the modding API, enable it in the .csproj.
        // It provides utilities like the KeybindManager, utilities for Instantiating objects including the 
        // NineSols lifecycle hooks, displaying toast messages and preloading objects from other scenes.
        // If you do use the API make sure do download the [NineSolsAPI.dll](https://github.com/nine-sols-modding/NineSolsAPI/releases/)
        // and put it in BepInEx/plugins/.

        KeybindManager.Add(this, TestMethod, () => somethingKeyboardShortcut.Value);
        //KeybindManager.Add(this, z, KeyCode.Z);
        //KeybindManager.Add(this, x, KeyCode.X);
        //KeybindManager.Add(this, c, KeyCode.C);
        //KeybindManager.Add(this, v, KeyCode.V);

        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");



        assetBundles = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.AssetBundles");
        cubeBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.cube");
        sceneBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.scene");
        testBuundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.testscript");
        tree = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.tree");


        //bundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.cube.bundle").LoadAsset<GameObject>("Square");
        pepe = cubeBundle.LoadAsset<GameObject>("pepe");
        danceRemoveObject = tree.LoadAsset<GameObject>("danceRemoveObject");
        //newClip = tree.LoadAsset<AnimationClip>("treeAnimation");
        //newClip = tree.LoadAsset<AnimationClip>("wolfAnimation");
        //newClip = tree.LoadAsset<AnimationClip>("danceAnimation");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update() { 
       
    }
    public static string GetGameObjectPath(GameObject obj)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    void SpawnMonster(MonsterPoolObjectWrapper monster)
    {
        LoopWanderingPointGenerator overridePointGenerator = null;
        MonsterPoolObjectWrapper monsterPoolObjectWrapper = monster;
        MonsterPoolObjectWrapper monsterPoolObjectWrapper2;
        Logger.LogInfo(monsterPoolObjectWrapper.gameObject.scene.name);
        if (monsterPoolObjectWrapper.gameObject.scene == SceneManager.GetActiveScene())
        {
            Logger.LogInfo($"SpawnMonster In Scece {monsterPoolObjectWrapper.gameObject.name}");
            monsterPoolObjectWrapper2 = monsterPoolObjectWrapper;
            monsterPoolObjectWrapper2.gameObject.SetActive(true);
        }
        else
        {
            monsterPoolObjectWrapper2 = SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate<MonsterPoolObjectWrapper>(monsterPoolObjectWrapper, base.transform.position, Quaternion.identity, base.transform, null);
        }
        monsterPoolObjectWrapper2.transform.parent = null;
        MonsterBase monsterBase = monsterPoolObjectWrapper2.GetComponent<MonsterBase>();
        StealthWandering stealthWandering = monsterBase.FindState(MonsterBase.States.Wandering) as StealthWandering;
        StealthWanderingIdle stealthWanderingIdle = monsterBase.FindState(MonsterBase.States.WanderingIdle) as StealthWanderingIdle;
        FlyingMonsterWandering flyingMonsterWandering = monsterBase.FindState(MonsterBase.States.Wandering) as FlyingMonsterWandering;
        if (stealthWanderingIdle != null)
        {
            stealthWanderingIdle.newPosTime = 2f;
            stealthWanderingIdle.SinglePointIdle = false;
        }
        if (overridePointGenerator != null)
        {
            if (stealthWandering != null)
            {
                stealthWandering.wanderingPointGenerator.OverridePoints(overridePointGenerator.TargetPoints);
            }
            else if (flyingMonsterWandering != null)
            {
                flyingMonsterWandering.patrolPoints.Clear();
                flyingMonsterWandering.patrolPoints.Add(overridePointGenerator.TargetPoints[0].transform);
                flyingMonsterWandering.patrolPoints.Add(overridePointGenerator.TargetPoints[1].transform);
            }
        }
        else if (stealthWandering != null)
        {
            stealthWandering.wanderingPointGenerator.DetachFromParent();
        }
        monsterPoolObjectWrapper2.transform.position = base.transform.position;
        monsterPoolObjectWrapper2.gameObject.SetActive(true);
        //monsterBase.LevelReset();
        monsterBase.FacePlayer();
        monsterBase.UpdateScaleFacing();
        monsterBase.ChangeStateIfValid(MonsterBase.States.ZEnter);
        monsterBase.ForceEngage();

        var playerPos = Player.i.transform.position;
        if (Player.i.Facing == Facings.Right)
            monsterBase.transform.position = new Vector3(playerPos.x + 250, playerPos.y, 0f);
        else if (Player.i.Facing == Facings.Left)
            monsterBase.transform.position = new Vector3(playerPos.x - 250, playerPos.y, 0f);

    }
    public AnimationClip newClip;

    private void ReplaceAnimationClip(Animator animator)
    {
        // Get the RuntimeAnimatorController
        var controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            Logger.LogError("AnimatorController not found");
            return;
        }

        // If the controller is an AnimatorOverrideController, you can replace clips more easily
        if (controller is AnimatorOverrideController overrideController)
        {
            ReplaceClipInOverrideController(overrideController);
        }
        else
        {
            // If it's a normal RuntimeAnimatorController, you can assign a new AnimatorOverrideController
            var newOverrideController = new AnimatorOverrideController(controller);
            ReplaceClipInOverrideController(newOverrideController);
            animator.runtimeAnimatorController = newOverrideController;
        }
    }

    private void ReplaceClipInOverrideController(AnimatorOverrideController overrideController)
    {
        // Create an array to hold the original clips
        var overrides = overrideController.animationClips; // Get all original animation clips

        foreach (var originalClip in overrides)
        {
            ToastManager.Toast(originalClip.name);  
            List<string> clipNamesToReplace = new List<string> { "Idle", "Run", "Jump", "Fall", "FallToGround", "RunStart", "RunBreak", "TurnAround", "Jump_2_Fall", "RunStart" };

            // Check if the originalClip.name is in the list
            if (clipNamesToReplace.Contains(originalClip.name))
            {
                ToastManager.Toast($"Replacing {originalClip.name} with newClip");
                overrideController[originalClip] = newClip; // Replace with the new clip
            }
        }
    }

    private void TestMethod() {
        if (!enableSomethingConfig.Value) return;

        //ToastManager.Toast(GameObject.Find("StealthGameMonster_Minion"));
        //ToastManager.Toast(GameObject.Find("A3_S2/Room/Prefab/Gameplay_2/NPC_ChiYou_A3狀態FSM/FSM Animator").GetComponent<Animator>().runtimeAnimatorController.animationClips[0]);

        //Player.i.animator.runtimeAnimatorController.animationClips[2] = GameObject.Find("A3_S2/Room/Prefab/Gameplay_2/NPC_ChiYou_A3狀態FSM/FSM Animator").GetComponent<Animator>().runtimeAnimatorController.animationClips[0];
        ToastManager.Toast(Player.i.animator.gameObject.name);

        Vector3 dancePos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 35, Player.i.transform.position.z);
        GameObject skin = Instantiate(danceRemoveObject, dancePos, Quaternion.identity, GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").transform);
        skin.transform.localPosition = new Vector3 ( 0f, 11.2001f, 0f );
        skin.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").layer = LayerMask.NameToLayer("UI");
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").GetComponent<SpriteRenderer>().enabled = false;
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").SetActive(false);


        //Vector3 dancePos = new Vector3(Player.i.transform.position.x, Player.i.transform.position.y + 50, Player.i.transform.position.z);
        //dancePos.x -= 50;
        //dancePos.y -= 25;
        //Instantiate(danceRemoveObject, dancePos, Quaternion.Euler(0, 0, 45)).transform.SetParent(GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").transform);
        //dancePos.x += 50;
        //dancePos.y += 25;
        //Instantiate(danceRemoveObject, dancePos, Quaternion.Euler(0, 0, 0)).transform.SetParent(GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").transform);
        //dancePos.x += 50;
        //dancePos.y -= 25;
        //Instantiate(danceRemoveObject, dancePos, Quaternion.Euler(0, 0, -45)).transform.SetParent(GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").transform);


        //newClip = Player.i.animator.runtimeAnimatorController.animationClips[1];
        //newClip = GameObject.Find("A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/Animator(FSM)").GetComponent<Animator>().runtimeAnimatorController.animationClips[0];
        //newClip = tree.LoadAsset<AnimationClip>("treeAnimation");
        //newClip = tree.LoadAsset<AnimationClip>("wolfAnimation");
        //newClip = tree.LoadAsset<AnimationClip>("danceAnimation");
        //newClip = tree.LoadAsset<AnimationClip>("danceRemoveAnimation");

        //if (newClip == null)
        //{
        //    return;
        //}
        //ToastManager.Toast(newClip);
        //ToastManager.Toast(GameObject.Find("A3_S2/Room/Prefab/NPC_GuideFish A3Variant/General FSM Object/Animator(FSM)").GetComponent<Animator>().runtimeAnimatorController.animationClips[0]);

        //ReplaceAnimationClip(Player.i.animator);
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder").transform.localPosition = new Vector3(12f, 26.9551f, 0f);
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").SetActive(false);
        //Destroy(GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerSprite").GetComponent<SpriteRenderer>());

        //var camera = GameObject.Find("A1_S2_GameLevel/CameraCore");
        //ToastManager.Toast(camera);
        //camera.transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(camera);

        //ToastManager.Toast($"test {GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)")}");

        //var nest = GameObject.Find("A1_S2_GameLevel/Room/MonsterNest");
        //MonsterSpawner spawner = nest.GetComponentInChildren<MonsterSpawner>();

        //ToastManager.Toast($"test {GetGameObjectPath(spawner.spawnTargetList[0].gameObject)}");
        //ToastManager.Toast($"test {GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)")}");
        //SpawnMonster(GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)").GetComponent<MonsterPoolObjectWrapper>());
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(spawner.spawnTargetList[0].gameObject, Player.i.transform.position, Quaternion.identity, null, null);
        //SpawnMonster(spawner.spawnTargetList[0].gameObject);
        //Logger.LogInfo(monstersBundle.LoadAsset());
        //ToastManager.Toast("Shortcut activated");
        //ToastManager.Toast(AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.cube.bundle").LoadAsset<GameObject>("Square"));
        //Instantiate(pepe, Player.i.transform.position, Quaternion.identity);
        //ToastManager.Toast("AlwaysShowTrueBody");
        //foreach (var trueBody in UnityEngine.Object.FindObjectsOfType<ButterflyTrueBody>(true))
        //{
        //    trueBody.SetShowTrueBodyHint(ButterflyTrueBody.ShowTrueBodyHintScheme.AlwaysShowTrueBody);
        //}
        //GameCore.Instance.GoToScene("A1_S2_ConnectionToElevator_Final");
        //boss = GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)");
        //boss = GameObject.Find("StealthGameMonster_Minion_prefab");
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(boss, Player.i.transform.position, Quaternion.identity, null, null);
        //var copy = Instantiate(boss, Player.i.transform.position, Quaternion.identity);
        //AutoAttributeManager.AutoReference(copy);
        //AutoAttributeManager.AutoReferenceAllChildren(copy);

        //Traverse.Create(copy.GetComponent<MonsterBase>()).Field("monsterID").SetValue("");
        //var xx = Guid.NewGuid();
        //Traverse.Create(copy.GetComponent<GuidComponent>()).Field("guid").SetValue(xx);
        //Traverse.Create(copy.GetComponent<GuidComponent>()).Field("serializedGuid").SetValue(xx.ToByteArray());

        //var levelAwakeList = copy.GetComponentsInChildren<ILevelAwake>(true);
        //for (var i = levelAwakeList.Length - 1; i >= 0; i--)
        //{
        //    var context = levelAwakeList[i];
        //    try { context.EnterLevelAwake(); } catch (Exception ex) { Log.Error(ex.StackTrace); }
        //}
        //ToastManager.Toast($"ShowTrueBody");
        //foreach (var trueBody in UnityEngine.Object.FindObjectsOfType<ButterflyTrueBody>(true))
        //{
        //    trueBody.SetShowTrueBodyHint(ButterflyTrueBody.ShowTrueBodyHintScheme.AlwaysShowTrueBody);
        //}
    }
    //string name = "GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung";
    //string name2 = "StealthGameMonster_SpearHorseMan";
    //string name = "A2_S5_ BossHorseman_GameLevel/Room/StealthGameMonster_SpearHorseMan";
    //string name2 = "StealthGameMonster_SpearHorseMan";
    //string name = "A4_S5/MechClaw Game Play/Monster_GiantMechClaw";
    //string name2 = "Monster_GiantMechClaw";
    //string name = "A5_S5/Room/EventBinder/General Boss Fight FSM Object_\u7d50\u6b0a/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_JieChuan";
    //string name2 = "StealthGameMonster_Boss_JieChuan";
    private void c()
    {
        ToastManager.Toast("C");

        //StartMenuLogic.Instance.StartGame("A4_S4_Container_Final");

        //StartMenuLogic.Instance.StartGame("testScript");
        //string name = "";
        //string name2 = "";


        //name = "A2_S5_ BossHorseman_GameLevel/Room/StealthGameMonster_SpearHorseMan";
        //name2 = "StealthGameMonster_SpearHorseMan";
        //ToastManager.Toast(GameObject.Find(name));
        //GameObject.Find(name).transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find(name2));

        //name = "GameLevel/Room/Prefab/EventBinder/General Boss Fight FSM Object Variant/FSM Animator/LogicRoot/---Boss---/Boss_Yi Gung";
        //name2 = "Boss_Yi Gung";
        //ToastManager.Toast(GameObject.Find(name));
        //GameObject.Find(name).transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find(name2));

        //name = "A4_S5/MechClaw Game Play/Monster_GiantMechClaw";
        //name2 = "Monster_GiantMechClaw";
        //ToastManager.Toast(GameObject.Find(name));
        //GameObject.Find(name).transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find(name2));

        //name = "A5_S5/Room/EventBinder/General Boss Fight FSM Object_\u7d50\u6b0a/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Boss_JieChuan";
        //name2 = "StealthGameMonster_Boss_JieChuan";
        //ToastManager.Toast(GameObject.Find(name));
        //GameObject.Find(name).transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find(name2));


        //SpawnMonster(GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant (RCGLifeCycle)").GetComponent<MonsterPoolObjectWrapper>());

    }

    public Camera cameraToUse;
    public GameObject objectToCapture;
    public int width = 1024;
    public int height = 1024;

    void CaptureScreenshot()
    {
        // Find the camera dynamically if not set in inspector
        //if (cameraToUse == null)
        //{
        //    //cameraToUse = GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera").GetComponent<Camera>();
        //    cameraToUse = GameObject.Find("SceneCamera").GetComponent<Camera>();
        //}

        cameraToUse = GameObject.Find("SceneCamera").GetComponent<Camera>();


        GameObject SceneCamera = GameObject.Find("SceneCamera");
        if (SceneCamera != null)
        {
            SceneCamera.SetActive(false);
            SceneCamera.SetActive(true);
        }

        //cameraToUse = GameObject.Find("A4_S5/Room/Prefab/A4_Screen_Camera").GetComponent<Camera>();

        // Set the desired resolution (3840x2160)
        int width = 3840;
        int height = 2160;

        // Create a RenderTexture with the desired resolution
        RenderTexture rt = new RenderTexture(width, height, 24);
        cameraToUse.targetTexture = rt;

        // Set the background to transparent
        cameraToUse.clearFlags = CameraClearFlags.SolidColor;
        cameraToUse.backgroundColor = new Color(0, 0, 0, 0); // Fully transparent

        // Render the object to the RenderTexture
        RenderTexture.active = rt;
        cameraToUse.Render();

        // Create a Texture2D to read pixels from the RenderTexture
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGBA32, false);
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        // Convert the texture to PNG
        byte[] bytes = screenshot.EncodeToPNG();

        // Save the PNG to file
        string path = Path.Combine(Application.dataPath, "Screenshot.png");
        File.WriteAllBytes(path, bytes);

        // Clean up
        cameraToUse.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        Debug.Log("Screenshot saved to: " + path);
    }


    private void v()
    {
        //ToastManager.Toast("DontDestroyForever ZGunAndDoor");
        //string name = "A4_S4/ZGunAndDoor/ZGun FSM Object Variant";
        //string name = "A4_S4/ZGunAndDoor";
        //GameObject.Find(name).transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find("ZGunAndDoor"));


        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/Always Canvas/DialoguePlayer(KeepThisEnable)").GetComponent<Canvas>().enabled = false;
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/Always Canvas/DialoguePlayer(KeepThisEnable)/BackgroundCanvas").active = false;
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/UIManager/GameplayUICamera/Always Canvas/DialoguePlayer(KeepThisEnable)/DialoguePanel").transform.localPosition = new Vector3(0,100,0);
        GameObject amplifyLightingSystem = GameObject.Find("AmplifyLightingSystem");
        if (amplifyLightingSystem != null && amplifyLightingSystem.activeSelf)
        {
            amplifyLightingSystem.SetActive(false);
        }

        GameObject bubbleCamera = GameObject.Find("BubbleCamera");
        if (bubbleCamera != null && bubbleCamera.activeSelf)
        {
            bubbleCamera.SetActive(false);
        }


        CaptureScreenshot();



        //GameObject.Find("A9_S1/Room/Prefab/Shield Giant Bot Control Provider Variant").transform.SetParent(null);
        //RCGLifeCycle.DontDestroyForever(GameObject.Find("Shield Giant Bot Control Provider Variant"));

        //ToastManager.Toast("V");
        //string name2 = "";
        ////ToastManager.Toast(GameObject.Find(name2 + " (RCGLifeCycle)"));
        //name2 = "StealthGameMonster_SpearHorseMan";
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //name2 = "Boss_Yi Gung";
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //name2 = "Monster_GiantMechClaw";
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //name2 = "StealthGameMonster_Boss_JieChuan";
        //SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //SpawnMonster(GameObject.Find(name + " (RCGLifeCycle)").GetComponent<MonsterPoolObjectWrapper>());
    }

    private void z()
    {
        //ToastManager.Toast(GameObject.Find("General FSM Object/--[States]/FSM/[State] 立繪對話"));
        //GameObject.Find("General FSM Object/--[States]/FSM/[State] 立繪對話").GetComponent<GeneralState>().OnStateEnter();
        //GameCore.Instance.GoToScene("A1_S2_ConnectionToElevator_Final");
        //GameCore.Instance.GoToScene("A11_S0_Boss_YiGung");

        //string[] s = { "SampleScene" };
        //GameCore.Instance.allScenes.AddRange(s);
        //SceneManager.LoadScene("SampleScene");
        //ToastManager.Toast("Cube");
        //pepe.layer = LayerMask.NameToLayer("OneWay");
        //pepe.layer = LayerMask.NameToLayer("Solid");  
        //Instantiate(pepe, Player.i.transform.position, Quaternion.identity);
        //ToastManager.Toast(sceneBundle.GetAllAssetNames());
        //ToastManager.Toast("SampleScene");
        //string[] s = {"SampleScene"};
        //GameCore.Instance.allScenes.AddRange(s);
        //var sceneList = GameCore.Instance.allScenes;
        //ToastManager.Toast(sceneList.Last());
        ////StartMenuLogic.Instance.StartGame("SampleScene");
        //TeleportPointData x = new TeleportPointData();
        //x.sceneName = "SampleScene";
        //ApplicationCore.Instance.StartGameGoTo(x);
        //SceneManager.LoadScene("SampleScene");

        SceneManager.LoadScene("testScript");
        Player.i.AllFull();
        //GameCore.Instance.GoToScene("SampleScene");
        //monstersBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.monsters");
        //GameCore.Instance.GoToScene("A1_S2_ConnectionToElevator_Final");
        //Logger.LogInfo(GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant"));
        //RCGLifeCycle.DontDestroyForever(GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant"));
        //boss = GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant");


    }

    public void PlayerHealthUpdate()
    {

    }

    private void x()
    {
        ToastManager.Toast("x");
        var light = GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerLightmask");
        ToastManager.Toast(GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerLightmask"));
        light.transform.localScale = new Vector3(5000f, 5000f, 5000f);
        Player.i.transform.position = new Vector3(0f, 75f, 0f);
        GameObject.Find("CameraCore (RCGLifeCycle)").GetComponent<ProCamera2DNumericBoundaries>().enabled = false;

        //boss = GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant");
        //RCGLifeCycle.DontDestroyForever(boss);
        //ToastManager.Toast(boss);
        //if (monstersBundle != null)
        //{
        //    monstersBundle.Unload(false);
        //}
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading
        if (assetBundles != null)
        {
            assetBundles.Unload(false);
        }
        if (cubeBundle != null)
        {
            cubeBundle.Unload(false);
        }

        if (sceneBundle != null)
        {
            sceneBundle.Unload(false);
        }

        if (testBuundle != null)
        {
            testBuundle.Unload(false);
        }

        if (tree != null)
        {
            tree.Unload(false);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //GameObject.Find("GameCore(Clone)/RCG LifeCycle/PPlayer/RotateProxy/SpriteHolder/PlayerLightmask").active = false;
        //GameObject.Find("CameraCore (RCGLifeCycle)/DockObj/OffsetObj/ShakeObj/SceneCamera").GetComponent<Camera>().enabled = false;
        foreach (var x in Camera.allCameras)
        {
            ToastManager.Toast(x);
        }
        //Logger.LogInfo($"Scene loaded: {scene.name} {scene.GetRootGameObjects()} {GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant")}");

        //ToastManager.Toast($"OnSceneLoaded {GameObject.Find("GameObject")}");
        //GameObject.Find("Boss").AddComponent<code>();
        //GameObject.Find("Boss/Canvas/HealthBar").AddComponent<FloatingHealthBar>(); 
        
        //GameObject.Find("Boss/Canvas/HealthBar").transform.localScale = new Vector3(5000, 5000, 5000);
        //GameObject.Find("Boss").AddComponent<EyeOfCthulhu>();
        //GameObject.Find("Boss").transform.localScale = new Vector3(50, 50, 50);
        //GameObject.Find("Boss").AddComponent<BossBehavior>();
        //GameObject.Find("Square").AddComponent<code>();
        //GameObject.Find("Ball").AddComponent<Ball>();
        //GameObject.Find("GroundArea").AddComponent<Shooter>();

    }
}