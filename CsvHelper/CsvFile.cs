using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace CsvHelper
{
    [Serializable]
    public sealed class CsvFile
    {
        public readonly List<string> Headers = new List<string>();
        public readonly CsvRecords Records = new CsvRecords();

        public void Populate(bool hasHeaderRow, string csvContent)
        {
            this.Populate(hasHeaderRow, csvContent, null, false);
        }

        public void Populate(Stream stream, bool hasHeaderRow)
        {
            this.Populate(stream, null, hasHeaderRow, false);
        }

        public void Populate(string filePath, bool hasHeaderRow)
        {
            this.Populate(filePath, null, hasHeaderRow, false);
        }

        public void Populate(bool hasHeaderRow, string csvContent, bool trimColumns)
        {
            this.Populate(hasHeaderRow, csvContent, null, trimColumns);
        }

        public void Populate(Stream stream, bool hasHeaderRow, bool trimColumns)
        {
            this.Populate(stream, null, hasHeaderRow, trimColumns);
        }

        public void Populate(string filePath, bool hasHeaderRow, bool trimColumns)
        {
            this.Populate(filePath, null, hasHeaderRow, trimColumns);
        }

        public void Populate(bool hasHeaderRow, string csvContent, Encoding encoding, bool trimColumns)
        {
            CsvReader reader2 = new CsvReader(encoding, csvContent)
            {
                HasHeaderRow = hasHeaderRow,
                TrimColumns = trimColumns
            };
            using (CsvReader reader = reader2)
            {
                this.PopulateCsvFile(reader);
            }
        }

        public void Populate(Stream stream, Encoding encoding, bool hasHeaderRow, bool trimColumns)
        {
            CsvReader reader2 = new CsvReader(stream, encoding)
            {
                HasHeaderRow = hasHeaderRow,
                TrimColumns = trimColumns
            };
            using (CsvReader reader = reader2)
            {
                this.PopulateCsvFile(reader);
            }
        }

        public void Populate(string filePath, Encoding encoding, bool hasHeaderRow, bool trimColumns)
        {
            CsvReader reader2 = new CsvReader(filePath, encoding)
            {
                HasHeaderRow = hasHeaderRow,
                TrimColumns = trimColumns
            };
            using (CsvReader reader = reader2)
            {
                this.PopulateCsvFile(reader);
            }
        }

        private void PopulateCsvFile(CsvReader reader)
        {
            Action<string> action = null;
            this.Headers.Clear();
            this.Records.Clear();
            bool flag = false;
            while (reader.ReadNextRecord())
            {
                CsvRecord record;
                if (reader.HasHeaderRow && !flag)
                {
                    if (action == null)
                    {
                        action = field => this.Headers.Add(field);
                    }
                    reader.Fields.ForEach(action);
                    flag = true;
                }
                else
                {
                    record = new CsvRecord();
                    reader.Fields.ForEach(field => record.Fields.Add(field));
                    this.Records.Add(record);
                }
            }
        }

        public int HeaderCount
        {
            get
            {
                return this.Headers.Count;
            }
        }

        public CsvRecord this[int recordIndex]
        {
            get
            {
                if (recordIndex > (this.Records.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));
                }
                return this.Records[recordIndex];
            }
        }

        public string this[int recordIndex, int fieldIndex]
        {
            get
            {
                if (recordIndex > (this.Records.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));
                }
                CsvRecord record = this.Records[recordIndex];
                if (fieldIndex > (record.Fields.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0} in record {1}.", fieldIndex, recordIndex));
                }
                return record.Fields[fieldIndex];
            }
            set
            {
                if (recordIndex > (this.Records.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));
                }
                CsvRecord record = this.Records[recordIndex];
                if (fieldIndex > (record.Fields.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0}.", fieldIndex));
                }
                record.Fields[fieldIndex] = value;
            }
        }

        public string this[int recordIndex, string fieldName]
        {
            get
            {
                if (recordIndex > (this.Records.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));
                }
                CsvRecord record = this.Records[recordIndex];
                int num = -1;
                for (int i = 0; i < this.Headers.Count; i++)
                {
                    if (string.Compare(this.Headers[i], fieldName) == 0)
                    {
                        num = i;
                        break;
                    }
                }
                if (num == -1)
                {
                    throw new ArgumentException(string.Format("There is no field header with the name '{0}'", fieldName));
                }
                if (num > (record.Fields.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0} in record {1}.", num, recordIndex));
                }
                return record.Fields[num];
            }
            set
            {
                if (recordIndex > (this.Records.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no record at index {0}.", recordIndex));
                }
                CsvRecord record = this.Records[recordIndex];
                int num = -1;
                for (int i = 0; i < this.Headers.Count; i++)
                {
                    if (string.Compare(this.Headers[i], fieldName) == 0)
                    {
                        num = i;
                        break;
                    }
                }
                if (num == -1)
                {
                    throw new ArgumentException(string.Format("There is no field header with the name '{0}'", fieldName));
                }
                if (num > (record.Fields.Count - 1))
                {
                    throw new IndexOutOfRangeException(string.Format("There is no field at index {0} in record {1}.", num, recordIndex));
                }
                record.Fields[num] = value;
            }
        }

        public int RecordCount
        {
            get
            {
                return this.Records.Count;
            }
        }
    }

    [Serializable]
    public sealed class CsvRecord
    {
        public readonly List<string> Fields = new List<string>();

        public int FieldCount
        {
            get
            {
                return this.Fields.Count;
            }
        }
    }

    [Serializable]
    public sealed class CsvRecords : List<CsvRecord>
    {
    }

}
