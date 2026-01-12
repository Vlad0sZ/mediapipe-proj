using System.Collections.Generic;
using Runtime.Game.Models;

namespace Runtime.Game.UI
{
    public interface IRecordsStorage
    {
        IReadOnlyList<UserRecord> GetRecords();
        UserRecord AddRecord(string userName, int userScore);
    }
}