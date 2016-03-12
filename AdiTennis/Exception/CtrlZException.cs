using System.IO;
using System.Runtime.Serialization;

namespace AdiTennis.Exception
{
    /// <summary>
    ///     Class for handle end of standard input.
    /// </summary>
    internal class CtrlZException : IOException
    {
        public CtrlZException()
        {
        }

        public CtrlZException(string message) : base(message)
        {
        }

        public CtrlZException(string message, int hresult) : base(message, hresult)
        {
        }

        protected CtrlZException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CtrlZException(string message, System.Exception innerException) : base(message, innerException)
        {
        }
    }
}