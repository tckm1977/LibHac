﻿using System;

namespace LibHac.IO.Save
{
    public class HierarchicalDuplexStorage : StorageBase
    {
        private DuplexStorage[] Layers { get; }
        private DuplexStorage DataLayer { get; }

        public HierarchicalDuplexStorage(DuplexFsLayerInfo[] layers, bool masterBit)
        {
            Layers = new DuplexStorage[layers.Length - 1];

            for (int i = 0; i < Layers.Length; i++)
            {
                IStorage bitmap;

                if (i == 0)
                {
                    bitmap = masterBit ? layers[0].DataB : layers[0].DataA;
                }
                else
                {
                    bitmap = Layers[i - 1];
                }

                Layers[i] = new DuplexStorage(layers[i + 1].DataA, layers[i + 1].DataB, bitmap, layers[i + 1].Info.BlockSize);
            }

            DataLayer = Layers[Layers.Length - 1];
            Length = DataLayer.Length;
        }

        protected override void ReadImpl(Span<byte> destination, long offset)
        {
            DataLayer.Read(destination, offset);
        }

        protected override void WriteImpl(ReadOnlySpan<byte> source, long offset)
        {
            DataLayer.Write(source, offset);
        }

        public override void Flush()
        {
            DataLayer.Flush();
        }

        public override long Length { get; }
    }

    public class DuplexFsLayerInfo
    {
        public IStorage DataA { get; set; }
        public IStorage DataB { get; set; }
        public DuplexInfo Info { get; set; }
    }
}
