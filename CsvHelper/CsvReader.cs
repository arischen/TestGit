using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CsvHelper
{
    public sealed class CsvReader : IDisposable
    {
        private readonly StringBuilder _columnBuilder;
        private Encoding _encoding;
        private FileStream _fileStream;
        private Stream _memoryStream;
        private Stream _stream;
        private StreamReader _streamReader;
        private StreamWriter _streamWriter;
        private readonly Type _type;

        public CsvReader(Stream stream)
        {
            this._columnBuilder = new StringBuilder(100);
            this._type = Type.File;
            this._type = Type.Stream;
            this.Initialise(stream, Encoding.Default);
        }

        public CsvReader(string filePath)
        {
            this._columnBuilder = new StringBuilder(100);
            this._type = Type.File;
            this._type = Type.File;
            this.Initialise(filePath, Encoding.Default);
        }

        public CsvReader(Stream stream, Encoding encoding)
        {
            this._columnBuilder = new StringBuilder(100);
            this._type = Type.File;
            this._type = Type.Stream;
            this.Initialise(stream, encoding);
        }

        public CsvReader(string filePath, Encoding encoding)
        {
            this._columnBuilder = new StringBuilder(100);
            this._type = Type.File;
            this._type = Type.File;
            this.Initialise(filePath, encoding);
        }

        public CsvReader(Encoding encoding, string csvContent)
        {
            this._columnBuilder = new StringBuilder(100);
            this._type = Type.File;
            this._type = Type.String;
            this.Initialise(encoding, csvContent);
        }

        public void Dispose()
        {
            if (this._streamReader != null)
            {
                this._streamReader.Close();
                this._streamReader.Dispose();
            }
            if (this._streamWriter != null)
            {
                this._streamWriter.Close();
                this._streamWriter.Dispose();
            }
            if (this._memoryStream != null)
            {
                this._memoryStream.Close();
                this._memoryStream.Dispose();
            }
            if (this._fileStream != null)
            {
                this._fileStream.Close();
                this._fileStream.Dispose();
            }
            if (((this._type == Type.String) || (this._type == Type.File)) && (this._stream != null))
            {
                this._stream.Close();
                this._stream.Dispose();
            }
        }

        private void Initialise(Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("The supplied stream is null.");
            }
            this._stream = stream;
            this._stream.Position = 0L;
            this._encoding = encoding ?? Encoding.Default;
            this._streamReader = new StreamReader(this._stream, this._encoding);
        }

        private void Initialise(string filePath, Encoding encoding)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("The file '{0}' does not exist.", filePath));
            }
            this._fileStream = File.OpenRead(filePath);
            this.Initialise(this._fileStream, encoding);
        }

        private void Initialise(Encoding encoding, string csvContent)
        {
            if (csvContent == null)
            {
                throw new ArgumentNullException("The supplied csvContent is null.");
            }
            this._encoding = encoding ?? Encoding.Default;
            this._memoryStream = new MemoryStream(csvContent.Length);
            this._streamWriter = new StreamWriter(this._memoryStream);
            this._streamWriter.Write(csvContent);
            this._streamWriter.Flush();
            this.Initialise(this._memoryStream, encoding);
        }

        private void ParseLine(string line)
        {
            this.Fields = new List<string>();
            bool flag = false;
            bool flag2 = false;
            this._columnBuilder.Remove(0, this._columnBuilder.Length);
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (!flag)
                {
                    if (ch == '"')
                    {
                        flag2 = true;
                    }
                    else
                    {
                        this._columnBuilder.Append(ch);
                    }
                    flag = true;
                }
                else
                {
                    if (flag2)
                    {
                        if ((ch == '"') && (((line.Length > (i + 1)) && (line[i + 1] == ',')) || ((i + 1) == line.Length)))
                        {
                            flag2 = false;
                            flag = false;
                            i++;
                        }
                        else if (((ch == '"') && (line.Length > (i + 1))) && (line[i + 1] == '"'))
                        {
                            i++;
                        }
                    }
                    else if (ch == ',')
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        this.Fields.Add(this.TrimColumns ? this._columnBuilder.ToString().Trim() : this._columnBuilder.ToString());
                        this._columnBuilder.Remove(0, this._columnBuilder.Length);
                    }
                    else
                    {
                        this._columnBuilder.Append(ch);
                    }
                }
            }
            if (flag)
            {
                this.Fields.Add(this.TrimColumns ? this._columnBuilder.ToString().Trim() : this._columnBuilder.ToString());
            }
        }

        public DataTable ReadIntoDataTable()
        {
            return this.ReadIntoDataTable(new System.Type[0]);
        }

        public DataTable ReadIntoDataTable(System.Type[] columnTypes)
        {
            DataTable table = new DataTable();
            bool flag = false;
            this._stream.Position = 0L;
            while (this.ReadNextRecord())
            {
                int num;
                if (!flag)
                {
                    num = 0;
                    while (num < this.Fields.Count)
                    {
                        table.Columns.Add(this.Fields[num], (columnTypes.Length > 0) ? columnTypes[num] : typeof(string));
                        num++;
                    }
                    flag = true;
                }
                else
                {
                    DataRow row = table.NewRow();
                    for (num = 0; num < this.Fields.Count; num++)
                    {
                        row[num] = this.Fields[num];
                    }
                    table.Rows.Add(row);
                }
            }
            this._fileStream.Close();
            return table;
        }

        public bool ReadNextRecord()
        {
            this.Fields = null;
            string line = this._streamReader.ReadLine();
            if (line == null)
            {
                return false;
            }
            this.ParseLine(line);
            return true;
        }

        public int? FieldCount
        {
            get
            {
                return ((this.Fields != null) ? new int?(this.Fields.Count) : null);
            }
        }

        public List<string> Fields { get; private set; }

        public bool HasHeaderRow { get; set; }

        public bool TrimColumns { get; set; }

        private enum Type
        {
            File,
            Stream,
            String
        }
    }
}
