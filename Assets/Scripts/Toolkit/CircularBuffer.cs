/**
 * ==========================================
 * Author：xuzq9
 * CreatTime：2025.4.28
 * Description：循环buffer
 * ==========================================
 */

using System;
using Stark.Core.Logs;

namespace StarWorld.Common.Utility
{
    public class CircularBuffer
    {
        private readonly byte[] buffer;
        private readonly int bufferSize;
        private int readCursor;  // 已读取位置
        private int writeCursor; // 写入位置
        private readonly object bufferLock = new object();
        private int dataCount;   // 当前缓冲区中的数据量

        public CircularBuffer(int size)
        {
            bufferSize = size;
            buffer = new byte[size];
            readCursor = 0;
            writeCursor = 0;
            dataCount = 0;
        }

        public int AvailableSpace
        {
            get
            {
                lock (bufferLock)
                {
                    return bufferSize - dataCount;
                }
            }
        }

        public int AvailableData
        {
            get
            {
                lock (bufferLock)
                {
                    return dataCount;
                }
            }
        }

        public bool Write(byte[] data)
        {
            if (data == null || data.Length == 0) return false;

            lock (bufferLock)
            {
                // 如果数据长度超过缓冲区大小，只写入最后一个缓冲区大小的数据
                if (data.Length > bufferSize)
                {
                    Log.Warn($"[CircularBuffer] 数据长度({data.Length})超过缓冲区大小({bufferSize})，将只写入最后{bufferSize}字节");
                    Array.Copy(data, data.Length - bufferSize, buffer, 0, bufferSize);
                    writeCursor = 0;
                    readCursor = 0;
                    dataCount = bufferSize;
                    return true;
                }

                // 如果写入的数据会导致覆盖未读数据，调整读指针
                if (data.Length > AvailableSpace)
                {
                    // 计算需要丢弃的数据量
                    int overflow = data.Length - AvailableSpace;
                    readCursor = (readCursor + overflow) % bufferSize;
                    dataCount = Math.Max(0, dataCount - overflow);
                }

                // 分两次写入数据（如果需要环绕）
                int firstWrite = Math.Min(data.Length, bufferSize - writeCursor);
                Array.Copy(data, 0, buffer, writeCursor, firstWrite);

                if (firstWrite < data.Length)
                {
                    // 环绕写入剩余数据
                    int secondWrite = data.Length - firstWrite;
                    Array.Copy(data, firstWrite, buffer, 0, secondWrite);
                    writeCursor = secondWrite;
                }
                else
                {
                    writeCursor = (writeCursor + firstWrite) % bufferSize;
                }

                dataCount = Math.Min(bufferSize, dataCount + data.Length);
                return true;
            }
        }

        public int Read(byte[] destination, int offset, int count)
        {
            if (destination == null || count <= 0) return 0;

            lock (bufferLock)
            {
                if (dataCount == 0) return 0;

                int toRead = Math.Min(count, dataCount);
                
                // 分两次读取数据（如果需要环绕）
                int firstRead = Math.Min(toRead, bufferSize - readCursor);
                Array.Copy(buffer, readCursor, destination, offset, firstRead);

                if (firstRead < toRead)
                {
                    // 环绕读取剩余数据
                    int secondRead = toRead - firstRead;
                    Array.Copy(buffer, 0, destination, offset + firstRead, secondRead);
                    readCursor = secondRead;
                }
                else
                {
                    readCursor = (readCursor + firstRead) % bufferSize;
                }

                dataCount -= toRead;
                return toRead;
            }
        }

        public void Clear()
        {
            lock (bufferLock)
            {
                readCursor = 0;
                writeCursor = 0;
                dataCount = 0;
            }
        }
    }
} 