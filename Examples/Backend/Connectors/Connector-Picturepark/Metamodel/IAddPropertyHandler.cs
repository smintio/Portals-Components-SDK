using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel
{
    interface IAddPropertyHandler
    {
        DataType Type { get; }

        /// <summary>
        /// If a property references a framework entity (e.g. <see cref="ELocalizedStringsModel"/>),
        /// this property returns that target entity.
        /// </summary>
        EntityModel FrameworkTargetEntity { get; }

        PropertyModel AddProperty(FieldBase field, EntityModel target, ConnectorMetamodel metamodel);
    }
}