using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using NineSolsAPI;
using NineSolsAPI.Utils;
using UnityEngine.SceneManagement;
using System;
using HarmonyLib;

namespace ExampleMod;

// [BepInDependency(NineSolsAPICore.PluginGUID)] if you want to use the API
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class ExampleMod : BaseUnityPlugin {
    // https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/4_configuration.html
    private ConfigEntry<bool> enableSomethingConfig;
    private ConfigEntry<KeyboardShortcut> somethingKeyboardShortcut;

    private AssetBundle cubeBundle;
    private AssetBundle sceneBundle;
    private AssetBundle monstersBundle;
    
    private GameObject cube;
    private GameObject pepe;

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
        KeybindManager.Add(this, TestMethod2, KeyCode.Z);
        KeybindManager.Add(this, TestMethod3, KeyCode.X);
        KeybindManager.Add(this, Show, KeyCode.C);
        KeybindManager.Add(this, Hide, KeyCode.V);


        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");




        cubeBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.cube");
        sceneBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.scene");
        
        //bundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.cube.bundle").LoadAsset<GameObject>("Square");
        pepe = cubeBundle.LoadAsset<GameObject>("pepe");
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

    private void TestMethod() {
        if (!enableSomethingConfig.Value) return;

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
    private void Show()
    {
        ToastManager.Toast("C");
        string name = "";
        string name2 = "";


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

    private void Hide()
    {
        ToastManager.Toast("V");
        string name2 = "";
        //ToastManager.Toast(GameObject.Find(name2 + " (RCGLifeCycle)"));
        name2 = "StealthGameMonster_SpearHorseMan";
        SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        name2 = "Boss_Yi Gung";
        SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        name2 = "Monster_GiantMechClaw";
        SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        name2 = "StealthGameMonster_Boss_JieChuan";
        SingletonBehaviour<PoolManager>.Instance.BorrowOrInstantiate(GameObject.Find(name2 + " (RCGLifeCycle)"), Player.i.transform.position, Quaternion.identity, null, null);
        //SpawnMonster(GameObject.Find(name + " (RCGLifeCycle)").GetComponent<MonsterPoolObjectWrapper>());
    }

    private GameObject boss;

    private void TestMethod2()
    {
        //ToastManager.Toast("Cube");
        //pepe.layer = LayerMask.NameToLayer("OneWay");
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
        //GameCore.Instance.GoToScene("SampleScene");
        monstersBundle = AssemblyUtils.GetEmbeddedAssetBundle("ExampleMod.Resources.monsters");
        GameCore.Instance.GoToScene("A1_S2_ConnectionToElevator_Final");
        //Logger.LogInfo(GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant"));
        //RCGLifeCycle.DontDestroyForever(GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant"));
        //boss = GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant");


    }

    private void TestMethod3()
    {
        ToastManager.Toast("save");

        boss = GameObject.Find("StealthGameMonster_Samurai_General_Boss Variant");
        RCGLifeCycle.DontDestroyForever(boss);
        ToastManager.Toast(boss);
        if (monstersBundle != null)
        {
            monstersBundle.Unload(false);
        }
    }

    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading
        if(cubeBundle != null)
        {
            cubeBundle.Unload(false);
        }

        if (sceneBundle != null)
        {
            sceneBundle.Unload(false);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        Logger.LogInfo($"Scene loaded: {scene.name} {scene.GetRootGameObjects()} {GameObject.Find("A1_S2_GameLevel/Room/Prefab/Gameplay5/EventBinder/LootProvider/General Boss Fight FSM ObjectA1_S2_\u5927\u528d\u5175/FSM Animator/LogicRoot/---Boss---/BossShowHealthArea/StealthGameMonster_Samurai_General_Boss Variant")}");


    }
}