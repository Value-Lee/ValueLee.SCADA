using System;
using System.Collections.Generic;

namespace SCADA.UART.Framework
{
    public interface IResponseInfo
    {
        string CommandName { get; set; }
    }

    public struct ResponseInfo<TCharOrByte> : IResponseInfo where TCharOrByte : struct, IConvertible, IComparable
    {
        public ResponseInfo(string commandName, IList<TCharOrByte> contents)
        {
            if (typeof(TCharOrByte) != typeof(byte) && typeof(TCharOrByte) != typeof(char))
            {
                throw new ArgumentException("TCharOrByte must be byte or char");
            }
            if (commandName == null) throw new ArgumentNullException(commandName);
            if (contents == null) throw new ArgumentNullException(nameof(contents));

            CommandName = commandName;
            Contents = contents;
        }

        public string CommandName { get; set; }
        public IList<TCharOrByte> Contents { get; set; }
    }
}