﻿namespace Nancy.Metadata.Module
{
    using Nancy.Bootstrapper;
    using Nancy.Routing;

    /// <summary>
    /// Performs application registations for metadata modules.
    /// </summary>
    public class MetadataModuleRegistrations : Registrations
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MetadataModuleRegistrations"/> class, that performs
        /// the default registrations of the metadata modules types.
        /// </summary>
        public MetadataModuleRegistrations()
        {
            this.RegisterAll<IMetadataConvention>();
            this.RegisterAll<IMetadataModule>();
            this.RegisterWithDefault<IMetadataModuleResolver>(typeof(DefaultMetadataModuleResolver));

        }
    }
}
