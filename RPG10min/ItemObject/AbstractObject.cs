using DxLibDLL;
using System;
using System.Drawing;

namespace RPG10min.ItemObject
{
    enum ObjectDirection: short
    {
        SOUTH,
        WEST,
        EAST,
        NORTH,
    }
    public abstract class AbstractObject
    {
        /// <summary>
        /// 画像アニメパターン水平方向最大値
        /// </summary>
        protected UInt16 _maxPatterX = 3;
        /// <summary>
        /// 画像アニメパターン垂直方向最大値
        /// </summary>
        protected UInt16 _maxPatterY = 4;
        /// <summary>
        /// 基準座標
        /// 実際の座標値の GameConfig.MOVE_MAGNIFICATION 倍の座標
        /// 足元の中央
        /// </summary>
        public virtual Point Position { set; get; }
        /// <summary>
        /// 中心座標
        /// </summary>
        public PointF Center
        {
            get
            {
                // 横幅がどのサイズであっても画像の中央が中心                      縦幅は足元を基準に画像高さの半分ほど上の位置
                return new PointF(Utils.ValueToWindow(this.Position.X), Utils.ValueToWindow(this.Position.Y) - this._size.Height / 2);
            }
        }
        /// <summary>
        /// オブジェクトのサイズ
        /// 画像サイズによって決まる
        /// </summary>
        protected Size _size;
        /// <summary>
        /// 画像パス
        /// </summary>
        //protected string _imagePath;
        /// <summary>
        /// 画像ハンドラ
        /// </summary>
        protected int[] _graphHandles;
        /// <summary>
        /// 画像表示向き
        /// </summary>
        protected short _direction;
        /// <summary>
        /// 画像アニメーションを行うか
        /// </summary>
        protected bool _isAnimation;
        /// <summary>
        /// 現在の画像アニメーションパターン
        /// </summary>
        protected short _animationPattern;
        /// <summary>
        /// 画像アニメ速度
        /// 値が少ないほど早い
        /// </summary>
        protected ushort _animationSpeed;
        /// <summary>
        /// 画像アニメ用カウンタ
        /// </summary>
        private ushort _animationCounter;
        /// <summary>
        /// 画像アニメ方向（1 or -1）
        /// </summary>
        private short _animationDirection;
        /// <summary>
        /// 移動可否
        /// </summary>
        public bool IsEnabledMove;
        /// <summary>
        /// 移動速度
        /// 設定値の(1/GameConfig.MOVE_MAGNIFICATION)の速度で移動する
        /// </summary>
        public uint MoveSpeed;
        /// <summary>
        /// これから移動する先の座標
        /// </summary>
        protected virtual Point _willPosition { set; get; }
        /// <summary>
        /// 移動中か
        /// </summary>
        public bool IsMoving
        {
            get { return !this._willPosition.Equals(this.Position); }
        }

        public void LoadImage(string path)
        {
            // 画像の全体サイズを取得
            int imgw, imgh;
            int graphAll = DX.LoadGraph(path);
            DX.GetGraphSize(graphAll, out imgw, out imgh);
            // 全体のサイズから指定分割数に合わせて１パターンあたりのサイズを取得
            this._size = new Size(imgw / this._maxPatterX, imgh / this._maxPatterY);
            // 画像の分割読み込み
            this._graphHandles = new int[this._maxPatterX * this._maxPatterY];

            DX.LoadDivGraph(path, this._graphHandles.Length, this._maxPatterX, this._maxPatterY, this._size.Width, this._size.Height, out _graphHandles[0]);
            // TODO: 画像アニメ設定も各オブジェクトクラスから設定する
            // 画像アニメのデフォルト設定
            this._direction = 0;
            this._animationPattern = 1;
            this._animationSpeed = 15;
            this._animationDirection = 1;
        }

        /// <summary>
        /// メイン処理
        /// 動作が全くないオブジェクトについては実装の必要なし
        /// </summary>
        public virtual void MainProcess()
        {
        }

        /// <summary>
        /// 画像描画
        /// </summary>
        public void Draw()
        {
            // 描画前処理
            if (!this.PreDraw())
            {
                return;
            }
            // アニメーション
            this.Animation();
            // 現在のアニメパターンから表示対象の分割画像を描画
            int graphIndex = (int)this._direction * this._maxPatterX + this._animationPattern;
            // カメラ位置を考慮
            // カメラが設定されていない場合は描画しない
            Process.Map.Camera camera = GameConfig.GameProcess.GetCamera();
            if (camera == null)
            {
                return;
            }
            DX.DrawRotaGraph2F(this.Center.X - this._size.Width / 2 - camera.topleft.X, this.Center.Y - this._size.Height / 2 - camera.topleft.Y, 0, 0, 1, 0, this._graphHandles[graphIndex], DX.TRUE);
            // 描画後処理
            this.AfterDraw();
        }

        /// <summary>
        /// 描画前処理
        /// </summary>
        /// <returns>描画実行可否</returns>
        public virtual bool PreDraw()
        {
            return true;
        }
        public virtual void AfterDraw() {}
        
        /// <summary>
        /// 画像パターンのアニメーション
        /// </summary>
        protected virtual void Animation()
        {
            // アニメーションOFFまたはアニメ速度が0の場合アニメなしとする
            if (!this._isAnimation || this._animationSpeed == 0)
            {
                return;
            }
            if (this._animationCounter == 0)
            {
                if (this._animationPattern > 0 && this._animationPattern < this._maxPatterX - 1)
                {
                    // パターンが範囲内の場合アニメ切り替え
                    this._animationPattern += this._animationDirection;
                }
                else
                {
                    // パターンを超過する場合アニメ方向切り替え
                    this._animationDirection *= -1;
                    this._animationPattern += this._animationDirection;
                }
            }
            this._animationCounter++;
            // 指定値までカウントアップしたら画像を切り替える
            // 横方向に切り替え、最大が3の場合は「1→2→3→2→1」、最大が4の場合は「1→2→3→4→3→2→1」となる
            if (this._animationCounter > this._animationSpeed)
            {
                this._animationCounter = 0;
            }
        }

        /// <summary>
        /// キャラクタを移動させる
        /// </summary>
        public void Move()
        {
            if (this.MoveSpeed == 0)
            {
                // 移動値が0の場合は何もしない
                return;
            }
            if (!this.IsEnabledMove)
            {
                // 移動不可
                return;
            }
            // 移動方向確認
            int diffX = this._willPosition.X - this.Position.X;
            int diffY = this._willPosition.Y - this.Position.Y;
            bool moveX = (diffX != 0);
            bool moveY = (diffY != 0);
            // X, Yともに移動が完了している
            if (!moveX && !moveY)
            {
                return;
            }
            int directionX = moveX ? (diffX / Math.Abs(diffX)) : 0;
            int directionY = moveY ? (diffY / Math.Abs(diffY)) : 0;
            // 移動実行
            int addX = moveX ? (int)this.MoveSpeed * directionX : 0;
            int addY = moveY ? (int)this.MoveSpeed * directionY : 0;
            // 構造体は値型のためプロパティ定義していたら直接書き換え不可
            Point movedPosition = this.Position;
            movedPosition.Offset(addX, addY);
            this.Position = movedPosition;
        }
    }
}
