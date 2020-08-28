using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LumaprintsTask.Helper
{
    public class CSVHelper
    {
        public static void CreateCSVFile(DataTable dt, string strFilePath)
        {
            // #Region "Export Grid to CSV"
            // Create the CSV file to which grid data will be exported.
            StreamWriter sw = new StreamWriter(strFilePath, false);
            // First we will write the headers.
            // DataTable dt = m_dsProducts.Tables[0];

            int iColCount = dt.Columns.Count;
            for (int i = 0; i <= iColCount - 1; i++)
            {
                sw.Write(dt.Columns[i]);

                if (i < iColCount - 1)
                    sw.Write(",");
            }

            sw.Write(sw.NewLine);

            // Now write all the rows.

            foreach (DataRow dr in dt.Rows)
            {
                for (int i = 0; i <= iColCount - 1; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dt.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                    //if (!Convert.IsDBNull(dr[i]))
                    //    sw.Write(dr[i].ToString());
                    //if (i < iColCount - 1)
                    //    sw.Write(",");
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }
        public static DataTable Read_CSV(string FileName, string Cursor)
        {
            // ----------------------------------------------------
            // Reads a csv file into a datatable, with the first row as column headers

            string myFile = FileName;
            DataTable myTable = new DataTable(); // (Cursor)
            int i;
            DataRow myRow;
            DataColumn myColumn;
            string[] fieldValues;

            string[] ColumnNames;
            StreamReader myReader = new StreamReader(myFile);
            try
            {

                // Open file and read first two lines
                ColumnNames = myReader.ReadLine().Split(Convert.ToChar(","));

                // Create data columns named according to first line of data, with type according to second line
                for (i = 0; i <= ColumnNames.Length - 1; i++)
                {
                    myColumn = new DataColumn();

                    myColumn.ColumnName = ColumnNames[i];
                    myTable.Columns.Add(myColumn);
                }

                // Read the body of the data to data table
                while (myReader.Peek() != -1)
                {
                    fieldValues = myReader.ReadLine().Split(Convert.ToChar(","));
                    myRow = myTable.NewRow();
                    for (i = 0; i <= fieldValues.Length - 1; i++)
                        myRow[i] = fieldValues[i].ToString();
                    myTable.Rows.Add(myRow);
                }
            }
            catch (Exception ex)
            {
                //Interaction.MsgBox("Error Loading...: " + ex.Message);
                return new DataTable();
            } // (Cursor)
            finally
            {
                myReader.Close();
            }

            return myTable;
        }
        public static DataTable DT_CSV_DATA(string filename, bool header)
        {
            string CSVFilePathName = filename;
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            Fields = Lines[0].Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();
            //1st row must be column names; force lower case to ensure matching later on.
            for (int i = 0; i < Cols; i++)
            {
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            }

            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }
            return dt;
        }
    }
}
