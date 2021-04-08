using System;
using BepInEx;
using UnityEngine;
using BepInEx.Configuration;
using System.Security.Cryptography;
using HarmonyLib;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Deployment.Internal;
using System.Collections.Generic;

namespace AssemblerVerticalConstruction
{
    [BepInPlugin("bifrom.com.DSP.AssemblerVerticalConstruction", "[戴森球计划] 制造厂的垂直建造 by 丰有珏", "1.0.6")]
    // 限制只有戴森球计划可以使用当前插件
    [BepInProcess("DSPGAME.exe")]

  
    public class AssemblerVerticalConstructionConfig
    {
        public int ID = 0;
        public Vector3 LapJoint;
        public float ColliderDataOffset;
        public AssemblerVerticalConstructionConfig(int id, Vector3 lapJoint, float colliderDataOffset)
        {
            this.ID = id;
            this.LapJoint = lapJoint;
            this.ColliderDataOffset = colliderDataOffset;
        }
    }

    public class AssemblerVerticalConstruction : BaseUnityPlugin
    {
        public static ConfigEntry<bool> IsResetNextIds;
        public static ConfigEntry<string> AssemblerVerticalConstructionJson;
        public static List<AssemblerVerticalConstructionConfig> AssemblerVerticalConstructionConfigs = new List<AssemblerVerticalConstructionConfig>();
        public static AssemblerComponentEx assemblerComponentEx = new AssemblerComponentEx();
        public static BepInEx.Logging.ManualLogSource mylog = BepInEx.Logging.Logger.CreateLogSource("AssemblerVerticalConstruction");
        ~AssemblerVerticalConstruction()
        {
            if (IsResetNextIds.Value == true)
            {
                IsResetNextIds.Value = false;
                Config.Save();
            }
        }

        public string AssemblerVerticalConstructionConfigsToString()
        {
            string ret = "\n";
            for (int i = 0; i < AssemblerVerticalConstructionConfigs.Count; i++)
            {
                if (ret != "\n")
                {
                    ret += ",";
                }
                ret += AssemblerVerticalConstructionConfigs[i].ID + "|";
                ret += AssemblerVerticalConstructionConfigs[i].LapJoint.ToString()+"|";
                ret += AssemblerVerticalConstructionConfigs[i].ColliderDataOffset;
            }
            return ret;
        }

