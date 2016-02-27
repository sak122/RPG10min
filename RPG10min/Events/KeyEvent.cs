using System;
using System.Reflection;
using DxLibDLL;
using System.Collections.Generic;

namespace RPG10min.Events
{
    /// <summary>
    /// キー入力イベント用のEventArgs
    /// </summary>
    public class KeyEventArgs : EventArgs
    {
        public List<KEY_MAPPING_TYPE> Keys;
    }

    public class KeyEvent
    {
        public event EventHandler<KeyEventArgs> OnInputKey;

        /// <summary>
        /// キー入力有効カウンター
        /// 連続入力でなく一定時間後の連打受付に対応させる
        /// </summary>
        private struct KeyCounter
        {
            /// <summary>
            /// 待機時間(およそ1/10ms)
            /// </summary>
            public UInt16 wait;
            /// <summary>
            /// 入力反映カウンター
            /// </summary>
            private UInt16 _counter;
            /// <summary>
            /// カウントアップ
            /// </summary>
            /// <returns>待機時間まで到達したか</returns>
            public Boolean CountUp()
            {
                if (this._counter++ == 0)
                {
                    return true;
                }
                if (this._counter >= this.wait)
                {
                    this.ResetCount();
                }
                return false;
            }
            /// <summary>
            /// カウンターを0にリセット
            /// </summary>
            public void ResetCount()
            {
                this._counter = 0;
            }

            public override String ToString()
            {
                return String.Format("Wait: {0}, Count: {1}", this.wait, this._counter);
            }
        }
        
        /// <summary>
        /// ウェイト付きボタンカウンター
        /// </summary>
        private KeyCounter[] _counter;
        
        public KeyEvent()
        {
            this.Init(250, 250);
        }
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="enter">決定ボタン待機時間(10ms)</param>
        /// <param name="cancel">キャンセルボタン待機時間(10ms)</param>
        public void Init(UInt16 enter, UInt16 cancel)
        {
            this._counter = new KeyCounter[Enum.GetValues(typeof(KEY_MAPPING_TYPE)).Length];
            this._counter[(UInt16)KEY_MAPPING_TYPE.KEY_ENTER].wait = (UInt16)Math.Ceiling(enter / 10.0);
            this._counter[(UInt16)KEY_MAPPING_TYPE.KEY_CANCEL].wait = (UInt16)Math.Ceiling(cancel / 10.0);
        }

        /// <summary>
        /// 指定機能に該当するキーが押されているか
        /// </summary>
        /// <returns></returns>
        public static Boolean IsInputKeyType(KEY_MAPPING_TYPE type)
        {
            int[] keys = GameConfig.KeyMapping[(int)type];
            for (int i = 0; i < keys.Length; i++)
            {
                if (DX.CheckHitKey(keys[i]) == 1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// キー入力を確認する
        /// </summary>
        public void CheckInputKey()
        {
            Array keyTypes = Enum.GetValues(typeof(KEY_MAPPING_TYPE));

            List<KEY_MAPPING_TYPE> inputKey = new List<KEY_MAPPING_TYPE>();
            foreach (KEY_MAPPING_TYPE type in keyTypes)
            {
                if (KeyEvent.IsInputKeyType(type))
                {
                    // ウェイトがあるタイプはカウンターを確認する
                    if (this._counter[(UInt16)type].wait != 0)
                    {
                        if (!this._counter[(UInt16)type].CountUp())
                        {
                            continue;
                        }
                    }
                    inputKey.Add(type);
                }
                else if (this._counter[(UInt16)type].wait != 0)
                {
                    // ウェイトがあるタイプは入力がされていなければカウンターをリセット
                    this._counter[(UInt16)type].ResetCount();
                }
            }
            // 何かしらのキーが押されていたらdelegateで画面ごとのキー入力処理を実行
            if (inputKey.Count != 0)
            {
                if (this.OnInputKey != null)
                {
                    KeyEventArgs e = new KeyEventArgs();
                    e.Keys = inputKey;
#if DEBUG
                    inputKey.ForEach(i => Console.WriteLine("{0}, ", i));
#endif
                    this.OnInputKey(this, e);
                }
            }
        }
    }
}
