using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AboloLib
{
    [CreateAssetMenu(menuName = "ArtUtils/GameConfig")]
    public class GameConfiguration : ScriptableObject
    {
        /// <summary>
        /// 游戏道具配置数据路径
        /// </summary>
        [Header("道具合成物配置数据文件路径")]
        public string GameItemDataPath;
        /// <summary>
        /// 游戏敌人配置数据路径
        /// </summary>
        [Header("敌人配置数据文件路径")]
        public string GameEnemyDataPath;
        /// <summary>
        /// 游戏道具配方配置数据路径
        /// </summary>
        [Header("道具合成物配方配置数据文件路径")]
        public string GameRecipeDataPath;
        /// <summary>
        /// 订单配置数据文件路径
        /// </summary>
        [Header("订单配置数据文件路径")]
        public string GameOrderDataPath;
        /// <summary>
        /// 游戏道具Icon资源路径配置
        /// </summary>
        [Header("道具合成物Icon资源路径")]
        public string GameItemResPath;
        /// <summary>
        /// 游戏UI预制体资源路径配置
        /// </summary>
        [Header("UI预制体资源路径")]
        public string GameUIFactoryPath;
        /// <summary>
        /// 游戏特效预制体资源路径配置
        /// </summary>
        [Header("特效预制体资源路径")]
        public string GameEffectFactoryPath;
        /// <summary>
        /// 游戏音效资源路径配置
        /// </summary>
        [Header("音效资源预设路径")]
        public string GameAudioPresetsPath;
        /// <summary>
        /// 游戏道具预制体资源路径配置
        /// </summary>
        [Header("合成物预制体资源路径")]
        public string GameItemFactoryPath;
        /// <summary>
        /// 游戏存档名称配置
        /// </summary>
        [Header("游戏存档名称")]
        public string GameSaveDataName;       
    }
}
