using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace SQLHelper
{
   public  class AOI_BLL
    {
        public static DataSet GetSQLByBarcode(string barcode)
        {
            string rowNames = "Barcode,SaveTime,CellIndex,FAI1_1,FAI1_2,FAI1_3,FAI1_4,FAI2_1,FAI2_2,FAI2_3,FAI3,FAI4,FAI5,FAI6_1,FAI6_2,FAI7,FAI10_1,FAI10_2,FAI10_3,"
                + "FAI12,FAI13,FAI14_1,FAI14_2,FAI15,FAI16,FAI17,FAI18,FAI19,FAI20,FAI24,FAI25";
            string tableName = "TestResult";
            string sqlStr = string.Format("select {0} from {1} where Barcode=\'{2}\'", rowNames, tableName, barcode);
            return SQL_DAL.GetDataSet(sqlStr, false, tableName);
        }
     public static DataSet GetSQLByTime(DateTime startTime,DateTime endTime)
        {
           
            string rowNames = "Barcode,SaveTime,CellIndex,FAI1_1,FAI1_2,FAI1_3,FAI1_4,FAI2_1,FAI2_2,FAI2_3,FAI3,FAI4,FAI5,FAI6_1,FAI6_2,FAI7,FAI10_1,FAI10_2,FAI10_3,"
                + "FAI12,FAI13,FAI14_1,FAI14_2,FAI15,FAI16,FAI17,FAI18,FAI19,FAI20,FAI24,FAI25";
            string tableName = "TestResult";
            string sqlStr = string.Format("select {0} from {1} where SaveTime between \'{2}\' and \'{3}\'", rowNames, tableName, startTime.ToString(), endTime.ToString());
            return SQL_DAL.GetDataSet(sqlStr, false, tableName);
        }
       public static int InsertSQL(string[] values)
        {
            if (values == null) { return 0; }
            int count = values.Length;
            if (count == 0) { return 0; }
            string rowNames = "DataIndex,Barcode,SaveTime,CellIndex,FAI1_1,FAI1_2,FAI1_3,FAI1_4,FAI2_1,FAI2_2,FAI2_3,FAI3,FAI4,FAI5,FAI6_1,FAI6_2,FAI7,FAI10_1,FAI10_2,FAI10_3,"
             + "FAI12,FAI13,FAI14_1,FAI14_2,FAI15,FAI16,FAI17,FAI18,FAI19,FAI20,FAI24,FAI25";
            string tableName = "TestResult";
            string valueStr = "";
            for (int i = 0; i < count; i++)
            {
                if (i == count - 1)
                {
                    valueStr = valueStr + values[i] + ",";
                }
                else
                {
                    valueStr += values[i];
                }

            }
            string sqlStr = string.Format("insert into {0} ({1}) values ({2}) ", tableName, rowNames , valueStr );
            return SQL_DAL.ExecuteNonQuery(sqlStr, false);
        }

    }
}
