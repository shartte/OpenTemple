using System;
using System.Collections.Generic;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using Vortice.D3DCompiler;
using Vortice.Direct3D11;
using Vortice.Direct3D11.Shader;
using ShaderDefines = System.Collections.Generic.Dictionary<string, string>;

namespace OpenTemple.Core.GFX
{
    public abstract class Shader<TSelf, T> : GpuResource<TSelf> where TSelf : GpuResource<TSelf> where T : ID3D11DeviceChild
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public string Name { get; }

        public Shader(string name, byte[] compiledShader)
        {
            Name = name;
            mCompiledShader = compiledShader;
        }

        private ID3D11ShaderReflection Reflect()
        {
            var err = Compiler.Reflect(mCompiledShader, out ID3D11ShaderReflection reflector);
            if (!err.Success)
            {
                throw new Exception("Failed to reflect shader: " + err);
            }

            return reflector;
        }

        private void PrintConstantBuffers()
        {
            using var reflector = Reflect();

            var shaderDesc = reflector.Description;

            Logger.Info("Vertex Shader '{0}' has {1} constant buffers:", Name, shaderDesc.ConstantBuffers);

            for (var i = 0; i < shaderDesc.ConstantBuffers; i++)
            {
                var cbufferDesc = reflector.ConstantBuffers[i];
                var bufferDesc = cbufferDesc.Description;

                Logger.Info("  Constant Buffer #{0} '{1}'", i, bufferDesc.Name);

                for (var j = 0; j < bufferDesc.VariableCount; j++)
                {
                    var variable = cbufferDesc.GetVariableByIndex(j);
                    var variableDesc = variable.Description;

                    Logger.Info("    {0} @ {1}", variableDesc.Name, variableDesc.StartOffset);
                }
            }
        }

        public abstract void CreateShader();

        public void FreeShader()
        {
            FreeResource();
        }

        protected override void FreeResource()
        {
            mDeviceShader?.Dispose();
            mDeviceShader = null;
        }

        public abstract void Bind();
        public abstract void Unbind();

        public byte[] CompiledCode => mCompiledShader;

