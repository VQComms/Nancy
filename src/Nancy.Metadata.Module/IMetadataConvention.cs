using System.Collections.Generic;
namespace Nancy.Metadata.Module
{
    public interface IMetadataConvention
    {
        bool CanUseModule(INancyModule module, IEnumerable<IMetadataModule> metadataModules);

        IMetadataModule DiscoverMetadataModule(INancyModule module, IEnumerable<IMetadataModule> metadataModules);
    }
}
