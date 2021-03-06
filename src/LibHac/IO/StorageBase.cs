﻿using System;
using System.Collections.Generic;

namespace LibHac.IO
{
    public abstract class StorageBase : IStorage
    {
        private bool _isDisposed;
        protected internal List<IDisposable> ToDispose { get; } = new List<IDisposable>();
        protected bool CanAutoExpand { get; set; }

        protected abstract void ReadImpl(Span<byte> destination, long offset);
        protected abstract void WriteImpl(ReadOnlySpan<byte> source, long offset);
        public abstract void Flush();
        public abstract long Length { get; }

        public void Read(Span<byte> destination, long offset)
        {
            ValidateParameters(destination, offset);
            ReadImpl(destination, offset);
        }

        public void Write(ReadOnlySpan<byte> source, long offset)
        {
            ValidateParameters(source, offset);
            WriteImpl(source, offset);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                Flush();
                foreach (IDisposable item in ToDispose)
                {
                    item?.Dispose();
                }
            }

            _isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void ValidateParameters(ReadOnlySpan<byte> span, long offset)
        {
            if (_isDisposed) throw new ObjectDisposedException(null);
            if (span == null) throw new ArgumentNullException(nameof(span));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "Argument must be non-negative.");

            if (Length != -1 && !CanAutoExpand)
            {
                if (offset + span.Length > Length) throw new ArgumentException("The given offset and count exceed the length of the Storage");
            }
        }
    }
}
