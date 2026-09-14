using Picturepark.SDK.V1.Contract;
using SmintIo.Portals.ConnectorSDK.Metamodel;
using SmintIo.Portals.SDK.Core.Models.Metamodel.Model;

namespace SmintIo.Portals.Connector.Picturepark.Metamodel
{
    public abstract class AbstractAddPropertyHandler : IAddPropertyHandler
    {
        protected AbstractAddPropertyHandler(DataType dataType)
            : this(dataType, null)
        {
        }

        protected AbstractAddPropertyHandler(DataType dataType, EntityModel frameworkTargetEntity)
        {
            FrameworkTargetEntity = frameworkTargetEntity;
            Type = dataType;
        }

        public DataType Type { get; }

        public EntityModel FrameworkTargetEntity { get; }

        public abstract PropertyModel AddProperty(FieldBase field, EntityModel target, ConnectorMetamodel metamodel);
    }
}