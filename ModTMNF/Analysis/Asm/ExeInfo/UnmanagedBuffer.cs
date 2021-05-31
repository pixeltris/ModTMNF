using System;
using System.Runtime.InteropServices;
using Bea;

namespace ModTMNF.Analysis.Asm
{
    public class UnmanagedBuffer : IDisposable
    {
        private uint codeOffset;
        private uint codeSize;
        private uint imageBase;
        private uint bufferCodeSectionStart;
        private uint bufferCodeSectionEnd;
        private uint codeSectionStart;
        private uint codeSectionEnd;

        public IntPtr Ptr { get; private set; }
        public int Length { get; private set; }
        public uint CodeOffset
        {
            get { return codeOffset; }
            set
            {
                codeOffset = value;
                codeSectionStart = imageBase + codeOffset;
                codeSectionEnd = codeSectionStart + codeSize;
            }
        }
        public uint CodeSize
        {
            get { return codeSize; }
            set
            {
                codeSize = value;
                bufferCodeSectionEnd = (uint)(Ptr.ToInt32() + codeSize);
                codeSectionEnd = codeSectionStart + codeSize;
            }
        }
        public uint ImageBase
        {
            get { return imageBase; }
            set
            {
                imageBase = value;
                codeSectionStart = imageBase + codeOffset;
                codeSectionEnd = codeSectionStart + codeSize;
            }
        }
        public uint CodeSectionStart
        {
            get { return codeSectionStart; }
        }
        public uint CodeSectionEnd
        {
            get { return codeSectionEnd; }
        }
        public uint BufferCodeSectionStart
        {
            get { return bufferCodeSectionStart; }
        }
        public uint BufferCodeSectionEnd
        {
            get { return bufferCodeSectionEnd; }
        }

        public UnmanagedBuffer()
        {
            Ptr = IntPtr.Zero;
            Length = 0;
            ImageBase = 0x400000;
        }

        public UnmanagedBuffer(byte[] data)
        {
            ImageBase = 0x400000;
            SetData(data);
        }

        ~UnmanagedBuffer()
        {
            Close();
        }

        public void SetData(byte[] data)
        {
            if(Ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(Ptr);

            Ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, Ptr, data.Length);
            Length = data.Length;
            
            bufferCodeSectionStart = (uint)Ptr.ToInt32();
            bufferCodeSectionEnd = (uint)(Ptr.ToInt32() + codeSize);
        }

        public byte[] Read(uint address, uint length)
        {
            return Read((long)address, length);
        }

        public byte[] Read(long address, uint length)
        {
            byte[] buffer = new byte[length];

            // Validate that this address within the range
            long addr = AddressToEip(address);
            if (addr >= BufferCodeSectionStart && addr + length <= BufferCodeSectionEnd)
            {
                Marshal.Copy((IntPtr)addr, buffer, 0, (int)length);
            }

            return buffer;
        }

        /// <summary>
        /// Translates a runtime address to an eip relative to the buffer
        /// </summary>
        /// <param name="address">The runtime address</param>
        /// <returns>An eip value relative to the buffer</returns>
        public uint AddressToEip(uint address)
        {
            return (uint)(Ptr.ToInt32() + (address - codeSectionStart));
        }

        public long AddressToEip(long address)
        {
            return Ptr.ToInt32() + (address - codeSectionStart);
        }

        /// <summary>
        /// Translates a buffer eip to a runtime address
        /// </summary>
        /// <param name="bufferEip">The buffer eip</param>
        /// <returns>A runtime address equivalent of the eip</returns>
        public uint EipToAddress(uint bufferEip)
        {
            return bufferEip - bufferCodeSectionStart + codeSectionStart;
        }

        public long EipToAddress(long bufferEip)
        {
            return bufferEip - bufferCodeSectionStart + codeSectionStart;
        }

        public void Dispose()
        {
            Close();
        }

        private void Close()
        {
            if (Ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
        }
    }
}