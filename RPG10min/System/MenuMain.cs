using DxLibDLL;
using RPG10min.Events;
using RPG10min.Process;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RPG10min.System
{
    public class MenuMain
    {
        //private KeyEvent.OnInputKey _beforeInputKey;
        /// <summary>
        /// メニュー選択欄
        /// </summary>
        private Window _menu;
        /// <summary>
        /// サブ情報表示欄
        /// </summary>
        private Window _subinfo;
        /// <summary>
        /// メッセージ表示欄
        /// </summary>
        private Window _message;

        public void Load()
        {
            //this._beforeInputKey = GameConfig.KeySystem.GetCallback();
            //KeyEvent.OnInputKey callbackKey = InputKey;
            //GameConfig.KeySystem.Callback = callbackKey;
            GameProcess.Key.OnInputKey += OnInputKey;
            // メニュー選択欄
            this._menu = new Window();
            this._menu.SetWindowInfo(0, 0, 150, 300);
            // サブ情報表示欄
            this._subinfo = new Window();
            this._subinfo.SetWindowInfo(150, 0, GameConfig.WINDOW_SIZE.Width - 150, GameConfig.WINDOW_SIZE.Height - 30);
            // メッセージ表示欄
            this._message = new Window();
            this._message.SetWindowInfo(0, GameConfig.WINDOW_SIZE.Height - 30, GameConfig.WINDOW_SIZE.Width, 30);
        }
        
        /// <summary>
        /// メイン処理
        /// </summary>
        public void Main()
        {
        }

        /// <summary>
        /// 描画
        /// </summary>
        public void Draw()
        {
            // ウィンドウを描画
            if (!this._menu.DrawWindow()) {
                return;
            }
            if (!this._subinfo.DrawWindow())
            {
                return;
            }
            if (!this._message.DrawWindow())
            {
                return;
            }
        }

        /// <summary>
        /// キー入力を確認する
        /// </summary>
        public void OnInputKey(Object sender, KeyEventArgs e)
        {
            if (e.Keys.Contains(KEY_MAPPING_TYPE.KEY_CANCEL))
            {
                // メニューを閉じる
                GameConfig.IsOpenMenu = false;
                // キー受付のコールバックを解除
                GameProcess.Key.OnInputKey -= OnInputKey;
            }
        }

    }
}
