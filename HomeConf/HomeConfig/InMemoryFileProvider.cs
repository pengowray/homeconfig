using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Primitives;

namespace HomeConf {

    //via: https://stackoverflow.com/a/52405277

    public class InMemoryFileProvider : IFileProvider {
        private class InMemoryFile : IFileInfo {
            private readonly byte[] _data;
            public InMemoryFile(string json) => _data = Encoding.UTF8.GetBytes(json);
            public Stream CreateReadStream() => new MemoryStream(_data);
            public bool Exists { get; } = true;
            public long Length => _data.Length;
            public string PhysicalPath { get; } = string.Empty;
            public string Name { get; } = string.Empty;
            public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;
            public bool IsDirectory { get; } = false;
        }

        private readonly IFileInfo _fileInfo;
        public InMemoryFileProvider(string json) => _fileInfo = new InMemoryFile(json);
        public IFileInfo GetFileInfo(string _) => _fileInfo;
        public IDirectoryContents GetDirectoryContents(string _) => null;
        public IChangeToken Watch(string _) => NullChangeToken.Singleton;
    }


}
