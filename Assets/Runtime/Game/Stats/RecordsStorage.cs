using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.Game.UI
{
    public sealed class RecordsStorage : IRecordsStorage
    {
        private const string PlayerPrefsKey = "USER_RECORDS";
        private const int MaxRecordsCount = 10;

        public IReadOnlyList<UserRecord> GetRecords()
        {
            return LoadRecords()
                .OrderBy(r => r.place)
                .Take(MaxRecordsCount)
                .ToList();
        }

        public void AddRecord(string userName, int userScore)
        {
            var records = LoadRecords();

            records.Add(new UserRecord
            {
                userName = userName,
                userScore = userScore
            });

            records = records
                .OrderByDescending(r => r.userScore)
                .Take(MaxRecordsCount)
                .ToList();

            for (int i = 0; i < records.Count; i++)
            {
                var record = records[i];
                record.place = i + 1;
                records[i] = record;
            }

            SaveRecords(records);
        }

        private static List<UserRecord> LoadRecords()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return new List<UserRecord>();

            string json = PlayerPrefs.GetString(PlayerPrefsKey);
            var wrapper = JsonUtility.FromJson<UserRecordList>(json);

            return wrapper?.Records ?? new List<UserRecord>();
        }

        private static void SaveRecords(List<UserRecord> records)
        {
            var wrapper = new UserRecordList
            {
                Records = records
            };

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(PlayerPrefsKey, json);
            PlayerPrefs.Save();
        }

        [System.Serializable]
        public sealed class UserRecordList
        {
            public List<UserRecord> Records = new List<UserRecord>();
        }
    }
}