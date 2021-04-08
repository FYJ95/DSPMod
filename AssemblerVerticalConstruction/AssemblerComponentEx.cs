using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;

namespace AssemblerVerticalConstruction
{
    public class AssemblerComponentEx
    {

        public int[][] assemblerNextIds = new int[64*6][];
        public int assemblerCapacity = 64*6;
        public void SetAssemblerCapacity(int newCapacity){
            var array = this.assemblerNextIds;
            this.assemblerNextIds = new int[newCapacity][];
            if (array != null)
            {
                Array.Copy(array, this.assemblerNextIds, (newCapacity <= this.assemblerCapacity) ? newCapacity : assemblerCapacity);
            }
            this.assemblerCapacity = newCapacity;
        }

        public int GetNextId(int index, int assemblerId){

           if(index >= assemblerNextIds.Length)
           {
               return 0;
           }
           if (this.assemblerNextIds[index] == null || assemblerId >= this.assemblerNextIds[index].Length)
           {
                return 0;
           }
           return this.assemblerNextIds[index][assemblerId];
        }

         public void SetAssemblerInsertTarget(PlanetFactory __instance, int assemblerId, int nextEntityId)
        {
            
            var index = __instance.factorySystem.factory.index;
            if (index >= assemblerNextIds.Length)
            {
                this.SetAssemblerCapacity(this.assemblerCapacity * 2);
            }
            
            if (assemblerId != 0 && __instance.factorySystem.assemblerPool[assemblerId].id == assemblerId)
            {
                if (nextEntityId == 0)
                {
                    this.assemblerNextIds[index][assemblerId] = 0;
                }
                else
                {
                    this.assemblerNextIds[index][assemblerId] = __instance.entityPool[nextEntityId].assemblerId;
                    var nextAssemblerId = __instance.entityPool[nextEntityId].assemblerId;
                    if (nextAssemblerId != 0 && __instance.factorySystem.assemblerPool[assemblerId].recipeId != __instance.factorySystem.assemblerPool[nextAssemblerId].recipeId)
                    {
                        __instance.factorySystem.assemblerPool[nextAssemblerId].SetRecipe(__instance.factorySystem.assemblerPool[assemblerId].recipeId, __instance.factorySystem.factory.entitySignPool);
                    }
                }
            }
        }

         public void SetAssemblerNextId(int index, int assemblerId, int nextId)
         {
             if (index >= assemblerNextIds.Length)
             {
                 this.SetAssemblerCapacity(this.assemblerCapacity * 2);
             }

             if (assemblerNextIds[index] == null || assemblerId >= assemblerNextIds[index].Length)
             {
                 var array = this.assemblerNextIds[index];
               
                var newCapacity = assemblerId * 2;
                newCapacity = newCapacity > 256 ? newCapacity : 256;
                this.assemblerNextIds[index] = new int[newCapacity];
                 if (array != null)
                 {
                    var len = array.Length;
                    Array.Copy(array, this.assemblerNextIds[index], (newCapacity <= len) ? newCapacity : len);
                 }
             }
             this.assemblerNextIds[index][assemblerId] = nextId;
         }
        public void UpdateOutputToNext(int planeIndex, int assemblerId, AssemblerComponent[] assemblerPool)
        {
            if(planeIndex >= assemblerNextIds.Length || assemblerNextIds[planeIndex] == null || assemblerId >= assemblerNextIds[planeIndex].Length || assemblerId >= assemblerPool.Length)
            {
                return;
            }
            var assemblerNextId = assemblerNextIds[planeIndex][assemblerId];
            if (assemblerNextId >= assemblerPool.Length)
            {
                return;
            }
            var _this = assemblerPool[assemblerId];
            if (assemblerPool[assemblerNextId].id == 0 || assemblerPool[assemblerNextId].id != assemblerNextId)
            {
                Assert.CannotBeReached();
                this.assemblerNextIds[planeIndex][assemblerId] = 0;
            }
            if (assemblerPool[assemblerNextId].needs != null && _this.recipeId == assemblerPool[assemblerNextId].recipeId)
            {
                if (_this.served != null && assemblerPool[assemblerNextId].served != null)
                {
                    int num = _this.served.Length;
                    for (int i = 0; i < num; i++)
                    {
                        if (_this.needs[i] == 0 && assemblerPool[assemblerNextId].needs[i] == _this.requires[i] && _this.served[i] >= _this.requireCounts[i] * 5)
                        {
                            _this.served[i]--;
                            assemblerPool[assemblerNextId].served[i]++;
                        }
                    }
                }
                if (_this.produced != null && assemblerPool[assemblerNextId].produced != null)
                {
                    for (int l = 0; l < _this.productCounts.Length; l++)
                    {
                        var maxCount = _this.productCounts[l] * 9;
                        if (_this.produced[l] < maxCount && assemblerPool[assemblerNextId].produced[l] > 0  )
                        {
                            _this.produced[l]++;
                            assemblerPool[assemblerNextId].produced[l]--;
                        }
                    }
                }
            }
        }
     
    }
}
