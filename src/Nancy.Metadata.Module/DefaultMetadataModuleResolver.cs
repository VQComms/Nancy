﻿namespace Nancy.Metadata.Module
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Nancy.Routing;

    /// <summary>
    /// Default implementation on how metadata modules are resolved by Nancy.
    /// </summary>
    public class DefaultMetadataModuleResolver : IMetadataModuleResolver
    {
        private readonly IEnumerable<IMetadataConvention> conventions;

        private readonly IEnumerable<IMetadataModule> metadataModules;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMetadataModuleResolver"/> class.
        /// </summary>
        /// <param name="conventions">The conventions that the resolver should use to determine which metadata module to return.</param>
        /// <param name="metadataModules">The metadata modules to use resolve against.</param>
        public DefaultMetadataModuleResolver(IEnumerable<IMetadataConvention> conventions, IEnumerable<IMetadataModule> metadataModules)
        {
            if (conventions == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultMetadataModuleResolver with conventions parameter having null value.");
            }

            if (metadataModules == null)
            {
                throw new InvalidOperationException("Cannot create an instance of DefaultMetadataModuleResolver with metadataModules parameter having null value.");
            }

            this.conventions = conventions;
            this.metadataModules = metadataModules;
        }

        /// <summary>
        /// Resolves a metadata module instance based on the provided information.
        /// </summary>
        /// <param name="module">The <see cref="INancyModule"/>.</param>
        /// <returns>An <see cref="IMetadataModule"/> instance if one could be found, otherwise <see langword="null"/>.</returns>
        public IMetadataModule GetMetadataModule(INancyModule module)
        {
            foreach (var convention in this.conventions)
            {
                if (convention.CanUseModule(module, metadataModules))
                {
                    return convention.DiscoverMetadataModule(module, metadataModules);
                }
            }

            return null;
        }
    }
}