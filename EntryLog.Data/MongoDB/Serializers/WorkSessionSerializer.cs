using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace EntryLog.Data.MongoDB.Serializers
{
    internal static class WorkSessionSerializer
    {
        public static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(WorkSession)))
            {
                BsonClassMap.RegisterClassMap<WorkSession>(cm =>
                {
                    cm.MapMember(x => x.Id)
                        .SetElementName("_id")
                        .SetIdGenerator(GuidGenerator.Instance)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.MapMember(x => x.EmployeeId).SetElementName("id_empleado");
                    cm.MapMember(x => x.CheckIn).SetElementName("entrada");
                    cm.MapMember(x => x.CheckOut).SetElementName("salida");
                    
                    cm.MapMember(x => x.TotalWorked)
                         .SetElementName("total_trabajado")
                         .SetIgnoreIfNull(true);

                    cm.MapMember(x => x.Status)
                        .SetElementName("estado")
                        .SetSerializer(new EnumSerializer<SessionStatus>(BsonType.String));
                });
            }
        }
    }
}
