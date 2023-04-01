using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseRepository.ViewModels
{
    public class OperationResult<T>
    {
        public int RecordId { get; set; } = 0;
        public string Opration { get; set; }
        public string TableName { get; set; }
        public T? Model { get; set; }
        public long OprationDate { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Message { get; set; }
        public bool Success { get; set; } = false;
        public OperationResult(string tablename)
        {
            TableName = tablename;
        }
    }
}
