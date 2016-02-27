using DxLibDLL;
using System;

namespace RPG10min
{
    class Utils
    {
        /// <summary>
        /// 値が指定のフラグを含んでいるか
        /// </summary>
        /// <param name="value">値</param>
        /// <param name="flag">確認フラグ値</param>
        /// <returns>True:含まれる</returns>
        public static Boolean IsOnFlag(Int32 value, Int32 flag)
        {
            // 確認フラグ値が「0」の場合は値も「0」でなければ含んでいる扱いにしない
            if (flag == 0 && value != 0)
            {
                return false;
            }
            if ((value & flag) == flag)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 実値から画面上の値へ変換
        /// </summary>
        /// <param name="val">実値</param>
        /// <returns>画面値</returns>
        public static float ValueToWindow(Int32 val)
        {
            return val / GameConfig.MOVE_MAGNIFICATION;
        }
        /// <summary>
        /// 実値から画面上の値へ変換
        /// </summary>
        /// <param name="val">実値</param>
        /// <returns>画面値</returns>
        public static Int32 WindowToValue(Int32 val)
        {
            return val * GameConfig.MOVE_MAGNIFICATION;
        }
    }
}
