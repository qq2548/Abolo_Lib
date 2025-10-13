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
        public string GameItemDataPath;
        /// <summary>
        /// 游戏敌人配置数据路径
        /// </summary>
        public string GameEnemyDataPath;
                /// <summary>
        /// 游戏道具Icon资源路径配置
        /// </summary>
        public string GameItemResPath;
        /// <summary>
        /// 游戏UI预制体资源路径配置
        /// </summary>
        public string GameUIFactoryPath;
        /// <summary>
        /// 游戏特效预制体资源路径配置
        /// </summary>
        public string GameEffectFactoryPath;
        /// <summary>
        /// 游戏音效资源路径配置
        /// </summary>
        public string GameAudioPresetsPath;
        /// <summary>
        /// 游戏道具预制体资源路径配置
        /// </summary>
        public string GameItemFactoryPath;        
        /// <summary>
        /// 游戏存档名称配置
        /// </summary>
        public string GameSaveDataName;       
    }
}
