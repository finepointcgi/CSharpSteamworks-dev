using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSteamworks.Networking
{
    public class Packet : IDisposable
    {
        private List<byte> _writeBuffer;
        private byte[] _readBuffer;
        private int _position;

        private bool disposed = false;

        public int Length => _writeBuffer.Count;
        public int UnreadLength => Length - _position;

        public Packet()
        {
            _writeBuffer = new List<byte>();
            _position = 0;
        }

        public Packet(int id) : this()
        {
            WriteInt(id);
        }

        public Packet(byte[] data) : this()
        {
            SetBytes(data);
        }

        #region Read
        public byte ReadByte(bool moveIndex = true)
        {
            CheckPosition(typeof(byte));

            // If there are unread bytes
            byte value = _readBuffer[_position];
            MovePosition(moveIndex, sizeof(byte));
            return value;
        }

        public byte[] ReadBytes(int length, bool movePosition = true)
        {
            CheckPosition(typeof(byte[]));

            byte[] value = _writeBuffer.GetRange(_position, length).ToArray();
            MovePosition(movePosition, length);
            return value;
        }

        public int ReadInt(bool movePosition = true)
        {
            CheckPosition(typeof(int));

            int value = BitConverter.ToInt32(_readBuffer, _position);
            MovePosition(movePosition, sizeof(int));
            return value;
        }

        public string ReadString(bool movePosition = true)
        {
            try
            {
                int length = ReadInt();
                string value = Encoding.ASCII.GetString(_readBuffer, _position, length);

                if (movePosition && value.Length > 0)
                {
                    _position += length;
                }

                return value;
            }
            catch
            {
                throw new Exception($"Could not read value of type {typeof(string).Name}");
            }
        }
        #endregion

        #region Write
        public void WriteLength() => _writeBuffer.InsertRange(0, BitConverter.GetBytes(_writeBuffer.Count));
        public void WriteByte(byte value) => _writeBuffer.Add(value);
        public void WriteBytes(byte[] value) => _writeBuffer.AddRange(value);
        public void WriteInt(int value) => _writeBuffer.AddRange(BitConverter.GetBytes(value));

        public void WriteString(string value)
        {
            WriteInt(value.Length);
            _writeBuffer.AddRange(Encoding.ASCII.GetBytes(value));
        }
        #endregion

        #region Utils
        public void CheckPosition(Type type)
        {
            if (_position >= _writeBuffer.Count)
            {
                throw new Exception($"Could not read value of type '{type.Name}'");
            }
        }

        public void MovePosition(bool move, int length)
        {
            if (move)
            {
                _position += length;
            }
        }

        public void PrependInt(int value) => _writeBuffer.InsertRange(0, BitConverter.GetBytes(value));

        public byte[] ToArray()
        {
            _readBuffer = _writeBuffer.ToArray();
            return _readBuffer;
        }

        public void SetBytes(byte[] data)
        {
            // write bytes to the buffer here.
            WriteBytes(data);
            _readBuffer = _writeBuffer.ToArray();
        }
        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _writeBuffer = null;
                    _readBuffer = null;
                    _position = 0;
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
