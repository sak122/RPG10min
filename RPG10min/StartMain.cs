using DxLibDLL;
using System;
using System.Threading.Tasks;

namespace RPG10min
{
    class StartMain
    {
        [STAThread]
        static void Main()
        {
            // ウィンドウモード
            DX.ChangeWindowMode(DX.TRUE);
            DX.SetGraphMode(GameConfig.WINDOW_SIZE.Width, GameConfig.WINDOW_SIZE.Height, 32);
            //// 低解像度モード
            //if (DX.SetEmulation320x240(1) == -1)
            //{
            //    return;
            //}
            // 初期化
            if (DX.DxLib_Init() == -1)
            {
                return;
            }
            
            // DB読み込み
            //GameConfig.sqlite = new File.SQLiteHelper();
            
            // ゲーム起動
            GameConfig.GameProcess = new Process.GameProcess();
            GameConfig.GameProcess.Init();

            // メインループ
            while (DX.ProcessMessage() == 0)
            {
                if (!GameConfig.GameProcess.MainLoop())
                {
                    break;
                }
                if (DX.WaitTimer(10) == -1)
                {
                    break;
                }
            }
            DX.DxLib_End();
        }
    }
}
