﻿using System.IO;
using Lucene.Net.Store;
using MrCMS.Entities.Multisite;
using Directory = Lucene.Net.Store.Directory;

namespace MrCMS.Indexing.Management
{
    public class FSDirectoryIndexManager<TEntity, TDefinition> : IndexManager<TEntity, TDefinition>
        where TEntity : class
        where TDefinition : IIndexDefinition<TEntity>, new()
    {
        public FSDirectoryIndexManager(CurrentSite currentSite) : base(currentSite)
        {
        }

        protected override Directory GetDirectory()
        {
            return FSDirectory.Open(new DirectoryInfo(Definition.GetLocation(CurrentSite)));
        }
    }
}