        protected T mDeviceShader;
        protected byte[] mCompiledShader;
    }

    public class VertexShader : Shader<VertexShader, ID3D11VertexShader>
    {
        private readonly RenderingDevice _device;

        public VertexShader(RenderingDevice device, string name, byte[] compiledShader) : base(name, compiledShader)
        {
            _device = device;
        }

        public override void CreateShader()
        {
            FreeResource();

            mDeviceShader = _device.mD3d11Device.CreateVertexShader(mCompiledShader);
            if (_device.IsDebugDevice())
            {
                mDeviceShader.DebugName = Name;
            }
        }

        public override void Bind()
        {
            _device.mContext.VSSetShader(mDeviceShader, 0, null);
        }

        public override void Unbind()
        {
            _device.mContext.VSSetShader(null, 0, null);
        }
    }

    public class PixelShader : Shader<PixelShader, ID3D11PixelShader>
    {
        private readonly RenderingDevice _device;

        public PixelShader(RenderingDevice device, string name, byte[] compiledShader) : base(name, compiledShader)
        {
            _device = device;
        }

        public override void CreateShader()
        {
            FreeResource();

            mDeviceShader = _device.mD3d11Device.CreatePixelShader(mCompiledShader);
            if (_device.IsDebugDevice())
            {
                mDeviceShader.DebugName = Name;
            }
        }

        public override void Bind()
        {
            _device.mContext.PSSetShader(mDeviceShader, 0, null);
        }

        public override void Unbind()
        {
            _device.mContext.PSSetShader(null, 0, null);
        }
    }

    public class Shaders : IDisposable
    {
        private static readonly ShaderDefines EmptyDefines = new ShaderDefines();

        private readonly RenderingDevice _device;

        private readonly IFileSystem _fs;

        public Shaders(IFileSystem fs, RenderingDevice device)
        {
            _fs = fs;
            _device = device;
            mRegistration = new ResourceLifecycleCallbacks(device, CreateResources, FreeResources);
        }

        /// <summary>
        /// Compares two dictionaries for equality (ignoring ordering).
        /// </summary>
        private static bool AreDefinesEqual(ShaderDefines a, ShaderDefines b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            foreach (var pair in a)
            {
                if (!b.TryGetValue(pair.Key, out var valueB))
                {
                    return false;
                }

                if (!pair.Value.Equals(valueB))
                {
                    return false;
                }
            }

            return true;
        }

        public ResourceRef<VertexShader> LoadVertexShader(string name, ShaderDefines defines)
        {
            if (!mVertexShaders.TryGetValue(name, out var shaderCode))
            {
                var content = _fs.ReadTextFile($"shaders/{name}.hlsl");
                shaderCode = new ShaderCode<VertexShader>(content);
                mVertexShaders[name] = shaderCode;
            }
            else
            {
                // Search for a variant that matches the defines that are required
                foreach (var variant in shaderCode.compiledVariants)
                {
                    if (AreDefinesEqual(variant.Item1, defines))
                    {
                        return variant.Item2.CloneRef();
                    }
                }
            }

            // No variant was available for the requested defines, so
            // we compile it now
            var compiler = new ShaderCompiler(_fs);
            compiler.Defines = defines;
            compiler.Name = name;
            compiler.SourceCode = shaderCode.source;
            compiler.DebugMode = _device.IsDebugDevice();
            var shader = compiler.CompileVertexShader(_device);
            shader.Resource.CreateShader();

            // Insert the newly created shader into the cache
            shaderCode.compiledVariants.Add(Tuple.Create(defines, shader));

            return shader.CloneRef();
        }

        public ResourceRef<VertexShader> LoadVertexShader(string name)
        {
            return LoadVertexShader(name, EmptyDefines);
        }

        public ResourceRef<PixelShader> LoadPixelShader(string name, ShaderDefines defines)
        {
            if (!mPixelShaders.TryGetValue(name, out var shaderCode))
            {
                var content = _fs.ReadTextFile($"shaders/{name}.hlsl");
                shaderCode = new ShaderCode<PixelShader>(content);
                mPixelShaders[name] = shaderCode;
            }
            else
            {
                // Search for a variant that matches the defines that are required
                foreach (var variant in shaderCode.compiledVariants)
                {
                    if (AreDefinesEqual(variant.Item1, defines))
                    {
                        return new ResourceRef<PixelShader>(variant.Item2.Resource);
                    }
                }
            }

            // No variant was available for the requested defines, so
            // we compile it now
            var compiler = new ShaderCompiler(_fs);
            compiler.Defines = defines;
            compiler.Name = name;
            compiler.SourceCode = shaderCode.source;
            compiler.DebugMode = _device.IsDebugDevice();
            var shader = compiler.CompilePixelShader(_device);
            shader.Resource.CreateShader();

            // Insert the newly created shader into the cache
            shaderCode.compiledVariants.Add(Tuple.Create(defines, shader));

            return shader;
        }

        public ResourceRef<PixelShader> LoadPixelShader(string name)
        {
            return LoadPixelShader(name, EmptyDefines);
        }

        public void Dispose()
        {
            mRegistration.Dispose();
        }

        private class ShaderCode<T> where T : GpuResource<T>
        {
            // The HLSL shader source code
            public readonly string source;

            // The compiled variants based on the defines used to compile them
            public readonly List<Tuple<ShaderDefines, ResourceRef<T>>> compiledVariants;

            public ShaderCode(string source)
            {
                this.source = source;
                this.compiledVariants = new List<Tuple<ShaderDefines, ResourceRef<T>>>();
            }
        }

        private void CreateResources(RenderingDevice device)
        {
            foreach (var pair in mVertexShaders.Values)
            {
                foreach (var variant in pair.compiledVariants)
                {
                    variant.Item2.Resource.CreateShader();
                }
            }

            foreach (var pair in mPixelShaders.Values)
            {
                foreach (var variant in pair.compiledVariants)
                {
                    variant.Item2.Resource.CreateShader();
                }
            }
        }

        private void FreeResources(RenderingDevice device)
        {
            foreach (var pair in mVertexShaders.Values)
            {
                foreach (var variant in pair.compiledVariants)
                {
                    variant.Item2.Resource.FreeShader();
                }
            }

            foreach (var pair in mPixelShaders.Values)
            {
                foreach (var variant in pair.compiledVariants)
                {
                    variant.Item2.Resource.FreeShader();
                }
            }
        }

        private RenderingDevice Device;

        // For each shader file, we may have multiple compiled
        // variants depending on the defines used
        private Dictionary<string, ShaderCode<VertexShader>> mVertexShaders =
            new Dictionary<string, ShaderCode<VertexShader>>();

        private Dictionary<string, ShaderCode<PixelShader>> mPixelShaders =
            new Dictionary<string, ShaderCode<PixelShader>>();

        private ResourceLifecycleCallbacks mRegistration;
    }
}