        void Start()
        {
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2303, new Vector3(0, 5.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2304, new Vector3(0, 5.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2305, new Vector3(0, 5.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2302, new Vector3(0, 4.3f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2309, new Vector3(0, 7.0f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2308, new Vector3(0, 16.0f, 0), -3));
                 
            IsResetNextIds = Config.Bind("config", "IsResetNextIds", false, "在加载存档的时候重新计算建筑物的叠加关系 true需要重新计算 重新计算的时候会有一定的卡顿");
            AssemblerVerticalConstructionJson = Config.Bind("config", "AssemblerVerticalConstructionJson", AssemblerVerticalConstructionConfigsToString(), "建筑间隔信息 ");

            Harmony.CreateAndPatchAll(typeof(AssemblerVerticalConstruction));
        }

        public static void ResetNextIds()
        {
            if (IsResetNextIds.Value == false)
            {
                return;
            }
            for (int i = 0; i < GameMain.data.factories.Length; i++)
            {
                if (GameMain.data.factories[i] == null)
                {
                    continue;
                }
                var _this = GameMain.data.factories[i].factorySystem;
                if (_this == null)
                {
                    continue;
                }
                var assemblerCapacity = Traverse.Create(_this).Field("assemblerCapacity").GetValue<int>();
                for (int j = 1; j < assemblerCapacity; j++)
                {
                    var assemblerId = j;
                    int entityId = _this.assemblerPool[assemblerId].entityId;
                    if (entityId == 0)
                    {
                        continue;
                    }
                    int num = entityId;
                    int num2 = 0;
                    do
                    {
                        bool flag;
                        int num3;
                        int num4;
                        int num5 = num;
                        _this.factory.ReadObjectConn(num, 15, out flag, out num3, out num4);
                        num = num3;
                        if (num > 0)
                        {
                          
                            int assemblerId3 = _this.factory.entityPool[num5].assemblerId;
                            int assemblerId2 = _this.factory.entityPool[num].assemblerId;
                            if (assemblerId2 > 0 && _this.assemblerPool[assemblerId2].id == assemblerId2)
                            {

                                assemblerComponentEx.SetAssemblerInsertTarget(GameMain.data.factories[i], assemblerId3, num);
                            }
                        }
                        if (num2 > 256)
                        {
                            break;
                        }
                    }
                    while (num != 0);
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemProto), "Preload")]
        private static bool PreloadPatch(ItemProto __instance, int _index)
         {
            ModelProto modelProto = LDB.models.modelArray[__instance.ModelIndex];
            if (modelProto != null && modelProto.prefabDesc != null && modelProto.prefabDesc.isAssembler == true)
             {
                Vector3 lapJoint = Vector3.zero;
                if (__instance.ID == 2303 || __instance.ID == 2304 || __instance.ID == 2305)
                {
                    lapJoint = new Vector3(0, 5.1f, 0);
                }
                else if (__instance.ID == 2302)
                {
                    lapJoint = new Vector3(0, 4.3f, 0);
                }
                else if (__instance.ID == 2309)
                {
                    lapJoint = new Vector3(0, 7.0f, 0);
                }else if (__instance.ID == 2308)
                {
                    lapJoint = new Vector3(0, 16.0f, 0);
                    
                }
                if (lapJoint != Vector3.zero)
                {
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.multiLevel = true;
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.multiLevelAllowInserter = true;
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.lapJoint = lapJoint;
                }
            }
            return true;
         }

        [HarmonyPrefix, HarmonyPatch(typeof(FactorySystem), "SetAssemblerCapacity")]
        private static bool SetAssemblerCapacityPatch(FactorySystem __instance, int newCapacity)
        {
            var index = __instance.factory.index;
            if (index > assemblerComponentEx.assemblerNextIds.Length)
            {
                assemblerComponentEx.SetAssemblerCapacity(assemblerComponentEx.assemblerCapacity * 2);
            }
            var assemblerCapacity = Traverse.Create(__instance).Field("assemblerCapacity").GetValue<int>();
            int[] array = assemblerComponentEx.assemblerNextIds[index];
            assemblerComponentEx.assemblerNextIds[index] = new int[newCapacity];
            if (array != null)
            {
                Array.Copy(array, assemblerComponentEx.assemblerNextIds[index], (newCapacity <= assemblerCapacity) ? newCapacity : assemblerCapacity);
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        public static bool ApplyInsertTargetPatch(PlanetFactory __instance, int entityId, int insertTarget, int slotId, int offset)
        {
            var _this = __instance;
            if (entityId == 0)
            {
                return true;
            }
            if (insertTarget < 0)
            {
                Assert.CannotBeReached();
                insertTarget = 0;
            }
            int assemblerId = _this.entityPool[entityId].assemblerId;
            if (assemblerId > 0 && _this.entityPool[insertTarget].assemblerId > 0)
            {
                assemblerComponentEx.SetAssemblerInsertTarget(__instance, assemblerId, insertTarget);
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        public static bool ApplyEntityDisconnectionPatch(PlanetFactory __instance, int otherEntityId, int removingEntityId, int otherSlotId, int removingSlotId)
        {
            if (otherEntityId == 0)
            {
                return true;
            }
            var _this = __instance;
            int assemblerId = _this.entityPool[otherEntityId].assemblerId;
            if (assemblerId > 0)
            {
                int assemblerId2 = _this.entityPool[removingEntityId].assemblerId;
                if (_this.entityPool[assemblerId].assemblerId == assemblerId2 && assemblerId2 > 0)
                {
                    assemblerComponentEx.SetAssemblerInsertTarget(__instance, assemblerId, 0);
                }
            }
            return true;
        }
       
        [HarmonyPostfix, HarmonyPatch(typeof(FactorySystem), "GameTick")]
        public static void GameTickPatch(FactorySystem __instance, long time, bool isActive)
        {
            var _this = __instance;
            var assemblerCursor = Traverse.Create(__instance).Field("assemblerCursor").GetValue<int>();
            for (int num17 = 1; num17 < assemblerCursor; num17++)
            {
                var NextId = assemblerComponentEx.GetNextId(__instance.factory.index, num17);
                if (_this.assemblerPool[num17].id == num17 && NextId > 0)
                {
                    assemblerComponentEx.UpdateOutputToNext(__instance.factory.index, num17, _this.assemblerPool);
                }
            }
            return ;
        }

        public static void SyncAssemblerFunctions(FactorySystem factorySystem, Player player, int assemblerId)
        {
            var _this = factorySystem;
            int entityId = _this.assemblerPool[assemblerId].entityId;
            if (entityId == 0)
            {
                return;
            }
            int num = entityId;
            int num2 = 0;
            do
            {
                bool flag;
                int num3;
                int num4;
                _this.factory.ReadObjectConn(num, 14, out flag, out num3, out num4);
                num = num3;
                if (num > 0)
                {
                    int assemblerId2 = _this.factory.entityPool[num].assemblerId;
                    if (assemblerId2 > 0 && _this.assemblerPool[assemblerId2].id == assemblerId2)
                    {
                       if (_this.assemblerPool[assemblerId].recipeId > 0)
                       {
                            if (_this.assemblerPool[assemblerId2].recipeId != _this.assemblerPool[assemblerId].recipeId)
                            {
                                _this.TakeBackItems_Assembler(player, assemblerId2);
                                _this.assemblerPool[assemblerId2].SetRecipe(_this.assemblerPool[assemblerId].recipeId, _this.factory.entitySignPool);
                            }
                       }
                       else if ( _this.assemblerPool[assemblerId2].recipeId != 0)
                       {
                            _this.TakeBackItems_Assembler(player, assemblerId2);
                            _this.assemblerPool[assemblerId2].SetRecipe(0, _this.factory.entitySignPool);
                       }
                    }
                }
                if (num2 > 256)
                {
                    break;
                }
            }
            while (num != 0);
            num = entityId;
            num2 = 0;
            do
            {
                bool flag;
                int num3;
                int num4;
                _this.factory.ReadObjectConn(num, 15, out flag, out num3, out num4);
                num = num3;
                if (num > 0)
                {
                    int assemblerId3 = _this.factory.entityPool[num].assemblerId;
                    if (assemblerId3 > 0 && _this.assemblerPool[assemblerId3].id == assemblerId3)
                    {
                        if (_this.assemblerPool[assemblerId].recipeId > 0)
                        {
                            if ( _this.assemblerPool[assemblerId3].recipeId != _this.assemblerPool[assemblerId].recipeId)
                            {
                                _this.TakeBackItems_Assembler(_this.factory.gameData.mainPlayer, assemblerId3);
                                _this.assemblerPool[assemblerId3].SetRecipe(_this.assemblerPool[assemblerId].recipeId, _this.factory.entitySignPool);
                            }
                        }
                        else if (_this.assemblerPool[assemblerId3].recipeId != 0)
                        {
                            _this.TakeBackItems_Assembler(_this.factory.gameData.mainPlayer, assemblerId3);
                            _this.assemblerPool[assemblerId3].SetRecipe(0, _this.factory.entitySignPool);
                        }
                    }
                }
                if (num2 > 256)
                {
                    break;
                }
            }
            while (num != 0);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIAssemblerWindow), "OnRecipeResetClick")]
        public static void OnRecipeResetClickPatch(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                return;
            }
            AssemblerComponent assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assemblerComponent.id != __instance.assemblerId)
            {
                return;
            }
            SyncAssemblerFunctions(__instance.factorySystem, __instance.player, __instance.assemblerId);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIAssemblerWindow), "OnRecipePickerReturn")]
        public static void OnRecipePickerReturnPatch(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                return;
            }
            AssemblerComponent assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assemblerComponent.id != __instance.assemblerId)
            {
                return;
            }
            SyncAssemblerFunctions(__instance.factorySystem, __instance.player, __instance.assemblerId);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlanetFactory), "PasteEntitySetting")]
        public static void PasteEntitySettingPatch(PlanetFactory __instance, int entityId)
        {
            if (entityId <= 0)
            {
                return;
            }
            EntitySettingDesc clipboard = EntitySettingDesc.clipboard;
            int assemblerId = __instance.entityPool[entityId].assemblerId;
            if (assemblerId != 0 && clipboard.type == EntitySettingType.Assembler && __instance.factorySystem.assemblerPool[assemblerId].recipeId == clipboard.recipeId)
            {
                ItemProto itemProto = LDB.items.Select((int)__instance.entityPool[entityId].protoId);
                if (itemProto != null && itemProto.prefabDesc != null)
                {
                    SyncAssemblerFunctions(__instance.factorySystem, __instance.gameData.mainPlayer, assemblerId);
                }
            }
            
        }


        public static void  IsAutoSave(string saveName){
            if(saveName != GameSave.AutoSaveTmp){
                return;
            }
            string text = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + GameSave.saveExt + ".acnext";
			string text2 = GameConfig.gameSaveFolder + GameSave.AutoSave0 + GameSave.saveExt + ".acnext";
			string text3 = GameConfig.gameSaveFolder + "_autosave_1" + GameSave.saveExt + ".acnext";
			string text4 = GameConfig.gameSaveFolder + "_autosave_2" + GameSave.saveExt + ".acnext";
			string text5 = GameConfig.gameSaveFolder + "_autosave_3" + GameSave.saveExt + ".acnext";
			if (File.Exists(text))
			{
				if (File.Exists(text5))
				{
					File.Delete(text5);
				}
				if (File.Exists(text4))
				{
					File.Move(text4, text5);
				}
				if (File.Exists(text3))
				{
					File.Move(text3, text4);
				}
				if (File.Exists(text2))
				{
					File.Move(text2, text3);
				}
				File.Move(text, text2);
			}
        }

        [HarmonyPrefix, HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
        public static bool SaveCurrentGamePatch(string saveName)
        {
            if (DSPGame.Game == null)
            {
                return true;
            }
            saveName = saveName.ValidFileName();
            string path = GameConfig.gameSaveFolder + saveName + GameSave.saveExt + ".acnext";
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(4194304))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                    {
                        binaryWriter.Write(-1);
                        binaryWriter.Write(assemblerComponentEx.assemblerCapacity);
                        for (int i = 0; i < assemblerComponentEx.assemblerCapacity; i++)
                        {
                            if (assemblerComponentEx.assemblerNextIds[i] != null)
                            {
                                binaryWriter.Write(assemblerComponentEx.assemblerNextIds[i].Length);
                                for (int j = 0; j < assemblerComponentEx.assemblerNextIds[i].Length; j++)
                                {
                                    binaryWriter.Write(assemblerComponentEx.assemblerNextIds[i][j]);
                                }
                            }
                            else
                            {
                                binaryWriter.Write(0);
                            }
                        }
                        using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            memoryStream.WriteTo(fileStream);
                        }
                        IsAutoSave(saveName);
                    }
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception); 
            }
            return true;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
        public static bool LoadCurrentGamePatch(bool __result, string saveName)
        {
            if (DSPGame.Game == null || __result == false)
            {
                return true;
            }
            saveName = saveName.ValidFileName();
            string path = GameConfig.gameSaveFolder + saveName + GameSave.saveExt + ".acnext";
            if (!File.Exists(path))
            {
                if (IsResetNextIds.Value == true)
                {
                    ResetNextIds();
                }
                return true;
            }
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader binaryReader = new BinaryReader(fileStream))
                    {
                        var assemblerCapacity = binaryReader.ReadInt32();
                        var lastAssemblerCapacity = assemblerCapacity;
                        if (assemblerCapacity < 0)
                        {
                            assemblerCapacity = binaryReader.ReadInt32();
                        }
                       
                        if (assemblerCapacity > assemblerComponentEx.assemblerCapacity)
                        {
                            assemblerComponentEx.SetAssemblerCapacity(assemblerCapacity);
                        }

                        for (int i = 0; i < assemblerCapacity; i++)
                        {
                            var num = binaryReader.ReadInt32();
                            for (int j = 0; j < num; j++)
                            {
                                var nextId = binaryReader.ReadInt32();
                                assemblerComponentEx.SetAssemblerNextId(i, j, nextId);
                            }
                        }
                        if (lastAssemblerCapacity > 0)
                        {
                            var len = assemblerComponentEx.assemblerNextIds.Length;
                            
                            if (len > 2 && assemblerComponentEx.assemblerNextIds[2] != null)
                            {
                                assemblerComponentEx.assemblerNextIds[1] = new int[assemblerComponentEx.assemblerNextIds[2].Length];
                                Array.Copy(assemblerComponentEx.assemblerNextIds[2], assemblerComponentEx.assemblerNextIds[1], assemblerComponentEx.assemblerNextIds[2].Length);
                            }
                            if (len > 2 && assemblerComponentEx.assemblerNextIds[2] != null)
                            {
                                assemblerComponentEx.assemblerNextIds[0] = new int[assemblerComponentEx.assemblerNextIds[2].Length];
                                Array.Copy(assemblerComponentEx.assemblerNextIds[0], assemblerComponentEx.assemblerNextIds[2], assemblerComponentEx.assemblerNextIds[0].Length);
                            }
                            if (len > 3 && assemblerComponentEx.assemblerNextIds[3] != null)
                            {
                                assemblerComponentEx.assemblerNextIds[0] = new int[assemblerComponentEx.assemblerNextIds[3].Length];
                                Array.Copy(assemblerComponentEx.assemblerNextIds[3], assemblerComponentEx.assemblerNextIds[0], assemblerComponentEx.assemblerNextIds[3].Length);
                            }
                        }
                    }
                }

                if (IsResetNextIds.Value == true)
                {
                    ResetNextIds();
                }
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogException(exception);

            }
            return true;
        }


        public static bool ObjectIsAssembler(PlayerAction_Build __instance, int objId)
        {
            if (objId == 0)
            {
                return false;
            }
            if (objId > 0)
            {
                ItemProto itemProto = LDB.items.Select((int)__instance.player.factory.entityPool[objId].protoId);
                return itemProto != null && itemProto.prefabDesc.isAssembler;
            }
            ItemProto itemProto2 = LDB.items.Select((int)__instance.player.factory.prebuildPool[-objId].protoId);
            return itemProto2 != null && itemProto2.prefabDesc.isAssembler;
        }

       // [HarmonyPrefix, HarmonyPatch(typeof(PlayerAction_Build), "CheckBuildConditions")]
        //public static bool CheckBuildConditionsPatch(PlayerAction_Build __instance, ref bool __result)
        //{
        //    GameHistoryData history = GameMain.history;
        //    bool flag = true;
        //    for (int i = 0; i < __instance.buildPreviews.Count; i++)
        //    {
        //        BuildPreview buildPreview = __instance.buildPreviews[i];
        //        bool isBelt = buildPreview.desc.isBelt;
        //        bool isInserter = buildPreview.desc.isInserter;
        //        bool flag2 = false;
        //        if (buildPreview.condition == EBuildCondition.Ok)
        //        {
        //            Vector3 vector = __instance.previewPose.position + __instance.previewPose.rotation * buildPreview.lpos;
        //            Vector3 vector2 = __instance.previewPose.position + __instance.previewPose.rotation * buildPreview.lpos2;
        //            if (vector.sqrMagnitude < 1f)
        //            {
        //                return true;
        //            }
        //            else
        //            {
        //                /*if (buildPreview.coverObjId == 0 || buildPreview.willCover)
        //                {
        //                    int id = buildPreview.item.ID;
        //                    int num = 1;
        //                    if (__instance.tmpInhandId == id && __instance.tmpInhandCount > 0)
        //                    {
        //                        num = 1;
        //                        __instance.tmpInhandCount--;
        //                    }
        //                    else
        //                    {
        //                        __instance.tmpPackage.TakeTailItems(ref id, ref num, false);
        //                    }
        //                    if (num == 0)
        //                    {
        //                        return true;
        //                    }
        //                }*/
        //                if (buildPreview.desc.isAssembler && buildPreview.desc.hasBuildCollider)
        //                {
        //                    Pose pose;
        //                    pose.position = __instance.previewPose.position + __instance.previewPose.rotation * buildPreview.lpos;
        //                    pose.rotation = __instance.previewPose.rotation * buildPreview.lrot;
        //                    ColliderData[] buildColliders = buildPreview.desc.buildColliders;
        //                    for (int num57 = 0; num57 < buildColliders.Length; num57++)
        //                    {
        //                        ColliderData colliderData = buildPreview.desc.buildColliders[num57];
        //                        colliderData.pos = pose.position + pose.rotation * colliderData.pos;
        //                        colliderData.q = pose.rotation * colliderData.q;

        //                        colliderData.pos.y = colliderData.pos.y - 3.0f;
        //                        colliderData.ext.y = colliderData.ext.y - 3.0f;
        //                        //mylog.LogInfo("目标是建造类建筑没错了:" + buildPreview.item.name);
        //                        int buildMode = buildPreview.item.BuildMode;
        //                        bool flag4 = buildMode == 1;
        //                        if (flag4)
        //                        {
        //                            int layermask = 165888;
        //                            if (Physics.CheckBox(colliderData.pos, colliderData.ext, colliderData.q, layermask, QueryTriggerInteraction.Collide))
        //                            {
        //                                if (ObjectIsAssembler(__instance, buildPreview.inputObjId))
        //                                {
                                          
        //                                    //mylog.LogInfo("目标是建造类建筑没错了:" + buildPreview.item.name);
        //                                    __result = true;
        //                                }
        //                                else
        //                                {
        //                                    __result = false;
        //                                }
                                        
        //                                // 如果是建造类建筑则我这边处理
        //                                //mylog.LogInfo("是建造类建筑没错了:" + buildPreview.item.name);
        //                                //buildPreview.condition = EBuildCondition.Collide;
        //                                return true;
        //                            }
        //                            else
        //                            {
        //                                if (ObjectIsAssembler(__instance, buildPreview.inputObjId))
        //                                {
        //                                    __result = true;
        //                                    return false;
        //                                }
        //                                return true;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}
    }
}
