using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPG10min.Map
{
    /// <summary>
    /// マップ辺り判定フラグ
    /// </summary>
    public enum MapPassableFlag
    {
        AllPass   = 0,  // 全方向通行可能
        UpNone    = 1,  // 上通行不可
        RigheNone = 2,  // 右通行不可
        DownNone  = 4,  // 下通行不可
        LeftNone  = 8,  // 左通行不可
        AllNone   = 15, // 全方向通行不可
        ShipOnly  = 16, // 船のみ通行可（海とか）
    }
    public class ChipData
    {
        private MapPassableFlag[] _hitList;
        private Boolean[] _frontList;

        public ChipData(UInt32 mapId)
        {
            // マップデータごとの設定
            switch (mapId)
            {
                case 1:
                    this._hitList = new MapPassableFlag[]
                    {
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllPass,
                        MapPassableFlag.UpNone|MapPassableFlag.DownNone, MapPassableFlag.LeftNone|MapPassableFlag.RigheNone, MapPassableFlag.UpNone|MapPassableFlag.DownNone, MapPassableFlag.LeftNone|MapPassableFlag.RigheNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.UpNone|MapPassableFlag.LeftNone|MapPassableFlag.RigheNone, MapPassableFlag.UpNone|MapPassableFlag.LeftNone|MapPassableFlag.RigheNone,
                        MapPassableFlag.UpNone, MapPassableFlag.AllPass, MapPassableFlag.UpNone, MapPassableFlag.AllPass, MapPassableFlag.UpNone, MapPassableFlag.UpNone, MapPassableFlag.AllPass, MapPassableFlag.AllPass,
                        MapPassableFlag.DownNone, MapPassableFlag.DownNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone,
                        MapPassableFlag.UpNone, MapPassableFlag.UpNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.UpNone, MapPassableFlag.UpNone, MapPassableFlag.UpNone, MapPassableFlag.UpNone,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllNone,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.DownNone, MapPassableFlag.DownNone, MapPassableFlag.AllNone,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.UpNone, MapPassableFlag.UpNone, MapPassableFlag.AllNone,
                        MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllPass, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone, MapPassableFlag.AllNone
                    };
                    this._frontList = new Boolean[this._hitList.Length];
                    this._frontList[32] = true;
                    this._frontList[33]= true;
                    this._frontList[69] = true;
                    this._frontList[70] = true;
                    break;
            }
        }

        /// <summary>
        /// マップチップが通行可能か確認
        /// </summary>
        /// <param name="chipId">マップチップID</param>
        /// <param name="flag">通行不可フラグ</param>
        /// <returns>通行可能か</returns>
        public Boolean CanPassable(UInt16 chipId, MapPassableFlag flag)
        {
            if (chipId >= this._hitList.Length)
            {
                return false;
            }
            MapPassableFlag chipFlag = this._hitList[chipId];
            // 通行可能か？
            if (Utils.IsOnFlag((Int32)chipFlag, (Int32)flag))
            {
                // 通行不可のフラグであるため、フラグが合致すれば不可
                return false;
            }
            return true;
        }

        public Boolean IsFront(UInt16 chipId)
        {
            if (chipId >= this._frontList.Length)
            {
                return false;
            }
            return this._frontList[chipId];
        }
    }
}
