using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CsvHelper
{
    public sealed class CsvWriter : IDisposable
    {
        private string _carriageReturnAndLineFeedReplacement = ",";
        private bool _replaceCarriageReturnsAndLineFeedsFromFieldValues = true;
        private StreamWriter _streamWriter;

        public void Dispose()
        {
            if (this._streamWriter != null)
            {
                this._streamWriter.Close();
                this._streamWriter.Dispose();
            }
        }

        public void WriteCsv(CsvFile csvFile, Stream stream)
        {
            this.WriteCsv(csvFile, stream, null);
        }

        public void WriteCsv(CsvFile csvFile, string filePath)
        {
            this.WriteCsv(csvFile, filePath, null);
        }

        public string WriteCsv(CsvFile csvFile, Encoding encoding)
        {
            string str = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding ?? Encoding.Default))
                {
                    this.WriteToStream(csvFile, writer);
                    writer.Flush();
                    stream.Position = 0L;
                    using (StreamReader reader = new StreamReader(stream, encoding ?? Encoding.Default))
                    {
                        str = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        stream.Close();
                    }
                    return str;
                }
            }
        }

        public void WriteCsv(DataTable dataTable, Stream stream)
        {
            this.WriteCsv(dataTable, stream, null);
        }

        public void WriteCsv(DataTable dataTable, string filePath)
        {
            this.WriteCsv(dataTable, filePath, null);
        }

        public string WriteCsv(DataTable dataTable, Encoding encoding)
        {
            string str = string.Empty;
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream, encoding ?? Encoding.Default))
                {
                    this.WriteToStream(dataTable, writer);
                    writer.Flush();
                    stream.Position = 0L;
                    using (StreamReader reader = new StreamReader(stream, encoding ?? Encoding.Default))
                    {
                        str = reader.ReadToEnd();
                        writer.Close();
                        reader.Close();
                        stream.Close();
                    }
                    return str;
                }
            }
        }

        public void WriteCsv(CsvFile csvFile, Stream stream, Encoding encoding)
        {
            stream.Position = 0L;
            this._streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
            this.WriteToStream(csvFile, this._streamWriter);
            this._streamWriter.Flush();
            stream.Position = 0L;
        }

        public void WriteCsv(CsvFile csvFile, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (StreamWriter writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
            {
                this.WriteToStream(csvFile, writer);
                writer.Flush();
                writer.Close();
            }
        }

        public void WriteCsv(DataTable dataTable, Stream stream, Encoding encoding)
        {
            stream.Position = 0L;
            this._streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
            this.WriteToStream(dataTable, this._streamWriter);
            this._streamWriter.Flush();
            stream.Position = 0L;
        }

        public void WriteCsv(DataTable dataTable, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            FileStream fs = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            string data = "";
            //写出列名称
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                data += dataTable.Columns[i].ColumnName.ToString();
                if (i < dataTable.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw.WriteLine(data);

            //写出各行数据
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    data += dataTable.Rows[i][j].ToString();
                    if (j < dataTable.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw.WriteLine(data);
            }
            sw.Close();
            fs.Close();
        }

        private void WriteRecord(IList<string> fields, TextWriter writer)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                bool flag = fields[i].Contains(",");
                bool flag2 = fields[i].Contains("\"");
                string str = flag2 ? fields[i].Replace("\"", "\"\"") : fields[i];
                if (this.ReplaceCarriageReturnsAndLineFeedsFromFieldValues && (str.Contains("\r") || str.Contains("\n")))
                {
                    flag = true;
                    str = str.Replace("\r\n", this.CarriageReturnAndLineFeedReplacement).Replace("\r", this.CarriageReturnAndLineFeedReplacement).Replace("\n", this.CarriageReturnAndLineFeedReplacement);
                }
                writer.Write(string.Format("{0}{1}{0}{2}", (flag || flag2) ? "\"" : string.Empty, str, (i < (fields.Count - 1)) ? "," : string.Empty));
            }
            writer.WriteLine();
        }

        public void WriteRecordToFile(CsvRecord csvRecord, string filePath)
        {
            StreamWriter writer = new StreamWriter(filePath, true, Encoding.Default);
            this.WriteRecord(csvRecord.Fields, writer);
            writer.Close();
        }

        private void WriteToStream(CsvFile csvFile, TextWriter writer)
        {
            if (csvFile.Headers.Count > 0)
            {
                this.WriteRecord(csvFile.Headers, writer);
            }
            csvFile.Records.ForEach(record => this.WriteRecord(record.Fields, writer));
        }

        private void WriteToStream(DataTable dataTable, TextWriter writer)
        {
        }

        public string CarriageReturnAndLineFeedReplacement
        {
            get
            {
                return this._carriageReturnAndLineFeedReplacement;
            }
            set
            {
                this._carriageReturnAndLineFeedReplacement = value;
            }
        }

        public bool ReplaceCarriageReturnsAndLineFeedsFromFieldValues
        {
            get
            {
                return this._replaceCarriageReturnsAndLineFeedsFromFieldValues;
            }
            set
            {
                this._replaceCarriageReturnsAndLineFeedsFromFieldValues = value;
            }
        }
    }
}
