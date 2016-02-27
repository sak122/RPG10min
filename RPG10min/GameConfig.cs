using DxLibDLL;
using RPG10min.System;
using System;
using System.Drawing;

namespace RPG10min
{
    /// <summary>
    /// キーマッピング用の機能リスト
    /// </summary>
    public enum KEY_MAPPING_TYPE
    {
        KEY_UP,
        KEY_DOWN,
        KEY_LEFT,
        KEY_RIGHT,
        KEY_CANCEL,
        KEY_ENTER,
        KEY_SUBKEY
    }

    public static class GameConfig
    {
        /// <summary>
        /// ウィンドウサイズ
        /// </summary>
        public static readonly Size WINDOW_SIZE = new Size(640, 480);
        /// <summary>
        /// チップ（正方形）のサイズ
        /// </summary>
        public const UInt16 CHIP_SIZE = 32;
        ///// <summary>
        ///// キャラクター移動サイズ
        ///// </summary>
        //public const UInt16 CHARA_WALK = 32;
        /// <summary>
        /// 移動時の倍率（小数変換）
        /// </summary>
        public const UInt16 MOVE_MAGNIFICATION = 10;
        /// <summary>
        /// 画面進行状態
        /// </summary>
        public static Process.ProcessStates State;
        /// <summary>
        /// 
        /// </summary>
        public static UInt32 MmainEventProcess;
        /// <summary>
        /// メニュー開閉可否
        /// </summary>
        public static Boolean CanOpenMenu;
        /// <summary>
        /// メニュー開閉
        /// </summary>
        public static Boolean IsOpenMenu;
        /// <summary>
        /// ゲームの流れ本体
        /// </summary>
        public static Process.GameProcess GameProcess;
        /// <summary>
        /// DB
        /// </summary>
        //public static File.SQLiteHelper sqlite;
        ///// <summary>
        ///// キー入力状態確認
        ///// </summary>
        //public static KeyEvent KeySystem;

        /// <summary>
        /// 機能と入力キーのマッピング
        /// </summary>
        public static int[][] KeyMapping =
        {
            new int[] { DX.KEY_INPUT_UP, DX.KEY_INPUT_NUMPAD8 },
            new int[] { DX.KEY_INPUT_DOWN, DX.KEY_INPUT_NUMPAD2 },
            new int[] { DX.KEY_INPUT_LEFT, DX.KEY_INPUT_NUMPAD4 },
            new int[] { DX.KEY_INPUT_RIGHT, DX.KEY_INPUT_NUMPAD6 },
            new int[] { DX.KEY_INPUT_ESCAPE, DX.KEY_INPUT_Z },
            new int[] { DX.KEY_INPUT_RETURN, DX.KEY_INPUT_X, DX.KEY_INPUT_SPACE },
            new int[] { DX.KEY_INPUT_LSHIFT, DX.KEY_INPUT_RSHIFT }
        };
    }
}
