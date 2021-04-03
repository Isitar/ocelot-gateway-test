namespace Auth
{
    using System.Collections.Generic;
    using System.Linq;

    public class Result
    {
        public bool Successful { get; set; }
        public string[] Errors { get; set; }

        public Result(bool successful, IEnumerable<string> errors)
        {
            Errors = errors.ToArray();
            Successful = successful;
        }


        public static Result Success()
        {
            return new Result(true, new string[0]);
        }

        public static Result Failure(IEnumerable<string> errors)
        {
            return new Result(false, errors);
        }

        public string ErrorsCompact()
        {
            return string.Join(", ", Errors);
        }
    }

    public class Result<T> : Result
    {
        public T Data { get; set; }

        public Result(bool successful, IEnumerable<string> errors) : base(successful, errors) { }

        public Result(bool successful, IEnumerable<string> errors, T data) : base(successful, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>(true, new string[0], data);
        }

        public new static Result<T> Failure(IEnumerable<string> errors)
        {
            return new Result<T>(false, errors);
        }
    }
}