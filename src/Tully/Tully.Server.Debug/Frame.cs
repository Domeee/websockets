using System;
using System.Text;

namespace Tully.Server.Debug
{
    /// <summary>
    /// WebSocket frame implementation according to http://tools.ietf.org/html/rfc6455#section-5.2
    /// </summary>
    /// <remarks>
    /// 0               1               2               3
    /// 8 7 6 5 4 3 2 1 8 7 6 5 4 3 2 1 8 7 6 5 4 3 2 1 8 7 6 5 4 3 2 1
    /// +-+-+-+-+-------+-+-------------+-------------------------------+
    /// |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
    /// |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
    /// |N|V|V|V|       |S|             |   (if payload len==126/127)   |
    /// | |1|2|3|       |K|             |                               |
    /// +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
    /// |     Extended payload length continued, if payload len == 127  |
    /// + - - - - - - - - - - - - - - - +-------------------------------+
    /// |                               |Masking-key, if MASK set to 1  |
    /// +-------------------------------+-------------------------------+
    /// | Masking-key (continued)       |          Payload Data         |
    /// +-------------------------------- - - - - - - - - - - - - - - - +
    /// :                     Payload Data continued ...                :
    /// + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
    /// |                     Payload Data continued ...                |
    /// +---------------------------------------------------------------+
    /// </remarks>
    public class Frame
    {
        // FIN bit at position 8 in the byte
        private const byte FinBit = 8;

        // MASK bit at position 8 in the byte
        private const byte MaskBit = 8;

        // OpCode uses the first 4 bit of the byte (00001111)
        private const byte OpCodeBytes = 15;

        // The WebSocket frame as raw byte array
        private readonly byte[] _frameBytes;

        public Frame(byte[] frameBytes)
        {
            _frameBytes = frameBytes;
            IsFIN = GetBit(FinBit, _frameBytes[0]);
            IsMasked = GetBit(MaskBit, frameBytes[1]);
            OpCode = (uint)GetInt(OpCodeBytes, frameBytes[0]);
            PayloadLength = GetPayloadLength();
            MaskingKey = GetMaskingKey();
            ApplicationData = GetApplicationData();
        }

        public byte[] ApplicationData { get; private set; }

        public bool IsFIN { get; private set; }

        public bool IsMasked { get; private set; }

        public byte[] MaskingKey { get; private set; }

        public uint OpCode { get; private set; }

        public uint PayloadLength { get; private set; }

        private bool GetBit(byte bitNumber, byte data)
        {
            return (data & (1 << bitNumber - 1)) != 0;
        }

        private int GetInt(byte bytes, byte data)
        {
            return bytes & data;
        }

        /// <summary>
        /// Payload length: 7 bits, 7+16 bits, or 7+64 bits.
        /// </summary>
        private uint GetPayloadLength()
        {
            if (!IsMasked)
            {
                // TODO: Throw protocol error (http://tools.ietf.org/html/rfc6455#section-7.4.1)
            }

            // Payload length (7bit) minus Mask bit (MSB -> 2^7)
            uint length = (uint)_frameBytes[1] - 128;

            if (length > -1)
            {
                if (length == 126)
                {
                    throw new NotImplementedException();
                }
                if (length == 127)
                {
                    throw new NotImplementedException();
                }
            }

            return length;
        }

        private byte[] GetMaskingKey()
        {
            byte[] copy = new byte[4];
            Buffer.BlockCopy(_frameBytes, 2, copy, 0, 4);
            return copy;
        }

        private byte[] GetApplicationData()
        {
            var decoded = new byte[PayloadLength];
            var encoded = new ArraySegment<byte>(_frameBytes, 6, 3);

            int encodedPointer = encoded.Offset;

            for (int i = 0; i < encoded.Count; i++)
            {
                decoded[i] = (byte)(encoded.Array[encodedPointer++] ^ MaskingKey[i % 4]);
            }

            return decoded;
        }
    }
}