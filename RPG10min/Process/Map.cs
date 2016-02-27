using DxLibDLL;
using RPG10min.Map;
using RPG10min.ItemObject;
using RPG10min.System;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RPG10min.Process
{
    public class Map: ProcessAbstract
    {
        /// <summary>
        /// マップチップの透過ID
        /// </summary>
        protected const UInt16 CHIP_HIDDEN_ID = 0;

        /// <summary>
        /// 描画用マップ情報
        /// 前面描画にて使用
        /// </summary>
        protected struct DrawMapInfo
        {
            /// <summary>
            /// マップ座標
            /// </summary>
            public Point position;
            /// <summary>
            /// チップ番号
            /// </summary>
            public UInt16 chipId;
        }

        /// <summary>
        /// カメラ
        /// </summary>
        public Camera camera
        {
            private set;
            get;
        }
        /// <summary>
        /// 画像ハンドラ
        /// </summary>
        private int[] _graphHandles;
        /// <summary>
        /// マップデータ
        /// </summary>
        private MapReader _mapdata;
        /// <summary>
        /// マップのX方向ループが可能か
        /// </summary>
        public Boolean isLoopX;
        /// <summary>
        /// マップのY方向ループが可能か
        /// </summary>
        public Boolean isLoopY;
        /// <summary>
        /// マップチップ情報
        /// </summary>
        protected ChipData _chipData;
        /// <summary>
        /// 前面描画用マップ情報一覧
        /// </summary>
        private List<DrawMapInfo>[] _frontMapInfos;

        public Map()
        {
            this.camera = new Camera();
        }

        /// <summary>
        /// 情報読み込み、初期化
        /// </summary>
        public override void Load()
        {
            string path = "Data/Images/MapChip/pipo-map001.png";
            int imgw, imgh;
            int graphAll = DX.LoadGraph(path);
            DX.GetGraphSize(graphAll, out imgw, out imgh);
            this._graphHandles = new int[imgw / GameConfig.CHIP_SIZE * imgh / GameConfig.CHIP_SIZE];
            DX.LoadDivGraph(path, this._graphHandles.Length, imgw / GameConfig.CHIP_SIZE, imgh / GameConfig.CHIP_SIZE, GameConfig.CHIP_SIZE, GameConfig.CHIP_SIZE, out this._graphHandles[0]);

            // マップ読み込み
            this._mapdata = new MapReader();
            this._mapdata.Load("Data/Map/test.fmf");
            // チップ情報ロード
            this._chipData = new ChipData(1);
            // 一応レイヤー数分確保しておく
            this._frontMapInfos = new List<DrawMapInfo>[this._mapdata.Header.LayerCount];

            // カメラ初期設定
            this.camera.MoveSpeed = GameConfig.GameProcess.Player.MoveSpeed;
            this.camera.IsEnabledMove = GameConfig.GameProcess.Player.IsEnabledMove;
            this.camera.isConformPlayer = true;

            // ループ設定
            this.isLoopX = true;
            this.isLoopY = true;
            this.camera.isLoopX = this.isLoopX;
            this.camera.isLoopY = this.isLoopY;

            this.camera.Init(_mapdata.Header);
        }

        /// <summary>
        /// メイン処理
        /// </summary>
        public override void Main()
        {
            // カメラ移動
            camera.Move();
        }
        
        /// <summary>
        /// 描画
        /// </summary>
        public override void Draw()
        {
            // 描画前にカメラを設定
            this.camera.PreDraw();

            // レイヤーごとに描画
            Byte count = _mapdata.Header.LayerCount;
            for (Byte i = 0; i < count; i++)
            {
                this.DrawLayer(i);
            }
#if DEBUG
            DX.DrawString(0, 0, String.Format("Camera ({0}, {1})", this.camera.Center.X, this.camera.Center.Y/*, this.camera.willPosition.X/GameConfig.MOVE_MAGNIFICATION, this.camera.willPosition.Y/GameConfig.MOVE_MAGNIFICATION*/), DX.GetColor(255, 255, 255));
#endif
        }

        /// <summary>
        /// 指定レイヤーのマップを描画
        /// </summary>
        /// <param name="layer">描画対象レイヤー番号(0～)</param>
        private void DrawLayer(Byte layer)
        {
            this._frontMapInfos[layer] = new List<DrawMapInfo>();

            // +-1表示
            // ((カメラ中心座標 - 画面サイズ半分) / チップサイズ - 1(スクロール用に画面外1マス分))の切り上げ
            Int32 startX = (Int32)Math.Floor((camera.Center.X - GameConfig.WINDOW_SIZE.Width / 2) / GameConfig.CHIP_SIZE - 1);
            //startX = this.isLoopX ? (startX) : Math.Max(0, startX);
            Int32 startY = (Int32)Math.Floor((camera.Center.Y - GameConfig.WINDOW_SIZE.Height / 2) / GameConfig.CHIP_SIZE - 1);
            //startY = this.isLoopY ? (startY) : Math.Max(0, startY);

            Int32 endX = (Int32)(startX + GameConfig.WINDOW_SIZE.Width / GameConfig.CHIP_SIZE + 1);
            Int32 endY = (Int32)(startY + GameConfig.WINDOW_SIZE.Height / GameConfig.CHIP_SIZE + 1);
            for (Int32 y = startY; y <= endY; y++)
            {
                for (Int32 x = startX; x <= endX; x++)
                {
                    // ループに対応する場合、はみ出た場合に範囲内に収まるようセットする
                    Int32 idxX = x;
                    Int32 idxY = y;
                    idxX = GetDrawMapRoundRange((Int32)idxX, this._mapdata.Header.MapWidth, this.isLoopX);
                    idxY = GetDrawMapRoundRange((Int32)idxY, this._mapdata.Header.MapHeight, this.isLoopY);
                    UInt16 graphIndex = this._mapdata.GetDataByLayerPosition(layer, (UInt32)idxX, (UInt32)idxY);
                    // 透明は何もしない
                    if (graphIndex == CHIP_HIDDEN_ID)
                    {
                        continue;
                    }
                    // 前面表示の場合は退避しておく
                    // キャラが後ろに回っているときのみ
                    if (this._chipData.IsFront(graphIndex)
                        && (y + 1) * GameConfig.CHIP_SIZE * GameConfig.MOVE_MAGNIFICATION >= GameConfig.GameProcess.Player.Position.Y
                    )
                    {
                        DrawMapInfo frontInfo;
                        frontInfo.position = new Point(x, y);
                        frontInfo.chipId = graphIndex;
                        this._frontMapInfos[layer].Add(frontInfo);
                        continue;
                    }
                    // 表示座標はカメラ位置分ずらす
                    if (DX.DrawRotaGraph2F(x * GameConfig.CHIP_SIZE - this.camera.topleft.X, y * GameConfig.CHIP_SIZE - this.camera.topleft.Y, 0, 0, 1, 0, this._graphHandles[graphIndex], DX.TRUE) == -1)
                    {
                        Console.WriteLine("マップ描画失敗");
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 前面チップを描画
        /// スプライト描画後に呼び出される
        /// </summary>
        public void DrawFront()
        {
            // 下層レイヤーから重ねていく
            for (Byte layer = 0; layer < this._mapdata.Header.LayerCount; layer++)
            {
                if (this._frontMapInfos[layer] == null)
                {
                    continue;
                }
                foreach (DrawMapInfo frontInfo in this._frontMapInfos[layer])
                {
                    Int32 x = frontInfo.position.X;
                    Int32 y = frontInfo.position.Y;
                    // 表示座標はカメラ位置分ずらす
                    if (DX.DrawRotaGraph2F(x * GameConfig.CHIP_SIZE - this.camera.topleft.X, y * GameConfig.CHIP_SIZE - this.camera.topleft.Y, 0, 0, 1, 0, this._graphHandles[frontInfo.chipId], DX.TRUE) == -1)
                    {
                        Console.WriteLine("フロントマップ描画失敗");
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// マップのインデックス値をマップデータ範囲内に収まるよう取得
        /// </summary>
        /// <param name="center">カメラ中央座標</param>
        /// <param name="windowSize">ウィンドウサイズ</param>
        /// <param name="isLoop">ループ有無</param>
        /// <returns></returns>
        //private Int32 GetDrawMapRoundRange(Single center, Int32 windowSize, Boolean isLoop)
        private Int32 GetDrawMapRoundRange(Int32 idx, UInt32 mapsize, Boolean isLoop)
        {
            // 有効範囲は0～マップサイズ
            if (idx < 0)
            {
                if (isLoop)
                {
                    idx += (Int32)mapsize;
                }
                else
                {
                    idx = 0;
                }
            }
            else if (idx >= mapsize)
            {
                if (isLoop)
                {
                    idx -= (Int32)mapsize;
                }
                else
                {
                    idx = (Int32)(mapsize - 1);
                }
            }
            return idx;
        }

        /// <summary>
        /// キー入力を確認する
        /// </summary>
        public void InputKey(List<KEY_MAPPING_TYPE> keys)
        {
            // メニュー開閉可否確認
            if (true)
            {
                if (keys.Contains(KEY_MAPPING_TYPE.KEY_CANCEL))
                {
                    // メニューを開く
                    GameConfig.IsOpenMenu = true;
                }
            }
        }

        /// <summary>
        /// 指定座標に位置するマップ座標を取得
        /// </summary>
        /// <param name="position">確認座標（(X, Y) * GameConfig.MOVE_MAGNIFICATIONの値）</param>
        /// <returns></returns>
        public Point GetMapPosition(Point position)
        {
            // Yは足元ぴったりで渡されるため少し上にあげて判定する
            Int32 mapX = (Int32)Math.Ceiling((double)(position.X / GameConfig.MOVE_MAGNIFICATION / GameConfig.CHIP_SIZE));
            Int32 mapY = (Int32)Math.Ceiling((double)((position.Y / GameConfig.MOVE_MAGNIFICATION - 1) / GameConfig.CHIP_SIZE));
            return new Point(mapX, mapY);
        }

        // TODO: 当たり判定を1マスごとにレイヤー判定するのでなく、画像のチップ単位で設定する(TWのコタエ君のリプ参照）
        /// <summary>
        /// ある座標からある座標へ通行可能かどうか
        /// 当たり判定レイヤーを確認する
        /// </summary>
        /// <param name="now">基準座標</param>
        /// <param name="moveX">X軸移動（1以上の数値が指定されていても1マス分しか見ない）</param>
        /// <param name="moveY">Y軸移動（1以上の数値が指定されていても1マス分しか見ない）</param>
        /// <returns></returns>
        public Boolean CanPassable(Point now, Int32 moveX, Int32 moveY)
        {
            // 基準座標に位置するマップ座標を取得
            Point mapPosition = this.GetMapPosition(now);
            Int32 mapX = mapPosition.X;
            Int32 mapY = mapPosition.Y;
            // 移動方向へ移動
            if (moveX != 0)
            {
                mapX += (moveX / Math.Abs(moveX));
            }
            if (moveY != 0)
            {
                mapY += (moveY / Math.Abs(moveY));
            }
            // マップ番号を範囲内に収める
            mapX = this.GetDrawMapRoundRange(mapX, this._mapdata.Header.MapWidth, this.isLoopX);
            mapY = this.GetDrawMapRoundRange(mapY, this._mapdata.Header.MapHeight, this.isLoopY);
            // 対象座標のレイヤーを取得
            // 上のレイヤーから順に確認
            Byte lastLayerIdx = (Byte)(_mapdata.Header.LayerCount - 1);
            for (Int16 layer = lastLayerIdx; layer >= 0; layer--)
            {
                UInt16 nowGraphIndex = this._mapdata.GetDataByLayerPosition((Byte)layer, (UInt32)mapPosition.X, (UInt32)mapPosition.Y);
                UInt16 nextGraphIndex = this._mapdata.GetDataByLayerPosition((Byte)layer, (UInt32)mapX, (UInt32)mapY);
                MapPassableFlag flag = MapPassableFlag.AllPass;
                // 現在の位置から移動方向へ通行可能か
                if (moveX < 0)
                {
                    // 左へ行きたいけど現在のチップは左への通り抜け不可？
                    flag |= MapPassableFlag.LeftNone;
                }
                else if (moveX > 0)
                {
                    flag |= MapPassableFlag.RigheNone;
                }
                if (moveY < 0)
                {
                    flag |= MapPassableFlag.UpNone;
                }
                else if (moveY > 0)
                {
                    flag |= MapPassableFlag.DownNone;
                }
                if (!this._chipData.CanPassable(nowGraphIndex, flag))
                {
                    // 一つでも通行不可の情報があれば通れない
                    return false;
                }
                // 進行方向のマップへ通行可能か
                flag = MapPassableFlag.AllPass;
                if (moveX < 0)
                {
                    // 左へ行きたいけど次のチップは左からの通り抜け不可？
                    flag |= MapPassableFlag.RigheNone;
                }
                else if (moveX > 0)
                {
                    flag |= MapPassableFlag.LeftNone;
                }
                if (moveY < 0)
                {
                    flag |= MapPassableFlag.DownNone;
                }
                else if (moveY > 0)
                {
                    flag |= MapPassableFlag.UpNone;
                }
                if (!this._chipData.CanPassable(nextGraphIndex, flag))
                {
                    // 一つでも通行不可の情報があれば通れない
                    return false;
                }
            }
            // どこにも引っかからなかった場合は通り抜け可
            return true;
        }

        /// <summary>
        /// マップ上のカメラクラス
        /// マップ全体のうち、ウィンドウに収めるカメラ座標を管理
        /// </summary>
        public class Camera: AbstractObject
        {
            private const int MOVE_SPEED_DEFAULT = 1;

            public Point cameraMin;
            public Point cameraMax;
            public Boolean isLoopX;
            public Boolean isLoopY;
            /// <summary>
            /// キャラクターに追従するか
            /// </summary>
            public Boolean isConformPlayer;

            /// <summary>
            /// 中心座標
            /// 実際の座標値の GameConfig.MOVE_MAGNIFICATION 倍の座標
            /// </summary>
            protected Point __position;
            public override Point Position
            {
                set
                {
                    if (value.X < this.cameraMin.X)
                    {
                        if (this.isLoopX)
                        {
                            value.X += cameraMax.X;
                        }
                        else
                        {
                            value.X = cameraMin.X;
                        }
                    }
                    if (value.X > this.cameraMax.X)
                    {
                        if (this.isLoopX)
                        {
                            value.X += cameraMin.X;
                        }
                        else
                        {
                            value.X = cameraMax.X;
                        }
                    }
                    if (value.Y < cameraMin.Y)
                    {
                        if (this.isLoopY)
                        {
                            value.Y += cameraMax.Y;
                        }
                        else
                        {
                            value.Y = cameraMin.Y;
                        }
                    }
                    if (value.Y > cameraMax.Y)
                    {
                        if (this.isLoopY)
                        {
                            value.Y += cameraMin.Y;
                        }
                        else
                        {
                            value.Y = cameraMax.Y;
                        }
                    }
                    this.__position = value;
                }
                get { return this.__position; }
            }
            public PointF topleft
            {
                get { return new PointF(this.Center.X - GameConfig.WINDOW_SIZE.Width / 2, this.Center.Y - GameConfig.WINDOW_SIZE.Height / 2); }
            }
            /// <summary>
            /// これから移動する先の座標
            /// </summary>
            protected Point __willPosition;
            protected override Point _willPosition
            {
                set
                {
                    if (value.X < this.cameraMin.X)
                    {
                        value.X = this.cameraMin.X;
                    }
                    if (value.X > this.cameraMax.X)
                    {
                        value.X = this.cameraMax.X;
                    }
                    if (value.Y < this.cameraMin.Y)
                    {
                        value.Y = this.cameraMin.Y;
                    }
                    if (value.Y > this.cameraMax.Y)
                    {
                        value.Y = this.cameraMax.Y;
                    }
                    this.__willPosition = value;
                }
                get { return this.__willPosition; }
            }

            public Camera()
            {
                this._isAnimation = false;
            }

            public void Init(MapReader.FMFHeader mapdata)
            {
                Int32 minX, minY, maxX = 0, maxY;
                if (this.isLoopX)
                {
                    minX = 0;
                    maxX = (Int32)(mapdata.MapWidth * GameConfig.CHIP_SIZE - minX);
                }
                else
                {
                    minX = GameConfig.WINDOW_SIZE.Width / 2;
                    maxX = (Int32)(mapdata.MapWidth * GameConfig.CHIP_SIZE - minX);
                }
                if (this.isLoopY)
                {
                    minY = 0;
                    maxY = (Int32)(mapdata.MapHeight * GameConfig.CHIP_SIZE - minY);
                }
                else
                {
                    minY = GameConfig.WINDOW_SIZE.Height / 2;
                    maxY = (Int32)(mapdata.MapHeight * GameConfig.CHIP_SIZE - minY);
                }
                this.cameraMin = new Point(minX * GameConfig.MOVE_MAGNIFICATION, minY * GameConfig.MOVE_MAGNIFICATION);
                this.cameraMax = new Point(maxX * GameConfig.MOVE_MAGNIFICATION, maxY * GameConfig.MOVE_MAGNIFICATION);
                // 座標再設定
                this.Position = new Point(this.cameraMin.X, this.cameraMin.Y);
                this._willPosition = this.Position;
            }

            /// <summary>
            /// 移動情報を設定
            /// </summary>
            public void SetMove(int moveX, int moveY)
            {
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
                    this._willPosition = Point.Add(this.Position, new Size(moveX * GameConfig.CHIP_SIZE * GameConfig.MOVE_MAGNIFICATION, moveY * GameConfig.CHIP_SIZE * GameConfig.MOVE_MAGNIFICATION));
                }
            }

            public override bool PreDraw()
            {
                bool result = base.PreDraw();
                // キャラ追従するか？
                if (this.isConformPlayer)
                {
                    this.Position = GameConfig.GameProcess.Player.Position;
                    this._willPosition = this.Position;
                }
                return result;
            }
        }
    }
}
