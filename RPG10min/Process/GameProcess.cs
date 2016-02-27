using System;
using DxLibDLL;
using RPG10min.System;
using RPG10min.Events;

namespace RPG10min.Process
{
    public enum ProcessStates
    {
        Title,
        Load,
        Save,
        Map,
        Battle
    };

    public class GameProcess
    {
        public static KeyEvent Key;

        private ProcessAbstract _process;
        public ItemObject.Player Player
        {
            private set;
            get;
        }
        public MenuMain menu;

        public GameProcess()
        {
        }
        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Init()
        {
            // イベント
            GameProcess.Key = new KeyEvent();

            // TODO: とりあえずテスト用にマップ開始
            GameConfig.State = ProcessStates.Map;
            this.ChangeInitData(GameConfig.State);
            
            this.menu = new MenuMain();
        }

        /// <summary>
        /// メインループ処理
        /// </summary>
        /// <returns>処理状態　TRUE:継続 FALSE:終了</returns>
        public bool MainLoop()
        {
            // バックグラウンド描画
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);
            // スクリーン初期化
            DX.ClearDrawScreen();

            ProcessStates beforeState = GameConfig.State;
            Boolean isBeforeOpenedMenu = GameConfig.IsOpenMenu;

            // キー入力確認
            GameProcess.Key.CheckInputKey();

            // 進行状態ごとに処理
            switch (GameConfig.State)
            {
                case ProcessStates.Title:
                    break;
                case ProcessStates.Load:
                    break;
                case ProcessStates.Save:
                    break;
                case ProcessStates.Map:
                    // 1. マップ処理
                    // 3. キャラクター処理
                    // 2. マップ描画
                    // 4. キャラクター描画
                    // 5. 前面マップ描画
                    this._process.Main();
                    this.Player.MainProcess();
                    this._process.Draw();
                    this.Player.Draw();
                    ((Map)this._process).DrawFront();
                    break;
                case ProcessStates.Battle:
                    break;
                default:
                    break;
            }

            // メニュー
            if (GameConfig.IsOpenMenu)
            {
                // メニュー開いた直後
                if (!isBeforeOpenedMenu)
                {
                    this.menu.Load();
                }
                this.menu.Main();
                this.menu.Draw();

            }

            // バックグラウンドの描画をフロントへ
            DX.ScreenFlip();

            ProcessStates afterState = GameConfig.State;
            // 進行状態の前後を比較して変更されていたら初期化
            if (!beforeState.Equals(afterState))
            {
                this.ChangeInitData(afterState);
            }

            return true;
        }

        private void ChangeInitData(ProcessStates newState)
        {
            switch (newState)
            {
                case ProcessStates.Title:
                    break;
                case ProcessStates.Load:
                    break;
                case ProcessStates.Save:
                    break;
                case ProcessStates.Map:
                    _process = new Map();
                    Player = new ItemObject.Player(0);
                    this._process.Load();
                    //// キー受付
                    //Key.OnInputKey callbackKey = this.player.InputKey;
                    //callbackKey += ((Map)this._process).InputKey;
                    //GameConfig.keySystem.callback = callbackKey;
                    break;
                case ProcessStates.Battle:
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// マップに設定されたカメラを取得
        /// </summary>
        /// <returns>カメラインスタンス</returns>
        public Map.Camera GetCamera()
        {
            if (GameConfig.State != ProcessStates.Map)
            {
                // 進行状態がマップの場合のみ
                return null;
            }
            Map map = ((Map)_process);
            if (map == null || map.camera == null)
            {
                // マップ、またはカメラのインスタンスが生成されていない場合はNG
                return null;
            }
            return map.camera;
        }

        /// <summary>
        /// マップを取得
        /// </summary>
        /// <returns>マップインスタンス</returns>
        public Map GetMap()
        {
            if (GameConfig.State != ProcessStates.Map)
            {
                // 進行状態がマップの場合のみ
                return null;
            }
            Map map = ((Map)_process);
            if (map == null)
            {
                // マップのインスタンスが生成されていない場合はNG
                return null;
            }
            return map;
        }
    }
}
