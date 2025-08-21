using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace EntryLog.Data.MongoDB.Serializers
{
    internal static class AppUserSerializer
    {
        public static void Init()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(AppUser)))
            {
                BsonClassMap.RegisterClassMap<AppUser>(cm =>
                {
                    cm.MapMember(x => x.Id)
                        .SetElementName("_id")
                        .SetIdGenerator(GuidGenerator.Instance)
                        .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));

                    cm.SetIdMember(cm.GetMemberMap(x => x.Id));

                    cm.MapMember(x => x.Code).SetElementName("codigo");
                    cm.MapMember(x => x.Name).SetElementName("nombre");

                    cm.MapMember(x => x.Role)
                        .SetElementName("rol")
                        .SetSerializer(new EnumSerializer<RoleType>(BsonType.String));

                    cm.MapMember(x => x.Email).SetElementName("email");
                    cm.MapMember(x => x.CellPhone).SetElementName("telefono_celular");

                    cm.MapMember(x => x.Password).SetElementName("clave");

                    cm.MapMember(x => x.Attempts).SetElementName("intentos");
                    cm.MapMember(x => x.RecoveryToken).SetElementName("token_recuperacion");
                    cm.MapMember(x => x.RecoveryTokenActive).SetElementName("token_recuperacion_activo");
                    cm.MapMember(x => x.FaceID).SetElementName("faceid");
                    cm.MapMember(x => x.Active).SetElementName("activo");
                });
            }
        }
    }
}
