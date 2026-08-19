using System;

namespace BaseRepository.ViewModels
{
    public class OperationResult<T>
    {
        public string TableName { get; }
        public T? Model { get; set; }
        public long OperationDate { get; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string? Message { get; set; }
        public bool Success { get; set; }

        public OperationResult(string tableName)
        {
            TableName = tableName;
        }
    }
}
