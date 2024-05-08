using OPSProServer.Contracts.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class Result
    {
        public bool Success { get; }
        public List<OPSException> Errors { get; }

        public Result(bool success)
        {
            Success = success;
            Errors = new List<OPSException>();
        }

        public Result(List<OPSException> errors)
        {
            Success = errors.Count == 0;
            Errors = errors;
        }

        public void ThrowIfError()
        {
            if (Errors.Count > 0)
            {
                throw Errors.First();
            }
        }
    }

    public class Result<T> : Result
    {
        public T Data { get; }

        public Result(T data) : base(true)
        {
            Data = data;
        }
    }
}
