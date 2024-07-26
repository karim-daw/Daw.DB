using System.Collections.Generic;

namespace Daw.DB.Interfaces
{
    public interface IRelationshipService
    {

        void CreateManyToManyRelationship(string entityA, string entityB, string joinTable);
        List<Dictionary<string, object>> GetManyToManyRelatedRecords(string entityA, string entityB, string joinTable, int entityAId);
    }
}
