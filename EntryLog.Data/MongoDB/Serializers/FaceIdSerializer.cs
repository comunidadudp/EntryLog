using EntryLog.Entities.POCOEntities;
using MongoDB.Bson.Serialization;

namespace EntryLog.Data.MongoDB.Serializers
{
    internal static class FaceIdSerializer
    {
        public static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(FaceID)))
            {
                BsonClassMap.RegisterClassMap<FaceID>(cm =>
                {
                    cm.MapMember(x => x.ImageURL).SetElementName("url_imagen");
                    cm.MapMember(x => x.RegisterDate).SetElementName("fecha_registro");
                    cm.MapMember(x => x.Descriptor).SetElementName("descriptor");
                    cm.MapMember(x => x.Active).SetElementName("activo");
                });
            }
        }
    }
}
