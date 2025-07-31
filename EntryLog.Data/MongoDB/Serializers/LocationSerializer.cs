using EntryLog.Entities.POCOEntities;
using MongoDB.Bson.Serialization;

namespace EntryLog.Data.MongoDB.Serializers
{
    internal static class LocationSerializer
    {
        public static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Location)))
            {
                BsonClassMap.RegisterClassMap<Location>(cm =>
                {
                    cm.MapMember(x => x.Latitude).SetElementName("latitud");
                    cm.MapMember(x => x.Longitude).SetElementName("longitud");
                    cm.MapMember(x => x.ApproximateAddress)
                         .SetElementName("direccion_aproximada")
                         .SetIgnoreIfNull(true);

                    cm.MapMember(x => x.IpAddress).SetElementName("direccion_ip");
                });
            }
        }
    }
}
