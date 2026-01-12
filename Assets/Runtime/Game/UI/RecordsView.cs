using System;
using System.Collections.Generic;
using Runtime.Game.Models;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Runtime.Game.UI
{
    public sealed class RecordsView : AbstractGameScreenUI
    {
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private RecordRow prefab;
        [SerializeField] private GameObject delimiterPrefab;

        private readonly List<RecordRow> _allRows = new List<RecordRow>(12);
        private IRecordsStorage _recordsStorage;

        [Inject]
        public void Construct(IRecordsStorage recordsStorage) =>
            _recordsStorage = recordsStorage;

        private void Start()
        {
            var parent = scroll.content;
            var hasDelimiter = delimiterPrefab != null;
            for (int i = 0; i < 10; i++)
            {
                var item = Instantiate(prefab, parent, false);
                if (hasDelimiter)
                    Instantiate(delimiterPrefab, parent, false);

                _allRows.Add(item);
            }
        }

        protected override void OnScreenShowing()
        {
            var records = _recordsStorage.GetRecords();
            UpdateRows(records);
        }

        private void UpdateRows(IReadOnlyList<UserRecord> records)
        {
            if (_allRows.Count == 0)
                return;

            for (int i = 0; i < _allRows.Count; i++)
            {
                var hasRecord = i < records.Count;
                var item = _allRows[i];

                if (hasRecord)
                {
                    var record = records[i];
                    item.Setup(record);
                }
                else
                    item.Setup(new UserRecord() {place = i + 1, userName = "---"});
            }
        }
    }
}