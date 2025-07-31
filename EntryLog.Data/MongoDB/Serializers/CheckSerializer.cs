using EntryLog.Entities.POCOEntities;
using MongoDB.Bson.Serialization;

namespace EntryLog.Data.MongoDB.Serializers
{
    internal static class CheckSerializer
    {
        public static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Check)))
            {
                BsonClassMap.RegisterClassMap<Check>(cm =>
                {
                    cm.MapMember(x => x.Method).SetElementName("metodo");

                    cm.MapMember(x => x.DeviceName)
                         .SetElementName("nombre_dispositivo")
                         .SetIgnoreIfNull(true);

                    cm.MapMember(x => x.Date).SetElementName("fecha");
                    cm.MapMember(x => x.Location).SetElementName("ubicacion");

                    cm.MapMember(x => x.PhotoUrl).SetElementName("url_foto");

                    cm.MapMember(x => x.Notes)
                         .SetElementName("notas")
                         .SetIgnoreIfNull(true);
                });
            }
        }
    }
}
