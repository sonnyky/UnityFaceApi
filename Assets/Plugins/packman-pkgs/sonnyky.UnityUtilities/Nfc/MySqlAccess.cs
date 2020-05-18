using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using System.Threading;

public class MySqlAccess : MonoBehaviour
{
    string SERVER    = "test";
    string DATABASE  = "test";
    string USERID    = "test";
    string PORT      = "3306";
    string PASSWORD  = "test";
    string TABLENAME = "test";


    public async Task<string> SelectData()
	{
        return await Task.Run<string>(() => {
            // 三秒間だけ待ってやる.
            Thread.Sleep(3000);
            const string result = "データー取得しました\n";

            MySqlConnection con = null;

            string conCmd =
                    "Server=" + SERVER + ";" +
                    "Database=" + DATABASE + ";" +
                    "Userid=" + USERID + ";" +
                    "Password=" + PASSWORD + ";" +
                    "Port=" + PORT + ";" +
                    "CharSet=utf8;";

            try
            {
                Debug.Log("cmd : " + conCmd);
                con = new MySqlConnection(conCmd);
                Debug.Log("connection : " + con.State);
                con.Open();

            }
            catch (MySqlException ex)
            {
                Debug.Log(ex.ToString());
            }

            string selCmd = "SELECT * FROM " + TABLENAME + " LIMIT 0, 1200;";

            MySqlCommand cmd = new MySqlCommand(selCmd, con);

            IAsyncResult iAsync = cmd.BeginExecuteReader();

            while (!iAsync.IsCompleted)
            {
               
            }

            MySqlDataReader rdr = cmd.EndExecuteReader(iAsync);

            while (rdr.Read())
            {
                if (!rdr.IsDBNull(rdr.GetOrdinal("card_number")))
                {
                    Debug.Log("card number : " + rdr.GetString("card_number"));
                }
            }

            rdr.Close();
            rdr.Dispose();
            con.Close();
            con.Dispose();

            return result;
        });
      
    }
}
