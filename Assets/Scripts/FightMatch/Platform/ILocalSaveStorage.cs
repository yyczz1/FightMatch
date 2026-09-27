using System;
using System.Collections.Generic;
using System.IO;

namespace FightMatch.Platform
{
    public interface ILocalSaveStorage
    {
        SaveStorageProfile Profile { get; }
        IDisposable AcquireWriterLease(bool createDirectory);
        IEnumerable<string> EnumerateNames();
        Stream OpenRead(string name);
        Stream CreateWork(string name);
        void FlushFile(Stream stream);
        void PromoteNoReplace(string workName, string finalName);
        void DeleteUncommitted(string name);
        void DeleteIndexedOld(string name);
    }
}
