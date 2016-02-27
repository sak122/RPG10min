using RPG10min.File.FileException;
using System;
using System.Drawing;
using System.IO;

namespace RPG10min.Map
{
    public class MapReader
    {
        /// <summary>
        /// FMFデータのヘッダー情報
        /// </summary>
        public struct FMFHeader
        {
            /// <summary>
            /// ヘッダを除いたデータサイズ
            /// </summary>
            public UInt32 Size;
            /// <summary>
            /// マップの横幅
            /// </summary>
            public UInt32 MapWidth;
            /// <summary>
            /// マップの縦幅
            /// </summary>
            public UInt32 MapHeight;
            /// <summary>
            /// パーツの横幅
            /// </summary>
            public Byte ChipWidth;
            /// <summary>
            /// パーツの縦幅
            /// </summary>
            public Byte ChipHeight;
            /// <summary>
            /// レイヤー数
            /// </summary>
            public Byte LayerCount;
            /// <summary>
            /// レイヤーデータのビットカウント(8 or 16)
            /// </summary>
            public Byte BitCount;
            /// <summary>
            /// 文字列へ変換
            /// デバッグ用
            /// </summary>
            /// <returns>構造体文字列</returns>
            public override string ToString()
            {
                return "FMFHeader: size:" + Size
                        + ", mapWidth:" + MapWidth
                        + ", mapHeight:" + MapHeight
                        + ", chipWidth:" + ChipWidth
                        + ", chipHeight:" + ChipHeight
                        + ", layerCount:" + LayerCount
                        + ", bitCount:" + BitCount;
            }
        }

        public FMFHeader Header;
        private Byte[] _data8;
        private UInt16[] _data16;
        
        /// <summary>
        /// マップデータ読み込み
        /// </summary>
        /// <param name="path">FMFファイルパス</param>
        /// <returns>読み込み成否</returns>
        public bool Load(String path)
        {
            // FMFファイル読み込み
            using (FileStream fstream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader breader = new BinaryReader(fstream))
            {
                try
                {
                    // 先にヘッダー情報を読み込む
                    // 先頭4バイトでFMFファイルかどうかを検知
                    String id = new String(breader.ReadChars(4));
                    if (!id.Equals("FMF_"))
                    {
                        return false;
                    }
                    // ヘッダー情報を順に読み込んでいく
                    this.Header.Size = breader.ReadUInt32();
                    this.Header.MapWidth = breader.ReadUInt32();
                    this.Header.MapHeight = breader.ReadUInt32();
                    this.Header.ChipWidth = breader.ReadByte();
                    this.Header.ChipHeight = breader.ReadByte();
                    this.Header.LayerCount = breader.ReadByte();
                    this.Header.BitCount = breader.ReadByte();
                    // マップデータを読み込む
                    switch (this.Header.BitCount)
                    {
                        case 8:
                            this._data8 = breader.ReadBytes((int)this.Header.Size);
                            break;
                        case 16:
                            this._data16 = new UInt16[this.Header.Size / 2];
                            for (int i = 0; i < _data16.Length; i++)
                            {
                                this._data16[i] = breader.ReadUInt16();
                            }
                            break;
                        default:
                            return false;
                    }
                    Console.WriteLine(this.Header);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 指定レイヤーの指定位置のデータ番号を取得する
        /// </summary>
        /// <param name="layer">レイヤー番号(0～)</param>
        /// <param name="x">取得X座標(0～)</param>
        /// <param name="y">取得Y座標(0～)</param>
        /// <returns></returns>
        public UInt16 GetDataByLayerPosition(Byte layer, UInt32 x, UInt32 y)
        {
            UInt32 index = this.GetIndexByLayer(layer);
            index += (y * this.Header.MapWidth);
            index += x;
            // ビット数ごとに見る変数が異なる
            switch (this.Header.BitCount)
            {
                case 8:
                    if (this._data8 == null)
                    {
                        // データが読み込まれていない
                        throw new MapReaderException("8bitマップデータが読み込まれていません。");
                    }
                    if (this._data8.Length <= index)
                    {
                        // 指定位置のデータが存在しない
                        throw new MapReaderException("8bitマップデータの範囲外を読み込もうとしています。");
                    }
                    return (UInt16)this._data8[index];
                case 16:
                    if (this._data16 == null)
                    {
                        // データが読み込まれていない
                        throw new MapReaderException("16bitマップデータが読み込まれていません。");
                    }
                    if (this._data16.Length <= index)
                    {
                        // 指定位置のデータが存在しない
                        throw new MapReaderException("16bitマップデータの範囲外を読み込もうとしています。");
                    }
                    return this._data16[index];
                default:
                    throw new MapReaderException("マップデータが読み込まれていません。");
            }
        }

        /// <summary>
        /// 指定レイヤーのマップデータ開始インデックス
        /// 
        /// </summary>
        /// <param name="layer">開始位置取得対象レイヤー</param>
        /// <returns></returns>
        private UInt32 GetIndexByLayer(Byte layer)
        {
            if (layer < 0)
            {
                throw new MapReaderException("レイヤーの指定が正しくありません。");
            }
            if (layer > this.Header.LayerCount)
            {
                throw new MapReaderException("レイヤーの指定が実際のデータより大きいです。");
            }
            return (UInt32)(this.Header.MapWidth * this.Header.MapHeight * (layer));
        }
    }
}
