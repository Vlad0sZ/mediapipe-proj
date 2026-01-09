using System.Collections.Generic;

namespace Runtime.Game.UI
{
    public interface IRecordsStorage
    {
        IReadOnlyList<UserRecord> GetRecords();
        void AddRecord(string userName, int userScore);
    }
}