using DxLibDLL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG10min.System
{
    class Window
    {
        private int[] _windowGraphHandle;
        private Size _windowPieceSize;
        private int[] _cursorGraphHandle;
        private Point _position;
        private Size _size;

        public Window()
        {
            String imagePath = "Data/Images/System/";
            this.Init(imagePath + "pipo-WindowBase001.png", imagePath + "pipo-pipo-CursorBase001.png");
        }
        //public Window(UInt16 id)
        //{
        //    // IDごとにファイルパスを取得
        //    this.Init(winPath, curPath, pausePath);
        //}
        public Window(String winPath, String curPath)
        {
            this.Init(winPath, curPath);
        }
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="winPath">ウィンドウ画像パス</param>
        /// <param name="curPath">カーソル画像パス</param>
        public void Init(String winPath, String curPath)
        {
            int imgw, imgh;
            // ウィンドウ
            // 3x3に分割して読み込む
            DX.GetGraphSize(DX.LoadGraph(winPath), out imgw, out imgh);
            this._windowGraphHandle = new int[9];
            DX.LoadDivGraph(winPath, this._windowGraphHandle.Length, 3, 3, imgw / 3, imgh / 3, out this._windowGraphHandle[0]);
            this._windowPieceSize = new Size(imgw / 3, imgh / 3);
        }

        /// <summary>
        /// ウィンドウ情報を設定
        /// </summary>
        /// <param name="x">表示位置X座標</param>
        /// <param name="y">表示位置Y座標</param>
        /// <param name="w">表示サイズ横幅</param>
        /// <param name="h">表示サイズ縦幅</param>
        public void SetWindowInfo(Int32 x, Int32 y, Int32 w, Int32 h)
        {
            this.SetWindowInfo(new Point(x, y), new Size(w, h));
        }
        /// <summary>
        /// ウィンドウ情報を設定
        /// </summary>
        /// <param name="pos">表示位置</param>
        /// <param name="size">表示サイズ</param>
        public void SetWindowInfo(Point pos, Size size)
        {
            this._position = pos;
            this._size = size;
        }

        /// <summary>
        /// ウィンドウ描画
        /// </summary>
        public Boolean DrawWindow()
        {
            Boolean result = true;
            UInt16 powerX = 10;
            UInt16 powerY = 10;
            // 幅、高さが分割した画像の両端を合わせたサイズより小さい場合縮小
            if (this._size.Width < this._windowPieceSize.Width * 2)
            {
                powerX = (UInt16)Math.Ceiling((double)(this._size.Width * 10 / (this._windowPieceSize.Width * 2)));
            }
            if (this._size.Height < this._windowPieceSize.Height * 2)
            {
                powerY = (UInt16)Math.Ceiling((double)(this._size.Height * 10 / (this._windowPieceSize.Height * 2)));
            }
            // 左上
            Int32 width = this._windowPieceSize.Width * powerX;
            Int32 height = this._windowPieceSize.Height * powerY;
            Int32 x = this._position.X;
            Int32 y = this._position.Y;
            result = result && DX.DrawExtendGraph(x, y, x + width / 10, y + height / 10, this._windowGraphHandle[0], DX.TRUE) != -1;
            // 右上
            x = this._position.X + this._size.Width;
            y = this._position.Y;
            result = result && DX.DrawExtendGraph(x - width / 10, y, x, y + height / 10, this._windowGraphHandle[2], DX.TRUE) != -1;
            // 左下
            x = this._position.X;
            y = this._position.Y + this._size.Height;
            result = result && DX.DrawExtendGraph(x, y - height / 10, x + width / 10, y, this._windowGraphHandle[6], DX.TRUE) != -1;
            // 右下
            x = this._position.X + this._size.Width;
            y = this._position.Y + this._size.Height;
            result = result && DX.DrawExtendGraph(x - width / 10, y - height / 10, x, y, this._windowGraphHandle[8], DX.TRUE) != -1;
            // 真ん中の描画は必要な場合のみ
            // 指定サイズが分割サイズの両端より少なければ描画は不要
            if (powerX >= 10)
            {
                // 上真ん中
                x = this._position.X;
                y = this._position.Y;
                result = result && DX.DrawExtendGraph(x + width / 10, y, x + this._size.Width - width / 10, y + height / 10, this._windowGraphHandle[1], DX.TRUE) != -1;
                // 下真ん中
                x = this._position.X;
                y = this._position.Y + this._size.Height;
                result = result && DX.DrawExtendGraph(x + width / 10, y - height / 10, x + this._size.Width - width / 10, y, this._windowGraphHandle[7], DX.TRUE) != -1;
            }
            if (powerX >= 10 && powerY >= 10)
            {
                // 中心
                x = this._position.X;
                y = this._position.Y;
                result = result && DX.DrawExtendGraph(x + width / 10, y + height / 10, x + this._size.Width - width / 10, y + this._size.Height - height / 10, this._windowGraphHandle[4], DX.TRUE) != -1;
            }
            if (powerY >= 10)
            {
                // 左真ん中
                x = this._position.X;
                y = this._position.Y;
                result = result && DX.DrawExtendGraph(x, y + height / 10, x + width / 10, y + this._size.Height - height / 10, this._windowGraphHandle[3], DX.TRUE) != -1;
                // 右真ん中
                x = this._position.X + this._size.Width;
                y = this._position.Y;
                result = result && DX.DrawExtendGraph(x - width / 10, y + height / 10, x, y + this._size.Height - height / 10, this._windowGraphHandle[5], DX.TRUE) != -1;
            }

            return result;
        }
    }
}
