using DxLibDLL;
using RPG10min.Events;
using RPG10min.Process;
using RPG10min.System;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RPG10min.ItemObject
{
    public class Player: AbstractObject
    {
        public int Number;
        public int CharaNumber
        {
            private set;
            get;
        }
        public UInt32 DefaultMoveSpeed
        {
            private set;
            get;
        }
        private Process.Map map;
        /// <summary>
        /// これから移動する先の座標
        /// </summary>
        protected Point __willPosition;
        protected override Point _willPosition
        {
            set
            {
                if (this.map != null && this.map.camera != null)
                {
                    if (!this.map.camera.isLoopX)
                    {
                        // Xは中央に座標を置いているため÷2する
                        if (value.X < Utils.WindowToValue(GameConfig.CHIP_SIZE / 2))
                        {
                            value.X = Utils.WindowToValue(GameConfig.CHIP_SIZE / 2);
                        }
                        if (value.X > this.map.camera.cameraMax.X + Utils.WindowToValue(GameConfig.WINDOW_SIZE.Width / 2 - GameConfig.CHIP_SIZE / 2))
                        {
                            value.X = this.map.camera.cameraMax.X + Utils.WindowToValue(GameConfig.WINDOW_SIZE.Width / 2 - GameConfig.CHIP_SIZE / 2);
                        }
                    }
                    if (!this.map.camera.isLoopY)
                    {
                        if (value.Y < Utils.WindowToValue(GameConfig.CHIP_SIZE))
                        {
                            value.Y = Utils.WindowToValue(GameConfig.CHIP_SIZE);
                        }
                        if (value.Y > this.map.camera.cameraMax.Y + Utils.WindowToValue(GameConfig.WINDOW_SIZE.Height / 2))
                        {
                            value.Y = this.map.camera.cameraMax.Y + Utils.WindowToValue(GameConfig.WINDOW_SIZE.Height / 2);
                        }
                    }
                }
                this.__willPosition = value;
            }
            get { return this.__willPosition; }
        }

        public Player(int number)
        {
            this.Number = number;
            // TODO: キャラごとに固定になる
            this.CharaNumber = number;
            this.DefaultMoveSpeed = 20;
            this.MoveSpeed = this.DefaultMoveSpeed;
            this.IsEnabledMove = true;

            LoadImage("Data/Images/pl_boy.png");
            this.Position = new Point(Utils.WindowToValue(20 * GameConfig.CHIP_SIZE + this._size.Width / 2), Utils.WindowToValue(1 * GameConfig.CHIP_SIZE));
            this._willPosition = this.Position;

            // this._willPosition設定後でなければ未設定状態のカメラ最小、最大値を見て書き換えてしまう
            this.map = GameConfig.GameProcess.GetMap();

            // イベント設定
            this.SetEvents();
        }

        /// <summary>
        /// イベント設定
        /// </summary>
        private void SetEvents()
        {
            GameProcess.Key.OnInputKey += OnInputKey;
        }

        /// <summary>
        /// メイン処理
        /// </summary>
        public override void MainProcess()
        {
            //this.InputKey();
            // 移動中のみアニメーション
            this._isAnimation = this.IsMoving;
            // 移動が終了したら画像は停止状態で固定
            if (!this.IsMoving)
            {
                this._animationPattern = (short)Math.Floor((double)this._maxPatterX / 2);
            }
            // 移動
            this.Move();

            // ループをする場合のみ、座標がマップの最小/最大まで到達したらワープさせる
            // 移動先座標は現在座標を切り替えるタイミングで移動する
            Point loopPosition = this.Position;
            Point loopWillPosition = this._willPosition;
            if (this.map.camera.isLoopX)
            {
                if (loopPosition.X < this.map.camera.cameraMin.X)
                {
                    loopPosition.X += this.map.camera.cameraMax.X;
                    loopWillPosition.X += this.map.camera.cameraMax.X;
                }
                else if (loopPosition.X > this.map.camera.cameraMax.X)
                {
                    loopPosition.X -= this.map.camera.cameraMax.X;
                    loopWillPosition.X -= this.map.camera.cameraMax.X;
                }
            }
            if (this.map.camera.isLoopY)
            {
                if (loopPosition.Y < this.map.camera.cameraMin.Y)
                {
                    loopPosition.Y += this.map.camera.cameraMax.Y;
                    loopWillPosition.Y += this.map.camera.cameraMax.Y;
                }
                else if (loopPosition.Y > this.map.camera.cameraMax.Y)
                {
                    loopPosition.Y -= this.map.camera.cameraMax.Y;
                    loopWillPosition.Y -= this.map.camera.cameraMax.Y;
                }
            }
            this.Position = loopPosition;
            this._willPosition = loopWillPosition;
        }

        /// <summary>
        /// 移動情報を設定
        /// </summary>
        public void SetMove(int moveX, int moveY)
        {
            // ダッシュ有無
            // 移動途中で速度変更はできない
            if (!this.IsMoving)
            {
                //GameConfig.gameProcess.player.moveSpeed = (Key.IsInputKeyType(KEY_MAPPING_TYPE.KEY_SUBKEY))
                //    ? GameConfig.gameProcess.player.defaultMoveSpeed * 2
                //    : GameConfig.gameProcess.player.defaultMoveSpeed;
            }
            // 上下左右キーで移動と向きを切り替え
            // 移動中は受け付けない
            if (!this.IsMoving)
            {
                if (moveY < 0)
                {
                    this._direction = (short)ObjectDirection.NORTH;
                }
                else if (moveY > 0)
                {
                    this._direction = (short)ObjectDirection.SOUTH;
                }
                else if (moveX < 0)
                {
                    this._direction = (short)ObjectDirection.WEST;
                }
                else if (moveX > 0)
                {
                    this._direction = (short)ObjectDirection.EAST;
                }
                // 移動先を設定
                if (moveX != 0 || moveY != 0)
                {
                    // 今から移動しようとしている先が通行可能かどうか
                    // 基準位置は足元
                    if (!map.CanPassable(this.Position, moveX, moveY))
                    {
                        // 通行不可の場合はスルー
                        return;
                    }
                    Point willMovePos = Point.Add(this.Position, new Size(Utils.WindowToValue(moveX * GameConfig.CHIP_SIZE), Utils.WindowToValue(moveY * GameConfig.CHIP_SIZE)));
                    this._willPosition = new Point(willMovePos.X, willMovePos.Y);
                }
            }
        }
        /// <summary>
        /// キー入力を確認する
        /// </summary>
        public void OnInputKey(Object sender, KeyEventArgs e)
        {
            if (!this.IsMoving)
            {
                // ダッシュ有無
                // 移動途中で速度変更はできない
                GameConfig.GameProcess.Player.MoveSpeed = (KeyEvent.IsInputKeyType(KEY_MAPPING_TYPE.KEY_SUBKEY))
                    ? GameConfig.GameProcess.Player.DefaultMoveSpeed * 2
                    : GameConfig.GameProcess.Player.DefaultMoveSpeed;

                // 上下左右キーで移動と向きを切り替え
                // 移動中は受け付けない
                // 斜め移動不可
                int moveX = 0;
                int moveY = 0;
                if (e.Keys.Contains(KEY_MAPPING_TYPE.KEY_UP))
                {
                    moveY = -1;
                }
                else if (e.Keys.Contains(KEY_MAPPING_TYPE.KEY_DOWN))
                {
                    moveY = 1;
                }
                else if (e.Keys.Contains(KEY_MAPPING_TYPE.KEY_LEFT))
                {
                    moveX = -1;
                }
                else if (e.Keys.Contains(KEY_MAPPING_TYPE.KEY_RIGHT))
                {
                    moveX = 1;
                }
                this.SetMove(moveX, moveY);
                // カメラも合わせて移動
                if (this.map.camera != null)
                {
                    this.map.camera.SetMove(moveX, moveY);
                }
            }
        }


        public override void AfterDraw()
        {
            base.AfterDraw();
#if DEBUG
            DX.DrawString(0, 24, String.Format("Player ({0}, {1}) -> ({2}, {3})", Utils.ValueToWindow(this.Position.X), Utils.ValueToWindow(this.Position.Y), Utils.ValueToWindow(this._willPosition.X), Utils.ValueToWindow(this._willPosition.Y)), DX.GetColor(255, 255, 255));
#endif
        }
    }
}
