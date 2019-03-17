﻿using SharpDX;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace TheDX11.Resources
{
    public enum BufferTypeEnum
    {
        Vertex,
        Index,
        ConstPixel,
        ConstVertex,
    }

    public class NativeBuffer<T> : IDisposable where T : struct
    {
        private readonly Device device;

        public BufferTypeEnum BufferType { get; }

        public int Length { get; private set; }

        internal Buffer Buffer { get; private set; }
        
        internal VertexBufferBinding VertexBufferBinding { get; private set; }

        internal NativeBuffer(Device device, BufferTypeEnum bufferType, int length)
        {
            this.device = device;
            this.BufferType = bufferType;
                        
            Length = length;
            CreateBuffer();
        }

        internal NativeBuffer(Device device, BufferTypeEnum bufferType, T[] data)
        {
            this.device = device;
            this.BufferType = bufferType;

            Length = data.Length;
            CreateBufferWithData(data);
        }

        private void CreateBuffer()
        {
            BufferDescription bufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Utilities.SizeOf<T>() * Length, // Must be divisable by 16 bytes, so this is equated to 32 (?)
                BindFlags = BufferTypeToBindFlags(BufferType),
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            Buffer = new Buffer(device, bufferDesc);
            VertexBufferBinding = new VertexBufferBinding(Buffer, Utilities.SizeOf<T>(), 0);
        }

        private void CreateBufferWithData(T[] data)
        {
            BufferDescription bufferDesc = new BufferDescription()
            {
                Usage = ResourceUsage.Default,
                SizeInBytes = Utilities.SizeOf<T>() * Length, // Must be divisable by 16 bytes, so this is equated to 32 (?)
                BindFlags = BufferTypeToBindFlags(BufferType),
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };
            Buffer = Buffer.Create(device, data, bufferDesc);
            VertexBufferBinding = new VertexBufferBinding(Buffer, Utilities.SizeOf<T>(), 0);
        }

        // this method can be called only from render thread
        public void UpdateBuffer(T[] newData)
        {
            if (Length != newData.Length)
            {
                Dispose();
                Length = newData.Length;
                CreateBuffer();
            }
            device.ImmediateContext.UpdateSubresource(newData, Buffer);
        }
        
        // this method can be called only from render thread
        public void UpdateBuffer(ref T newData)
        {
            if (Length != 1)
            {
                Dispose();
                Length = 1;
                CreateBuffer();
            }
            device.ImmediateContext.UpdateSubresource(ref newData, Buffer);
        }

        private static BindFlags BufferTypeToBindFlags(BufferTypeEnum bufferType)
        {
            switch (bufferType)
            {
                case BufferTypeEnum.Vertex:
                    return BindFlags.VertexBuffer;
                case BufferTypeEnum.Index:
                    return BindFlags.IndexBuffer;
                case BufferTypeEnum.ConstPixel:
                case BufferTypeEnum.ConstVertex:
                    return BindFlags.ConstantBuffer;
                default:
                    throw new Exception("Unsupported buffer type");
            }
        }

        public void Activate(int slot)
        {
            if (BufferType == BufferTypeEnum.Vertex)
            {
                device.ImmediateContext.InputAssembler.SetVertexBuffers(slot, VertexBufferBinding);
            }
            else if (BufferType == BufferTypeEnum.Index)
            {
                device.ImmediateContext.InputAssembler.SetIndexBuffer(Buffer, SharpDX.DXGI.Format.R32_UInt, slot);
            }
            else if (BufferType == BufferTypeEnum.ConstPixel)
            {
                device.ImmediateContext.PixelShader.SetConstantBuffer(slot, Buffer);
            }
            else if (BufferType == BufferTypeEnum.ConstVertex)
            {
                device.ImmediateContext.VertexShader.SetConstantBuffer(slot, Buffer);
            }
            else
            {
                throw new Exception("Unsupported buffer type");
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
            Buffer = null;
        }
    }
}
