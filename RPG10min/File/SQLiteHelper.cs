using System;
using System.Data.SQLite;

namespace RPG10min.File
{
    public class SQLiteHelper
    {
        private static SQLiteConnection _sqlite = null;
        private const String _file = "mayu.db"; 

        public SQLiteHelper()
        {
            this.Connection();
        }
        public void Connection()
        {
            // 既に読み込み済みの場合はそのまま
            if (_sqlite != null)
            {
                return;
            }
            using (_sqlite = new SQLiteConnection("Data Source=" + _file))
            {
                _sqlite.Open();
                // テーブルが存在しなければ生成
                using (SQLiteCommand command = _sqlite.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE IF NOT EXISTS players ("
                                + " id         INTEGER PRIMARY KEY AUTOINCREMENT,"
                                + " name       TEXT,"
                                + " lv         INTEGER,"
                                + " hp         INTEGER,"
                                + " mp         INTEGER,"
                                + " sp         INTEGER,"
                                + " image      TEXT,"
                                + " attack     INTEGER,"
                                + " diffence   INTEGER,"
                                + " m_attack   INTEGER,"
                                + " m_diffence INTEGER,"
                                + " speed      INTEGER,"
                                + " deft       INTEGER,"
                                + " org_param  INTEGER,"
                                + " state_good TEXT,"
                                + " state_bad  TEXT"
                                + ")";
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
