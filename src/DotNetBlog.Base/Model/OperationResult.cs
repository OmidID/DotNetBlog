using System.Collections.Generic;

namespace DotNetBlog.Model
{
    public class OperationResult
    {
        public static readonly OperationResult SuccessResult = new OperationResult
        {
            Success = true
        };

        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public OperationResult()
        {
            this.Success = true;
        }

        public static OperationResult Failure(string errorMessage)
        {
            return new OperationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public OperationResult(T data)
        {
            this.Success = true;
            this.Data = data;
        }

        public static new OperationResult<T> Failure(string errorMessage)
        {
            return new OperationResult<T>(default(T))
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public class PagedResult<T> : OperationResult
    {
        public List<T> Data { get; set; }

        public int Total { get; set; }

        private PagedResult()
        {
        }

        public PagedResult(List<T> data)
            : this(data, data.Count)
        {

        }

        public PagedResult(List<T> data, int total)
        {
            Data = data;
            Total = total;
            Success = true;
        }

        public static new PagedResult<T> Failure(string errorMessage)
        {
            return new PagedResult<T>
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
